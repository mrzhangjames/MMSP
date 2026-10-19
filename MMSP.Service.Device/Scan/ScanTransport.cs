namespace MMSP.Service.Device.Scan
{
    /// <summary>
    /// 扫码枪的传输方式。
    /// </summary>
    public enum ScanTransport
    {
        /// <summary>
        /// TCP 客户端。Host 填 IP 或主机名，Port 填端口。
        /// </summary>
        Tcp,

        /// <summary>
        /// 串口。Host 填 COM 口。
        /// </summary>
        Serial
    }
}
