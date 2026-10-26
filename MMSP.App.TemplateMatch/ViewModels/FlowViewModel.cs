using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.IconPacks;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.WpfBase.Core;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Helper;
using MMSP.Core.Model.Sys;
using MMSP.Plugin.Tool;
using MMSP.Service.Database;
using MMSP.Service.Flow;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 流程页。流程数据页放列表、工具和流程步，MES 配置单独一页。配置按当前客户和设备选出的类读写。
    /// 拖入工具会写入流程步，流程步可上下拖动并写回顺序。属性窗口会写回工具配置。新增只改流程名称。名称和变量从流程卡片右键进入。删除会连同流程步和变量一起去掉。
    /// 执行跑当前流程一轮，单步执行只跑右键的那一步，并在步骤上显示状态和耗时。复制会连同流程步和变量一起新增一条。
    /// </summary>
    public partial class FlowViewModel : BaseViewModel, IDropTarget
    {
        private bool _loading = true;
        private FlowCard? _selectedFlow;
        private int _pageIndex;
        private string _savedCfg = "";
        private string _mesError = "";
        /// <summary>
        /// 单次执行和单步执行走这一条服务。
        /// </summary>
        private readonly IFlowRunService _flowRun;

        /// <summary>
        /// 创建流程页。工具按种类生成，流程等进入页面后再读库。
        /// </summary>
        public FlowViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator,
            IFlowRunService flowRun)
            : base(region, container, eventAggregator)
        {
            _flowRun = flowRun;
            Flows = [];
            GeneralTools = [];
            CustomerTools = [];
            MesFields = [];
            SelectCommand = new DelegateCommand<FlowCard?>(Select);
            SelectStepCommand = new DelegateCommand<FlowStep?>(SelectStep);
            RunStepCommand = new DelegateCommand<FlowStep?>(RunStep);
            ShowPropertyCommand = new DelegateCommand<FlowStep?>(ShowProperty);
            DeleteStepCommand = new DelegateCommand<FlowStep?>(DeleteStep);
            SaveMesCommand = new DelegateCommand(SaveMes);
            CancelMesCommand = new DelegateCommand(CloseMes);
            AddCommand = new DelegateCommand(OpenAdd);
            EditCommand = new DelegateCommand<FlowCard?>(OpenEdit);
            EditVarsCommand = new DelegateCommand<FlowCard?>(OpenVars);
            DeleteCommand = new DelegateCommand(Delete);
            RunCommand = new DelegateCommand(RunSelected);
            CopyCommand = new DelegateCommand(CopySelected);
            ExportCommand = new DelegateCommand(ExportFlows);
            ImportCommand = new DelegateCommand(ImportFlows);
            _loading = false;
        }

        /// <summary>
        /// 流程卡片。
        /// </summary>
        public ObservableCollection<FlowCard> Flows { get; }

        /// <summary>
        /// 通用工具。编号 0 到 9。
        /// </summary>
        public ObservableCollection<FlowTool> GeneralTools { get; }

        /// <summary>
        /// 当前客户插件登记的工具。没有登记结果时为空。
        /// </summary>
        public ObservableCollection<FlowTool> CustomerTools { get; }

        /// <summary>
        /// 客户工具组标题。没有客户工具时不显示。
        /// </summary>
        public string CustomerToolGroup { get; private set; } = "";

        /// <summary>
        /// 是否显示客户工具组。
        /// </summary>
        public bool HasCustomerTools => CustomerTools.Count > 0;

        /// <summary>
        /// 选中一张流程步卡片。
        /// </summary>
        public DelegateCommand<FlowStep?> SelectStepCommand { get; }

        /// <summary>
        /// 只执行右键的这一步。流程未启用也可以跑。
        /// </summary>
        public DelegateCommand<FlowStep?> RunStepCommand { get; }

        /// <summary>
        /// 打开选中流程步的工具属性。
        /// </summary>
        public DelegateCommand<FlowStep?> ShowPropertyCommand { get; }

        /// <summary>
        /// 删除选中的流程步。
        /// </summary>
        public DelegateCommand<FlowStep?> DeleteStepCommand { get; }

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
        /// MES 配置页的输入项。来自当前配置类的公共属性。
        /// </summary>
        public ObservableCollection<MesFieldItem> MesFields { get; }

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
        /// 打开编辑流程弹窗。参数是被点中的卡片。只改名称。
        /// </summary>
        public DelegateCommand<FlowCard?> EditCommand { get; }

        /// <summary>
        /// 打开变量列表。参数是被点中的卡片。
        /// </summary>
        public DelegateCommand<FlowCard?> EditVarsCommand { get; }

        /// <summary>
        /// 确认后删除当前流程，并删除它的流程步和变量。
        /// </summary>
        public DelegateCommand DeleteCommand { get; }

        /// <summary>
        /// 执行当前流程一轮，并在步骤卡片上显示状态和耗时。
        /// </summary>
        public DelegateCommand RunCommand { get; }

        /// <summary>
        /// 复制当前流程、流程步和变量。
        /// </summary>
        public DelegateCommand CopyCommand { get; }

        /// <summary>
        /// 导出全部流程配置。
        /// </summary>
        public DelegateCommand ExportCommand { get; }

        /// <summary>
        /// 导入全部流程配置。确认后替换客户库里的流程、流程步和变量。
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
        /// 通用工具固定为 0 到 9。客户工具只取当前插件登记的种类。同一次运行里再次进入不重复生成。
        /// </summary>
        private void LoadTools()
        {
            if (GeneralTools.Count > 0 || CustomerTools.Count > 0)
                return;

            foreach (var kind in Enum.GetValues<FlowToolKind>())
            {
                if ((int)kind > (int)FlowToolKind.Uph)
                    continue;

                GeneralTools.Add(MakeTool(kind));
            }

            if (IContainerProviderExtensions.IsRegistered<CustomerPluginCatalog>(ContainerExt))
            {
                var catalog = ContainerExt.Resolve<CustomerPluginCatalog>();
                CustomerToolGroup = catalog.ToolGroupName;
                foreach (var tool in catalog.Tools)
                    CustomerTools.Add(MakeTool(tool.Kind));
            }

            RaisePropertyChanged(nameof(CustomerToolGroup));
            RaisePropertyChanged(nameof(HasCustomerTools));
        }

        /// <summary>
        /// 做成工具箱里的一张卡片。
        /// </summary>
        private static FlowTool MakeTool(FlowToolKind kind)
        {
            return new FlowTool
            {
                ToolKind = kind,
                Name = Describe(kind),
                Icon = Icon(kind)
            };
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
            }

            LoadMes();

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
            if (LicenseGate.Block(ContainerExt))
            {
                RestoreFlags(card);
                return;
            }

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
        /// 按当前配置类读取已保存的 JSON。没有对上的类时不显示输入项。
        /// </summary>
        private void LoadMes()
        {
            MesError = "";
            var model = CurrentModel();
            if (model is null)
            {
                _savedCfg = "";
                ShowFields(null);
                return;
            }

            var typeName = model.GetType().FullName ?? model.GetType().Name;
            var json = "";
            if (SqlSugarContext.IsOpen)
            {
                try
                {
                    var row = SqlSugarContext.CustomerDb.Queryable<FlowMesEntity>().ToList()
                        .FirstOrDefault(item => item.ConfigType == typeName);
                    json = row?.CfgValue ?? "";
                    MesCfgJson.Read(model, json);
                    json = MesCfgJson.Write(model);
                }
                catch (Exception)
                {
                    MesCfgJson.Read(model, "");
                    json = MesCfgJson.Write(model);
                    MesError = "MES 配置无法读取";
                }
            }
            else
            {
                MesCfgJson.Read(model, "");
                json = MesCfgJson.Write(model);
            }

            _savedCfg = json;
            ShowFields(model);
        }

        /// <summary>
        /// 当前客户和设备选出的配置实例。没有登记时为空。
        /// </summary>
        private MesCfgModel? CurrentModel()
        {
            if (!IContainerProviderExtensions.IsRegistered<MesCfgModel>(ContainerExt))
                return null;

            return ContainerExt.Resolve<MesCfgModel>();
        }

        /// <summary>
        /// 把已保存的 MES 配置抄进配置页。
        /// </summary>
        private void FillDraft()
        {
            MesError = "";
            var model = CurrentModel();
            if (model is null)
            {
                ShowFields(null);
                return;
            }

            try
            {
                MesCfgJson.Read(model, _savedCfg);
            }
            catch (Exception)
            {
                MesCfgJson.Read(model, "");
                MesError = "MES 配置无法读取";
            }

            ShowFields(model);
        }

        /// <summary>
        /// 按配置类的公共属性生成输入项。
        /// </summary>
        private void ShowFields(MesCfgModel? model)
        {
            MesFields.Clear();
            if (model is null)
                return;

            foreach (var field in MesCfgJson.Fields(model))
                MesFields.Add(new MesFieldItem(field.Name, field.Label, field.Text, field.IsCheck));
        }

        /// <summary>
        /// 把界面文字写回配置类，并按配置类全名保存一行。
        /// </summary>
        private void SaveMes()
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            MesError = "";
            var model = CurrentModel();
            if (model is null || !SqlSugarContext.IsOpen)
            {
                MesError = Text("User_SaveFailed", "保存失败");
                return;
            }

            var rejected = MesCfgJson.Apply(model, MesFields.Select(item => new MesCfgField(item.Name, item.Label, item.Value)).ToList());
            if (rejected is not null)
            {
                MesError = rejected + "无法识别";
                return;
            }

            var typeName = model.GetType().FullName ?? model.GetType().Name;
            var json = MesCfgJson.Write(model);
            try
            {
                var db = SqlSugarContext.CustomerDb;
                var entity = db.Queryable<FlowMesEntity>().ToList()
                    .FirstOrDefault(item => item.ConfigType == typeName);
                if (entity is null)
                {
                    entity = new FlowMesEntity
                    {
                        Id = Guid.NewGuid(),
                        ConfigType = typeName,
                        CfgValue = json
                    };
                    db.Insertable(entity).ExecuteCommand();
                }
                else
                {
                    entity.CfgValue = json;
                    db.Updateable(entity).ExecuteCommand();
                }

                _savedCfg = json;
                ShowFields(model);
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

        /// <summary>
        /// 步骤说明。只从工具配置解释，没有配置时为空。
        /// </summary>
        private static string DetailText(FlowStepEntity step)
        {
            return string.IsNullOrWhiteSpace(step.CfgValue)
                ? ""
                : ToolCatalog.Detail(step.ToolKind, step.CfgValue);
        }

        /// <summary>
        /// 去掉两端空白，并截到指定长度。
        /// </summary>
        private static string Limit(string text, int max)
        {
            var value = text.Trim();
            return value.Length <= max ? value : value[..max];
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
                FlowToolKind.Uph => PackIconPhosphorIconsKind.ChartLine,
                FlowToolKind.Lux_AlarmLogTool => PackIconPhosphorIconsKind.Warning,
                _ => PackIconPhosphorIconsKind.Code
            };
        }
    }
}
