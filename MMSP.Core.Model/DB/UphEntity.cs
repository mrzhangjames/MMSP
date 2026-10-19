using MMSP.Core.Model.Base;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// UPH 小时数据。写在客户库，一小时一行。
    /// </summary>
    [SugarTable("biz_uph")]
    public class UphEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 小时起点。同一小时只保留一行。
        /// </summary>
        [SugarColumn(ColumnName = "hour_time")]
        public DateTime HourTime { get; set; }

        /// <summary>
        /// 小时产量。
        /// </summary>
        [SugarColumn(ColumnName = "hour_output")]
        public int HourOutput { get; set; }

        /// <summary>
        /// 该小时的累计产量。
        /// </summary>
        [SugarColumn(ColumnName = "total_output")]
        public int TotalOutput { get; set; }

        /// <summary>
        /// 良品数。
        /// </summary>
        [SugarColumn(ColumnName = "good_count")]
        public int GoodCount { get; set; }

        /// <summary>
        /// 不良数。良品率用良品数除以良品数与不良数之和。
        /// </summary>
        [SugarColumn(ColumnName = "defect_count")]
        public int DefectCount { get; set; }
    }
}
