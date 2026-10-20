using MMSP.Core.Model.Sys;
using MMSP.Service.Device.Plc;
using MMSP.Service.Device.Scan;
using MMSP.Service.Flow;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 测试用设备。不打开真实连接。
    /// </summary>
    internal sealed class FakeDevices : IFlowDeviceSession
    {
        /// <summary>
        /// 已经触发的枪名和命令。
        /// </summary>
        public List<(string Name, string Command)> Triggers { get; } = [];

        /// <summary>
        /// 依次返回的扫码结果。空了就返回空条码成功。
        /// </summary>
        public Queue<ScanIoResult> ScanResults { get; } = new();

        /// <summary>
        /// 已经发出的读取。
        /// </summary>
        public List<(string Address, PlcDataType DataType, int Length)> Reads { get; } = [];

        /// <summary>
        /// 依次返回的读取结果。空了就失败。
        /// </summary>
        public Queue<PlcIoResult> ReadResults { get; } = new();

        /// <summary>
        /// 已经发出的写入。
        /// </summary>
        public List<(string Address, PlcDataType DataType, string Value)> Writes { get; } = [];

        /// <summary>
        /// 依次返回的写入结果。空了就成功。
        /// </summary>
        public Queue<PlcIoResult> WriteResults { get; } = new();

        /// <summary>
        /// 已经等待过的毫秒数。
        /// </summary>
        public List<int> Waits { get; } = [];

        /// <summary>
        /// 等待是否走完。false 表示被取消。
        /// </summary>
        public bool WaitCompletes { get; set; } = true;

        /// <summary>
        /// 替换等待结果。为空时仍按 <see cref="WaitCompletes"/> 和取消标记判断。
        /// </summary>
        public Func<int, bool>? OnWait { get; set; }

        /// <summary>
        /// 保护等待记录。多条流程会同时等待。
        /// </summary>
        private readonly object _gate = new();

        /// <summary>
        /// 记下触发并返回预设结果。
        /// </summary>
        public ScanIoResult Trigger(string scannerName, string command, CancellationToken token)
        {
            Triggers.Add((scannerName, command));
            return ScanResults.Count > 0 ? ScanResults.Dequeue() : ScanIoResult.Ok("");
        }

        /// <summary>
        /// 记下读取并返回预设结果。
        /// </summary>
        public PlcIoResult Read(string address, PlcDataType dataType, int length, CancellationToken token)
        {
            Reads.Add((address, dataType, length));
            return ReadResults.Count > 0 ? ReadResults.Dequeue() : PlcIoResult.Fail("没有读数");
        }

        /// <summary>
        /// 记下写入并返回预设结果。
        /// </summary>
        public PlcIoResult Write(string address, PlcDataType dataType, string value, CancellationToken token)
        {
            Writes.Add((address, dataType, value));
            return WriteResults.Count > 0 ? WriteResults.Dequeue() : PlcIoResult.Ok("");
        }

        /// <summary>
        /// 记下等待。不真正睡觉。
        /// </summary>
        public bool Wait(int milliseconds, CancellationToken token)
        {
            lock (_gate)
                Waits.Add(milliseconds);

            if (OnWait is not null)
                return OnWait(milliseconds);

            return WaitCompletes && !token.IsCancellationRequested;
        }

        /// <summary>
        /// 测试里没有连接要关。
        /// </summary>
        public void Dispose()
        {
        }
    }
}
