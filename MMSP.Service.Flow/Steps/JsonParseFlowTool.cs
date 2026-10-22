using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// Json 解析。这一期不执行解析。
    /// </summary>
    public sealed class JsonParseFlowTool : FlowToolStep
    {
        /// <summary>
        /// 解析配置。失败时为空。这一期仍不执行解析。
        /// </summary>
        private readonly JsonParseToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 解析配置。路径计算留到后面。
        /// </summary>
        public JsonParseFlowTool(string cfg)
        {
            if (!TryRead<JsonParseToolEntity>(cfg, out var tool, out var error))
            {
                _error = error;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 入参必须已声明。解析本身这一期仍失败。
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
