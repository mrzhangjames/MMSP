using MMSP.App.Main.ViewModels;
using MMSP.App.Main.Views;
using MMSP.App.TemplateMatch;
using MMSP.Common.Helper.Logger;
using MMSP.Service.Database;
using MMSP.Service.Device.Plc;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MMSP.App.Main
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void RegisterTypes(IContainerRegistry services)
        {
            //Register：每一次解析都会创建一个实例
            //RegisterSingleton ：整个应用程序生命周期以内只创建一个实例（单例）
            //RegisterScoped ：在同一个Scope内只初始化一个实例
            //RegisterScoped ：在同一个Scope内只初始化一个实例

            ////注册服务（数据库、日志、文件操作）
            //services.RegisterSingleton<IServiceManaged, ServiceManaged>();
            ////注册系统操作
            //services.RegisterSingleton<ISystemManaged, SystemManaged>();
            ////注册菜单导航
            //services.RegisterSingleton<INavigationMenuService, NavigationMenuService>();

            //主窗体（window）
            services.RegisterForNavigation<MainView, MainViewModel>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            //TODO: 根据当前客户动态加载
            moduleCatalog.AddModule<TemplateMatchModule>();
            base.ConfigureModuleCatalog(moduleCatalog);
        }

        protected override Window? CreateShell()
        {
            return null;
        }

        protected override void OnInitialized()
        {
            SqlSugarContext.Open();
            LogHelper.Use(Container.Resolve<IEventAggregator>());
            if (HslLicense.Apply())
                LogHelper.SysInfo("HSL 授权成功");
            else
                LogHelper.SysError("HSL 授权失败");

            var missing = PlcClients.MissingProtocols();
            if (missing.Count > 0)
                LogHelper.SysError("PLC 协议未登记 " + string.Join(", ", missing));

            LogHelper.SysInfo("程序启动");

            //从容器换取MainView实例对象
            var container = ContainerLocator.Container;

            var shell = container.Resolve<object>("MainView");
            if (shell is Window view)
            {
                //更新Prism注册区域信息，通过region容器重新注入Window
                var regionManager = container.Resolve<IRegionManager>();
                RegionManager.SetRegionManager(view, regionManager);
                RegionManager.UpdateRegions();

                //找到首页的导航，初始化首页
                if (view.DataContext is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedTo(null);
                }

                App.Current.MainWindow = view;
                view.Show();
            }
            base.OnInitialized();
        }
    }

}
