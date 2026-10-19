using MMSP.Core.Model.Sys;

namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 一台 PLC 的调用入口。同一条连接上的连接、读取和写入排队执行。
    /// </summary>
    public sealed class PlcClient : IPlcClient
    {
        private readonly HslPlcDriver _driver;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private bool _disposed;

        /// <summary>
        /// 接上已经按协议创建好的驱动。
        /// </summary>
        internal PlcClient(HslPlcDriver driver)
        {
            _driver = driver;
        }

        /// <summary>
        /// 协议。
        /// </summary>
        public PlcProtocol Protocol => _driver.Protocol;

        /// <summary>
        /// 是否已连接。
        /// </summary>
        public bool IsConnected => _driver.IsConnected;

        /// <summary>
        /// 建立通讯。
        /// </summary>
        public PlcIoResult Connect()
        {
            return Run(_driver.Connect);
        }

        /// <summary>
        /// 读取点位。
        /// </summary>
        public PlcIoResult Read(string address, PlcDataType dataType, int length)
        {
            return Run(() => _driver.Read(address, dataType, length));
        }

        /// <summary>
        /// 写入点位。
        /// </summary>
        public PlcIoResult Write(string address, PlcDataType dataType, string value)
        {
            return Run(() => _driver.Write(address, dataType, value));
        }

        /// <summary>
        /// 关闭连接。
        /// </summary>
        public void Close()
        {
            _gate.Wait();
            try
            {
                _driver.Close();
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
        private PlcIoResult Run(Func<PlcIoResult> action)
        {
            if (_disposed)
                return PlcIoResult.Fail("连接已释放");

            _gate.Wait();
            try
            {
                return _disposed ? PlcIoResult.Fail("连接已释放") : action();
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
