namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 运行日志的业务类型。数值与客户库 biz_type 列一致。
    /// </summary>
    public enum RunLogBiz
    {
        /// <summary>
        /// 系统。
        /// </summary>
        Sys = 0,

        /// <summary>
        /// PLC。
        /// </summary>
        Plc = 1,

        /// <summary>
        /// 扫码。
        /// </summary>
        Scan = 2,

        /// <summary>
        /// 流程。
        /// </summary>
        Flow = 3,

        /// <summary>
        /// MES。
        /// </summary>
        Mes = 4
    }
}
