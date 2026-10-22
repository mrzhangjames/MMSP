using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// PLC 读取工具。
    /// </summary>
    public class PlcReadToolEntity : PluginToolModel
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
        /// 读取长度。
        /// </summary>
        public int Length { get; set; } = 1;

        /// <summary>
        /// 读数写入的流程变量，表单上叫结果。空表示这一步不写出参。
        /// </summary>
        public string Output { get; set; } = "";
    }
}
