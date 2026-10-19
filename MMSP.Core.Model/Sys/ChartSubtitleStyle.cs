using System.ComponentModel;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 图表卡片副标题样式。数值与客户库 subtitle_style 列一致。
    /// </summary>
    public enum ChartSubtitleStyle
    {
        /// <summary>
        /// 默认。
        /// </summary>
        [Description("默认")]
        Default = 0,

        /// <summary>
        /// 上升。
        /// </summary>
        [Description("上升")]
        Up = 1,

        /// <summary>
        /// 下降。
        /// </summary>
        [Description("下降")]
        Down = 2
    }
}
