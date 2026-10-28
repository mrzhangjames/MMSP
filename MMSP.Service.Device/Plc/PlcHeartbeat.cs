using System.Globalization;
using System.Net.Sockets;
using MMSP.Core.Model.Helper;
using MMSP.Core.Model.Sys;

namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 按已保存的连接，在单独的长连接上每秒把 short 写入心跳地址。
    /// 地址为空时不连接。不替代设备页和流程自己的 PLC 连接。
    /// </summary>
    public sealed class PlcHeartbeat
    {
        /// <summary>
        /// 保护连接、地址和停止标记。
        /// </summary>
        private readonly object _gate = new();

        /// <summary>
        /// 配置变化或退出时叫醒等待。
        /// </summary>
        private readonly ManualResetEventSlim _wake = new(false);

        /// <summary>
        /// 当前心跳连接。地址为空时没有。
        /// </summary>
        private PlcConnection? _connection;

        /// <summary>
        /// 心跳地址。空则不连接。
        /// </summary>
        private string _address = "";

        /// <summary>
        /// 断线后的重连间隔，毫秒。
        /// </summary>
        private int _reconnectMs;

        /// <summary>
        /// 配置代数。换配置之后，旧循环不再写入。
        /// </summary>
        private int _version;

        /// <summary>
        /// 正在退出。
        /// </summary>
        private bool _stopping;

        /// <summary>
        /// 心跳线程。没启动时为空。
        /// </summary>
        private Thread? _thread;

        /// <summary>
        /// 进程内这一条心跳。
        /// </summary>
        public static PlcHeartbeat Shared { get; } = new();

        /// <summary>
        /// 失败时调用一次。同一条原因不重复调用。由壳接到 PLC 错误日志。第二个参数是捕获到的异常，没有时为空。
        /// </summary>
        public Action<string, Exception?>? Failure { get; set; }

        /// <summary>
        /// 最近一次心跳写入是否成功。
        /// </summary>
        private bool _linkUp;

        /// <summary>
        /// 配了心跳地址和连接。底栏直接看 <see cref="LinkUp"/>，不再另开一条 PLC 连接。
        /// </summary>
        public bool HasWatch
        {
            get
            {
                lock (_gate)
                    return !_stopping && _address.Length > 0 && _connection is not null;
            }
        }

        /// <summary>
        /// 最近一次心跳写入成功。地址空着或正在重连时为 false。
        /// </summary>
        public bool LinkUp
        {
            get
            {
                lock (_gate)
                    return _linkUp;
            }
        }

        /// <summary>
        /// 换成新的连接和地址。地址空白时断开并停在等待。相同参数不重连。
        /// </summary>
        public void Apply(PlcConnection? connection, string? address, int reconnectIntervalMs)
        {
            var text = (address ?? "").Trim();
            lock (_gate)
            {
                if (_stopping)
                    return;

                if (Same(_connection, connection) && _address == text && _reconnectMs == reconnectIntervalMs && _thread is { IsAlive: true })
                    return;

                _connection = connection;
                _address = text;
                _reconnectMs = reconnectIntervalMs;
                _linkUp = false;
                _version++;
                if (_thread is not { IsAlive: true })
                {
                    _thread = new Thread(Loop)
                    {
                        IsBackground = true,
                        Name = "PlcHeartbeat"
                    };
                    _thread.Start();
                }

                _wake.Set();
            }
        }

        /// <summary>
        /// 退出时停掉循环并关掉连接。写操作仍卡在驱动里时最多再等三秒。
        /// </summary>
        public void Stop()
        {
            Thread? thread;
            lock (_gate)
            {
                _stopping = true;
                _address = "";
                _linkUp = false;
                _version++;
                thread = _thread;
                _wake.Set();
            }

            if (thread is not null && thread.IsAlive && !ReferenceEquals(thread, Thread.CurrentThread))
                thread.Join(TimeSpan.FromSeconds(3));
        }

        /// <summary>
        /// 后台循环。连接参数变化时丢掉旧客户端再连。
        /// </summary>
        private void Loop()
        {
            IPlcClient? client = null;
            var seen = -1;
            short value = 0;
            string? lastError = null;
            try
            {
                while (true)
                {
                    PlcConnection? connection;
                    string address;
                    int reconnectMs;
                    int version;
                    lock (_gate)
                    {
                        if (_stopping)
                            break;

                        connection = _connection;
                        address = _address;
                        reconnectMs = _reconnectMs;
                        version = _version;
                        _wake.Reset();
                    }

                    if (address.Length == 0)
                    {
                        Drop(ref client);
                        SetLink(false);
                        lastError = null;
                        _wake.Wait();
                        continue;
                    }

                    if (connection is null || !Enum.IsDefined(connection.Protocol))
                    {
                        Drop(ref client);
                        SetLink(false);
                        Report(ref lastError, "PLC 协议无效");
                        _wake.Wait();
                        continue;
                    }

                    if (version != seen)
                    {
                        Drop(ref client);
                        SetLink(false);
                        seen = version;
                        value = 0;
                        lastError = null;
                    }

                    if (client is null && !TryConnect(connection, ref client, ref lastError))
                    {
                        SetLink(false);
                        WaitReconnect(reconnectMs);
                        continue;
                    }

                    value = Next(value);
                    if (!TryWrite(client!, address, value, ref lastError))
                    {
                        Drop(ref client);
                        SetLink(false);
                        WaitReconnect(reconnectMs);
                        continue;
                    }

                    SetLink(true);
                    lastError = null;
                    _wake.Wait(1000);
                }
            }
            finally
            {
                Drop(ref client);
            }
        }

        /// <summary>
        /// 1 到 10000，下一次回到 0。
        /// </summary>
        private static short Next(short current)
        {
            var next = (short)(current + 1);
            return next > 10000 ? (short)0 : next;
        }

        /// <summary>
        /// 打开并连接。失败时关掉客户端，并只上报一次。
        /// </summary>
        private bool TryConnect(PlcConnection connection, ref IPlcClient? client, ref string? lastError)
        {
            IPlcClient? opened = null;
            try
            {
                opened = PlcClients.Open(connection);
                var connected = opened.Connect();
                if (!connected.IsSuccess)
                {
                    DisposeClient(opened);
                    opened = null;
                    Report(ref lastError, connected.Message);
                    return false;
                }

                client = opened;
                opened = null;
                return true;
            }
            catch (TimeoutException ex)
            {
                DisposeClient(opened);
                Report(ref lastError, "连接 PLC 失败", ex);
                return false;
            }
            catch (SocketException ex)
            {
                DisposeClient(opened);
                Report(ref lastError, "连接 PLC 失败", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                DisposeClient(opened);
                Report(ref lastError, "连接 PLC 失败", ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                DisposeClient(opened);
                Report(ref lastError, "连接 PLC 失败", ex);
                return false;
            }
            catch (IOException ex)
            {
                DisposeClient(opened);
                Report(ref lastError, "连接 PLC 失败", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                DisposeClient(opened);
                Report(ref lastError, "连接 PLC 失败", ex);
                return false;
            }
            catch (ArgumentException ex)
            {
                DisposeClient(opened);
                Report(ref lastError, "连接 PLC 失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 写入当前心跳值。失败时只上报一次。
        /// </summary>
        private bool TryWrite(IPlcClient client, string address, short value, ref string? lastError)
        {
            try
            {
                var written = client.Write(address, PlcDataType.Int16, value.ToString(CultureInfo.InvariantCulture));
                if (written.IsSuccess)
                    return true;

                Report(ref lastError, written.Message);
                return false;
            }
            catch (TimeoutException ex)
            {
                Report(ref lastError, "写入心跳失败", ex);
                return false;
            }
            catch (SocketException ex)
            {
                Report(ref lastError, "写入心跳失败", ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                Report(ref lastError, "写入心跳失败", ex);
                return false;
            }
            catch (IOException ex)
            {
                Report(ref lastError, "写入心跳失败", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Report(ref lastError, "写入心跳失败", ex);
                return false;
            }
            catch (ArgumentException ex)
            {
                Report(ref lastError, "写入心跳失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 按已存重连间隔等待。间隔不是正数时等一秒，避免空转。
        /// </summary>
        private void WaitReconnect(int reconnectMs)
        {
            _wake.Wait(reconnectMs > 0 ? reconnectMs : 1000);
        }

        /// <summary>
        /// 同一条原因只交给 Failure 一次。
        /// </summary>
        private void Report(ref string? lastError, string? message, Exception? exception = null)
        {
            var text = exception is null
                ? (string.IsNullOrWhiteSpace(message) ? "心跳失败" : "心跳 " + message.Trim())
                : (string.IsNullOrWhiteSpace(message) ? "心跳失败" : message.Trim());
            if (text == lastError)
                return;

            lastError = text;
            try
            {
                Failure?.Invoke(text, exception);
            }
            catch (InvalidOperationException)
            {
                // 日志订阅失败不打断心跳循环。
            }
        }

        /// <summary>
        /// 记下心跳是否还连着。底栏只读这个结果。
        /// </summary>
        private void SetLink(bool up)
        {
            lock (_gate)
                _linkUp = up;
        }

        /// <summary>
        /// 关掉当前客户端。
        /// </summary>
        private static void Drop(ref IPlcClient? client)
        {
            var current = client;
            client = null;
            DisposeClient(current);
        }

        /// <summary>
        /// 关闭并释放。重复关闭不再抛出。
        /// </summary>
        private static void DisposeClient(IPlcClient? client)
        {
            if (client is null)
                return;

            try
            {
                client.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException ex)
            {
                CaughtLog.Report(CaughtLogChannel.Plc, "关闭 PLC 连接失败", ex);
            }
            catch (IOException ex)
            {
                CaughtLog.Report(CaughtLogChannel.Plc, "关闭 PLC 连接失败", ex);
            }
        }

        /// <summary>
        /// 连接参数没变时不重开。
        /// </summary>
        private static bool Same(PlcConnection? left, PlcConnection? right)
        {
            if (left is null || right is null)
                return left is null && right is null;

            return left.Protocol == right.Protocol
                && left.Host == right.Host
                && left.Port == right.Port
                && left.Station == right.Station
                && left.DataFormat == right.DataFormat
                && left.ConnectTimeout == right.ConnectTimeout
                && left.ReceiveTimeout == right.ReceiveTimeout
                && left.WriteTimeout == right.WriteTimeout;
        }
    }
}
