using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;

namespace MMSP.Plugin.Lux.ViewModels
{
    /// <summary>
    /// 立讯报警日志表格的编辑模型。只改配置，不执行读取。
    /// </summary>
    public sealed class LuxAlarmLogToolViewModel
    {
        /// <summary>
        /// 与属性窗口相同的 Json 选项。属性名忽略大小写，不改命名。
        /// </summary>
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// CSV 列。顺序与表格一致。记录日志不在文件里。
        /// </summary>
        private static readonly string[] Columns = ["地址", "数据类型", "触发值", "报警描述"];

        /// <summary>
        /// 正在编辑的工具。
        /// </summary>
        private readonly LuxAlarmLogToolEntity _tool;

        /// <summary>
        /// 用一份空配置创建。
        /// </summary>
        public LuxAlarmLogToolViewModel()
            : this(new LuxAlarmLogToolEntity())
        {
        }

        /// <summary>
        /// 编辑一份已有的报警配置。
        /// </summary>
        public LuxAlarmLogToolViewModel(LuxAlarmLogToolEntity tool)
        {
            _tool = tool;
            _tool.Rows ??= [];
            Rows = new ObservableCollection<LuxAlarmLogRow>(_tool.Rows.Where(row => row is not null));
        }

        /// <summary>
        /// 表格行。新行由表格创建，默认 UInt16。
        /// </summary>
        public ObservableCollection<LuxAlarmLogRow> Rows { get; }

        /// <summary>
        /// 整份工具是否记录上升沿。不放进表格。
        /// </summary>
        public bool Log
        {
            get => _tool.Log;
            set => _tool.Log = value;
        }

        /// <summary>
        /// 数据类型下拉项。UInt16 放在第一位。
        /// </summary>
        public IReadOnlyList<PlcDataType> DataTypes { get; } =
        [
            PlcDataType.UInt16,
            PlcDataType.Int16,
            PlcDataType.Int32,
            PlcDataType.Float,
            PlcDataType.String
        ];

        /// <summary>
        /// 保存时交给属性窗口的配置。地址为空的行丢掉，表格集合本身不换。
        /// </summary>
        public PluginToolModel EditedTool
        {
            get
            {
                _tool.Rows = Rows
                    .Where(row => row is not null && !string.IsNullOrWhiteSpace(row.Address))
                    .Select(row => new LuxAlarmLogRow
                    {
                        Address = row.Address.Trim(),
                        DataType = row.DataType,
                        TriggerValue = row.TriggerValue ?? "",
                        Description = row.Description ?? ""
                    })
                    .ToList();
                return _tool;
            }
        }

        /// <summary>
        /// 选择 CSV 后替换表格。取消对话框或格式不对时保留当前行。
        /// </summary>
        public void Import()
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
                var imported = ReadRows(CsvFileHelper.ReadTable(dialog.FileName));
                Rows.Clear();
                foreach (var row in imported)
                    Rows.Add(row);
            }
            catch (Exception)
            {
                MetroDialogService.ShowModalMessage("导入", "文件格式不正确");
            }
        }

        /// <summary>
        /// 把地址不为空的行存成 CSV。取消对话框则不写文件。
        /// </summary>
        public void Export()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = "LUX报警.csv",
                DefaultExt = ".csv",
                AddExtension = true
            };
            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var table = Rows
                    .Where(row => row is not null && !string.IsNullOrWhiteSpace(row.Address))
                    .Select(row => (IReadOnlyDictionary<string, string>)new Dictionary<string, string>
                    {
                        ["地址"] = row.Address.Trim(),
                        ["数据类型"] = row.DataType.ToString(),
                        ["触发值"] = row.TriggerValue ?? "",
                        ["报警描述"] = row.Description ?? ""
                    });
                CsvFileHelper.WriteTable(dialog.FileName, Columns, table);
            }
            catch (Exception)
            {
                MetroDialogService.ShowModalMessage("导出", "文件无法保存");
            }
        }

        /// <summary>
        /// 把表头表转成报警行。缺列、地址为空或类型认不出时整份失败。
        /// </summary>
        private static List<LuxAlarmLogRow> ReadRows(CsvTable table)
        {
            foreach (var name in Columns)
            {
                if (!table.Headers.Contains(name, StringComparer.Ordinal))
                    throw new InvalidDataException();
            }

            var rows = new List<LuxAlarmLogRow>();
            foreach (var record in table.Rows)
            {
                var address = Cell(record, "地址").Trim();
                var typeText = Cell(record, "数据类型").Trim();
                var trigger = Cell(record, "触发值").Trim();
                var description = Cell(record, "报警描述").Trim();
                if (address.Length == 0 && typeText.Length == 0 && trigger.Length == 0 && description.Length == 0)
                    continue;

                if (address.Length == 0 || !Enum.TryParse(typeText, ignoreCase: true, out PlcDataType dataType) || !Enum.IsDefined(dataType))
                    throw new InvalidDataException();

                rows.Add(new LuxAlarmLogRow
                {
                    Address = address,
                    DataType = dataType,
                    TriggerValue = trigger,
                    Description = description
                });
            }

            return rows;
        }

        /// <summary>
        /// 取出一列。没有这一列时返回空。
        /// </summary>
        private static string Cell(IReadOnlyDictionary<string, string> row, string name)
        {
            return row.TryGetValue(name, out var value) ? value ?? "" : "";
        }

        /// <summary>
        /// 从步骤配置读出表格。空白或损坏时用空表，和其它工具的编辑窗口一样。
        /// </summary>
        public static LuxAlarmLogToolViewModel FromCfg(string cfg)
        {
            if (string.IsNullOrWhiteSpace(cfg))
                return new LuxAlarmLogToolViewModel(new LuxAlarmLogToolEntity());

            try
            {
                var tool = JsonSerializer.Deserialize<LuxAlarmLogToolEntity>(cfg, Json) ?? new LuxAlarmLogToolEntity();
                tool.Rows ??= [];
                return new LuxAlarmLogToolViewModel(tool);
            }
            catch (JsonException)
            {
                return new LuxAlarmLogToolViewModel(new LuxAlarmLogToolEntity());
            }
        }
    }
}
