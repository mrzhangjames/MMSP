using HslCommunication.Core;
using HslCommunication.Profinet.AllenBradley;

namespace MMSP.Service.Device.Plc.Drivers
{
    /// <summary>
    /// 罗克韦尔。网口使用 Host、Port。带槽号的 CIP 使用 Slot。DF1 串口使用 Host 作为 COM 口、Station 和 BaudRate。
    /// </summary>
    internal static class AllenBradleyDrivers
    {
        /// <summary>
        /// 登记罗克韦尔客户端。
        /// </summary>
        public static void Register(Action<PlcProtocol, Func<PlcConnection, HslPlcDriver>> add)
        {
            add(PlcProtocol.AllenBradleyNet, link => Tcp(link, static item => WithSlot(new AllenBradleyNet(item.Host, item.Port), item)));
            add(PlcProtocol.AllenBradleyConnectedCipNet, link => Tcp(link, static item => new AllenBradleyConnectedCipNet(item.Host, item.Port)));
            add(PlcProtocol.AllenBradleyMicroCip, link => Tcp(link, static item => WithSlot(new AllenBradleyMicroCip(item.Host, item.Port), item)));
            add(PlcProtocol.AllenBradleyPcccNet, link => Tcp(link, static item => new AllenBradleyPcccNet(item.Host, item.Port)));
            add(PlcProtocol.AllenBradleySLCNet, link => Tcp(link, static item => new AllenBradleySLCNet(item.Host, item.Port)));
            add(PlcProtocol.AllenBradleyDF1Serial, link => new SerialPortPlcDriver(link, static item =>
            {
                var plc = new AllenBradleyDF1Serial();
                PlcDeviceSetup.UseSerial(plc, item);
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

        private static IReadWriteNet WithSlot(IReadWriteNet plc, PlcConnection item)
        {
            PlcDeviceSetup.SetByte(plc, "Slot", item.Slot);
            return plc;
        }
    }
}
