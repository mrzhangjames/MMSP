using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 配置页保存的主题、语言、窗口标题和日志保留天数。系统库里只留一行。
    /// </summary>
    [SugarTable("sys_app_config")]
    public class AppConfigEntity : BaseDBEntity
    {
        /// <summary>
        /// 这一行的固定主键。重复打开时用来找到同一行。
        /// </summary>
        public static readonly Guid RowId = new("8f4c2a1e-6b37-4d5a-9c18-2e7b0a4d6f31");

        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 主题。light 或 dark。
        /// </summary>
        [SugarColumn(ColumnName = "theme")]
        public string Theme { get; set; } = AppConfigDefaults.ThemeLight;

        /// <summary>
        /// 语言。zh-CN 或 en-US。
        /// </summary>
        [SugarColumn(ColumnName = "language")]
        public string Language { get; set; } = AppConfigDefaults.LanguageZh;

        /// <summary>
        /// 主窗标题。
        /// </summary>
        [SugarColumn(ColumnName = "window_title")]
        public string WindowTitle { get; set; } = AppConfigDefaults.WindowTitle;

        /// <summary>
        /// 客户库运行日志保留天数。0 表示一直保留。
        /// </summary>
        [SugarColumn(ColumnName = "db_log_days")]
        public int DbLogDays { get; set; } = AppConfigDefaults.DbLogDays;

        /// <summary>
        /// Data\Log 按日文件保留天数。0 表示一直保留。
        /// </summary>
        [SugarColumn(ColumnName = "file_log_days")]
        public int FileLogDays { get; set; } = AppConfigDefaults.FileLogDays;

        /// <summary>
        /// 做出一行默认配置。
        /// </summary>
        public static AppConfigEntity CreateDefault()
        {
            return new AppConfigEntity
            {
                Id = RowId,
                Theme = AppConfigDefaults.ThemeLight,
                Language = AppConfigDefaults.LanguageZh,
                WindowTitle = AppConfigDefaults.WindowTitle,
                DbLogDays = AppConfigDefaults.DbLogDays,
                FileLogDays = AppConfigDefaults.FileLogDays
            };
        }
    }
}
