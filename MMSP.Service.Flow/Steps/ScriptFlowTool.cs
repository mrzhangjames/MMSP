using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// 脚本。这一期不编译，只把正文写入流程日志，便于测试。
    /// </summary>
    public sealed class ScriptFlowTool : FlowToolStep
    {
        /// <summary>
        /// 脚本配置。解析失败时为空。
        /// </summary>
        private readonly ScriptToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 解析脚本配置。
        /// </summary>
        public ScriptFlowTool(string cfg)
        {
            if (!TryRead<ScriptToolEntity>(cfg, out var tool, out var error))
            {
                _error = error;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 把脚本正文写入流程日志，并让这一步成功。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            var missing = RequireInputs(context, _tool.Inputs);
            if (missing is not null)
                return missing;

            var text = _tool.Script ?? "";
            LogHelper.FlowRunInfo(text);
            return Ok(text);
        }
    }
}
