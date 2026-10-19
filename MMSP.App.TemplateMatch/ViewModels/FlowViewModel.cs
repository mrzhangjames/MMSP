using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.IconPacks;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.WpfBase.Core;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Plugin.Tool;
using MMSP.Service.Database;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 流程页。流程数据页放列表、工具和流程步，MES 配置单独一页。连接读客户库。
    /// 拖入工具会写入流程步，流程步可上下拖动并写回顺序。属性窗口会写回工具配置。新增和编辑只改流程名称。删除会连同流程步一起去掉。
    /// </summary>
    public partial class FlowViewModel : BaseViewModel, IDropTarget
    {
        private const int EndpointLimit = 256;
        private const int StationLimit = 32;

        private bool _loading = true;
        private FlowCard? _selectedFlow;
        private int _pageIndex;
        private Guid? _mesId;
        private string _mesEndpoint = "";
        private string _mesStation = "";
        private string _mesTimeout = "";
        private string _mesRetry = "";
        private string _mesEnabled = "";
        private string _draftEndpoint = "";
        private string _draftStation = "";
        private string _draftTimeout = "";
        private string _draftRetry = "";
        private string _draftEnabled = "";
        private string _mesError = "";

        /// <summary>
        /// 创建流程页。工具按种类生成，流程等进入页面后再读库。
        /// </summary>
        public FlowViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator)
            : base(region, container, eventAggregator)
        {
            Flows = [];
            Tools = [];
            EnabledChoices = [Text("User_Yes", "是"), Text("User_No", "否")];
            _mesEnabled = NoText();
            _draftEnabled = _mesEnabled;
            SelectCommand = new DelegateCommand<FlowCard?>(Select);
            SelectStepCommand = new DelegateCommand<FlowStep?>(SelectStep);
            ShowPropertyCommand = new DelegateCommand<FlowStep?>(ShowProperty);
            SaveMesCommand = new DelegateCommand(SaveMes);
            CancelMesCommand = new DelegateCommand(CloseMes);
            AddCommand = new DelegateCommand(OpenAdd);
            EditCommand = new DelegateCommand<FlowCard?>(OpenEdit);
            DeleteCommand = new DelegateCommand(Delete);
            RunCommand = new DelegateCommand(LayoutOnly);
            CopyCommand = new DelegateCommand(LayoutOnly);
            ExportCommand = new DelegateCommand(LayoutOnly);
            ImportCommand = new DelegateCommand(LayoutOnly);
            _loading = false;
        }

        /// <summary>
        /// 流程卡片。
        /// </summary>
        public ObservableCollection<FlowCard> Flows { get; }

        /// <summary>
        /// 已加载工具。打开流程页时按枚举填入。
        /// </summary>
        public ObservableCollection<FlowTool> Tools { get; }

        /// <summary>
        /// 选中一张流程步卡片。
        /// </summary>
        public DelegateCommand<FlowStep?> SelectStepCommand { get; }

        /// <summary>
        /// 打开选中流程步的工具属性。
        /// </summary>
        public DelegateCommand<FlowStep?> ShowPropertyCommand { get; }

        /// <summary>
        /// 工具数量。
        /// </summary>
        public int ToolCount => Tools.Count;

        /// <summary>
        /// 启用下拉项。是、否。
        /// </summary>
        public IReadOnlyList<string> EnabledChoices { get; }

        /// <summary>
        /// 当前流程。流程数据页右侧标题和步骤跟着它。没有流程时为空。
        /// </summary>
        public FlowCard? SelectedFlow
        {
            get => _selectedFlow;
            private set => SetProperty(ref _selectedFlow, value);
        }

        /// <summary>
        /// 当前页。0 是流程数据，1 是 MES 配置。
        /// </summary>
        public int PageIndex
        {
            get => _pageIndex;
            set
            {
                if (!SetProperty(ref _pageIndex, value))
                    return;

                if (value == 1)
                    FillDraft();
            }
        }

        /// <summary>
        /// MES 配置页里的接口地址。
        /// </summary>
        public string DraftEndpoint
        {
            get => _draftEndpoint;
            set => SetProperty(ref _draftEndpoint, value);
        }

        /// <summary>
        /// MES 配置页里的工位。
        /// </summary>
        public string DraftStation
        {
            get => _draftStation;
            set => SetProperty(ref _draftStation, value);
        }

        /// <summary>
        /// MES 配置页里的超时。
        /// </summary>
        public string DraftTimeout
        {
            get => _draftTimeout;
            set => SetProperty(ref _draftTimeout, value);
        }

        /// <summary>
        /// MES 配置页里的重试次数。
        /// </summary>
        public string DraftRetry
        {
            get => _draftRetry;
            set => SetProperty(ref _draftRetry, value);
        }

        /// <summary>
        /// MES 配置页里的启用。是或否。
        /// </summary>
        public string DraftEnabled
        {
            get => _draftEnabled;
            set => SetProperty(ref _draftEnabled, value);
        }

        /// <summary>
        /// MES 配置页里的保存错误。空字符串表示没有错误。
        /// </summary>
        public string MesError
        {
            get => _mesError;
            private set => SetProperty(ref _mesError, value);
        }

        /// <summary>
        /// 选中一张流程卡片。
        /// </summary>
        public DelegateCommand<FlowCard?> SelectCommand { get; }

        /// <summary>
        /// 保存 MES 配置。
        /// </summary>
        public DelegateCommand SaveMesCommand { get; }

        /// <summary>
        /// 放弃未保存的修改，恢复成已保存的值。
        /// </summary>
        public DelegateCommand CancelMesCommand { get; }

        /// <summary>
        /// 打开新增流程弹窗。
        /// </summary>
        public DelegateCommand AddCommand { get; }

        /// <summary>
        /// 打开编辑流程弹窗。参数是被点中的卡片。
        /// </summary>
        public DelegateCommand<FlowCard?> EditCommand { get; }

        /// <summary>
        /// 确认后删除当前流程，并删除它的流程步。
        /// </summary>
        public DelegateCommand DeleteCommand { get; }

        /// <summary>
        /// 执行。这一批只摆按钮。
        /// </summary>
        public DelegateCommand RunCommand { get; }

        /// <summary>
        /// 复制。这一批只摆按钮。
        /// </summary>
        public DelegateCommand CopyCommand { get; }

        /// <summary>
        /// 导出。这一批只摆按钮。
        /// </summary>
        public DelegateCommand ExportCommand { get; }

        /// <summary>
        /// 导入。这一批只摆按钮。
        /// </summary>
        public DelegateCommand ImportCommand { get; }

        /// <summary>
        /// 进入页面时读取流程和 MES 连接。
        /// </summary>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadTools();
            Load(_selectedFlow?.Id);
        }

        /// <summary>
        /// 按工具枚举生成中间工具区。同一次运行里再次进入不重复生成。
        /// </summary>
        private void LoadTools()
        {
            if (Tools.Count > 0)
                return;

            foreach (var tool in CreateTools())
                Tools.Add(tool);

            RaisePropertyChanged(nameof(ToolCount));
        }

        private void Load(Guid? selectId)
        {
            _loading = true;
            MesError = "";
            foreach (var flow in Flows)
                flow.PropertyChanged -= OnFlowChanged;

            Flows.Clear();
            if (SqlSugarContext.IsOpen)
            {
                var db = SqlSugarContext.CustomerDb;
                var flows = db.Queryable<FlowEntity>().OrderBy(item => item.SortIndex).ToList();
                var steps = db.Queryable<FlowStepEntity>().OrderBy(item => item.SortIndex).ToList();
                var stepsByFlow = steps
                    .GroupBy(item => item.FlowId)
                    .ToDictionary(group => group.Key, group => (IReadOnlyList<FlowStepEntity>)group.ToList());
                foreach (var row in flows)
                {
                    stepsByFlow.TryGetValue(row.Id, out var owned);
                    var card = ToCard(row, owned ?? []);
                    card.PropertyChanged += OnFlowChanged;
                    Flows.Add(card);
                }

                var mes = db.Queryable<FlowMesEntity>().OrderBy(item => item.Id).ToList().FirstOrDefault();
                if (mes is null)
                    ApplyMes(null, "", "", "", "", false);
                else
                    ApplyMes(mes.Id, mes.Endpoint, mes.Station, TimeoutText(mes.TimeoutMs), RetryText(mes.RetryCount), mes.IsEnabled);
            }
            else
            {
                ApplyMes(null, "", "", "", "", false);
            }

            _loading = false;
            var selected = Flows.FirstOrDefault(item => item.Id == selectId) ?? Flows.FirstOrDefault();
            if (selected is null)
            {
                SelectedFlow = null;
                return;
            }

            Select(selected);
        }

        private void OnFlowChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_loading || sender is not FlowCard card)
                return;

            if (e.PropertyName is nameof(FlowCard.IsEnabled) or nameof(FlowCard.LogOutput))
                SaveFlags(card);
        }

        /// <summary>
        /// 把启用和日志输出写回当前流程。失败时拨回库里的值。
        /// </summary>
        private void SaveFlags(FlowCard card)
        {
            if (!SqlSugarContext.IsOpen)
                return;

            try
            {
                var db = SqlSugarContext.CustomerDb;
                var entity = db.Queryable<FlowEntity>().Where(item => item.Id == card.Id).ToList().FirstOrDefault();
                if (entity is null)
                    return;

                entity.IsEnabled = card.IsEnabled;
                entity.LogOutput = card.LogOutput;
                db.Updateable(entity).ExecuteCommand();
            }
            catch (Exception)
            {
                RestoreFlags(card);
            }
        }

        /// <summary>
        /// 保存失败时，开关回到客户库里的值。
        /// </summary>
        private void RestoreFlags(FlowCard card)
        {
            if (!SqlSugarContext.IsOpen)
                return;

            try
            {
                var entity = SqlSugarContext.CustomerDb.Queryable<FlowEntity>()
                    .Where(item => item.Id == card.Id)
                    .ToList()
                    .FirstOrDefault();
                if (entity is null)
                    return;

                _loading = true;
                card.IsEnabled = entity.IsEnabled;
                card.LogOutput = entity.LogOutput;
            }
            catch (Exception)
            {
                // 库也读不回来时，保留界面上这次拨动。
            }
            finally
            {
                _loading = false;
            }
        }

        private void Select(FlowCard? card)
        {
            if (card is null)
                return;

            foreach (var flow in Flows)
                flow.IsSelected = ReferenceEquals(flow, card);

            SelectedFlow = card;
        }

        /// <summary>
        /// 把已保存的 MES 配置抄进配置页。
        /// </summary>
        private void FillDraft()
        {
            DraftEndpoint = _mesEndpoint;
            DraftStation = _mesStation;
            DraftTimeout = _mesTimeout;
            DraftRetry = _mesRetry;
            DraftEnabled = _mesEnabled;
            MesError = "";
        }

        private void SaveMes()
        {
            MesError = "";
            if (!TryNonNegative(DraftTimeout, out var timeoutMs))
            {
                MesError = Text("Flow_NeedTimeout", "超时请填写非负整数");
                return;
            }

            if (!TryNonNegative(DraftRetry, out var retryCount))
            {
                MesError = Text("Flow_NeedRetry", "重试次数请填写非负整数");
                return;
            }

            if (!SqlSugarContext.IsOpen)
            {
                MesError = Text("User_SaveFailed", "保存失败");
                return;
            }

            var endpoint = Limit(DraftEndpoint, EndpointLimit);
            var station = Limit(DraftStation, StationLimit);
            var enabled = EnabledChoices.Count > 0 && DraftEnabled == EnabledChoices[0];
            try
            {
                var db = SqlSugarContext.CustomerDb;
                var entity = _mesId is Guid id
                    ? db.Queryable<FlowMesEntity>().Where(item => item.Id == id).ToList().FirstOrDefault()
                    : null;
                if (entity is null)
                {
                    entity = new FlowMesEntity { Id = Guid.NewGuid() };
                    CopyMes(entity, endpoint, station, timeoutMs, retryCount, enabled);
                    db.Insertable(entity).ExecuteCommand();
                }
                else
                {
                    CopyMes(entity, endpoint, station, timeoutMs, retryCount, enabled);
                    db.Updateable(entity).ExecuteCommand();
                }

                ApplyMes(entity.Id, endpoint, station, TimeoutText(timeoutMs), RetryText(retryCount), enabled);
                LogHelper.FlowInfo("保存 MES 配置");
            }
            catch (Exception)
            {
                MesError = Text("User_SaveFailed", "保存失败");
            }
        }

        private void CloseMes()
        {
            FillDraft();
        }

        /// <summary>
        /// 工具栏按钮只出现在界面上，不改列表。
        /// </summary>
        private void LayoutOnly()
        {
        }

        private void ApplyMes(Guid? id, string endpoint, string station, string timeout, string retry, bool enabled)
        {
            _mesId = id;
            _mesEndpoint = endpoint;
            _mesStation = station;
            _mesTimeout = timeout;
            _mesRetry = retry;
            _mesEnabled = enabled ? YesText() : NoText();
            if (_pageIndex == 1)
                FillDraft();
        }

        private static void CopyMes(FlowMesEntity entity, string endpoint, string station, int timeoutMs, int retryCount, bool enabled)
        {
            entity.Endpoint = endpoint;
            entity.Station = station;
            entity.TimeoutMs = timeoutMs;
            entity.RetryCount = retryCount;
            entity.IsEnabled = enabled;
        }

        private FlowCard ToCard(FlowEntity row, IReadOnlyList<FlowStepEntity> owned)
        {
            var steps = ToSteps(owned);
            var card = new FlowCard
            {
                Id = row.Id,
                Name = row.Name,
                SortIndex = row.SortIndex,
                IsEnabled = row.IsEnabled,
                LogOutput = row.LogOutput,
                Steps = steps
            };
            steps.CollectionChanged += (_, _) => card.NotifyStepCount();
            return card;
        }

        private static ObservableCollection<FlowStep> ToSteps(IReadOnlyList<FlowStepEntity> rows)
        {
            var steps = new ObservableCollection<FlowStep>();
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                steps.Add(new FlowStep
                {
                    Id = row.Id,
                    Number = i + 1,
                    ToolKind = row.ToolKind,
                    Kind = Describe(row.ToolKind),
                    Title = row.Title,
                    Detail = DetailText(row),
                    ShowStem = i < rows.Count - 1,
                    CfgValue = row.CfgValue
                });
            }

            return steps;
        }

        private static string DetailText(FlowStepEntity step)
        {
            if (!string.IsNullOrWhiteSpace(step.CfgValue))
                return ToolCatalog.Detail(step.ToolKind, step.CfgValue);

            return step.ToolKind == FlowToolKind.Delay && step.NumberValue > 0
                ? "超时 " + step.NumberValue + " ms"
                : "";
        }

        private static string TimeoutText(int timeoutMs)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{timeoutMs} ms");
        }

        private static string RetryText(int retryCount)
        {
            return retryCount.ToString(CultureInfo.InvariantCulture);
        }

        private static bool TryNonNegative(string text, out int value)
        {
            var source = text.Trim();
            if (source.EndsWith("ms", StringComparison.OrdinalIgnoreCase))
                source = source[..^2].Trim();

            if (int.TryParse(source, NumberStyles.None, CultureInfo.InvariantCulture, out value) && value >= 0)
                return true;

            value = 0;
            return false;
        }

        private static string Limit(string text, int max)
        {
            var value = text.Trim();
            return value.Length <= max ? value : value[..max];
        }

        private string YesText()
        {
            return EnabledChoices.Count > 0 ? EnabledChoices[0] : Text("User_Yes", "是");
        }

        private string NoText()
        {
            return EnabledChoices.Count > 1 ? EnabledChoices[1] : Text("User_No", "否");
        }

        private static string Text(string key, string fallback)
        {
            if (Application.Current?.TryFindResource(key) is string value && value.Length > 0)
                return value;

            return fallback;
        }

        private static string Describe(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var text = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
            return string.IsNullOrWhiteSpace(text) ? value.ToString() : text;
        }

        private static IReadOnlyList<FlowTool> CreateTools()
        {
            return Enum.GetValues<FlowToolKind>()
                .Select(kind => new FlowTool
                {
                    ToolKind = kind,
                    Name = Describe(kind),
                    Icon = Icon(kind)
                })
                .ToArray();
        }

        private static PackIconPhosphorIconsKind Icon(FlowToolKind kind)
        {
            return kind switch
            {
                FlowToolKind.SingleScan => PackIconPhosphorIconsKind.Barcode,
                FlowToolKind.MultiScan => PackIconPhosphorIconsKind.Stack,
                FlowToolKind.PlcRead => PackIconPhosphorIconsKind.ArrowDown,
                FlowToolKind.PlcWrite => PackIconPhosphorIconsKind.ArrowUp,
                FlowToolKind.PlcMonitor => PackIconPhosphorIconsKind.Eye,
                FlowToolKind.Delay => PackIconPhosphorIconsKind.Timer,
                FlowToolKind.JsonParse => PackIconPhosphorIconsKind.BracketsCurly,
                FlowToolKind.Condition => PackIconPhosphorIconsKind.GitFork,
                _ => PackIconPhosphorIconsKind.Code
            };
        }
    }
}
