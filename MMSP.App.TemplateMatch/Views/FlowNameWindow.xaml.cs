using System.Windows;
using MahApps.Metro.Controls;

namespace MMSP.App.TemplateMatch.Views
{
    /// <summary>
    /// 新增或编辑流程名称。使用 Metro 窗口。
    /// </summary>
    public partial class FlowNameWindow : MetroWindow
    {
        /// <summary>
        /// 打开名称窗口。error 是上次保存失败时带回来的说明。
        /// </summary>
        public FlowNameWindow(string title, string name, string error)
        {
            InitializeComponent();
            Title = title;
            NameBox.Text = name ?? "";
            ShowError(error);
            Loaded += (_, _) => NameBox.Focus();
        }

        /// <summary>
        /// 确定后的流程名称。取消时为空。
        /// </summary>
        public string? Result { get; private set; }

        /// <summary>
        /// 名称为空时留在窗口里。有名称时关闭并交回。
        /// </summary>
        private void Accept(object sender, RoutedEventArgs e)
        {
            var name = NameBox.Text.Trim();
            if (name.Length > 32)
                name = name[..32];

            if (name.Length == 0)
            {
                ShowError(FindResource("Flow_NeedName") as string ?? "请填写流程名称");
                return;
            }

            Result = name;
            DialogResult = true;
        }

        /// <summary>
        /// 显示或收起错误。
        /// </summary>
        private void ShowError(string? error)
        {
            var text = error?.Trim() ?? "";
            ErrorText.Text = text;
            ErrorText.Visibility = text.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
