namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 一次 PLC 连接、读取或写入的结果。
    /// </summary>
    public sealed class PlcIoResult
    {
        private PlcIoResult(bool isSuccess, string value, string message)
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
        /// 读取到的文本。写入和连接成功时为空。
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// 失败原因。成功时为空。
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 成功。
        /// </summary>
        public static PlcIoResult Ok(string value)
        {
            return new PlcIoResult(true, value, "");
        }

        /// <summary>
        /// 失败。
        /// </summary>
        public static PlcIoResult Fail(string message)
        {
            return new PlcIoResult(false, "", message);
        }
    }
}
