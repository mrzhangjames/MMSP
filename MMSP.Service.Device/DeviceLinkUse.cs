using MMSP.Core.Model.Helper;

namespace MMSP.Service.Device
{
    /// <summary>
    /// 设备页和流程已经连上的 PLC、扫码枪。底栏看到占用时直接显示已连接，不再另开短连接。
    /// </summary>
    public static class DeviceLinkUse
    {
        /// <summary>
        /// 保护两份回调。调用回调时不持有这把锁。
        /// </summary>
        private static readonly object Gate = new();

        /// <summary>
        /// 读取 PLC 是否正由设备页或流程连着。
        /// </summary>
        private static readonly List<Func<bool>> PlcReaders = new();

        /// <summary>
        /// 读取某一把扫码枪是否正由设备页或流程连着。
        /// </summary>
        private static readonly List<Func<Guid, bool>> ScanReaders = new();

        /// <summary>
        /// 这一轮 PLC 占用查询失败已经写过日志。有一次成功后清掉。
        /// </summary>
        private static bool _plcErrorLogged;

        /// <summary>
        /// 这一轮扫码枪占用查询失败已经写过日志。有一次成功后清掉。
        /// </summary>
        private static bool _scanErrorLogged;

        /// <summary>
        /// 登记一个 PLC 占用查询。同一个委托只保留一份。
        /// </summary>
        public static void AddPlc(Func<bool> read)
        {
            lock (Gate)
            {
                if (!PlcReaders.Contains(read))
                    PlcReaders.Add(read);
            }
        }

        /// <summary>
        /// 去掉一个 PLC 占用查询。
        /// </summary>
        public static void RemovePlc(Func<bool> read)
        {
            lock (Gate)
                PlcReaders.Remove(read);
        }

        /// <summary>
        /// 登记一个扫码枪占用查询。同一个委托只保留一份。
        /// </summary>
        public static void AddScan(Func<Guid, bool> read)
        {
            lock (Gate)
            {
                if (!ScanReaders.Contains(read))
                    ScanReaders.Add(read);
            }
        }

        /// <summary>
        /// 去掉一个扫码枪占用查询。
        /// </summary>
        public static void RemoveScan(Func<Guid, bool> read)
        {
            lock (Gate)
                ScanReaders.Remove(read);
        }

        /// <summary>
        /// 设备页或流程里的 PLC 当前已连接。
        /// </summary>
        public static bool PlcConnected()
        {
            Func<bool>[] readers;
            lock (Gate)
                readers = PlcReaders.ToArray();

            foreach (var read in readers)
            {
                try
                {
                    if (read())
                    {
                        _plcErrorLogged = false;
                        return true;
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    NoteUse(ref _plcErrorLogged, CaughtLogChannel.Plc, ex);
                }
                catch (InvalidOperationException ex)
                {
                    NoteUse(ref _plcErrorLogged, CaughtLogChannel.Plc, ex);
                }
            }

            return false;
        }

        /// <summary>
        /// 设备页或流程里的这把扫码枪当前已连接。
        /// </summary>
        public static bool ScanConnected(Guid id)
        {
            Func<Guid, bool>[] readers;
            lock (Gate)
                readers = ScanReaders.ToArray();

            foreach (var read in readers)
            {
                try
                {
                    if (read(id))
                    {
                        _scanErrorLogged = false;
                        return true;
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    NoteUse(ref _scanErrorLogged, CaughtLogChannel.Scan, ex);
                }
                catch (InvalidOperationException ex)
                {
                    NoteUse(ref _scanErrorLogged, CaughtLogChannel.Scan, ex);
                }
            }

            return false;
        }

        /// <summary>
        /// 占用查询失败只记一次，直到下一次查到已连接。
        /// </summary>
        private static void NoteUse(ref bool logged, CaughtLogChannel channel, Exception exception)
        {
            if (logged)
                return;

            logged = true;
            CaughtLog.Report(channel, "读取设备占用失败", exception);
        }
    }
}
