using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.Plugin.Tool.Views
{
    /// <summary>
    /// CSV 工具的配置界面。
    /// </summary>
    public partial class CsvToolView : UserControl
    {
        /// <summary>
        /// 创建 CSV 配置界面。
        /// </summary>
        public CsvToolView()
        {
            DataContext = new CsvToolViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// 换成另一份 CSV 配置。
        /// </summary>
        public void Load(CsvToolEntity tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            DataContext = new CsvToolViewModel(tool);
        }

        /// <summary>
        /// 填上当前流程名和变量，用来预览文件名、目录和数据来源。
        /// </summary>
        public void Bind(string? flowName, IReadOnlyList<string>? variables)
        {
            if (DataContext is CsvToolViewModel model)
                model.Bind(flowName, variables);
        }

        /// <summary>
        /// 在末尾加一行并滚到可见。
        /// </summary>
        private void AddRow(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CsvToolViewModel model)
                return;

            model.AddRow();
            Reveal();
            FocusEditor();
        }

        /// <summary>
        /// 编辑选中行。
        /// </summary>
        private void EditRow(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CsvToolViewModel model)
                return;

            model.EditSelected();
            Reveal();
            FocusEditor();
        }

        /// <summary>
        /// 删除选中行。
        /// </summary>
        private void DeleteRow(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CsvToolViewModel model)
                return;

            model.DeleteSelected();
        }

        /// <summary>
        /// 把正在编辑的行滚进列表。
        /// </summary>
        private void Reveal()
        {
            if (DataContext is CsvToolViewModel model && model.Selected is not null)
            {
                Grid.SelectedItem = model.Selected;
                Grid.ScrollIntoView(model.Selected);
            }
        }

        /// <summary>
        /// 等这一行画出来后，把光标放进列名。
        /// </summary>
        private void FocusEditor()
        {
            _ = Dispatcher.InvokeAsync(FocusColumn, DispatcherPriority.Loaded);
        }

        /// <summary>
        /// 聚焦正在编辑行的列名。
        /// </summary>
        private void FocusColumn()
        {
            if (DataContext is not CsvToolViewModel model || model.Selected is null)
                return;

            Grid.UpdateLayout();
            Grid.ScrollIntoView(model.Selected);
            if (Grid.ItemContainerGenerator.ContainerFromItem(model.Selected) is not DataGridRow row)
                return;

            FindBox(row)?.Focus();
        }

        /// <summary>
        /// 找到行里可见的列名输入框。
        /// </summary>
        private static TextBox? FindBox(DependencyObject parent)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var index = 0; index < count; index++)
            {
                var child = VisualTreeHelper.GetChild(parent, index);
                if (child is TextBox box && box.IsVisible)
                    return box;

                var nested = FindBox(child);
                if (nested is not null)
                    return nested;
            }

            return null;
        }
    }
}
