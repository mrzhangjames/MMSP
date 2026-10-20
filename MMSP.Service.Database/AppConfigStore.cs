using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;

namespace MMSP.Service.Database
{
    /// <summary>
    /// 读写系统库里的那一行界面配置。
    /// </summary>
    public static class AppConfigStore
    {
        /// <summary>
        /// 读出配置。库没打开或还没有行时返回默认值，并在库已打开时补上这一行。
        /// </summary>
        public static AppConfigEntity Load()
        {
            if (!SqlSugarContext.IsOpen)
                return AppConfigEntity.CreateDefault();

            var db = SqlSugarContext.SystemDb;
            var row = db.Queryable<AppConfigEntity>()
                .Where(item => item.Id == AppConfigEntity.RowId)
                .Take(1)
                .ToList()
                .FirstOrDefault();
            if (row is null)
            {
                row = AppConfigEntity.CreateDefault();
                db.Insertable(row).ExecuteCommand();
                return row;
            }

            return Normalize(row);
        }

        /// <summary>
        /// 只改主题。
        /// </summary>
        public static void SaveTheme(string theme)
        {
            if (!SqlSugarContext.IsOpen)
                return;

            var row = Load();
            SqlSugarContext.SystemDb.Updateable<AppConfigEntity>()
                .SetColumns(item => item.Theme == theme)
                .Where(item => item.Id == row.Id)
                .ExecuteCommand();
        }

        /// <summary>
        /// 只改语言。
        /// </summary>
        public static void SaveLanguage(string language)
        {
            if (!SqlSugarContext.IsOpen)
                return;

            var row = Load();
            SqlSugarContext.SystemDb.Updateable<AppConfigEntity>()
                .SetColumns(item => item.Language == language)
                .Where(item => item.Id == row.Id)
                .ExecuteCommand();
        }

        /// <summary>
        /// 保存窗口标题和两项保留天数。主题和语言保持原值。
        /// </summary>
        public static void SaveGeneral(string windowTitle, int dbLogDays, int fileLogDays)
        {
            if (!SqlSugarContext.IsOpen)
                return;

            var row = Load();
            row.WindowTitle = windowTitle;
            row.DbLogDays = dbLogDays;
            row.FileLogDays = fileLogDays;
            SqlSugarContext.SystemDb.Updateable(row).ExecuteCommand();
        }

        /// <summary>
        /// 把认不出的主题、语言和天数收成默认值。标题空白时用产品名。
        /// </summary>
        private static AppConfigEntity Normalize(AppConfigEntity row)
        {
            if (row.Theme != AppConfigDefaults.ThemeLight && row.Theme != AppConfigDefaults.ThemeDark)
                row.Theme = AppConfigDefaults.ThemeLight;

            if (row.Language != AppConfigDefaults.LanguageZh && row.Language != AppConfigDefaults.LanguageEn)
                row.Language = AppConfigDefaults.LanguageZh;

            if (string.IsNullOrWhiteSpace(row.WindowTitle))
                row.WindowTitle = AppConfigDefaults.WindowTitle;
            else
                row.WindowTitle = row.WindowTitle.Trim();

            if (row.DbLogDays < 0 || row.DbLogDays > AppConfigDefaults.MaxKeepDays)
                row.DbLogDays = AppConfigDefaults.DbLogDays;

            if (row.FileLogDays < 0 || row.FileLogDays > AppConfigDefaults.MaxKeepDays)
                row.FileLogDays = AppConfigDefaults.FileLogDays;

            return row;
        }
    }
}
