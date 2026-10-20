namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 配置页主题、语言和常规项的默认值。系统库没有记录时用这些。
    /// </summary>
    public static class AppConfigDefaults
    {
        /// <summary>
        /// 浅色主题。
        /// </summary>
        public const string ThemeLight = "light";

        /// <summary>
        /// 深色主题。
        /// </summary>
        public const string ThemeDark = "dark";

        /// <summary>
        /// 简体中文。
        /// </summary>
        public const string LanguageZh = "zh-CN";

        /// <summary>
        /// English。
        /// </summary>
        public const string LanguageEn = "en-US";

        /// <summary>
        /// 窗口标题为空时用的产品名。
        /// </summary>
        public const string WindowTitle = "LD Software Platform";

        /// <summary>
        /// 数据库运行日志默认保留天数。
        /// </summary>
        public const int DbLogDays = 90;

        /// <summary>
        /// 日志文件默认保留天数。
        /// </summary>
        public const int FileLogDays = 30;

        /// <summary>
        /// 保留天数上限。再大时日期加减会越过 DateTime 范围。
        /// </summary>
        public const int MaxKeepDays = 3650;
    }
}
