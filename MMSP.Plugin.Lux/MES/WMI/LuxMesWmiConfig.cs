using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;

namespace MMSP.Plugin.Lux.WMI
{
    /// <summary>
    /// Lux WMI 的 MES 配置。
    /// </summary>
    [Customer(Customer.Lux)]
    [DeviceType(DeviceType.PAD_001)]
    public class LuxMesWmiConfig : MesCfgModel
    {
    }
}

