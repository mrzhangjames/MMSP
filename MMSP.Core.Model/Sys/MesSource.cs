using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 本次运行选中的 MES 配置。一次只有一个。
    /// </summary>
    public sealed class MesSource
    {
        /// <summary>
        /// 记下名称和具体配置类型。
        /// </summary>
        public MesSource(string name, Type configType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("MES 来源名称是空的。", nameof(name));
            if (configType is null)
                throw new ArgumentNullException(nameof(configType));
            if (configType.IsAbstract || !configType.IsSubclassOf(typeof(MesCfgModel)))
                throw new ArgumentException("MES 配置必须是 MesCfgModel 的具体子类。", nameof(configType));

            Name = name.Trim();
            ConfigType = configType;
        }

        /// <summary>
        /// 来源名称。图表在没有数据时显示它。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 选中的配置类型。
        /// </summary>
        public Type ConfigType { get; }
    }
}
