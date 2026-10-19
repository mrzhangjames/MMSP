using HslCommunication.Core;
using HslCommunication.Profinet.Siemens;

namespace MMSP.Service.Device.Plc.Drivers
{
    /// <summary>
    /// 西门子。S7 使用 Host、Port、Series、Rack、Slot 和超时。PPI、MPI 串口使用 Host 作为 COM 口。
    /// </summary>
    internal static class SiemensDrivers
    {
        /// <summary>
        /// 登记西门子客户端。Series 取 S1200、S300、S400、S1500、S200Smart、S200，认不出时用 S1200。
        /// </summary>
        public static void Register(Action<PlcProtocol, Func<PlcConnection, HslPlcDriver>> add)
        {
            add(PlcProtocol.SiemensS7, link => new TcpPlcDriver(link, static item =>
            {
                var plc = new SiemensS7Net(Cpu(item.Series), item.Host) { Port = item.Port };
                PlcDeviceSetup.SetByte(plc, "Rack", item.Rack);
                PlcDeviceSetup.SetByte(plc, "Slot", item.Slot);
                PlcDeviceSetup.UseTimeouts(plc, item);
                return plc;
            }));
            add(PlcProtocol.SiemensFetchWrite, link => new TcpPlcDriver(link, static item =>
            {
                var plc = new SiemensFetchWriteNet(item.Host, item.Port);
                PlcDeviceSetup.UseTimeouts(plc, item);
                return plc;
            }));
            add(PlcProtocol.SiemensPPI, link => new SerialPortPlcDriver(link, static item => Prepare(new SiemensPPI(), item)));
            add(PlcProtocol.SiemensPPIOverTcp, link => new TcpPlcDriver(link, static item =>
            {
                var plc = new SiemensPPIOverTcp(item.Host, item.Port);
                PlcDeviceSetup.SetStation(plc, item.Station);
                PlcDeviceSetup.UseTimeouts(plc, item);
                return plc;
            }));
            add(PlcProtocol.SiemensMPI, link => new SerialPortPlcDriver(link, static item => Prepare(new SiemensMPI(), item)));
        }

        private static IReadWriteNet Prepare(IReadWriteNet plc, PlcConnection item)
        {
            PlcDeviceSetup.SetStation(plc, item.Station);
            PlcDeviceSetup.UseSerial(plc, item);
            return plc;
        }

        /// <summary>
        /// 把 Series 收成 HSL 的 CPU 枚举。
        /// </summary>
        private static SiemensPLCS Cpu(string series)
        {
            return series.Trim().Replace(" ", "").ToUpperInvariant() switch
            {
                "S300" => SiemensPLCS.S300,
                "S400" => SiemensPLCS.S400,
                "S1500" => SiemensPLCS.S1500,
                "S200SMART" => SiemensPLCS.S200Smart,
                "S200" => SiemensPLCS.S200,
                _ => SiemensPLCS.S1200
            };
        }
    }
}
