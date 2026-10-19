using HslCommunication.Core;
using HslCommunication.Profinet.Omron;

namespace MMSP.Service.Device.Plc.Drivers
{
    /// <summary>
    /// 欧姆龙。FINS 和 CIP 使用 Host、Port。HostLink 串口使用 Host 作为 COM 口和 BaudRate。CIP 槽号使用 Slot。
    /// </summary>
    internal static class OmronDrivers
    {
        /// <summary>
        /// 登记欧姆龙客户端。FINS UDP 没有长连接，参数就绪即视为连接成功。
        /// </summary>
        public static void Register(Action<PlcProtocol, Func<PlcConnection, HslPlcDriver>> add)
        {
            add(PlcProtocol.OmronFinsNet, link => Tcp(link, static item => new OmronFinsNet(item.Host, item.Port)));
            add(PlcProtocol.OmronFinsUdp, link => Tcp(link, static item => new OmronFinsUdp(item.Host, item.Port)));
            add(PlcProtocol.OmronHostLink, link => Serial(link, static item => new OmronHostLink()));
            add(PlcProtocol.OmronHostLinkOverTcp, link => Tcp(link, static item => new OmronHostLinkOverTcp(item.Host, item.Port)));
            add(PlcProtocol.OmronHostLinkCMode, link => Serial(link, static item => new OmronHostLinkCMode()));
            add(PlcProtocol.OmronHostLinkCModeOverTcp, link => Tcp(link, static item => new OmronHostLinkCModeOverTcp(item.Host, item.Port)));
            add(PlcProtocol.OmronCipNet, link => Tcp(link, static item => WithSlot(new OmronCipNet(item.Host, item.Port), item)));
            add(PlcProtocol.OmronConnectedCipNet, link => Tcp(link, static item => new OmronConnectedCipNet(item.Host, item.Port)));
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

        private static IReadWriteNet WithSlot(IReadWriteNet plc, PlcConnection item)
        {
            PlcDeviceSetup.SetByte(plc, "Slot", item.Slot);
            return plc;
        }
    }
}
