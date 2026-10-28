using MMSP.Common.Helper;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow
{
    /// <summary>
    /// 把 CSV 工具的一行追加到当天文件。文件不存在时先写表头。表头和配置不一致时不改文件。
    /// </summary>
    internal static class CsvExport
    {
        /// <summary>
        /// 同一路径的读改写串行进行，避免两次导出互相覆盖。
        /// </summary>
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, object> Gates = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 追加一行。成功时返回空，written 是文件名。校验失败时不创建目录。
        /// </summary>
        public static string Append(CsvToolEntity tool, string? flowName, FlowVariables variables, DateTime now, string? dataRoot, out string written)
        {
            ArgumentNullException.ThrowIfNull(tool);
            ArgumentNullException.ThrowIfNull(variables);
            written = "";
            var error = ReadRow(tool, flowName, variables, out var columns, out var cells);
            if (error.Length > 0)
                return error;

            var fileName = CsvPaths.ResolveFile(tool, flowName, now);
            if (fileName.Length == 0)
                return "文件名无效";

            var folder = CsvPaths.FullDirectory(dataRoot ?? "", flowName);
            if (folder.Length == 0)
                return "流程名为空";

            var path = Path.Combine(folder, fileName);
            var gate = Gates.GetOrAdd(path, _ => new object());
            lock (gate)
            {
                try
                {
                    Directory.CreateDirectory(folder);
                    if (!File.Exists(path) || new FileInfo(path).Length == 0)
                    {
                        CsvFileHelper.Write(path, new[] { columns, cells });
                        written = fileName;
                        return "";
                    }

                    var existing = CsvFileHelper.Read(path);
                    if (existing.Count == 0)
                    {
                        CsvFileHelper.Write(path, new[] { columns, cells });
                        written = fileName;
                        return "";
                    }

                    if (!SameHeader(existing[0], columns))
                        return "表头不一致";

                    var all = new List<IReadOnlyList<string>>(existing.Count + 1);
                    foreach (var row in existing)
                        all.Add(row);

                    all.Add(cells);
                    CsvFileHelper.Write(path, all);
                    written = fileName;
                    return "";
                }
                catch (ArgumentException ex)
                {
                    LogHelper.FlowRunError("写入 CSV 失败", ex);
                    return "文件无法写入";
                }
                catch (UnauthorizedAccessException ex)
                {
                    LogHelper.FlowRunError("写入 CSV 失败", ex);
                    return "文件无法写入";
                }
                catch (IOException ex)
                {
                    LogHelper.FlowRunError("写入 CSV 失败", ex);
                    return "文件无法写入";
                }
            }
        }

        /// <summary>
        /// 读出表头和这一行的值。列名或数据来源不合法时返回原因。
        /// </summary>
        private static string ReadRow(CsvToolEntity tool, string? flowName, FlowVariables variables, out string[] columns, out string[] cells)
        {
            columns = [];
            cells = [];
            if (CsvPaths.Safe(flowName).Length == 0)
                return "流程名为空";

            var rows = new List<CsvColumn>();
            if (tool.Rows is not null)
            {
                foreach (var row in tool.Rows)
                {
                    if (row is not null)
                        rows.Add(row);
                }
            }

            if (rows.Count == 0)
                return "数据列表为空";

            columns = new string[rows.Count];
            cells = new string[rows.Count];
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < rows.Count; index++)
            {
                var column = (rows[index].Column ?? "").Trim();
                if (column.Length == 0)
                    return "列名为空";

                if (column.Length > FlowVarRules.NameLength)
                    return "列名最长 32";

                if (column.Contains('\r') || column.Contains('\n'))
                    return "列名无效";

                if (!seen.Add(column))
                    return "列名重复";

                var source = (rows[index].Source ?? "").Trim();
                if (source.Length == 0)
                    return "数据来源为空";

                if (!variables.Contains(source))
                    return "变量未声明";

                columns[index] = column;
                cells[index] = variables.Get(source);
            }

            return "";
        }

        /// <summary>
        /// 已有表头和配置列名是否一致。比较前去掉首尾空白。
        /// </summary>
        private static bool SameHeader(IReadOnlyList<string> actual, IReadOnlyList<string> expected)
        {
            if (actual.Count != expected.Count)
                return false;

            for (var index = 0; index < expected.Count; index++)
            {
                if (!string.Equals((actual[index] ?? "").Trim(), expected[index], StringComparison.Ordinal))
                    return false;
            }

            return true;
        }
    }
}
