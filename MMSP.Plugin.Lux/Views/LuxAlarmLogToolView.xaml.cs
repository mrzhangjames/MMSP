using System.Windows;
using System.Windows.Controls;
using MMSP.App.WpfBase.Services;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool.PluginLux;
using MMSP.Plugin.Lux.ViewModels;

namespace MMSP.Plugin.Lux.Views
{
    /// <summary>
    /// 立讯报警日志的配置表格。
    /// </summary>
    [Customer(Customer.Lux)]
    [DeviceType(DeviceType.PAD_001)]
    [FlowTool(FlowToolKind.Lux_AlarmLogTool)]
    public partial class LuxAlarmLogToolView : UserControl, ICustomerToolView
    {
        /// <summary>
        /// 创建一张空表。
        /// </summary>
        public LuxAlarmLogToolView()
        {
            InitializeComponent();
            DataContext = new LuxAlarmLogToolViewModel(new LuxAlarmLogToolEntity());
        }

        /// <summary>
        /// 按步骤里的配置重填表格。
        /// </summary>
        public void Load(string cfg)
        {
            DataContext = LuxAlarmLogToolViewModel.FromCfg(cfg);
        }

        /// <summary>
        /// 把正在编辑的单元格写回行，再把新行放进集合。保存前调用。
        /// </summary>
        public void CommitEdit()
        {
            AlarmGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            AlarmGrid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        /// <summary>
        /// 选择 CSV 后替换表格。格式不对时保留当前行。
        /// </summary>
        private void ImportClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is LuxAlarmLogToolViewModel model)
                model.Import();
        }

        /// <summary>
        /// 先提交正在编辑的单元格，再把表格存成 CSV。
        /// </summary>
        private void ExportClick(object sender, RoutedEventArgs e)
        {
            CommitEdit();
            if (DataContext is LuxAlarmLogToolViewModel model)
                model.Export();
        }
    }
}
