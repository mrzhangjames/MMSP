using System.Windows.Controls;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.Plugin.Tool.Views
{
    /// <summary>
    /// PLC 写入工具的配置界面。
    /// </summary>
    public partial class PlcWriteToolView : UserControl
    {
        /// <summary>
        /// 创建PLC 写入配置界面。
        /// </summary>
        public PlcWriteToolView()
        {
            DataContext = new PlcWriteToolViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// 换成另一份 PLC 写入配置，并带上点位列表。
        /// </summary>
        public void Load(PlcWriteToolEntity tool, IReadOnlyList<PlcPointChoice>? points = null)
        {
            ArgumentNullException.ThrowIfNull(tool);
            DataContext = new PlcWriteToolViewModel(tool, points);
        }
    }
}
