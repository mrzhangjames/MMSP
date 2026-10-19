namespace MMSP.Service.Device.Scan
{
    /// <summary>
    /// 一次扫码连接或触发的结果。
    /// </summary>
    public sealed class ScanIoResult
    {
        private ScanIoResult(bool isSuccess, string value, string message)
        {
            IsSuccess = isSuccess;
            Value = value;
            Message = message;
        }

        /// <summary>
        /// 是否成功。
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 读到的条码文本。连接成功时为空。
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// 失败原因。成功时为空。
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 成功。
        /// </summary>
        public static ScanIoResult Ok(string value)
        {
            return new ScanIoResult(true, value, "");
        }

        /// <summary>
        /// 失败。
        /// </summary>
        public static ScanIoResult Fail(string message)
        {
            return new ScanIoResult(false, "", message);
        }
    }
}
