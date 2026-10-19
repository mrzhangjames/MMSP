using MMSP.Core.Model.Sys;

namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 一台 PLC 连接。调用方不接触 HSL 的类型。
    /// </summary>
    public interface IPlcClient : IDisposable
    {
        /// <summary>
        /// 协议。
        /// </summary>
        PlcProtocol Protocol { get; }

        /// <summary>
        /// 当前是否已连接。串口打开成功、网口 ConnectServer 成功，或 UDP 参数就绪时为 true。
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 按连接参数建立通讯。失败时返回原因，不抛出。
        /// </summary>
        PlcIoResult Connect();

        /// <summary>
        /// 按点位类型读取。地址原样交给协议。
        /// </summary>
        PlcIoResult Read(string address, PlcDataType dataType, int length);

        /// <summary>
        /// 按点位类型写入。文本无法按类型解析时不发送报文。
        /// </summary>
        PlcIoResult Write(string address, PlcDataType dataType, string value);

        /// <summary>
        /// 关闭连接。可以再次 Connect。
        /// </summary>
        void Close();
    }
}
