using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// PLC 读取。
    /// </summary>
    public sealed class PlcReadFlowTool : FlowToolStep
    {
        /// <summary>
        /// 读取配置。解析失败时为空。
        /// </summary>
        private readonly PlcReadToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 解析读取配置。
        /// </summary>
        public PlcReadFlowTool(string cfg)
        {
            if (!TryRead<PlcReadToolEntity>(cfg, out var tool, out var error))
            {
                _error = error;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 读地址上的值。地址为空时不访问设备。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            if (context.Token.IsCancellationRequested)
                return Cancelled();

            if (string.IsNullOrWhiteSpace(_tool.Address))
                return Fail("地址为空");

            var result = context.Devices.Read(_tool.Address.Trim(), _tool.DataType, _tool.Length, context.Token);
            return Store(context, _tool.Output, FromPlc(context, result));
        }
    }
}
