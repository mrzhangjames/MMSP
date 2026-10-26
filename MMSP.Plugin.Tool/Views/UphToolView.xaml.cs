using System.Windows.Controls;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.Plugin.Tool.Views
{
    /// <summary>
    /// UPH 工具的配置界面。
    /// </summary>
    public partial class UphToolView : UserControl
    {
        /// <summary>
        /// 创建 UPH 配置界面。
        /// </summary>
        public UphToolView()
        {
            DataContext = new UphToolViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// 换成另一份 UPH 配置。
        /// </summary>
        public void Load(UphToolEntity tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            DataContext = new UphToolViewModel(tool);
        }
    }
}
