namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 一台 PLC 的连接参数。各协议只用自己需要的字段，其余保持默认。
    /// </summary>
    public sealed class PlcConnection
    {
        /// <summary>
        /// 协议。
        /// </summary>
        public PlcProtocol Protocol { get; init; }

        /// <summary>
        /// 网口填 IP，串口填 COM 口名称。
        /// </summary>
        public string Host { get; init; } = "";

        /// <summary>
        /// 网口端口。小于等于 0 时网口连接失败。串口不使用。
        /// </summary>
        public int Port { get; init; }

        /// <summary>
        /// 站号。默认 1。
        /// </summary>
        public byte Station { get; init; } = 1;

        /// <summary>
        /// 字节序。ABCD、CDAB、BADC、DCBA。Modbus 及其派生协议使用。
        /// </summary>
        public string DataFormat { get; init; } = "ABCD";

        /// <summary>
        /// 连接超时，毫秒。
        /// </summary>
        public int ConnectTimeout { get; init; } = 1000;

        /// <summary>
        /// 接收超时，毫秒。
        /// </summary>
        public int ReceiveTimeout { get; init; } = 1000;

        /// <summary>
        /// 西门子 S7 机架号。
        /// </summary>
        public int Rack { get; init; }

        /// <summary>
        /// 西门子 S7 槽号，也用于带 Slot 的 CIP 设备。
        /// </summary>
        public int Slot { get; init; }

        /// <summary>
        /// 系列。西门子 CPU、台达、汇川、信捷使用。认不出时保留库的默认值。
        /// </summary>
        public string Series { get; init; } = "";

        /// <summary>
        /// 串口波特率。小于等于 0 时用 9600。
        /// </summary>
        public int BaudRate { get; init; } = 9600;

        /// <summary>
        /// 串口校验位名称，例如 None、Even、Odd。HSL 设备对象公开该属性时才写入。
        /// </summary>
        public string Parity { get; init; } = "None";
    }
}
