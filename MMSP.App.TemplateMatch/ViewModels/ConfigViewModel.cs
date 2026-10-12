using MMSP.App.WpfBase.Core;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 配置。只占位，不实现业务。
    /// </summary>
    public class ConfigViewModel : BaseViewModel
    {
        /// <summary>
        /// 创建配置。
        /// </summary>
        public ConfigViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator)
            : base(region, container, eventAggregator)
        {
        }
    }
}
