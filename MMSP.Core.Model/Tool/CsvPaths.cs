namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// CSV 的文件名和目录。目录固定在客户数据根下的 File\流程名，文件名默认是流程名加当天年月日。
    /// </summary>
    public static class CsvPaths
    {
        /// <summary>
        /// 客户数据根下的导出目录名。
        /// </summary>
        public const string Folder = "File";

        /// <summary>
        /// 自定义文件名最长长度，含扩展名。
        /// </summary>
        public const int FileNameLength = 120;

        /// <summary>
        /// 界面上的存放目录。用反斜杠，和效果图一致。
        /// </summary>
        public static string DisplayDirectory(string? flowName)
        {
            return "Data\\" + Folder + "\\" + Safe(flowName);
        }

        /// <summary>
        /// 磁盘上的存放目录。flowName 去不掉非法字符时返回空。
        /// </summary>
        public static string FullDirectory(string dataRoot, string? flowName)
        {
            var folder = Safe(flowName);
            if (folder.Length == 0 || string.IsNullOrWhiteSpace(dataRoot))
                return "";

            return Path.Combine(dataRoot, Folder, folder);
        }

        /// <summary>
        /// 当天的默认文件名。流程名无法变成文件名时返回空。
        /// </summary>
        public static string DefaultFile(string? flowName, DateTime day)
        {
            var folder = Safe(flowName);
            if (folder.Length == 0)
                return "";

            return folder + day.ToString("yyyyMMdd") + ".csv";
        }

        /// <summary>
        /// 这一步实际要写的文件名。按日期生成时忽略配置里的预览名。自定义名补上 .csv。非法时返回空。
        /// </summary>
        public static string ResolveFile(CsvToolEntity tool, string? flowName, DateTime day)
        {
            ArgumentNullException.ThrowIfNull(tool);
            if (tool.NameFromDate || string.IsNullOrWhiteSpace(tool.FileName))
                return DefaultFile(flowName, day);

            var text = tool.FileName.Trim();
            if (!text.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                text += ".csv";

            return IsFileName(text) ? text : "";
        }

        /// <summary>
        /// 去掉文件名里不允许的字符，并去掉首尾空白和末尾的点。
        /// </summary>
        public static string Safe(string? name)
        {
            var text = (name ?? "").Trim();
            if (text.Length == 0)
                return "";

            var invalid = Path.GetInvalidFileNameChars();
            var kept = text.Where(item => !invalid.Contains(item)).ToArray();
            return new string(kept).Trim().TrimEnd('.').Trim();
        }

        /// <summary>
        /// 单个文件名。不能带目录，也不能是空或过长。
        /// </summary>
        public static bool IsFileName(string? name)
        {
            var text = (name ?? "").Trim();
            if (text.Length == 0 || text.Length > FileNameLength)
                return false;

            if (text is "." or "..")
                return false;

            return text.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }
    }
}
