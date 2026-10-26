using MMSP.Service.Device.Scan;

namespace MMSP.App.TemplateMatch.Models
{
    /// <summary>
    /// 一张扫码枪卡片。Id 与 biz_scanners 主键相同。最近一次结果不入库。
    /// </summary>
    public sealed class ScannerCard : BindableBase
    {
        private string _name = "";
        private string _ip = "";
        private string _port = "";
        private string _triggerText = "";
        private ScanTransport _transport;
        private bool _isEnabled = true;
        private string _enabledText = "";
        private string _lastResult = "";
        private string _enabledOn = "";
        private string _enabledOff = "";

        /// <summary>
        /// 卡片标识。和 biz_scanners 主键相同。
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// 传输方式。
        /// </summary>
        public ScanTransport Transport
        {
            get => _transport;
            set
            {
                if (!SetProperty(ref _transport, value))
                    return;

                RaisePropertyChanged(nameof(IsTcp));
            }
        }

        /// <summary>
        /// 当前是 TCP。串口卡片不显示端口。
        /// </summary>
        public bool IsTcp => Transport == ScanTransport.Tcp;

        /// <summary>
        /// 串口波特率。串口弹窗可改。
        /// </summary>
        public int BaudRate { get; set; } = 9600;

        /// <summary>
        /// 串口校验位。
        /// </summary>
        public string Parity { get; set; } = "None";

        /// <summary>
        /// 串口数据位。
        /// </summary>
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// 串口停止位。
        /// </summary>
        public string StopBits { get; set; } = "One";

        /// <summary>
        /// 连接超时，毫秒。
        /// </summary>
        public int ConnectTimeout { get; set; } = 1000;

        /// <summary>
        /// 接收超时，毫秒。
        /// </summary>
        public int ReceiveTimeout { get; set; } = 1000;

        /// <summary>
        /// 结束符。
        /// </summary>
        public string NewLine { get; set; } = "\r";

        /// <summary>
        /// 编码名。
        /// </summary>
        public string EncodingName { get; set; } = "ASCII";

        /// <summary>
        /// 扫码枪名称。
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// IP 地址。
        /// </summary>
        public string Ip
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }

        /// <summary>
        /// 端口文字。
        /// </summary>
        public string Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        /// <summary>
        /// 触发字符串。
        /// </summary>
        public string TriggerText
        {
            get => _triggerText;
            set => SetProperty(ref _triggerText, value);
        }

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
        /// 最近一次扫码结果。停用枪保持空白。
        /// </summary>
        public string LastResult
        {
            get => _lastResult;
            set => SetProperty(ref _lastResult, value);
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
