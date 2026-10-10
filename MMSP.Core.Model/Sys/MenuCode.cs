using System.ComponentModel;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 菜单图标。选中和未选中各用一个 Phosphor 图标名。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MenuIconsAttribute : Attribute
    {
        /// <summary>
        /// 记录一对图标名。
        /// </summary>
        /// <param name="selectedIcon">选中时的实心图标。</param>
        /// <param name="unselectedIcon">未选中时的线框图标。</param>
        public MenuIconsAttribute(string selectedIcon, string unselectedIcon)
        {
            SelectedIcon = selectedIcon;
            UnselectedIcon = unselectedIcon;
        }

        /// <summary>
        /// 选中图标名。
        /// </summary>
        public string SelectedIcon { get; }

        /// <summary>
        /// 未选中图标名。
        /// </summary>
        public string UnselectedIcon { get; }
    }

    /// <summary>
    /// 主界面菜单。顺序与导航从左到右一致。
    /// </summary>
    public enum MenuCode : uint
    {
        /// <summary>
        /// 主页
        /// </summary>
        [Description("主页")]
        [MenuIcons("HouseFill", "House")]
        Home = 0,

        /// <summary>
        /// 流程
        /// </summary>
        [Description("流程")]
        [MenuIcons("FlowArrowFill", "FlowArrow")]
        Flow = 1,

        /// <summary>
        /// 设备
        /// </summary>
        [Description("设备")]
        [MenuIcons("MonitorFill", "Monitor")]
        Device = 2,

        /// <summary>
        /// 图表
        /// </summary>
        [Description("图表")]
        [MenuIcons("ChartBarFill", "ChartBar")]
        Chart = 3,

        /// <summary>
        /// 配置
        /// </summary>
        [Description("配置")]
        [MenuIcons("SlidersFill", "Sliders")]
        Config = 4,

        /// <summary>
        /// 用户
        /// </summary>
        [Description("用户")]
        [MenuIcons("UserFill", "User")]
        User = 5
    }
}
