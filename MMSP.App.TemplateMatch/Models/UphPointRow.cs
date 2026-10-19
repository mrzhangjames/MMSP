namespace MMSP.App.TemplateMatch.Models
{
    /// <summary>
    /// 图表数据表的一行。来自客户库 biz_uph。
    /// </summary>
    public sealed class UphPointRow
    {
        /// <summary>
        /// 小时起点。
        /// </summary>
        public DateTime HourTime { get; init; }

        /// <summary>
        /// 时间文字。显示为 HH:mm。
        /// </summary>
        public string TimeText { get; init; } = "";

        /// <summary>
        /// 小时产量。图用这个数。
        /// </summary>
        public double HourOutput { get; init; }

        /// <summary>
        /// 小时产量文字。
        /// </summary>
        public string HourOutputText { get; init; } = "";

        /// <summary>
        /// 累计产量文字。
        /// </summary>
        public string TotalOutputText { get; init; } = "";

        /// <summary>
        /// 良品数文字。
        /// </summary>
        public string GoodCountText { get; init; } = "";

        /// <summary>
        /// 不良数文字。
        /// </summary>
        public string DefectCountText { get; init; } = "";

        /// <summary>
        /// 良品率文字。由良品数和不良数算出。
        /// </summary>
        public string YieldText { get; init; } = "";
    }
}
