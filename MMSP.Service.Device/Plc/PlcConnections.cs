namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 把已保存的连接字段收成 PlcConnection。不读数据库。
    /// </summary>
    public static class PlcConnections
    {
        /// <summary>
        /// 协议用调用方已经确认过的值。非法字节序改成 ABCD，站号收成一个字节。读、写超时小于等于 0 时按 1000 毫秒。
        /// </summary>
        public static PlcConnection Create(
            PlcProtocol protocol,
            string? host,
            int port,
            int station,
            string? dataFormat,
            int readTimeoutMs,
            int writeTimeoutMs)
        {
            var format = dataFormat;
            if (format is not ("ABCD" or "CDAB" or "BADC" or "DCBA"))
                format = "ABCD";

            var timeout = readTimeoutMs > 0 ? readTimeoutMs : 1000;
            var writeTimeout = writeTimeoutMs > 0 ? writeTimeoutMs : 1000;
            return new PlcConnection
            {
                Protocol = protocol,
                Host = host ?? "",
                Port = port,
                Station = ClampStation(station),
                DataFormat = format,
                ConnectTimeout = timeout,
                ReceiveTimeout = timeout,
                WriteTimeout = writeTimeout
            };
        }

        /// <summary>
        /// 站号收成一个字节。
        /// </summary>
        private static byte ClampStation(int station)
        {
            if (station < 0)
                return 0;

            if (station > 255)
                return 255;

            return (byte)station;
        }
    }
}
