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
    }
}
