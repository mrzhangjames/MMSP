using MMSP.Core.Model.Sys;

namespace MMSP.Common.Helper.Logger
{
    /// <summary>
    /// 一条已经写好的运行日志。主页用它显示，不直接读库表。
    /// </summary>
    public sealed class RunLogMessage
    {
        /// <summary>
        /// 与客户库主键相同。主页用它去掉重复行。
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// 日志时间。
        /// </summary>
        public DateTime LogTime { get; init; }

        /// <summary>
        /// 级别。
        /// </summary>
        public RunLogLevel Level { get; init; }

        /// <summary>
        /// 级别文字：信息、报警、异常。
        /// </summary>
        public string LevelText { get; init; } = "";

        /// <summary>
        /// 业务类型。
        /// </summary>
        public RunLogBiz BizType { get; init; }

        /// <summary>
        /// 界面上的业务类型：Sys、Plc、Scan、Flow、Mes。
        /// </summary>
        public string BizText => BizType.ToString();

        /// <summary>
        /// 内容。
        /// </summary>
        public string Message { get; init; } = "";

        /// <summary>
        /// 界面上的时间文字。
        /// </summary>
        public string TimeText => LogTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
