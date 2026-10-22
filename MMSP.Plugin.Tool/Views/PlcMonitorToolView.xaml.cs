using System.Windows.Controls;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.Plugin.Tool.Views
{
    /// <summary>
    /// PLC 监控工具的配置界面。
    /// </summary>
    public partial class PlcMonitorToolView : UserControl
    {
        /// <summary>
        /// 创建PLC 监控配置界面。
        /// </summary>
        public PlcMonitorToolView()
        {
            DataContext = new PlcMonitorToolViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// 换成另一份 PLC 监控配置，并带上点位列表。
        /// </summary>
        public void Load(PlcMonitorToolEntity tool, IReadOnlyList<PlcPointChoice>? points = null)
        {
            ArgumentNullException.ThrowIfNull(tool);
            DataContext = new PlcMonitorToolViewModel(tool, points);
        }
    }
}
