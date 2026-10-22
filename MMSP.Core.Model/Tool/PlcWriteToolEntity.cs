using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// PLC 写入工具。
    /// </summary>
    public class PlcWriteToolEntity : PluginToolModel
    {
        /// <summary>
        /// PLC 地址。
        /// </summary>
        public string Address { get; set; } = "";

        /// <summary>
        /// 数据类型。
        /// </summary>
        public PlcDataType DataType { get; set; } = PlcDataType.Int16;

        /// <summary>
        /// 写入值取流程变量。为 false 时 Value 是字面值。
        /// </summary>
        public bool UseVariable { get; set; }

        /// <summary>
        /// 写入值。固定值时是字面值，变量时是流程变量名。
        /// </summary>
        public string Value { get; set; } = "";

        /// <summary>
        /// 变量写入时选中的流程变量。固定值时为空。
        /// </summary>
        public List<ToolInput> Inputs { get; set; } = [];
    }
}
