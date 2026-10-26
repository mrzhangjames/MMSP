using MMSP.Core.Model.DB;
using MMSP.Service.Database;
using MMSP.Service.Device;
using MMSP.Service.Device.Plc;
using MMSP.Service.Device.Scan;

namespace MMSP.App.Main.Services
{
    /// <summary>
    /// 一轮读到的一台设备。名称和是否连上，界面再拼连接状态文字。
    /// </summary>
    public sealed class DeviceLinkReading
    {
        /// <summary>
        /// 创建一条读数。
        /// </summary>
        public DeviceLinkReading(string name, bool isConnected)
        {
            Name = name;
            IsConnected = isConnected;
        }

        /// <summary>
        /// 设备名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 这一轮是否连上。
        /// </summary>
        public bool IsConnected { get; }
    }

    /// <summary>
    /// 按客户库里的 PLC 和启用扫码枪看连接。一轮结束后再等 3 秒，不写运行日志。
    /// 设备页或流程已经连着时直接显示已连接。PLC 配了心跳地址就看心跳写入，否则短连接。
    /// 短连接失败时连续再连，一共 3 次，间隔 200 毫秒，仍然失败才显示未连接。
    /// </summary>
    public sealed class DeviceLinkWatch
    {
        private readonly object _gate = new();
        private readonly ManualResetEventSlim _wake = new(false);
        private Thread? _thread;
        private bool _stopping;
        private IReadOnlyList<DeviceLinkReading> _last = [];

        /// <summary>
        /// 进程内这一条探测。
        /// </summary>
        public static DeviceLinkWatch Shared { get; } = new();

        /// <summary>
        /// 读数变化。在探测线程上触发，界面自己切回界面线程。
        /// </summary>
        public event Action<IReadOnlyList<DeviceLinkReading>>? Updated;

        /// <summary>
        /// 开始探测。已经在跑时不再开第二条。
        /// </summary>
        public void Start()
        {
            lock (_gate)
            {
                if (_stopping || _thread is { IsAlive: true })
                    return;

                _thread = new Thread(Loop)
                {
                    IsBackground = true,
                    Name = "DeviceLinkWatch"
                };
                _thread.Start();
            }
        }

        /// <summary>
        /// 退出时停掉探测。正在连接时最多再等三秒。
        /// </summary>
        public void Stop()
        {
            Thread? thread;
            lock (_gate)
            {
                _stopping = true;
                thread = _thread;
                _wake.Set();
            }

            if (thread is not null && thread.IsAlive && !ReferenceEquals(thread, Thread.CurrentThread))
                thread.Join(TimeSpan.FromSeconds(3));
        }

        /// <summary>
        /// 后台循环。一轮结束再等 3 秒。停止时立刻醒来。
        /// </summary>
        private void Loop()
        {
            while (true)
            {
                lock (_gate)
                {
                    if (_stopping)
                        return;

                    _wake.Reset();
                }

                try
                {
                    Publish(ReadLinks());
                }
                catch (Exception)
                {
                    // 读库失败不写运行日志，下一轮再试。
                }

                _wake.Wait(3000);
            }
        }

        /// <summary>
        /// 抄下 PLC 和启用的扫码枪。库没打开时给空列表。
        /// </summary>
        private List<DeviceLinkReading> ReadLinks()
        {
            var result = new List<DeviceLinkReading>();
            if (!SqlSugarContext.IsOpen)
                return result;

            var db = SqlSugarContext.CustomerDb;
            var link = db.Queryable<PlcLinkEntity>().ToList().FirstOrDefault();
            if (link is not null)
                result.Add(new DeviceLinkReading("PLC", ReadPlc(link)));

            var guns = db.Queryable<ScannerEntity>()
                .Where(item => item.IsValid)
                .OrderBy(item => item.SortIndex)
                .ToList();
            foreach (var gun in guns)
                result.Add(new DeviceLinkReading(ScannerName(gun), ReadScan(gun)));

            return result;
        }

        /// <summary>
        /// PLC：占用中的连接算已连接。配了心跳地址看最近一次写入，否则短连接。
        /// </summary>
        private bool ReadPlc(PlcLinkEntity link)
        {
            if (DeviceLinkUse.PlcConnected())
                return true;

            if (!string.IsNullOrWhiteSpace(link.HeartbeatAddress))
                return PlcHeartbeat.Shared.LinkUp;

            return ConnectRepeated(() => ProbePlc(link));
        }

        /// <summary>
        /// 扫码枪：占用中的连接算已连接，否则短连接。不发送触发。
        /// </summary>
        private bool ReadScan(ScannerEntity entity)
        {
            if (DeviceLinkUse.ScanConnected(entity.Id))
                return true;

            return ConnectRepeated(() => ProbeScan(entity));
        }

        /// <summary>
        /// 连续连接 3 次，间隔 200 毫秒。有一次成功就停。三次都失败才算未连接。
        /// </summary>
        private bool ConnectRepeated(Func<bool> once)
        {
            for (var attempt = 0; attempt < 3; attempt++)
            {
                lock (_gate)
                {
                    if (_stopping)
                        return false;
                }

                if (once())
                    return true;

                if (attempt == 2)
                    break;

                if (_wake.Wait(200))
                {
                    lock (_gate)
                    {
                        if (_stopping)
                            return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 短连接看 PLC 能否连上，连完就关掉。不写心跳。
        /// </summary>
        private static bool ProbePlc(PlcLinkEntity link)
        {
            if (!Enum.IsDefined(typeof(PlcProtocol), link.Protocol))
                return false;

            IPlcClient? client = null;
            try
            {
                client = PlcClients.Open(PlcConnections.Create(
                    (PlcProtocol)link.Protocol,
                    link.Host,
                    link.Port,
                    link.Station,
                    link.DataFormat,
                    link.ReadTimeoutMs,
                    link.WriteTimeoutMs));
                return client.Connect().IsSuccess;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                Dispose(client);
            }
        }

        /// <summary>
        /// 短连接看扫码枪能否连上，不发送触发。连完就关掉。
        /// </summary>
        private static bool ProbeScan(ScannerEntity entity)
        {
            IScanClient? client = null;
            try
            {
                client = ScanClients.Open(ToScan(entity));
                return client.Connect().IsSuccess;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                Dispose(client);
            }
        }

        /// <summary>
        /// 名称空白时用地址。地址也空时用扫码枪。
        /// </summary>
        private static string ScannerName(ScannerEntity entity)
        {
            var name = (entity.Name ?? "").Trim();
            if (name.Length > 0)
                return name;

            var host = (entity.Host ?? "").Trim();
            return host.Length > 0 ? host : "扫码枪";
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
        /// 读数和上一轮不同时才通知。
        /// </summary>
        private void Publish(IReadOnlyList<DeviceLinkReading> next)
        {
            if (Same(_last, next))
                return;

            _last = next;
            var handler = Updated;
            if (handler is null)
                return;

            try
            {
                handler(next);
            }
            catch (Exception)
            {
                // 界面更新失败不打断探测。
            }
        }

        /// <summary>
        /// 名称和连接结果都相同才算没变。
        /// </summary>
        private static bool Same(IReadOnlyList<DeviceLinkReading> left, IReadOnlyList<DeviceLinkReading> right)
        {
            if (left.Count != right.Count)
                return false;

            for (var index = 0; index < left.Count; index++)
            {
                if (left[index].Name != right[index].Name || left[index].IsConnected != right[index].IsConnected)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 关掉探测用的短连接。
        /// </summary>
        private static void Dispose(IDisposable? client)
        {
            if (client is null)
                return;

            try
            {
                client.Dispose();
            }
            catch (Exception)
            {
                // 探测结束时，通道已经关掉就忽略。
            }
        }
    }
}
