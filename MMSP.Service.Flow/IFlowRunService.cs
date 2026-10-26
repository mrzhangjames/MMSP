namespace MMSP.Service.Flow
{
    /// <summary>
    /// 流程执行服务。每条启用流程单独循环。启动和停止由主壳按钮调用。
    /// </summary>
    public interface IFlowRunService
    {
        /// <summary>
        /// 正在跑或正在停都是 true。连接释放并且循环退出后才是 false。
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 运行状态变化。参数为变化后的 <see cref="IsRunning"/>。在调用方线程上触发，界面自己切回界面线程。
        /// </summary>
        event Action<bool>? RunningChanged;

        /// <summary>
        /// 接受一次启动并立刻返回。已经在跑、正在停、客户库没打开或没有启用流程时返回 false。
        /// </summary>
        bool TryStart();

        /// <summary>
        /// 请求停止。只取消本轮，不等待当前通讯结束，也不在调用线程上关闭连接。
        /// </summary>
        void Stop();

        /// <summary>
        /// 单次执行一条流程，不论是否启用。已经在跑、客户库没打开时返回 false。
        /// 进度在后台线程上报告，调用方自己切回界面线程。
        /// </summary>
        Task<bool> RunOnceAsync(Guid flowId, IProgress<FlowStepProgress>? progress);

        /// <summary>
        /// 只执行一条流程里的一步，不论流程是否启用。已经在跑、客户库没打开或这一步不在时返回 false。
        /// 不重跑前面的步骤，变量用初始值。进度在后台线程上报告。
        /// </summary>
        Task<bool> RunStepAsync(Guid flowId, Guid stepId, IProgress<FlowStepProgress>? progress);
    }
}
