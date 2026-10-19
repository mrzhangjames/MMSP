using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 延时工具。
    /// </summary>
    public class DelayToolEntity : PluginToolModel
    {
        /// <summary>
        /// 等待的毫秒数。
        /// </summary>
        public int Milliseconds { get; set; } = 0;
    }
}
