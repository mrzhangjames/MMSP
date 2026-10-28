namespace MMSP.Core.License
{
    /// <summary>
    /// 许可证库捕获异常后的出口。本库不引用日志项目。主程序启动时接到运行日志。没接上时不写。
    /// </summary>
    public static class LicenseCaughtLog
    {
        /// <summary>
        /// 写一条异常。由日志模块登记。
        /// </summary>
        public static Action<string, Exception>? Error { get; set; }

        /// <summary>
        /// 记下一条异常。
        /// </summary>
        public static void Report(string message, Exception exception)
        {
            Error?.Invoke(message, exception);
        }
    }
}
