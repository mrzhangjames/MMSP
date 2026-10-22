using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// 单扫码。按名称找枪，触发一次。
    /// </summary>
    public sealed class SingleScanFlowTool : FlowToolStep
    {
        /// <summary>
        /// 扫码配置。解析失败时为空。
        /// </summary>
        private readonly SingleScanToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 解析单扫码配置。
        /// </summary>
        public SingleScanFlowTool(string cfg)
        {
            if (!TryRead<SingleScanToolEntity>(cfg, out var tool, out var error))
            {
                _error = error;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 触发一把枪并返回条码。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            if (context.Token.IsCancellationRequested)
                return Cancelled();

            if (string.IsNullOrWhiteSpace(_tool.Scanner))
                return Fail("配置无效");

            if (string.IsNullOrWhiteSpace(_tool.Trigger))
                return Fail("触发内容为空");

            var result = context.Devices.Trigger(_tool.Scanner.Trim(), _tool.Trigger, context.Token);
            var scanned = FromScan(context, result);
            if (scanned.Status != FlowStepStatus.Ok)
                return scanned;

            if (_tool.WritePlc)
            {
                if (string.IsNullOrWhiteSpace(_tool.WriteAddress))
                    return Fail("地址为空");

                var written = FromPlc(
                    context,
                    context.Devices.Write(_tool.WriteAddress.Trim(), _tool.WriteDataType, scanned.Text, context.Token));
                if (written.Status != FlowStepStatus.Ok)
                    return written;
            }

            return Store(context, _tool.Output, scanned);
        }
    }
}
