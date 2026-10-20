using System.Windows.Controls;
using MMSP.Core.Model.Sys;

namespace MMSP.App.WpfBase.Services
{
    /// <summary>
    /// 当前客户插件登记的一个工具。
    /// </summary>
    public sealed class CustomerPluginTool
    {
        /// <summary>
        /// 按配置创建界面。
        /// </summary>
        private readonly Func<string, UserControl> _create;

        /// <summary>
        /// 记下种类和创建方法。
        /// </summary>
        public CustomerPluginTool(FlowToolKind kind, Func<string, UserControl> create)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            Kind = kind;
        }

        /// <summary>
        /// 工具种类。
        /// </summary>
        public FlowToolKind Kind { get; }

        /// <summary>
        /// 创建已经装入配置的界面。
        /// </summary>
        public UserControl Create(string cfg)
        {
            return _create(cfg ?? "");
        }
    }

    /// <summary>
    /// 当前客户插件登记的工具和 MES 来源。一次运行一份。
    /// </summary>
    public sealed class CustomerPluginCatalog
    {
        /// <summary>
        /// 记下工具组名称、工具和 MES 来源。
        /// </summary>
        public CustomerPluginCatalog(string toolGroupName, IReadOnlyList<CustomerPluginTool> tools, MesSource? mes)
        {
            ToolGroupName = toolGroupName ?? "";
            Tools = tools ?? [];
            Mes = mes;
        }

        /// <summary>
        /// 流程页客户工具组的标题。
        /// </summary>
        public string ToolGroupName { get; }

        /// <summary>
        /// 已加载的客户工具。
        /// </summary>
        public IReadOnlyList<CustomerPluginTool> Tools { get; }

        /// <summary>
        /// 已加载的 MES 来源。没有对上的配置时为空。
        /// </summary>
        public MesSource? Mes { get; }

        /// <summary>
        /// 按种类创建界面。当前客户没有这个工具时返回空。
        /// </summary>
        public UserControl? Create(FlowToolKind kind, string cfg)
        {
            foreach (var tool in Tools)
            {
                if (tool.Kind == kind)
                    return tool.Create(cfg);
            }

            return null;
        }
    }
}
