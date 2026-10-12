using MMSP.App.WpfBase.Core;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 图表。只占位，不实现业务。
    /// </summary>
    public class ChartViewModel : BaseViewModel
    {
        /// <summary>
        /// 创建图表。
        /// </summary>
        public ChartViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator)
            : base(region, container, eventAggregator)
        {
        }
    }
}
