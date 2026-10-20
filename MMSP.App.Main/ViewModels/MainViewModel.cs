using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using MahApps.Metro.IconPacks;
using MMSP.App.Main.Models;
using MMSP.App.WpfBase.Core;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper;
using MMSP.Common.Helper.Logger;
using System.ComponentModel;
using System.Reflection;
using MMSP.Core.License;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;
using MMSP.Service.Flow;

namespace MMSP.App.Main.ViewModels
{
    /// <summary>
    /// 主壳。点击菜单切换内容区页面，底栏保持占位。
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        /// <summary>
        /// 底栏时间格式。
        /// </summary>
        private const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// 底栏到期日格式。
        /// </summary>
        private const string ExpiryDateFormat = "yyyy-MM-dd";

        /// <summary>
        /// 内容区区域名，与 MainView 的 RegionName 相同。
        /// </summary>
        private const string ContentRegionName = "MainViewContentRegion";

        /// <summary>
        /// 每秒刷新底栏时间。定时器在界面线程上跑。
        /// </summary>
        private readonly DispatcherTimer _clockTimer;

        /// <summary>
        /// 当前选中的菜单。
        /// </summary>
        private ShellMenuItem? _selectedMenu;

        /// <summary>
        /// 当前选中的运行按钮。
        /// </summary>
        private ShellRunItem? _selectedRun;

        /// <summary>
        /// 流程执行服务。启动和停止按钮调用它。
        /// </summary>
        private readonly IFlowRunService _flowRun;

        /// <summary>
        /// 正在按服务状态回写运行按钮，避免再次启动或停止。
        /// </summary>
        private bool _applyingRun;

        /// <summary>
        /// 底栏运行状态。在跑是启动，停稳是停止。
        /// </summary>
        private string _runStatusText = "";

        /// <summary>
        /// 底栏时间文字。
        /// </summary>
        private string _currentTimeText = "";

        /// <summary>
        /// 底栏许可证状态。
        /// </summary>
        private string _licenseStatusText = "";

        /// <summary>
        /// 当前许可证。导入后通过事件更新底栏。
        /// </summary>
        private readonly LicenseStatusStore _licenses;

        /// <summary>
        /// 界面上是否已登录。
        /// </summary>
        private bool _isLoggedIn;

        /// <summary>
        /// 注销确认框正在显示，避免重复弹出。
        /// </summary>
        private bool _logoutPromptOpen;

        /// <summary>
        /// 当前登录角色。未登录时为空，导航只留主页。
        /// </summary>
        private UserRole? _currentRole;

        /// <summary>
        /// 登录窗是否打开。
        /// </summary>
        private bool _isLoginOpen;

        /// <summary>
        /// 底栏登录名。未登录时是「未登录」。
        /// </summary>
        private string _loginName = "";

        /// <summary>
        /// 登录窗里的登录名。不写入数据库。
        /// </summary>
        private string _draftLoginName = "";

        /// <summary>
        /// 登录窗里的密码。只在核对时使用，不写日志。
        /// </summary>
        private string _draftPassword = "";

        /// <summary>
        /// 登录失败时留在窗上的提示。空字符串表示没有错误。
        /// </summary>
        private string _loginError = "";

        /// <summary>
        /// 创建主壳数据。未登录时只打开主页。
        /// </summary>
        public MainViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator,
            IFlowRunService flowRun,
            LicenseStatusStore licenses)
            : base(region, container, eventAggregator)
        {
            _flowRun = flowRun;
            _licenses = licenses;
            _licenses.Changed += OnLicenseChanged;
            _flowRun.RunningChanged += OnFlowRunningChanged;
            Menus = new ObservableCollection<ShellMenuItem>();
            LoadMenus();
            RunCommands = new ObservableCollection<ShellRunItem>();
            CreateRunCommands();
            RunStatusText = RunTitle(RunState.Stop);
            VersionText = AppVersionHelper.GetVersionText();
            RefreshLicenseStatus();
            LoginName = ReadText("Footer_NotLoggedIn", "未登录");
            SessionCommand = new DelegateCommand(OnSession);
            ConfirmLoginCommand = new DelegateCommand(ConfirmLogin);
            CancelLoginCommand = new DelegateCommand(CloseLogin);
            CurrentTimeText = DateTime.Now.ToString(TimeFormat);
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += OnClockTick;
            _clockTimer.Start();
            AppAppearance.LanguageChanged += OnLanguageChanged;
        }

        /// <summary>
        /// 系统库中启用的菜单，按排序号排列。
        /// </summary>
        public ObservableCollection<ShellMenuItem> Menus { get; }

        /// <summary>
        /// 导航中间的启动、停止、刷新。不放进 <see cref="Menus"/>。
        /// </summary>
        public ObservableCollection<ShellRunItem> RunCommands { get; }

        /// <summary>
        /// 当前点中的运行按钮。底栏文字不跟这个选中项走。
        /// </summary>
        public ShellRunItem? SelectedRun
        {
            get => _selectedRun;
            set
            {
                if (_applyingRun)
                {
                    SetProperty(ref _selectedRun, value);
                    return;
                }

                if (value is null || ReferenceEquals(_selectedRun, value))
                {
                    RaisePropertyChanged(nameof(SelectedRun));
                    return;
                }

                if (value.State == RunState.Start && !_flowRun.TryStart())
                {
                    RaisePropertyChanged(nameof(SelectedRun));
                    return;
                }

                if (value.State == RunState.Stop)
                    _flowRun.Stop();

                SetProperty(ref _selectedRun, value);
            }
        }

        /// <summary>
        /// 底栏运行状态。在跑显示启动，停稳显示停止。
        /// </summary>
        public string RunStatusText
        {
            get => _runStatusText;
            private set => SetProperty(ref _runStatusText, value);
        }

        /// <summary>
        /// 当前选中的菜单。不允许空选，避免内容标题被清掉。
        /// </summary>
        public ShellMenuItem? SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                if (value is null)
                {
                    RaisePropertyChanged(nameof(SelectedMenu));
                    return;
                }

                if (SetProperty(ref _selectedMenu, value))
                {
                    RaisePropertyChanged(nameof(ContentTitle));
                    OpenSelectedPage();
                }
            }
        }

        /// <summary>
        /// 内容区标题，跟随 <see cref="SelectedMenu"/>。
        /// </summary>
        public string ContentTitle => SelectedMenu?.Title ?? "";

        /// <summary>
        /// 底栏版本，构造后不再变化。
        /// </summary>
        public string VersionText { get; }

        /// <summary>
        /// 底栏许可证状态，放在版本号后面。
        /// </summary>
        public string LicenseStatusText
        {
            get => _licenseStatusText;
            private set => SetProperty(ref _licenseStatusText, value);
        }

        /// <summary>
        /// 底栏登录名。未登录显示「未登录」，登录后显示窗里填写的名字。
        /// </summary>
        public string LoginName
        {
            get => _loginName;
            private set => SetProperty(ref _loginName, value);
        }

        /// <summary>
        /// 是否已在界面上登录。
        /// </summary>
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            private set
            {
                if (SetProperty(ref _isLoggedIn, value))
                {
                    RaisePropertyChanged(nameof(SessionIcon));
                    RaisePropertyChanged(nameof(SessionToolTip));
                }
            }
        }

        /// <summary>
        /// 登录窗是否打开。
        /// </summary>
        public bool IsLoginOpen
        {
            get => _isLoginOpen;
            private set => SetProperty(ref _isLoginOpen, value);
        }

        /// <summary>
        /// 登录窗中的登录名。
        /// </summary>
        public string DraftLoginName
        {
            get => _draftLoginName;
            set => SetProperty(ref _draftLoginName, value);
        }

        /// <summary>
        /// 登录窗中的密码。核对用户表，不写进日志。
        /// </summary>
        public string DraftPassword
        {
            get => _draftPassword;
            set => SetProperty(ref _draftPassword, value);
        }

        /// <summary>
        /// 登录窗上的错误提示。账号、密码或库异常都写在这里。
        /// </summary>
        public string LoginError
        {
            get => _loginError;
            private set => SetProperty(ref _loginError, value);
        }

        /// <summary>
        /// 右侧按钮图标。未登录是进入，已登录是离开。
        /// </summary>
        public PackIconPhosphorIconsKind SessionIcon =>
            IsLoggedIn ? PackIconPhosphorIconsKind.SignOut : PackIconPhosphorIconsKind.SignIn;

        /// <summary>
        /// 右侧按钮提示。未登录是登录，已登录是注销。
        /// </summary>
        public string SessionToolTip =>
            IsLoggedIn ? ReadText("Shell_Logout", "注销") : ReadText("Shell_Login", "登录");

        /// <summary>
        /// 打开登录窗，或在已登录时回到未登录。
        /// </summary>
        public DelegateCommand SessionCommand { get; }

        /// <summary>
        /// 核对账号密码。正确才关闭登录窗并切换成注销外观。
        /// </summary>
        public DelegateCommand ConfirmLoginCommand { get; }

        /// <summary>
        /// 关闭登录窗，不改变登录外观。
        /// </summary>
        public DelegateCommand CancelLoginCommand { get; }

        /// <summary>
        /// 底栏时间，每秒更新。
        /// </summary>
        public string CurrentTimeText
        {
            get => _currentTimeText;
            private set => SetProperty(ref _currentTimeText, value);
        }

        /// <summary>
        /// 按当前角色重算导航。主页每个角色都保留。未登录只留主页。
        /// </summary>
        private void LoadMenus()
        {
            var currentView = _selectedMenu?.ViewName;
            Menus.Clear();

            var rows = SqlSugarContext.SystemDb.Queryable<MenuEntity>()
                .Where(item => item.IsValid)
                .OrderBy(item => item.SortIndex)
                .ToList();

            foreach (var row in rows)
            {
                if (_currentRole is UserRole role)
                {
                    if (row.Code != MenuCode.Home && !UserRoleAccess.CanOpen(role, row.MinRole))
                        continue;
                }
                else if (row.Code != MenuCode.Home)
                {
                    continue;
                }

                var item = CreateMenuItem(row);
                if (item is null)
                    continue;

                Menus.Add(item);
            }

            var keep = Menus.FirstOrDefault(item => item.ViewName == currentView);
            if (keep is not null)
            {
                SelectedMenu = keep;
                return;
            }

            if (Menus.Count > 0)
            {
                SelectedMenu = Menus[0];
                return;
            }

            ClearContent();
        }

        /// <summary>
        /// 把一条菜单变成导航项。图标名对不上时返回空。
        /// </summary>
        private static ShellMenuItem? CreateMenuItem(MenuEntity row)
        {
            if (!Enum.TryParse(row.SelectedIcon, out PackIconPhosphorIconsKind selected))
                return null;
            if (!Enum.TryParse(row.UnSelectedIcon, out PackIconPhosphorIconsKind unselected))
                return null;

            var title = Application.Current?.TryFindResource(row.PageName) as string;
            if (string.IsNullOrEmpty(title))
                title = string.IsNullOrEmpty(row.Name) ? row.Code.ToString() : row.Name;

            return new ShellMenuItem(title, row.PageName ?? "", selected, unselected, $"{row.Code}View");
        }

        /// <summary>
        /// 语言切换后重读导航、运行按钮和未登录文字。不重新打开当前页。
        /// </summary>
        private void OnLanguageChanged()
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null || dispatcher.CheckAccess())
            {
                ApplyLanguageText();
                return;
            }

            dispatcher.BeginInvoke(new Action(ApplyLanguageText));
        }

        /// <summary>
        /// 用当前语言资源替换主壳上已经生成的文字。
        /// </summary>
        private void ApplyLanguageText()
        {
            foreach (var menu in Menus)
            {
                if (string.IsNullOrEmpty(menu.TitleKey))
                    continue;

                var title = Application.Current?.TryFindResource(menu.TitleKey) as string;
                if (!string.IsNullOrEmpty(title))
                    menu.UseTitle(title);
            }

            foreach (var command in RunCommands)
                command.UseTitle(RunTitle(command.State));

            RunStatusText = RunTitle(_flowRun.IsRunning ? RunState.Start : RunState.Stop);
            if (!IsLoggedIn)
                LoginName = ReadText("Footer_NotLoggedIn", "未登录");

            RefreshLicenseStatus();
            RaisePropertyChanged(nameof(SessionToolTip));
            RaisePropertyChanged(nameof(ContentTitle));
        }

        /// <summary>
        /// 去掉当前菜单和内容区里的页面。列表重绑时的空选不走这里。
        /// </summary>
        private void ClearContent()
        {
            _selectedMenu = null;
            RaisePropertyChanged(nameof(SelectedMenu));
            RaisePropertyChanged(nameof(ContentTitle));
            if (!RegionManager.Regions.ContainsRegionWithName(ContentRegionName))
                return;

            RegionManager.Regions[ContentRegionName].RemoveAll();
        }

        /// <summary>
        /// 区域就绪后打开当前菜单页。构造时区域还不存在，所以启动时再调一次。
        /// </summary>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (OpenSelectedPage())
                return;

            Application.Current?.Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() => OpenSelectedPage()));
        }

        /// <summary>
        /// 把当前菜单对应的页面放进内容区。区域还没登记时返回 false。
        /// </summary>
        private bool OpenSelectedPage()
        {
            var viewName = SelectedMenu?.ViewName;
            if (string.IsNullOrEmpty(viewName))
                return false;
            if (!RegionManager.Regions.ContainsRegionWithName(ContentRegionName))
                return false;

            RegionManager.RequestNavigate(ContentRegionName, viewName);
            return true;
        }

        /// <summary>
        /// 按运行状态放入启动、停止、刷新。初始都是白底，点中后变黄。
        /// </summary>
        private void CreateRunCommands()
        {
            RunCommands.Add(CreateRun(RunState.Start, PackIconPhosphorIconsKind.PlayFill));
            RunCommands.Add(CreateRun(RunState.Stop, PackIconPhosphorIconsKind.StopFill));
            RunCommands.Add(CreateRun(RunState.Refresh, PackIconPhosphorIconsKind.ArrowClockwise));
        }

        /// <summary>
        /// 生成一项运行按钮。文字优先用语言资源，否则用枚举说明。
        /// </summary>
        private static ShellRunItem CreateRun(RunState state, PackIconPhosphorIconsKind icon)
        {
            return new ShellRunItem(state, RunTitle(state), icon);
        }

        /// <summary>
        /// 运行状态的显示文字。优先用语言资源，否则用枚举说明。
        /// </summary>
        private static string RunTitle(RunState state)
        {
            var key = "Run_" + state;
            var title = Application.Current?.TryFindResource(key) as string;
            if (string.IsNullOrEmpty(title))
                title = ReadRunText(state);

            return title;
        }

        /// <summary>
        /// 服务状态变化后改底栏。过期通知以服务当前值为准。
        /// </summary>
        private void OnFlowRunningChanged(bool running)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null || dispatcher.CheckAccess())
            {
                ShowRunning(running);
                return;
            }

            dispatcher.BeginInvoke(new Action(() => ShowRunning(running)));
        }

        /// <summary>
        /// 把底栏改成启动或停止。停稳时把停止按钮点亮。
        /// </summary>
        private void ShowRunning(bool running)
        {
            if (running != _flowRun.IsRunning)
                running = _flowRun.IsRunning;

            RunStatusText = RunTitle(running ? RunState.Start : RunState.Stop);
            if (!running)
                SelectRun(RunState.Stop);
        }

        /// <summary>
        /// 按状态点亮运行按钮，不再次调用启动或停止。
        /// </summary>
        private void SelectRun(RunState state)
        {
            var item = RunCommands.FirstOrDefault(command => command.State == state);
            _applyingRun = true;
            try
            {
                SelectedRun = item;
            }
            finally
            {
                _applyingRun = false;
            }
        }

        /// <summary>
        /// 读取运行状态的中文说明。
        /// </summary>
        private static string ReadRunText(RunState state)
        {
            var field = typeof(RunState).GetField(state.ToString());
            var description = field?.GetCustomAttribute<DescriptionAttribute>();
            return description?.Description ?? state.ToString();
        }

        /// <summary>
        /// 把底栏时间改成当前时刻。
        /// </summary>
        private void OnClockTick(object? sender, EventArgs e)
        {
            CurrentTimeText = DateTime.Now.ToString(TimeFormat);
            RefreshLicenseStatus();
        }

        /// <summary>
        /// 导入许可证后改底栏。事件可能不在界面线程。
        /// </summary>
        private void OnLicenseChanged(object? sender, EventArgs e)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null || dispatcher.CheckAccess())
            {
                RefreshLicenseStatus();
                return;
            }

            dispatcher.BeginInvoke(new Action(RefreshLicenseStatus));
        }

        /// <summary>
        /// 按当前许可证和语言生成底栏文字。
        /// </summary>
        private void RefreshLicenseStatus()
        {
            var evaluation = _licenses.Current;
            var state = LicenseFooter.StateOf(evaluation, DateTimeOffset.UtcNow);
            LicenseStatusText = state switch
            {
                LicenseFooterState.Authorized => ReadText("Footer_LicenseAuthorized", "已授权"),
                LicenseFooterState.Expired => ReadText("Footer_LicenseExpired", "授权到期"),
                LicenseFooterState.Expiring => string.Format(
                    ReadText("Footer_LicenseExpiring", "{0}到期"),
                    FormatExpiry(evaluation.NotAfterUtc)),
                _ => ReadText("Footer_LicenseUnauthorized", "未授权")
            };
        }

        /// <summary>
        /// 到期日显示到本地日期。没有值时是空串。
        /// </summary>
        private static string FormatExpiry(DateTimeOffset? value)
        {
            return value is { } time ? time.ToLocalTime().ToString(ExpiryDateFormat) : "";
        }

        /// <summary>
        /// 未登录时打开登录窗。已登录时先确认，确认后写日志并回到未登录。
        /// </summary>
        private async void OnSession()
        {
            if (IsLoggedIn)
            {
                if (_logoutPromptOpen)
                    return;

                _logoutPromptOpen = true;
                try
                {
                    var confirmed = await MetroDialogService.ConfirmAsync(
                        ReadText("Logout_Title", "注销"),
                        ReadText("Logout_Message", "是否确认注销？"));
                    if (!confirmed)
                        return;

                    var loginName = LoginName;
                    var role = _currentRole;
                    IsLoggedIn = false;
                    _currentRole = null;
                    LoginName = ReadText("Footer_NotLoggedIn", "未登录");
                    LogHelper.SysInfo($"注销成功 {loginName} 角色={role}");
                    LoadMenus();
                }
                finally
                {
                    _logoutPromptOpen = false;
                }

                return;
            }

            DraftLoginName = "";
            DraftPassword = "";
            LoginError = "";
            IsLoginOpen = true;
        }

        /// <summary>
        /// 账号和密码都对上才切换注销外观。失败留在登录窗上。
        /// </summary>
        private void ConfirmLogin()
        {
            if (!TryLogin(out var loginName, out var role))
                return;

            _currentRole = role;
            IsLoggedIn = true;
            LoginName = loginName;
            LogHelper.SysInfo($"登录成功 {loginName} 角色={role}");
            CloseLogin();
            LoadMenus();
        }

        /// <summary>
        /// 用系统库里的登录名和密码核对。密码不写日志。
        /// </summary>
        private bool TryLogin(out string loginName, out UserRole role)
        {
            loginName = DraftLoginName.Trim();
            role = UserRole.Admin;
            LoginError = "";

            if (loginName.Length == 0)
            {
                LoginError = ReadText("User_NeedLogin", "请填写登录名");
                return false;
            }

            if (DraftPassword.Length == 0)
            {
                LoginError = ReadText("User_NeedPassword", "请填写密码");
                return false;
            }

            if (!SqlSugarContext.IsOpen)
            {
                LoginError = ReadText("Login_Failed", "登录失败。");
                return false;
            }

            try
            {
                var typedName = loginName;
                var matches = SqlSugarContext.SystemDb.Queryable<UserEntity>()
                    .ToList()
                    .Where(item => string.Equals(item.LoginName?.Trim(), typedName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (matches.Count != 1 || !string.Equals(matches[0].Password, DraftPassword, StringComparison.Ordinal))
                {
                    LoginError = ReadText("Login_BadCredential", "用户名或密码不正确。");
                    return false;
                }

                if (!matches[0].IsValid)
                {
                    LoginError = ReadText("Login_UserDisabled", "该用户已停用。");
                    return false;
                }

                var storedName = matches[0].LoginName?.Trim();
                if (!string.IsNullOrEmpty(storedName))
                    loginName = storedName;
                role = matches[0].RoleLevel;
                return true;
            }
            catch (Exception ex)
            {
                LoginError = ReadText("Login_Failed", "登录失败。");
                LogHelper.SysError($"登录失败 {loginName}", ex);
                return false;
            }
        }

        /// <summary>
        /// 关掉登录窗并清掉窗里的输入和错误提示。
        /// </summary>
        private void CloseLogin()
        {
            IsLoginOpen = false;
            DraftLoginName = "";
            DraftPassword = "";
            LoginError = "";
        }

        /// <summary>
        /// 读取语言资源。没有资源时用后备文字。
        /// </summary>
        private static string ReadText(string key, string fallback)
        {
            var text = Application.Current?.TryFindResource(key) as string;
            return string.IsNullOrEmpty(text) ? fallback : text;
        }
    }
}
