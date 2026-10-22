using MMSP.Core.Model.Tool;
using MMSP.Core.Model.Sys;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// PLC 监控工具的编辑模型。
    /// </summary>
    public sealed class PlcMonitorToolViewModel : ToolEditViewModel<PlcMonitorToolEntity>
    {
        /// <summary>
        /// 用一份空配置创建。
        /// </summary>
        public PlcMonitorToolViewModel()
            : this(new PlcMonitorToolEntity(), null)
        {
        }

        /// <summary>
        /// 编辑一份已有的 PLC 监控配置。points 是点位列表，数据源默认流程。
        /// </summary>
        public PlcMonitorToolViewModel(PlcMonitorToolEntity tool, IReadOnlyList<PlcPointChoice>? points = null)
            : base(tool)
        {
            Points = new PlcPointField(points, tool.Address, tool.DataType, tool.Length, ApplyPoint);
        }

        /// <summary>
        /// 数据源和点位。选中点位后写回地址、数据类型和长度。
        /// </summary>
        public PlcPointField Points { get; }

        /// <summary>
        /// 数据类型下拉项。
        /// </summary>
        public IReadOnlyList<PlcDataType> DataTypes { get; } = Enum.GetValues<PlcDataType>();

        /// <summary>
        /// PLC 地址。
        /// </summary>
        public string Address
        {
            get => Tool.Address;
            set => SetText(Tool.Address, value, text => Tool.Address = text);
        }

        /// <summary>
        /// 数据类型。
        /// </summary>
        public PlcDataType DataType
        {
            get => Tool.DataType;
            set => SetValue(Tool.DataType, value, item => Tool.DataType = item);
        }

        /// <summary>
        /// 读取长度。
        /// </summary>
        public int Length
        {
            get => Tool.Length;
            set => SetNumber(Tool.Length, value, number => Tool.Length = number);
        }

        /// <summary>
        /// 把点位上的地址、数据类型和长度写回。长度不是正数时保留原来的长度。
        /// </summary>
        private void ApplyPoint(PlcPointChoice point)
        {
            Address = point.Address.Trim();
            DataType = point.DataType;
            if (point.Length > 0)
                Length = point.Length;
        }

        /// <summary>
        /// 期望值。
        /// </summary>
        public string ExpectValue
        {
            get => Tool.ExpectValue;
            set => SetText(Tool.ExpectValue, value, text => Tool.ExpectValue = text);
        }

        /// <summary>
        /// 读到期望值后是否写回重置值。
        /// </summary>
        public bool Reset
        {
            get => Tool.Reset;
            set => SetValue(Tool.Reset, value, item => Tool.Reset = item);
        }

        /// <summary>
        /// 间隔，毫秒。
        /// </summary>
        public int Interval
        {
            get => Tool.Interval;
            set => SetNumber(Tool.Interval, value, number => Tool.Interval = number);
        }

        /// <summary>
        /// 重置时写入的值。
        /// </summary>
        public string ResetValue
        {
            get => Tool.ResetValue;
            set => SetText(Tool.ResetValue, value, text => Tool.ResetValue = text);
        }
    }
}
