namespace MMSP.App.TemplateMatch.Models
{
    /// <summary>
    /// 设备页日志一行的方向。用来上色，不写入运行日志。
    /// </summary>
    public enum DeviceLogKind
    {
        /// <summary>
        /// 普通说明。
        /// </summary>
        Info = 0,

        /// <summary>
        /// 发送。
        /// </summary>
        Send = 1,

        /// <summary>
        /// 接收。
        /// </summary>
        Receive = 2,

        /// <summary>
        /// 超时。
        /// </summary>
        Timeout = 3
    }

    /// <summary>
    /// 设备页自己的一行通讯记录。只在页面上显示。
    /// </summary>
    public sealed class DeviceLogLine
    {
        /// <summary>
        /// 时间，HH:mm:ss.fff。
        /// </summary>
        public string TimeText { get; init; } = "";

        /// <summary>
        /// 方向文字。普通行留空。
        /// </summary>
        public string DirectionText { get; init; } = "";

        /// <summary>
        /// 方向。
        /// </summary>
        public DeviceLogKind Kind { get; init; }

        /// <summary>
        /// 内容。
        /// </summary>
        public string Message { get; init; } = "";
    }
}
