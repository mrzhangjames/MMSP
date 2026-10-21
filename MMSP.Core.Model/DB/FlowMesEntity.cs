using MMSP.Core.Model.Base;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 流程页的 MES 配置。一行对应一种按客户和设备选出的配置类。
    /// </summary>
    [SugarTable("biz_flow_mes")]
    public class FlowMesEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 配置类全名。同一种配置只保留一行。
        /// </summary>
        [SugarColumn(ColumnName = "config_type", Length = 256)]
        public string ConfigType { get; set; } = "";

        /// <summary>
        /// 配置类公共属性的 JSON。
        /// </summary>
        [SugarColumn(ColumnName = "cfg_value", ColumnDataType = "text")]
        public string CfgValue { get; set; } = "";
    }
}
