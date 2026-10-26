using System.Text;
using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// Json 解析。从来源变量取出路径上的值，成功时写入出参。
    /// 路径里的 {变量名} 先换成该变量的当前文本。
    /// </summary>
    public sealed class JsonParseFlowTool : FlowToolStep
    {
        /// <summary>
        /// 解析配置。失败时为空。
        /// </summary>
        private readonly JsonParseToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 解析配置。
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
        /// 用来源变量里的 JSON 按路径取值。成功时结果是转换后的文本，并写入出参。
        /// 字符串去掉引号，数字、布尔、空值和对象保留 JSON 原文。失败不写出参。
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

            var sourceName = (_tool.Source ?? "").Trim();
            if (sourceName.Length == 0)
                return Fail("来源为空");

            if (!context.Variables.Contains(sourceName))
                return Fail("变量未声明");

            if (!TryExpand(context, _tool.JsonPath, out var path, out var reason))
                return Fail(reason);

            var json = context.Variables.Get(sourceName);
            if (string.IsNullOrWhiteSpace(json))
                return Fail("来源为空");

            var read = JsonPathValue.Read(json, path);
            if (!read.Ok)
                return Fail(read.Text);

            return Store(context, _tool.Target, Ok(read.Text));
        }

        /// <summary>
        /// 把路径里的 {变量名} 换成当前文本。没有花括号时原样返回。
        /// 花括号不配对，或名称未声明，则失败。只替换一层，不看替换后的文本。
        /// </summary>
        private static bool TryExpand(FlowStepContext context, string? path, out string expanded, out string reason)
        {
            expanded = "";
            reason = "";
            var text = path ?? "";
            var builder = new StringBuilder(text.Length);
            var index = 0;
            while (index < text.Length)
            {
                var start = text.IndexOf('{', index);
                if (start < 0)
                {
                    builder.Append(text, index, text.Length - index);
                    break;
                }

                var end = text.IndexOf('}', start + 1);
                if (end < 0)
                {
                    reason = "路径无效";
                    return false;
                }

                var name = text[(start + 1)..end].Trim();
                if (name.Length == 0 || !context.Variables.Contains(name))
                {
                    reason = "变量未声明";
                    return false;
                }

                builder.Append(text, index, start - index);
                builder.Append(context.Variables.Get(name));
                index = end + 1;
            }

            expanded = builder.ToString();
            return true;
        }
    }
}
