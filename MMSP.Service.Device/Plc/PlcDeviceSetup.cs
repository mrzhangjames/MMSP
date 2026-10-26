using System.IO.Ports;
using System.Reflection;
using HslCommunication.Core;
using HslCommunication.Core.Device;

namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 把连接参数写到 HSL 设备上。某个属性不存在时跳过。
    /// </summary>
    internal static class PlcDeviceSetup
    {
        /// <summary>
        /// Modbus 及派生协议的字节序，地址从 0 开始。
        /// </summary>
        public static void UseModbus(object device, PlcConnection connection)
        {
            Set(device, "DataFormat", ToDataFormat(connection.DataFormat));
            Set(device, "AddressStartWithZero", true);
            SetStation(device, connection.Station);
            UseTimeouts(device, connection);
        }

        /// <summary>
        /// 网口超时。IP 和端口由各协议的构造函数写入。
        /// </summary>
        public static void UseTimeouts(object device, PlcConnection connection)
        {
            Set(device, "ConnectTimeOut", connection.ConnectTimeout);
            Set(device, "ReceiveTimeOut", connection.ReceiveTimeout);
        }

        /// <summary>
        /// 改这一次调用的接收超时。属性不存在时跳过。
        /// </summary>
        public static void SetReceiveTimeout(object device, int timeout)
        {
            Set(device, "ReceiveTimeOut", timeout > 0 ? timeout : 1000);
        }

        /// <summary>
        /// 串口名称、波特率和接收超时。
        /// </summary>
        public static void UseSerial(object device, PlcConnection connection)
        {
            Set(device, "PortName", connection.Host);
            Set(device, "BaudRate", connection.BaudRate > 0 ? connection.BaudRate : 9600);
            Set(device, "ReceiveTimeOut", connection.ReceiveTimeout);
            SetStation(device, connection.Station);
            if (Enum.TryParse<Parity>(connection.Parity, true, out var parity))
                Set(device, "Parity", parity);
        }

        /// <summary>
        /// 站号。属性不是 byte 时跳过。
        /// </summary>
        public static void SetStation(object device, byte station)
        {
            Set(device, "Station", station);
        }

        /// <summary>
        /// 系列枚举。文本对不上该属性的枚举时保留库的默认值。
        /// </summary>
        public static void SetSeries(object device, string series)
        {
            if (string.IsNullOrWhiteSpace(series))
                return;

            var property = device.GetType().GetProperty("Series");
            if (property is null || !property.PropertyType.IsEnum)
                return;

            if (Enum.TryParse(property.PropertyType, series.Trim(), true, out var value) && value is not null)
                property.SetValue(device, value);
        }

        /// <summary>
        /// 机架号或槽号，收成 byte。
        /// </summary>
        public static void SetByte(object device, string name, int value)
        {
            Set(device, name, (byte)Math.Clamp(value, 0, 255));
        }

        /// <summary>
        /// ABCD、CDAB、BADC、DCBA。其余按 ABCD。
        /// </summary>
        public static DataFormat ToDataFormat(string text)
        {
            return text.Trim().ToUpperInvariant() switch
            {
                "CDAB" => DataFormat.CDAB,
                "BADC" => DataFormat.BADC,
                "DCBA" => DataFormat.DCBA,
                _ => DataFormat.ABCD
            };
        }

        /// <summary>
        /// 属性存在且类型一致时写入。
        /// </summary>
        private static void Set(object device, string name, object value)
        {
            var property = device.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (property is null || !property.CanWrite || !property.PropertyType.IsInstanceOfType(value))
                return;

            property.SetValue(device, value);
        }
    }
}
