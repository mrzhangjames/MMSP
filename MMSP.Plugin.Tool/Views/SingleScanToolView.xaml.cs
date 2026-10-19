using System.Windows.Controls;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.Plugin.Tool.Views
{
    /// <summary>
    /// 单扫码工具的配置界面。
    /// </summary>
    public partial class SingleScanToolView : UserControl
    {
        /// <summary>
        /// 创建单扫码配置界面。
        /// </summary>
        public SingleScanToolView()
        {
            DataContext = new SingleScanToolViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// 换成另一份单扫码配置。scanners 是设备里的扫码枪。
        /// </summary>
        public void Load(SingleScanToolEntity tool, IReadOnlyList<string> scanners)
        {
            ArgumentNullException.ThrowIfNull(tool);
            ArgumentNullException.ThrowIfNull(scanners);
            DataContext = new SingleScanToolViewModel(tool, scanners);
        }
    }
}
