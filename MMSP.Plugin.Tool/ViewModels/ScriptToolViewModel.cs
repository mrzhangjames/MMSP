using MMSP.Core.Model.Tool;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// 脚本工具的编辑模型。
    /// </summary>
    public sealed class ScriptToolViewModel : ToolEditViewModel<ScriptToolEntity>
    {
        /// <summary>
        /// 用一份空配置创建。
        /// </summary>
        public ScriptToolViewModel()
            : this(new ScriptToolEntity())
        {
        }

        /// <summary>
        /// 编辑一份已有的脚本配置。
        /// </summary>
        public ScriptToolViewModel(ScriptToolEntity tool)
            : base(tool)
        {
        }

        /// <summary>
        /// 脚本正文。
        /// </summary>
        public string Script
        {
            get => Tool.Script;
            set => SetText(Tool.Script, value, text => Tool.Script = text);
        }
    }
}
