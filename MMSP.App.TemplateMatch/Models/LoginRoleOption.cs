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
        public LoginRoleOption(UserRole role, string text)
        {
            Role = role;
            Text = text;
        }

        /// <summary>
        /// 角色。
        /// </summary>
        public UserRole Role { get; }

        /// <summary>
        /// 界面文字。
        /// </summary>
        public string Text { get; }
    }
}
