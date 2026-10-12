using MMSP.App.TemplateMatch.ViewModels;
using MMSP.App.TemplateMatch.Views;

namespace MMSP.App.TemplateMatch
{
    /// <summary>
    /// 模板匹配模块。登记主界面六个菜单对应的页面。
    /// </summary>
    public class TemplateMatchModule : IModule
    {
        /// <summary>
        /// 模块初始化。这里不打开页面。
        /// </summary>
        public void OnInitialized(IContainerProvider containerProvider)
        {
            return;
        }

        /// <summary>
        /// 按菜单顺序登记页面。导航名是视图类名。
        /// </summary>
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<HomeView, HomeViewModel>();
            containerRegistry.RegisterForNavigation<FlowView, FlowViewModel>();
            containerRegistry.RegisterForNavigation<DeviceView, DeviceViewModel>();
            containerRegistry.RegisterForNavigation<ChartView, ChartViewModel>();
            containerRegistry.RegisterForNavigation<ConfigView, ConfigViewModel>();
            containerRegistry.RegisterForNavigation<UserView, UserViewModel>();
        }
    }
}
