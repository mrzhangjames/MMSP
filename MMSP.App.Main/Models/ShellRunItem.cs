using MahApps.Metro.IconPacks;
using MMSP.Core.Model.Sys;
using Prism.Mvvm;

namespace MMSP.App.Main.Models
{
    /// <summary>
    /// 导航右侧的一项运行操作。按钮上不写文字，标题只出现在提示里。
    /// </summary>
    public sealed class ShellRunItem : BindableBase
    {
        /// <summary>
        /// 提示文字。
        /// </summary>
        private string _title;

        /// <summary>
        /// 创建一项运行操作。
        /// </summary>
        /// <param name="state">对应的运行状态。</param>
        /// <param name="title">提示文字，例如启动。</param>
        /// <param name="icon">按钮中央的图标。</param>
        public ShellRunItem(RunState state, string title, PackIconPhosphorIconsKind icon)
        {
            State = state;
            _title = title;
            Icon = icon;
        }

        /// <summary>
        /// 运行状态。
        /// </summary>
        public RunState State { get; }

        /// <summary>
        /// 提示文字。
        /// </summary>
        public string Title
        {
            get => _title;
            private set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// 换成另一种语言的提示文字。
        /// </summary>
        public void UseTitle(string title) => Title = title;

        /// <summary>
        /// 图标。
        /// </summary>
        public PackIconPhosphorIconsKind Icon { get; }
    }
}
