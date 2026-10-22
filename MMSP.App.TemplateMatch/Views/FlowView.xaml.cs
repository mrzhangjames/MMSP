using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.TemplateMatch.ViewModels;

namespace MMSP.App.TemplateMatch.Views
{
    /// <summary>
    /// 流程页。右键流程卡片改名称或变量，双击流程步打开工具配置。
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
        /// 右键流程卡片时先选中它。空白区域没有这个菜单。
        /// </summary>
        private void OnFlowCardPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: FlowCard card } || DataContext is not FlowViewModel model)
                return;

            model.SelectCommand.Execute(card);
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
