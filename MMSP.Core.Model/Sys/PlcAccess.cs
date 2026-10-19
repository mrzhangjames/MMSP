using System.ComponentModel;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// PLC 点位读写方式。数值与客户库 access 列一致。
    /// </summary>
    public enum PlcAccess
    {
        /// <summary>
        /// 读写。
        /// </summary>
        [Description("读写")]
        ReadWrite = 0,

        /// <summary>
        /// 只读。
        /// </summary>
        [Description("只读")]
        ReadOnly = 1
    }
}
