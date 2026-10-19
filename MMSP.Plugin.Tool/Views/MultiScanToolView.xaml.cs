using System.Windows.Controls;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.Plugin.Tool.Views
{
    /// <summary>
    /// 多扫码工具的配置界面。
    /// </summary>
    public partial class MultiScanToolView : UserControl
    {
        /// <summary>
        /// 创建多扫码配置界面。
        /// </summary>
        public MultiScanToolView()
        {
            DataContext = new MultiScanToolViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// 换成另一份多扫码配置。scanners 是设备里的扫码枪。
        /// </summary>
        public void Load(MultiScanToolEntity tool, IReadOnlyList<string> scanners)
        {
            ArgumentNullException.ThrowIfNull(tool);
            ArgumentNullException.ThrowIfNull(scanners);
            DataContext = new MultiScanToolViewModel(tool, scanners);
        }
    }
}
