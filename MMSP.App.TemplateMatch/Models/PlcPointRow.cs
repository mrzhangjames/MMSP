using MMSP.Core.Model.Sys;

namespace MMSP.App.TemplateMatch.Models
{
    /// <summary>
    /// PLC 点位配置的一行。只在设备页内存里。
    /// </summary>
    public sealed class PlcPointRow : BindableBase
    {
        private bool _isChecked;
        private string _name = "";
        private string _address = "";
        private string _dataType = "";
        private string _lengthText = "";
        private string _accessText = "";
        private bool _isEnabled = true;
        private string _enabledText = "";
        private string _currentValue = "";
        private string _valueDetail = "";
        private string _enabledOn = "";
        private string _enabledOff = "";

        /// <summary>
        /// 行标识。和 biz_plc_points 主键相同。当前值和勾选不入库。
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// 是否勾选。勾选行用金色底，删除用这些行。
        /// </summary>
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        /// <summary>
        /// 点位名称。
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 地址。
        /// </summary>
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        /// <summary>
        /// 数据类型。
        /// </summary>
        public string DataType
        {
            get => _dataType;
            set => SetProperty(ref _dataType, value);
        }

        /// <summary>
        /// 长度文字。
        /// </summary>
        public string LengthText
        {
            get => _lengthText;
            set => SetProperty(ref _lengthText, value);
        }

        /// <summary>
        /// 读写文字。
        /// </summary>
        public string AccessText
        {
            get => _accessText;
            set => SetProperty(ref _accessText, value);
        }

        /// <summary>
        /// 点位类型。新行默认流程。心跳和状态各只保留一条。界面暂不编辑。
        /// </summary>
        public PlcPointKind PointKind { get; set; } = PlcPointKind.Flow;

        /// <summary>
        /// Bit 位。空表示整字。界面暂不编辑。
        /// </summary>
        public int? BitIndex { get; set; }

        /// <summary>
        /// 点位描述。界面暂不编辑。
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (!SetProperty(ref _isEnabled, value))
                    return;

                EnabledText = value ? _enabledOn : _enabledOff;
            }
        }

        /// <summary>
        /// 启用或停用。
        /// </summary>
        public string EnabledText
        {
            get => _enabledText;
            private set => SetProperty(ref _enabledText, value);
        }

        /// <summary>
        /// 最近一次读取或写入的值。
        /// </summary>
        public string CurrentValue
        {
            get => _currentValue;
            set => SetProperty(ref _currentValue, value);
        }

        /// <summary>
        /// 当前值下面的说明。
        /// </summary>
        public string ValueDetail
        {
            get => _valueDetail;
            set => SetProperty(ref _valueDetail, value);
        }

        /// <summary>
        /// 记下启用、停用的显示文字，再设置启用状态。
        /// </summary>
        public void UseEnabledLabels(string enabledOn, string enabledOff, bool enabled)
        {
            _enabledOn = enabledOn;
            _enabledOff = enabledOff;
            IsEnabled = enabled;
            EnabledText = enabled ? enabledOn : enabledOff;
        }
    }
}
