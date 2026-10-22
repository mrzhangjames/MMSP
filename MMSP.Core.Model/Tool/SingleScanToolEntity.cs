using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 单扫码工具。一次读取一个条码。
    /// </summary>
    public class SingleScanToolEntity : PluginToolModel
    {
        /// <summary>
        /// 扫码枪名。
        /// </summary>
        public string Scanner { get; set; } = "";

        /// <summary>
        /// 触发字符串。
        /// </summary>
        public string Trigger { get; set; } = "";

        /// <summary>
        /// 扫码结果写入的流程变量，表单上叫扫码结果。空表示这一步不写出参。
        /// </summary>
        public string Output { get; set; } = "";

        /// <summary>
        /// 扫到后是否再写入 PLC。
        /// </summary>
        public bool WritePlc { get; set; }

        /// <summary>
        /// 写入 PLC 的地址。
        /// </summary>
        public string WriteAddress { get; set; } = "";

        /// <summary>
        /// 写入 PLC 的数据类型。这是 PLC 类型，不是流程变量类型。
        /// </summary>
        public PlcDataType WriteDataType { get; set; } = PlcDataType.String;

        /// <summary>
        /// 写入 PLC 的长度。字符串时使用。
        /// </summary>
        public int WriteLength { get; set; } = 32;
    }
}
