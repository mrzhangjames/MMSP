using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;
using MMSP.Service.Database;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// UPH。读一个变量，在内部判成 OK 或 NG，再给当前小时记 1。不写出参。
    /// </summary>
    public sealed class UphFlowTool : FlowToolStep
    {
        /// <summary>
        /// UPH 配置。解析失败时为空。
        /// </summary>
        private readonly UphToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 解析配置。判定在执行时读取。
        /// </summary>
        public UphFlowTool(string cfg)
        {
            if (!TryRead<UphToolEntity>(cfg, out var tool, out var error))
            {
                _error = error;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 核对变量类型和取值。OK、NG 都记 1。对不上、值为空或不是 0/1 时失败，不写表。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (context.Token.IsCancellationRequested)
                return Cancelled();

            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            var kind = KindOf(_tool.ValueType);
            if (kind.Length == 0)
                return Fail("配置无效");

            var name = BoundName(_tool);
            if (name.Length == 0)
                return Fail("配置无效");

            if (!context.Variables.Contains(name))
                return Fail("变量未声明");

            var value = context.Variables.Get(name).Trim();
            if (value.Length == 0)
                return Fail("值为空");

            var declared = context.Variables.DeclaredType(name);
            if (!TypeMatches(kind, declared))
                return Fail("类型不一致");

            if (!TryJudge(kind, value, out var ok))
                return Fail("判定无法识别");

            if (context.Token.IsCancellationRequested)
                return Cancelled();

            if (!SqlSugarContext.IsOpen)
                return Fail("客户库未打开");

            var error = UphHour.Add(SqlSugarContext.CustomerDb, DateTime.Now, ok);
            if (error is not null)
                return Fail(error);

            return Ok(ok ? "OK" : "NG");
        }

        /// <summary>
        /// 下拉文字。空白按 bool。既不是 bool 也不是 int 时返回空。
        /// </summary>
        private static string KindOf(string? value)
        {
            var text = (value ?? "").Trim();
            if (text.Length == 0 || text == "bool")
                return "bool";

            return text == "int" ? "int" : "";
        }

        /// <summary>
        /// 判定变量。名称为空时，用入参里唯一的那个名称。
        /// </summary>
        private static string BoundName(UphToolEntity tool)
        {
            var name = (tool.Variable ?? "").Trim();
            if (name.Length > 0)
                return name;

            if (tool.Inputs is null)
                return "";

            var found = "";
            foreach (var input in tool.Inputs)
            {
                var text = (input?.Variable ?? "").Trim();
                if (text.Length == 0)
                    continue;

                if (found.Length > 0)
                    return "";

                found = text;
            }

            return found;
        }

        /// <summary>
        /// 下拉 bool 只认变量表的 bool。下拉 int 只认变量表的 Int。
        /// </summary>
        private static bool TypeMatches(string kind, string declared)
        {
            if (kind == "bool")
                return declared == FlowVarType.Bool;

            return declared == FlowVarType.Int;
        }

        /// <summary>
        /// bool 的 true 为 OK、false 为 NG。int 的 1 为 OK、0 为 NG。大小写按原文，true 和 false 不区分大小写。
        /// </summary>
        private static bool TryJudge(string kind, string value, out bool ok)
        {
            if (kind == "bool")
            {
                if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    ok = true;
                    return true;
                }

                if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    ok = false;
                    return true;
                }
            }
            else if (value == "1")
            {
                ok = true;
                return true;
            }
            else if (value == "0")
            {
                ok = false;
                return true;
            }

            ok = false;
            return false;
        }
    }
}
