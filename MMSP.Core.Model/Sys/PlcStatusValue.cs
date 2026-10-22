using System.ComponentModel;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 唯一状态点的读数。数值与 PLC 整字一致。
    /// </summary>
    public enum PlcStatusValue
    {
        /// <summary>
        /// 运行。
        /// </summary>
        [Description("运行")]
        Run = 0,

        /// <summary>
        /// 暂停。
        /// </summary>
        [Description("暂停")]
        Pause = 1,

        /// <summary>
        /// 急停。
        /// </summary>
        [Description("急停")]
        EStop = 2,

        /// <summary>
        /// 调试。
        /// </summary>
        [Description("调试")]
        Debug = 3,

        /// <summary>
        /// 手动。
        /// </summary>
        [Description("手动")]
        Manual = 4
    }
}
