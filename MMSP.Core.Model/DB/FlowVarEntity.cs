using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 一条流程变量。写在客户库，属于一条流程。运行中的值不入库。
    /// </summary>
    [SugarTable("biz_flow_vars")]
    public class FlowVarEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 所属流程。
        /// </summary>
        [SugarColumn(ColumnName = "flow_id")]
        public Guid FlowId { get; set; }

        /// <summary>
        /// 名称。最长 32。同一流程内唯一，区分大小写。
        /// </summary>
        [SugarColumn(ColumnName = "name", Length = 32)]
        public string Name { get; set; } = "";

        /// <summary>
        /// 数据类型。取值为 Int、string、double、bool。
        /// </summary>
        [SugarColumn(ColumnName = "data_type", Length = 16)]
        public string DataType { get; set; } = FlowVarType.String;

        /// <summary>
        /// 初始文本。启动时写入。默认空。
        /// </summary>
        [SugarColumn(ColumnName = "initial_value", ColumnDataType = "text")]
        public string InitialValue { get; set; } = "";

        /// <summary>
        /// 显示顺序。从 1 起，小的在前。
        /// </summary>
        [SugarColumn(ColumnName = "sort_index")]
        public int SortIndex { get; set; }
    }
}
