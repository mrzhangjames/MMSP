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

        /// <summary>
        /// 本步读取的流程变量。名称来自变量表。这一期不编译脚本。
        /// </summary>
        public List<ToolInput> Inputs { get; set; } = [];
    }
}
