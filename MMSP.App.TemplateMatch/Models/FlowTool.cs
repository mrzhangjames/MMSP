using MahApps.Metro.IconPacks;
using MMSP.Core.Model.Sys;

namespace MMSP.App.TemplateMatch.Models
{
    /// <summary>
    /// 一个已加载工具。由工具种类生成，不入库。
    /// </summary>
    public sealed class FlowTool
    {
        /// <summary>
        /// 工具种类。拖到流程步上时写入这一步。
        /// </summary>
        public FlowToolKind ToolKind { get; init; }

        /// <summary>
        /// 工具名称。
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// 工具图标。
        /// </summary>
        public PackIconPhosphorIconsKind Icon { get; init; }
    }
}
