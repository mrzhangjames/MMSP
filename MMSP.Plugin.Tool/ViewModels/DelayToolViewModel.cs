using MMSP.Core.Model.Tool;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// 延时工具的编辑模型。
    /// </summary>
    public sealed class DelayToolViewModel : ToolEditViewModel<DelayToolEntity>
    {
        /// <summary>
        /// 用一份空配置创建。
        /// </summary>
        public DelayToolViewModel()
            : this(new DelayToolEntity())
        {
        }

        /// <summary>
        /// 编辑一份已有的延时配置。
        /// </summary>
        public DelayToolViewModel(DelayToolEntity tool)
            : base(tool)
        {
        }

        /// <summary>
        /// 等待的毫秒数。
        /// </summary>
        public int Milliseconds
        {
            get => Tool.Milliseconds;
            set => SetNumber(Tool.Milliseconds, value, number => Tool.Milliseconds = number);
        }
    }
}
