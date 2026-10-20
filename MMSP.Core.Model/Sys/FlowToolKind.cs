using System.ComponentModel;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 流程步骤的工具种类。数值与客户库 tool_kind 列一致。0 到 8 是通用工具，100 到 200 是立讯工具。
    /// </summary>
    public enum FlowToolKind
    {
        /// <summary>
        /// 单扫码。
        /// </summary>
        [Description("单扫码")]
        SingleScan = 0,

        /// <summary>
        /// 多扫码。
        /// </summary>
        [Description("多扫码")]
        MultiScan = 1,

        /// <summary>
        /// PLC 读取。
        /// </summary>
        [Description("PLC 读取")]
        PlcRead = 2,

        /// <summary>
        /// PLC 写入。
        /// </summary>
        [Description("PLC 写入")]
        PlcWrite = 3,

        /// <summary>
        /// PLC 监控。
        /// </summary>
        [Description("PLC 监控")]
        PlcMonitor = 4,

        /// <summary>
        /// 延时。
        /// </summary>
        [Description("延时")]
        Delay = 5,

        /// <summary>
        /// Json 解析。
        /// </summary>
        [Description("Json 解析")]
        JsonParse = 6,

        /// <summary>
        /// 条件判断。
        /// </summary>
        [Description("条件判断")]
        Condition = 7,

        /// <summary>
        /// 脚本。
        /// </summary>
        [Description("脚本")]
        Script = 8,

        /// <summary>
        /// 立讯报警日志。立讯工具从 100 排到 200。
        /// </summary>
        [Description("LUX报警")]
        Lux_AlarmLogTool = 100
    }
}
