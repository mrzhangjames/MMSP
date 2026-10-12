using MMSP.Core.Model.Sys;

namespace MMSP.App.TemplateMatch.Models
{
    /// <summary>
    /// 用户表格的一行。不含密码。
    /// </summary>
    public sealed class UserRow : BindableBase
    {
        private bool _isChecked;
        private string _loginName = "";
        private string _userName = "";
        private string _roleName = "";
        private string _enabledText = "";
        private string _createTimeText = "";

        /// <summary>
        /// 系统库主键。
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// 角色。
        /// </summary>
        public UserRole Role { get; init; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// 是否勾选。勾选行用金色底，删除用这些行。
        /// </summary>
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        /// <summary>
        /// 登录名。
        /// </summary>
        public string LoginName
        {
            get => _loginName;
            set => SetProperty(ref _loginName, value);
        }

        /// <summary>
        /// 用户名。
        /// </summary>
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        /// <summary>
        /// 角色文字。
        /// </summary>
        public string RoleName
        {
            get => _roleName;
            set => SetProperty(ref _roleName, value);
        }

        /// <summary>
        /// 启用文字。是或否。
        /// </summary>
        public string EnabledText
        {
            get => _enabledText;
            set => SetProperty(ref _enabledText, value);
        }

        /// <summary>
        /// 创建时间文字。
        /// </summary>
        public string CreateTimeText
        {
            get => _createTimeText;
            set => SetProperty(ref _createTimeText, value);
        }
    }
}
