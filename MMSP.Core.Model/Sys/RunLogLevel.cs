using System.ComponentModel;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 运行日志级别。数值与客户库 level 列一致。
    /// </summary>
    public enum RunLogLevel
    {
        /// <summary>
        /// 信息
        /// </summary>
        [Description("信息")]
        Info = 0,

        /// <summary>
        /// 报警
        /// </summary>
        [Description("报警")]
        Alarm = 1,

        /// <summary>
        /// 异常
        /// </summary>
        [Description("异常")]
        Error = 2
    }
}
