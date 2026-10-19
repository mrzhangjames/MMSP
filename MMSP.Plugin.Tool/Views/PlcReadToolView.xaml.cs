using System.Windows.Controls;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.Plugin.Tool.Views
{
    /// <summary>
    /// PLC 读取工具的配置界面。
    /// </summary>
    public partial class PlcReadToolView : UserControl
    {
        /// <summary>
        /// 创建PLC 读取配置界面。
        /// </summary>
        public PlcReadToolView()
        {
            DataContext = new PlcReadToolViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// 换成另一份PLC 读取配置。
        /// </summary>
        public void Load(PlcReadToolEntity tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            DataContext = new PlcReadToolViewModel(tool);
        }
    }
}
