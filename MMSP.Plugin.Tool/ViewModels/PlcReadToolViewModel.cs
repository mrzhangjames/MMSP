using MMSP.Core.Model.Tool;
using MMSP.Core.Model.Sys;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// PLC 读取工具的编辑模型。
    /// </summary>
    public sealed class PlcReadToolViewModel : ToolEditViewModel<PlcReadToolEntity>
    {
        /// <summary>
        /// 用一份空配置创建。
        /// </summary>
        public PlcReadToolViewModel()
            : this(new PlcReadToolEntity())
        {
        }

        /// <summary>
        /// 编辑一份已有的PLC 读取配置。
        /// </summary>
        public PlcReadToolViewModel(PlcReadToolEntity tool)
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
        /// 读取长度。
        /// </summary>
        public int Length
        {
            get => Tool.Length;
            set => SetNumber(Tool.Length, value, number => Tool.Length = number);
        }
    }
}
