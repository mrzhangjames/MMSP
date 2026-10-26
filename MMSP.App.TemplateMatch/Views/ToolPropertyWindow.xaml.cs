using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MMSP.App.TemplateMatch.Services;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool;
using MMSP.Plugin.Tool.ViewModels;
using MMSP.Service.Database;

namespace MMSP.App.TemplateMatch.Views
{
    /// <summary>
    /// 流程步的工具属性窗口。使用 Metro 窗口，确定后把配置交回流程页。
    /// </summary>
    public partial class ToolPropertyWindow : MetroWindow
    {
        /// <summary>
        /// 正在编辑的工具种类。
        /// </summary>
        private readonly FlowToolKind _kind;

        /// <summary>
        /// 工具名称为空时用的标题。
        /// </summary>
        private readonly string _fallbackTitle;

        /// <summary>
        /// 当前窗口能否保存。当前客户没有这个工具时为 false。
        /// </summary>
        private readonly bool _editable;

        /// <summary>
        /// 工具表单上的属性变化。用来在改条码数量或 Json 结果时刷新左右两列。
        /// </summary>
        private INotifyPropertyChanged? _toolChanges;

        /// <summary>
        /// 打开一种工具的配置。fallbackTitle 是种类名称，工具名称为空时用它。
        /// 客户工具从已登记的插件取界面。当前客户没有这个工具时只提示，不能保存。
        /// variables 是本流程的变量名。priorOutputs 是前序步骤已经占用的出参。
        /// </summary>
        public ToolPropertyWindow(
            FlowToolKind kind,
            string cfg,
            string fallbackTitle,
            CustomerPluginCatalog? catalog,
            IReadOnlyList<string>? variables = null,
            IReadOnlySet<string>? priorOutputs = null)
        {
            _kind = kind;
            _fallbackTitle = fallbackTitle;
            InitializeComponent();
            if (catalog?.Create(kind, cfg) is UserControl customer)
            {
                if (kind == FlowToolKind.Lux_AlarmLogTool)
                    UseAlarmSize();

                HideSides();
                Body.Content = customer;
                _editable = true;
                return;
            }

            if (IsGeneral(kind))
            {
                var scanners = kind is FlowToolKind.SingleScan or FlowToolKind.MultiScan ? ReadScanners() : [];
                var points = kind is FlowToolKind.PlcRead or FlowToolKind.PlcWrite or FlowToolKind.PlcMonitor ? ReadPoints() : [];
                Body.Content = ToolCatalog.Create(kind, cfg, scanners, points);
                if (kind is FlowToolKind.PlcMonitor or FlowToolKind.Delay)
                {
                    HideSides();
                    UseFormSize();
                }
                else
                    AttachSides(variables ?? [], priorOutputs ?? new HashSet<string>(StringComparer.Ordinal));

                _editable = true;
                return;
            }

            HideSides();
            Width = 440;
            MinWidth = 440;
            MaxWidth = 440;
            Height = 240;
            MinHeight = 240;
            MaxHeight = 240;
            _editable = false;
            SaveButton.IsEnabled = false;
            Body.Content = new TextBlock
            {
                Text = UnavailableText(),
                Margin = new Thickness(20, 16, 20, 16),
                TextWrapping = TextWrapping.Wrap
            };
        }

        /// <summary>
        /// 通用工具是枚举里的 0 到 9。
        /// </summary>
        private static bool IsGeneral(FlowToolKind kind)
        {
            return (int)kind <= (int)FlowToolKind.Uph;
        }

        /// <summary>
        /// 通用工具加上左右两列。中间仍是原来的表单。
        /// </summary>
        private void AttachSides(IReadOnlyList<string> variables, IReadOnlySet<string> priorOutputs)
        {
            if (Body.Content is not UserControl view || view.DataContext?.GetType().GetProperty("EditedTool")?.GetValue(view.DataContext) is not PluginToolModel tool)
            {
                HideSides();
                return;
            }

            var model = new ToolIoModel(
                _kind,
                tool,
                variables,
                priorOutputs,
                name => SetOutput(view.DataContext, tool, name),
                name => SetInput(view.DataContext, name),
                () => RefreshScanResults(view.DataContext));
            Panes.DataContext = model;
            if (view.DataContext is INotifyPropertyChanged changes)
            {
                _toolChanges = changes;
                changes.PropertyChanged += OnToolChanged;
            }
        }

        /// <summary>
        /// 写回单出参。Json、PLC 读取和单扫码的结果框与出参是同一个名称。
        /// </summary>
        private static void SetOutput(object viewModel, PluginToolModel tool, string name)
        {
            if (viewModel is JsonParseToolViewModel json)
            {
                json.Target = name;
                return;
            }

            if (viewModel is PlcReadToolViewModel read)
            {
                read.Result = name;
                return;
            }

            if (viewModel is SingleScanToolViewModel scan)
            {
                scan.Result = name;
                return;
            }

            ToolVars.SetOutput(tool, name);
        }

        /// <summary>
        /// 多扫码出参变化后重画中间的结果表。
        /// </summary>
        private static void RefreshScanResults(object viewModel)
        {
            if (viewModel is MultiScanToolViewModel multi)
                multi.RefreshResults();
        }

        /// <summary>
        /// 写回点选的入参名。PLC 写入先改成变量。JSON 解析写进来源。
        /// </summary>
        private static void SetInput(object viewModel, string name)
        {
            var text = name ?? "";
            if (viewModel is PlcWriteToolViewModel write)
            {
                if (!write.ValueFromVariable)
                    write.ValueMode = PlcWriteToolViewModel.VariableValueMode;

                write.Value = text;
                return;
            }

            if (viewModel is JsonParseToolViewModel json)
                json.Source = text;

            if (viewModel is UphToolViewModel uph)
                uph.Variable = text;
        }

        /// <summary>
        /// 条码数量、扫码枪、结果或写入值变化后，按工具上的值重画左右两列。
        /// </summary>
        private void OnToolChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is not ("Count" or "Scanner" or "Target" or "Result" or "Value" or "ValueMode" or "Source" or "Variable"))
                return;

            if (Panes.DataContext is ToolIoModel model)
                model.Refresh();
        }

        /// <summary>
        /// 没有入参和出参时，窗口只留中间表单的宽度。
        /// </summary>
        private void UseFormSize()
        {
            const double width = 460;
            Width = width;
            MinWidth = width;
            MaxWidth = width;
        }

        /// <summary>
        /// 左右两列收起。报警表、PLC 监控、延时和不可用提示只用中间。
        /// </summary>
        private void HideSides()
        {
            InputColumn.Width = new GridLength(0);
            OutputColumn.Width = new GridLength(0);
            InputPane.Visibility = Visibility.Collapsed;
            OutputPane.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 立讯报警表格需要可调整的大窗口。
        /// </summary>
        private void UseAlarmSize()
        {
            Width = 760;
            MinWidth = 640;
            MaxWidth = 1600;
            Height = 520;
            MinHeight = 360;
            MaxHeight = 900;
            SizeToContent = SizeToContent.Manual;
            ResizeMode = ResizeMode.CanResize;
        }

        /// <summary>
        /// 当前客户没有这个工具时的提示。
        /// </summary>
        private static string UnavailableText()
        {
            if (Application.Current?.TryFindResource("Flow_ToolUnavailable") is string value && value.Length > 0)
                return value;

            return "当前客户未加载此工具";
        }

        /// <summary>
        /// 确定后的配置。取消时为空。
        /// </summary>
        public ToolEditResult? Result { get; private set; }

        /// <summary>
        /// 保存窗口里的工具配置并关闭。
        /// </summary>
        private void Accept(object sender, RoutedEventArgs e)
        {
            if (!_editable)
                return;

            if (Body.Content is ICustomerToolView customer)
                customer.CommitEdit();

            if (Body.Content is not UserControl view)
                return;

            var cfg = ToolCatalog.Config(view);
            Result = new ToolEditResult
            {
                Title = Limit(ToolCatalog.Title(_kind, cfg, _fallbackTitle)),
                Detail = ToolCatalog.Detail(_kind, cfg),
                CfgValue = cfg
            };
            DialogResult = true;
        }

        /// <summary>
        /// 按排序读取点位。地址为空的不进入下拉。库没打开或读取失败时返回空列表。
        /// </summary>
        private static IReadOnlyList<PlcPointChoice> ReadPoints()
        {
            if (!SqlSugarContext.IsOpen)
                return [];

            try
            {
                return SqlSugarContext.CustomerDb.Queryable<PlcPointEntity>()
                    .OrderBy(item => item.SortIndex)
                    .ToList()
                    .Where(item => !string.IsNullOrWhiteSpace(item.Address))
                    .Select(item => new PlcPointChoice
                    {
                        Name = (item.Name ?? "").Trim(),
                        Address = item.Address.Trim(),
                        DataType = item.DataType,
                        Length = item.DataLength,
                        Kind = PlcPointEntity.NormalizeKind(item.PointKind)
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                LogHelper.PlcError("读取点位失败", ex);
                return [];
            }
        }

        /// <summary>
        /// 按排序读取扫码枪名称。库没打开或读取失败时返回空列表，窗口仍可编辑。
        /// </summary>
        private static IReadOnlyList<string> ReadScanners()
        {
            if (!SqlSugarContext.IsOpen)
                return [];

            try
            {
                return SqlSugarContext.CustomerDb.Queryable<ScannerEntity>()
                    .OrderBy(item => item.SortIndex)
                    .ToList()
                    .Select(item => item.Name.Trim())
                    .Where(item => item.Length > 0)
                    .Distinct()
                    .ToList();
            }
            catch (Exception ex)
            {
                LogHelper.ScanError("读取扫码枪失败", ex);
                return [];
            }
        }

        /// <summary>
        /// 标题最长 32，和流程步表一致。
        /// </summary>
        private static string Limit(string text)
        {
            var value = text.Trim();
            return value.Length <= 32 ? value : value[..32];
        }
    }

    /// <summary>
    /// 属性窗口确认后写回流程步的内容。
    /// </summary>
    public sealed class ToolEditResult
    {
        /// <summary>
        /// 步骤标题。
        /// </summary>
        public string Title { get; init; } = "";

        /// <summary>
        /// 步骤说明。
        /// </summary>
        public string Detail { get; init; } = "";

        /// <summary>
        /// 工具配置。
        /// </summary>
        public string CfgValue { get; init; } = "";
    }
}
