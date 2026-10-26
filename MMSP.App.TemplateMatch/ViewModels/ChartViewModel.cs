using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.WpfBase.Core;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;
using SkiaSharp;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 图表页。卡片定义在客户库，UPH 图读 biz_uph。MES 还没有数据表，有来源时只显示来源名称。
    /// </summary>
    public class ChartViewModel : BaseViewModel
    {
        private static readonly SKTypeface LabelFace = SKTypeface.FromFamilyName("Microsoft YaHei") ?? SKTypeface.Default;
        private static readonly SKColor[] PieColors =
        [
            new(0xFC, 0xC8, 0x00),
            new(0xC4, 0xA3, 0x5A),
            new(0x8C, 0x73, 0x40),
            new(0xE6, 0xD7, 0xA8),
            new(0xA6, 0x8B, 0x4B),
            new(0xF2, 0xE6, 0xB8)
        ];

        private static readonly SolidColorBrush UpBrush = Freeze(Color.FromRgb(0x2F, 0x9E, 0x4F));
        private static readonly SolidColorBrush DownBrush = Freeze(Color.FromRgb(0xCE, 0x3D, 0x3A));
        private static readonly SolidColorBrush DefaultBrush = Freeze(Color.FromRgb(0x88, 0x88, 0x88));

        private enum DrawKind
        {
            Line,
            Column,
            Pie
        }

        private DrawKind _kind = DrawKind.Line;
        private ChartCard? _selectedChart;
        private bool _isEditorOpen;
        private bool _isDataOpen = true;
        private Guid? _editingId;
        private string _editorTitle = "";
        private string _editorName = "";
        private string _editorSourceKind = "";
        private string _editorSource = "";
        private string _editorDisplay = "";
        private string _editorSubtitle = "";
        private string _editorSubtitleStyle = "";
        private string _editorError = "";
        private string _searchText = "";
        private string _appliedSearch = "";
        private string _collapseText = "";
        private GridLength _dataPaneLength = new(1, GridUnitType.Star);
        private bool _loading;

        /// <summary>
        /// 创建图表页。
        /// </summary>
        public ChartViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator)
            : base(region, container, eventAggregator)
        {
            LiveCharts.Configure(config => config.HasTextSettings(new TextSettings
            {
                DefaultTypeface = LabelFace
            }));
            MesSourceName = "";
            if (IContainerProviderExtensions.IsRegistered<CustomerPluginCatalog>(container))
                MesSourceName = container.Resolve<CustomerPluginCatalog>().Mes?.Name ?? "";
            Charts = new ObservableCollection<ChartCard>();
            Points = new ObservableCollection<UphPointRow>();
            CartesianSeries = [];
            PieSeries = [];
            XAxes = [];
            YAxes = [];
            SourceKinds = [Text("Chart_SourceUph"), Text("Chart_SourceMes")];
            SubtitleStyles = [Text("Chart_StyleDefault"), Text("Chart_StyleUp"), Text("Chart_StyleDown")];
            CollapseText = Text("Chart_Collapse");
            AppAppearance.LanguageChanged += OnLanguageChanged;
            AddCommand = new DelegateCommand(OpenAdd);
            EditCommand = new DelegateCommand(OpenEdit);
            DeleteCommand = new DelegateCommand(Delete);
            ConfirmCommand = new DelegateCommand(SaveEditor);
            CancelCommand = new DelegateCommand(CloseEditor);
            SetKindCommand = new DelegateCommand<string?>(SetKind);
            QueryCommand = new DelegateCommand(Query);
            ToggleDataCommand = new DelegateCommand(ToggleData);
        }

        /// <summary>
        /// 左侧卡片。
        /// </summary>
        public ObservableCollection<ChartCard> Charts { get; }

        /// <summary>
        /// 当前图的小时数据。
        /// </summary>
        public ObservableCollection<UphPointRow> Points { get; }

        /// <summary>
        /// 折线或柱状序列。
        /// </summary>
        public ISeries[] CartesianSeries { get; private set; }

        /// <summary>
        /// 饼图序列。
        /// </summary>
        public ISeries[] PieSeries { get; private set; }

        /// <summary>
        /// 横轴。
        /// </summary>
        public Axis[] XAxes { get; private set; }

        /// <summary>
        /// 纵轴。
        /// </summary>
        public Axis[] YAxes { get; private set; }

        /// <summary>
        /// 来源类型下拉。
        /// </summary>
        private IReadOnlyList<string> _sourceKinds = [];

        /// <summary>
        /// 副标题样式下拉。
        /// </summary>
        private IReadOnlyList<string> _subtitleStyles = [];

        /// <summary>
        /// 来源类型下拉。
        /// </summary>
        public IReadOnlyList<string> SourceKinds
        {
            get => _sourceKinds;
            private set => SetProperty(ref _sourceKinds, value);
        }

        /// <summary>
        /// 副标题样式下拉。
        /// </summary>
        public IReadOnlyList<string> SubtitleStyles
        {
            get => _subtitleStyles;
            private set => SetProperty(ref _subtitleStyles, value);
        }

        /// <summary>
        /// 打开新增弹窗。
        /// </summary>
        public DelegateCommand AddCommand { get; }

        /// <summary>
        /// 打开编辑弹窗。用当前选中的卡片。
        /// </summary>
        public DelegateCommand EditCommand { get; }

        /// <summary>
        /// 删除当前选中的卡片。
        /// </summary>
        public DelegateCommand DeleteCommand { get; }

        /// <summary>
        /// 保存弹窗。
        /// </summary>
        public DelegateCommand ConfirmCommand { get; }

        /// <summary>
        /// 关闭弹窗。
        /// </summary>
        public DelegateCommand CancelCommand { get; }

        /// <summary>
        /// 切换折线、柱状或饼图。参数是 Line、Column、Pie。
        /// </summary>
        public DelegateCommand<string?> SetKindCommand { get; }

        /// <summary>
        /// 按搜索框里的时间过滤表格和图。
        /// </summary>
        public DelegateCommand QueryCommand { get; }

        /// <summary>
        /// 折叠或展开下方数据区。
        /// </summary>
        public DelegateCommand ToggleDataCommand { get; }

        /// <summary>
        /// 当前卡片。
        /// </summary>
        public ChartCard? SelectedChart
        {
            get => _selectedChart;
            set
            {
                if (_loading)
                {
                    _selectedChart = value;
                    return;
                }

                if (!SetProperty(ref _selectedChart, value))
                    return;

                RaisePropertyChanged(nameof(SelectedName));
                RaisePropertyChanged(nameof(IsUph));
                RaisePropertyChanged(nameof(IsMes));
                LoadPoints();
            }
        }

        /// <summary>
        /// 图标题。没有选中卡片时为空。
        /// </summary>
        public string SelectedName => _selectedChart?.Name ?? "";

        /// <summary>
        /// 当前卡片是 UPH。
        /// </summary>
        public bool IsUph => _selectedChart?.SourceKind == ChartSourceKind.Uph;

        /// <summary>
        /// 当前卡片是 MES。
        /// </summary>
        public bool IsMes => _selectedChart?.SourceKind == ChartSourceKind.Mes;

        /// <summary>
        /// 当前客户插件登记的 MES 来源名称。没有来源时为空。
        /// </summary>
        public string MesSourceName { get; }

        /// <summary>
        /// 是否显示 MES 来源名称。
        /// </summary>
        public bool HasMesSource => MesSourceName.Length > 0;

        /// <summary>
        /// 当前是折线图。
        /// </summary>
        public bool IsLine => _kind == DrawKind.Line;

        /// <summary>
        /// 当前是柱状图。
        /// </summary>
        public bool IsColumn => _kind == DrawKind.Column;

        /// <summary>
        /// 当前是饼图。
        /// </summary>
        public bool IsPie => _kind == DrawKind.Pie;

        /// <summary>
        /// 下方数据区是否展开。
        /// </summary>
        public bool IsDataOpen
        {
            get => _isDataOpen;
            private set => SetProperty(ref _isDataOpen, value);
        }

        /// <summary>
        /// 下方数据区高度。展开时与图各占一半，折叠时只留表头。
        /// </summary>
        public GridLength DataPaneLength
        {
            get => _dataPaneLength;
            private set => SetProperty(ref _dataPaneLength, value);
        }

        /// <summary>
        /// 语言切换后重写下拉文字，并按原来的位置保住当前选项。
        /// </summary>
        private void OnLanguageChanged()
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is not null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(OnLanguageChanged));
                return;
            }

            var sourceIndex = ChoiceIndex(SourceKinds, EditorSourceKind);
            var styleIndex = ChoiceIndex(SubtitleStyles, EditorSubtitleStyle);
            SourceKinds = [Text("Chart_SourceUph"), Text("Chart_SourceMes")];
            SubtitleStyles = [Text("Chart_StyleDefault"), Text("Chart_StyleUp"), Text("Chart_StyleDown")];
            if (sourceIndex >= 0 && sourceIndex < SourceKinds.Count)
                EditorSourceKind = SourceKinds[sourceIndex];
            if (styleIndex >= 0 && styleIndex < SubtitleStyles.Count)
                EditorSubtitleStyle = SubtitleStyles[styleIndex];
            CollapseText = IsDataOpen ? Text("Chart_Collapse") : Text("Chart_Expand");
        }

        /// <summary>
        /// 在下拉里找当前文字的位置。没有时返回 -1。
        /// </summary>
        private static int ChoiceIndex(IReadOnlyList<string> choices, string value)
        {
            for (var index = 0; index < choices.Count; index++)
            {
                if (choices[index] == value)
                    return index;
            }

            return -1;
        }

        /// <summary>
        /// 折叠按钮文字。
        /// </summary>
        public string CollapseText
        {
            get => _collapseText;
            private set => SetProperty(ref _collapseText, value);
        }

        /// <summary>
        /// 搜索框文字。点查询后才生效。
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
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
        /// 弹窗标题。
        /// </summary>
        public string EditorTitle
        {
            get => _editorTitle;
            private set => SetProperty(ref _editorTitle, value);
        }

        /// <summary>
        /// 名称。
        /// </summary>
        public string EditorName
        {
            get => _editorName;
            set => SetProperty(ref _editorName, value);
        }

        /// <summary>
        /// 来源类型文字。
        /// </summary>
        public string EditorSourceKind
        {
            get => _editorSourceKind;
            set => SetProperty(ref _editorSourceKind, value);
        }

        /// <summary>
        /// 来源键。
        /// </summary>
        public string EditorSource
        {
            get => _editorSource;
            set => SetProperty(ref _editorSource, value);
        }

        /// <summary>
        /// 显示值。
        /// </summary>
        public string EditorDisplay
        {
            get => _editorDisplay;
            set => SetProperty(ref _editorDisplay, value);
        }

        /// <summary>
        /// 副标题。
        /// </summary>
        public string EditorSubtitle
        {
            get => _editorSubtitle;
            set => SetProperty(ref _editorSubtitle, value);
        }

        /// <summary>
        /// 副标题样式文字。
        /// </summary>
        public string EditorSubtitleStyle
        {
            get => _editorSubtitleStyle;
            set => SetProperty(ref _editorSubtitleStyle, value);
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
        /// 进入页面时读取卡片。
        /// </summary>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadCharts(_selectedChart?.Id);
        }

        /// <summary>
        /// 从客户库填充左侧卡片，并选中指定的一张。
        /// </summary>
        private void LoadCharts(Guid? selectId)
        {
            _loading = true;
            Charts.Clear();
            if (SqlSugarContext.IsOpen)
            {
                var rows = SqlSugarContext.CustomerDb.Queryable<ChartEntity>()
                    .OrderBy(item => item.SortIndex)
                    .ToList();
                foreach (var row in rows)
                    Charts.Add(ToCard(row));
            }

            _loading = false;
            SelectedChart = Charts.FirstOrDefault(item => item.Id == selectId) ?? Charts.FirstOrDefault();
        }

        /// <summary>
        /// 读取当前卡片对应的点。UPH 读 biz_uph，MES 留空。
        /// </summary>
        private void LoadPoints()
        {
            Points.Clear();
            if (IsUph && SqlSugarContext.IsOpen)
            {
                var rows = SqlSugarContext.CustomerDb.Queryable<UphEntity>()
                    .OrderBy(item => item.HourTime)
                    .ToList();
                var keyword = _appliedSearch.Trim();
                foreach (var row in rows)
                {
                    var time = row.HourTime.ToString("HH:mm", CultureInfo.InvariantCulture);
                    var full = row.HourTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    if (keyword.Length > 0
                        && time.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0
                        && full.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    Points.Add(ToPoint(row, time));
                }
            }

            RebuildChart();
        }

        /// <summary>
        /// 按当前图形类型重画。
        /// </summary>
        private void RebuildChart()
        {
            var labels = Points.Select(item => item.TimeText).ToList();
            var values = Points.Select(item => item.HourOutput).ToArray();
            var name = SelectedName;
            CartesianSeries = _kind switch
            {
                DrawKind.Column => [Column(values, name)],
                DrawKind.Line => [Line(values, name)],
                _ => []
            };
            PieSeries = _kind == DrawKind.Pie ? Pie(values, labels) : [];
            XAxes =
            [
                new Axis
                {
                    Labels = labels,
                    LabelsPaint = TextPaint(),
                    TextSize = 12,
                    SeparatorsPaint = GridPaint(),
                    TicksPaint = GridPaint()
                }
            ];
            YAxes =
            [
                new Axis
                {
                    MinLimit = 0,
                    LabelsPaint = TextPaint(),
                    TextSize = 12,
                    SeparatorsPaint = GridPaint()
                }
            ];
            RaisePropertyChanged(nameof(CartesianSeries));
            RaisePropertyChanged(nameof(PieSeries));
            RaisePropertyChanged(nameof(XAxes));
            RaisePropertyChanged(nameof(YAxes));
        }

        /// <summary>
        /// 折线面积。
        /// </summary>
        private static LineSeries<double> Line(double[] values, string name)
        {
            return new LineSeries<double>
            {
                Name = name,
                Values = values,
                Stroke = new SolidColorPaint(new SKColor(0xA6, 0x84, 0x3D)) { StrokeThickness = 2 },
                Fill = new SolidColorPaint(new SKColor(0xC4, 0xB0, 0x7A, 150)),
                GeometrySize = 0,
                LineSmoothness = 0.35,
                GeometryFill = null,
                GeometryStroke = null
            };
        }

        /// <summary>
        /// 柱状。
        /// </summary>
        private static ColumnSeries<double> Column(double[] values, string name)
        {
            return new ColumnSeries<double>
            {
                Name = name,
                Values = values,
                Fill = new SolidColorPaint(new SKColor(0xC4, 0xA3, 0x5A)),
                Stroke = null,
                MaxBarWidth = 28
            };
        }

        /// <summary>
        /// 每个小时一块。产量为 0 的小时不画。
        /// </summary>
        private static ISeries[] Pie(double[] values, IReadOnlyList<string> labels)
        {
            var series = new List<ISeries>();
            for (var index = 0; index < values.Length; index++)
            {
                if (values[index] <= 0)
                    continue;

                var color = PieColors[index % PieColors.Length];
                series.Add(new PieSeries<double>
                {
                    Name = index < labels.Count ? labels[index] : "",
                    Values = [values[index]],
                    Fill = new SolidColorPaint(color),
                    Stroke = null
                });
            }

            return series.ToArray();
        }

        /// <summary>
        /// 坐标文字。
        /// </summary>
        private static SolidColorPaint TextPaint()
        {
            return new SolidColorPaint(new SKColor(0x88, 0x88, 0x88))
            {
                SKTypeface = LabelFace
            };
        }

        /// <summary>
        /// 网格线。
        /// </summary>
        private static SolidColorPaint GridPaint()
        {
            return new SolidColorPaint(new SKColor(0xE4, 0xE4, 0xE4));
        }

        /// <summary>
        /// 切换图形。
        /// </summary>
        private void SetKind(string? kind)
        {
            _kind = kind switch
            {
                "Column" => DrawKind.Column,
                "Pie" => DrawKind.Pie,
                _ => DrawKind.Line
            };
            RaisePropertyChanged(nameof(IsLine));
            RaisePropertyChanged(nameof(IsColumn));
            RaisePropertyChanged(nameof(IsPie));
            RebuildChart();
        }

        /// <summary>
        /// 用搜索框的内容重新过滤。
        /// </summary>
        private void Query()
        {
            _appliedSearch = SearchText.Trim();
            LoadPoints();
        }

        /// <summary>
        /// 折叠或展开数据区。表头保留。
        /// </summary>
        private void ToggleData()
        {
            IsDataOpen = !IsDataOpen;
            DataPaneLength = IsDataOpen ? new GridLength(1, GridUnitType.Star) : new GridLength(56);
            CollapseText = IsDataOpen ? Text("Chart_Collapse") : Text("Chart_Expand");
        }

        /// <summary>
        /// 打开空白新增弹窗。来源类型默认 UPH，副标题样式默认。
        /// </summary>
        private void OpenAdd()
        {
            _editingId = null;
            EditorTitle = Text("Chart_AddTitle");
            EditorName = "";
            EditorSourceKind = SourceKinds[0];
            EditorSource = "";
            EditorDisplay = "";
            EditorSubtitle = "";
            EditorSubtitleStyle = SubtitleStyles[0];
            EditorError = "";
            IsEditorOpen = true;
        }

        /// <summary>
        /// 把当前卡片带回编辑弹窗。没有选中时不打开。
        /// </summary>
        private void OpenEdit()
        {
            if (_selectedChart is null)
                return;

            _editingId = _selectedChart.Id;
            EditorTitle = Text("Chart_EditTitle");
            EditorName = _selectedChart.Name;
            EditorSourceKind = SourceText(_selectedChart.SourceKind);
            EditorSource = _selectedChart.Source;
            EditorDisplay = _selectedChart.DisplayText;
            EditorSubtitle = _selectedChart.Subtitle;
            EditorSubtitleStyle = StyleText(_selectedChart.SubtitleStyle);
            EditorError = "";
            IsEditorOpen = true;
        }

        /// <summary>
        /// 确认后删除当前卡片。不删除 UPH 小时数据。
        /// </summary>
        private async void Delete()
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            if (_selectedChart is null || !SqlSugarContext.IsOpen)
                return;

            var confirmed = await MetroDialogService.ConfirmAsync(Text("Chart_DeleteTitle"), Text("Chart_DeleteMessage"));
            if (!confirmed)
                return;

            var id = _selectedChart.Id;
            SqlSugarContext.CustomerDb.Deleteable<ChartEntity>()
                .Where(item => item.Id == id)
                .ExecuteCommand();
            LoadCharts(null);
        }

        /// <summary>
        /// 校验后写入客户库。
        /// </summary>
        private void SaveEditor()
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            if (!TrySave(out var name, out var id, out var created))
                return;

            LogHelper.SysInfo(created ? $"新增图表 {name}" : $"编辑图表 {name}");
            CloseEditor();
            LoadCharts(id);
        }

        /// <summary>
        /// 新增或更新一条图表定义。
        /// </summary>
        private bool TrySave(out string name, out Guid id, out bool created)
        {
            name = Limit(EditorName.Trim(), 16);
            id = _editingId ?? Guid.Empty;
            created = _editingId is null;
            EditorError = "";

            if (name.Length == 0)
            {
                EditorError = Text("Chart_NeedName");
                return false;
            }

            if (!SqlSugarContext.IsOpen)
            {
                EditorError = Text("User_SaveFailed");
                return false;
            }

            try
            {
                var db = SqlSugarContext.CustomerDb;
                var source = Limit(EditorSource.Trim(), 64);
                var display = Limit(EditorDisplay.Trim(), 12);
                var subtitle = Limit(EditorSubtitle.Trim(), 24);
                var kind = ReadKind();
                var style = ReadStyle();

                if (_editingId is Guid editingId)
                {
                    var entity = db.Queryable<ChartEntity>().Where(item => item.Id == editingId).ToList().FirstOrDefault();
                    if (entity is null)
                    {
                        EditorError = Text("User_SaveFailed");
                        return false;
                    }

                    entity.Name = name;
                    entity.SourceKind = kind;
                    entity.Source = source;
                    entity.DisplayText = display;
                    entity.Subtitle = subtitle;
                    entity.SubtitleStyle = style;
                    db.Updateable(entity).ExecuteCommand();
                    id = entity.Id;
                }
                else
                {
                    var sortValues = db.Queryable<ChartEntity>().Select(item => item.SortIndex).ToList();
                    id = Guid.NewGuid();
                    db.Insertable(new ChartEntity
                    {
                        Id = id,
                        Name = name,
                        SourceKind = kind,
                        Source = source,
                        DisplayText = display,
                        Subtitle = subtitle,
                        SubtitleStyle = style,
                        SortIndex = sortValues.Count == 0 ? 0 : sortValues.Max() + 1
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
        /// 关闭弹窗。
        /// </summary>
        private void CloseEditor()
        {
            IsEditorOpen = false;
            EditorError = "";
        }

        /// <summary>
        /// 实体转卡片。
        /// </summary>
        private static ChartCard ToCard(ChartEntity entity)
        {
            return new ChartCard
            {
                Id = entity.Id,
                Name = entity.Name,
                SourceKind = entity.SourceKind,
                Source = entity.Source,
                DisplayText = entity.DisplayText,
                Subtitle = entity.Subtitle,
                SubtitleStyle = entity.SubtitleStyle,
                SubtitleBrush = BrushOf(entity.SubtitleStyle),
                SortIndex = entity.SortIndex
            };
        }

        /// <summary>
        /// 实体转表格行。
        /// </summary>
        private static UphPointRow ToPoint(UphEntity entity, string time)
        {
            var denom = entity.GoodCount + entity.DefectCount;
            var rate = denom == 0 ? 0d : entity.GoodCount * 100d / denom;
            return new UphPointRow
            {
                HourTime = entity.HourTime,
                TimeText = time,
                HourOutput = entity.HourOutput,
                HourOutputText = entity.HourOutput.ToString("N0", CultureInfo.CurrentCulture),
                TotalOutputText = entity.TotalOutput.ToString("N0", CultureInfo.CurrentCulture),
                GoodCountText = entity.GoodCount.ToString("N0", CultureInfo.CurrentCulture),
                DefectCountText = entity.DefectCount.ToString("N0", CultureInfo.CurrentCulture),
                YieldText = rate.ToString("0.0", CultureInfo.CurrentCulture) + "%"
            };
        }

        /// <summary>
        /// 副标题颜色。
        /// </summary>
        private static Brush BrushOf(ChartSubtitleStyle style)
        {
            return style switch
            {
                ChartSubtitleStyle.Up => UpBrush,
                ChartSubtitleStyle.Down => DownBrush,
                _ => DefaultBrush
            };
        }

        /// <summary>
        /// 来源类型在界面上的文字。
        /// </summary>
        private static string SourceText(ChartSourceKind kind)
        {
            return kind == ChartSourceKind.Mes ? Text("Chart_SourceMes") : Text("Chart_SourceUph");
        }

        /// <summary>
        /// 副标题样式在界面上的文字。
        /// </summary>
        private static string StyleText(ChartSubtitleStyle style)
        {
            return style switch
            {
                ChartSubtitleStyle.Up => Text("Chart_StyleUp"),
                ChartSubtitleStyle.Down => Text("Chart_StyleDown"),
                _ => Text("Chart_StyleDefault")
            };
        }

        /// <summary>
        /// 从下拉文字读来源类型。认不出时按 UPH。
        /// </summary>
        private ChartSourceKind ReadKind()
        {
            return EditorSourceKind == Text("Chart_SourceMes") ? ChartSourceKind.Mes : ChartSourceKind.Uph;
        }

        /// <summary>
        /// 从下拉文字读副标题样式。认不出时按默认。
        /// </summary>
        private ChartSubtitleStyle ReadStyle()
        {
            if (EditorSubtitleStyle == Text("Chart_StyleUp"))
                return ChartSubtitleStyle.Up;
            if (EditorSubtitleStyle == Text("Chart_StyleDown"))
                return ChartSubtitleStyle.Down;
            return ChartSubtitleStyle.Default;
        }

        /// <summary>
        /// 截断到效果图上的长度。
        /// </summary>
        private static string Limit(string value, int length)
        {
            return value.Length <= length ? value : value[..length];
        }

        /// <summary>
        /// 冻结画刷，供卡片反复使用。
        /// </summary>
        private static SolidColorBrush Freeze(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
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
