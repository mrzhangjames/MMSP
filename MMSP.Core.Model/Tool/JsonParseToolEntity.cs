using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// Json 解析工具。从来源文本取出路径上的值。
    /// </summary>
    public class JsonParseToolEntity : PluginToolModel
    {
        /// <summary>
        /// 来源变量名。由左侧入参点选，试算不使用它。
        /// </summary>
        public string Source { get; set; } = "";

        /// <summary>
        /// Json 路径。
        /// </summary>
        public string JsonPath { get; set; } = "";

        /// <summary>
        /// 解析结果写入的名称。也是这一步的出参。
        /// </summary>
        public string Target { get; set; } = "";

        /// <summary>
        /// 与来源对应的入参。点选时只保留这一个。
        /// </summary>
        public List<ToolInput> Inputs { get; set; } = [];
    }
}
