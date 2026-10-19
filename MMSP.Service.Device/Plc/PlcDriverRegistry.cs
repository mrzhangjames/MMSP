namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 协议到创建函数的登记表。新增协议只在这里补一项。
    /// </summary>
    internal static class PlcDriverRegistry
    {
        private static readonly Dictionary<PlcProtocol, Func<PlcConnection, HslPlcDriver>> Map = [];

        /// <summary>
        /// 登记全部品牌。
        /// </summary>
        static PlcDriverRegistry()
        {
            Drivers.ModbusDrivers.Register(Add);
            Drivers.SiemensDrivers.Register(Add);
            Drivers.MelsecDrivers.Register(Add);
            Drivers.OmronDrivers.Register(Add);
            Drivers.AllenBradleyDrivers.Register(Add);
            Drivers.KeyenceDrivers.Register(Add);
            Drivers.PanasonicDrivers.Register(Add);
            Drivers.DeltaDrivers.Register(Add);
            Drivers.InovanceDrivers.Register(Add);
            Drivers.XinJeDrivers.Register(Add);
            Drivers.OtherPlcDrivers.Register(Add);
        }

        /// <summary>
        /// 按连接里的协议创建驱动。没有登记时抛出，避免静默走错协议。
        /// </summary>
        public static HslPlcDriver Create(PlcConnection connection)
        {
            if (!Map.TryGetValue(connection.Protocol, out var create))
                throw new InvalidOperationException("PLC 协议未登记 " + connection.Protocol);

            return create(connection);
        }

        /// <summary>
        /// 枚举里有、登记表里没有的协议。
        /// </summary>
        public static IReadOnlyList<PlcProtocol> Missing()
        {
            return Enum.GetValues<PlcProtocol>().Where(item => !Map.ContainsKey(item)).ToArray();
        }

        /// <summary>
        /// 登记一个协议。重复登记直接覆盖。
        /// </summary>
        private static void Add(PlcProtocol protocol, Func<PlcConnection, HslPlcDriver> create)
        {
            Map[protocol] = create;
        }
    }
}
