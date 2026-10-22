using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 多扫码工具。可以选多把枪，每把枪按条码数量读取。
    /// </summary>
    public class MultiScanToolEntity : PluginToolModel
    {
        /// <summary>
        /// 扫码枪名。多把枪用顿号连接。
        /// </summary>
        public string Scanner { get; set; } = "";

        /// <summary>
        /// 触发字符串。
        /// </summary>
        public string Trigger { get; set; } = "";

        /// <summary>
        /// 要读取的条码个数。
        /// </summary>
        public int Count { get; set; } = 1;

        /// <summary>
        /// 每个条码写入的流程变量。下标从 0 起。顺序与扫码相同：按枪名顿号顺序，每把枪连续 Count 个。空项表示该条码不写出参。
        /// </summary>
        public List<string> Outputs { get; set; } = [];

        /// <summary>
        /// 扫到后是否再写入 PLC。
        /// </summary>
        public bool WritePlc { get; set; }

        /// <summary>
        /// 写入 PLC 的地址。全部条码拼成一条后写入。
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
