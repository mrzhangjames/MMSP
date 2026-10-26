using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using MMSP.App.WpfBase.Core;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper;
using MMSP.Common.Helper.Logger;
using MMSP.Core.License;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 配置页。主题和语言点下去就生效并写入系统库。常规区保存窗口标题和日志保留天数。许可证区显示当前证书，导入只替换许可证文件。
    /// </summary>
    public class ConfigViewModel : BaseViewModel
    {
        /// <summary>
        /// 窗口标题草稿。
        /// </summary>
        private string _windowTitle = AppConfigDefaults.WindowTitle;

        /// <summary>
        /// 浅色主题是否选中。
        /// </summary>
        private bool _isLightTheme = true;

        /// <summary>
        /// 简体中文是否选中。
        /// </summary>
        private bool _isChinese = true;

        /// <summary>
        /// 数据库日志保留天数草稿。
        /// </summary>
        private string _dbLogDaysText = AppConfigDefaults.DbLogDays.ToString();

        /// <summary>
        /// 日志文件保留天数草稿。
        /// </summary>
        private string _fileLogDaysText = AppConfigDefaults.FileLogDays.ToString();

        /// <summary>
        /// 常规区保存失败时的提示。空字符串表示没有错误。
        /// </summary>
        private string _saveError = "";

        /// <summary>
        /// 许可证状态文字。
        /// </summary>
        private string _licenseStatusText = "";

        /// <summary>
        /// 当前许可证是否有效。
        /// </summary>
        private bool _licenseIsValid;

        /// <summary>
        /// 许可证上的客户编码。
        /// </summary>
        private string _licenseCustomer = "";

        /// <summary>
        /// 绑定种类。本机或通用。
        /// </summary>
        private string _licenseBind = "";

        /// <summary>
        /// 生效日期。
        /// </summary>
        private string _licenseNotBefore = "";

        /// <summary>
        /// 到期日期。永久时显示“永久”。
        /// </summary>
        private string _licenseNotAfter = "";

        /// <summary>
        /// 授权模块，用逗号连接。
        /// </summary>
        private string _licenseModules = "";

        /// <summary>
        /// 导入许可证后的说明。空字符串表示没有说明。
        /// </summary>
        private string _licenseMessage = "";

        /// <summary>
        /// 启动时接受的许可证。
        /// </summary>
        private readonly LicenseStatusStore _licenses;

        /// <summary>
        /// 主程序 Config 目录里的许可证文件。
        /// </summary>
        private readonly LicenseDirectoryStore _licenseFiles;

        /// <summary>
        /// 创建配置页，并读出已经保存的主题、语言和常规项。
        /// </summary>
        public ConfigViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator,
            LicenseStatusStore licenses,
            LicenseDirectoryStore licenseFiles)
            : base(region, container, eventAggregator)
        {
            _licenses = licenses;
            _licenseFiles = licenseFiles;
            ProductName = AppConfigDefaults.WindowTitle;
            AppVersion = AppVersionHelper.GetVersionText();
            FileVersion = ReadFileVersionText();
            SelectLightCommand = new DelegateCommand(SelectLight);
            SelectDarkCommand = new DelegateCommand(SelectDark);
            SelectChineseCommand = new DelegateCommand(SelectChinese);
            SelectEnglishCommand = new DelegateCommand(SelectEnglish);
            SaveCommand = new DelegateCommand(SaveGeneral);
            OpenLogCommand = new DelegateCommand(OpenLog);
            ImportCommand = new DelegateCommand(Import);
            LoadSaved();
            ShowLicense(licenses.Current);
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
        /// 数据库日志保留天数草稿。
        /// </summary>
        public string DbLogDaysText
        {
            get => _dbLogDaysText;
            set => SetProperty(ref _dbLogDaysText, value);
        }

        /// <summary>
        /// 日志文件保留天数草稿。
        /// </summary>
        public string FileLogDaysText
        {
            get => _fileLogDaysText;
            set => SetProperty(ref _fileLogDaysText, value);
        }

        /// <summary>
        /// 常规区的保存错误。空字符串表示没有错误。
        /// </summary>
        public string SaveError
        {
            get => _saveError;
            private set => SetProperty(ref _saveError, value);
        }

        /// <summary>
        /// 许可证状态文字。
        /// </summary>
        public string LicenseStatusText
        {
            get => _licenseStatusText;
            private set => SetProperty(ref _licenseStatusText, value);
        }

        /// <summary>
        /// 当前许可证是否有效。
        /// </summary>
        public bool LicenseIsValid
        {
            get => _licenseIsValid;
            private set => SetProperty(ref _licenseIsValid, value);
        }

        /// <summary>
        /// 许可证上的客户编码。
        /// </summary>
        public string LicenseCustomer
        {
            get => _licenseCustomer;
            private set => SetProperty(ref _licenseCustomer, value);
        }

        /// <summary>
        /// 绑定种类。
        /// </summary>
        public string LicenseBind
        {
            get => _licenseBind;
            private set => SetProperty(ref _licenseBind, value);
        }

        /// <summary>
        /// 生效日期。
        /// </summary>
        public string LicenseNotBefore
        {
            get => _licenseNotBefore;
            private set => SetProperty(ref _licenseNotBefore, value);
        }

        /// <summary>
        /// 到期日期。
        /// </summary>
        public string LicenseNotAfter
        {
            get => _licenseNotAfter;
            private set => SetProperty(ref _licenseNotAfter, value);
        }

        /// <summary>
        /// 授权模块。
        /// </summary>
        public string LicenseModules
        {
            get => _licenseModules;
            private set => SetProperty(ref _licenseModules, value);
        }

        /// <summary>
        /// 导入后的说明。
        /// </summary>
        public string LicenseMessage
        {
            get => _licenseMessage;
            private set => SetProperty(ref _licenseMessage, value);
        }

        /// <summary>
        /// 产品名称。不随窗口标题改变。
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
        /// 保存窗口标题和保留天数。
        /// </summary>
        public DelegateCommand SaveCommand { get; }

        /// <summary>
        /// 打开日志目录。
        /// </summary>
        public DelegateCommand OpenLogCommand { get; }

        /// <summary>
        /// 从导出目录导入公钥和许可证。验签通过后才写入。
        /// </summary>
        public DelegateCommand ImportCommand { get; }

        /// <summary>
        /// 切到浅色并记住。
        /// </summary>
        private void SelectLight() => UseTheme(light: true);

        /// <summary>
        /// 切到深色并记住。
        /// </summary>
        private void SelectDark() => UseTheme(light: false);

        /// <summary>
        /// 切到简体中文并记住。
        /// </summary>
        private void SelectChinese() => UseLanguage(chinese: true);

        /// <summary>
        /// 切到 English 并记住。
        /// </summary>
        private void SelectEnglish() => UseLanguage(chinese: false);

        /// <summary>
        /// 校验后写入标题和保留天数，并按新天数清理日志。
        /// </summary>
        private void SaveGeneral()
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            SaveError = "";
            if (!LogRetention.TryParseDays(DbLogDaysText, out var dbDays)
                || !LogRetention.TryParseDays(FileLogDaysText, out var fileDays))
            {
                SaveError = Text("Config_DaysInvalid");
                return;
            }

            var title = WindowTitle.Trim();
            if (title.Length == 0)
                title = AppConfigDefaults.WindowTitle;

            WindowTitle = title;
            DbLogDaysText = dbDays.ToString();
            FileLogDaysText = fileDays.ToString();

            try
            {
                AppConfigStore.SaveGeneral(title, dbDays, fileDays);
                AppAppearance.ApplyTitle(title);
            }
            catch (Exception ex)
            {
                SaveError = Text("Config_SaveFailed");
                LogHelper.SysError("保存常规配置失败", ex);
                return;
            }

            try
            {
                LogRetentionCleaner.Apply(dbDays, fileDays);
            }
            catch (Exception ex)
            {
                SaveError = Text("Config_CleanFailed");
                LogHelper.SysError("清理过期日志失败", ex);
            }
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
                LogHelper.SysError("打开日志目录失败", ex);
            }
        }

        /// <summary>
        /// 选择导出目录。用其中的公钥验许可证，通过后才写入本机 Config。
        /// </summary>
        private void Import()
        {
            LicenseMessage = "";
            var dialog = new OpenFolderDialog
            {
                Title = Text("License_PickExportFolder"),
                Multiselect = false
            };
            if (dialog.ShowDialog() != true)
                return;

            var folder = dialog.FolderName;
            var publicKeyPath = Path.Combine(folder, LicenseDirectoryStore.PublicKeyFileName);
            var licensePath = Path.Combine(folder, LicenseDirectoryStore.LicenseFileName);
            if (!File.Exists(publicKeyPath) || !File.Exists(licensePath))
            {
                LicenseMessage = Text("License_PairMissing");
                return;
            }

            byte[] publicKey;
            byte[] license;
            try
            {
                if (!LicenseDirectoryStore.TryDecodePublicKey(File.ReadAllText(publicKeyPath), out publicKey))
                {
                    LicenseMessage = Text("License_PublicKeyInvalid");
                    return;
                }

                license = File.ReadAllBytes(licensePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                LicenseMessage = Text("License_FileUnreadable");
                LogHelper.SysError("读取许可证失败", ex);
                return;
            }

            var fingerprint = MachineFingerprintReader.TryRead(out var value) ? value : "";
            var evaluation = LicenseRuntime.CheckPair(
                publicKey,
                license,
                fingerprint,
                _licenseFiles.ReadSeen(),
                DateTimeOffset.UtcNow);
            if (!evaluation.IsValid)
            {
                LicenseMessage = Text(LicensePhrases.Key(evaluation.Code));
                return;
            }

            try
            {
                _licenseFiles.WritePublicKey(publicKey);
                _licenseFiles.WriteLicense(license);
                LicenseRuntime.Remember(_licenseFiles, DateTimeOffset.UtcNow);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                LicenseMessage = Text("License_FileUnreadable");
                LogHelper.SysError("写入许可证失败", ex);
                return;
            }

            _licenses.Replace(evaluation);
            ShowLicense(evaluation);
            LicenseMessage = Text("License_ImportOk");
            LogHelper.SysInfo("许可证 " + evaluation.ToLogText());
        }

        /// <summary>
        /// 把校验结果填到许可证区。
        /// </summary>
        private void ShowLicense(LicenseEvaluation evaluation)
        {
            LicenseIsValid = evaluation.IsValid;
            LicenseStatusText = evaluation.IsValid
                ? Text("Config_StatusValid")
                : Text("Config_StatusUnauthorized");
            LicenseCustomer = evaluation.CustomerCode ?? "";
            LicenseBind = evaluation.IsUniversal switch
            {
                true => Text("Config_BindUniversal"),
                false => Text("Config_BindMachine"),
                _ => ""
            };
            LicenseNotBefore = FormatLicenseDate(evaluation.NotBeforeUtc);
            LicenseNotAfter = evaluation.NotAfterUtc is { } end && LicenseDates.IsPerpetual(end)
                ? Text("Config_Perpetual")
                : FormatLicenseDate(evaluation.NotAfterUtc);
            LicenseModules = evaluation.Modules.Count == 0 ? "" : string.Join(", ", evaluation.Modules);
        }

        /// <summary>
        /// 许可证时间显示到日。没有值时是空串。
        /// </summary>
        private static string FormatLicenseDate(DateTimeOffset? value)
        {
            return value is { } time ? time.ToLocalTime().ToString("yyyy-MM-dd") : "";
        }

        /// <summary>
        /// 用系统库里的值填草稿，不在这里再次切换主题。
        /// </summary>
        private void LoadSaved()
        {
            var row = AppConfigStore.Load();
            IsLightTheme = row.Theme != AppConfigDefaults.ThemeDark;
            IsChinese = row.Language != AppConfigDefaults.LanguageEn;
            WindowTitle = row.WindowTitle;
            DbLogDaysText = row.DbLogDays.ToString();
            FileLogDaysText = row.FileLogDays.ToString();
        }

        /// <summary>
        /// 应用主题并写入系统库。写入失败时界面仍保持刚选中的主题。
        /// </summary>
        private void UseTheme(bool light)
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            IsLightTheme = light;
            AppAppearance.ApplyTheme(light);
            try
            {
                AppConfigStore.SaveTheme(light ? AppConfigDefaults.ThemeLight : AppConfigDefaults.ThemeDark);
            }
            catch (Exception ex)
            {
                LogHelper.SysError("保存主题失败", ex);
            }
        }

        /// <summary>
        /// 应用语言并写入系统库。写入失败时界面仍保持刚选中的语言。
        /// </summary>
        private void UseLanguage(bool chinese)
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            IsChinese = chinese;
            var language = chinese ? AppConfigDefaults.LanguageZh : AppConfigDefaults.LanguageEn;
            AppAppearance.ApplyLanguage(language);
            try
            {
                AppConfigStore.SaveLanguage(language);
            }
            catch (Exception ex)
            {
                LogHelper.SysError("保存语言失败", ex);
            }
        }

        /// <summary>
        /// 读取语言资源。没有资源时返回键名。
        /// </summary>
        private static string Text(string key)
        {
            if (System.Windows.Application.Current?.TryFindResource(key) is string value && value.Length > 0)
                return value;

            return key;
        }

        /// <summary>
        /// 读取入口程序的文件版本。
        /// </summary>
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
