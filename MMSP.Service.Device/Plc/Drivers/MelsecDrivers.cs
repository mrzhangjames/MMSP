using HslCommunication.Core;
using HslCommunication.Profinet.Melsec;

namespace MMSP.Service.Device.Plc.Drivers
{
    /// <summary>
    /// 三菱。网口使用 Host、Port 和超时。带站号的协议使用 Station。CIP 使用 Slot。串口使用 Host 作为 COM 口和 BaudRate。
    /// </summary>
    internal static class MelsecDrivers
    {
        /// <summary>
        /// 登记三菱客户端。
        /// </summary>
        public static void Register(Action<PlcProtocol, Func<PlcConnection, HslPlcDriver>> add)
        {
            add(PlcProtocol.MelsecMcNet, link => Tcp(link, static item => new MelsecMcNet(item.Host, item.Port)));
            add(PlcProtocol.MelsecMcAsciiNet, link => Tcp(link, static item => new MelsecMcAsciiNet(item.Host, item.Port)));
            add(PlcProtocol.MelsecMcUdp, link => Tcp(link, static item => new MelsecMcUdp(item.Host, item.Port)));
            add(PlcProtocol.MelsecMcAsciiUdp, link => Tcp(link, static item => new MelsecMcAsciiUdp(item.Host, item.Port)));
            add(PlcProtocol.MelsecMcRNet, link => Tcp(link, static item => new MelsecMcRNet(item.Host, item.Port)));
            add(PlcProtocol.MelsecA1ENet, link => Tcp(link, static item => new MelsecA1ENet(item.Host, item.Port)));
            add(PlcProtocol.MelsecA1EAsciiNet, link => Tcp(link, static item => new MelsecA1EAsciiNet(item.Host, item.Port)));
            add(PlcProtocol.MelsecFxSerial, link => Serial(link, static item => new MelsecFxSerial()));
            add(PlcProtocol.MelsecFxSerialOverTcp, link => Tcp(link, static item => new MelsecFxSerialOverTcp(item.Host, item.Port)));
            add(PlcProtocol.MelsecFxLinks, link => Serial(link, static item => new MelsecFxLinks()));
            add(PlcProtocol.MelsecFxLinksOverTcp, link => Tcp(link, static item => WithStation(new MelsecFxLinksOverTcp(item.Host, item.Port), item)));
            add(PlcProtocol.MelsecA3CNet, link => Serial(link, static item => new MelsecA3CNet()));
            add(PlcProtocol.MelsecA3CNetOverTcp, link => Tcp(link, static item => WithStation(new MelsecA3CNetOverTcp(item.Host, item.Port), item)));
            add(PlcProtocol.MelsecA4CNet, link => Serial(link, static item => new MelsecA4CNet()));
            add(PlcProtocol.MelsecA4CNetOverTcp, link => Tcp(link, static item => WithStation(new MelsecA4CNetOverTcp(item.Host, item.Port), item)));
            add(PlcProtocol.MelsecCipNet, link => Tcp(link, static item =>
            {
                var plc = new MelsecCipNet(item.Host, item.Port);
                PlcDeviceSetup.SetByte(plc, "Slot", item.Slot);
                return plc;
            }));
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

        private static HslPlcDriver Serial(PlcConnection link, Func<PlcConnection, IReadWriteNet> create)
        {
            return new SerialPortPlcDriver(link, item =>
            {
                var plc = create(item);
                PlcDeviceSetup.UseSerial(plc, item);
                return plc;
            });
        }

        private static IReadWriteNet WithStation(IReadWriteNet plc, PlcConnection item)
        {
            PlcDeviceSetup.SetStation(plc, item.Station);
            return plc;
        }
    }
}
