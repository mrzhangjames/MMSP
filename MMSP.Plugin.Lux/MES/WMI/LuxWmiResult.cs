namespace MMSP.Plugin.Lux.WMI
{
    /// <summary>
    /// 一次 WMI 调用的结果。成功时 Content 是返回的 content，失败时 Error 是原因。
    /// </summary>
    public sealed class LuxWmiResult
    {
        /// <summary>
        /// 成功和失败都从这里创建。
        /// </summary>
        private LuxWmiResult(bool isSuccess, string content, string error)
        {
            IsSuccess = isSuccess;
            Content = content;
            Error = error;
        }

        /// <summary>
        /// state 为 true。
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 返回内容。对象和数组保留 JSON 文本。
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// 失败原因。不含密钥和密文。
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// 成功。
        /// </summary>
        public static LuxWmiResult Ok(string content)
        {
            return new LuxWmiResult(true, content ?? "", "");
        }

        /// <summary>
        /// 失败。
        /// </summary>
        public static LuxWmiResult Fail(string error)
        {
            return new LuxWmiResult(false, "", error ?? "");
        }
    }
}
