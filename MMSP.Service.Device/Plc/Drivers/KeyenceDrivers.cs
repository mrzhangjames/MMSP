using HslCommunication.Core;
using HslCommunication.Profinet.Keyence;

namespace MMSP.Service.Device.Plc.Drivers
{
    /// <summary>
    /// 基恩士 PLC。网口使用 Host、Port。Nano 串口使用 Host 作为 COM 口、Station 和 BaudRate。不含 SR2000 读码器。
    /// </summary>
    internal static class KeyenceDrivers
    {
        /// <summary>
        /// 登记基恩士 PLC 客户端。
        /// </summary>
        public static void Register(Action<PlcProtocol, Func<PlcConnection, HslPlcDriver>> add)
        {
            add(PlcProtocol.KeyenceMcNet, link => Tcp(link, static item => new KeyenceMcNet(item.Host, item.Port)));
            add(PlcProtocol.KeyenceMcAsciiNet, link => Tcp(link, static item => new KeyenceMcAsciiNet(item.Host, item.Port)));
            add(PlcProtocol.KeyenceNanoSerial, link => new SerialPortPlcDriver(link, static item =>
            {
                var plc = new KeyenceNanoSerial();
                PlcDeviceSetup.UseSerial(plc, item);
                return plc;
            }));
            add(PlcProtocol.KeyenceNanoSerialOverTcp, link => Tcp(link, static item =>
            {
                var plc = new KeyenceNanoSerialOverTcp(item.Host, item.Port);
                PlcDeviceSetup.SetStation(plc, item.Station);
                return plc;
            }));
            add(PlcProtocol.KeyenceKvOld, link => Tcp(link, static item => new KeyenceKvOld(item.Host, item.Port)));
        }

        private static HslPlcDriver Tcp(PlcConnection link, Func<PlcConnection, IReadWriteNet> create)
        {
            return new TcpPlcDriver(link, item =>
            {
                var plc = create(item);
                PlcDeviceSetup.UseTimeouts(plc, item);
                return plc;
            });
        }
    }
}
