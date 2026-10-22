using System.ComponentModel;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// PLC 点位类型。数值与客户库 point_kind 列一致。心跳和状态各只允许一条。
    /// </summary>
    public enum PlcPointKind
    {
        /// <summary>
        /// 流程。
        /// </summary>
        [Description("流程")]
        Flow = 1,

        /// <summary>
        /// 监控。
        /// </summary>
        [Description("监控")]
        Monitor = 2,

        /// <summary>
        /// 心跳。整表只能有一条。
        /// </summary>
        [Description("心跳")]
        Heartbeat = 3,

        /// <summary>
        /// 状态。整表只能有一条，读数对应 PlcStatusValue。
        /// </summary>
        [Description("状态")]
        Status = 4
    }
}
