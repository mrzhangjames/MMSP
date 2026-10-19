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
    }
}
