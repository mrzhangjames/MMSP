using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MMSP.App.WpfBase.Services;

namespace MMSP.App.Main.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : MetroWindow
    {
        /// <summary>
        /// 用户已经在对话框里确认退出。
        /// </summary>
        private bool _exitConfirmed;

        /// <summary>
        /// 退出确认框正在显示，避免重复弹出。
        /// </summary>
        private bool _exitPromptOpen;

        /// <summary>
        /// 创建主窗。
        /// </summary>
        public MainView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 退出按钮走和标题栏关闭相同的确认框。
        /// </summary>
        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 登录窗打开后把焦点放在登录名。按名字查找，不依赖生成字段。
        /// </summary>
        private void OnLoginOverlayVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true && FindName("LoginNameBox") is IInputElement box)
                box.Focus();
        }

        /// <summary>
        /// 关闭前用 Metro 对话框确认。取消则留下窗口。
        /// </summary>
        protected override async void OnClosing(CancelEventArgs e)
        {
            if (_exitConfirmed)
            {
                base.OnClosing(e);
                return;
            }

            e.Cancel = true;
            if (_exitPromptOpen)
                return;

            _exitPromptOpen = true;
            try
            {
                var settings = new MetroDialogSettings
                {
                    AffirmativeButtonText = Text("Exit_Confirm", "确认"),
                    NegativeButtonText = Text("Exit_Cancel", "取消"),
                    OwnerCanCloseWithDialog = false
                };
                var confirmed = await MetroDialogService.ConfirmAsync(
                    Text("Exit_Title", "退出"),
                    Text("Exit_Message", "是否确认退出？"),
                    settings);
                if (confirmed)
                {
                    _exitConfirmed = true;
                    Close();
                }
            }
            finally
            {
                _exitPromptOpen = false;
            }
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
