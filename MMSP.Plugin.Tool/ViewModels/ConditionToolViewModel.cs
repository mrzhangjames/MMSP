using MMSP.Core.Model.Tool;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// 条件判断工具的编辑模型。
    /// </summary>
    public sealed class ConditionToolViewModel : ToolEditViewModel<ConditionToolEntity>
    {
        /// <summary>
        /// 用一份空配置创建。
        /// </summary>
        public ConditionToolViewModel()
            : this(new ConditionToolEntity())
        {
        }

        /// <summary>
        /// 编辑一份已有的条件判断配置。
        /// </summary>
        public ConditionToolViewModel(ConditionToolEntity tool)
            : base(tool)
        {
        }

        /// <summary>
        /// 条件表达式。
        /// </summary>
        public string Expression
        {
            get => Tool.Expression;
            set => SetText(Tool.Expression, value, text => Tool.Expression = text);
        }
    }
}
