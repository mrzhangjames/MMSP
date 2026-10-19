using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// Json 解析工具。从来源文本取出路径上的值。
    /// </summary>
    public class JsonParseToolEntity : PluginToolModel
    {
        /// <summary>
        /// 待解析的文本或变量名。
        /// </summary>
        public string Source { get; set; } = "";

        /// <summary>
        /// Json 路径。
        /// </summary>
        public string JsonPath { get; set; } = "";

        /// <summary>
        /// 解析结果写入的名称。
        /// </summary>
        public string Target { get; set; } = "";
    }
}
