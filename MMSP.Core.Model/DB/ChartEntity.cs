using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 图表定义。写在客户库。
    /// </summary>
    [SugarTable("biz_charts")]
    public class ChartEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 名称。最长 16。
        /// </summary>
        [SugarColumn(ColumnName = "name", Length = 16)]
        public string Name { get; set; } = "";

        /// <summary>
        /// 来源类型。UPH 或 MES。
        /// </summary>
        [SugarColumn(ColumnName = "source_kind")]
        public ChartSourceKind SourceKind { get; set; }

        /// <summary>
        /// 来源键。最长 64，原样保存。
        /// </summary>
        [SugarColumn(ColumnName = "source", Length = 64)]
        public string Source { get; set; } = "";

        /// <summary>
        /// 显示值。按卡片文本保存，最长 12。
        /// </summary>
        [SugarColumn(ColumnName = "display_text", Length = 12)]
        public string DisplayText { get; set; } = "";

        /// <summary>
        /// 副标题。最长 24。
        /// </summary>
        [SugarColumn(ColumnName = "subtitle", Length = 24)]
        public string Subtitle { get; set; } = "";

        /// <summary>
        /// 副标题样式。
        /// </summary>
        [SugarColumn(ColumnName = "subtitle_style")]
        public ChartSubtitleStyle SubtitleStyle { get; set; }

        /// <summary>
        /// 左侧卡片从上到下的顺序。
        /// </summary>
        [SugarColumn(ColumnName = "sort_index")]
        public int SortIndex { get; set; }
    }
}
