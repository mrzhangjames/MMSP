using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 脚本工具。
    /// </summary>
    public class ScriptToolEntity : PluginToolModel
    {
        /// <summary>
        /// 脚本正文。
        /// </summary>
        public string Script { get; set; } = "";
    }
}
