namespace MMSP.Service.Flow
{
    /// <summary>
    /// 交给工具 <c>Execute</c> 的参数。工具只通过它访问取消标记和设备会话。
    /// </summary>
    public sealed class FlowStepContext
    {
        /// <summary>
        /// 创建一步的执行参数。variables 为空时这一步不能读写流程变量。
        /// </summary>
        public FlowStepContext(CancellationToken token, IFlowDeviceSession devices, FlowVariables? variables = null, string? flowName = null)
        {
            Token = token;
            Devices = devices;
            Variables = variables ?? FlowVariables.Empty;
            FlowName = flowName ?? "";
        }

        /// <summary>
        /// 本轮的取消标记。停止时取消。
        /// </summary>
        public CancellationToken Token { get; }

        /// <summary>
        /// 本轮的 PLC 和扫码枪。
        /// </summary>
        public IFlowDeviceSession Devices { get; }

        /// <summary>
        /// 本流程这一次运行的变量。各轮共用，不与其它流程共享。
        /// </summary>
        public FlowVariables Variables { get; }

        /// <summary>
        /// 流程名称。一轮开始时由执行循环写入，CSV 用它生成目录和默认文件名。
        /// </summary>
        public string FlowName { get; set; }
    }
}
