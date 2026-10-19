using System.Windows.Controls;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.Plugin.Tool.Views
{
    /// <summary>
    /// 脚本工具的配置界面。
    /// </summary>
    public partial class ScriptToolView : UserControl
    {
        /// <summary>
        /// 创建脚本配置界面。
        /// </summary>
        public ScriptToolView()
        {
            DataContext = new ScriptToolViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// 换成另一份脚本配置。
        /// </summary>
        public void Load(ScriptToolEntity tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            DataContext = new ScriptToolViewModel(tool);
        }
    }
}
