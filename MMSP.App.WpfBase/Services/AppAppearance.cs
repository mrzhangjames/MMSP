using System.Windows;
using ControlzEx.Theming;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;

namespace MMSP.App.WpfBase.Services
{
    /// <summary>
    /// 把已保存的主题、语言和窗口标题应用到当前程序。
    /// </summary>
    public static class AppAppearance
    {
        /// <summary>
        /// 浅色 MahApps 主题。
        /// </summary>
        private const string LightThemeName = "Light.Olive";

        /// <summary>
        /// 深色 MahApps 主题。
        /// </summary>
        private const string DarkThemeName = "Dark.Olive";

        /// <summary>
        /// 语言字典在程序集里的目录。
        /// </summary>
        private const string LanguagePack = "pack://application:,,,/MMSP.App.Shared;component/Assets/Languages/";

        /// <summary>
        /// 深色画刷字典。盖在默认浅色画刷上面。
        /// </summary>
        private const string DarkBrushPack = "pack://application:,,,/MMSP.App.Shared;component/Themes/Styles/Brushes.Dark.xaml";

        /// <summary>
        /// 语言字典已经换成另一本。主壳用来刷新自己拼出来的文字。
        /// </summary>
        public static event Action? LanguageChanged;

        /// <summary>
        /// 按配置行切换主题和语言。主窗已经存在时同时改标题。
        /// </summary>
        public static void Apply(AppConfigEntity config)
        {
            ApplyTheme(config.Theme == AppConfigDefaults.ThemeLight);
            ApplyLanguage(config.Language);
            ApplyTitle(config.WindowTitle);
        }

        /// <summary>
        /// 切换浅色或深色。浅色拿掉深色画刷，深色把深色画刷放到资源最后。
        /// </summary>
        public static void ApplyTheme(bool light)
        {
            var app = Application.Current;
            if (app is null)
                return;

            ThemeManager.Current.ChangeTheme(app, light ? LightThemeName : DarkThemeName);
            var merged = app.Resources.MergedDictionaries;
            var existing = FindDictionary(merged, "Brushes.Dark.xaml");
            if (light)
            {
                if (existing is not null)
                    merged.Remove(existing);
                return;
            }

            if (existing is not null)
                return;

            merged.Add(new ResourceDictionary { Source = new Uri(DarkBrushPack, UriKind.Absolute) });
        }

        /// <summary>
        /// 换成 zh-CN 或 en-US 语言字典。认不出的代码按简体中文处理。
        /// </summary>
        public static void ApplyLanguage(string? language)
        {
            var app = Application.Current;
            if (app is null)
                return;

            var code = language == AppConfigDefaults.LanguageEn
                ? AppConfigDefaults.LanguageEn
                : AppConfigDefaults.LanguageZh;
            var file = code == AppConfigDefaults.LanguageEn ? "EN-US.xaml" : "ZH-CN.xaml";
            var merged = app.Resources.MergedDictionaries;
            var existing = FindDictionary(merged, "/Assets/Languages/");
            var next = new ResourceDictionary
            {
                Source = new Uri(LanguagePack + file, UriKind.Absolute)
            };
            if (existing is null)
                merged.Add(next);
            else
            {
                var index = merged.IndexOf(existing);
                merged.Remove(existing);
                merged.Insert(index, next);
            }

            LanguageChanged?.Invoke();
        }

        /// <summary>
        /// 改主窗标题。主窗还没创建时什么也不做。
        /// </summary>
        public static void ApplyTitle(string? title)
        {
            if (Application.Current?.MainWindow is not Window window)
                return;

            var text = string.IsNullOrWhiteSpace(title) ? AppConfigDefaults.WindowTitle : title.Trim();
            window.Title = text;
        }

        /// <summary>
        /// 在合并字典里按源地址找一本。
        /// </summary>
        private static ResourceDictionary? FindDictionary(IList<ResourceDictionary> merged, string marker)
        {
            foreach (var item in merged)
            {
                var source = item.Source?.OriginalString;
                if (source is not null && source.Contains(marker, StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return null;
        }
    }
}
