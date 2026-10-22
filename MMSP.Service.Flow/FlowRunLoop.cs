namespace MMSP.Service.Flow
{
    /// <summary>
    /// 流程错误日志。第二个参数是异常，没有异常时为空。
    /// </summary>
    internal delegate void FlowErrorLog(string message, Exception? exception = null);

    /// <summary>
    /// 每条启用流程单独循环。流程之间并行，一条退出不影响其它流程。
    /// </summary>
    internal static class FlowRunLoop
    {
        /// <summary>
        /// 一轮结束后再进入下一轮之前等待的毫秒数。
        /// </summary>
        public const int RoundIntervalMs = 50;

        /// <summary>
        /// 按流程各起一个任务并等到全部退出。返回值表示这次是被取消停下的。
        /// </summary>
        public static bool Execute(
            IReadOnlyList<RunnableFlow> flows,
            FlowStepContext context,
            Action<string> info,
            FlowErrorLog error)
        {
            var tasks = new Task[flows.Count];
            for (var index = 0; index < flows.Count; index++)
            {
                var flow = flows[index];
                // 循环会停在通讯和等待上，不用线程池线程。
                tasks[index] = Task.Factory.StartNew(
                    () => RunFlow(flow, context, info, error),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }

            Task.WaitAll(tasks);
            return context.Token.IsCancellationRequested;
        }

        /// <summary>
        /// 循环一条流程。没有步骤或未预料异常时只退出本任务。取消和这两种退出都不再等待。
        /// </summary>
        private static void RunFlow(
            RunnableFlow flow,
            FlowStepContext context,
            Action<string> info,
            FlowErrorLog error)
        {
            var failures = new FlowFailureLog();
            var variables = FlowVariables.FromSeeds(flow.Variables);
            var local = new FlowStepContext(context.Token, context.Devices, variables);
            try
            {
                while (true)
                {
                    if (local.Token.IsCancellationRequested)
                        return;

                    var stop = FlowCycle.Execute(
                        new[] { flow },
                        local,
                        info,
                        message => error(message),
                        failures);
                    if (stop == FlowCycleStop.Cancelled)
                        return;

                    if (stop == FlowCycleStop.Empty)
                    {
                        error("没有可执行的步骤");
                        return;
                    }

                    if (!local.Devices.Wait(RoundIntervalMs, local.Token))
                        return;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                error("流程执行失败", ex);
            }
        }
    }
}
