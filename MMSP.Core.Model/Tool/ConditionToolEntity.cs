using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 条件判断工具。
    /// </summary>
    public class ConditionToolEntity : PluginToolModel
    {
        /// <summary>
        /// 条件表达式。
        /// </summary>
        public string Expression { get; set; } = "";

        /// <summary>
        /// 判断结果写入的流程变量。空表示这一步不写出参。
        /// </summary>
        public string Output { get; set; } = "";

        /// <summary>
        /// 本步读取的流程变量。名称来自变量表。
        /// </summary>
        public List<ToolInput> Inputs { get; set; } = [];
    }
}
