using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// 条件判断。这一期不计算表达式。不成立时仍按失败结束本轮。
    /// </summary>
    public sealed class ConditionFlowTool : FlowToolStep
    {
        /// <summary>
        /// 条件配置。解析失败时为空。
        /// </summary>
        private readonly ConditionToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 解析配置。表达式计算留到后面。
        /// </summary>
        public ConditionFlowTool(string cfg)
        {
            if (!TryRead<ConditionToolEntity>(cfg, out var tool, out var error))
            {
                _error = error;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 入参必须已声明。表达式这一期仍失败，所以不会写出参。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            var missing = RequireInputs(context, _tool.Inputs);
            if (missing is not null)
                return missing;

            return Fail("未实现");
        }
    }
}
