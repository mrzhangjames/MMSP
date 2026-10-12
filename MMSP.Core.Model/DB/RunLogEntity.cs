using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 运行日志。写在客户库。
    /// </summary>
    [SugarTable("biz_logs")]
    public class RunLogEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 日志时间。
        /// </summary>
        [SugarColumn(ColumnName = "log_time")]
        public DateTime LogTime { get; set; }

        /// <summary>
        /// 级别。
        /// </summary>
        [SugarColumn(ColumnName = "level")]
        public RunLogLevel Level { get; set; }

        /// <summary>
        /// 业务类型。Sys、Plc、Scan、Flow、Mes。
        /// </summary>
        [SugarColumn(ColumnName = "biz_type")]
        public RunLogBiz BizType { get; set; }

        /// <summary>
        /// 内容。
        /// </summary>
        [SugarColumn(ColumnName = "message")]
        public string Message { get; set; } = "";

        /// <summary>
        /// 异常文本。主页不显示。
        /// </summary>
        [SugarColumn(ColumnName = "exception", IsNullable = true)]
        public string? Exception { get; set; }

        /// <summary>
        /// 来源类别。主页不显示。
        /// </summary>
        [SugarColumn(ColumnName = "logger", IsNullable = true)]
        public string? Logger { get; set; }
    }
}
