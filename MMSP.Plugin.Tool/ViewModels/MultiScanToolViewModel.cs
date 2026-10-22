using System.Collections.ObjectModel;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// 多扫码工具的编辑模型。
    /// </summary>
    public sealed class MultiScanToolViewModel : ToolEditViewModel<MultiScanToolEntity>
    {
        /// <summary>
        /// 用一份空配置创建。
        /// </summary>
        public MultiScanToolViewModel()
            : this(new MultiScanToolEntity(), [])
        {
        }

        /// <summary>
        /// 编辑一份已有的多扫码配置。scanners 是设备里的扫码枪。
        /// </summary>
        public MultiScanToolViewModel(MultiScanToolEntity tool, IReadOnlyList<string> scanners)
            : base(tool)
        {
            ArgumentNullException.ThrowIfNull(scanners);
            ToolVars.FitSlots(tool, ToolVars.OutputSlotCount(tool));
            var selected = ScannerList.Split(tool.Scanner);
            Choices = new ObservableCollection<ScannerChoice>(
                ScannerList.Merge(scanners, tool.Scanner)
                    .Select(name => new ScannerChoice(name, selected.Contains(name), WriteScanner)));
            RefreshResults();
        }

        /// <summary>
        /// 扫码结果。一行一个条码，按枪名列出。
        /// </summary>
        public ObservableCollection<ScanResultRow> Results { get; } = [];

        /// <summary>
        /// 结果表高度。表头加每一行，整表放进中间滚动，不再在表内裁掉。
        /// </summary>
        public double ResultsHeight => 40d + Results.Count * 36d;

        /// <summary>
        /// 下拉中的扫码枪。
        /// </summary>
        public ObservableCollection<ScannerChoice> Choices { get; }

        /// <summary>
        /// 已选扫码枪。多把枪用顿号连接。
        /// </summary>
        public string Scanner => Tool.Scanner;

        /// <summary>
        /// 触发字符串。
        /// </summary>
        public string Trigger
        {
            get => Tool.Trigger;
            set => SetText(Tool.Trigger, value, text => Tool.Trigger = text);
        }

        /// <summary>
        /// 每把枪要读取的条码个数。出参槽按枪数乘这个数量伸缩。没选枪时按一把算。
        /// </summary>
        public int Count
        {
            get => Tool.Count;
            set
            {
                SetNumber(Tool.Count, value, number => Tool.Count = number);
                ToolVars.FitSlots(Tool, ToolVars.OutputSlotCount(Tool));
                RefreshResults();
            }
        }

        /// <summary>
        /// 写入 PLC 的数据类型下拉项。
        /// </summary>
        public IReadOnlyList<PlcDataType> DataTypes { get; } = Enum.GetValues<PlcDataType>();

        /// <summary>
        /// 扫到后是否写入 PLC。
        /// </summary>
        public bool WritePlc
        {
            get => Tool.WritePlc;
            set => SetValue(Tool.WritePlc, value, flag => Tool.WritePlc = flag);
        }

        /// <summary>
        /// 写入地址。
        /// </summary>
        public string WriteAddress
        {
            get => Tool.WriteAddress;
            set => SetText(Tool.WriteAddress, value, text => Tool.WriteAddress = text);
        }

        /// <summary>
        /// 写入的 PLC 数据类型。
        /// </summary>
        public PlcDataType WriteDataType
        {
            get => Tool.WriteDataType;
            set => SetValue(Tool.WriteDataType, value, item => Tool.WriteDataType = item);
        }

        /// <summary>
        /// 写入长度。
        /// </summary>
        public int WriteLength
        {
            get => Tool.WriteLength;
            set => SetNumber(Tool.WriteLength, value, number => Tool.WriteLength = number);
        }

        /// <summary>
        /// 把勾选的枪名写回配置。
        /// </summary>
        private void WriteScanner()
        {
            var text = string.Join("、", Choices.Where(item => item.IsChecked).Select(item => item.Name));
            SetText(Tool.Scanner, text, value => Tool.Scanner = value, nameof(Scanner));
            RefreshResults();
        }

        /// <summary>
        /// 按当前出参槽重画结果表。顺序与扫码相同：每把枪连续 Count 条。
        /// </summary>
        public void RefreshResults()
        {
            Results.Clear();
            var slots = ToolVars.Slots(Tool);
            var guns = ScannerList.Split(Tool.Scanner);
            var perGun = Tool.Count < 0 ? 0 : Tool.Count;
            for (var index = 0; index < slots.Count; index++)
            {
                var gunIndex = perGun > 0 ? index / perGun : 0;
                var gun = gunIndex < guns.Count ? guns[gunIndex] : "";
                Results.Add(new ScanResultRow(gun, (slots[index] ?? "").Trim()));
            }

            RaisePropertyChanged(nameof(ResultsHeight));
        }
    }

    /// <summary>
    /// 多扫码结果表的一行。
    /// </summary>
    public sealed class ScanResultRow
    {
        /// <summary>
        /// 创建一行。
        /// </summary>
        public ScanResultRow(string scanner, string variable)
        {
            Scanner = scanner ?? "";
            Variable = variable ?? "";
        }

        /// <summary>
        /// 扫码枪名称。还没选枪时为空。
        /// </summary>
        public string Scanner { get; }

        /// <summary>
        /// 这一格写入的流程变量。空表示还没选。
        /// </summary>
        public string Variable { get; }
    }
}
