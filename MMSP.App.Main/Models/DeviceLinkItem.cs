namespace MMSP.App.Main.Models
{
    /// <summary>
    /// 底栏里的一台设备。圆点看是否连上，文字是名称加连接状态。
    /// </summary>
    public sealed class DeviceLinkItem
    {
        /// <summary>
        /// 创建一项设备状态。
        /// </summary>
        /// <param name="name">PLC 或扫码枪名称。</param>
        /// <param name="isConnected">当前这一轮探测是否连上。</param>
        /// <param name="statusText">已连接或未连接。</param>
        public DeviceLinkItem(string name, bool isConnected, string statusText)
        {
            Name = name;
            IsConnected = isConnected;
            StatusText = statusText;
            Text = name + " " + statusText;
        }

        /// <summary>
        /// 设备名称。PLC 固定写 PLC。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 这一轮是否连上。
        /// </summary>
        public bool IsConnected { get; }

        /// <summary>
        /// 连接状态文字。
        /// </summary>
        public string StatusText { get; }

        /// <summary>
        /// 底栏上显示的整句。
        /// </summary>
        public string Text { get; }
    }
}
