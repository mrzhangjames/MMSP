using System.Windows.Controls;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.Plugin.Tool.Views
{
    /// <summary>
    /// Json 解析工具的配置界面。
    /// </summary>
    public partial class JsonParseToolView : UserControl
    {
        /// <summary>
        /// 创建Json 解析配置界面。
        /// </summary>
        public JsonParseToolView()
        {
            DataContext = new JsonParseToolViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// 换成另一份Json 解析配置。
        /// </summary>
        public void Load(JsonParseToolEntity tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            DataContext = new JsonParseToolViewModel(tool);
        }
    }
}
