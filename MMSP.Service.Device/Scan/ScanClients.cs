namespace MMSP.Service.Device.Scan
{
    /// <summary>
    /// 创建扫码枪客户端。每把枪一个实例，不是单例。
    /// </summary>
    public static class ScanClients
    {
        /// <summary>
        /// 按传输方式创建客户端。
        /// </summary>
        public static IScanClient Open(ScanConnection connection)
        {
            ScanChannel channel = connection.Transport switch
            {
                ScanTransport.Tcp => new TcpScanChannel(connection),
                ScanTransport.Serial => new SerialScanChannel(connection),
                _ => throw new InvalidOperationException("扫码传输未登记 " + connection.Transport)
            };
            return new ScanClient(channel);
        }
    }
}
