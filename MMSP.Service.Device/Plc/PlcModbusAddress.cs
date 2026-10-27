namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// Modbus 地址。汇川 H5U 的 D 元件号与保持寄存器号相同，例如 D5244 就是 5244。
    /// </summary>
    internal static class PlcModbusAddress
    {
        /// <summary>
        /// 去掉单独的 D 前缀。纯数字、线圈和其他元件保持原样。
        /// </summary>
        public static string Normalize(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return address ?? "";

            var text = address.Trim();
            if (text.Length > 1 && (text[0] == 'D' || text[0] == 'd') && ushort.TryParse(text.AsSpan(1), out _))
                return text[1..];

            return address;
        }
    }
}
