using System.ComponentModel;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;

namespace MMSP.Service.Device.Scan
{
    /// <summary>
    /// 扫码通道。子类只负责打开、丢弃、发送、读取和关闭，触发流程在这里完成。
    /// </summary>
    internal abstract class ScanChannel : IDisposable
    {
        private const int MaxReplyBytes = 4096;

        /// <summary>
        /// 记下连接参数。
        /// </summary>
        protected ScanChannel(ScanConnection connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// 连接参数。
        /// </summary>
        protected ScanConnection Connection { get; }

        /// <summary>
        /// 传输方式。
        /// </summary>
        public ScanTransport Transport => Connection.Transport;

        /// <summary>
        /// 是否已连接。
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// 打开通道。地址为空时不创建对象。
        /// </summary>
        public ScanIoResult Connect()
        {
            Close();
            if (string.IsNullOrWhiteSpace(Connection.Host))
                return ScanIoResult.Fail("地址为空");

            var invalid = Validate(Connection);
            if (invalid is not null)
                return invalid;

            var opened = false;
            try
            {
                var result = OpenCore();
                IsConnected = result.IsSuccess;
                opened = result.IsSuccess;
                return result;
            }
            finally
            {
                if (!opened)
                    Close();
            }
        }

        /// <summary>
        /// 丢掉旧数据，发送命令，读到结束符。
        /// </summary>
        public ScanIoResult Trigger(string command)
        {
            if (!IsConnected)
                return ScanIoResult.Fail("尚未连接");

            if (string.IsNullOrWhiteSpace(command))
                return ScanIoResult.Fail("触发内容为空");

            try
            {
                DiscardCore();
                var encoding = TextEncoding(Connection.EncodingName);
                var terminator = Terminator(Connection.NewLine);
                SendCore(encoding.GetBytes(command + terminator));
                return ReadLine(encoding, encoding.GetBytes(terminator));
            }
            catch (TimeoutException)
            {
                return ScanIoResult.Fail("接收超时");
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
            {
                return ScanIoResult.Fail("接收超时");
            }
            catch (IOException ex) when (IsSocketTimeout(ex))
            {
                return ScanIoResult.Fail("接收超时");
            }
        }

        /// <summary>
        /// 关闭当前通道。
        /// </summary>
        public void Close()
        {
            IsConnected = false;
            try
            {
                CloseCore();
            }
            catch (ObjectDisposedException)
            {
                // 已经关掉。下次 Connect 会新建对象。
            }
            catch (SocketException)
            {
                // 关闭失败不再向外抛，避免盖住这次连接或触发的异常。
            }
            catch (Win32Exception)
            {
                // 串口关闭失败不再向外抛。
            }
            catch (UnauthorizedAccessException)
            {
                // 关闭失败不再向外抛。
            }
            catch (IOException)
            {
                // 关闭失败不再向外抛。
            }
            catch (InvalidOperationException)
            {
                // 关闭失败不再向外抛。
            }
        }

        /// <summary>
        /// 关闭通道。
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// 参数不完整时返回失败。通过时返回 null。
        /// </summary>
        protected virtual ScanIoResult? Validate(ScanConnection connection)
        {
            return null;
        }

        /// <summary>
        /// 打开已经校验过的通道。
        /// </summary>
        protected abstract ScanIoResult OpenCore();

        /// <summary>
        /// 丢掉缓冲区里已经到达的字节。
        /// </summary>
        protected abstract void DiscardCore();

        /// <summary>
        /// 写出一整段命令。
        /// </summary>
        protected abstract void SendCore(byte[] data);

        /// <summary>
        /// 读一个字节。流结束时返回 -1。超时抛出。
        /// </summary>
        protected abstract int ReadCore(int timeoutMs);

        /// <summary>
        /// 关闭已经打开的通道。
        /// </summary>
        protected abstract void CloseCore();

        /// <summary>
        /// 超时毫秒。小于等于 0 时按 1000。
        /// </summary>
        protected static int TimeoutOf(int milliseconds)
        {
            return milliseconds > 0 ? milliseconds : 1000;
        }

        /// <summary>
        /// 读到结束符或超时。结束符本身不放进结果。
        /// </summary>
        private ScanIoResult ReadLine(Encoding encoding, byte[] terminator)
        {
            var data = new List<byte>(64);
            var deadline = Environment.TickCount64 + TimeoutOf(Connection.ReceiveTimeout);
            while (true)
            {
                var left = deadline - Environment.TickCount64;
                if (left <= 0)
                    return ScanIoResult.Fail("接收超时");

                int next;
                try
                {
                    next = ReadCore((int)Math.Min(left, int.MaxValue));
                }
                catch (TimeoutException)
                {
                    return ScanIoResult.Fail("接收超时");
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    return ScanIoResult.Fail("接收超时");
                }
                catch (IOException ex) when (IsSocketTimeout(ex))
                {
                    return ScanIoResult.Fail("接收超时");
                }

                if (next < 0)
                    return ScanIoResult.Fail("连接已断开");

                data.Add((byte)next);
                if (EndsWith(data, terminator))
                {
                    var count = data.Count - terminator.Length;
                    return ScanIoResult.Ok(encoding.GetString(data.ToArray(), 0, count));
                }

                if (data.Count >= MaxReplyBytes)
                    return ScanIoResult.Fail("应答过长");
            }
        }

        /// <summary>
        /// ASCII 与 UTF-8。其余按 ASCII。
        /// </summary>
        private static Encoding TextEncoding(string name)
        {
            var text = name.Trim();
            if (text.Equals("UTF-8", StringComparison.OrdinalIgnoreCase) || text.Equals("UTF8", StringComparison.OrdinalIgnoreCase))
                return Encoding.UTF8;

            return Encoding.ASCII;
        }

        /// <summary>
        /// 空白结束符按回车。
        /// </summary>
        private static string Terminator(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? "\r" : text;
        }

        /// <summary>
        /// 缓冲区是否以结束符结尾。
        /// </summary>
        private static bool EndsWith(List<byte> data, byte[] terminator)
        {
            if (terminator.Length == 0 || data.Count < terminator.Length)
                return false;

            var start = data.Count - terminator.Length;
            for (var i = 0; i < terminator.Length; i++)
            {
                if (data[start + i] != terminator[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 套接字读超时包在 IO 异常里。串口超时是 TimeoutException，由调用处单独捕获。
        /// </summary>
        private static bool IsSocketTimeout(IOException exception)
        {
            if (exception.InnerException is TimeoutException)
                return true;

            return exception.InnerException is SocketException socket && socket.SocketErrorCode == SocketError.TimedOut;
        }
    }

    /// <summary>
    /// TCP 扫码枪。主动连接枪的命令口。
    /// </summary>
    internal sealed class TcpScanChannel : ScanChannel
    {
        private TcpClient? _client;
        private NetworkStream? _stream;

        /// <summary>
        /// 记下连接参数。
        /// </summary>
        public TcpScanChannel(ScanConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// TCP 必须有正的端口。
        /// </summary>
        protected override ScanIoResult? Validate(ScanConnection connection)
        {
            return connection.Port <= 0 ? ScanIoResult.Fail("端口无效") : null;
        }

        /// <summary>
        /// 在连接超时内完成 TCP 握手。
        /// </summary>
        protected override ScanIoResult OpenCore()
        {
            var client = new TcpClient();
            try
            {
                using var cancel = new CancellationTokenSource(TimeoutOf(Connection.ConnectTimeout));
                client.ConnectAsync(Connection.Host.Trim(), Connection.Port, cancel.Token).GetAwaiter().GetResult();
                _client = client;
                _stream = client.GetStream();
                return ScanIoResult.Ok("");
            }
            catch (OperationCanceledException)
            {
                return ScanIoResult.Fail("连接超时");
            }
            finally
            {
                if (_client is null)
                    client.Dispose();
            }
        }

        /// <summary>
        /// 丢掉已经到达、还没读的字节。
        /// </summary>
        protected override void DiscardCore()
        {
            if (_stream is null)
                return;

            var buffer = new byte[256];
            while (_stream.DataAvailable)
            {
                var read = _stream.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    break;
            }
        }

        /// <summary>
        /// 写出命令。
        /// </summary>
        protected override void SendCore(byte[] data)
        {
            var stream = _stream ?? throw new InvalidOperationException("尚未连接");
            stream.WriteTimeout = TimeoutOf(Connection.ReceiveTimeout);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        /// <summary>
        /// 读一个字节。
        /// </summary>
        protected override int ReadCore(int timeoutMs)
        {
            var stream = _stream ?? throw new InvalidOperationException("尚未连接");
            stream.ReadTimeout = Math.Max(timeoutMs, 1);
            return stream.ReadByte();
        }

        /// <summary>
        /// 关闭套接字。
        /// </summary>
        protected override void CloseCore()
        {
            _stream?.Dispose();
            _client?.Dispose();
            _stream = null;
            _client = null;
        }
    }

    /// <summary>
    /// 串口扫码枪。Host 填 COM 口。
    /// </summary>
    internal sealed class SerialScanChannel : ScanChannel
    {
        private SerialPort? _port;

        /// <summary>
        /// 记下连接参数。
        /// </summary>
        public SerialScanChannel(ScanConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// 打开串口。口不存在时由异常收成失败。
        /// </summary>
        protected override ScanIoResult OpenCore()
        {
            var port = new SerialPort(Connection.Host.Trim(), BaudRate(), ParityOf(), DataBits(), StopBitsOf())
            {
                ReadTimeout = TimeoutOf(Connection.ReceiveTimeout),
                WriteTimeout = TimeoutOf(Connection.ReceiveTimeout),
                Encoding = Encoding.ASCII,
                NewLine = "\r"
            };
            port.Open();
            _port = port;
            return ScanIoResult.Ok("");
        }

        /// <summary>
        /// 丢掉串口缓冲区里的旧字节。
        /// </summary>
        protected override void DiscardCore()
        {
            if (_port is null)
                return;

            var buffer = new byte[256];
            while (_port.BytesToRead > 0)
                _port.Read(buffer, 0, Math.Min(buffer.Length, _port.BytesToRead));
        }

        /// <summary>
        /// 写出命令。
        /// </summary>
        protected override void SendCore(byte[] data)
        {
            var port = _port ?? throw new InvalidOperationException("尚未连接");
            port.WriteTimeout = TimeoutOf(Connection.ReceiveTimeout);
            port.Write(data, 0, data.Length);
        }

        /// <summary>
        /// 读一个字节。
        /// </summary>
        protected override int ReadCore(int timeoutMs)
        {
            var port = _port ?? throw new InvalidOperationException("尚未连接");
            port.ReadTimeout = Math.Max(timeoutMs, 1);
            return port.ReadByte();
        }

        /// <summary>
        /// 关闭串口。
        /// </summary>
        protected override void CloseCore()
        {
            if (_port is null)
                return;

            _port.Dispose();
            _port = null;
        }

        /// <summary>
        /// 小于等于 0 时用 9600。
        /// </summary>
        private int BaudRate()
        {
            return Connection.BaudRate > 0 ? Connection.BaudRate : 9600;
        }

        /// <summary>
        /// 不在 5 到 8 时用 8。
        /// </summary>
        private int DataBits()
        {
            return Connection.DataBits is >= 5 and <= 8 ? Connection.DataBits : 8;
        }

        /// <summary>
        /// 认不出时用 None。
        /// </summary>
        private Parity ParityOf()
        {
            return Enum.TryParse<Parity>(Connection.Parity, true, out var parity) ? parity : Parity.None;
        }

        /// <summary>
        /// 认不出时用 One。
        /// </summary>
        private StopBits StopBitsOf()
        {
            return Enum.TryParse<StopBits>(Connection.StopBits, true, out var stopBits) ? stopBits : StopBits.One;
        }
    }
}
