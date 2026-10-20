using System.Windows.Controls;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using MMSP.Plugin.Lux.Views;
using MMSP.Plugin.Lux.WMI;

namespace MMSP.Plugin.Lux
{
    /// <summary>
    /// 立讯插件。按安装身份登记对上的工具和唯一的 MES 配置。
    /// </summary>
    public sealed class LuxModule : IModule
    {
        /// <summary>
        /// 流程页客户工具组的标题。
        /// </summary>
        private const string ToolGroupName = "LUX";

        /// <summary>
        /// 候选 MES 配置。名称随类型一起配置。
        /// </summary>
        private readonly record struct MesCandidate(Type ConfigType, string Name);

        /// <summary>
        /// 立讯插件可以提供的 MES 配置。
        /// </summary>
        private static readonly MesCandidate[] MesConfigs =
        [
            new(typeof(LuxMesWmiConfig), "WMI")
        ];

        /// <summary>
        /// 立讯插件可以提供的工具界面。
        /// </summary>
        private static readonly Type[] ToolViews = [typeof(LuxAlarmLogToolView)];

        /// <summary>
        /// 模块初始化。工具和 MES 配置在登记类型时已经放进容器。
        /// </summary>
        public void OnInitialized(IContainerProvider containerProvider)
        {
            return;
        }

        /// <summary>
        /// 筛出对上当前客户和设备类型的工具与 MES 配置，并放进容器。
        /// </summary>
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var profile = AppProfile.Current;
            var pick = PluginScope.Pick(
                MesConfigs.Select(item => item.ConfigType).ToArray(),
                ToolViews,
                profile.Customer,
                profile.DeviceType);
            foreach (var note in pick.Skipped)
                LogHelper.SysError(note);

            MesSource? mes = null;
            if (pick.MesConfigs.Count == 1)
            {
                var configType = pick.MesConfigs[0];
                var name = MesConfigs.Single(item => item.ConfigType == configType).Name;
                mes = new MesSource(name, configType);
                containerRegistry.RegisterSingleton(typeof(MesCfgModel), configType);
            }

            var tools = pick.Tools
                .Select(mark => new CustomerPluginTool(mark.Kind, cfg => CreateView(mark.ViewType, cfg)))
                .ToArray();
            containerRegistry.RegisterInstance(new CustomerPluginCatalog(ToolGroupName, tools, mes));
            if (mes is null)
                LogHelper.SysInfo("未加载 MES 配置");
            else
                LogHelper.MesInfo($"MES 来源 {mes.Name}");
        }

        /// <summary>
        /// 创建工具界面并装入已有配置。
        /// </summary>
        private static UserControl CreateView(Type viewType, string cfg)
        {
            if (Activator.CreateInstance(viewType) is not UserControl view)
                throw new InvalidOperationException($"{viewType.Name} 不是界面。");
            if (view is not ICustomerToolView tool)
                throw new InvalidOperationException($"{viewType.Name} 未实现工具界面。");

            tool.Load(cfg ?? "");
            return view;
        }
    }
}
