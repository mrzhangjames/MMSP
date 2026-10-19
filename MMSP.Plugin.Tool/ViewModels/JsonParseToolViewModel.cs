using MMSP.Core.Model.Tool;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// Json 解析工具的编辑模型。
    /// </summary>
    public sealed class JsonParseToolViewModel : ToolEditViewModel<JsonParseToolEntity>
    {
        /// <summary>
        /// 用一份空配置创建。
        /// </summary>
        public JsonParseToolViewModel()
            : this(new JsonParseToolEntity())
        {
        }

        /// <summary>
        /// 编辑一份已有的Json 解析配置。
        /// </summary>
        public JsonParseToolViewModel(JsonParseToolEntity tool)
            : base(tool)
        {
        }

        /// <summary>
        /// 待解析的文本或变量名。
        /// </summary>
        public string Source
        {
            get => Tool.Source;
            set => SetText(Tool.Source, value, text => Tool.Source = text);
        }

        /// <summary>
        /// Json 路径。
        /// </summary>
        public string JsonPath
        {
            get => Tool.JsonPath;
            set => SetText(Tool.JsonPath, value, text => Tool.JsonPath = text);
        }

        /// <summary>
        /// 解析结果写入的名称。
        /// </summary>
        public string Target
        {
            get => Tool.Target;
            set => SetText(Tool.Target, value, text => Tool.Target = text);
        }
    }
}
