using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 单扫码工具。一次读取一个条码。
    /// </summary>
    public class SingleScanToolEntity : PluginToolModel
    {
        /// <summary>
        /// 扫码枪名。
        /// </summary>
        public string Scanner { get; set; } = "";

        /// <summary>
        /// 触发字符串。
        /// </summary>
        public string Trigger { get; set; } = "";
    }
}
