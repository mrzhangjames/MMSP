using System.Globalization;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 日志保留天数。0 表示一直保留。只判断该不该删，不访问库和磁盘。
    /// </summary>
    public static class LogRetention
    {
        /// <summary>
        /// 按日日志文件名的长度，例如 20261020.log。
        /// </summary>
        private const int DayFileNameLength = 12;

        /// <summary>
        /// 读保留天数。只接受 0 到 <see cref="AppConfigDefaults.MaxKeepDays"/> 的整数。
        /// </summary>
        public static bool TryParseDays(string? text, out int days)
        {
            days = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (!int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out days))
                return false;

            return days >= 0 && days <= AppConfigDefaults.MaxKeepDays;
        }

        /// <summary>
        /// 早于这一天的日志要删除。天数为 0 或负数时不删除，返回空。
        /// </summary>
        public static DateTime? CutoffDate(DateTime today, int days)
        {
            if (days <= 0)
                return null;

            if (days > AppConfigDefaults.MaxKeepDays)
                days = AppConfigDefaults.MaxKeepDays;

            return today.Date.AddDays(-days);
        }

        /// <summary>
        /// 这个按日文件是否早于保留天数。文件名不是 yyyyMMdd.log 时不删。
        /// </summary>
        public static bool IsExpiredFile(string path, DateTime today, int days)
        {
            var cutoff = CutoffDate(today, days);
            if (cutoff is null || string.IsNullOrWhiteSpace(path))
                return false;

            var name = Path.GetFileName(path);
            if (name.Length != DayFileNameLength || !name.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!DateTime.TryParseExact(
                    name.Substring(0, 8),
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var fileDate))
                return false;

            return fileDate.Date < cutoff.Value;
        }
    }
}
