using HslCommunication.Profinet.Delta;

namespace MMSP.Service.Device.Plc.Drivers
{
    /// <summary>
    /// 台达。使用 Host、Port、Station、DataFormat、Series 和超时。串口另用 BaudRate。Series 取 Dvp 或 AS。
    /// </summary>
    internal static class DeltaDrivers
    {
        /// <summary>
        /// 登记台达客户端。
        /// </summary>
        public static void Register(Action<PlcProtocol, Func<PlcConnection, HslPlcDriver>> add)
        {
            add(PlcProtocol.DeltaTcp, link => new TcpPlcDriver(link, static item => Prepare(new DeltaTcpNet(item.Host, item.Port, item.Station), item, serial: false)));
            add(PlcProtocol.DeltaSerial, link => new SerialPortPlcDriver(link, static item => Prepare(new DeltaSerial(item.Station), item, serial: true)));
            add(PlcProtocol.DeltaSerialOverTcp, link => new TcpPlcDriver(link, static item => Prepare(new DeltaSerialOverTcp(item.Host, item.Port, item.Station), item, serial: false)));
            add(PlcProtocol.DeltaSerialAscii, link => new SerialPortPlcDriver(link, static item => Prepare(new DeltaSerialAscii(item.Station), item, serial: true)));
            add(PlcProtocol.DeltaSerialAsciiOverTcp, link => new TcpPlcDriver(link, static item => Prepare(new DeltaSerialAsciiOverTcp(item.Host, item.Port, item.Station), item, serial: false)));
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
