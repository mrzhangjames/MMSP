namespace MMSP.Service.Flow
{
    /// <summary>
    /// 一轮要跑的一条流程。配置在启动时抄下，本轮内不再改。
    /// </summary>
    internal sealed class RunnableFlow
    {
        /// <summary>
        /// 创建一条待执行流程。
        /// </summary>
        public RunnableFlow(
            string name,
            bool logOutput,
            IReadOnlyList<RunnableStep> steps,
            IReadOnlyList<(string Name, string Value, string Type)>? variables = null)
        {
            Name = name;
            LogOutput = logOutput;
            Steps = steps;
            Variables = variables ?? [];
        }

        /// <summary>
        /// 流程名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 是否写出步骤成功日志。
        /// </summary>
        public bool LogOutput { get; }

        /// <summary>
        /// 按执行顺序排列的步骤。
        /// </summary>
        public IReadOnlyList<RunnableStep> Steps { get; }

        /// <summary>
        /// 启动时抄下的变量名、初始值和变量表类型。运行中的值不在这里。
        /// </summary>
        public IReadOnlyList<(string Name, string Value, string Type)> Variables { get; }
    }

    /// <summary>
    /// 一轮里的一步。工具对象在快照时创建，多轮复用。
    /// </summary>
    internal sealed class RunnableStep
    {
        /// <summary>
        /// 创建一步。没有主键时界面改用顺序号对上卡片。
        /// </summary>
        public RunnableStep(int sortIndex, string title, Steps.FlowToolStep tool, Guid id = default)
        {
            SortIndex = sortIndex;
            Title = title;
            Tool = tool;
            Id = id;
        }

        /// <summary>
        /// 执行顺序。从 1 开始。
        /// </summary>
        public int SortIndex { get; }

        /// <summary>
        /// 步骤标题。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 这一步的工具。
        /// </summary>
        public Steps.FlowToolStep Tool { get; }

        /// <summary>
        /// 流程步主键。
        /// </summary>
        public Guid Id { get; }
    }

    /// <summary>
    /// 一轮结束后服务要做的事。
    /// </summary>
    internal enum FlowCycleStop
    {
        /// <summary>
        /// 本轮有步骤执行完，可以进入下一轮。
        /// </summary>
        None,

        /// <summary>
        /// 停止信号打断了本轮。
        /// </summary>
        Cancelled,

        /// <summary>
        /// 本轮一步都没有。调用方应停掉这条流程，避免空转。
        /// </summary>
        Empty
    }

    /// <summary>
    /// 把流程名、顺序、标题、耗时和结果拼成一行日志。
    /// </summary>
    internal static class FlowLogText
    {
        /// <summary>
        /// 拼一行。结果文本为空时不留尾部空格。耗时用不变文化的毫秒数。
        /// </summary>
        public static string Format(string flowName, int sortIndex, string title, long elapsedMs, string text)
        {
            if (elapsedMs < 0)
                elapsedMs = 0;

            var culture = System.Globalization.CultureInfo.InvariantCulture;
            var head = $"{flowName.Trim()} {sortIndex.ToString(culture)} {title.Trim()} {elapsedMs.ToString(culture)} ms".Trim();
            var detail = (text ?? "").Trim();
            if (detail.Length == 0)
                return head;

            return head + " " + detail;
        }
    }

    /// <summary>
    /// 按快照顺序执行一轮。不读库，也不持有连接。
    /// </summary>
    internal static class FlowCycle
    {
        /// <summary>
        /// 跑完一轮。失败只打断当前流程，后面的流程继续。成功日志交给 <paramref name="info"/>。
        /// <paramref name="failures"/> 跨轮记住同一步的失败原因；为空时每次失败都写日志。
        /// <paramref name="progress"/> 在调用线程上通知每步状态，为空时不通知。
        /// </summary>
        public static FlowCycleStop Execute(
            IReadOnlyList<RunnableFlow> flows,
            FlowStepContext context,
            Action<string> info,
            Action<string> error,
            FlowFailureLog? failures = null,
            Action<FlowStepProgress>? progress = null)
        {
            var executed = 0;
            foreach (var flow in flows)
            {
                if (context.Token.IsCancellationRequested)
                {
                    SkipFrom(flow, 0, progress);
                    return FlowCycleStop.Cancelled;
                }

                for (var index = 0; index < flow.Steps.Count; index++)
                {
                    if (context.Token.IsCancellationRequested)
                    {
                        SkipFrom(flow, index, progress);
                        return FlowCycleStop.Cancelled;
                    }

                    var step = flow.Steps[index];
                    Report(progress, step, FlowStepRunState.Running, 0, "");
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    FlowStepResult result;
                    try
                    {
                        result = step.Tool.Execute(context);
                    }
                    catch (OperationCanceledException)
                    {
                        watch.Stop();
                        Report(progress, step, FlowStepRunState.Cancelled, watch.ElapsedMilliseconds, "");
                        SkipFrom(flow, index + 1, progress);
                        return FlowCycleStop.Cancelled;
                    }
                    catch (Exception ex)
                    {
                        result = FlowStepResult.Fail(ex.Message);
                    }

                    watch.Stop();
                    var elapsed = watch.ElapsedMilliseconds;
                    if (result.Status == FlowStepStatus.Cancelled)
                    {
                        Report(progress, step, FlowStepRunState.Cancelled, elapsed, "");
                        SkipFrom(flow, index + 1, progress);
                        return FlowCycleStop.Cancelled;
                    }

                    executed++;
                    var line = FlowLogText.Format(flow.Name, step.SortIndex, step.Title, elapsed, result.Text);
                    if (result.Status == FlowStepStatus.Failed)
                    {
                        Report(progress, step, FlowStepRunState.Failed, elapsed, result.Text);
                        if (failures is null || failures.ShouldWrite(step.SortIndex, result.Text))
                            error(line);

                        SkipFrom(flow, index + 1, progress);
                        break;
                    }

                    failures?.Clear(step.SortIndex);
                    Report(progress, step, FlowStepRunState.Ok, elapsed, result.Text);
                    if (flow.LogOutput)
                        info(line);
                }
            }

            if (context.Token.IsCancellationRequested)
                return FlowCycleStop.Cancelled;

            return executed == 0 ? FlowCycleStop.Empty : FlowCycleStop.None;
        }

        /// <summary>
        /// 通知界面。没有订阅时什么都不做。
        /// </summary>
        private static void Report(
            Action<FlowStepProgress>? progress,
            RunnableStep step,
            FlowStepRunState state,
            long elapsedMs,
            string detail)
        {
            progress?.Invoke(new FlowStepProgress(step.Id, step.SortIndex, state, elapsedMs, detail));
        }

        /// <summary>
        /// 从这一下标开始的步骤都标成跳过。
        /// </summary>
        private static void SkipFrom(RunnableFlow flow, int index, Action<FlowStepProgress>? progress)
        {
            for (var skip = index; skip < flow.Steps.Count; skip++)
                Report(progress, flow.Steps[skip], FlowStepRunState.Skipped, 0, "");
        }
    }

    /// <summary>
    /// 记住一条流程里每一步上次的失败原因。同一步、同一原因连续出现时只写第一次。
    /// </summary>
    internal sealed class FlowFailureLog
    {
        /// <summary>
        /// 步骤顺序到上次失败原因。
        /// </summary>
        private readonly Dictionary<int, string> _reason = new();

        /// <summary>
        /// 原因没变时返回 false。原因变了或这一步还没失败过时记下并返回 true。
        /// </summary>
        public bool ShouldWrite(int sortIndex, string reason)
        {
            var text = reason ?? "";
            if (_reason.TryGetValue(sortIndex, out var previous) && previous == text)
                return false;

            _reason[sortIndex] = text;
            return true;
        }

        /// <summary>
        /// 这一步成功后清掉失败原因，下次再失败时重新写。
        /// </summary>
        public void Clear(int sortIndex)
        {
            _reason.Remove(sortIndex);
        }
    }
}
