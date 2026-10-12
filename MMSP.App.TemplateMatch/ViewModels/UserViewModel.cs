using System.Collections.ObjectModel;
using System.Windows;
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
    /// 用户页。新增、编辑、删除写入系统库。弹窗保存后再写运行日志。
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
        private string _editorError = "";
        private bool _editorEnabled = true;
        private UserRow? _selectedUser;

        /// <summary>
        /// 创建用户页。
        /// </summary>
        public UserViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator)
            : base(region, container, eventAggregator)
        {
            Users = new ObservableCollection<UserRow>();
            Roles = RoleOrder.Select(RoleText).ToArray();
            EditorRole = Roles[0];
            AddCommand = new DelegateCommand(OpenAdd);
            EditCommand = new DelegateCommand<UserRow?>(OpenEdit);
            DeleteCommand = new DelegateCommand(Delete);
            ConfirmCommand = new DelegateCommand(SaveEditor);
            CancelCommand = new DelegateCommand(CloseEditor);
        }

        /// <summary>
        /// 表格行。来自系统库。
        /// </summary>
        public ObservableCollection<UserRow> Users { get; }

        /// <summary>
        /// 角色下拉项。
        /// </summary>
        public IReadOnlyList<string> Roles { get; }

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
        /// 弹窗里选中的角色。
        /// </summary>
        public string EditorRole
        {
            get => _editorRole;
            set => SetProperty(ref _editorRole, value);
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
                    if (EditorPassword.Length > 0)
                        entity.Password = EditorPassword;
                    db.Updateable(entity).ExecuteCommand();
                }
                else
                {
                    db.Insertable(new UserEntity
                    {
                        Id = Guid.NewGuid(),
                        LoginName = loginName,
                        UserName = userName,
                        Password = EditorPassword,
                        RoleLevel = role,
                        IsValid = EditorEnabled,
                        SessionTimeout = 0,
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
        /// 关掉弹窗并清空输入。
        /// </summary>
        private void CloseEditor()
        {
            IsEditorOpen = false;
            _editingId = null;
            EditorLoginName = "";
            EditorUserName = "";
            EditorPassword = "";
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
