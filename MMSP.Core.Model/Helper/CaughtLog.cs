namespace MMSP.Core.Model.Helper
{
    /// <summary>
    /// 模型、数据库和设备库捕获异常后的出口。这些项目不能引用日志项目。
    /// 主程序启动时把 <see cref="Error"/> 接到日志。没接上时不写。
    /// </summary>
    public static class CaughtLog
    {
        /// <summary>
        /// 按通道写一条异常。由日志模块登记。
        /// </summary>
        public static Action<CaughtLogChannel, string, Exception>? Error { get; set; }

        /// <summary>
        /// 记下一条异常。
        /// </summary>
        public static void Report(CaughtLogChannel channel, string message, Exception exception)
        {
            Error?.Invoke(channel, message, exception);
        }
    }

    /// <summary>
    /// 捕获后写入哪一类日志。设备读写进设备日志，流程运行进流程日志，MES 交互进 MES 日志。
    /// </summary>
    public enum CaughtLogChannel
    {
        /// <summary>
        /// 只写运行日志。
        /// </summary>
        Sys = 0,

        /// <summary>
        /// 运行日志和设备日志。PLC 读写。
        /// </summary>
        Plc = 1,

        /// <summary>
        /// 运行日志和设备日志。扫码读写。
        /// </summary>
        Scan = 2,

        /// <summary>
        /// 运行日志和流程日志。
        /// </summary>
        Flow = 3,

        /// <summary>
        /// 运行日志和 MES 日志。
        /// </summary>
        Mes = 4
    }
}
