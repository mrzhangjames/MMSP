using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using MahApps.Metro.IconPacks;
using MMSP.App.Main.Models;
using MMSP.App.Main.Services;
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
    /// 主壳。点击菜单切换内容区页面。底栏按设备配置显示 PLC 和扫码枪连接状态。
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
        /// 每秒刷新底栏时间，并检查会话超时。定时器在界面线程上跑。
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
        /// 最近一轮设备读数。语言切换时用它重拼状态文字。
        /// </summary>
        private IReadOnlyList<DeviceLinkReading> _deviceReadings = [];

        /// <summary>
        /// 底栏是否已经有设备。集合个数不会通知界面，所以单独记这一位。
        /// </summary>
        private bool _hasDeviceLinks;

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
        /// 当前登录角色。未登录时为空。主页和用户页始终保留。
        /// </summary>
        private UserRole? _currentRole;

        /// <summary>
        /// 底栏登录名。未登录时是「未登录」。
        /// </summary>
        private string _loginName = "";

        /// <summary>
        /// 用户页写入的登录会话。
        /// </summary>
        private readonly UserSession _session;

        /// <summary>
        /// 创建主壳数据。未登录时只打开主页。
        /// </summary>
        public MainViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator,
            IFlowRunService flowRun,
            LicenseStatusStore licenses,
            UserSession session)
            : base(region, container, eventAggregator)
        {
            _flowRun = flowRun;
            _licenses = licenses;
            _session = session;
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
            _session.Changed += OnSessionChanged;
            CurrentTimeText = DateTime.Now.ToString(TimeFormat);
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += OnClockTick;
            _clockTimer.Start();
            DeviceLinks = new ObservableCollection<DeviceLinkItem>();
            AppAppearance.LanguageChanged += OnLanguageChanged;
            DeviceLinkWatch.Shared.Updated += OnDeviceLinks;
            DeviceLinkWatch.Shared.Start();
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

                if (value.State == RunState.Start)
                {
                    if (_licenses.IsExpired)
                    {
                        LogHelper.FlowError(ReadText(
                            "License_UseBlocked",
                            "许可证已过期，当前功能不可用。导入有效许可证后即可恢复。"));
                        RaisePropertyChanged(nameof(SelectedRun));
                        return;
                    }

                    if (!_flowRun.TryStart())
                    {
                        RaisePropertyChanged(nameof(SelectedRun));
                        return;
                    }
                }

                if (value.State == RunState.Stop)
                    _flowRun.Stop();

                SetProperty(ref _selectedRun, value);
            }
        }

        /// <summary>
        /// 底栏设备文字。PLC 在前，启用的扫码枪按配置顺序排在后面。第一轮读完之前为空。
        /// 每一项的连接方式见 <see cref="ShowLinks"/>。
        /// </summary>
        public ObservableCollection<DeviceLinkItem> DeviceLinks { get; }

        /// <summary>
        /// 底栏是否显示设备连接。没有 PLC 行、也没有启用的扫码枪时为 false。
        /// </summary>
        public bool HasDeviceLinks
        {
            get => _hasDeviceLinks;
            private set => SetProperty(ref _hasDeviceLinks, value);
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
            private set => SetProperty(ref _isLoggedIn, value);
        }

        /// <summary>
        /// 底栏时间，每秒更新。
        /// </summary>
        public string CurrentTimeText
        {
            get => _currentTimeText;
            private set => SetProperty(ref _currentTimeText, value);
        }

        /// <summary>
        /// 按当前角色重算导航。主页和用户页始终保留。未登录只留这两页。
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
                if (row.Code is MenuCode.Home or MenuCode.User)
                {
                    // 主页和用户页不按角色裁掉。用户页负责登录和注销。
                }
                else if (_currentRole is UserRole role)
                {
                    if (!UserRoleAccess.CanOpen(role, row.MinRole))
                        continue;
                }
                else
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
            ShowLinks();
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
        /// 探测线程上的读数。切回界面线程再改底栏。
        /// </summary>
        private void OnDeviceLinks(IReadOnlyList<DeviceLinkReading> readings)
        {
            var copy = readings.ToArray();
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null || dispatcher.CheckAccess())
            {
                ApplyDeviceLinks(copy);
                return;
            }

            dispatcher.BeginInvoke(new Action(() => ApplyDeviceLinks(copy)));
        }

        /// <summary>
        /// 记下这一轮读数，并按当前语言拼底栏文字。
        /// </summary>
        private void ApplyDeviceLinks(IReadOnlyList<DeviceLinkReading> readings)
        {
            _deviceReadings = readings;
            ShowLinks();
        }

        /// <summary>
        /// 用最近一轮读数重建设备文字。读数来自 DeviceLinkWatch，这里拼成「名称 已连接」或「名称 未连接」。
        /// 客户库有 PLC 配置行时，第一项固定写 PLC。后面只排启用的扫码枪，按配置顺序。停用的枪不出现。
        /// 枪名为空时用地址，地址也为空时写扫码枪。客户库没打开，或这一轮没有设备时，底栏不显示这段。
        /// 是否连上按下面的顺序判断。设备页或流程里这个客户端已经连着，直接算已连接，不再另开一条连接。
        /// PLC 配了心跳地址时，看最近一次心跳写入。写入成功是已连接，还没写成功、正在重连或写入失败是未连接。这条不做短连接。
        /// PLC 没有心跳地址，以及没有被占用的扫码枪，按保存的参数做短连接。Connect 成功就是已连接，连完立刻关掉。扫码枪不发送触发。
        /// 短连接失败后连续再连，一共 3 次，间隔 200 毫秒。一次成功就停，三次都失败才是未连接。这一轮结束后再等 3 秒看下一轮。
        /// </summary>
        private void ShowLinks()
        {
            var connected = ReadText("Footer_Connected", "已连接");
            var disconnected = ReadText("Footer_Disconnected", "未连接");
            if (_deviceReadings.Count == 0)
            {
                HasDeviceLinks = false;
                DeviceLinks.Clear();
                return;
            }

            DeviceLinks.Clear();
            foreach (var reading in _deviceReadings)
            {
                var word = reading.IsConnected ? connected : disconnected;
                DeviceLinks.Add(new DeviceLinkItem(reading.Name, reading.IsConnected, word));
            }

            HasDeviceLinks = true;
        }

        /// <summary>
        /// 把底栏时间改成当前时刻。
        /// </summary>
        private void OnClockTick(object? sender, EventArgs e)
        {
            CurrentTimeText = DateTime.Now.ToString(TimeFormat);
            RefreshLicenseStatus();
            ExpireSession();
        }

        /// <summary>
        /// 会话超时到点就注销。0 不注销。不读流程，也不停流程。
        /// </summary>
        private void ExpireSession()
        {
            if (!_session.IsLoggedIn || _session.LoginTime is not DateTime loginTime)
                return;

            if (!UserSessionTimeout.Due(_session.TimeoutMinutes, loginTime, DateTime.Now))
                return;

            var loginName = _session.LoginName;
            var role = _session.Role;
            _session.SignOut();
            LogHelper.SysInfo($"会话超时 {loginName} 角色={role}");
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
        /// 用户页登录或注销后刷新底栏和菜单。
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
        /// 把会话抄到底栏角色，并按新角色重算菜单。
        /// </summary>
        private void ApplySession()
        {
            if (_session.IsLoggedIn)
            {
                _currentRole = _session.Role;
                IsLoggedIn = true;
                LoginName = _session.LoginName;
            }
            else
            {
                _currentRole = null;
                IsLoggedIn = false;
                LoginName = ReadText("Footer_NotLoggedIn", "未登录");
            }

            LoadMenus();
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
