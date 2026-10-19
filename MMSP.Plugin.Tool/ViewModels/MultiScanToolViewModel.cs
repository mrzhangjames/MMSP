using System.Collections.ObjectModel;
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
            var selected = ScannerList.Split(tool.Scanner);
            Choices = new ObservableCollection<ScannerChoice>(
                ScannerList.Merge(scanners, tool.Scanner)
                    .Select(name => new ScannerChoice(name, selected.Contains(name), WriteScanner)));
        }

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
        /// 要读取的条码个数。
        /// </summary>
        public int Count
        {
            get => Tool.Count;
            set => SetNumber(Tool.Count, value, number => Tool.Count = number);
        }

        /// <summary>
        /// 把勾选的枪名写回配置。
        /// </summary>
        private void WriteScanner()
        {
            var text = string.Join("、", Choices.Where(item => item.IsChecked).Select(item => item.Name));
            SetText(Tool.Scanner, text, value => Tool.Scanner = value, nameof(Scanner));
        }
    }
}
