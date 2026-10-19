using MMSP.Core.Model.Base;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 一条流程。写在客户库。选中状态不入库。
    /// </summary>
    [SugarTable("biz_flows")]
    public class FlowEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 名称。最长 32。
        /// </summary>
        [SugarColumn(ColumnName = "name", Length = 32)]
        public string Name { get; set; } = "";

        /// <summary>
        /// 是否启用。
        /// </summary>
        [SugarColumn(ColumnName = "is_enabled")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否输出日志。
        /// </summary>
        [SugarColumn(ColumnName = "log_output")]
        public bool LogOutput { get; set; }

        /// <summary>
        /// 左侧卡片从上到下的顺序。
        /// </summary>
        [SugarColumn(ColumnName = "sort_index")]
        public int SortIndex { get; set; }
    }
}
