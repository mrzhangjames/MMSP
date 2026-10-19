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
        /// 写入值。
        /// </summary>
        public string Value { get; set; } = "";
    }
}
