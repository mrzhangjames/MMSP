using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Service.Database;
using MMSP.Service.Device;
using MMSP.Service.Device.Plc;
using MMSP.Service.Device.Scan;
using SqlSugar;

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
    /// 按客户库里的 PLC 和启用扫码枪看连接。一轮结束后再等 3 秒。
    /// 读库或短连接失败时各记一次，恢复成功后再允许下一条。
    /// 设备页或流程已经连着时直接显示已连接。PLC 配了心跳地址就看心跳写入，否则短连接。
    /// 短连接失败时连续再连，一共 3 次，间隔 200 毫秒，仍然失败才显示未连接。
    /// </summary>
    public sealed class DeviceLinkWatch
    {
        /// <summary>
        /// 保护线程和停止标记。
        /// </summary>
        private readonly object _gate = new();

        /// <summary>
        /// 提前结束这一轮的等待。
        /// </summary>
        private readonly ManualResetEventSlim _wake = new(false);

        /// <summary>
        /// 探测线程。没启动时为空。
        /// </summary>
        private Thread? _thread;

        /// <summary>
        /// 正在退出。
        /// </summary>
        private bool _stopping;

        /// <summary>
        /// 上一轮读数。没变化时不通知界面。
        /// </summary>
        private IReadOnlyList<DeviceLinkReading> _last = [];

        /// <summary>
        /// 这一轮读库失败已经写过日志。恢复成功后清掉，避免每 3 秒重复记同一条。
        /// </summary>
        private bool _readErrorLogged;

        /// <summary>
        /// 界面更新失败已经写过日志。下一次更新成功后清掉。
        /// </summary>
        private bool _publishErrorLogged;

        /// <summary>
        /// 短连接失败已经写过日志的设备名。连上后从这里去掉。
        /// </summary>
        private readonly HashSet<string> _probeLogged = new();

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
                    _readErrorLogged = false;
                }
                catch (SqlSugarException ex)
                {
                    if (!_readErrorLogged)
                    {
                        _readErrorLogged = true;
                        LogHelper.SysError("读取设备连接失败", ex);
                    }
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

            return ConnectRepeated(() => Probe("PLC", () => ProbePlc(link), true));
        }

        /// <summary>
        /// 扫码枪：占用中的连接算已连接，否则短连接。不发送触发。
        /// </summary>
        private bool ReadScan(ScannerEntity entity)
        {
            if (DeviceLinkUse.ScanConnected(entity.Id))
                return true;

            return ConnectRepeated(() => Probe(ScannerName(entity), () => ProbeScan(entity), false));
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
                _publishErrorLogged = false;
            }
            catch (InvalidOperationException ex)
            {
                if (_publishErrorLogged)
                    return;

                _publishErrorLogged = true;
                LogHelper.SysError("更新设备连接状态失败", ex);
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
            catch (ObjectDisposedException)
            {
                // 探测结束时，通道已经关掉就忽略。
            }
            catch (SocketException)
            {
                // 关掉短连接的失败不另记，连接失败那一条已经记下。
            }
            catch (Win32Exception)
            {
                // 关掉串口的失败不另记。
            }
            catch (UnauthorizedAccessException)
            {
                // 关掉短连接的失败不另记。
            }
            catch (IOException)
            {
                // 关掉短连接的失败不另记。
            }
            catch (InvalidOperationException)
            {
                // 关掉短连接的失败不另记。
            }
        }

        /// <summary>
        /// 做一次短连接。失败时同一台设备只记一条，连上后允许再记。
        /// </summary>
        private bool Probe(string name, Func<bool> once, bool plc)
        {
            try
            {
                var connected = once();
                if (connected)
                    _probeLogged.Remove(name);

                return connected;
            }
            catch (TimeoutException ex)
            {
                return NoteProbe(name, plc, ex);
            }
            catch (SocketException ex)
            {
                return NoteProbe(name, plc, ex);
            }
            catch (ObjectDisposedException ex)
            {
                return NoteProbe(name, plc, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                return NoteProbe(name, plc, ex);
            }
            catch (IOException ex)
            {
                return NoteProbe(name, plc, ex);
            }
            catch (InvalidOperationException ex)
            {
                return NoteProbe(name, plc, ex);
            }
            catch (ArgumentException ex)
            {
                return NoteProbe(name, plc, ex);
            }
        }

        /// <summary>
        /// 短连接抛出的异常记一条设备日志，并当作没连上。
        /// </summary>
        private bool NoteProbe(string name, bool plc, Exception exception)
        {
            if (_probeLogged.Add(name))
            {
                if (plc)
                    LogHelper.PlcIoError("连接 PLC 失败", exception);
                else
                    LogHelper.ScanIoError("连接扫码枪失败", exception);
            }

            return false;
        }
    }
}
