using MahApps.Metro.IconPacks;
using Prism.Mvvm;

namespace MMSP.App.Main.Models
{
    /// <summary>
    /// 主壳导航的一项。文字和图标来自系统库。
    /// </summary>
    public sealed class ShellMenuItem : BindableBase
    {
        /// <summary>
        /// 导航上显示的文字。
        /// </summary>
        private string _title;

        /// <summary>
        /// 创建一项导航。
        /// </summary>
        /// <param name="title">导航上显示的文字。</param>
        /// <param name="titleKey">语言资源键。切换语言时用它重读文字。</param>
        /// <param name="selectedIcon">选中时的实心图标。</param>
        /// <param name="unselectedIcon">未选中时的线框图标。</param>
        /// <param name="viewName">内容区要打开的页面名。</param>
        public ShellMenuItem(
            string title,
            string titleKey,
            PackIconPhosphorIconsKind selectedIcon,
            PackIconPhosphorIconsKind unselectedIcon,
            string viewName)
        {
            _title = title;
            TitleKey = titleKey;
            SelectedIcon = selectedIcon;
            UnselectedIcon = unselectedIcon;
            ViewName = viewName;
        }

        /// <summary>
        /// 导航上显示的文字。
        /// </summary>
        public string Title
        {
            get => _title;
            private set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// 语言资源键。
        /// </summary>
        public string TitleKey { get; }

        /// <summary>
        /// 换成另一种语言的导航文字。
        /// </summary>
        public void UseTitle(string title) => Title = title;

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
