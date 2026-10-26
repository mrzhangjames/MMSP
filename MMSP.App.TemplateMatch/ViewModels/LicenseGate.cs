using System.Windows;
using MMSP.App.WpfBase.Services;
using MMSP.Core.License;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 许可证过期后拦住写操作。导入有效许可证并 Replace 之后，下一次操作即可通过。
    /// </summary>
    internal static class LicenseGate
    {
        /// <summary>
        /// 已经有一句提示时不再叠第二句。
        /// </summary>
        private static int _dialog;

        /// <summary>
        /// 当前结果是过期。容器里还没有状态时不限制。
        /// </summary>
        public static bool IsExpired(IContainerProvider container)
        {
            if (!container.IsRegistered<LicenseStatusStore>())
                return false;

            return container.Resolve<LicenseStatusStore>().IsExpired;
        }

        /// <summary>
        /// 过期时弹出一句提示并返回 true。调用方应直接返回，不再写库。
        /// </summary>
        public static bool Block(IContainerProvider container)
        {
            if (!IsExpired(container))
                return false;

            _ = ShowAsync();
            return true;
        }

        /// <summary>
        /// 只显示一次。没有主窗口时不抛出。
        /// </summary>
        private static async Task ShowAsync()
        {
            if (Interlocked.Exchange(ref _dialog, 1) == 1)
                return;

            try
            {
                await MetroDialogService.ShowMessageAsync(Title(), Message());
            }
            catch (Exception)
            {
                // 窗口还没起来时不再追加提示。
            }
            finally
            {
                Interlocked.Exchange(ref _dialog, 0);
            }
        }

        /// <summary>
        /// 提示标题。
        /// </summary>
        private static string Title()
        {
            if (Application.Current?.TryFindResource("License_UseBlockedTitle") is string title && title.Length > 0)
                return title;

            return "许可证";
        }

        /// <summary>
        /// 提示正文。
        /// </summary>
        private static string Message()
        {
            if (Application.Current?.TryFindResource("License_UseBlocked") is string message && message.Length > 0)
                return message;

            return "许可证已过期，当前功能不可用。导入有效许可证后即可恢复。";
        }
    }
}
