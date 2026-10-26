using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// UPH 工具。读一个流程变量，按 bool 或 int 在内部判成 OK 或 NG。
    /// </summary>
    public class UphToolEntity : PluginToolModel
    {
        /// <summary>
        /// 判定变量。空表示还没点选。
        /// </summary>
        public string Variable { get; set; } = "";

        /// <summary>
        /// 下拉类型。文字是 bool 或 int。缺省 bool。变量表里的整数仍叫 Int。
        /// </summary>
        public string ValueType { get; set; } = "bool";

        /// <summary>
        /// 本步读取的流程变量。与 <see cref="Variable"/> 保持同一个名称。
        /// </summary>
        public List<ToolInput> Inputs { get; set; } = [];
    }
}
