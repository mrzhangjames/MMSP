namespace MMSP.Service.Flow
{
    /// <summary>
    /// 交给工具 <c>Execute</c> 的参数。工具只通过它访问取消标记和设备会话。
    /// </summary>
    public sealed class FlowStepContext
    {
        /// <summary>
        /// 创建一步的执行参数。
        /// </summary>
        public FlowStepContext(CancellationToken token, IFlowDeviceSession devices)
        {
            Token = token;
            Devices = devices;
        }

        /// <summary>
        /// 本轮的取消标记。停止时取消。
        /// </summary>
        public CancellationToken Token { get; }

        /// <summary>
        /// 本轮的 PLC 和扫码枪。
        /// </summary>
        public IFlowDeviceSession Devices { get; }
    }
}
