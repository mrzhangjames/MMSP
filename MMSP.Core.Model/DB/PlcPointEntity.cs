using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// PLC 点位配置。写在客户库。当前值和勾选状态不入库。
    /// </summary>
    [SugarTable("biz_plc_points")]
    public class PlcPointEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 点位名称。最长 64。
        /// </summary>
        [SugarColumn(ColumnName = "name", Length = 64)]
        public string Name { get; set; } = "";

        /// <summary>
        /// 地址。最长 64，原样保存。
        /// </summary>
        [SugarColumn(ColumnName = "address", Length = 64)]
        public string Address { get; set; } = "";

        /// <summary>
        /// 数据类型。
        /// </summary>
        [SugarColumn(ColumnName = "data_type")]
        public PlcDataType DataType { get; set; }

        /// <summary>
        /// 长度。保存时要求是正整数。
        /// </summary>
        [SugarColumn(ColumnName = "data_length")]
        public int DataLength { get; set; }

        /// <summary>
        /// 读写方式。
        /// </summary>
        [SugarColumn(ColumnName = "access")]
        public PlcAccess Access { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        [SugarColumn(ColumnName = "is_valid")]
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// 列表从上到下的顺序。
        /// </summary>
        [SugarColumn(ColumnName = "sort_index")]
        public int SortIndex { get; set; }
    }
}
