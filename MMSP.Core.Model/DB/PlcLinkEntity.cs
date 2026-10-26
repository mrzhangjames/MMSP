using MMSP.Core.Model.Base;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 一台 PLC 的连接配置。写在客户库，只保留一行。
    /// </summary>
    [SugarTable("biz_plc_link")]
    public class PlcLinkEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// IP 地址。最长 64。
        /// </summary>
        [SugarColumn(ColumnName = "host", Length = 64)]
        public string Host { get; set; } = "";

        /// <summary>
        /// 协议。数值与 PlcProtocol 一致。新建行记为 ModbusTcp，已有值保持不变。
        /// </summary>
        [SugarColumn(ColumnName = "protocol")]
        public int Protocol { get; set; }

        /// <summary>
        /// 端口。
        /// </summary>
        [SugarColumn(ColumnName = "port")]
        public int Port { get; set; }

        /// <summary>
        /// 字节序。ABCD、CDAB、BADC、DCBA。
        /// </summary>
        [SugarColumn(ColumnName = "data_format", Length = 8)]
        public string DataFormat { get; set; } = "ABCD";

        /// <summary>
        /// 站号。0 到 255，默认 1。
        /// </summary>
        [SugarColumn(ColumnName = "station")]
        public int Station { get; set; } = 1;

        /// <summary>
        /// 轮询周期，毫秒。只保存。打开设备页不按它读取点位。
        /// </summary>
        [SugarColumn(ColumnName = "poll_ms")]
        public int PollMs { get; set; }

        /// <summary>
        /// 读取超时，毫秒。连接超时和接收超时都用它。
        /// </summary>
        [SugarColumn(ColumnName = "read_timeout_ms")]
        public int ReadTimeoutMs { get; set; } = 1000;

        /// <summary>
        /// 写入超时，毫秒。写入时当作这一次的接收超时。
        /// </summary>
        [SugarColumn(ColumnName = "write_timeout_ms")]
        public int WriteTimeoutMs { get; set; } = 1000;

        /// <summary>
        /// 重连间隔，毫秒。
        /// </summary>
        [SugarColumn(ColumnName = "reconnect_interval_ms")]
        public int ReconnectIntervalMs { get; set; }

        /// <summary>
        /// 重连次数。第一次失败之后再试这么多次。
        /// </summary>
        [SugarColumn(ColumnName = "reconnect_count")]
        public int ReconnectCount { get; set; }

        /// <summary>
        /// 心跳地址。空表示不启用。
        /// </summary>
        [SugarColumn(ColumnName = "heartbeat_address", Length = 64)]
        public string HeartbeatAddress { get; set; } = "";
    }
}
