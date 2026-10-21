using MMSP.Core.Model.Sys;

namespace MMSP.App.WpfBase.Services
{
    /// <summary>
    /// 当前登录会话。用户页写入，主壳用来刷新底栏和菜单。
    /// </summary>
    public sealed class UserSession : BindableBase
    {
        private bool _isLoggedIn;
        private string _loginName = "";
        private string _userName = "";
        private UserRole _role;
        private DateTime? _loginTime;

        /// <summary>
        /// 登录或注销之后通知。主壳据此重算菜单。
        /// </summary>
        public event EventHandler? Changed;

        /// <summary>
        /// 是否已登录。
        /// </summary>
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            private set => SetProperty(ref _isLoggedIn, value);
        }

        /// <summary>
        /// 登录名。未登录时为空。
        /// </summary>
        public string LoginName
        {
            get => _loginName;
            private set => SetProperty(ref _loginName, value);
        }

        /// <summary>
        /// 用户名。未登录时为空。
        /// </summary>
        public string UserName
        {
            get => _userName;
            private set => SetProperty(ref _userName, value);
        }

        /// <summary>
        /// 当前角色。未登录时无意义。
        /// </summary>
        public UserRole Role
        {
            get => _role;
            private set => SetProperty(ref _role, value);
        }

        /// <summary>
        /// 本次登录时间。未登录时为空。
        /// </summary>
        public DateTime? LoginTime
        {
            get => _loginTime;
            private set => SetProperty(ref _loginTime, value);
        }

        /// <summary>
        /// 记下一次成功登录。密码不进会话。
        /// </summary>
        public void SignIn(string loginName, string userName, UserRole role, DateTime loginTime)
        {
            LoginName = loginName;
            UserName = userName;
            Role = role;
            LoginTime = loginTime;
            IsLoggedIn = true;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 清掉本次登录。
        /// </summary>
        public void SignOut()
        {
            IsLoggedIn = false;
            LoginName = "";
            UserName = "";
            Role = UserRole.Admin;
            LoginTime = null;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
