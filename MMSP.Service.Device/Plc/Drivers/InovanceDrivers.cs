using HslCommunication.Profinet.Inovance;

namespace MMSP.Service.Device.Plc.Drivers
{
    /// <summary>
    /// 汇川。Modbus 派生协议使用 Host、Port、Station、DataFormat、Series。ComputerLink 是串口。CIP 和 Easy 使用 Host、Port。
    /// </summary>
    internal static class InovanceDrivers
    {
        /// <summary>
        /// 登记汇川客户端。Series 交给 HSL 的系列枚举。空着时用 H5U，否则库默认 AM，不认 D 地址。
        /// </summary>
        public static void Register(Action<PlcProtocol, Func<PlcConnection, HslPlcDriver>> add)
        {
            add(PlcProtocol.InovanceTcp, link => new TcpPlcDriver(link, static item => Prepare(new InovanceTcpNet(item.Host, item.Port, item.Station), item, serial: false)));
            add(PlcProtocol.InovanceSerial, link => new SerialPortPlcDriver(link, static item => Prepare(new InovanceSerial(item.Station), item, serial: true)));
            add(PlcProtocol.InovanceSerialOverTcp, link => new TcpPlcDriver(link, static item => Prepare(new InovanceSerialOverTcp(item.Host, item.Port, item.Station), item, serial: false)));
            add(PlcProtocol.InovanceComputerLink, link => new SerialPortPlcDriver(link, static item =>
            {
                var plc = new InovanceComputerLink();
                PlcDeviceSetup.UseSerial(plc, item);
                return plc;
            }));
            add(PlcProtocol.InovanceConnectedCip, link => new TcpPlcDriver(link, static item =>
            {
                var plc = new InovanceConnectedCipNet(item.Host, item.Port);
                PlcDeviceSetup.UseTimeouts(plc, item);
                return plc;
            }));
            add(PlcProtocol.InovanceEasyNet, link => new TcpPlcDriver(link, static item =>
            {
                var plc = new InovanceEasyNet(item.Host, item.Port);
                PlcDeviceSetup.UseTimeouts(plc, item);
                return plc;
            }));
        }

        private static T Prepare<T>(T plc, PlcConnection item, bool serial)
        {
            PlcDeviceSetup.UseModbus(plc!, item);
            PlcDeviceSetup.SetSeries(plc!, string.IsNullOrWhiteSpace(item.Series) ? "H5U" : item.Series);
            if (serial)
                PlcDeviceSetup.UseSerial(plc!, item);
            return plc;
        }
    }
}
