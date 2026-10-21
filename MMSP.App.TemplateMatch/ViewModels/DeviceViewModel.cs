using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.WpfBase.Core;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;
using MMSP.Service.Device.Scan;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 设备页。配置、点位和扫码枪写在客户库，读写和触发走设备服务。
    /// </summary>
    public partial class DeviceViewModel : BaseViewModel
    {
        private enum EditorKind
        {
            None,
            Point,
            Scanner
        }

        private bool _isPlcConfigTab = true;
        private EditorKind _editor = EditorKind.None;
        private Guid? _editingId;
        private string _editorTitle = "";
        private string _editorError = "";
        private string _editorName = "";
        private string _editorAddress = "";
        private string _editorDataType = "";
        private string _editorLength = "";
        private string _editorAccess = "";
        private string _editorIp = "";
        private string _editorPort = "";
        private string _editorTrigger = "";
        private string _editorTransport = "";
        private bool _editorEnabled = true;
        private PlcPointRow? _selectedPoint;
        private ScannerCard? _selectedScanner;
        private string _plcIp = "192.168.1.10";
        private string _plcConnectionType = "TCP";
        private string _plcPort = "502";
        private string _plcDataFormat = "ABCD";
        private string _plcPoll = "200 ms";
        private string _plcReadTimeout = "1000 ms";
        private string _plcWriteTimeout = "1000 ms";
        private string _plcReconnectInterval = "3000 ms";
        private string _plcReconnectCount = "3";
        private string _testAddress = "";
        private string _testDataType = "Int16";
        private string _testLength = "1";
        private string _testWriteValue = "";
        private string _currentValue = "";
        private string _currentDetail = "";
        private string _scanResult = "";

        /// <summary>
        /// 创建设备页。点位和扫码枪在进入页面时从客户库加载。
        /// </summary>
        public DeviceViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator)
            : base(region, container, eventAggregator)
        {
            Points = new ObservableCollection<PlcPointRow>();
            Scanners = new ObservableCollection<ScannerCard>();
            PlcLogs = new ObservableCollection<DeviceLogLine>();
            ScanLogs = new ObservableCollection<DeviceLogLine>();
            ConnectionTypes = ["TCP"];
            DataFormats = ["ABCD", "CDAB", "BADC", "DCBA"];
            DataTypes = ["Int16", "Int32", "Float", "String"];
            AccessModes = [Text("Device_AccessReadWrite"), Text("Device_AccessRead")];
            ScanTransports = [Text("Device_Tcp"), Text("Device_Serial")];
            AppAppearance.LanguageChanged += OnLanguageChanged;
            EditorDataType = DataTypes[0];
            EditorAccess = AccessModes[0];
            EditorTransport = ScanTransports[0];

            ShowPlcConfigCommand = new DelegateCommand(() => IsPlcConfigTab = true);
            ShowPlcTestCommand = new DelegateCommand(() => IsPlcConfigTab = false);
            AddPointCommand = new DelegateCommand(OpenAddPoint);
            EditPointCommand = new DelegateCommand(OpenEditPoint);
            DeletePointCommand = new DelegateCommand(DeletePoints);
            ImportPointsCommand = new DelegateCommand(ImportPoints);
            ExportPointsCommand = new DelegateCommand(ExportPoints);
            AddScannerCommand = new DelegateCommand(OpenAddScanner);
            EditScannerCommand = new DelegateCommand(OpenEditScanner);
            DeleteScannerCommand = new DelegateCommand(DeleteScanner);
            ConfirmEditorCommand = new DelegateCommand(SaveEditor);
            CancelEditorCommand = new DelegateCommand(CloseEditor);
            ReadCustomCommand = new DelegateCommand(ReadCustom);
            WriteCustomCommand = new DelegateCommand(WriteCustom);
            ReadPointCommand = new DelegateCommand(ReadSelectedPoint);
            WritePointCommand = new DelegateCommand(WriteSelectedPoint);
            TriggerCommand = new DelegateCommand(TriggerScanner);
        }

        /// <summary>
        /// 点位列表。
        /// </summary>
        public ObservableCollection<PlcPointRow> Points { get; }

        /// <summary>
        /// 扫码枪卡片。
        /// </summary>
        public ObservableCollection<ScannerCard> Scanners { get; }

        /// <summary>
        /// PLC 日志。
        /// </summary>
        public ObservableCollection<DeviceLogLine> PlcLogs { get; }

        /// <summary>
        /// 扫码枪日志。
        /// </summary>
        public ObservableCollection<DeviceLogLine> ScanLogs { get; }

        /// <summary>
        /// 连接类型下拉项。
        /// </summary>
        public IReadOnlyList<string> ConnectionTypes { get; }

        /// <summary>
        /// 数据格式下拉项。
        /// </summary>
        public IReadOnlyList<string> DataFormats { get; }

        /// <summary>
        /// 数据类型下拉项。
        /// </summary>
        public IReadOnlyList<string> DataTypes { get; }

        /// <summary>
        /// 读写下拉项。
        /// </summary>
        private IReadOnlyList<string> _accessModes = [];

        /// <summary>
        /// 扫码传输下拉项。TCP 和串口。
        /// </summary>
        private IReadOnlyList<string> _scanTransports = [];

        /// <summary>
        /// 读写下拉项。
        /// </summary>
        public IReadOnlyList<string> AccessModes
        {
            get => _accessModes;
            private set => SetProperty(ref _accessModes, value);
        }

        /// <summary>
        /// 扫码传输下拉项。TCP 和串口。
        /// </summary>
        public IReadOnlyList<string> ScanTransports
        {
            get => _scanTransports;
            private set => SetProperty(ref _scanTransports, value);
        }

        /// <summary>
        /// 语言切换后重写读写、传输和启用文字，并保住当前选项。
        /// </summary>
        private void OnLanguageChanged()
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is not null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(OnLanguageChanged));
                return;
            }

            var accessIndex = ChoiceIndex(AccessModes, EditorAccess);
            var transportIndex = ChoiceIndex(ScanTransports, EditorTransport);
            var previousAccess = AccessModes;
            var readWrite = Text("Device_AccessReadWrite");
            var readOnly = Text("Device_AccessRead");
            var enabledOn = Text("Device_On");
            var enabledOff = Text("Device_Off");
            foreach (var point in Points)
            {
                var index = ChoiceIndex(previousAccess, point.AccessText);
                point.UseEnabledLabels(enabledOn, enabledOff, point.IsEnabled);
                if (index == 0)
                    point.AccessText = readWrite;
                else if (index == 1)
                    point.AccessText = readOnly;
            }

            foreach (var card in Scanners)
                card.UseEnabledLabels(enabledOn, enabledOff, card.IsEnabled);

            AccessModes = [readWrite, readOnly];
            ScanTransports = [Text("Device_Tcp"), Text("Device_Serial")];
            if (accessIndex >= 0 && accessIndex < AccessModes.Count)
                EditorAccess = AccessModes[accessIndex];
            if (transportIndex >= 0 && transportIndex < ScanTransports.Count)
                EditorTransport = ScanTransports[transportIndex];
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
        /// 切到 PLC 配置。
        /// </summary>
        public DelegateCommand ShowPlcConfigCommand { get; }

        /// <summary>
        /// 切到 PLC 测试。
        /// </summary>
        public DelegateCommand ShowPlcTestCommand { get; }

        /// <summary>
        /// 新增点位。
        /// </summary>
        public DelegateCommand AddPointCommand { get; }

        /// <summary>
        /// 编辑勾选或当前点位。
        /// </summary>
        public DelegateCommand EditPointCommand { get; }

        /// <summary>
        /// 删除勾选的点位。
        /// </summary>
        public DelegateCommand DeletePointCommand { get; }

        /// <summary>
        /// 从 CSV 导入点位，替换当前列表。
        /// </summary>
        public DelegateCommand ImportPointsCommand { get; }

        /// <summary>
        /// 把当前点位导出为 CSV。
        /// </summary>
        public DelegateCommand ExportPointsCommand { get; }

        /// <summary>
        /// 新增扫码枪。
        /// </summary>
        public DelegateCommand AddScannerCommand { get; }

        /// <summary>
        /// 编辑当前扫码枪。
        /// </summary>
        public DelegateCommand EditScannerCommand { get; }

        /// <summary>
        /// 删除当前扫码枪。
        /// </summary>
        public DelegateCommand DeleteScannerCommand { get; }

        /// <summary>
        /// 保存弹窗。
        /// </summary>
        public DelegateCommand ConfirmEditorCommand { get; }

        /// <summary>
        /// 关闭弹窗。
        /// </summary>
        public DelegateCommand CancelEditorCommand { get; }

        /// <summary>
        /// 按测试区手填的地址、类型、长度读取。
        /// </summary>
        public DelegateCommand ReadCustomCommand { get; }

        /// <summary>
        /// 按测试区手填的地址和写入值写入。
        /// </summary>
        public DelegateCommand WriteCustomCommand { get; }

        /// <summary>
        /// 右键读取当前点位。
        /// </summary>
        public DelegateCommand ReadPointCommand { get; }

        /// <summary>
        /// 右键写入当前点位。
        /// </summary>
        public DelegateCommand WritePointCommand { get; }

        /// <summary>
        /// 向当前扫码枪发触发字符串。
        /// </summary>
        public DelegateCommand TriggerCommand { get; }

        /// <summary>
        /// 当前是 PLC 配置页。
        /// </summary>
        public bool IsPlcConfigTab
        {
            get => _isPlcConfigTab;
            private set
            {
                if (!SetProperty(ref _isPlcConfigTab, value))
                    return;

                RaisePropertyChanged(nameof(IsPlcTestTab));
            }
        }

        /// <summary>
        /// 当前是 PLC 测试页。
        /// </summary>
        public bool IsPlcTestTab => !IsPlcConfigTab;

        /// <summary>
        /// 弹窗是否打开。
        /// </summary>
        public bool IsEditorOpen => _editor != EditorKind.None;

        /// <summary>
        /// 点位弹窗是否打开。
        /// </summary>
        public bool IsPointEditor => _editor == EditorKind.Point;

        /// <summary>
        /// 扫码枪弹窗是否打开。
        /// </summary>
        public bool IsScannerEditor => _editor == EditorKind.Scanner;

        /// <summary>
        /// 弹窗标题。
        /// </summary>
        public string EditorTitle
        {
            get => _editorTitle;
            private set => SetProperty(ref _editorTitle, value);
        }

        /// <summary>
        /// 弹窗错误。空则不显示。
        /// </summary>
        public string EditorError
        {
            get => _editorError;
            private set => SetProperty(ref _editorError, value);
        }

        /// <summary>
        /// 弹窗名称。点位名称或扫码枪名称。
        /// </summary>
        public string EditorName
        {
            get => _editorName;
            set => SetProperty(ref _editorName, value);
        }

        /// <summary>
        /// 弹窗地址。
        /// </summary>
        public string EditorAddress
        {
            get => _editorAddress;
            set => SetProperty(ref _editorAddress, value);
        }

        /// <summary>
        /// 弹窗数据类型。
        /// </summary>
        public string EditorDataType
        {
            get => _editorDataType;
            set => SetProperty(ref _editorDataType, value);
        }

        /// <summary>
        /// 弹窗长度。
        /// </summary>
        public string EditorLength
        {
            get => _editorLength;
            set => SetProperty(ref _editorLength, value);
        }

        /// <summary>
        /// 弹窗读写。
        /// </summary>
        public string EditorAccess
        {
            get => _editorAccess;
            set => SetProperty(ref _editorAccess, value);
        }

        /// <summary>
        /// 弹窗 IP。
        /// </summary>
        public string EditorIp
        {
            get => _editorIp;
            set => SetProperty(ref _editorIp, value);
        }

        /// <summary>
        /// 弹窗端口。
        /// </summary>
        public string EditorPort
        {
            get => _editorPort;
            set => SetProperty(ref _editorPort, value);
        }

        /// <summary>
        /// 弹窗触发字符串。
        /// </summary>
        public string EditorTrigger
        {
            get => _editorTrigger;
            set => SetProperty(ref _editorTrigger, value);
        }

        /// <summary>
        /// 弹窗传输。TCP 或串口。
        /// </summary>
        public string EditorTransport
        {
            get => _editorTransport;
            set
            {
                if (!SetProperty(ref _editorTransport, value))
                    return;

                RaisePropertyChanged(nameof(IsEditorTcp));
            }
        }

        /// <summary>
        /// 弹窗当前选的是 TCP。串口时隐藏端口。
        /// </summary>
        public bool IsEditorTcp => EditorTransport == Text("Device_Tcp");

        /// <summary>
        /// 弹窗启用。
        /// </summary>
        public bool EditorEnabled
        {
            get => _editorEnabled;
            set => SetProperty(ref _editorEnabled, value);
        }

        /// <summary>
        /// 当前点位。选中后填到测试区。
        /// </summary>
        public PlcPointRow? SelectedPoint
        {
            get => _selectedPoint;
            set
            {
                if (!SetProperty(ref _selectedPoint, value) || value is null)
                    return;

                ApplyPoint(value);
            }
        }

        /// <summary>
        /// 当前扫码枪。点列表后只在触发区显示，名称和地址在这里不能改。
        /// </summary>
        public ScannerCard? SelectedScanner
        {
            get => _selectedScanner;
            set
            {
                if (!SetProperty(ref _selectedScanner, value))
                    return;

                ScanResult = value?.LastResult ?? "";
            }
        }

        /// <summary>
        /// PLC IP。
        /// </summary>
        public string PlcIp
        {
            get => _plcIp;
            set
            {
                if (SetProperty(ref _plcIp, value))
                    CommitLink();
            }
        }

        /// <summary>
        /// 连接类型。
        /// </summary>
        public string PlcConnectionType
        {
            get => _plcConnectionType;
            set
            {
                if (SetProperty(ref _plcConnectionType, value))
                    CommitLink();
            }
        }

        /// <summary>
        /// 端口。
        /// </summary>
        public string PlcPort
        {
            get => _plcPort;
            set
            {
                if (SetProperty(ref _plcPort, value))
                    CommitLink();
            }
        }

        /// <summary>
        /// 数据格式。
        /// </summary>
        public string PlcDataFormat
        {
            get => _plcDataFormat;
            set
            {
                if (SetProperty(ref _plcDataFormat, value))
                    CommitLink();
            }
        }

        /// <summary>
        /// 轮询周期。
        /// </summary>
        public string PlcPoll
        {
            get => _plcPoll;
            set
            {
                if (SetProperty(ref _plcPoll, value))
                    CommitLink();
            }
        }

        /// <summary>
        /// 读取超时。
        /// </summary>
        public string PlcReadTimeout
        {
            get => _plcReadTimeout;
            set
            {
                if (SetProperty(ref _plcReadTimeout, value))
                    CommitLink();
            }
        }

        /// <summary>
        /// 写入超时。
        /// </summary>
        public string PlcWriteTimeout
        {
            get => _plcWriteTimeout;
            set
            {
                if (SetProperty(ref _plcWriteTimeout, value))
                    CommitLink();
            }
        }

        /// <summary>
        /// 重连间隔。
        /// </summary>
        public string PlcReconnectInterval
        {
            get => _plcReconnectInterval;
            set
            {
                if (SetProperty(ref _plcReconnectInterval, value))
                    CommitLink();
            }
        }

        /// <summary>
        /// 重连次数。
        /// </summary>
        public string PlcReconnectCount
        {
            get => _plcReconnectCount;
            set
            {
                if (SetProperty(ref _plcReconnectCount, value))
                    CommitLink();
            }
        }

        /// <summary>
        /// 测试区地址。自定义读写用它。
        /// </summary>
        public string TestAddress
        {
            get => _testAddress;
            set => SetProperty(ref _testAddress, value);
        }

        /// <summary>
        /// 测试区数据类型。
        /// </summary>
        public string TestDataType
        {
            get => _testDataType;
            set => SetProperty(ref _testDataType, value);
        }

        /// <summary>
        /// 测试区长度。
        /// </summary>
        public string TestLength
        {
            get => _testLength;
            set => SetProperty(ref _testLength, value);
        }

        /// <summary>
        /// 测试区写入值。
        /// </summary>
        public string TestWriteValue
        {
            get => _testWriteValue;
            set => SetProperty(ref _testWriteValue, value);
        }

        /// <summary>
        /// 当前值。
        /// </summary>
        public string CurrentValue
        {
            get => _currentValue;
            private set => SetProperty(ref _currentValue, value);
        }

        /// <summary>
        /// 当前值说明。
        /// </summary>
        public string CurrentDetail
        {
            get => _currentDetail;
            private set => SetProperty(ref _currentDetail, value);
        }

        /// <summary>
        /// 扫码结果。
        /// </summary>
        public string ScanResult
        {
            get => _scanResult;
            private set => SetProperty(ref _scanResult, value);
        }

        /// <summary>
        /// 把点位抄进测试区。自定义读写仍可再改这些框。
        /// </summary>
        private void ApplyPoint(PlcPointRow row)
        {
            TestAddress = row.Address;
            TestDataType = row.DataType;
            TestLength = row.LengthText;
            TestWriteValue = string.IsNullOrEmpty(row.CurrentValue) ? "0" : row.CurrentValue;
            CurrentValue = row.CurrentValue;
            CurrentDetail = string.IsNullOrEmpty(row.ValueDetail)
                ? row.Address + " · " + row.DataType
                : row.ValueDetail;
        }

        /// <summary>
        /// 打开空白点位弹窗。
        /// </summary>
        private void OpenAddPoint()
        {
            _editingId = null;
            EditorTitle = Text("Device_AddPoint");
            EditorName = "";
            EditorAddress = "";
            EditorDataType = DataTypes[0];
            EditorLength = "1";
            EditorAccess = AccessModes[0];
            EditorEnabled = true;
            EditorError = "";
            SetEditor(EditorKind.Point);
        }

        /// <summary>
        /// 打开当前点位。只勾了一行就用那一行。
        /// </summary>
        private void OpenEditPoint()
        {
            var row = FindPointTarget();
            if (row is null)
                return;

            _editingId = row.Id;
            EditorTitle = Text("Device_EditPoint");
            EditorName = row.Name;
            EditorAddress = row.Address;
            EditorDataType = row.DataType;
            EditorLength = row.LengthText;
            EditorAccess = row.AccessText;
            EditorEnabled = row.IsEnabled;
            EditorError = "";
            SetEditor(EditorKind.Point);
        }

        /// <summary>
        /// 确认后删除勾选的点位。
        /// </summary>
        private async void DeletePoints()
        {
            var rows = Points.Where(item => item.IsChecked).ToList();
            if (rows.Count == 0)
                return;

            var confirmed = await MetroDialogService.ConfirmAsync(Text("Device_DeletePointTitle"), Text("Device_DeleteMessage"));
            if (!confirmed)
                return;

            foreach (var row in rows)
                Points.Remove(row);

            RemovePoints(rows);
            if (SelectedPoint is null || !Points.Contains(SelectedPoint))
                SelectedPoint = Points.FirstOrDefault();
        }

        /// <summary>
        /// 选择 CSV 后替换当前点位。格式不对时保留原列表。
        /// </summary>
        private async void ImportPoints()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var imported = ReadPointFile(CsvFileHelper.Read(dialog.FileName));
                if (!SqlSugarContext.IsOpen)
                {
                    await MetroDialogService.ShowMessageAsync(Text("Device_Import"), Text("Device_DbClosed"));
                    return;
                }

                ReplacePoints(imported);
                SelectedPoint = null;
                Points.Clear();
                foreach (var row in imported)
                    Points.Add(row);

                SelectedPoint = Points.FirstOrDefault();
            }
            catch (Exception)
            {
                await MetroDialogService.ShowMessageAsync(Text("Device_Import"), Text("Device_ImportFailed"));
            }
        }

        /// <summary>
        /// 把当前点位存成 CSV。取消对话框则不写文件。
        /// </summary>
        private async void ExportPoints()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = Text("Device_PointFile") + ".csv",
                DefaultExt = ".csv",
                AddExtension = true
            };
            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var lines = new List<string[]>
                {
                    new[]
                    {
                        Text("Device_PointName"),
                        Text("Device_Address"),
                        Text("Device_DataType"),
                        Text("Device_Length"),
                        Text("Device_Access"),
                        Text("Device_On")
                    }
                };
                foreach (var point in Points)
                {
                    lines.Add(new[]
                    {
                        point.Name,
                        point.Address,
                        point.DataType,
                        point.LengthText,
                        point.AccessText,
                        point.EnabledText
                    });
                }

                CsvFileHelper.Write(dialog.FileName, lines);
            }
            catch (Exception)
            {
                await MetroDialogService.ShowMessageAsync(Text("Device_Export"), Text("Device_ExportFailed"));
            }
        }

        /// <summary>
        /// 读点位 CSV。第一行长度不是正整数时当作表头跳过。
        /// </summary>
        private List<PlcPointRow> ReadPointFile(IReadOnlyList<string[]> table)
        {
            if (table.Count == 0)
                throw new InvalidDataException();

            var start = 0;
            if (table[0].Length < 4 || !int.TryParse(table[0][3].Trim(), out var headerLength) || headerLength <= 0)
                start = 1;

            var rows = new List<PlcPointRow>();
            for (var index = start; index < table.Count; index++)
            {
                var cells = table[index];
                if (cells.Length == 1 && cells[0].Trim().Length == 0)
                    continue;

                if (cells.Length < 6)
                    throw new InvalidDataException();

                var name = cells[0].Trim();
                var address = cells[1].Trim();
                var dataType = cells[2].Trim();
                var length = cells[3].Trim();
                if (name.Length == 0 || address.Length == 0 || !Enum.TryParse<PlcDataType>(dataType, out var parsedType) || !Enum.IsDefined(parsedType))
                    throw new InvalidDataException();

                if (!int.TryParse(length, out var lengthValue) || lengthValue <= 0)
                    throw new InvalidDataException();

                var enabled = ParseEnabled(cells[5]);
                var row = new PlcPointRow
                {
                    Name = name,
                    Address = address,
                    DataType = dataType,
                    LengthText = length,
                    AccessText = NormalizeAccess(cells[4])
                };
                row.UseEnabledLabels(Text("Device_On"), Text("Device_Off"), enabled);
                rows.Add(row);
            }

            return rows;
        }

        /// <summary>
        /// 把读写文字收成当前语言。认不出的原文保留。
        /// </summary>
        private string NormalizeAccess(string text)
        {
            var value = text.Trim();
            if (value.Equals("只读", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Read only", StringComparison.OrdinalIgnoreCase)
                || value.Equals(Text("Device_AccessRead"), StringComparison.OrdinalIgnoreCase))
                return Text("Device_AccessRead");

            if (value.Equals("读写", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Read/Write", StringComparison.OrdinalIgnoreCase)
                || value.Equals(Text("Device_AccessReadWrite"), StringComparison.OrdinalIgnoreCase))
                return Text("Device_AccessReadWrite");

            return value;
        }

        /// <summary>
        /// 启用文字。认不出时整份文件视为无效。
        /// </summary>
        private static bool ParseEnabled(string text)
        {
            var value = text.Trim();
            if (value.Equals("启用", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Enabled", StringComparison.OrdinalIgnoreCase)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("1", StringComparison.OrdinalIgnoreCase)
                || value.Equals("是", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.Equals("停用", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Disabled", StringComparison.OrdinalIgnoreCase)
                || value.Equals("false", StringComparison.OrdinalIgnoreCase)
                || value.Equals("0", StringComparison.OrdinalIgnoreCase)
                || value.Equals("否", StringComparison.OrdinalIgnoreCase)
                || value.Equals("no", StringComparison.OrdinalIgnoreCase))
                return false;

            throw new InvalidDataException();
        }

        /// <summary>
        /// 打开空白扫码枪弹窗。
        /// </summary>
        private void OpenAddScanner()
        {
            _editingId = null;
            EditorTitle = Text("Device_AddScanner");
            EditorName = "";
            EditorIp = "";
            EditorPort = "9004";
            EditorTrigger = "LON";
            EditorTransport = ScanTransports[0];
            EditorEnabled = true;
            EditorError = "";
            SetEditor(EditorKind.Scanner);
        }

        /// <summary>
        /// 打开当前扫码枪。
        /// </summary>
        private void OpenEditScanner()
        {
            if (SelectedScanner is null)
                return;

            _editingId = SelectedScanner.Id;
            EditorTitle = Text("Device_EditScanner");
            EditorName = SelectedScanner.Name;
            EditorIp = SelectedScanner.Ip;
            EditorPort = SelectedScanner.Port;
            EditorTrigger = SelectedScanner.TriggerText;
            EditorTransport = TransportText(SelectedScanner.Transport);
            EditorEnabled = SelectedScanner.IsEnabled;
            EditorError = "";
            SetEditor(EditorKind.Scanner);
        }

        /// <summary>
        /// 确认后删除当前扫码枪。
        /// </summary>
        private async void DeleteScanner()
        {
            if (SelectedScanner is null)
                return;

            var confirmed = await MetroDialogService.ConfirmAsync(Text("Device_DeleteScannerTitle"), Text("Device_DeleteMessage"));
            if (!confirmed || SelectedScanner is null)
                return;

            var remove = SelectedScanner;
            var index = Scanners.IndexOf(remove);
            remove.PropertyChanged -= OnScannerChanged;
            Scanners.Remove(remove);
            RemoveScanner(remove);
            if (Scanners.Count == 0)
            {
                SelectedScanner = null;
                return;
            }

            SelectedScanner = Scanners[Math.Min(index, Scanners.Count - 1)];
        }

        /// <summary>
        /// 校验后写入内存列表。
        /// </summary>
        private void SaveEditor()
        {
            if (_editor == EditorKind.Point)
                SavePoint();
            else if (_editor == EditorKind.Scanner)
                SaveScanner();
        }

        /// <summary>
        /// 保存点位。名称为空或长度不是正整数时停在弹窗里。
        /// </summary>
        private void SavePoint()
        {
            var name = EditorName.Trim();
            var address = EditorAddress.Trim();
            var length = EditorLength.Trim();
            if (name.Length == 0)
            {
                EditorError = Text("Device_NeedName");
                return;
            }

            if (address.Length == 0)
            {
                EditorError = Text("Device_NeedAddress");
                return;
            }

            if (!int.TryParse(length, out var lengthValue) || lengthValue <= 0)
            {
                EditorError = Text("Device_NeedLength");
                return;
            }

            var row = _editingId is null ? null : Points.FirstOrDefault(item => item.Id == _editingId);
            if (row is null)
            {
                row = AddPoint(name, address, EditorDataType, length, EditorAccess, EditorEnabled, false);
                SelectedPoint = row;
            }
            else
            {
                row.Name = name;
                row.Address = address;
                row.DataType = EditorDataType;
                row.LengthText = length;
                row.AccessText = EditorAccess;
                row.UseEnabledLabels(Text("Device_On"), Text("Device_Off"), EditorEnabled);
                if (SelectedPoint == row)
                    ApplyPoint(row);
            }

            StorePoint(row);
            StorePointOrder();
            CloseEditor();
        }

        /// <summary>
        /// 保存扫码枪。TCP 要求 IP 和端口，串口要求 COM 口。
        /// </summary>
        private void SaveScanner()
        {
            var name = EditorName.Trim();
            var host = EditorIp.Trim();
            var port = EditorPort.Trim();
            var trigger = EditorTrigger.Trim();
            var transport = TransportOf(EditorTransport);
            if (name.Length == 0)
            {
                EditorError = Text("Device_NeedName");
                return;
            }

            if (host.Length == 0)
            {
                EditorError = transport == ScanTransport.Serial ? Text("Device_NeedCom") : Text("Device_NeedIp");
                return;
            }

            if (transport == ScanTransport.Tcp && (!int.TryParse(port, out var portValue) || portValue <= 0))
            {
                EditorError = Text("Device_NeedPort");
                return;
            }

            if (trigger.Length == 0)
            {
                EditorError = Text("Device_NeedTrigger");
                return;
            }

            var card = _editingId is null ? null : Scanners.FirstOrDefault(item => item.Id == _editingId);
            if (card is null)
            {
                card = AddScanner(name, host, transport == ScanTransport.Tcp ? port : "", trigger, transport, EditorEnabled);
                SelectedScanner = card;
            }
            else
            {
                _suppressScanSave = true;
                card.Name = name;
                card.Transport = transport;
                card.Ip = host;
                card.Port = transport == ScanTransport.Tcp ? port : "";
                card.TriggerText = trigger;
                card.UseEnabledLabels(Text("Device_On"), Text("Device_Off"), EditorEnabled);
                _suppressScanSave = false;
            }

            StoreScanner(card);
            DropScanner(card.Id);
            StoreScannerOrder();
            CloseEditor();
        }

        /// <summary>
        /// 关闭弹窗，不改列表。
        /// </summary>
        private void CloseEditor()
        {
            _editingId = null;
            EditorError = "";
            SetEditor(EditorKind.None);
        }

        /// <summary>
        /// 用测试区当前内容读一次，并把结果写进日志和当前值。
        /// </summary>
        private async void ReadCustom()
        {
            var address = TestAddress.Trim();
            if (address.Length == 0)
                return;

            var type = string.IsNullOrWhiteSpace(TestDataType) ? DataTypes[0] : TestDataType;
            var length = string.IsNullOrWhiteSpace(TestLength) ? "1" : TestLength.Trim();
            var point = FindByAddress(address);
            if (point is { IsEnabled: false })
            {
                NotePlc(DeviceLogKind.Timeout, Text("Device_PointDisabled"), true);
                return;
            }

            if (!Enum.TryParse<PlcDataType>(type, out _))
            {
                NotePlc(DeviceLogKind.Timeout, Text("Device_BadType"), true);
                return;
            }

            if (!int.TryParse(length, out var lengthValue) || lengthValue <= 0)
            {
                NotePlc(DeviceLogKind.Timeout, Text("Device_NeedLength"), true);
                return;
            }

            NotePlc(DeviceLogKind.Send, "读取 地址 " + address + " 类型 " + type + " 长度 " + length, false);
            var result = await ExchangePlc(address, type, length, null);
            if (result is null)
                return;

            if (!result.IsSuccess)
            {
                NotePlc(DeviceLogKind.Timeout, result.Message, true);
                return;
            }

            NotePlc(DeviceLogKind.Receive, "应答 读取值 " + result.Value, false);
            RememberValue(point, address, type, result.Value);
        }

        /// <summary>
        /// 用测试区当前地址和写入值写一次。
        /// </summary>
        private async void WriteCustom()
        {
            var address = TestAddress.Trim();
            if (address.Length == 0)
                return;

            var type = string.IsNullOrWhiteSpace(TestDataType) ? DataTypes[0] : TestDataType;
            var value = TestWriteValue.Trim();
            var point = FindByAddress(address);
            if (point is { IsEnabled: false })
            {
                NotePlc(DeviceLogKind.Timeout, Text("Device_PointDisabled"), true);
                return;
            }

            if (point is not null && point.AccessText == Text("Device_AccessRead"))
            {
                NotePlc(DeviceLogKind.Timeout, Text("Device_ReadOnlyPoint"), true);
                return;
            }

            if (!Enum.TryParse<PlcDataType>(type, out _))
            {
                NotePlc(DeviceLogKind.Timeout, Text("Device_BadType"), true);
                return;
            }

            NotePlc(DeviceLogKind.Send, "写入 地址 " + address + " 类型 " + type + " 值 " + value, false);
            var result = await ExchangePlc(address, type, "1", value);
            if (result is null)
                return;

            if (!result.IsSuccess)
            {
                NotePlc(DeviceLogKind.Timeout, result.Message, true);
                return;
            }

            NotePlc(DeviceLogKind.Receive, "写入成功", false);
            RememberValue(point, address, type, value);
        }

        /// <summary>
        /// 先把当前行填进测试区，再读取。
        /// </summary>
        private void ReadSelectedPoint()
        {
            if (SelectedPoint is null)
                return;

            ApplyPoint(SelectedPoint);
            ReadCustom();
        }

        /// <summary>
        /// 先把当前行填进测试区。地址没变时保留手填的写入值。
        /// </summary>
        private void WriteSelectedPoint()
        {
            if (SelectedPoint is null)
                return;

            var sameAddress = string.Equals(TestAddress.Trim(), SelectedPoint.Address, StringComparison.OrdinalIgnoreCase);
            var writeValue = sameAddress ? TestWriteValue : SelectedPoint.CurrentValue;
            ApplyPoint(SelectedPoint);
            if (!string.IsNullOrEmpty(writeValue))
                TestWriteValue = writeValue;

            WriteCustom();
        }

        /// <summary>
        /// 向当前扫码枪发送触发字符串。停用时记超时，不打开连接。
        /// </summary>
        private async void TriggerScanner()
        {
            var scanner = SelectedScanner;
            if (scanner is null)
                return;

            var trigger = string.IsNullOrWhiteSpace(scanner.TriggerText) ? "LON" : scanner.TriggerText.Trim();
            NoteScan(DeviceLogKind.Send, scanner.Name + " 触发字符串 " + trigger, false);
            if (!scanner.IsEnabled)
            {
                NoteScan(DeviceLogKind.Timeout, scanner.Name + " 1000 ms 无应答", true);
                return;
            }

            var result = await ExchangeScan(scanner, trigger);
            if (result is null)
                return;

            if (!result.IsSuccess)
            {
                NoteScan(DeviceLogKind.Timeout, scanner.Name + " " + result.Message, true);
                return;
            }

            scanner.LastResult = result.Value;
            if (SelectedScanner == scanner)
                ScanResult = result.Value;

            NoteScan(DeviceLogKind.Receive, scanner.Name + " " + result.Value, false);
        }

        /// <summary>
        /// 只勾了一行就用那一行，否则用当前点中的行。
        /// </summary>
        private PlcPointRow? FindPointTarget()
        {
            var checkedRows = Points.Where(item => item.IsChecked).ToList();
            if (checkedRows.Count == 1)
                return checkedRows[0];

            return SelectedPoint;
        }

        /// <summary>
        /// 按地址找点位。自定义读写用来回写当前值。
        /// </summary>
        private PlcPointRow? FindByAddress(string address)
        {
            return Points.FirstOrDefault(item => string.Equals(item.Address, address, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 记下这次读写的值和时间。
        /// </summary>
        private void RememberValue(PlcPointRow? point, string address, string type, string value)
        {
            var detail = address + " · " + type + " · " + DateTime.Now.ToString("HH:mm:ss.fff");
            CurrentValue = value;
            CurrentDetail = detail;
            if (point is null)
                return;

            point.CurrentValue = value;
            point.ValueDetail = detail;
        }

        /// <summary>
        /// 追加一行 PLC 日志。
        /// </summary>
        private void AddPlc(DeviceLogKind kind, string message)
        {
            PlcLogs.Add(Line(DateTime.Now.ToString("HH:mm:ss.fff"), kind, DirectionText(kind), message));
        }

        /// <summary>
        /// 追加一行扫码日志。
        /// </summary>
        private void AddScan(DeviceLogKind kind, string message)
        {
            ScanLogs.Add(Line(DateTime.Now.ToString("HH:mm:ss.fff"), kind, DirectionText(kind), message));
        }

        /// <summary>
        /// 方向文字。普通行没有方向。
        /// </summary>
        private static string DirectionText(DeviceLogKind kind)
        {
            return kind switch
            {
                DeviceLogKind.Send => Text("Device_LogSend"),
                DeviceLogKind.Receive => Text("Device_LogReceive"),
                DeviceLogKind.Timeout => Text("Device_LogTimeout"),
                _ => ""
            };
        }

        /// <summary>
        /// 造一行日志。
        /// </summary>
        private static DeviceLogLine Line(string time, DeviceLogKind kind, string direction, string message)
        {
            return new DeviceLogLine
            {
                TimeText = time,
                Kind = kind,
                DirectionText = direction,
                Message = message
            };
        }

        /// <summary>
        /// 加入一个点位。
        /// </summary>
        private PlcPointRow AddPoint(
            string name,
            string address,
            string dataType,
            string length,
            string access,
            bool enabled,
            bool isChecked)
        {
            var row = new PlcPointRow
            {
                Name = name,
                Address = address,
                DataType = dataType,
                LengthText = length,
                AccessText = access,
                IsChecked = isChecked
            };
            row.UseEnabledLabels(Text("Device_On"), Text("Device_Off"), enabled);
            Points.Add(row);
            return row;
        }

        /// <summary>
        /// 加入一个扫码枪。
        /// </summary>
        private ScannerCard AddScanner(string name, string host, string port, string trigger, ScanTransport transport, bool enabled)
        {
            var card = new ScannerCard
            {
                Name = name,
                Ip = host,
                Port = port,
                TriggerText = trigger,
                Transport = transport
            };
            card.UseEnabledLabels(Text("Device_On"), Text("Device_Off"), enabled);
            card.PropertyChanged += OnScannerChanged;
            Scanners.Add(card);
            return card;
        }

        /// <summary>
        /// 切换弹窗，并通知两种内容的可见性。
        /// </summary>
        private void SetEditor(EditorKind kind)
        {
            _editor = kind;
            RaisePropertyChanged(nameof(IsEditorOpen));
            RaisePropertyChanged(nameof(IsPointEditor));
            RaisePropertyChanged(nameof(IsScannerEditor));
        }

        /// <summary>
        /// 取语言资源。没有时退回键名。
        /// </summary>
        private static string Text(string key)
        {
            if (Application.Current?.TryFindResource(key) is string value && value.Length > 0)
                return value;

            return key;
        }
    }
}
