namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 创建 PLC 客户端。每台 PLC 一个实例，不是单例。
    /// </summary>
    public static class PlcClients
    {
        /// <summary>
        /// 按协议创建客户端。尚未授权时先写入授权码。
        /// </summary>
        public static IPlcClient Open(PlcConnection connection)
        {
            HslLicense.Apply();
            return new PlcClient(PlcDriverRegistry.Create(connection));
        }

        /// <summary>
        /// 还没有驱动的协议。空列表表示枚举和登记表一致。
        /// </summary>
        public static IReadOnlyList<PlcProtocol> MissingProtocols()
        {
            return PlcDriverRegistry.Missing();
        }
    }
}
