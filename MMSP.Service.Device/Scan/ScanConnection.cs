namespace MMSP.Service.Device.Scan
{
    /// <summary>
    /// 一把扫码枪的连接参数。TCP 和串口共用这一份，各自只用自己的字段。
    /// </summary>
    public sealed class ScanConnection
    {
        /// <summary>
        /// 传输方式。
        /// </summary>
        public ScanTransport Transport { get; init; }

        /// <summary>
        /// TCP 填 IP 或主机名，串口填 COM 口。
        /// </summary>
        public string Host { get; init; } = "";

        /// <summary>
        /// TCP 端口。小于等于 0 时 TCP 连接失败。串口不使用。
        /// </summary>
        public int Port { get; init; }

        /// <summary>
        /// 串口波特率。小于等于 0 时用 9600。
        /// </summary>
        public int BaudRate { get; init; } = 9600;

        /// <summary>
        /// 串口校验位名称。None、Even、Odd、Mark、Space。认不出时用 None。
        /// </summary>
        public string Parity { get; init; } = "None";

        /// <summary>
        /// 串口数据位。不在 5 到 8 时用 8。
        /// </summary>
        public int DataBits { get; init; } = 8;

        /// <summary>
        /// 串口停止位名称。One、Two、OnePointFive。认不出时用 One。
        /// </summary>
        public string StopBits { get; init; } = "One";

        /// <summary>
        /// 连接超时，毫秒。小于等于 0 时按 1000。
        /// </summary>
        public int ConnectTimeout { get; init; } = 1000;

        /// <summary>
        /// 接收超时，毫秒。小于等于 0 时按 1000。
        /// </summary>
        public int ReceiveTimeout { get; init; } = 1000;

        /// <summary>
        /// 命令结尾和应答结束符。空白时用回车。
        /// </summary>
        public string NewLine { get; init; } = "\r";

        /// <summary>
        /// 编码名。ASCII 或 UTF-8。其他名称按 ASCII。
        /// </summary>
        public string EncodingName { get; init; } = "ASCII";
    }
}
