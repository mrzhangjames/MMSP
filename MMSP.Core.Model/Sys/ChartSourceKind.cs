using System.ComponentModel;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 图表定义的数据来源。数值与客户库 source_kind 列一致。
    /// </summary>
    public enum ChartSourceKind
    {
        /// <summary>
        /// UPH。小时数据在 biz_uph。
        /// </summary>
        [Description("UPH")]
        Uph = 0,

        /// <summary>
        /// MES。枚举先占位，客户库还没有对应的表。
        /// </summary>
        [Description("MES")]
        Mes = 1
    }
}
