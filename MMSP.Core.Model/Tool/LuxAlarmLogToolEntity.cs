using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 立讯报警日志工具。每一行是一个要监视的地址。
    /// </summary>
    public class LuxAlarmLogToolEntity : PluginToolModel
    {
        /// <summary>
        /// 上升沿是否写入流程日志。整份工具共用，默认勾选。
        /// </summary>
        public bool Log { get; set; } = true;

        /// <summary>
        /// 报警行。按集合顺序执行。
        /// </summary>
        public List<LuxAlarmLogRow> Rows { get; set; } = [];
    }

    /// <summary>
    /// 立讯报警日志的一行。地址为空的行不执行。
    /// </summary>
    public class LuxAlarmLogRow
    {
        /// <summary>
        /// PLC 地址。
        /// </summary>
        public string Address { get; set; } = "";

        /// <summary>
        /// 数据类型。新行默认 UInt16。
        /// </summary>
        public PlcDataType DataType { get; set; } = PlcDataType.UInt16;

        /// <summary>
        /// 触发值。读数与它相等时记一次上升沿。
        /// </summary>
        public string TriggerValue { get; set; } = "";

        /// <summary>
        /// 报警描述。写进日志。
        /// </summary>
        public string Description { get; set; } = "";
    }
}
