using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// 多扫码。枪名按顿号拆开，每把枪按个数触发。
    /// </summary>
    public sealed class MultiScanFlowTool : FlowToolStep
    {
        /// <summary>
        /// 多扫码配置。解析失败时为空。
        /// </summary>
        private readonly MultiScanToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 解析多扫码配置。
        /// </summary>
        public MultiScanFlowTool(string cfg)
        {
            if (!TryRead<MultiScanToolEntity>(cfg, out var tool, out var error))
            {
                _error = error;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 按配置顺序触发。任意一次失败就返回，不再触发后面的。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            if (context.Token.IsCancellationRequested)
                return Cancelled();

            if (string.IsNullOrWhiteSpace(_tool.Trigger))
                return Fail("触发内容为空");

            if (_tool.Count < 1)
                return Fail("配置无效");

            var names = (_tool.Scanner ?? "").Split('、', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (names.Length == 0)
                return Fail("配置无效");

            var codes = new List<string>();
            foreach (var name in names)
            {
                for (var index = 0; index < _tool.Count; index++)
                {
                    if (context.Token.IsCancellationRequested)
                        return Cancelled();

                    var result = context.Devices.Trigger(name, _tool.Trigger, context.Token);
                    var step = FromScan(context, result);
                    if (step.Status != FlowStepStatus.Ok)
                        return step;

                    codes.Add(step.Text);
                }
            }

            return Ok(string.Join("、", codes));
        }
    }
}
