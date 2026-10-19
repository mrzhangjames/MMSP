using System.Windows.Controls;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.Plugin.Tool.Views
{
    /// <summary>
    /// 条件判断工具的配置界面。
    /// </summary>
    public partial class ConditionToolView : UserControl
    {
        /// <summary>
        /// 创建条件判断配置界面。
        /// </summary>
        public ConditionToolView()
        {
            DataContext = new ConditionToolViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// 换成另一份条件判断配置。
        /// </summary>
        public void Load(ConditionToolEntity tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            DataContext = new ConditionToolViewModel(tool);
        }
    }
}
