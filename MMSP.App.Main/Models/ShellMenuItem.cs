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
        /// <param name="viewName">内容区要打开的页面名。</param>
        public ShellMenuItem(
            string title,
            PackIconPhosphorIconsKind selectedIcon,
            PackIconPhosphorIconsKind unselectedIcon,
            string viewName)
        {
            Title = title;
            SelectedIcon = selectedIcon;
            UnselectedIcon = unselectedIcon;
            ViewName = viewName;
        }

        /// <summary>
        /// 导航上显示的文字。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 内容区页面名，对应已登记的视图。
        /// </summary>
        public string ViewName { get; }

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
