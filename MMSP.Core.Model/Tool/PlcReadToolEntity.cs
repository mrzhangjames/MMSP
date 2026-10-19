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
    }
}
