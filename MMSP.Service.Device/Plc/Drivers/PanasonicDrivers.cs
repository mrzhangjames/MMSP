using HslCommunication.Profinet.Panasonic;

namespace MMSP.Service.Device.Plc.Drivers
{
    /// <summary>
    /// 松下。MC 和 Mewtocol over TCP 使用 Host、Port、Station。Mewtocol 串口使用 Host 作为 COM 口和 BaudRate。
    /// </summary>
    internal static class PanasonicDrivers
    {
        /// <summary>
        /// 登记松下客户端。
        /// </summary>
        public static void Register(Action<PlcProtocol, Func<PlcConnection, HslPlcDriver>> add)
        {
            add(PlcProtocol.PanasonicMcNet, link => new TcpPlcDriver(link, static item =>
            {
                var plc = new PanasonicMcNet(item.Host, item.Port);
                PlcDeviceSetup.UseTimeouts(plc, item);
                return plc;
            }));
            add(PlcProtocol.PanasonicMewtocol, link => new SerialPortPlcDriver(link, static item =>
            {
                var plc = new PanasonicMewtocol(item.Station);
                PlcDeviceSetup.UseSerial(plc, item);
                return plc;
            }));
            add(PlcProtocol.PanasonicMewtocolOverTcp, link => new TcpPlcDriver(link, static item =>
            {
                var plc = new PanasonicMewtocolOverTcp(item.Host, item.Port, item.Station);
                PlcDeviceSetup.UseTimeouts(plc, item);
                return plc;
            }));
        }
    }
}
