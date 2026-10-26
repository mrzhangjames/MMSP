using MMSP.Core.Model.Tool;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// UPH 工具的编辑模型。判定由左侧点选。类型只有 bool 和 int，新建默认 bool。
    /// </summary>
    public sealed class UphToolViewModel : ToolEditViewModel<UphToolEntity>
    {
        /// <summary>
        /// 用一份空配置创建。类型为 bool。
        /// </summary>
        public UphToolViewModel()
            : this(new UphToolEntity())
        {
        }

        /// <summary>
        /// 编辑一份已有的 UPH 配置。类型不是 bool 或 int 时改回 bool。入参里的名称补进判定框。
        /// </summary>
        public UphToolViewModel(UphToolEntity tool)
            : base(tool)
        {
            if (Tool.ValueType is not ("bool" or "int"))
                Tool.ValueType = "bool";

            if (string.IsNullOrWhiteSpace(Tool.Variable))
            {
                var only = OnlyInput(Tool.Inputs);
                if (only.Length > 0)
                    Tool.Variable = only;
            }

            SyncInput();
        }

        /// <summary>
        /// 类型下拉。顺序固定，默认第一项。
        /// </summary>
        public IReadOnlyList<string> ValueTypes { get; } = ["bool", "int"];

        /// <summary>
        /// 判定变量。只由左侧入参写入。
        /// </summary>
        public string Variable
        {
            get => Tool.Variable;
            set
            {
                SetText(Tool.Variable, value, text => Tool.Variable = text);
                SyncInput();
            }
        }

        /// <summary>
        /// 下拉类型。不是 int 时按 bool 保存。
        /// </summary>
        public string ValueType
        {
            get => Tool.ValueType;
            set => SetText(Tool.ValueType, value == "int" ? "int" : "bool", text => Tool.ValueType = text);
        }

        /// <summary>
        /// 入参只保留判定变量这一个名称。
        /// </summary>
        private void SyncInput()
        {
            Tool.Inputs ??= [];
            Tool.Inputs.Clear();
            var name = (Tool.Variable ?? "").Trim();
            if (name.Length > 0)
                Tool.Inputs.Add(new ToolInput { Variable = name });
        }

        /// <summary>
        /// 入参里只有一个名称时返回它。没有或不止一个时返回空。
        /// </summary>
        private static string OnlyInput(List<ToolInput>? inputs)
        {
            if (inputs is null)
                return "";

            var name = "";
            var count = 0;
            foreach (var input in inputs)
            {
                var text = (input?.Variable ?? "").Trim();
                if (text.Length == 0)
                    continue;

                count++;
                name = text;
                if (count > 1)
                    return "";
            }

            return count == 1 ? name : "";
        }
    }
}
