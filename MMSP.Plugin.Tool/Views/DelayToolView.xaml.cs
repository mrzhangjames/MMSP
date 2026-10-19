using System.Windows.Controls;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.Plugin.Tool.Views
{
    /// <summary>
    /// 延时工具的配置界面。
    /// </summary>
    public partial class DelayToolView : UserControl
    {
        /// <summary>
        /// 创建延时配置界面。
        /// </summary>
        public DelayToolView()
        {
            DataContext = new DelayToolViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// 换成另一份延时配置。
        /// </summary>
        public void Load(DelayToolEntity tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            DataContext = new DelayToolViewModel(tool);
        }
    }
}
