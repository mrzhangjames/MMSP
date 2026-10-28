using System.IO;
using System.Windows;
using Microsoft.Win32;
using MMSP.Common.Helper.Logger;
using MMSP.Core.License;

namespace MMSP.App.LicenseIssuer
{
    /// <summary>
    /// 生成一次私钥，按有效期导出公钥和许可证。
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// 私钥目录名。放在签发项目目录下，编译清理不会把它清掉。
        /// </summary>
        private const string KeyFolderName = "Key";

        /// <summary>
        /// 固定私钥文件名。文件已存在时不再生成。
        /// </summary>
        private const string PrivateKeyFileName = "license-dev.pkcs8";

        /// <summary>
        /// 用来识别签发项目目录的工程文件。
        /// </summary>
        private const string ProjectFileName = "MMSP.App.LicenseIssuer.csproj";

        /// <summary>
        /// 写入许可证的客户编码。窗口不再让人选。
        /// </summary>
        private const string CustomerCode = "LD";

        /// <summary>
        /// 创建窗口。
        /// </summary>
        public MainWindow()
        {
            LogHelper.BindCaughtLog();
            InitializeComponent();
        }

        /// <summary>
        /// 填上默认日期，并显示私钥是否已经生成。
        /// </summary>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            StartPicker.SelectedDate = today;
            EndPicker.SelectedDate = today.AddYears(1);
            RefreshKeyState();
        }

        /// <summary>
        /// 永久勾选后不再使用到期日期。
        /// </summary>
        private void PerpetualChanged(object sender, RoutedEventArgs e)
        {
            if (EndPicker is null)
                return;

            EndPicker.IsEnabled = PerpetualBox.IsChecked != true;
        }

        /// <summary>
        /// 只在私钥文件不存在时生成一次。已有文件时直接返回，不覆盖。
        /// </summary>
        private void GenerateClick(object sender, RoutedEventArgs e)
        {
            var privateKeyPath = GetPrivateKeyPath();
            if (File.Exists(privateKeyPath))
            {
                ResultText.Text = "私钥已固定，不能重新生成。";
                RefreshKeyState();
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(privateKeyPath)!);
                var (privateKey, _) = LicenseKeys.Generate();
                File.WriteAllBytes(privateKeyPath, privateKey);
            }
            catch (ArgumentException ex)
            {
                LicenseCaughtLog.Report("写入私钥失败", ex);
                ResultText.Text = "私钥没有写成功。" + ex.Message;
                return;
            }
            catch (NotSupportedException ex)
            {
                LicenseCaughtLog.Report("写入私钥失败", ex);
                ResultText.Text = "私钥没有写成功。" + ex.Message;
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                LicenseCaughtLog.Report("写入私钥失败", ex);
                ResultText.Text = "私钥没有写成功。" + ex.Message;
                return;
            }
            catch (IOException ex)
            {
                LicenseCaughtLog.Report("写入私钥失败", ex);
                ResultText.Text = "私钥没有写成功。" + ex.Message;
                return;
            }

            RefreshKeyState();
            ResultText.Text = "密钥已生成，之后不能重新生成。";
        }

        /// <summary>
        /// 按有效期签出通用证，并把公钥和许可证写到选定目录。
        /// </summary>
        private void ExportClick(object sender, RoutedEventArgs e)
        {
            ResultText.Text = "";
            var privateKeyPath = GetPrivateKeyPath();
            if (!File.Exists(privateKeyPath))
            {
                ResultText.Text = "请先生成密钥。";
                return;
            }

            if (!TryReadDraft(out var draft, out var error))
            {
                ResultText.Text = error;
                return;
            }

            var dialog = new OpenFolderDialog
            {
                Title = "选择导出目录",
                Multiselect = false
            };
            if (dialog.ShowDialog(this) != true)
                return;

            try
            {
                var privateKey = File.ReadAllBytes(privateKeyPath);
                var license = MMSP.Core.License.LicenseIssuer.Sign(draft, privateKey);
                var publicKey = LicenseKeys.PublicFromPrivate(privateKey);
                var store = new LicenseDirectoryStore(dialog.FolderName);
                store.WritePublicKey(publicKey);
                store.WriteLicense(license);
                ResultText.Text = "已导出到 " + dialog.FolderName;
            }
            catch (ArgumentException ex)
            {
                LicenseCaughtLog.Report("导出许可证失败", new ArgumentException("导出许可证失败"));
                ResultText.Text = ex.Message;
            }
            catch (NotSupportedException ex)
            {
                LicenseCaughtLog.Report("导出许可证失败", ex);
                ResultText.Text = "没有导出。" + ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                LicenseCaughtLog.Report("导出许可证失败", ex);
                ResultText.Text = "没有导出。" + ex.Message;
            }
            catch (IOException ex)
            {
                LicenseCaughtLog.Report("导出许可证失败", ex);
                ResultText.Text = "没有导出。" + ex.Message;
            }
        }

        /// <summary>
        /// 根据私钥文件刷新说明。已有私钥时隐藏生成按钮，不显示私钥内容。
        /// </summary>
        private void RefreshKeyState()
        {
            var privateKeyPath = GetPrivateKeyPath();
            var exists = File.Exists(privateKeyPath);
            GenerateButton.Visibility = exists ? Visibility.Collapsed : Visibility.Visible;
            KeyStateText.Text = exists
                ? "私钥已固定，不能重新生成。文件在 " + privateKeyPath
                : "还没有私钥。生成后将固定保存在 " + privateKeyPath;
        }

        /// <summary>
        /// 私钥完整路径：MMSP.App.LicenseIssuer\Key\license-dev.pkcs8。
        /// </summary>
        private static string GetPrivateKeyPath()
        {
            return Path.Combine(FindProjectDirectory(), KeyFolderName, PrivateKeyFileName);
        }

        /// <summary>
        /// 从程序目录向上找到签发项目目录。找不到时用程序目录，私钥仍放在其下的 Key。
        /// </summary>
        private static string FindProjectDirectory()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null)
            {
                if (File.Exists(Path.Combine(dir.FullName, ProjectFileName)))
                    return dir.FullName;

                dir = dir.Parent;
            }

            return AppContext.BaseDirectory;
        }

        /// <summary>
        /// 从界面收集有效期。许可证固定为通用证，并带上壳模块和立讯插件。
        /// </summary>
        private bool TryReadDraft(out LicenseDraft draft, out string error)
        {
            draft = new LicenseDraft();
            error = "";
            if (StartPicker.SelectedDate is not DateTime start)
            {
                error = "请选择生效日期。";
                return false;
            }

            var notBefore = new DateTimeOffset(DateTime.SpecifyKind(start.Date, DateTimeKind.Local));
            DateTimeOffset notAfter;
            if (PerpetualBox.IsChecked == true)
            {
                notAfter = LicenseDates.PerpetualNotAfter;
            }
            else if (EndPicker.SelectedDate is not DateTime end)
            {
                error = "请选择到期日期。";
                return false;
            }
            else
            {
                notAfter = new DateTimeOffset(new DateTime(end.Year, end.Month, end.Day, 23, 59, 59, DateTimeKind.Local));
            }

            if (notBefore > notAfter)
            {
                error = "生效日期晚于到期日期。";
                return false;
            }

            draft = new LicenseDraft
            {
                LicenseId = Guid.NewGuid(),
                CustomerCode = CustomerCode,
                MachineFingerprint = MachineFingerprintFormat.Universal,
                NotBeforeUtc = notBefore,
                NotAfterUtc = notAfter,
                IssuedAtUtc = DateTimeOffset.UtcNow,
                Modules = [LicenseModules.TemplateMatch, LicenseModules.Lux]
            };
            return true;
        }
    }
}
