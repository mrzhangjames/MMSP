using Prism.Mvvm;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// 多扫码下拉里的一把枪。
    /// </summary>
    public sealed class ScannerChoice : BindableBase
    {
        private readonly Action _changed;
        private bool _isChecked;

        /// <summary>
        /// 创建一项。勾选变化时通知编辑模型。
        /// </summary>
        public ScannerChoice(string name, bool isChecked, Action changed)
        {
            Name = name;
            _isChecked = isChecked;
            _changed = changed;
        }

        /// <summary>
        /// 扫码枪名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 是否选中。
        /// </summary>
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (!SetProperty(ref _isChecked, value))
                    return;

                _changed();
            }
        }
    }
}
