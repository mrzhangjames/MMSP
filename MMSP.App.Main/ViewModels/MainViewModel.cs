using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using MahApps.Metro.IconPacks;
using MMSP.App.Main.Models;
using MMSP.App.WpfBase.Core;
using MMSP.Common.Helper;
using System.ComponentModel;
using System.Reflection;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;

namespace MMSP.App.Main.ViewModels
{
    /// <summary>
    /// 主壳。这一批只提供六个菜单、内容标题和底栏占位，不打开业务页。
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        /// <summary>
        /// 底栏时间格式。
        /// </summary>
        private const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

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
        /// 底栏时间文字。
        /// </summary>
        private string _currentTimeText = "";

        /// <summary>
        /// 创建主壳数据，并选中主页。
        /// </summary>
        public MainViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator)
            : base(region, container, eventAggregator)
        {
            Menus = new ObservableCollection<ShellMenuItem>();
            CreateMenus();
            if (Menus.Count > 0)
                SelectedMenu = Menus[0];
            RunCommands = new ObservableCollection<ShellRunItem>();
            CreateRunCommands();
            VersionText = AppVersionHelper.GetVersionText();
            LoginName = "admin";
            CurrentTimeText = DateTime.Now.ToString(TimeFormat);
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += OnClockTick;
            _clockTimer.Start();
        }

        /// <summary>
        /// 系统库中启用的菜单，按排序号排列。
        /// </summary>
        public ObservableCollection<ShellMenuItem> Menus { get; }

        /// <summary>
        /// 导航右侧的启动、停止、刷新。不放进 <see cref="Menus"/>。
        /// </summary>
        public ObservableCollection<ShellRunItem> RunCommands { get; }

        /// <summary>
        /// 当前运行按钮。切换后底栏显示对应状态。
        /// </summary>
        public ShellRunItem? SelectedRun
        {
            get => _selectedRun;
            set
            {
                if (SetProperty(ref _selectedRun, value))
                    RaisePropertyChanged(nameof(RunStatusText));
            }
        }

        /// <summary>
        /// 底栏运行状态。未选择时为空。
        /// </summary>
        public string RunStatusText => SelectedRun?.Title ?? "";

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
                    RaisePropertyChanged(nameof(ContentTitle));
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
        /// 底栏登录名。这一批固定为 admin，对应登录账户的 LoginName。
        /// </summary>
        public string LoginName { get; }

        /// <summary>
        /// 底栏时间，每秒更新。
        /// </summary>
        public string CurrentTimeText
        {
            get => _currentTimeText;
            private set => SetProperty(ref _currentTimeText, value);
        }

        /// <summary>
        /// 从系统库读取启用的菜单。
        /// </summary>
        private void CreateMenus()
        {
            var rows = SqlSugarContext.SystemDb.Queryable<MenuEntity>()
                .Where(item => item.IsValid)
                .OrderBy(item => item.SortIndex)
                .ToList();

            foreach (var row in rows)
            {
                if (!Enum.TryParse(row.SelectedIcon, out PackIconPhosphorIconsKind selected))
                    continue;
                if (!Enum.TryParse(row.UnSelectedIcon, out PackIconPhosphorIconsKind unselected))
                    continue;

                var title = Application.Current?.TryFindResource(row.PageName) as string;
                if (string.IsNullOrEmpty(title))
                    title = string.IsNullOrEmpty(row.Name) ? row.Code.ToString() : row.Name;

                Menus.Add(new ShellMenuItem(title, selected, unselected));
            }
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
            var key = "Run_" + state;
            var title = Application.Current?.TryFindResource(key) as string;
            if (string.IsNullOrEmpty(title))
                title = ReadRunText(state);

            return new ShellRunItem(state, title, icon);
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
        }
    }
}
