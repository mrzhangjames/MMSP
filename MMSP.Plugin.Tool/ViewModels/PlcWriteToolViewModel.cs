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
            : this(new PlcWriteToolEntity())
        {
        }

        /// <summary>
        /// 编辑一份已有的PLC 写入配置。
        /// </summary>
        public PlcWriteToolViewModel(PlcWriteToolEntity tool)
            : base(tool)
        {
        }

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
        /// 写入值。
        /// </summary>
        public string Value
        {
            get => Tool.Value;
            set => SetText(Tool.Value, value, text => Tool.Value = text);
        }
    }
}
