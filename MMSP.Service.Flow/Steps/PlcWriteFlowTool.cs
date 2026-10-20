using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// PLC 写入。
    /// </summary>
    public sealed class PlcWriteFlowTool : FlowToolStep
    {
        /// <summary>
        /// 写入配置。解析失败时为空。
        /// </summary>
        private readonly PlcWriteToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 解析写入配置。
        /// </summary>
        public PlcWriteFlowTool(string cfg)
        {
            if (!TryRead<PlcWriteToolEntity>(cfg, out var tool, out var error))
            {
                _error = error;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 把配置里的值写到地址。地址为空时不访问设备。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            if (context.Token.IsCancellationRequested)
                return Cancelled();

            if (string.IsNullOrWhiteSpace(_tool.Address))
                return Fail("地址为空");

            var result = context.Devices.Write(_tool.Address.Trim(), _tool.DataType, _tool.Value ?? "", context.Token);
            return FromPlc(context, result);
        }
    }
}
