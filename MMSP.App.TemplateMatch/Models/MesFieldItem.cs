using Prism.Mvvm;

namespace MMSP.App.TemplateMatch.Models
{
    /// <summary>
    /// MES 配置页上的一个输入项。
    /// </summary>
    public sealed class MesFieldItem : BindableBase
    {
        private string _value;

        /// <summary>
        /// 记下属性名、标题、当前文字，以及是否用勾选框。
        /// </summary>
        public MesFieldItem(string name, string label, string value, bool isCheck)
        {
            Name = name;
            Label = label;
            _value = value;
            IsCheck = isCheck;
        }

        /// <summary>
        /// 属性名。保存时按它写回。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 界面标题。
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// 为 true 时界面用勾选框。
        /// </summary>
        public bool IsCheck { get; }

        /// <summary>
        /// 当前文字。
        /// </summary>
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        /// <summary>
        /// 勾选状态。对应文字 true 或 false，保存时仍走 Value。
        /// </summary>
        public bool Checked
        {
            get => string.Equals(_value, "true", StringComparison.OrdinalIgnoreCase);
            set
            {
                var text = value ? "true" : "false";
                if (string.Equals(_value, text, StringComparison.Ordinal))
                    return;

                Value = text;
                RaisePropertyChanged();
            }
        }
    }
}