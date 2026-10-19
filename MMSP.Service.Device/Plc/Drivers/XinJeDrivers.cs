using HslCommunication.Profinet.XINJE;

namespace MMSP.Service.Device.Plc.Drivers
{
    /// <summary>
    /// 信捷。Modbus 派生协议使用 Host、Port、Station、DataFormat、Series。内部协议使用 Host、Port、Station、DataFormat。
    /// </summary>
    internal static class XinJeDrivers
    {
        /// <summary>
        /// 登记信捷客户端。Series 交给 HSL 的系列枚举，对不上时保留默认系列。
        /// </summary>
        public static void Register(Action<PlcProtocol, Func<PlcConnection, HslPlcDriver>> add)
        {
            add(PlcProtocol.XinJETcp, link => new TcpPlcDriver(link, static item => Prepare(new XinJETcpNet(item.Host, item.Port, item.Station), item, serial: false)));
            add(PlcProtocol.XinJESerial, link => new SerialPortPlcDriver(link, static item => Prepare(new XinJESerial(item.Station), item, serial: true)));
            add(PlcProtocol.XinJESerialOverTcp, link => new TcpPlcDriver(link, static item => Prepare(new XinJESerialOverTcp(item.Host, item.Port, item.Station), item, serial: false)));
            add(PlcProtocol.XinJEInternalNet, link => new TcpPlcDriver(link, static item => Prepare(new XinJEInternalNet(item.Host, item.Port, item.Station), item, serial: false)));
        }

        private static T Prepare<T>(T plc, PlcConnection item, bool serial)
        {
            PlcDeviceSetup.UseModbus(plc!, item);
            PlcDeviceSetup.SetSeries(plc!, item.Series);
            if (serial)
                PlcDeviceSetup.UseSerial(plc!, item);
            return plc;
        }
    }
}
