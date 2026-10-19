namespace MMSP.Service.Device.Scan
{
    /// <summary>
    /// 一把扫码枪。调用方不接触套接字和串口。
    /// </summary>
    public interface IScanClient : IDisposable
    {
        /// <summary>
        /// 传输方式。
        /// </summary>
        ScanTransport Transport { get; }

        /// <summary>
        /// 当前是否已连接。
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 按连接参数建立通讯。失败时返回原因，不抛出。
        /// </summary>
        ScanIoResult Connect();

        /// <summary>
        /// 发送触发命令并读回一行。命令末尾会补上连接里的结束符。
        /// </summary>
        ScanIoResult Trigger(string command);

        /// <summary>
        /// 关闭连接。可以再次 Connect。
        /// </summary>
        void Close();
    }
}
