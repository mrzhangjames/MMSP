using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Device.Plc;
using MMSP.Service.Device.Scan;

namespace MMSP.Service.Flow
{
    /// <summary>
    /// 一次运行自己的 PLC 和扫码枪。多条流程任务共用这一套连接，全部退出后由协调任务释放。
    /// </summary>
    public sealed class FlowDeviceSession : IFlowDeviceSession
    {
        /// <summary>
        /// 保护释放标记和客户端的第一次创建。不包住通讯和等待。
        /// </summary>
        private readonly object _gate = new();

        /// <summary>
        /// 启动时抄下的 PLC 连接。没有这一行时 PLC 步骤失败。
        /// </summary>
        private readonly PlcLinkEntity? _link;

        /// <summary>
        /// 启动时抄下的扫码枪，顺序就是同名枪的优先级。
        /// </summary>
        private readonly IReadOnlyList<ScannerEntity> _scanners;

        /// <summary>
        /// 创建 PLC 客户端。生产环境使用 <see cref="PlcClients.Open"/>。
        /// </summary>
        private readonly Func<PlcConnection, IPlcClient> _openPlc;

        /// <summary>
        /// 创建扫码枪客户端。生产环境使用 <see cref="ScanClients.Open"/>。
        /// </summary>
        private readonly Func<ScanConnection, IScanClient> _openScan;

        /// <summary>
        /// 已经创建的扫码枪。键是枪的主键。
        /// </summary>
        private readonly Dictionary<Guid, IScanClient> _scans = [];

        /// <summary>
        /// 已经创建的 PLC。还没用到时为空。
        /// </summary>
        private IPlcClient? _plc;

        /// <summary>
        /// 连接已经交给协调任务释放。
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 记下启动时抄下的连接。此时还不打开设备。
        /// </summary>
        public FlowDeviceSession(PlcLinkEntity? link, IReadOnlyList<ScannerEntity> scanners)
            : this(link, scanners, PlcClients.Open, ScanClients.Open)
        {
        }

        /// <summary>
        /// 记下连接，并指定客户端的创建方法。测试用它替换真实设备。
        /// </summary>
        internal FlowDeviceSession(
            PlcLinkEntity? link,
            IReadOnlyList<ScannerEntity> scanners,
            Func<PlcConnection, IPlcClient> openPlc,
            Func<ScanConnection, IScanClient> openScan)
        {
            _link = link;
            _scanners = scanners;
            _openPlc = openPlc;
            _openScan = openScan;
        }

        /// <summary>
        /// 按名称触发一把枪。名称重复时用列表里靠前的那把。
        /// </summary>
        public ScanIoResult Trigger(string scannerName, string command, CancellationToken token)
        {
            var client = PrepareScan(scannerName, command, token, out var failure);
            if (client is null)
                return failure;

            if (!client.IsConnected)
            {
                var connected = client.Connect();
                if (!connected.IsSuccess)
                    return connected;
            }

            if (token.IsCancellationRequested)
                return ScanIoResult.Fail("已取消");

            return client.Trigger(command);
        }

        /// <summary>
        /// 读取 PLC。没有连接配置时直接失败。
        /// </summary>
        public PlcIoResult Read(string address, PlcDataType dataType, int length, CancellationToken token)
        {
            var reader = PreparePlc(token, out var ready);
            if (reader is null)
                return ready;

            if (token.IsCancellationRequested)
                return PlcIoResult.Fail("已取消");

            return reader.Read(address, dataType, length);
        }

        /// <summary>
        /// 写入 PLC。没有连接配置时直接失败。
        /// </summary>
        public PlcIoResult Write(string address, PlcDataType dataType, string value, CancellationToken token)
        {
            var writer = PreparePlc(token, out var ready);
            if (writer is null)
                return ready;

            if (token.IsCancellationRequested)
                return PlcIoResult.Fail("已取消");

            return writer.Write(address, dataType, value ?? "");
        }

        /// <summary>
        /// 等待。取消标记触发时返回 false。不占用客户端创建锁。
        /// </summary>
        public bool Wait(int milliseconds, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return false;

            if (milliseconds <= 0)
                return true;

            return !token.WaitHandle.WaitOne(milliseconds);
        }

        /// <summary>
        /// 关闭本轮打开的 PLC 和扫码枪。
        /// </summary>
        public void Dispose()
        {
            IScanClient[] scans;
            IPlcClient? plc;
            lock (_gate)
            {
                if (_disposed)
                    return;

                _disposed = true;
                scans = _scans.Values.ToArray();
                _scans.Clear();
                plc = _plc;
                _plc = null;
            }

            Exception? error = null;
            foreach (var client in scans)
            {
                try
                {
                    client.Dispose();
                }
                catch (Exception ex)
                {
                    error ??= ex;
                }
            }

            try
            {
                plc?.Dispose();
            }
            catch (Exception ex)
            {
                error ??= ex;
            }

            if (error is not null)
                throw error;
        }

        /// <summary>
        /// 按去掉首尾空白后的名称找枪。列表顺序就是优先级。
        /// </summary>
        internal static ScannerEntity? FindScanner(IReadOnlyList<ScannerEntity> scanners, string name)
        {
            var key = (name ?? "").Trim();
            if (key.Length == 0)
                return null;

            foreach (var item in scanners)
            {
                if (string.Equals((item.Name ?? "").Trim(), key, StringComparison.Ordinal))
                    return item;
            }

            return null;
        }

        /// <summary>
        /// 取出或创建扫码枪客户端。连接和触发留在锁外。失败时返回空。
        /// </summary>
        private IScanClient? PrepareScan(
            string scannerName,
            string command,
            CancellationToken token,
            out ScanIoResult failure)
        {
            lock (_gate)
            {
                if (_disposed)
                {
                    failure = ScanIoResult.Fail("连接已释放");
                    return null;
                }

                if (token.IsCancellationRequested)
                {
                    failure = ScanIoResult.Fail("已取消");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(command))
                {
                    failure = ScanIoResult.Fail("触发内容为空");
                    return null;
                }

                var entity = FindScanner(_scanners, scannerName);
                if (entity is null)
                {
                    failure = ScanIoResult.Fail("找不到扫码枪");
                    return null;
                }

                if (!_scans.TryGetValue(entity.Id, out var found) || found is null)
                {
                    try
                    {
                        found = _openScan(ToScan(entity));
                    }
                    catch (Exception ex)
                    {
                        failure = ScanIoResult.Fail(ex.Message);
                        return null;
                    }

                    if (found is null)
                    {
                        failure = ScanIoResult.Fail("找不到扫码枪");
                        return null;
                    }

                    _scans[entity.Id] = found;
                }

                failure = ScanIoResult.Ok("");
                return found;
            }
        }

        /// <summary>
        /// 取出或创建 PLC 客户端，并在锁外完成连接和重连等待。失败时返回空。
        /// </summary>
        private IPlcClient? PreparePlc(CancellationToken token, out PlcIoResult failure)
        {
            IPlcClient client;
            PlcLinkEntity link;
            lock (_gate)
            {
                if (_disposed)
                {
                    failure = PlcIoResult.Fail("连接已释放");
                    return null;
                }

                if (_link is null)
                {
                    failure = PlcIoResult.Fail("没有 PLC 连接");
                    return null;
                }

                if (_plc is null)
                {
                    try
                    {
                        _plc = _openPlc(ToPlc(_link));
                    }
                    catch (Exception ex)
                    {
                        failure = PlcIoResult.Fail(ex.Message);
                        return null;
                    }
                }

                if (_plc is null)
                {
                    failure = PlcIoResult.Fail("没有 PLC 连接");
                    return null;
                }

                client = _plc;
                link = _link;
            }

            if (client.IsConnected)
            {
                failure = PlcIoResult.Ok("");
                return client;
            }

            var connected = client.Connect();
            if (connected.IsSuccess)
            {
                failure = connected;
                return client;
            }

            var extra = link.ReconnectCount;
            var waitMs = link.ReconnectIntervalMs;
            for (var attempt = 0; attempt < extra; attempt++)
            {
                if (!Wait(waitMs, token))
                {
                    failure = PlcIoResult.Fail("已取消");
                    return null;
                }

                if (client.IsConnected)
                {
                    failure = PlcIoResult.Ok("");
                    return client;
                }

                connected = client.Connect();
                if (connected.IsSuccess)
                {
                    failure = connected;
                    return client;
                }
            }

            failure = connected;
            return null;
        }

        /// <summary>
        /// 客户库里的一行转成扫码连接。不改库。
        /// </summary>
        private static ScanConnection ToScan(ScannerEntity entity)
        {
            var transport = entity.Transport == (int)ScanTransport.Serial
                ? ScanTransport.Serial
                : ScanTransport.Tcp;
            return new ScanConnection
            {
                Transport = transport,
                Host = entity.Host ?? "",
                Port = entity.Port,
                BaudRate = entity.BaudRate,
                Parity = string.IsNullOrWhiteSpace(entity.Parity) ? "None" : entity.Parity,
                DataBits = entity.DataBits,
                StopBits = string.IsNullOrWhiteSpace(entity.StopBits) ? "One" : entity.StopBits,
                ConnectTimeout = entity.ConnectTimeout,
                ReceiveTimeout = entity.ReceiveTimeout,
                NewLine = string.IsNullOrEmpty(entity.NewLine) ? "\r" : entity.NewLine,
                EncodingName = string.IsNullOrWhiteSpace(entity.EncodingName) ? "ASCII" : entity.EncodingName
            };
        }

        /// <summary>
        /// 客户库里的一行转成 PLC 连接。协议用原值，不改库。
        /// </summary>
        private static PlcConnection ToPlc(PlcLinkEntity link)
        {
            var format = link.DataFormat;
            if (format is not ("ABCD" or "CDAB" or "BADC" or "DCBA"))
                format = "ABCD";

            return new PlcConnection
            {
                Protocol = (PlcProtocol)link.Protocol,
                Host = link.Host ?? "",
                Port = link.Port,
                Station = ClampStation(link.Station),
                DataFormat = format,
                ConnectTimeout = link.ReadTimeoutMs,
                ReceiveTimeout = link.ReadTimeoutMs
            };
        }

        /// <summary>
        /// 站号收成一个字节。
        /// </summary>
        private static byte ClampStation(int station)
        {
            if (station < 0)
                return 0;

            if (station > 255)
                return 255;

            return (byte)station;
        }
    }
}
