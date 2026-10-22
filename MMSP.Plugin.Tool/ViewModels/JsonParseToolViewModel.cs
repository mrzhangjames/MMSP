using MMSP.Core.Model.Tool;
using Prism.Commands;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// Json 解析工具的编辑模型。来源和结果由左右两列点选。样例只用于试算，不保存。
    /// </summary>
    public sealed class JsonParseToolViewModel : ToolEditViewModel<JsonParseToolEntity>
    {
        /// <summary>
        /// 试算用的样例文本。不写入配置。
        /// </summary>
        private string _sample = "";

        /// <summary>
        /// 最近一次试算的文本。还没试算时为空。
        /// </summary>
        private string _preview = "";

        /// <summary>
        /// 最近一次试算是否失败。
        /// </summary>
        private bool _previewFailed;

        /// <summary>
        /// 用一份空配置创建。
        /// </summary>
        public JsonParseToolViewModel()
            : this(new JsonParseToolEntity())
        {
        }

        /// <summary>
        /// 编辑一份已有的 Json 解析配置。只有一个入参且来源为空时，用来源显示它。
        /// </summary>
        public JsonParseToolViewModel(JsonParseToolEntity tool)
            : base(tool)
        {
            if (string.IsNullOrWhiteSpace(Tool.Source))
            {
                var only = OnlyInput(Tool.Inputs);
                if (only.Length > 0)
                    Tool.Source = only;
            }

            TryCommand = new DelegateCommand(TrySample);
        }

        /// <summary>
        /// 用样例和当前路径试算。
        /// </summary>
        public DelegateCommand TryCommand { get; }

        /// <summary>
        /// 来源变量名。只由左侧入参写入。
        /// </summary>
        public string Source
        {
            get => Tool.Source;
            set => SetText(Tool.Source, value, text => Tool.Source = text);
        }

        /// <summary>
        /// Json 路径。改路径后清掉上次试算。
        /// </summary>
        public string JsonPath
        {
            get => Tool.JsonPath;
            set
            {
                var next = value ?? "";
                if (Tool.JsonPath == next)
                    return;

                Tool.JsonPath = next;
                RaisePropertyChanged();
                ClearPreview();
            }
        }

        /// <summary>
        /// 解析结果写入的名称。只由右侧出参写入。
        /// </summary>
        public string Target
        {
            get => Tool.Target;
            set => SetText(Tool.Target, value, text => Tool.Target = text);
        }

        /// <summary>
        /// 试算样例。不写入配置。改样例后清掉上次试算。
        /// </summary>
        public string Sample
        {
            get => _sample;
            set
            {
                var next = value ?? "";
                if (_sample == next)
                    return;

                _sample = next;
                RaisePropertyChanged();
                ClearPreview();
            }
        }

        /// <summary>
        /// 试算结果或失败原因。
        /// </summary>
        public string Preview
        {
            get => _preview;
            private set
            {
                if (_preview == value)
                    return;

                _preview = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 试算失败时结果用红色显示。
        /// </summary>
        public bool PreviewFailed
        {
            get => _previewFailed;
            private set
            {
                if (_previewFailed == value)
                    return;

                _previewFailed = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 按当前路径读取样例，把文本或原因放到试算结果。
        /// </summary>
        private void TrySample()
        {
            var read = JsonPathValue.Read(Sample, Tool.JsonPath);
            PreviewFailed = !read.Ok;
            Preview = read.Text;
        }

        /// <summary>
        /// 样例或路径变了，上次结果不再对应当前输入。
        /// </summary>
        private void ClearPreview()
        {
            PreviewFailed = false;
            Preview = "";
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

            return name;
        }
    }
}
