using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Service.Database;
using MMSP.Service.Flow.Steps;

namespace MMSP.Service.Flow
{
    /// <summary>
    /// 流程执行。启动时抄下启用流程，每条流程单独在后台循环，直到停止。
    /// 页面上的执行只跑选中的一条，单步执行只跑右键的那一步。跑完即停，并按步通知状态和耗时。
    /// </summary>
    public sealed class FlowRunService : IFlowRunService
    {
        /// <summary>
        /// 服务所处的阶段。
        /// </summary>
        private enum RunPhase
        {
            /// <summary>
            /// 没有循环。
            /// </summary>
            Idle,

            /// <summary>
            /// 循环正在跑。
            /// </summary>
            Running,

            /// <summary>
            /// 已请求停止，循环还没退出。
            /// </summary>
            Stopping
        }

        /// <summary>
        /// 保护阶段、取消源和轮次号。
        /// </summary>
        private readonly object _gate = new();

        /// <summary>
        /// 当前阶段。
        /// </summary>
        private RunPhase _state = RunPhase.Idle;

        /// <summary>
        /// 本轮的取消源。空闲时为空。
        /// </summary>
        private CancellationTokenSource? _cts;

        /// <summary>
        /// 每接受一次启动就加一。用来丢掉已经过期的停止通知。
        /// </summary>
        private int _runId;

        /// <summary>
        /// 运行状态变化。
        /// </summary>
        public event Action<bool>? RunningChanged;

        /// <summary>
        /// 正在跑或正在停。
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (_gate)
                    return _state != RunPhase.Idle;
            }
        }

        /// <summary>
        /// 抄下启用流程并开始循环。条件不满足时写流程错误日志。
        /// </summary>
        public bool TryStart()
        {
            if (!SqlSugarContext.IsOpen)
            {
                LogHelper.FlowError("客户库未打开");
                return false;
            }

            try
            {
                var count = SqlSugarContext.CustomerDb.Queryable<FlowEntity>()
                    .Where(item => item.IsEnabled)
                    .Count();
                if (count == 0)
                {
                    LogHelper.FlowError("没有启用的流程");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.FlowError("读取流程失败", ex);
                return false;
            }

            ManualResetEventSlim? started = null;
            var accepted = false;
            lock (_gate)
            {
                if (_state == RunPhase.Idle)
                {
                    _cts = new CancellationTokenSource();
                    var token = _cts.Token;
                    started = new ManualResetEventSlim(false);
                    _runId++;
                    var runId = _runId;
                    _state = RunPhase.Running;
                    // 协调任务要等到全部流程退出，不用线程池线程。
                    _ = Task.Factory.StartNew(
                        () => Run(token, started, runId),
                        CancellationToken.None,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default);
                    accepted = true;
                }
            }

            if (!accepted || started is null)
            {
                LogHelper.FlowError("流程正在运行");
                return false;
            }

            Notify(true);
            started.Set();
            return true;
        }

        /// <summary>
        /// 取消当前循环。当前这一次 PLC 或扫码调用仍会走完。
        /// </summary>
        public void Stop()
        {
            lock (_gate)
            {
                if (_state != RunPhase.Running)
                    return;

                _state = RunPhase.Stopping;
                _cts?.Cancel();
            }
        }

        /// <summary>
        /// 单次执行一条流程。已经在跑或库没打开时返回 false。完成后任务结束。
        /// </summary>
        public Task<bool> RunOnceAsync(Guid flowId, IProgress<FlowStepProgress>? progress)
        {
            return StartPass((token, started, runId) => RunOnce(flowId, token, started, runId, progress, null));
        }

        /// <summary>
        /// 只执行一步。和单次执行共用空闲门闩，壳上的循环也占着这道门。
        /// </summary>
        public Task<bool> RunStepAsync(Guid flowId, Guid stepId, IProgress<FlowStepProgress>? progress)
        {
            return StartPass((token, started, runId) => RunOnce(flowId, token, started, runId, progress, stepId));
        }

        /// <summary>
        /// 接受一轮单次执行并在后台跑完。已经在跑或库没打开时返回 false。
        /// </summary>
        private Task<bool> StartPass(Func<CancellationToken, ManualResetEventSlim, int, bool> body)
        {
            if (!SqlSugarContext.IsOpen)
            {
                LogHelper.FlowError("客户库未打开");
                return Task.FromResult(false);
            }

            ManualResetEventSlim? started = null;
            Task<bool>? task = null;
            lock (_gate)
            {
                if (_state == RunPhase.Idle)
                {
                    _cts = new CancellationTokenSource();
                    var token = _cts.Token;
                    started = new ManualResetEventSlim(false);
                    _runId++;
                    var runId = _runId;
                    _state = RunPhase.Running;
                    var gate = started;
                    task = Task.Factory.StartNew(
                        () => body(token, gate, runId),
                        CancellationToken.None,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default);
                }
            }

            if (task is null || started is null)
            {
                LogHelper.FlowError("流程正在运行");
                return Task.FromResult(false);
            }

            Notify(true);
            started.Set();
            return task;
        }

        /// <summary>
        /// 后台循环。启动等待放行后才读快照，避免停止通知早于启动通知。
        /// </summary>
        private void Run(CancellationToken token, ManualResetEventSlim started, int runId)
        {
            FlowDeviceSession? session = null;
            try
            {
                LogHelper.FlowInfo("流程启动");
                try
                {
                    started.Wait(token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (token.IsCancellationRequested)
                    return;

                var flows = LoadFlows();
                if (flows.Count == 0)
                {
                    LogHelper.FlowError("没有启用的流程");
                    return;
                }

                if (token.IsCancellationRequested)
                    return;

                session = new FlowDeviceSession(LoadLink(), LoadScanners());
                var context = new FlowStepContext(token, session);
                FlowRunLoop.Execute(flows, context, LogHelper.FlowInfo, LogHelper.FlowError);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                LogHelper.FlowError("流程执行失败", ex);
            }
            finally
            {
                Release(runId, token, session, started);
            }
        }

        /// <summary>
        /// 单次执行。启动等待放行后才读这一条，避免停止通知早于开始通知。
        /// </summary>
        private bool RunOnce(
            Guid flowId,
            CancellationToken token,
            ManualResetEventSlim started,
            int runId,
            IProgress<FlowStepProgress>? progress,
            Guid? stepId)
        {
            FlowDeviceSession? session = null;
            try
            {
                try
                {
                    started.Wait(token);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }

                if (token.IsCancellationRequested)
                    return false;

                var flow = LoadOne(flowId);
                if (flow is null)
                {
                    LogHelper.FlowError("流程不存在");
                    return false;
                }

                if (stepId is Guid only)
                {
                    var step = flow.Steps.FirstOrDefault(item => item.Id == only);
                    if (step is null)
                    {
                        LogHelper.FlowError("步骤不存在");
                        return false;
                    }

                    LogHelper.FlowInfo($"执行步骤 {flow.Name.Trim()} {step.Title.Trim()}");
                    flow = new RunnableFlow(flow.Name, flow.LogOutput, new[] { step }, flow.Variables);
                }
                else
                {
                    LogHelper.FlowInfo($"执行流程 {flow.Name.Trim()}");
                }

                if (token.IsCancellationRequested)
                    return false;

                session = new FlowDeviceSession(LoadLink(), LoadScanners());
                var local = new FlowStepContext(token, session, FlowVariables.FromSeeds(flow.Variables));
                var stop = FlowCycle.Execute(
                    new[] { flow },
                    local,
                    LogHelper.FlowInfo,
                    message => LogHelper.FlowError(message),
                    progress: progress is null ? null : progress.Report);
                if (stop == FlowCycleStop.Empty)
                    LogHelper.FlowError("没有可执行的步骤");

                return !token.IsCancellationRequested;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.FlowError("流程执行失败", ex);
                return false;
            }
            finally
            {
                Release(runId, token, session, started);
            }
        }

        /// <summary>
        /// 释放这一轮的连接和取消源，并在仍是这一轮时通知界面已停止。
        /// </summary>
        private void Release(int runId, CancellationToken token, FlowDeviceSession? session, ManualResetEventSlim started)
        {
            var cancelled = token.IsCancellationRequested;
            if (session is not null)
            {
                try
                {
                    session.Dispose();
                }
                catch (Exception ex)
                {
                    LogHelper.FlowError("释放设备连接失败", ex);
                }
            }

            var notifyStop = false;
            lock (_gate)
            {
                if (_runId == runId)
                {
                    _state = RunPhase.Idle;
                    _cts?.Dispose();
                    _cts = null;
                    notifyStop = true;
                }
            }

            if (notifyStop)
            {
                lock (_gate)
                    notifyStop = _runId == runId && _state == RunPhase.Idle;
            }

            try
            {
                started.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.FlowError("释放启动等待失败", ex);
            }

            if (cancelled)
                LogHelper.FlowInfo("流程停止");

            if (notifyStop)
                Notify(false);
        }

        /// <summary>
        /// 抄下启用流程和步骤，并按种类创建工具。
        /// </summary>
        private static List<RunnableFlow> LoadFlows()
        {
            var db = SqlSugarContext.CustomerDb;
            var flows = db.Queryable<FlowEntity>()
                .Where(item => item.IsEnabled)
                .OrderBy(item => item.SortIndex)
                .ToList();
            var steps = db.Queryable<FlowStepEntity>()
                .OrderBy(item => item.SortIndex)
                .ToList();
            var variables = db.Queryable<FlowVarEntity>()
                .OrderBy(item => item.SortIndex)
                .ToList();
            var stepsByFlow = steps
                .GroupBy(item => item.FlowId)
                .ToDictionary(group => group.Key, group => group.ToList());
            var varsByFlow = variables
                .GroupBy(item => item.FlowId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var result = new List<RunnableFlow>(flows.Count);
            foreach (var flow in flows)
            {
                stepsByFlow.TryGetValue(flow.Id, out var owned);
                varsByFlow.TryGetValue(flow.Id, out var seeds);
                result.Add(ToRunnable(flow, owned ?? [], seeds ?? []));
            }

            return result;
        }

        /// <summary>
        /// 抄下一条流程，不论是否启用。没有这一行时返回空。
        /// </summary>
        private static RunnableFlow? LoadOne(Guid flowId)
        {
            var db = SqlSugarContext.CustomerDb;
            var flow = db.Queryable<FlowEntity>().Where(item => item.Id == flowId).ToList().FirstOrDefault();
            if (flow is null)
                return null;

            var steps = db.Queryable<FlowStepEntity>()
                .Where(item => item.FlowId == flowId)
                .OrderBy(item => item.SortIndex)
                .ToList();
            var variables = db.Queryable<FlowVarEntity>()
                .Where(item => item.FlowId == flowId)
                .OrderBy(item => item.SortIndex)
                .ToList();
            return ToRunnable(flow, steps, variables);
        }

        /// <summary>
        /// 把库里的一条流程收成一轮快照。步骤和变量按顺序号排列。
        /// </summary>
        private static RunnableFlow ToRunnable(
            FlowEntity flow,
            IReadOnlyList<FlowStepEntity> steps,
            IReadOnlyList<FlowVarEntity> variables)
        {
            var runnableSteps = new List<RunnableStep>(steps.Count);
            foreach (var step in steps.OrderBy(item => item.SortIndex))
            {
                runnableSteps.Add(new RunnableStep(
                    step.SortIndex,
                    step.Title ?? "",
                    FlowToolCatalog.Create(step.ToolKind, step.CfgValue ?? ""),
                    step.Id));
            }

            var seeds = variables
                .OrderBy(item => item.SortIndex)
                .Select(item => (item.Name ?? "", item.InitialValue ?? "", item.DataType ?? ""))
                .ToList();
            return new RunnableFlow(flow.Name ?? "", flow.LogOutput, runnableSteps, seeds);
        }

        /// <summary>
        /// 读 PLC 连接。没有这一行时返回空，PLC 步骤再失败。
        /// </summary>
        private static PlcLinkEntity? LoadLink()
        {
            return SqlSugarContext.CustomerDb.Queryable<PlcLinkEntity>().ToList().FirstOrDefault();
        }

        /// <summary>
        /// 读启用的扫码枪，按卡片顺序。
        /// </summary>
        private static List<ScannerEntity> LoadScanners()
        {
            return SqlSugarContext.CustomerDb.Queryable<ScannerEntity>()
                .Where(item => item.IsValid)
                .OrderBy(item => item.SortIndex)
                .ToList();
        }

        /// <summary>
        /// 通知界面。订阅方抛出的异常不中断循环。
        /// </summary>
        private void Notify(bool running)
        {
            var handler = RunningChanged;
            if (handler is null)
                return;

            try
            {
                handler(running);
            }
            catch (Exception ex)
            {
                LogHelper.FlowError("运行状态通知失败", ex);
            }
        }
    }
}
