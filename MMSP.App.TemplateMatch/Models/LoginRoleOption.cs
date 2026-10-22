using MahApps.Metro.IconPacks;
using MMSP.Core.Model.Sys;

namespace MMSP.App.TemplateMatch.Models
{
    /// <summary>
    /// 登录区角色列表的一项。
    /// </summary>
    public sealed class LoginRoleOption
    {
        /// <summary>
        /// 创建一项角色。
        /// </summary>
        /// <param name="role">角色。</param>
        /// <param name="text">界面文字。</param>
        /// <param name="icon">行首图标。</param>
        public LoginRoleOption(UserRole role, string text, PackIconPhosphorIconsKind icon)
        {
            Role = role;
            Text = text;
            Icon = icon;
        }

        /// <summary>
        /// 角色。
        /// </summary>
        public UserRole Role { get; }

        /// <summary>
        /// 界面文字。
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 行首图标。管理员盾牌，操作员人，工程师扳手，供应商厂房。
        /// </summary>
        public PackIconPhosphorIconsKind Icon { get; }
    }
}
