using System.Collections.ObjectModel;
using System.Windows;
using MahApps.Metro.IconPacks;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.WpfBase.Core;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 用户页。未登录时在这里登录。登录后按角色显示用户信息和管理区。新增、编辑、删除写入系统库。
    /// </summary>
    public class UserViewModel : BaseViewModel
    {
        private static readonly UserRole[] RoleOrder =
        [
            UserRole.Admin,
            UserRole.Operator,
            UserRole.Engineer,
            UserRole.Supplier
        ];

        private bool _isEditorOpen;
        private Guid? _editingId;
        private string _editorTitle = "";
        private string _editorLoginName = "";
        private string _editorUserName = "";
        private string _editorPassword = "";
        private string _editorRole = "";
        private string _editorSessionTimeout = "0";
        private string _editorError = "";
        private bool _editorEnabled = true;
        private UserRole _editorRoleEnum = UserRole.Admin;
        private UserRow? _selectedUser;
        private string _draftLoginName = "";
        private string _draftPassword = "";
        private string _loginError = "";
        private LoginRoleOption? _selectedLoginRole;
        private bool _logoutPromptOpen;

        /// <summary>
        /// 当前登录会话。
        /// </summary>
        private readonly UserSession _session;

        /// <summary>
        /// 创建用户页。
        /// </summary>
        public UserViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator,
            UserSession session)
            : base(region, container, eventAggregator)
        {
            _session = session;
            Users = new ObservableCollection<UserRow>();
            LoginRoles = new ObservableCollection<LoginRoleOption>();
            Roles = RoleOrder.Select(RoleText).ToArray();
            EditorRole = Roles[0];
            ReloadLoginRoles();
            AddCommand = new DelegateCommand(OpenAdd);
            EditCommand = new DelegateCommand<UserRow?>(OpenEdit);
            DeleteCommand = new DelegateCommand(Delete);
            ConfirmCommand = new DelegateCommand(SaveEditor);
            CancelCommand = new DelegateCommand(CloseEditor);
            LoginCommand = new DelegateCommand(Login);
            LogoutCommand = new DelegateCommand(Logout);
            _session.Changed += OnSessionChanged;
            AppAppearance.LanguageChanged += OnLanguageChanged;
        }

        /// <summary>
        /// 表格行。来自系统库。
        /// </summary>
        public ObservableCollection<UserRow> Users { get; }

        /// <summary>
        /// 角色下拉项。
        /// </summary>
        public IReadOnlyList<string> Roles
        {
            get => _roles;
            private set => SetProperty(ref _roles, value);
        }

        /// <summary>
        /// 角色下拉项的当前文字。
        /// </summary>
        private IReadOnlyList<string> _roles = [];

        /// <summary>
        /// 登录区的角色列表。
        /// </summary>
        public ObservableCollection<LoginRoleOption> LoginRoles { get; }

        /// <summary>
        /// 登录时点中的角色。必须和账号角色一致。
        /// </summary>
        public LoginRoleOption? SelectedLoginRole
        {
            get => _selectedLoginRole;
            set => SetProperty(ref _selectedLoginRole, value);
        }

        /// <summary>
        /// 登录名输入。不写入数据库。
        /// </summary>
        public string DraftLoginName
        {
            get => _draftLoginName;
            set => SetProperty(ref _draftLoginName, value);
        }

        /// <summary>
        /// 密码输入。只在核对时使用，不写日志。
        /// </summary>
        public string DraftPassword
        {
            get => _draftPassword;
            set => SetProperty(ref _draftPassword, value);
        }

        /// <summary>
        /// 登录失败时留在登录区的提示。空字符串表示没有错误。
        /// </summary>
        public string LoginError
        {
            get => _loginError;
            private set => SetProperty(ref _loginError, value);
        }

        /// <summary>
        /// 是否已登录。
        /// </summary>
        public bool IsLoggedIn => _session.IsLoggedIn;

        /// <summary>
        /// 未登录时显示居中的登录区。
        /// </summary>
        public bool ShowLogin => !_session.IsLoggedIn;

        /// <summary>
        /// 已登录且右侧要显示管理区。管理员和工程师靠左，其余角色继续居中。
        /// </summary>
        public bool ShowSide =>
            _session.IsLoggedIn && _session.Role is UserRole.Admin or UserRole.Engineer;

        /// <summary>
        /// 已登录但没有右侧管理区。
        /// </summary>
        public bool ShowCenteredSession => _session.IsLoggedIn && !ShowSide;

        /// <summary>
        /// 右侧显示用户管理。只有管理员。
        /// </summary>
        public bool ShowUserAdmin => _session.IsLoggedIn && _session.Role == UserRole.Admin;

        /// <summary>
        /// 右侧显示配置和插件入口。只有工程师。
        /// </summary>
        public bool ShowEngineerModules => _session.IsLoggedIn && _session.Role == UserRole.Engineer;

        /// <summary>
        /// 当前登录名。
        /// </summary>
        public string SessionLoginName => _session.LoginName;

        /// <summary>
        /// 当前用户名。
        /// </summary>
        public string SessionUserName => _session.UserName;

        /// <summary>
        /// 当前角色文字。
        /// </summary>
        public string SessionRoleText => _session.IsLoggedIn ? RoleText(_session.Role) : "";

        /// <summary>
        /// 本次登录时间。
        /// </summary>
        public string SessionLoginTimeText =>
            _session.LoginTime is { } time ? time.ToString("yyyy-MM-dd HH:mm:ss") : "";

        /// <summary>
        /// 核对角色、登录名和密码。
        /// </summary>
        public DelegateCommand LoginCommand { get; }

        /// <summary>
        /// 确认后注销。
        /// </summary>
        public DelegateCommand LogoutCommand { get; }

        /// <summary>
        /// 打开空白的新增弹窗。
        /// </summary>
        public DelegateCommand AddCommand { get; }

        /// <summary>
        /// 打开编辑弹窗。参数是双击的那一行。
        /// </summary>
        public DelegateCommand<UserRow?> EditCommand { get; }

        /// <summary>
        /// 删除勾选的用户。
        /// </summary>
        public DelegateCommand DeleteCommand { get; }

        /// <summary>
        /// 弹窗保存。写入系统库并记一条信息日志。
        /// </summary>
        public DelegateCommand ConfirmCommand { get; }

        /// <summary>
        /// 关闭弹窗。
        /// </summary>
        public DelegateCommand CancelCommand { get; }

        /// <summary>
        /// 当前点中的行。编辑按钮在没有唯一勾选时用它。
        /// </summary>
        public UserRow? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        /// <summary>
        /// 编辑弹窗是否打开。
        /// </summary>
        public bool IsEditorOpen
        {
            get => _isEditorOpen;
            private set => SetProperty(ref _isEditorOpen, value);
        }

        /// <summary>
        /// 弹窗标题。新增用户或编辑用户。
        /// </summary>
        public string EditorTitle
        {
            get => _editorTitle;
            private set => SetProperty(ref _editorTitle, value);
        }

        /// <summary>
        /// 弹窗里的登录名。
        /// </summary>
        public string EditorLoginName
        {
            get => _editorLoginName;
            set => SetProperty(ref _editorLoginName, value);
        }

        /// <summary>
        /// 弹窗里的用户名。
        /// </summary>
        public string EditorUserName
        {
            get => _editorUserName;
            set => SetProperty(ref _editorUserName, value);
        }

        /// <summary>
        /// 弹窗里的密码。编辑时留空表示不改密码。
        /// </summary>
        public string EditorPassword
        {
            get => _editorPassword;
            set => SetProperty(ref _editorPassword, value);
        }

        /// <summary>
        /// 弹窗里的会话超时，分钟。0 表示不超时。
        /// </summary>
        public string EditorSessionTimeout
        {
            get => _editorSessionTimeout;
            set => SetProperty(ref _editorSessionTimeout, value);
        }

        /// <summary>
        /// 弹窗里选中的角色。
        /// </summary>
        public string EditorRole
        {
            get => _editorRole;
            set
            {
                if (!SetProperty(ref _editorRole, value))
                    return;

                foreach (var item in RoleOrder)
                {
                    if (RoleText(item) == value)
                    {
                        _editorRoleEnum = item;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 弹窗里的启用开关。
        /// </summary>
        public bool EditorEnabled
        {
            get => _editorEnabled;
            set => SetProperty(ref _editorEnabled, value);
        }

        /// <summary>
        /// 保存失败时的提示。
        /// </summary>
        public string EditorError
        {
            get => _editorError;
            private set => SetProperty(ref _editorError, value);
        }

        /// <summary>
        /// 进入页面时读取用户。
        /// </summary>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (ShowUserAdmin)
                LoadUsers();
        }

        /// <summary>
        /// 从系统库填充表格。密码不进入表格。
        /// </summary>
        private void LoadUsers()
        {
            Users.Clear();
            if (!SqlSugarContext.IsOpen)
                return;

            var rows = SqlSugarContext.SystemDb.Queryable<UserEntity>()
                .OrderBy(item => item.RoleLevel)
                .OrderBy(item => item.LoginName)
                .ToList();
            foreach (var row in rows)
                Users.Add(ToRow(row));
        }

        /// <summary>
        /// 转成表格行。
        /// </summary>
        private static UserRow ToRow(UserEntity entity)
        {
            return new UserRow
            {
                Id = entity.Id,
                LoginName = entity.LoginName ?? "",
                UserName = entity.UserName ?? "",
                Role = entity.RoleLevel,
                RoleName = RoleText(entity.RoleLevel),
                IsValid = entity.IsValid,
                SessionTimeout = entity.SessionTimeout,
                EnabledText = entity.IsValid ? Text("User_Yes") : Text("User_No"),
                CreateTimeText = entity.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        /// <summary>
        /// 打开新增弹窗，字段留空。
        /// </summary>
        private void OpenAdd()
        {
            _editingId = null;
            EditorTitle = Text("User_AddTitle");
            EditorLoginName = "";
            EditorUserName = "";
            EditorPassword = "";
            EditorSessionTimeout = "0";
            EditorRole = Roles[0];
            EditorEnabled = true;
            EditorError = "";
            IsEditorOpen = true;
        }

        /// <summary>
        /// 打开编辑弹窗并带回这一行。没有目标时不打开。
        /// </summary>
        private void OpenEdit(UserRow? row)
        {
            row ??= FindEditTarget();
            if (row is null)
                return;

            _editingId = row.Id;
            EditorTitle = Text("User_EditTitle");
            EditorLoginName = row.LoginName;
            EditorUserName = row.UserName;
            EditorPassword = "";
            EditorSessionTimeout = row.SessionTimeout.ToString(System.Globalization.CultureInfo.InvariantCulture);
            EditorRole = RoleText(row.Role);
            EditorEnabled = row.IsValid;
            EditorError = "";
            IsEditorOpen = true;
        }

        /// <summary>
        /// 只勾了一行就用那一行，否则用当前点中的行。
        /// </summary>
        private UserRow? FindEditTarget()
        {
            var checkedRows = Users.Where(item => item.IsChecked).ToList();
            if (checkedRows.Count == 1)
                return checkedRows[0];

            return SelectedUser;
        }

        /// <summary>
        /// 确认后删除勾选的用户。取消则不删除。
        /// </summary>
        private async void Delete()
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            var ids = Users.Where(item => item.IsChecked).Select(item => item.Id).ToList();
            if (ids.Count == 0 || !SqlSugarContext.IsOpen)
                return;

            var confirmed = await MetroDialogService.ConfirmAsync(Text("User_DeleteTitle"), Text("User_DeleteMessage"));
            if (!confirmed)
                return;

            SqlSugarContext.SystemDb.Deleteable<UserEntity>()
                .Where(item => ids.Contains(item.Id))
                .ExecuteCommand();
            LoadUsers();
        }

        /// <summary>
        /// 校验后写入系统库，成功再记日志并关闭弹窗。
        /// </summary>
        private void SaveEditor()
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            if (!TrySave(out var loginName, out var created))
                return;

            LogHelper.SysInfo(created ? $"新增用户 {loginName}" : $"编辑用户 {loginName}");
            CloseEditor();
            LoadUsers();
        }

        /// <summary>
        /// 新增或更新一条用户。失败时留下提示，不写日志。
        /// </summary>
        private bool TrySave(out string loginName, out bool created)
        {
            loginName = EditorLoginName.Trim();
            created = _editingId is null;
            var userName = EditorUserName.Trim();
            EditorError = "";

            if (loginName.Length == 0)
            {
                EditorError = Text("User_NeedLogin");
                return false;
            }

            if (userName.Length == 0)
            {
                EditorError = Text("User_NeedName");
                return false;
            }

            if (created && EditorPassword.Length == 0)
            {
                EditorError = Text("User_NeedPassword");
                return false;
            }

            if (!TryReadRole(out var role))
            {
                EditorError = Text("User_NeedRole");
                return false;
            }

            if (!UserSessionTimeout.TryParse(EditorSessionTimeout, out var timeoutMinutes))
            {
                EditorError = Text("User_NeedTimeout");
                return false;
            }

            if (!SqlSugarContext.IsOpen)
            {
                EditorError = Text("User_SaveFailed");
                return false;
            }

            try
            {
                if (LoginExists(_editingId, loginName))
                {
                    EditorError = Text("User_LoginExists");
                    return false;
                }

                var db = SqlSugarContext.SystemDb;
                if (_editingId is Guid id)
                {
                    var entity = db.Queryable<UserEntity>().Where(item => item.Id == id).ToList().FirstOrDefault();
                    if (entity is null)
                    {
                        EditorError = Text("User_SaveFailed");
                        return false;
                    }

                    entity.LoginName = loginName;
                    entity.UserName = userName;
                    entity.RoleLevel = role;
                    entity.IsValid = EditorEnabled;
                    entity.SessionTimeout = timeoutMinutes;
                    if (EditorPassword.Length > 0)
                        entity.Password = UserPassword.Hash(EditorPassword);
                    db.Updateable(entity).ExecuteCommand();
                }
                else
                {
                    db.Insertable(new UserEntity
                    {
                        Id = Guid.NewGuid(),
                        LoginName = loginName,
                        UserName = userName,
                        Password = UserPassword.Hash(EditorPassword),
                        RoleLevel = role,
                        IsValid = EditorEnabled,
                        SessionTimeout = timeoutMinutes,
                        CreateTime = DateTime.Now
                    }).ExecuteCommand();
                }

                return true;
            }
            catch (Exception)
            {
                EditorError = Text("User_SaveFailed");
                return false;
            }
        }

        /// <summary>
        /// 登录名忽略大小写，不能和别人重复。
        /// </summary>
        private static bool LoginExists(Guid? exceptId, string loginName)
        {
            var rows = SqlSugarContext.SystemDb.Queryable<UserEntity>()
                .Select(item => new { item.Id, item.LoginName })
                .ToList();
            return rows.Any(item =>
                item.Id != exceptId
                && string.Equals(item.LoginName?.Trim(), loginName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 把下拉文字转回角色。
        /// </summary>
        private bool TryReadRole(out UserRole role)
        {
            foreach (var item in RoleOrder)
            {
                if (RoleText(item) == EditorRole)
                {
                    role = item;
                    return true;
                }
            }

            role = UserRole.Admin;
            return false;
        }

        /// <summary>
        /// 角色行图标。和登录效果图一致。
        /// </summary>
        private static PackIconPhosphorIconsKind RoleIcon(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => PackIconPhosphorIconsKind.Shield,
                UserRole.Operator => PackIconPhosphorIconsKind.User,
                UserRole.Engineer => PackIconPhosphorIconsKind.Wrench,
                UserRole.Supplier => PackIconPhosphorIconsKind.Factory,
                _ => PackIconPhosphorIconsKind.User
            };
        }

        /// <summary>
        /// 按当前语言重写角色列表。点中的角色保持不变。
        /// </summary>
        private void ReloadLoginRoles()
        {
            var selected = _selectedLoginRole?.Role ?? UserRole.Admin;
            LoginRoles.Clear();
            foreach (var role in RoleOrder)
                LoginRoles.Add(new LoginRoleOption(role, RoleText(role), RoleIcon(role)));

            SelectedLoginRole = LoginRoles.First(item => item.Role == selected);
        }

        /// <summary>
        /// 语言切换后重写角色文字。
        /// </summary>
        private void OnLanguageChanged()
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null || dispatcher.CheckAccess())
            {
                ApplyLanguage();
                return;
            }

            dispatcher.BeginInvoke(new Action(ApplyLanguage));
        }

        /// <summary>
        /// 用当前语言替换角色下拉、登录列表和已登录角色名。
        /// </summary>
        private void ApplyLanguage()
        {
            Roles = RoleOrder.Select(RoleText).ToArray();
            EditorRole = RoleText(_editorRoleEnum);
            ReloadLoginRoles();
            RaiseSession();
        }

        /// <summary>
        /// 会话变化后切换登录区和用户信息。注销时关掉编辑窗。
        /// </summary>
        private void OnSessionChanged(object? sender, EventArgs e)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null || dispatcher.CheckAccess())
            {
                ApplySession();
                return;
            }

            dispatcher.BeginInvoke(new Action(ApplySession));
        }

        /// <summary>
        /// 把会话反映到界面。管理员重新读用户表。
        /// </summary>
        private void ApplySession()
        {
            if (!_session.IsLoggedIn)
                CloseEditor();

            RaiseSession();
            if (ShowUserAdmin)
                LoadUsers();
        }

        /// <summary>
        /// 通知登录状态和用户信息属性。
        /// </summary>
        private void RaiseSession()
        {
            RaisePropertyChanged(nameof(IsLoggedIn));
            RaisePropertyChanged(nameof(ShowLogin));
            RaisePropertyChanged(nameof(ShowSide));
            RaisePropertyChanged(nameof(ShowCenteredSession));
            RaisePropertyChanged(nameof(ShowUserAdmin));
            RaisePropertyChanged(nameof(ShowEngineerModules));
            RaisePropertyChanged(nameof(SessionLoginName));
            RaisePropertyChanged(nameof(SessionUserName));
            RaisePropertyChanged(nameof(SessionRoleText));
            RaisePropertyChanged(nameof(SessionLoginTimeText));
        }

        /// <summary>
        /// 角色、登录名、口令哈希和同一条账号一致才登录。不按明文比较，密码不写日志。
        /// </summary>
        private void Login()
        {
            if (_session.IsLoggedIn)
                return;

            var loginName = DraftLoginName.Trim();
            LoginError = "";
            if (SelectedLoginRole is null)
            {
                LoginError = Text("User_NeedRole");
                return;
            }

            if (loginName.Length == 0)
            {
                LoginError = Text("User_NeedLogin");
                return;
            }

            if (DraftPassword.Length == 0)
            {
                LoginError = Text("User_NeedPassword");
                return;
            }

            if (!SqlSugarContext.IsOpen)
            {
                LoginError = Text("Login_Failed");
                return;
            }

            try
            {
                var selectedRole = SelectedLoginRole.Role;
                var matches = SqlSugarContext.SystemDb.Queryable<UserEntity>()
                    .ToList()
                    .Where(item => string.Equals(item.LoginName?.Trim(), loginName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (matches.Count != 1
                    || !UserPassword.Verify(matches[0].Password, DraftPassword)
                    || matches[0].RoleLevel != selectedRole)
                {
                    LoginError = Text("Login_BadCredential");
                    return;
                }

                if (!matches[0].IsValid)
                {
                    LoginError = Text("Login_UserDisabled");
                    return;
                }

                var storedName = matches[0].LoginName?.Trim();
                if (!string.IsNullOrEmpty(storedName))
                    loginName = storedName;

                DraftPassword = "";
                LoginError = "";
                _session.SignIn(
                    loginName,
                    matches[0].UserName?.Trim() ?? "",
                    selectedRole,
                    DateTime.Now,
                    matches[0].SessionTimeout);
                LogHelper.SysInfo($"登录成功 {loginName} 角色={selectedRole}");
            }
            catch (Exception ex)
            {
                LoginError = Text("Login_Failed");
                LogHelper.SysError($"登录失败 {loginName}", ex);
            }
        }

        /// <summary>
        /// 确认后注销。取消则保持登录。不读流程，也不停流程。
        /// </summary>
        private async void Logout()
        {
            if (!_session.IsLoggedIn || _logoutPromptOpen)
                return;

            _logoutPromptOpen = true;
            try
            {
                var confirmed = await MetroDialogService.ConfirmAsync(Text("Logout_Title"), Text("Logout_Message"));
                if (!confirmed || !_session.IsLoggedIn)
                    return;

                var loginName = _session.LoginName;
                var role = _session.Role;
                _session.SignOut();
                DraftLoginName = "";
                DraftPassword = "";
                LoginError = "";
                LogHelper.SysInfo($"注销成功 {loginName} 角色={role}");
            }
            finally
            {
                _logoutPromptOpen = false;
            }
        }

        /// <summary>
        /// 关掉弹窗并清空输入。
        /// </summary>
        private void CloseEditor()
        {
            IsEditorOpen = false;
            _editingId = null;
            EditorLoginName = "";
            EditorUserName = "";
            EditorPassword = "";
            EditorSessionTimeout = "0";
            EditorError = "";
        }

        /// <summary>
        /// 角色在界面上的文字。
        /// </summary>
        private static string RoleText(UserRole role)
        {
            var key = role switch
            {
                UserRole.Admin => "User_RoleAdmin",
                UserRole.Operator => "User_RoleOperator",
                UserRole.Engineer => "User_RoleEngineer",
                UserRole.Supplier => "User_RoleSupplier",
                _ => "User_RoleAdmin"
            };
            return Text(key);
        }

        /// <summary>
        /// 读取语言资源。找不到时退回键名。
        /// </summary>
        private static string Text(string key)
        {
            if (Application.Current?.TryFindResource(key) is string value && value.Length > 0)
                return value;

            return key;
        }
    }
}
