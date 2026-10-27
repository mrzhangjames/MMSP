using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.WpfBase.Core;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;
using MMSP.Service.Device.Plc;
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
        private string _editorPointKind = "";
        private string _editorBit = "";
        private string _editorDescription = "";
        private string _editorBaud = "9600";
        private string _editorParity = "None";
        private string _editorDataBits = "8";
        private string _editorStopBits = "One";
        private PlcPointRow? _selectedPoint;
        private ScannerCard? _selectedScanner;
        private string _plcIp = "192.168.1.105";
        private string _plcConnectionType = "";
        private string _plcStation = "1";
        private string _plcPort = "502";
        private string _plcDataFormat = "CDAB";
        private string _plcPoll = "200 ms";
        private string _plcReadTimeout = "1000 ms";
        private string _plcWriteTimeout = "1000 ms";
        private string _plcReconnectInterval = "3000 ms";
        private string _plcReconnectCount = "3";
        private string _plcHeartbeat = "";
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
            ConnectionTypes = PlcLinkProtocols.Editable.Select(PlcLinkProtocols.NameOf).ToArray();
            DataFormats = ["ABCD", "CDAB", "BADC", "DCBA"];
            DataTypes = ["Int16", "UInt16", "Int32", "Float", "String", "UInt16Array"];
            AccessModes = [Text("Device_AccessReadWrite"), Text("Device_AccessRead")];
            ScanTransports = [Text("Device_Tcp"), Text("Device_Serial")];
            PointKinds = KindChoices();
            ParityChoices = ["None", "Odd", "Even", "Mark", "Space"];
            DataBitChoices = ["7", "8"];
            StopBitChoices = ["One", "OnePointFive", "Two"];
            AppAppearance.LanguageChanged += OnLanguageChanged;
            EditorDataType = DataTypes[0];
            EditorAccess = AccessModes[0];
            EditorTransport = ScanTransports[0];
            EditorPointKind = PointKinds[0];
            PlcConnectionType = ConnectionTypes[0];

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
            RegisterLinkUse();
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
        /// 连接类型下拉项。已保存的串口或 S7 会追加在末尾，避免被改成 Modbus TCP。
        /// </summary>
        private IReadOnlyList<string> _connectionTypes = [];

        /// <summary>
        /// 点位类型下拉项。
        /// </summary>
        private IReadOnlyList<string> _pointKinds = [];

        /// <summary>
        /// 连接类型下拉项。
        /// </summary>
        public IReadOnlyList<string> ConnectionTypes
        {
            get => _connectionTypes;
            private set => SetProperty(ref _connectionTypes, value);
        }

        /// <summary>
        /// 点位类型下拉项。
        /// </summary>
        public IReadOnlyList<string> PointKinds
        {
            get => _pointKinds;
            private set => SetProperty(ref _pointKinds, value);
        }

        /// <summary>
        /// 串口校验位。
        /// </summary>
        public IReadOnlyList<string> ParityChoices { get; }

        /// <summary>
        /// 串口数据位。
        /// </summary>
        public IReadOnlyList<string> DataBitChoices { get; }

        /// <summary>
        /// 串口停止位。
        /// </summary>
        public IReadOnlyList<string> StopBitChoices { get; }

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
            var kindIndex = ChoiceIndex(PointKinds, EditorPointKind);
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

                point.KindText = KindText(point.PointKind);
            }

            foreach (var card in Scanners)
                card.UseEnabledLabels(enabledOn, enabledOff, card.IsEnabled);

            AccessModes = [readWrite, readOnly];
            ScanTransports = [Text("Device_Tcp"), Text("Device_Serial")];
            PointKinds = KindChoices();
            if (accessIndex >= 0 && accessIndex < AccessModes.Count)
                EditorAccess = AccessModes[accessIndex];
            if (transportIndex >= 0 && transportIndex < ScanTransports.Count)
                EditorTransport = ScanTransports[transportIndex];
            if (kindIndex >= 0 && kindIndex < PointKinds.Count)
                EditorPointKind = PointKinds[kindIndex];
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
                RaisePropertyChanged(nameof(IsEditorSerial));
            }
        }

        /// <summary>
        /// 弹窗当前选的是 TCP。串口时隐藏端口，并显示波特率等参数。
        /// </summary>
        public bool IsEditorTcp => EditorTransport == Text("Device_Tcp");

        /// <summary>
        /// 弹窗当前选的是串口。
        /// </summary>
        public bool IsEditorSerial => !IsEditorTcp;

        /// <summary>
        /// 弹窗点位类型。
        /// </summary>
        public string EditorPointKind
        {
            get => _editorPointKind;
            set => SetProperty(ref _editorPointKind, value);
        }

        /// <summary>
        /// 弹窗 Bit。空表示整字。
        /// </summary>
        public string EditorBit
        {
            get => _editorBit;
            set => SetProperty(ref _editorBit, value);
        }

        /// <summary>
        /// 弹窗描述。
        /// </summary>
        public string EditorDescription
        {
            get => _editorDescription;
            set => SetProperty(ref _editorDescription, value);
        }

        /// <summary>
        /// 弹窗波特率。
        /// </summary>
        public string EditorBaud
        {
            get => _editorBaud;
            set => SetProperty(ref _editorBaud, value);
        }

        /// <summary>
        /// 弹窗校验位。
        /// </summary>
        public string EditorParity
        {
            get => _editorParity;
            set => SetProperty(ref _editorParity, value);
        }

        /// <summary>
        /// 弹窗数据位。
        /// </summary>
        public string EditorDataBits
        {
            get => _editorDataBits;
            set => SetProperty(ref _editorDataBits, value);
        }

        /// <summary>
        /// 弹窗停止位。
        /// </summary>
        public string EditorStopBits
        {
            get => _editorStopBits;
            set => SetProperty(ref _editorStopBits, value);
        }

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
        /// 站号。0 到 255。
        /// </summary>
        public string PlcStation
        {
            get => _plcStation;
            set
            {
                if (SetProperty(ref _plcStation, value))
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
        /// 心跳地址。留空则不启用。
        /// </summary>
        public string PlcHeartbeat
        {
            get => _plcHeartbeat;
            set
            {
                if (SetProperty(ref _plcHeartbeat, value))
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
            EditorPointKind = PointKinds.Count > 0 ? PointKinds[0] : KindText(PlcPointKind.Flow);
            EditorBit = "";
            EditorDescription = "";
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
            EditorPointKind = string.IsNullOrEmpty(row.KindText) ? KindText(row.PointKind) : row.KindText;
            EditorBit = row.BitText;
            EditorDescription = row.Description;
            EditorEnabled = row.IsEnabled;
            EditorError = "";
            SetEditor(EditorKind.Point);
        }

        /// <summary>
        /// 勾选里是否包含最后一条心跳或状态。
        /// </summary>
        private bool RemovesLastSingleton(IReadOnlyList<PlcPointRow> rows)
        {
            return RemovesLast(rows, PlcPointKind.Heartbeat) || RemovesLast(rows, PlcPointKind.Status);
        }

        /// <summary>
        /// 这种类型已经有点，并且勾选会把它删光。
        /// </summary>
        private bool RemovesLast(IReadOnlyList<PlcPointRow> rows, PlcPointKind kind)
        {
            var existing = Points.Where(item => item.PointKind == kind).ToList();
            return existing.Count > 0 && existing.All(rows.Contains);
        }

        /// <summary>
        /// 确认后删除勾选的点位。
        /// </summary>
        private async void DeletePoints()
        {
            var rows = Points.Where(item => item.IsChecked).ToList();
            if (rows.Count == 0)
                return;

            if (RemovesLastSingleton(rows))
            {
                await MetroDialogService.ShowMessageAsync(Text("Device_DeletePointTitle"), Text("Device_KeepSingleton"));
                return;
            }

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
                    LogHelper.PlcError("导入点位配置失败");
                    await MetroDialogService.ShowMessageAsync(Text("Device_Import"), Text("Device_DbClosed"));
                    return;
                }

                ReplacePoints(imported);
                SelectedPoint = null;
                Points.Clear();
                foreach (var row in imported)
                    Points.Add(row);

                SelectedPoint = Points.FirstOrDefault();
                LogHelper.PlcInfo($"导入点位配置 {imported.Count}");
            }
            catch (Exception)
            {
                LogHelper.PlcError("导入点位配置失败");
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
                        Text("Device_On"),
                        Text("Device_PointKind"),
                        Text("Device_Bit"),
                        Text("Device_Description")
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
                        point.EnabledText,
                        string.IsNullOrEmpty(point.KindText) ? KindText(point.PointKind) : point.KindText,
                        point.BitText,
                        point.Description
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
        /// 读点位 CSV，并换成当前语言的读写和启用文字。
        /// </summary>
        private List<PlcPointRow> ReadPointFile(IReadOnlyList<string[]> table)
        {
            var rows = new List<PlcPointRow>();
            foreach (var item in PlcPointImport.Read(table))
            {
                var row = new PlcPointRow
                {
                    Name = item.Name,
                    Address = item.Address,
                    DataType = item.DataTypeText,
                    LengthText = item.LengthText,
                    AccessText = NormalizeAccess(item.AccessText),
                    PointKind = item.Kind,
                    BitIndex = item.BitIndex,
                    Description = item.Description,
                    KindText = KindText(item.Kind)
                };
                row.UseEnabledLabels(Text("Device_On"), Text("Device_Off"), item.Enabled);
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
            EditorBaud = "9600";
            EditorParity = ParityChoices[0];
            EditorDataBits = DataBitChoices[1];
            EditorStopBits = StopBitChoices[0];
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
            EditorBaud = SelectedScanner.BaudRate.ToString();
            EditorParity = SelectedScanner.Parity;
            EditorDataBits = SelectedScanner.DataBits.ToString();
            EditorStopBits = SelectedScanner.StopBits;
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

            if (!TryParseKind(EditorPointKind, out var kind))
            {
                EditorError = Text("Device_NeedKind");
                return;
            }

            if (!Enum.TryParse<PlcDataType>(EditorDataType, out var dataType) || !Enum.IsDefined(dataType))
            {
                EditorError = Text("Device_BadType");
                return;
            }

            int? bitIndex = null;
            var bitText = EditorBit.Trim();
            if (bitText.Length > 0)
            {
                if (!int.TryParse(bitText, out var bitValue))
                {
                    EditorError = Text("Device_BadBit");
                    return;
                }

                bitIndex = PlcPointEntity.NormalizeBitIndex(dataType, lengthValue, bitValue);
                if (bitIndex is null)
                {
                    EditorError = Text("Device_BadBit");
                    return;
                }
            }

            var description = PlcPointEntity.NormalizeDescription(EditorDescription);
            var row = _editingId is null ? null : Points.FirstOrDefault(item => item.Id == _editingId);
            if (PlcPointEntity.IsSingleton(kind) && Points.Any(item => item.PointKind == kind && item.Id != (row?.Id ?? Guid.Empty)))
            {
                EditorError = Text("Device_SingletonPoint");
                return;
            }

            var created = false;
            if (row is null)
            {
                row = AddPoint(name, address, EditorDataType, length, EditorAccess, EditorEnabled, false);
                created = true;
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

            row.PointKind = kind;
            row.KindText = KindText(kind);
            row.BitIndex = bitIndex;
            row.Description = description;
            if (!StorePoint(row))
            {
                if (created)
                {
                    Points.Remove(row);
                    SelectedPoint = Points.FirstOrDefault();
                }

                EditorError = Text("Device_SingletonPoint");
                return;
            }
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

            var baud = 9600;
            var parity = "None";
            var dataBits = 8;
            var stopBits = "One";
            if (transport == ScanTransport.Serial)
            {
                if (!int.TryParse(EditorBaud.Trim(), out baud) || baud <= 0)
                {
                    EditorError = Text("Device_NeedBaud");
                    return;
                }

                parity = EditorParity.Trim();
                if (!ParityChoices.Contains(parity))
                {
                    EditorError = Text("Device_BadSerial");
                    return;
                }

                if (!int.TryParse(EditorDataBits.Trim(), out dataBits) || dataBits is not (7 or 8))
                {
                    EditorError = Text("Device_BadSerial");
                    return;
                }

                stopBits = EditorStopBits.Trim();
                if (!StopBitChoices.Contains(stopBits))
                {
                    EditorError = Text("Device_BadSerial");
                    return;
                }
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
                if (transport == ScanTransport.Serial)
                {
                    card.BaudRate = baud;
                    card.Parity = parity;
                    card.DataBits = dataBits;
                    card.StopBits = stopBits;
                }

                card.UseEnabledLabels(Text("Device_On"), Text("Device_Off"), EditorEnabled);
                _suppressScanSave = false;
            }

            if (card.Transport == ScanTransport.Serial && _editingId is null)
            {
                card.BaudRate = baud;
                card.Parity = parity;
                card.DataBits = dataBits;
                card.StopBits = stopBits;
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
            if (LicenseGate.Block(ContainerExt))
                return;

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
            var bitIndex = BitOf(point, type, length);
            var formatStatus = point is not null
                && bitIndex is null
                && point.PointKind == PlcPointKind.Status
                && string.Equals(point.DataType, type, StringComparison.Ordinal)
                && string.Equals(point.LengthText.Trim(), length, StringComparison.Ordinal);
            var result = await ExchangePlc(address, type, length, null, bitIndex, formatStatus);
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
            if (LicenseGate.Block(ContainerExt))
                return;

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
            var result = await ExchangePlc(address, type, "1", value, BitOf(point, type, "1"), false);
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
            if (LicenseGate.Block(ContainerExt))
                return;

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
        /// 按地址找点位。自定义读写用来回写当前值。同地址只取第一行。
        /// </summary>
        private PlcPointRow? FindByAddress(string address)
        {
            return Points.FirstOrDefault(item => string.Equals(item.Address, address, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 类型和长度与这一行一致时，带上它的 Bit。
        /// </summary>
        private static int? BitOf(PlcPointRow? point, string type, string length)
        {
            if (point is null)
                return null;

            if (!string.Equals(point.DataType, type, StringComparison.Ordinal))
                return null;

            if (!string.Equals(point.LengthText.Trim(), length, StringComparison.Ordinal))
                return null;

            return point.BitIndex;
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
        /// 点位类型下拉。顺序与枚举说明一致。
        /// </summary>
        private static string[] KindChoices()
        {
            return
            [
                KindText(PlcPointKind.Flow),
                KindText(PlcPointKind.Monitor),
                KindText(PlcPointKind.Heartbeat),
                KindText(PlcPointKind.Status)
            ];
        }

        /// <summary>
        /// 点位类型的当前语言文字。
        /// </summary>
        private static string KindText(PlcPointKind kind)
        {
            var key = kind switch
            {
                PlcPointKind.Monitor => "Device_KindMonitor",
                PlcPointKind.Heartbeat => "Device_KindHeartbeat",
                PlcPointKind.Status => "Device_KindStatus",
                _ => "Device_KindFlow"
            };
            return Text(key);
        }

        /// <summary>
        /// 下拉文字或 CSV 文字转成点位类型。
        /// </summary>
        private static bool TryParseKind(string text, out PlcPointKind kind)
        {
            if (text == Text("Device_KindFlow"))
            {
                kind = PlcPointKind.Flow;
                return true;
            }

            if (text == Text("Device_KindMonitor"))
            {
                kind = PlcPointKind.Monitor;
                return true;
            }

            if (text == Text("Device_KindHeartbeat"))
            {
                kind = PlcPointKind.Heartbeat;
                return true;
            }

            if (text == Text("Device_KindStatus"))
            {
                kind = PlcPointKind.Status;
                return true;
            }

            return PlcPointColumns.TryParseKind(text, out kind);
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
