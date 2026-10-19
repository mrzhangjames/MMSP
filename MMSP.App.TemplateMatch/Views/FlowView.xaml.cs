using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.TemplateMatch.ViewModels;

namespace MMSP.App.TemplateMatch.Views
{
    /// <summary>
    /// 流程页。双击流程卡片改名称，双击流程步打开工具配置。
    /// </summary>
    public partial class FlowView : UserControl
    {
        /// <summary>
        /// 创建流程页。
        /// </summary>
        public FlowView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 双击流程卡片打开配置弹窗。点在开关上时不打开。
        /// </summary>
        private void OnFlowCardPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || e.ClickCount != 2)
                return;

            if (IsInsideToggle(e.OriginalSource as DependencyObject))
                return;

            if (sender is not FrameworkElement { DataContext: FlowCard card } || DataContext is not FlowViewModel model)
                return;

            model.EditCommand.Execute(card);
            e.Handled = true;
        }

        /// <summary>
        /// 点击是否落在开关上。
        /// </summary>
        private static bool IsInsideToggle(DependencyObject? source)
        {
            while (source is not null)
            {
                if (source is ToggleButton)
                    return true;

                source = source is Visual ? VisualTreeHelper.GetParent(source) : LogicalTreeHelper.GetParent(source);
            }

            return false;
        }

        /// <summary>
        /// 双击流程步打开工具配置窗口。
        /// </summary>
        private void OnStepPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || e.ClickCount != 2)
                return;

            if (sender is not FrameworkElement { DataContext: FlowStep step } || DataContext is not FlowViewModel model)
                return;

            model.ShowPropertyCommand.Execute(step);
            e.Handled = true;
        }
    }
}
