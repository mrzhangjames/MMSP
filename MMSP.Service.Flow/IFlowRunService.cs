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
    }
}
