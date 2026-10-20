using MMSP.Core.Model.Sys;
using MMSP.Service.Device.Plc;
using MMSP.Service.Device.Scan;

namespace MMSP.Service.Flow
{
    /// <summary>
    /// 一次运行使用的设备。多条流程任务可以同时调用，客户端的创建由实现串起来。
    /// </summary>
    public interface IFlowDeviceSession : IDisposable
    {
        /// <summary>
        /// 按名称找到扫码枪，连上后触发一次。
        /// </summary>
        ScanIoResult Trigger(string scannerName, string command, CancellationToken token);

        /// <summary>
        /// 读一个 PLC 点。第一次使用时才连接。
        /// </summary>
        PlcIoResult Read(string address, PlcDataType dataType, int length, CancellationToken token);

        /// <summary>
        /// 写一个 PLC 点。第一次使用时才连接。
        /// </summary>
        PlcIoResult Write(string address, PlcDataType dataType, string value, CancellationToken token);

        /// <summary>
        /// 等待指定毫秒。取消时返回 false。小于等于 0 时立刻返回。
        /// </summary>
        bool Wait(int milliseconds, CancellationToken token);
    }
}
