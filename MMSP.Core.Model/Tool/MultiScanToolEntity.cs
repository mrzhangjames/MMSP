using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 多扫码工具。可以选多把枪，每把枪按条码数量读取。
    /// </summary>
    public class MultiScanToolEntity : PluginToolModel
    {
        /// <summary>
        /// 扫码枪名。多把枪用顿号连接。
        /// </summary>
        public string Scanner { get; set; } = "";

        /// <summary>
        /// 触发字符串。
        /// </summary>
        public string Trigger { get; set; } = "";

        /// <summary>
        /// 要读取的条码个数。
        /// </summary>
        public int Count { get; set; } = 1;
    }
}
