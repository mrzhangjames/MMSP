using System.Windows.Media;
using MMSP.Core.Model.Sys;

namespace MMSP.App.TemplateMatch.Models
{
    /// <summary>
    /// 左侧一张图表卡片。数据来自客户库 biz_charts。
    /// </summary>
    public sealed class ChartCard
    {
        /// <summary>
        /// 主键。
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// 名称。
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// 来源类型。
        /// </summary>
        public ChartSourceKind SourceKind { get; init; }

        /// <summary>
        /// 来源键。
        /// </summary>
        public string Source { get; init; } = "";

        /// <summary>
        /// 卡片上的显示值。
        /// </summary>
        public string DisplayText { get; init; } = "";

        /// <summary>
        /// 副标题。
        /// </summary>
        public string Subtitle { get; init; } = "";

        /// <summary>
        /// 副标题样式。
        /// </summary>
        public ChartSubtitleStyle SubtitleStyle { get; init; }

        /// <summary>
        /// 副标题颜色。默认灰，上升绿，下降红。
        /// </summary>
        public Brush SubtitleBrush { get; init; } = Brushes.Gray;

        /// <summary>
        /// 排序。
        /// </summary>
        public int SortIndex { get; init; }
    }
}
