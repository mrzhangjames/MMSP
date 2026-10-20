using System.Reflection;
using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 客户特性和设备特性是否对上安装身份。
    /// </summary>
    public enum ScopeResult
    {
        /// <summary>
        /// 客户和设备都对上。
        /// </summary>
        Matched,

        /// <summary>
        /// 没有客户特性。
        /// </summary>
        MissingCustomer,

        /// <summary>
        /// 客户特性不是当前客户。
        /// </summary>
        CustomerMismatch,

        /// <summary>
        /// 没有设备特性。
        /// </summary>
        MissingDevice,

        /// <summary>
        /// 设备特性里没有当前设备类型。
        /// </summary>
        DeviceMismatch
    }

    /// <summary>
    /// 一个对上安装身份的工具界面。
    /// </summary>
    public sealed class PluginToolMark
    {
        /// <summary>
        /// 记下界面类型和工具种类。
        /// </summary>
        public PluginToolMark(Type viewType, FlowToolKind kind)
        {
            ViewType = viewType ?? throw new ArgumentNullException(nameof(viewType));
            Kind = kind;
        }

        /// <summary>
        /// 工具界面类型。
        /// </summary>
        public Type ViewType { get; }

        /// <summary>
        /// 工具种类。
        /// </summary>
        public FlowToolKind Kind { get; }
    }

    /// <summary>
    /// 按安装身份筛过的 MES 配置和工具。
    /// </summary>
    public sealed class PluginPick
    {
        /// <summary>
        /// 记下筛完的结果。
        /// </summary>
        public PluginPick(
            IReadOnlyList<Type> mesConfigs,
            IReadOnlyList<PluginToolMark> tools,
            IReadOnlyList<string> skipped)
        {
            MesConfigs = mesConfigs;
            Tools = tools;
            Skipped = skipped;
        }

        /// <summary>
        /// 对上的 MES 配置。零个或一个。两个及以上在筛选时已经失败。
        /// </summary>
        public IReadOnlyList<Type> MesConfigs { get; }

        /// <summary>
        /// 对上的工具。
        /// </summary>
        public IReadOnlyList<PluginToolMark> Tools { get; }

        /// <summary>
        /// 跳过的候选和原因。
        /// </summary>
        public IReadOnlyList<string> Skipped { get; }
    }

    /// <summary>
    /// 用客户特性和设备特性筛选插件里登记的类型。
    /// </summary>
    public static class PluginScope
    {
        /// <summary>
        /// 比较类型上的特性和当前安装身份。
        /// </summary>
        public static ScopeResult Match(Type type, Customer customer, DeviceType deviceType)
        {
            ArgumentNullException.ThrowIfNull(type);

            var customerMark = type.GetCustomAttribute<CustomerAttribute>(inherit: false);
            if (customerMark is null || !Enum.IsDefined(customerMark.Customer))
                return ScopeResult.MissingCustomer;
            if (customerMark.Customer != customer)
                return ScopeResult.CustomerMismatch;

            var devices = type.GetCustomAttributes<DeviceTypeAttribute>(inherit: false).ToArray();
            if (devices.Length == 0 || devices.All(item => !Enum.IsDefined(item.DeviceType)))
                return ScopeResult.MissingDevice;
            if (!devices.Any(item => item.DeviceType == deviceType))
                return ScopeResult.DeviceMismatch;

            return ScopeResult.Matched;
        }

        /// <summary>
        /// 筛选结果的中文说明。
        /// </summary>
        public static string Describe(ScopeResult result)
        {
            return result switch
            {
                ScopeResult.Matched => "匹配",
                ScopeResult.MissingCustomer => "缺少客户特性",
                ScopeResult.CustomerMismatch => "客户特性与当前配置不一致",
                ScopeResult.MissingDevice => "缺少设备特性",
                ScopeResult.DeviceMismatch => "设备特性不包含当前设备类型",
                _ => "无法识别的筛选结果"
            };
        }

        /// <summary>
        /// 按安装身份筛选候选。MES 配置超过一个，或同一种工具出现两次时失败。
        /// </summary>
        public static PluginPick Pick(
            IReadOnlyList<Type> mesCandidates,
            IReadOnlyList<Type> toolCandidates,
            Customer customer,
            DeviceType deviceType)
        {
            ArgumentNullException.ThrowIfNull(mesCandidates);
            ArgumentNullException.ThrowIfNull(toolCandidates);

            var skipped = new List<string>();
            var mes = new List<Type>();
            foreach (var type in mesCandidates)
            {
                var result = Match(type, customer, deviceType);
                if (result != ScopeResult.Matched)
                {
                    skipped.Add($"跳过 MES 配置 {type.Name}：{Describe(result)}");
                    continue;
                }

                if (type.IsAbstract || !type.IsSubclassOf(typeof(MesCfgModel)))
                    throw new InvalidOperationException($"{type.Name} 不是 MesCfgModel 的具体子类。");

                mes.Add(type);
            }

            if (mes.Count > 1)
            {
                var names = string.Join("、", mes.Select(item => item.Name));
                throw new InvalidOperationException($"MES 配置匹配到 {mes.Count} 个：{names}。一次运行只能使用一个。");
            }

            var tools = new List<PluginToolMark>();
            var seen = new HashSet<FlowToolKind>();
            foreach (var type in toolCandidates)
            {
                var result = Match(type, customer, deviceType);
                if (result != ScopeResult.Matched)
                {
                    skipped.Add($"跳过工具 {type.Name}：{Describe(result)}");
                    continue;
                }

                var tool = type.GetCustomAttribute<FlowToolAttribute>(inherit: false);
                if (tool is null || !Enum.IsDefined(tool.Kind))
                {
                    skipped.Add($"跳过工具 {type.Name}：缺少工具特性");
                    continue;
                }

                if (!seen.Add(tool.Kind))
                    throw new InvalidOperationException($"工具 {tool.Kind} 匹配到多个类型。");

                tools.Add(new PluginToolMark(type, tool.Kind));
            }

            return new PluginPick(mes, tools, skipped);
        }
    }
}
