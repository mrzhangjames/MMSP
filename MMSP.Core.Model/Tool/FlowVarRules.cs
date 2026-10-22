using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 保存流程变量之前的校验。通过时返回空字符串。
    /// </summary>
    public static class FlowVarRules
    {
        /// <summary>
        /// 名称最长长度。与流程名、步骤标题一致。
        /// </summary>
        public const int NameLength = 32;

        /// <summary>
        /// 检查整份名单。bound 是本流程步骤已经绑定的变量名，区分大小写。
        /// 已被绑定的名称不能改、不能删。数据类型和初始值不在这里限制。
        /// </summary>
        public static string Check(IReadOnlyList<FlowVarDraft> rows, IReadOnlySet<string> bound)
        {
            ArgumentNullException.ThrowIfNull(rows);
            ArgumentNullException.ThrowIfNull(bound);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var kept = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in rows)
            {
                var name = (row.Name ?? "").Trim();
                if (name.Length == 0)
                    return "请填写变量名称";

                if (name.Length > NameLength)
                    return "名称最长 32";

                if (name.Contains('{') || name.Contains('}'))
                    return "名称不能包含大括号";

                if (!seen.Add(name))
                    return "名称重复";

                if (!FlowVarType.IsDefined(row.DataType))
                    return "数据类型无效";

                var original = (row.OriginalName ?? "").Trim();
                if (original.Length > 0 && bound.Contains(original) && !string.Equals(original, name, StringComparison.Ordinal))
                    return original + "已被步骤使用";

                kept.Add(name);
            }

            foreach (var name in bound)
            {
                if (!kept.Contains(name))
                    return name + "已被步骤使用";
            }

            return "";
        }
    }
}
