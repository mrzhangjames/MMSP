using HslCommunication.Core;
using HslCommunication.ModBus;

namespace MMSP.Service.Device.Plc.Drivers
{
    /// <summary>
    /// Modbus。使用 Host、Port、Station、DataFormat、ConnectTimeout、ReceiveTimeout。串口另用 BaudRate、Parity。
    /// </summary>
    internal static class ModbusDrivers
    {
        /// <summary>
        /// 登记六种 Modbus 客户端。Modbus TCP 使用 Host、Port、Station 和 DataFormat。
        /// </summary>
        public static void Register(Action<PlcProtocol, Func<PlcConnection, HslPlcDriver>> add)
        {
            add(PlcProtocol.ModbusTcp, link => Tcp(link, static item =>
            {
                var plc = new ModbusTcpNet(item.Host, item.Port, item.Station);
                PlcDeviceSetup.UseModbus(plc, item);
                return plc;
            }));
            add(PlcProtocol.ModbusUdp, link => Tcp(link, static item =>
            {
                var plc = new ModbusUdpNet(item.Host, item.Port, item.Station);
                PlcDeviceSetup.UseModbus(plc, item);
                return plc;
            }));
            add(PlcProtocol.ModbusRtu, link => Serial(link, static item =>
            {
                var plc = new ModbusRtu(item.Station);
                PlcDeviceSetup.UseModbus(plc, item);
                PlcDeviceSetup.UseSerial(plc, item);
                return plc;
            }));
            add(PlcProtocol.ModbusRtuOverTcp, link => Tcp(link, static item =>
            {
                var plc = new ModbusRtuOverTcp(item.Host, item.Port, item.Station);
                PlcDeviceSetup.UseModbus(plc, item);
                return plc;
            }));
            add(PlcProtocol.ModbusAscii, link => Serial(link, static item =>
            {
                var plc = new ModbusAscii(item.Station);
                PlcDeviceSetup.UseModbus(plc, item);
                PlcDeviceSetup.UseSerial(plc, item);
                return plc;
            }));
            add(PlcProtocol.ModbusAsciiOverTcp, link => Tcp(link, static item =>
            {
                var plc = new ModbusAsciiOverTcp(item.Host, item.Port, item.Station);
                PlcDeviceSetup.UseModbus(plc, item);
                return plc;
            }));
        }

        private static HslPlcDriver Tcp(PlcConnection link, Func<PlcConnection, IReadWriteNet> create)
        {
            return new TcpPlcDriver(link, create);
        }

        private static HslPlcDriver Serial(PlcConnection link, Func<PlcConnection, IReadWriteNet> create)
        {
            return new SerialPortPlcDriver(link, create);
        }
    }
}
