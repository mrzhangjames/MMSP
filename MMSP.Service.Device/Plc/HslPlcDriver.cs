using HslCommunication.Core;
using HslCommunication.Core.Device;
using MMSP.Core.Model.Sys;

namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// HSL 设备适配器。子类只负责创建对象，连接、读写和关闭在这里完成。
    /// </summary>
    internal abstract class HslPlcDriver : IDisposable
    {
        private IReadWriteNet? _device;

        /// <summary>
        /// 记下连接参数。
        /// </summary>
        protected HslPlcDriver(PlcConnection connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// 连接参数。
        /// </summary>
        protected PlcConnection Connection { get; }

        /// <summary>
        /// 协议。
        /// </summary>
        public PlcProtocol Protocol => Connection.Protocol;

        /// <summary>
        /// 是否已连接。
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// 创建 HSL 设备并打开。地址为空时不创建对象。
        /// </summary>
        public PlcIoResult Connect()
        {
            Close();
            if (string.IsNullOrWhiteSpace(Connection.Host))
                return PlcIoResult.Fail("地址为空");

            if (!HslLicense.IsAuthorized)
                return PlcIoResult.Fail("HSL 授权失败");

            var invalid = Validate(Connection);
            if (invalid is not null)
                return invalid;

            try
            {
                _device = CreateDevice(Connection);
                var result = Open(_device);
                IsConnected = result.IsSuccess;
                if (!result.IsSuccess)
                    Close();

                return result;
            }
            catch (Exception ex)
            {
                Close();
                return PlcIoResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 读取。尚未连接时不访问设备。
        /// </summary>
        public PlcIoResult Read(string address, PlcDataType dataType, int length)
        {
            if (_device is null || !IsConnected)
                return PlcIoResult.Fail("尚未连接");

            return PlcValueCodec.Read(_device, address, dataType, length);
        }

        /// <summary>
        /// 写入。尚未连接时不访问设备。
        /// </summary>
        public PlcIoResult Write(string address, PlcDataType dataType, string value)
        {
            if (_device is null || !IsConnected)
                return PlcIoResult.Fail("尚未连接");

            return PlcValueCodec.Write(_device, address, dataType, value);
        }

        /// <summary>
        /// 关闭当前设备。
        /// </summary>
        public void Close()
        {
            var device = _device;
            _device = null;
            IsConnected = false;
            if (device is null)
                return;

            try
            {
                Shutdown(device);
            }
            catch (Exception)
            {
                // 关闭失败不再向外抛。下次 Connect 会新建对象。
            }
        }

        /// <summary>
        /// 关闭连接。
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// 按协议创建 HSL 对象。
        /// </summary>
        protected abstract IReadWriteNet CreateDevice(PlcConnection connection);

        /// <summary>
        /// 参数不完整时返回失败。通过时返回 null。
        /// </summary>
        protected virtual PlcIoResult? Validate(PlcConnection connection)
        {
            return null;
        }

        /// <summary>
        /// 打开已经创建好的设备。
        /// </summary>
        protected abstract PlcIoResult Open(IReadWriteNet device);

        /// <summary>
        /// 关闭已经创建好的设备。
        /// </summary>
        protected abstract void Shutdown(IReadWriteNet device);
    }

    /// <summary>
    /// 网口设备。端口无效时不连接。UDP 设备没有 ConnectServer，参数就绪即视为连接成功。
    /// </summary>
    internal abstract class NetworkPlcDriver : HslPlcDriver
    {
        /// <summary>
        /// 记下连接参数。
        /// </summary>
        protected NetworkPlcDriver(PlcConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// 网口必须有正的端口。
        /// </summary>
        protected override PlcIoResult? Validate(PlcConnection connection)
        {
            return connection.Port <= 0 ? PlcIoResult.Fail("端口无效") : null;
        }

        /// <summary>
        /// TCP 调用 ConnectServer。UDP 只确认对象已创建。
        /// </summary>
        protected override PlcIoResult Open(IReadWriteNet device)
        {
            if (device is DeviceTcpNet tcp)
                return PlcValueCodec.From(tcp.ConnectServer());

            if (device is DeviceUdpNet)
                return PlcIoResult.Ok("");

            return PlcIoResult.Fail("不支持的网口设备");
        }

        /// <summary>
        /// 断开 TCP。UDP 没有长连接可关。
        /// </summary>
        protected override void Shutdown(IReadWriteNet device)
        {
            if (device is DeviceTcpNet tcp)
                tcp.ConnectClose();
        }
    }

    /// <summary>
    /// 串口设备。Host 填 COM 口。
    /// </summary>
    internal abstract class SerialPlcDriver : HslPlcDriver
    {
        /// <summary>
        /// 记下连接参数。
        /// </summary>
        protected SerialPlcDriver(PlcConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// 打开串口。
        /// </summary>
        protected override PlcIoResult Open(IReadWriteNet device)
        {
            if (device is not DeviceSerialPort serial)
                return PlcIoResult.Fail("不支持的串口设备");

            return PlcValueCodec.From(serial.Open());
        }

        /// <summary>
        /// 关闭串口。
        /// </summary>
        protected override void Shutdown(IReadWriteNet device)
        {
            if (device is DeviceSerialPort serial)
                serial.Close();
        }
    }

    /// <summary>
    /// 用创建函数接上网口模板。
    /// </summary>
    internal sealed class TcpPlcDriver : NetworkPlcDriver
    {
        private readonly Func<PlcConnection, IReadWriteNet> _create;

        /// <summary>
        /// 创建函数只负责 new 出 HSL 对象并填协议参数。
        /// </summary>
        public TcpPlcDriver(PlcConnection connection, Func<PlcConnection, IReadWriteNet> create)
            : base(connection)
        {
            _create = create;
        }

        /// <summary>
        /// 创建网口设备。
        /// </summary>
        protected override IReadWriteNet CreateDevice(PlcConnection connection)
        {
            return _create(connection);
        }
    }

    /// <summary>
    /// 用创建函数接上串口模板。
    /// </summary>
    internal sealed class SerialPortPlcDriver : SerialPlcDriver
    {
        private readonly Func<PlcConnection, IReadWriteNet> _create;

        /// <summary>
        /// 创建函数只负责 new 出 HSL 对象并填协议参数。
        /// </summary>
        public SerialPortPlcDriver(PlcConnection connection, Func<PlcConnection, IReadWriteNet> create)
            : base(connection)
        {
            _create = create;
        }

        /// <summary>
        /// 创建串口设备。
        /// </summary>
        protected override IReadWriteNet CreateDevice(PlcConnection connection)
        {
            return _create(connection);
        }
    }
}
