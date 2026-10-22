using MMSP.Core.Model.Tool;
using MMSP.Core.Model.Sys;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// PLC 写入工具的编辑模型。
    /// </summary>
    public sealed class PlcWriteToolViewModel : ToolEditViewModel<PlcWriteToolEntity>
    {
        /// <summary>
        /// 用一份空配置创建。
        /// </summary>
        public PlcWriteToolViewModel()
            : this(new PlcWriteToolEntity(), null)
        {
        }

        /// <summary>
        /// 编辑一份已有的 PLC 写入配置。points 是点位列表，数据源默认流程。
        /// </summary>
        public PlcWriteToolViewModel(PlcWriteToolEntity tool, IReadOnlyList<PlcPointChoice>? points = null)
            : base(tool)
        {
            Points = new PlcPointField(points, tool.Address, tool.DataType, 0, ApplyPoint);
        }

        /// <summary>
        /// 数据源和点位。选中点位后写回地址和数据类型。
        /// </summary>
        public PlcPointField Points { get; }

        /// <summary>
        /// 写入值用字面值。
        /// </summary>
        public const string FixedValueMode = "固定值";

        /// <summary>
        /// 写入值用流程变量。
        /// </summary>
        public const string VariableValueMode = "变量";

        /// <summary>
        /// 数据类型下拉项。
        /// </summary>
        public IReadOnlyList<PlcDataType> DataTypes { get; } = Enum.GetValues<PlcDataType>();

        /// <summary>
        /// 写入值的两种来源。
        /// </summary>
        public IReadOnlyList<string> ValueModes { get; } = [FixedValueMode, VariableValueMode];

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
        /// 把点位上的地址和数据类型写回。
        /// </summary>
        private void ApplyPoint(PlcPointChoice point)
        {
            Address = point.Address.Trim();
            DataType = point.DataType;
        }

        /// <summary>
        /// 写入值来源。改来源时清掉原来的字面值或变量名。
        /// </summary>
        public string ValueMode
        {
            get => Tool.UseVariable ? VariableValueMode : FixedValueMode;
            set
            {
                var variable = value == VariableValueMode;
                if (Tool.UseVariable == variable)
                    return;

                Tool.UseVariable = variable;
                Tool.Inputs ??= [];
                Tool.Inputs.Clear();
                if (!string.IsNullOrEmpty(Tool.Value))
                    Value = "";

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ValueFromVariable));
            }
        }

        /// <summary>
        /// 当前是变量写入。写入值框只读，显示点中的变量名。
        /// </summary>
        public bool ValueFromVariable => Tool.UseVariable;

        /// <summary>
        /// 写入值。固定值时可改。变量时由左侧入参或右侧出参写入变量名。
        /// </summary>
        public string Value
        {
            get => Tool.Value;
            set => SetText(Tool.Value, value, text => Tool.Value = text);
        }
    }
}
