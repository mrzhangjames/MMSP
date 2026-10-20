using System.ComponentModel;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 设备类型。
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// PAD_001。
        /// </summary>
        [Description("PAD_001")]
        PAD_001 = 0,
        [Description("PAD_002")]
        PAD_002 = 1,
        [Description("PAD_003")]
        PAD_003 = 2,
        [Description("EH_001")]
        EH_001 = 3,
    }
}
