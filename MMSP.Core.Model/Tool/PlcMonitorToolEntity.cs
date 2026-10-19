using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// PLC 监控工具。读到期望值时这一步结束。
    /// </summary>
    public class PlcMonitorToolEntity : PluginToolModel
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
        /// 期望值。
        /// </summary>
        public string ExpectValue { get; set; } = "";

        /// <summary>
        /// 读到期望值后是否写回重置值。
        /// </summary>
        public bool Reset { get; set; }

        /// <summary>
        /// 间隔，毫秒。
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// 重置时写入的值。
        /// </summary>
        public string ResetValue { get; set; } = "";
    }
}
