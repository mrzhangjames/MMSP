using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace MMSP.App.WpfBase.Services
{
    /// <summary>
    /// MahApps.Metro 弹窗。消息框、输入框、登录框、进度框和自定义框都从当前 Metro 窗口弹出。登录密码不写日志。
    /// </summary>
    public static class MetroDialogService
    {
        /// <summary>
        /// 消息框。style 决定按钮：确定，确定和取消，再加一个或两个辅助按钮。
        /// </summary>
        public static Task<MessageDialogResult> ShowMessageAsync(
            string title,
            string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative,
            MetroDialogSettings? settings = null)
        {
            return RequireWindow().ShowMessageAsync(title, message, style, Settings(settings));
        }

        /// <summary>
        /// 确认框。确定返回 true，取消返回 false。
        /// </summary>
        public static async Task<bool> ConfirmAsync(string title, string message, MetroDialogSettings? settings = null)
        {
            var result = await ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, settings);
            return result == MessageDialogResult.Affirmative;
        }

        /// <summary>
        /// 输入框。取消时返回空。
        /// </summary>
        public static Task<string> ShowInputAsync(string title, string message, MetroDialogSettings? settings = null)
        {
            return RequireWindow().ShowInputAsync(title, message, Settings(settings));
        }

        /// <summary>
        /// 登录框。返回的密码只供调用方校验，不要写入日志。取消时返回空。
        /// </summary>
        public static Task<LoginDialogData> ShowLoginAsync(string title, string message, LoginDialogSettings? settings = null)
        {
            return RequireWindow().ShowLoginAsync(title, message, LoginSettings(settings));
        }

        /// <summary>
        /// 进度框。调用方用返回的控制器更新进度并关闭。
        /// </summary>
        public static Task<ProgressDialogController> ShowProgressAsync(
            string title,
            string message,
            bool isCancelable = false,
            MetroDialogSettings? settings = null)
        {
            return RequireWindow().ShowProgressAsync(title, message, isCancelable, Settings(settings));
        }

        /// <summary>
        /// 显示一个已经创建好的自定义弹窗。关闭要再调用 HideAsync。
        /// </summary>
        public static Task ShowAsync(BaseMetroDialog dialog, MetroDialogSettings? settings = null)
        {
            return RequireWindow().ShowMetroDialogAsync(dialog, Settings(settings));
        }

        /// <summary>
        /// 按类型创建并显示自定义弹窗。关闭要再调用 HideAsync。
        /// </summary>
        public static Task<TDialog> ShowAsync<TDialog>(MetroDialogSettings? settings = null)
            where TDialog : BaseMetroDialog
        {
            return RequireWindow().ShowMetroDialogAsync<TDialog>(Settings(settings));
        }

        /// <summary>
        /// 关掉当前窗口上的自定义弹窗。
        /// </summary>
        public static Task HideAsync(BaseMetroDialog dialog, MetroDialogSettings? settings = null)
        {
            return RequireWindow().HideMetroDialogAsync(dialog, settings);
        }

        /// <summary>
        /// 读取当前窗口上正在显示的弹窗。
        /// </summary>
        public static Task<TDialog> GetCurrentAsync<TDialog>()
            where TDialog : BaseMetroDialog
        {
            return RequireWindow().GetCurrentDialogAsync<TDialog>();
        }

        /// <summary>
        /// 在窗口外显示消息框，并等待关闭。
        /// </summary>
        public static MessageDialogResult ShowModalMessage(
            string title,
            string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative,
            MetroDialogSettings? settings = null)
        {
            return RequireWindow().ShowModalMessageExternal(title, message, style, Settings(settings));
        }

        /// <summary>
        /// 在窗口外显示输入框，并等待关闭。取消时返回空。
        /// </summary>
        public static string ShowModalInput(string title, string message, MetroDialogSettings? settings = null)
        {
            return RequireWindow().ShowModalInputExternal(title, message, Settings(settings));
        }

        /// <summary>
        /// 在窗口外显示登录框，并等待关闭。返回的密码不要写入日志。取消时返回空。
        /// </summary>
        public static LoginDialogData ShowModalLogin(string title, string message, LoginDialogSettings? settings = null)
        {
            return RequireWindow().ShowModalLoginExternal(title, message, LoginSettings(settings));
        }

        /// <summary>
        /// 在窗口外显示自定义弹窗，不等待关闭。
        /// </summary>
        public static TDialog ShowExternally<TDialog>(TDialog dialog, Window? owner = null, Action<Window>? configure = null)
            where TDialog : BaseMetroDialog
        {
            return dialog.ShowDialogExternally(owner, configure);
        }

        /// <summary>
        /// 在窗口外显示自定义弹窗，并等待关闭。
        /// </summary>
        public static TDialog ShowModalExternally<TDialog>(TDialog dialog, Window? owner = null, Action<Window>? configure = null)
            where TDialog : BaseMetroDialog
        {
            return dialog.ShowModalDialogExternally(owner, configure);
        }

        /// <summary>
        /// 当前活动的 Metro 窗口。没有窗口时不能弹窗。
        /// </summary>
        private static MetroWindow RequireWindow()
        {
            var app = Application.Current;
            if (app is null)
                throw new InvalidOperationException("没有找到 Metro 窗口。");

            var windows = app.Windows.OfType<MetroWindow>().ToList();
            var window = windows.FirstOrDefault(item => item.IsActive)
                ?? app.MainWindow as MetroWindow
                ?? windows.FirstOrDefault();
            if (window is null)
                throw new InvalidOperationException("没有找到 Metro 窗口。");

            return window;
        }

        /// <summary>
        /// 没有传入设置时，按钮用确认和取消，并且弹窗打开时不能关掉主窗口。
        /// </summary>
        private static MetroDialogSettings Settings(MetroDialogSettings? settings)
        {
            if (settings is not null)
                return settings;

            return new MetroDialogSettings
            {
                AffirmativeButtonText = Text("Dialog_Confirm", "确认"),
                NegativeButtonText = Text("Dialog_Cancel", "取消"),
                OwnerCanCloseWithDialog = false
            };
        }

        /// <summary>
        /// 登录框使用自己的设置类型，按钮文字与其他弹窗一致。
        /// </summary>
        private static LoginDialogSettings LoginSettings(LoginDialogSettings? settings)
        {
            if (settings is not null)
                return settings;

            return new LoginDialogSettings
            {
                AffirmativeButtonText = Text("Dialog_Confirm", "确认"),
                NegativeButtonText = Text("Dialog_Cancel", "取消"),
                OwnerCanCloseWithDialog = false
            };
        }

        /// <summary>
        /// 读取语言资源。没有资源时用后备文字。
        /// </summary>
        private static string Text(string key, string fallback)
        {
            var text = Application.Current?.TryFindResource(key) as string;
            return string.IsNullOrEmpty(text) ? fallback : text;
        }
    }
}
