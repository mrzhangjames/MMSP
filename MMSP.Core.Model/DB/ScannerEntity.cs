using MMSP.Core.Model.Base;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 一把扫码枪的配置。写在客户库。名称和地址可以重复。
    /// </summary>
    [SugarTable("biz_scanners")]
    public class ScannerEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键。和页面卡片 Id 相同。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 名称。最长 64。
        /// </summary>
        [SugarColumn(ColumnName = "name", Length = 64)]
        public string Name { get; set; } = "";

        /// <summary>
        /// 传输。0 为 TCP，1 为串口。
        /// </summary>
        [SugarColumn(ColumnName = "transport")]
        public int Transport { get; set; }

        /// <summary>
        /// TCP 填 IP，串口填 COM 口。最长 64。
        /// </summary>
        [SugarColumn(ColumnName = "host", Length = 64)]
        public string Host { get; set; } = "";

        /// <summary>
        /// TCP 端口。串口为 0。
        /// </summary>
        [SugarColumn(ColumnName = "port")]
        public int Port { get; set; }

        /// <summary>
        /// 触发字符串。最长 64。
        /// </summary>
        [SugarColumn(ColumnName = "trigger_text", Length = 64)]
        public string TriggerText { get; set; } = "";

        /// <summary>
        /// 串口波特率。
        /// </summary>
        [SugarColumn(ColumnName = "baud_rate")]
        public int BaudRate { get; set; } = 9600;

        /// <summary>
        /// 串口校验位。
        /// </summary>
        [SugarColumn(ColumnName = "parity", Length = 16)]
        public string Parity { get; set; } = "None";

        /// <summary>
        /// 串口数据位。
        /// </summary>
        [SugarColumn(ColumnName = "data_bits")]
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// 串口停止位。
        /// </summary>
        [SugarColumn(ColumnName = "stop_bits", Length = 16)]
        public string StopBits { get; set; } = "One";

        /// <summary>
        /// 连接超时，毫秒。
        /// </summary>
        [SugarColumn(ColumnName = "connect_timeout")]
        public int ConnectTimeout { get; set; } = 1000;

        /// <summary>
        /// 接收超时，毫秒。
        /// </summary>
        [SugarColumn(ColumnName = "receive_timeout")]
        public int ReceiveTimeout { get; set; } = 1000;

        /// <summary>
        /// 命令和应答的结束符。
        /// </summary>
        [SugarColumn(ColumnName = "new_line", Length = 8)]
        public string NewLine { get; set; } = "\r";

        /// <summary>
        /// 编码名。
        /// </summary>
        [SugarColumn(ColumnName = "encoding_name", Length = 16)]
        public string EncodingName { get; set; } = "ASCII";

        /// <summary>
        /// 是否启用。
        /// </summary>
        [SugarColumn(ColumnName = "is_valid")]
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// 卡片从上到下的顺序。
        /// </summary>
        [SugarColumn(ColumnName = "sort_index")]
        public int SortIndex { get; set; }
    }
}
