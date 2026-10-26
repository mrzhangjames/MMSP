using MMSP.App.Main.Services;
using MMSP.App.Main.ViewModels;
using MMSP.App.Main.Views;
using MMSP.App.TemplateMatch;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper.Logger;
using MMSP.Core.License;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Plugin.Lux;
using MMSP.Service.Database;
using MMSP.Service.Device.Plc;
using MMSP.Service.Flow;
using System.IO;
using System.Windows;

namespace MMSP.App.Main
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        /// <summary>
        /// 主程序 Config 目录里的许可证文件。
        /// </summary>
        private readonly LicenseDirectoryStore _licenseFiles = new(Path.Combine(PathCore.Folder_App, PathCore.Folder_Config));

        /// <summary>
        /// 配置页读取的当前许可证。
        /// </summary>
        private readonly LicenseStatusStore _licenseStatus = new();

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
            services.RegisterInstance(_licenseFiles);
            services.RegisterInstance(_licenseStatus);
            services.RegisterSingleton<IFlowRunService, FlowRunService>();
            services.RegisterSingleton<UserSession>();
            services.RegisterForNavigation<MainView, MainViewModel>();
        }

        /// <summary>
        /// 先读安装身份。配置无效或没有对应客户插件时直接退出。
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                AppProfile.Use(AppProfileLoader.Load());
                LoadLicense();
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                LogHelper.SysError("应用程序配置无效", ex);
                MessageBox.Show(ex.Message, "MMSP", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        /// <summary>
        /// 壳模块始终加载。客户插件按安装身份选择，对不上就失败。
        /// </summary>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<TemplateMatchModule>();
            var profile = AppProfile.Current;
            if (profile.Customer == Customer.Lux && profile.DeviceType == DeviceType.PAD_001)
                moduleCatalog.AddModule<LuxModule>();
            else
                throw new InvalidOperationException($"没有对应的客户插件：Customer={profile.Customer}，DeviceType={profile.DeviceType}。");

            base.ConfigureModuleCatalog(moduleCatalog);
        }

        /// <summary>
        /// 读取本机许可证供配置页显示。没有许可证或校验失败也继续启动。
        /// </summary>
        private void LoadLicense()
        {
            var fingerprint = MachineFingerprintReader.TryRead(out var value) ? value : "";
            LicenseEvaluation evaluation;
            try
            {
                evaluation = LicenseRuntime.Check(_licenseFiles, fingerprint, DateTimeOffset.UtcNow);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                LogHelper.SysError("读取许可证失败", ex);
                evaluation = LicenseEvaluation.Only(LicenseFailureCode.FileUnreadable);
            }

            if (evaluation.IsValid)
            {
                try
                {
                    LicenseRuntime.Remember(_licenseFiles, DateTimeOffset.UtcNow);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    LogHelper.SysError("写入许可证水位失败", ex);
                }
            }

            _licenseStatus.Replace(evaluation);
        }

        protected override Window? CreateShell()
        {
            return null;
        }

        protected override void OnInitialized()
        {
            SqlSugarContext.Open();
            var config = AppConfigStore.Load();
            AppAppearance.Apply(config);
            LogHelper.Use(Container.Resolve<IEventAggregator>());
            var profile = AppProfile.Current;
            LogHelper.SysInfo($"当前客户 {profile.Customer}，设备类型 {profile.DeviceType}");
            LogHelper.SysInfo("许可证 " + _licenseStatus.Current.ToLogText());
            try
            {
                LogRetentionCleaner.Apply(config.DbLogDays, config.FileLogDays);
            }
            catch (Exception ex)
            {
                LogHelper.SysError("清理过期日志失败", ex);
            }
            if (HslLicense.Apply())
                LogHelper.SysInfo("HSL 授权成功");
            else
                LogHelper.SysError("HSL 授权失败");

            var missing = PlcClients.MissingProtocols();
            if (missing.Count > 0)
                LogHelper.SysError("PLC 协议未登记 " + string.Join(", ", missing));

            LogHelper.SysInfo("程序启动");
            StartPlcHeartbeat();

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
                AppAppearance.ApplyTitle(config.WindowTitle);
                view.Show();
            }
            base.OnInitialized();
        }

        /// <summary>
        /// 用客户库里的连接启动心跳。地址为空时任务在等，不连接 PLC。
        /// </summary>
        private static void StartPlcHeartbeat()
        {
            PlcHeartbeat.Shared.Failure = message => LogHelper.PlcError(message);
            PlcLinkEntity? row = null;
            try
            {
                if (SqlSugarContext.IsOpen)
                    row = SqlSugarContext.CustomerDb.Queryable<PlcLinkEntity>().ToList().FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogHelper.PlcError("读取心跳配置失败 " + ex.Message);
            }

            if (row is null)
            {
                PlcHeartbeat.Shared.Apply(null, "", 0);
                return;
            }

            var format = row.DataFormat;
            if (format is not ("ABCD" or "CDAB" or "BADC" or "DCBA"))
                format = "ABCD";

            PlcConnection? connection = null;
            if (Enum.IsDefined(typeof(PlcProtocol), row.Protocol))
            {
                connection = PlcConnections.Create(
                    (PlcProtocol)row.Protocol,
                    row.Host,
                    row.Port,
                    row.Station,
                    format,
                    row.ReadTimeoutMs,
                    row.WriteTimeoutMs);
            }

            PlcHeartbeat.Shared.Apply(connection, row.HeartbeatAddress, row.ReconnectIntervalMs);
        }

        /// <summary>
        /// 退出时先停底栏探测，再停心跳连接。
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            DeviceLinkWatch.Shared.Stop();
            PlcHeartbeat.Shared.Stop();
            base.OnExit(e);
        }
    }

}
