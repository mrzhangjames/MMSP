using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// 单扫码工具的编辑模型。
    /// </summary>
    public sealed class SingleScanToolViewModel : ToolEditViewModel<SingleScanToolEntity>
    {
        /// <summary>
        /// 用一份空配置创建。
        /// </summary>
        public SingleScanToolViewModel()
            : this(new SingleScanToolEntity(), [])
        {
        }

        /// <summary>
        /// 编辑一份已有的单扫码配置。scanners 是设备里的扫码枪。
        /// </summary>
        public SingleScanToolViewModel(SingleScanToolEntity tool, IReadOnlyList<string> scanners)
            : base(tool)
        {
            ArgumentNullException.ThrowIfNull(scanners);
            Scanners = ScannerList.Merge(scanners, tool.Scanner);
        }

        /// <summary>
        /// 扫码枪下拉项。
        /// </summary>
        public IReadOnlyList<string> Scanners { get; }

        /// <summary>
        /// 扫码枪名。
        /// </summary>
        public string Scanner
        {
            get => Tool.Scanner;
            set
            {
                if (value is null)
                    return;

                SetText(Tool.Scanner, value, text => Tool.Scanner = text);
            }
        }

        /// <summary>
        /// 触发字符串。
        /// </summary>
        public string Trigger
        {
            get => Tool.Trigger;
            set => SetText(Tool.Trigger, value, text => Tool.Trigger = text);
        }

        /// <summary>
        /// 扫码结果写入的流程变量。与右侧出参是同一个名称。
        /// </summary>
        public string Result
        {
            get => Tool.Output;
            set => SetText(Tool.Output, value, text => Tool.Output = text);
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
    }
}
