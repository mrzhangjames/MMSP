using System.Windows.Controls;
using System.Windows.Input;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.TemplateMatch.ViewModels;

namespace MMSP.App.TemplateMatch.Views
{
    /// <summary>
    /// 设备。PLC 和扫码枪按效果图分开展示。
    /// </summary>
    public partial class DeviceView : UserControl
    {
        /// <summary>
        /// 创建设备页。
        /// </summary>
        public DeviceView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 右键点位时先选中该行，菜单里的读取和写入作用在这一行上。
        /// </summary>
        private void OnPointRightClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemsControl.ContainerFromElement(PointGrid, e.OriginalSource as System.Windows.DependencyObject) is DataGridRow row)
                row.IsSelected = true;
        }

        /// <summary>
        /// 双击点位打开编辑。
        /// </summary>
        private void OnPointDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemsControl.ContainerFromElement(PointGrid, e.OriginalSource as System.Windows.DependencyObject) is not DataGridRow)
                return;

            if (DataContext is DeviceViewModel model)
                model.EditPointCommand.Execute();
        }

        /// <summary>
        /// 双击扫码枪卡片打开编辑。
        /// </summary>
        private void OnScannerDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListBoxItem { DataContext: ScannerCard } || DataContext is not DeviceViewModel model)
                return;

            model.EditScannerCommand.Execute();
        }
    }
}
