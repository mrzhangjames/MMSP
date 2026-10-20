using MMSP.Core.Model.DB;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;

namespace MMSP.Common.Helper.Logger
{
    /// <summary>
    /// 按保留天数删除客户库运行日志和 Data\Log 里的按日文件。
    /// </summary>
    public static class LogRetentionCleaner
    {
        /// <summary>
        /// 删掉早于保留天数的记录和文件。天数为 0 的那一类不删。单个文件删不掉时跳过。
        /// </summary>
        public static void Apply(int dbLogDays, int fileLogDays)
        {
            Apply(dbLogDays, fileLogDays, DateTime.Today);
        }

        /// <summary>
        /// 用指定的“今天”清理。启动和保存配置时调用。
        /// </summary>
        public static void Apply(int dbLogDays, int fileLogDays, DateTime today)
        {
            DeleteDatabase(dbLogDays, today);
            DeleteFiles(fileLogDays, today);
        }

        /// <summary>
        /// 删除客户库 biz_logs 里早于截止日期的运行日志。
        /// </summary>
        private static void DeleteDatabase(int days, DateTime today)
        {
            var cutoff = LogRetention.CutoffDate(today, days);
            if (cutoff is null || !SqlSugarContext.IsOpen)
                return;

            SqlSugarContext.CustomerDb.Deleteable<RunLogEntity>()
                .Where(item => item.LogTime < cutoff.Value)
                .ExecuteCommand();
        }

        /// <summary>
        /// 删除日志目录里早于截止日期的 yyyyMMdd.log。
        /// </summary>
        private static void DeleteFiles(int days, DateTime today)
        {
            if (LogRetention.CutoffDate(today, days) is null)
                return;

            var directory = PathCore.LogDirectory;
            if (!Directory.Exists(directory))
                return;

            var failed = new List<string>();
            foreach (var file in Directory.EnumerateFiles(directory, "*.log"))
            {
                if (!LogRetention.IsExpiredFile(file, today, days))
                    continue;

                try
                {
                    File.Delete(file);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    failed.Add(Path.GetFileName(file));
                }
            }

            if (failed.Count > 0)
                throw new IOException("有日志文件没能删除：" + string.Join(", ", failed));
        }
    }
}
