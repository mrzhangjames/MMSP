using System.Diagnostics;
using System.IO;
using System.Reflection;
using MMSP.App.WpfBase.Core;
using MMSP.Common.Helper;
using MMSP.Core.Model.Base;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 配置页界面。主题和语言只切换卡片选中态。许可证文字按效果图展示。
    /// </summary>
    public class ConfigViewModel : BaseViewModel
    {
        private const string DefaultWindowTitle = "LD Software Platform";

        private bool _isLightTheme = true;
        private bool _isChinese = true;
        private string _windowTitle = DefaultWindowTitle;

        /// <summary>
        /// 创建配置页。
        /// </summary>
        public ConfigViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator)
            : base(region, container, eventAggregator)
        {
            ProductName = DefaultWindowTitle;
            AppVersion = AppVersionHelper.GetVersionText();
            FileVersion = ReadFileVersionText();
            SelectLightCommand = new DelegateCommand(SelectLight);
            SelectDarkCommand = new DelegateCommand(SelectDark);
            SelectChineseCommand = new DelegateCommand(SelectChinese);
            SelectEnglishCommand = new DelegateCommand(SelectEnglish);
            SaveCommand = new DelegateCommand(SaveTitle);
            OpenLogCommand = new DelegateCommand(OpenLog);
            ImportCommand = new DelegateCommand(Import);
        }

        /// <summary>
        /// 浅色主题处于选中。
        /// </summary>
        public bool IsLightTheme
        {
            get => _isLightTheme;
            private set
            {
                if (SetProperty(ref _isLightTheme, value))
                    RaisePropertyChanged(nameof(IsDarkTheme));
            }
        }

        /// <summary>
        /// 深色主题处于选中。
        /// </summary>
        public bool IsDarkTheme => !IsLightTheme;

        /// <summary>
        /// 简体中文处于选中。
        /// </summary>
        public bool IsChinese
        {
            get => _isChinese;
            private set
            {
                if (SetProperty(ref _isChinese, value))
                    RaisePropertyChanged(nameof(IsEnglish));
            }
        }

        /// <summary>
        /// English 处于选中。
        /// </summary>
        public bool IsEnglish => !IsChinese;

        /// <summary>
        /// 窗口标题草稿。保存前只留在本页。
        /// </summary>
        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        /// <summary>
        /// 产品名称。
        /// </summary>
        public string ProductName { get; }

        /// <summary>
        /// 程序版本，与底栏同一规则。
        /// </summary>
        public string AppVersion { get; }

        /// <summary>
        /// 文件版本。读不到时是 1.0.0.0。
        /// </summary>
        public string FileVersion { get; }

        /// <summary>
        /// 选中浅色。
        /// </summary>
        public DelegateCommand SelectLightCommand { get; }

        /// <summary>
        /// 选中深色。
        /// </summary>
        public DelegateCommand SelectDarkCommand { get; }

        /// <summary>
        /// 选中简体中文。
        /// </summary>
        public DelegateCommand SelectChineseCommand { get; }

        /// <summary>
        /// 选中 English。
        /// </summary>
        public DelegateCommand SelectEnglishCommand { get; }

        /// <summary>
        /// 保存窗口标题草稿。
        /// </summary>
        public DelegateCommand SaveCommand { get; }

        /// <summary>
        /// 打开日志目录。
        /// </summary>
        public DelegateCommand OpenLogCommand { get; }

        /// <summary>
        /// 导入按钮。这一批不读取许可证文件。
        /// </summary>
        public DelegateCommand ImportCommand { get; }

        private void SelectLight() => IsLightTheme = true;

        private void SelectDark() => IsLightTheme = false;

        private void SelectChinese() => IsChinese = true;

        private void SelectEnglish() => IsChinese = false;

        /// <summary>
        /// 去掉首尾空白。空白时回到默认标题。不写库，也不改主窗标题。
        /// </summary>
        private void SaveTitle()
        {
            var title = WindowTitle.Trim();
            WindowTitle = title.Length == 0 ? DefaultWindowTitle : title;
        }

        /// <summary>
        /// 用资源管理器打开日志目录。目录不存在或打开失败时留在本页。
        /// </summary>
        private void OpenLog()
        {
            try
            {
                var dir = PathCore.LogDirectory;
                if (!Directory.Exists(dir))
                    return;

                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or IOException)
            {
            }
        }

        /// <summary>
        /// 许可证导入尚未实现。
        /// </summary>
        private void Import()
        {
        }

        private static string ReadFileVersionText()
        {
            var entry = Assembly.GetEntryAssembly();
            if (entry?.Location is { Length: > 0 } path)
            {
                var info = FileVersionInfo.GetVersionInfo(path);
                if (!string.IsNullOrWhiteSpace(info.FileVersion))
                    return info.FileVersion;
            }

            return "1.0.0.0";
        }
    }
}
