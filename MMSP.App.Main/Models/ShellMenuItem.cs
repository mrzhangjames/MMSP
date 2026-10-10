using MahApps.Metro.IconPacks;

namespace MMSP.App.Main.Models
{
    /// <summary>
    /// 主壳导航的一项。文字和图标来自系统库。
    /// </summary>
    public sealed class ShellMenuItem
    {
        /// <summary>
        /// 创建一项导航。
        /// </summary>
        /// <param name="title">导航上显示的文字。</param>
        /// <param name="selectedIcon">选中时的实心图标。</param>
        /// <param name="unselectedIcon">未选中时的线框图标。</param>
        public ShellMenuItem(
            string title,
            PackIconPhosphorIconsKind selectedIcon,
            PackIconPhosphorIconsKind unselectedIcon)
        {
            Title = title;
            SelectedIcon = selectedIcon;
            UnselectedIcon = unselectedIcon;
        }

        /// <summary>
        /// 导航上显示的文字。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 选中图标。
        /// </summary>
        public PackIconPhosphorIconsKind SelectedIcon { get; }

        /// <summary>
        /// 未选中图标。
        /// </summary>
        public PackIconPhosphorIconsKind UnselectedIcon { get; }
    }
}
