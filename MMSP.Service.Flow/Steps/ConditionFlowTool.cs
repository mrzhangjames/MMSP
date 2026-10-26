using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// 条件判断。表达式写法见 <see cref="ConditionExpr"/>。
    /// 成立时成功，并把 true 写入出参。不成立返回「条件不成立」，本轮失败，不写出参。
    /// 没写表达式返回「条件为空」，写错返回「条件无效」，名称没有返回「变量未声明」。
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
        /// 解析配置。表达式在执行时计算。
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
        /// 先核对入参已经声明，再计算表达式。成立时结果文本是 true。
        /// 不成立、表达式为空或无效时失败，不写出参。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            if (context.Token.IsCancellationRequested)
                return Cancelled();

            var missing = RequireInputs(context, _tool.Inputs);
            if (missing is not null)
                return missing;

            if (string.IsNullOrWhiteSpace(_tool.Expression))
                return Fail("条件为空");

            if (!ConditionExpr.TryEval(_tool.Expression, context.Variables, out var passed, out var error))
                return Fail(error);

            if (!passed)
                return Fail("条件不成立");

            return Store(context, _tool.Output, Ok("true"));
        }
    }
}
