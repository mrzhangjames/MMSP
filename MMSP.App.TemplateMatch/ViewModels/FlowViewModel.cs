using MMSP.App.WpfBase.Core;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 流程。只占位，不实现业务。
    /// </summary>
    public class FlowViewModel : BaseViewModel
    {
        /// <summary>
        /// 创建流程。
        /// </summary>
        public FlowViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator)
            : base(region, container, eventAggregator)
        {
        }
    }
}
