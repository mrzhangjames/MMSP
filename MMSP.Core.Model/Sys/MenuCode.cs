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
    /// 菜单的最低角色。登录角色能否打开由 <see cref="UserRoleAccess.CanOpen"/> 决定。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MenuRoleAttribute : Attribute
    {
        /// <summary>
        /// 记录这个菜单要求的最低角色。
        /// </summary>
        /// <param name="minRole">最低角色。</param>
        public MenuRoleAttribute(UserRole minRole)
        {
            MinRole = minRole;
        }

        /// <summary>
        /// 最低角色。
        /// </summary>
        public UserRole MinRole { get; }
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
        [MenuRole(UserRole.Operator)]
        [MenuIcons("HouseFill", "House")]
        Home = 0,

        /// <summary>
        /// 流程。操作员和供应商不能打开。
        /// </summary>
        [Description("流程")]
        [MenuRole(UserRole.Engineer)]
        [MenuIcons("FlowArrowFill", "FlowArrow")]
        Flow = 1,

        /// <summary>
        /// 设备。操作员和供应商不能打开。
        /// </summary>
        [Description("设备")]
        [MenuRole(UserRole.Engineer)]
        [MenuIcons("MonitorFill", "Monitor")]
        Device = 2,

        /// <summary>
        /// 图表
        /// </summary>
        [Description("图表")]
        [MenuRole(UserRole.Operator)]
        [MenuIcons("ChartBarFill", "ChartBar")]
        Chart = 3,

        /// <summary>
        /// 配置。操作员和供应商不能打开。
        /// </summary>
        [Description("配置")]
        [MenuRole(UserRole.Engineer)]
        [MenuIcons("SlidersFill", "Sliders")]
        Config = 4,

        /// <summary>
        /// 用户。登录和注销都在这一页，四个角色都能打开。右侧管理内容另按角色显示。
        /// </summary>
        [Description("用户")]
        [MenuRole(UserRole.Operator)]
        [MenuIcons("UserFill", "User")]
        User = 5
    }
}
