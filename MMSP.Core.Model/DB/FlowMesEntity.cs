using MMSP.Core.Model.Base;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 流程页的 MES 连接。写在客户库，只使用一行。这不是报文表。
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
        /// 接口地址。最长 256。
        /// </summary>
        [SugarColumn(ColumnName = "endpoint", Length = 256)]
        public string Endpoint { get; set; } = "";

        /// <summary>
        /// 工位。最长 32。
        /// </summary>
        [SugarColumn(ColumnName = "station", Length = 32)]
        public string Station { get; set; } = "";

        /// <summary>
        /// 超时，毫秒。
        /// </summary>
        [SugarColumn(ColumnName = "timeout_ms")]
        public int TimeoutMs { get; set; }

        /// <summary>
        /// 重试次数。
        /// </summary>
        [SugarColumn(ColumnName = "retry_count")]
        public int RetryCount { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        [SugarColumn(ColumnName = "is_enabled")]
        public bool IsEnabled { get; set; }
    }
}
