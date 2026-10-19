namespace MMSP.Service.Device.Scan
{
    /// <summary>
    /// 一把扫码枪的调用入口。同一条连接上的连接、触发和关闭排队执行。
    /// </summary>
    public sealed class ScanClient : IScanClient
    {
        private readonly ScanChannel _channel;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private bool _disposed;

        /// <summary>
        /// 接上已经按传输方式创建好的通道。
        /// </summary>
        internal ScanClient(ScanChannel channel)
        {
            _channel = channel;
        }

        /// <summary>
        /// 传输方式。
        /// </summary>
        public ScanTransport Transport => _channel.Transport;

        /// <summary>
        /// 是否已连接。
        /// </summary>
        public bool IsConnected => _channel.IsConnected;

        /// <summary>
        /// 建立通讯。
        /// </summary>
        public ScanIoResult Connect()
        {
            return Run(_channel.Connect);
        }

        /// <summary>
        /// 发送触发命令并读回一行。
        /// </summary>
        public ScanIoResult Trigger(string command)
        {
            return Run(() => _channel.Trigger(command));
        }

        /// <summary>
        /// 关闭连接。
        /// </summary>
        public void Close()
        {
            _gate.Wait();
            try
            {
                _channel.Close();
            }
            finally
            {
                _gate.Release();
            }
        }

        /// <summary>
        /// 关闭并释放排队锁。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Close();
            _gate.Dispose();
        }

        /// <summary>
        /// 排队执行一次收发。对象已释放时直接失败。
        /// </summary>
        private ScanIoResult Run(Func<ScanIoResult> action)
        {
            if (_disposed)
                return ScanIoResult.Fail("连接已释放");

            _gate.Wait();
            try
            {
                return _disposed ? ScanIoResult.Fail("连接已释放") : action();
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
