using System.ComponentModel;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// PLC 点位数据类型。数值与客户库 data_type 列一致。
    /// </summary>
    public enum PlcDataType
    {
        /// <summary>
        /// 16 位有符号整数。
        /// </summary>
        [Description("Int16")]
        Int16 = 0,

        /// <summary>
        /// 32 位有符号整数。
        /// </summary>
        [Description("Int32")]
        Int32 = 1,

        /// <summary>
        /// 单精度浮点。
        /// </summary>
        [Description("Float")]
        Float = 2,

        /// <summary>
        /// 字符串。
        /// </summary>
        [Description("String")]
        String = 3
    }
}
