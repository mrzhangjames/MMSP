using MMSP.App.WpfBase.Core;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 设备。只占位，不实现业务。
    /// </summary>
    public class DeviceViewModel : BaseViewModel
    {
        /// <summary>
        /// 创建设备。
        /// </summary>
        public DeviceViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator)
            : base(region, container, eventAggregator)
        {
        }
    }
}
