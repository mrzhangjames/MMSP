using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using MMSP.App.TemplateMatch.Models;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;
using MMSP.Service.Device.Plc;
using MMSP.Service.Device.Scan;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 设备页的客户库读写和真实通讯。点位列表不自动读写 PLC。
    /// </summary>
    public partial class DeviceViewModel
    {
        /// <summary>
        /// 页面离开后，迟到的结果不再写到界面上。
        /// </summary>
        private const string ReleasedMessage = "连接已释放";

        /// <summary>
        /// 加载连接参数时，不把表单回写成客户库。
        /// </summary>
        private bool _suppressLinkSave;

        /// <summary>
        /// 弹窗写入卡片时，不重复保存扫码枪。
        /// </summary>
        private bool _suppressScanSave;

        /// <summary>
        /// 当前这一行 PLC 连接。空表示还没从客户库读到。
        /// </summary>
        private Guid _linkId;

        /// <summary>
        /// 站号。表单不编辑，连接时使用。
        /// </summary>
        private int _station = 1;

        /// <summary>
        /// 已保存的 IP。
        /// </summary>
        private string _host = "";

        /// <summary>
        /// 已保存的端口。
        /// </summary>
        private int _port = 502;

        /// <summary>
        /// 已保存的字节序。
        /// </summary>
        private string _dataFormat = "ABCD";

        /// <summary>
        /// 已保存的轮询周期。只入库和显示，点位列表不按它读取。
        /// </summary>
        private int _pollMs = 200;

        /// <summary>
        /// 已保存的读取超时。连接和接收都用它。
        /// </summary>
        private int _readTimeout = 1000;

        /// <summary>
        /// 已保存的写入超时。只显示和入库。
        /// </summary>
        private int _writeTimeout = 1000;

        /// <summary>
        /// 已保存的重连间隔。
        /// </summary>
        private int _reconnectInterval = 3000;

        /// <summary>
        /// 已保存的重连次数。第一次失败之后再试这么多次。
        /// </summary>
        private int _reconnectCount = 3;

        /// <summary>
        /// 当前这次进入页面的代数。
        /// </summary>
        private int _session;

        /// <summary>
        /// 这一次停留期间的取消源。右键读写用它。
        /// </summary>
        private CancellationTokenSource _life = new();

        /// <summary>
        /// PLC 客户端锁。连接、读写和释放都走它。
        /// </summary>
        private readonly object _plcLock = new();

        /// <summary>
        /// 当前 PLC。一台页面一个客户端。
        /// </summary>
        private IPlcClient? _plc;

        /// <summary>
        /// 当前客户端对应的地址、端口、字节序、读取超时和站号。
        /// </summary>
        private string _plcSignature = "";

        /// <summary>
        /// 每释放一次客户端加一。旧的重连不再关掉新客户端。
        /// </summary>
        private int _plcGeneration;

        /// <summary>
        /// 扫码客户端锁。
        /// </summary>
        private readonly object _scanLock = new();

        /// <summary>
        /// 每把枪一个客户端。键是卡片 Id。
        /// </summary>
        private readonly Dictionary<Guid, IScanClient> _scanClients = new();

        /// <summary>
        /// 每把枪当前客户端的连接和触发字符串。
        /// </summary>
        private readonly Dictionary<Guid, string> _scanSignatures = new();

        /// <summary>
        /// 进入页面时从客户库加载。不连接 PLC，也不读写点位。
        /// </summary>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            BumpSession();
            CancelLife();
            _life = new CancellationTokenSource();
            LoadDevice();
        }

        /// <summary>
        /// 离开页面时释放 PLC 和扫码枪。
        /// </summary>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            BumpSession();
            CancelLife();
            CloseDevices();
            CloseEditor();
        }

        /// <summary>
        /// 读连接、点位和扫码枪。客户库没打开时列表保持空白。
        /// </summary>
        private void LoadDevice()
        {
            var pointId = SelectedPoint?.Id;
            var scannerId = SelectedScanner?.Id;
            if (!SqlSugarContext.IsOpen)
            {
                _pollMs = 0;
                ClearDeviceLists();
                NotePlc(DeviceLogKind.Timeout, Text("Device_DbClosed"), true);
                return;
            }

            try
            {
                LoadLink();
                LoadPoints(pointId);
                LoadScanners(scannerId);
            }
            catch (Exception ex)
            {
                _pollMs = 0;
                NotePlc(DeviceLogKind.Timeout, ex.Message, true);
            }
        }

        /// <summary>
        /// 读唯一一行连接。没有就插入当前表单的默认值。
        /// </summary>
        private void LoadLink()
        {
            var db = SqlSugarContext.CustomerDb;
            var row = db.Queryable<PlcLinkEntity>().ToList().FirstOrDefault();
            if (row is null)
            {
                row = DefaultLink();
                db.Insertable(row).ExecuteCommand();
            }

            var fix = false;
            if (row.Protocol != (int)PlcProtocol.ModbusTcp)
            {
                row.Protocol = (int)PlcProtocol.ModbusTcp;
                fix = true;
            }

            if (row.DataFormat is not ("ABCD" or "CDAB" or "BADC" or "DCBA"))
            {
                row.DataFormat = "ABCD";
                fix = true;
            }

            if (fix)
                db.Updateable(row).ExecuteCommand();

            _suppressLinkSave = true;
            try
            {
                _linkId = row.Id;
                lock (_plcLock)
                {
                    _host = row.Host ?? "";
                    _port = row.Port;
                    _dataFormat = row.DataFormat;
                    _station = row.Station;
                    _pollMs = row.PollMs;
                    _readTimeout = row.ReadTimeoutMs;
                    _writeTimeout = row.WriteTimeoutMs;
                    _reconnectInterval = row.ReconnectIntervalMs;
                    _reconnectCount = row.ReconnectCount;
                }

                PlcIp = _host;
                PlcConnectionType = ConnectionTypes[0];
                PlcPort = row.Port.ToString();
                PlcDataFormat = row.DataFormat;
                PlcPoll = row.PollMs + " ms";
                PlcReadTimeout = row.ReadTimeoutMs + " ms";
                PlcWriteTimeout = row.WriteTimeoutMs + " ms";
                PlcReconnectInterval = row.ReconnectIntervalMs + " ms";
                PlcReconnectCount = row.ReconnectCount.ToString();
            }
            finally
            {
                _suppressLinkSave = false;
            }
        }

        /// <summary>
        /// 客户库第一次打开时的 PLC 连接。
        /// </summary>
        private static PlcLinkEntity DefaultLink()
        {
            return new PlcLinkEntity
            {
                Id = Guid.NewGuid(),
                Host = "192.168.1.10",
                Protocol = (int)PlcProtocol.ModbusTcp,
                Port = 502,
                DataFormat = "ABCD",
                Station = 1,
                PollMs = 200,
                ReadTimeoutMs = 1000,
                WriteTimeoutMs = 1000,
                ReconnectIntervalMs = 3000,
                ReconnectCount = 3
            };
        }

        /// <summary>
        /// 按排序读点位，并尽量恢复原来选中的行。
        /// </summary>
        private void LoadPoints(Guid? selectedId)
        {
            var rows = SqlSugarContext.CustomerDb.Queryable<PlcPointEntity>()
                .OrderBy(item => item.SortIndex)
                .ToList();

            SelectedPoint = null;
            Points.Clear();
            foreach (var entity in rows)
            {
                var row = new PlcPointRow
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Address = entity.Address,
                    DataType = entity.DataType.ToString(),
                    LengthText = entity.DataLength.ToString(),
                    AccessText = entity.Access == PlcAccess.ReadOnly ? Text("Device_AccessRead") : Text("Device_AccessReadWrite"),
                    PointKind = PlcPointEntity.NormalizeKind(entity.PointKind),
                    BitIndex = PlcPointEntity.NormalizeBitIndex(entity.DataType, entity.DataLength, entity.BitIndex),
                    Description = PlcPointEntity.NormalizeDescription(entity.Description)
                };
                row.UseEnabledLabels(Text("Device_On"), Text("Device_Off"), entity.IsValid);
                Points.Add(row);
            }

            SelectedPoint = Points.FirstOrDefault(item => item.Id == selectedId) ?? Points.FirstOrDefault();
        }

        /// <summary>
        /// 按排序读扫码枪，并尽量恢复原来选中的卡片。
        /// </summary>
        private void LoadScanners(Guid? selectedId)
        {
            var rows = SqlSugarContext.CustomerDb.Queryable<ScannerEntity>()
                .OrderBy(item => item.SortIndex)
                .ToList();

            DetachScanners();
            SelectedScanner = null;
            Scanners.Clear();
            foreach (var entity in rows)
                Scanners.Add(CreateCard(entity));

            SelectedScanner = Scanners.FirstOrDefault(item => item.Id == selectedId) ?? Scanners.FirstOrDefault();
        }

        /// <summary>
        /// 客户库里的一把枪转成卡片。先填完字段，再订阅修改。
        /// </summary>
        private ScannerCard CreateCard(ScannerEntity entity)
        {
            var transport = entity.Transport == (int)ScanTransport.Serial ? ScanTransport.Serial : ScanTransport.Tcp;
            var card = new ScannerCard
            {
                Id = entity.Id,
                Name = entity.Name,
                Ip = entity.Host,
                Port = transport == ScanTransport.Tcp && entity.Port > 0 ? entity.Port.ToString() : "",
                TriggerText = entity.TriggerText,
                Transport = transport,
                BaudRate = entity.BaudRate,
                Parity = string.IsNullOrWhiteSpace(entity.Parity) ? "None" : entity.Parity,
                DataBits = entity.DataBits,
                StopBits = string.IsNullOrWhiteSpace(entity.StopBits) ? "One" : entity.StopBits,
                ConnectTimeout = entity.ConnectTimeout,
                ReceiveTimeout = entity.ReceiveTimeout,
                NewLine = string.IsNullOrEmpty(entity.NewLine) ? "\r" : entity.NewLine,
                EncodingName = string.IsNullOrWhiteSpace(entity.EncodingName) ? "ASCII" : entity.EncodingName
            };
            card.UseEnabledLabels(Text("Device_On"), Text("Device_Off"), entity.IsValid);
            card.PropertyChanged += OnScannerChanged;
            return card;
        }

        /// <summary>
        /// 清空点位和扫码枪。页面日志保留。
        /// </summary>
        private void ClearDeviceLists()
        {
            SelectedPoint = null;
            Points.Clear();
            DetachScanners();
            SelectedScanner = null;
            Scanners.Clear();
        }

        /// <summary>
        /// 取消扫码枪卡片的保存订阅。
        /// </summary>
        private void DetachScanners()
        {
            foreach (var card in Scanners)
                card.PropertyChanged -= OnScannerChanged;
        }

        /// <summary>
        /// 表单失焦或下拉变化后，把能解析的连接参数写回客户库。
        /// </summary>
        private void CommitLink()
        {
            if (_suppressLinkSave || _linkId == Guid.Empty || !SqlSugarContext.IsOpen)
                return;

            if (!TryReadLink(out var host, out var port, out var format, out var pollMs, out var readTimeout, out var writeTimeout, out var reconnectInterval, out var reconnectCount))
                return;

            var connectionChanged = host != _host || port != _port || format != _dataFormat || readTimeout != _readTimeout;
            var pollChanged = pollMs != _pollMs;
            if (!connectionChanged
                && !pollChanged
                && writeTimeout == _writeTimeout
                && reconnectInterval == _reconnectInterval
                && reconnectCount == _reconnectCount)
                return;

            try
            {
                var db = SqlSugarContext.CustomerDb;
                var row = db.Queryable<PlcLinkEntity>().Where(item => item.Id == _linkId).ToList().FirstOrDefault();
                if (row is null)
                    return;

                row.Host = host;
                row.Protocol = (int)PlcProtocol.ModbusTcp;
                row.Port = port;
                row.DataFormat = format;
                row.Station = _station;
                row.PollMs = pollMs;
                row.ReadTimeoutMs = readTimeout;
                row.WriteTimeoutMs = writeTimeout;
                row.ReconnectIntervalMs = reconnectInterval;
                row.ReconnectCount = reconnectCount;
                db.Updateable(row).ExecuteCommand();

                lock (_plcLock)
                {
                    _host = host;
                    _port = port;
                    _dataFormat = format;
                    _pollMs = pollMs;
                    _readTimeout = readTimeout;
                    _writeTimeout = writeTimeout;
                    _reconnectInterval = reconnectInterval;
                    _reconnectCount = reconnectCount;
                    if (connectionChanged)
                        ClosePlcLocked();
                }
            }
            catch (Exception ex)
            {
                NotePlc(DeviceLogKind.Timeout, ex.Message, true);
            }
        }

        /// <summary>
        /// 从文本框取出连接参数。某个数字不合法时，沿用已经保存的值。
        /// </summary>
        private bool TryReadLink(
            out string host,
            out int port,
            out string format,
            out int pollMs,
            out int readTimeout,
            out int writeTimeout,
            out int reconnectInterval,
            out int reconnectCount)
        {
            host = PlcIp.Trim();
            if (host.Length == 0)
                host = _host;

            if (!TryReadNumber(PlcPort, out port) || port <= 0)
                port = _port;

            format = (PlcDataFormat ?? "").Trim();
            if (format is not ("ABCD" or "CDAB" or "BADC" or "DCBA"))
                format = _dataFormat;

            if (!TryReadNumber(PlcPoll, out pollMs) || pollMs < 0)
                pollMs = _pollMs;

            if (!TryReadNumber(PlcReadTimeout, out readTimeout) || readTimeout <= 0)
                readTimeout = _readTimeout;

            if (!TryReadNumber(PlcWriteTimeout, out writeTimeout) || writeTimeout <= 0)
                writeTimeout = _writeTimeout;

            if (!TryReadNumber(PlcReconnectInterval, out reconnectInterval) || reconnectInterval < 0)
                reconnectInterval = _reconnectInterval;

            if (!TryReadNumber(PlcReconnectCount, out reconnectCount) || reconnectCount < 0)
                reconnectCount = _reconnectCount;

            return host.Length > 0
                && port > 0
                && readTimeout > 0
                && writeTimeout > 0
                && format is "ABCD" or "CDAB" or "BADC" or "DCBA";
        }

        /// <summary>
        /// 取出文本里的第一个整数，例如 200 ms。
        /// </summary>
        private static bool TryReadNumber(string text, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var match = Regex.Match(text, @"-?\d+");
            return match.Success && int.TryParse(match.Value, out value);
        }

        /// <summary>
        /// 新增或更新一个点位。
        /// </summary>
        private void StorePoint(PlcPointRow row)
        {
            if (!SqlSugarContext.IsOpen)
                return;

            try
            {
                var db = SqlSugarContext.CustomerDb;
                var sort = Points.IndexOf(row);
                if (sort < 0)
                    sort = 0;

                var entity = ToPoint(row, sort);
                if (PlcPointEntity.IsSingleton(entity.PointKind))
                {
                    var another = db.Queryable<PlcPointEntity>()
                        .Where(item => item.PointKind == entity.PointKind && item.Id != entity.Id)
                        .ToList()
                        .FirstOrDefault();
                    if (another is not null)
                    {
                        NotePlc(DeviceLogKind.Timeout, Text("Device_SingletonPoint"), true);
                        return;
                    }
                }

                var existing = db.Queryable<PlcPointEntity>().Where(item => item.Id == row.Id).ToList().FirstOrDefault();
                if (existing is null)
                    db.Insertable(entity).ExecuteCommand();
                else
                    db.Updateable(entity).ExecuteCommand();
            }
            catch (Exception ex)
            {
                NotePlc(DeviceLogKind.Timeout, ex.Message, true);
            }
        }

        /// <summary>
        /// 按当前列表重写点位顺序。
        /// </summary>
        private void StorePointOrder()
        {
            if (!SqlSugarContext.IsOpen)
                return;

            try
            {
                var db = SqlSugarContext.CustomerDb;
                for (var index = 0; index < Points.Count; index++)
                {
                    var sort = index;
                    var id = Points[index].Id;
                    db.Updateable<PlcPointEntity>()
                        .SetColumns(item => item.SortIndex == sort)
                        .Where(item => item.Id == id)
                        .ExecuteCommand();
                }
            }
            catch (Exception ex)
            {
                NotePlc(DeviceLogKind.Timeout, ex.Message, true);
            }
        }

        /// <summary>
        /// 删除已经从列表拿掉的点位，再重写顺序。
        /// </summary>
        private void RemovePoints(IReadOnlyList<PlcPointRow> rows)
        {
            if (!SqlSugarContext.IsOpen || rows.Count == 0)
                return;

            try
            {
                var ids = rows.Select(item => item.Id).ToList();
                SqlSugarContext.CustomerDb.Deleteable<PlcPointEntity>()
                    .Where(item => ids.Contains(item.Id))
                    .ExecuteCommand();
                StorePointOrder();
            }
            catch (Exception ex)
            {
                NotePlc(DeviceLogKind.Timeout, ex.Message, true);
            }
        }

        /// <summary>
        /// 用导入结果替换客户库里的全部点位。失败时原表保留。
        /// </summary>
        private void ReplacePoints(IReadOnlyList<PlcPointRow> imported)
        {
            if (!SqlSugarContext.IsOpen)
                throw new InvalidOperationException(Text("Device_DbClosed"));

            var entities = new List<PlcPointEntity>(imported.Count);
            for (var index = 0; index < imported.Count; index++)
                entities.Add(ToPoint(imported[index], index));

            if (PlcPointEntity.ExtraSingleton(entities.Select(item => item.PointKind)) is not null)
                throw new InvalidDataException(Text("Device_SingletonPoint"));

            var db = SqlSugarContext.CustomerDb;
            db.Ado.BeginTran();
            try
            {
                var ids = db.Queryable<PlcPointEntity>().Select(item => item.Id).ToList();
                if (ids.Count > 0)
                {
                    db.Deleteable<PlcPointEntity>()
                        .Where(item => ids.Contains(item.Id))
                        .ExecuteCommand();
                }

                if (entities.Count > 0)
                    db.Insertable(entities).ExecuteCommand();

                db.Ado.CommitTran();
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 点位行转成客户库实体。
        /// </summary>
        private PlcPointEntity ToPoint(PlcPointRow row, int sort)
        {
            Enum.TryParse<PlcDataType>(row.DataType, out var dataType);
            if (!Enum.IsDefined(dataType))
                dataType = PlcDataType.Int16;

            var length = int.TryParse(row.LengthText, out var lengthValue) && lengthValue > 0 ? lengthValue : 1;
            var access = row.AccessText == Text("Device_AccessRead") ? PlcAccess.ReadOnly : PlcAccess.ReadWrite;
            return new PlcPointEntity
            {
                Id = row.Id,
                Name = row.Name,
                Address = row.Address,
                DataType = dataType,
                DataLength = length,
                Access = access,
                IsValid = row.IsEnabled,
                SortIndex = sort,
                PointKind = PlcPointEntity.NormalizeKind(row.PointKind),
                BitIndex = PlcPointEntity.NormalizeBitIndex(dataType, length, row.BitIndex),
                Description = PlcPointEntity.NormalizeDescription(row.Description)
            };
        }

        /// <summary>
        /// 传输方式的显示文字。
        /// </summary>
        private string TransportText(ScanTransport transport)
        {
            return transport == ScanTransport.Serial ? Text("Device_Serial") : Text("Device_Tcp");
        }

        /// <summary>
        /// 下拉文字转成传输方式。
        /// </summary>
        private ScanTransport TransportOf(string text)
        {
            return text == Text("Device_Serial") ? ScanTransport.Serial : ScanTransport.Tcp;
        }

        /// <summary>
        /// 名称、地址、端口或触发字符串变化后保存。触发区只读，不从那里写入。地址没填完整时不覆盖原行。
        /// </summary>
        private void OnScannerChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_suppressScanSave || sender is not ScannerCard card)
                return;

            var name = e.PropertyName;
            if (name is not (nameof(ScannerCard.Name) or nameof(ScannerCard.Ip) or nameof(ScannerCard.Port) or nameof(ScannerCard.TriggerText)))
                return;

            if (string.IsNullOrWhiteSpace(card.Ip))
                return;

            if (card.IsTcp && (!int.TryParse(card.Port, out var port) || port <= 0))
                return;

            StoreScanner(card);
            if (name is nameof(ScannerCard.Ip) or nameof(ScannerCard.Port) or nameof(ScannerCard.TriggerText))
                DropScanner(card.Id);
        }

        /// <summary>
        /// 新增或更新一把扫码枪。串口参数沿用卡片上的值。
        /// </summary>
        private void StoreScanner(ScannerCard card)
        {
            if (!SqlSugarContext.IsOpen)
                return;

            try
            {
                var db = SqlSugarContext.CustomerDb;
                var sort = Scanners.IndexOf(card);
                if (sort < 0)
                    sort = 0;

                var entity = ToScanner(card, sort);
                var existing = db.Queryable<ScannerEntity>().Where(item => item.Id == card.Id).ToList().FirstOrDefault();
                if (existing is null)
                    db.Insertable(entity).ExecuteCommand();
                else
                    db.Updateable(entity).ExecuteCommand();
            }
            catch (Exception ex)
            {
                NoteScan(DeviceLogKind.Timeout, ex.Message, true);
            }
        }

        /// <summary>
        /// 按当前卡片重写扫码枪顺序。
        /// </summary>
        private void StoreScannerOrder()
        {
            if (!SqlSugarContext.IsOpen)
                return;

            try
            {
                var db = SqlSugarContext.CustomerDb;
                for (var index = 0; index < Scanners.Count; index++)
                {
                    var sort = index;
                    var id = Scanners[index].Id;
                    db.Updateable<ScannerEntity>()
                        .SetColumns(item => item.SortIndex == sort)
                        .Where(item => item.Id == id)
                        .ExecuteCommand();
                }
            }
            catch (Exception ex)
            {
                NoteScan(DeviceLogKind.Timeout, ex.Message, true);
            }
        }

        /// <summary>
        /// 删除一把扫码枪，并释放它的客户端。
        /// </summary>
        private void RemoveScanner(ScannerCard card)
        {
            DropScanner(card.Id);
            if (!SqlSugarContext.IsOpen)
                return;

            try
            {
                SqlSugarContext.CustomerDb.Deleteable<ScannerEntity>()
                    .Where(item => item.Id == card.Id)
                    .ExecuteCommand();
                StoreScannerOrder();
            }
            catch (Exception ex)
            {
                NoteScan(DeviceLogKind.Timeout, ex.Message, true);
            }
        }

        /// <summary>
        /// 扫码枪卡片转成客户库实体。
        /// </summary>
        private static ScannerEntity ToScanner(ScannerCard card, int sort)
        {
            var port = 0;
            if (card.Transport == ScanTransport.Tcp && int.TryParse(card.Port, out var parsed) && parsed > 0)
                port = parsed;

            return new ScannerEntity
            {
                Id = card.Id,
                Name = card.Name ?? "",
                Transport = (int)card.Transport,
                Host = card.Ip ?? "",
                Port = port,
                TriggerText = card.TriggerText ?? "",
                BaudRate = card.BaudRate,
                Parity = string.IsNullOrWhiteSpace(card.Parity) ? "None" : card.Parity,
                DataBits = card.DataBits,
                StopBits = string.IsNullOrWhiteSpace(card.StopBits) ? "One" : card.StopBits,
                ConnectTimeout = card.ConnectTimeout,
                ReceiveTimeout = card.ReceiveTimeout,
                NewLine = string.IsNullOrEmpty(card.NewLine) ? "\r" : card.NewLine,
                EncodingName = string.IsNullOrWhiteSpace(card.EncodingName) ? "ASCII" : card.EncodingName,
                IsValid = card.IsEnabled,
                SortIndex = sort
            };
        }

        /// <summary>
        /// 丢掉一把枪的客户端。下次触发按新地址重建。
        /// </summary>
        private void DropScanner(Guid id)
        {
            lock (_scanLock)
            {
                if (!_scanClients.Remove(id, out var client))
                    return;

                _scanSignatures.Remove(id);
                client.Dispose();
            }
        }

        /// <summary>
        /// 记下一条 PLC 页面日志，并写入运行日志。
        /// </summary>
        private void NotePlc(DeviceLogKind kind, string text, bool error)
        {
            Post(() =>
            {
                AddPlc(kind, text);
                if (error)
                    LogHelper.PlcError(text);
                else
                    LogHelper.PlcInfo(text);
            });
        }

        /// <summary>
        /// 记下一条扫码页面日志，并写入运行日志。
        /// </summary>
        private void NoteScan(DeviceLogKind kind, string text, bool error)
        {
            Post(() =>
            {
                AddScan(kind, text);
                if (error)
                    LogHelper.ScanError(text);
                else
                    LogHelper.ScanInfo(text);
            });
        }

        /// <summary>
        /// 在后台连接并读取或写入。离开页面后返回空。
        /// </summary>
        private Task<PlcIoResult?> ExchangePlc(string address, string typeName, string lengthText, string? writeValue)
        {
            var session = Session;
            if (session == 0)
                return Task.FromResult<PlcIoResult?>(null);

            if (!Enum.TryParse(typeName, out PlcDataType dataType) || !Enum.IsDefined(dataType))
                return Task.FromResult<PlcIoResult?>(PlcIoResult.Fail(Text("Device_BadType")));

            if (!int.TryParse(lengthText, out var length) || length <= 0)
                return Task.FromResult<PlcIoResult?>(PlcIoResult.Fail(Text("Device_NeedLength")));

            var life = _life.Token;
            return Task.Run(() =>
            {
                if (session != Session || life.IsCancellationRequested)
                    return null;

                var connected = ConnectPlc(session, life);
                if (session != Session || life.IsCancellationRequested)
                    return null;

                if (!connected.IsSuccess)
                    return connected;

                lock (_plcLock)
                {
                    if (session != Session || _plc is null)
                        return session == Session ? PlcIoResult.Fail(ReleasedMessage) : null;

                    try
                    {
                        return writeValue is null
                            ? _plc.Read(address, dataType, length)
                            : _plc.Write(address, dataType, writeValue);
                    }
                    catch (Exception ex)
                    {
                        return PlcIoResult.Fail(ex.Message);
                    }
                }
            });
        }

        /// <summary>
        /// 在后台连接并触发。参数在进入后台前从卡片抄出。
        /// </summary>
        private Task<ScanIoResult?> ExchangeScan(ScannerCard scanner, string trigger)
        {
            var session = Session;
            var life = _life.Token;
            var id = scanner.Id;
            var connection = new ScanConnection
            {
                Transport = scanner.Transport,
                Host = scanner.Ip ?? "",
                Port = int.TryParse(scanner.Port, out var port) ? port : 0,
                BaudRate = scanner.BaudRate,
                Parity = scanner.Parity,
                DataBits = scanner.DataBits,
                StopBits = scanner.StopBits,
                ConnectTimeout = scanner.ConnectTimeout,
                ReceiveTimeout = scanner.ReceiveTimeout,
                NewLine = scanner.NewLine,
                EncodingName = scanner.EncodingName
            };
            var signature = ScanSignature(connection, trigger);
            return Task.Run(() =>
            {
                if (session != Session || life.IsCancellationRequested)
                    return null;

                try
                {
                    lock (_scanLock)
                    {
                        if (session != Session || life.IsCancellationRequested)
                            return null;

                        if (!_scanClients.TryGetValue(id, out var client) || _scanSignatures.GetValueOrDefault(id) != signature)
                        {
                            if (_scanClients.Remove(id, out var old))
                            {
                                _scanSignatures.Remove(id);
                                old.Dispose();
                            }

                            client = ScanClients.Open(connection);
                            _scanClients[id] = client;
                            _scanSignatures[id] = signature;
                        }

                        if (!client.IsConnected)
                        {
                            var connected = client.Connect();
                            if (!connected.IsSuccess)
                                return connected;
                        }

                        if (session != Session || life.IsCancellationRequested)
                            return null;

                        var result = client.Trigger(trigger);
                        return session == Session ? result : null;
                    }
                }
                catch (Exception ex)
                {
                    return ScanIoResult.Fail(ex.Message);
                }
            });
        }

        /// <summary>
        /// 按已保存的参数连接。第一次失败后，按重连间隔再试重连次数那么多次。
        /// </summary>
        private PlcIoResult ConnectPlc(int session, CancellationToken token)
        {
            lock (_plcLock)
            {
                if (session != Session || token.IsCancellationRequested)
                    return PlcIoResult.Fail(ReleasedMessage);

                var host = _host;
                var port = _port;
                var format = _dataFormat;
                var timeout = _readTimeout;
                var station = _station;
                var extra = _reconnectCount;
                var waitMs = _reconnectInterval;
                var signature = PlcSignature(host, port, format, timeout, station);
                if (_plc is null || _plcSignature != signature)
                {
                    ClosePlcLocked();
                    try
                    {
                        _plc = PlcClients.Open(new PlcConnection
                        {
                            Protocol = PlcProtocol.ModbusTcp,
                            Host = host,
                            Port = port,
                            Station = ClampStation(station),
                            DataFormat = format,
                            ConnectTimeout = timeout,
                            ReceiveTimeout = timeout
                        });
                        _plcSignature = signature;
                    }
                    catch (Exception ex)
                    {
                        _plc = null;
                        _plcSignature = "";
                        return PlcIoResult.Fail(ex.Message);
                    }
                }

                var mine = _plc;
                var generation = _plcGeneration;
                if (mine.IsConnected)
                    return PlcIoResult.Ok("");

                var failure = CallConnect(mine);
                if (session != Session)
                    return AbandonPlc(session, mine, generation);

                if (token.IsCancellationRequested)
                    return PlcIoResult.Fail(ReleasedMessage);

                if (failure.IsSuccess)
                    return failure;

                for (var attempt = 0; attempt < extra; attempt++)
                {
                    if (!WaitReconnect(token, waitMs) || session != Session || token.IsCancellationRequested)
                        return AbandonPlc(session, mine, generation);

                    if (!ReferenceEquals(_plc, mine))
                        return PlcIoResult.Fail(ReleasedMessage);

                    if (mine.IsConnected)
                        return PlcIoResult.Ok("");

                    failure = CallConnect(mine);
                    if (session != Session)
                        return AbandonPlc(session, mine, generation);

                    if (token.IsCancellationRequested)
                        return PlcIoResult.Fail(ReleasedMessage);

                    if (failure.IsSuccess)
                        return failure;
                }

                return failure;
            }
        }

        /// <summary>
        /// 调用连接。异常变成失败结果。
        /// </summary>
        private static PlcIoResult CallConnect(IPlcClient client)
        {
            try
            {
                return client.Connect();
            }
            catch (Exception ex)
            {
                return PlcIoResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 这次进入已经结束，并且客户端还是这一次连上的，就把它释放。
        /// </summary>
        private PlcIoResult AbandonPlc(int session, IPlcClient mine, int generation)
        {
            if (session != Session && _plcGeneration == generation && ReferenceEquals(_plc, mine))
                ClosePlcLocked();

            return PlcIoResult.Fail(ReleasedMessage);
        }

        /// <summary>
        /// 放开锁等待重连间隔。返回时仍然持有锁。取消时返回 false。
        /// </summary>
        private bool WaitReconnect(CancellationToken token, int waitMs)
        {
            Monitor.Exit(_plcLock);
            try
            {
                if (waitMs <= 0)
                    return !token.IsCancellationRequested;

                return !token.WaitHandle.WaitOne(waitMs);
            }
            finally
            {
                Monitor.Enter(_plcLock);
            }
        }

        /// <summary>
        /// 站号收成一个字节。
        /// </summary>
        private static byte ClampStation(int station)
        {
            if (station < 0)
                return 0;

            if (station > 255)
                return 255;

            return (byte)station;
        }

        /// <summary>
        /// 地址、端口、字节序、读取超时和站号拼成客户端签名。
        /// </summary>
        private static string PlcSignature(string host, int port, string format, int timeout, int station)
        {
            return host + "\u001f" + port + "\u001f" + format + "\u001f" + timeout + "\u001f" + station;
        }

        /// <summary>
        /// 连接参数和触发字符串拼成扫码客户端签名。
        /// </summary>
        private static string ScanSignature(ScanConnection connection, string trigger)
        {
            return string.Join(
                "\u001f",
                connection.Transport,
                connection.Host,
                connection.Port,
                connection.BaudRate,
                connection.Parity,
                connection.DataBits,
                connection.StopBits,
                connection.ConnectTimeout,
                connection.ReceiveTimeout,
                connection.NewLine,
                connection.EncodingName,
                trigger);
        }

        /// <summary>
        /// 释放 PLC 和全部扫码枪。
        /// </summary>
        private void CloseDevices()
        {
            ClosePlc();
            lock (_scanLock)
            {
                foreach (var client in _scanClients.Values)
                    client.Dispose();

                _scanClients.Clear();
                _scanSignatures.Clear();
            }
        }

        /// <summary>
        /// 释放当前 PLC 客户端。
        /// </summary>
        private void ClosePlc()
        {
            lock (_plcLock)
                ClosePlcLocked();
        }

        /// <summary>
        /// 在已经持有 PLC 锁时释放客户端。
        /// </summary>
        private void ClosePlcLocked()
        {
            _plcGeneration++;
            var client = _plc;
            _plc = null;
            _plcSignature = "";
            client?.Dispose();
        }

        /// <summary>
        /// 取消这一次停留上还在进行的读写。
        /// </summary>
        private void CancelLife()
        {
            try
            {
                _life.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        /// <summary>
        /// 进入或离开页面时推进代数。
        /// </summary>
        private int BumpSession()
        {
            return Interlocked.Increment(ref _session);
        }

        /// <summary>
        /// 当前页面代数。
        /// </summary>
        private int Session => Volatile.Read(ref _session);

        /// <summary>
        /// 把界面更新送回界面线程。已经在界面线程上就直接执行。
        /// </summary>
        private static void Post(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null || dispatcher.CheckAccess())
            {
                action();
                return;
            }

            dispatcher.BeginInvoke(action);
        }
    }
}
