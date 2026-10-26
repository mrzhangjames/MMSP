using System.Collections.ObjectModel;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.ViewModels;

namespace MMSP.App.TemplateMatch.Views
{
    /// <summary>
    /// 通用工具窗左右两侧。左侧入参，右侧出参。中间表单仍由各工具自己画。
    /// </summary>
    public sealed class ToolIoModel : BindableBase
    {
        /// <summary>
        /// 正在编辑的工具。
        /// </summary>
        private readonly PluginToolModel _tool;

        /// <summary>
        /// 工具种类。
        /// </summary>
        private readonly FlowToolKind _kind;

        /// <summary>
        /// 前序步骤已经当作写出参的名称。
        /// </summary>
        private readonly HashSet<string> _prior;

        /// <summary>
        /// 写回单出参。Json、PLC 读取和单扫码要走编辑模型，好让中间的结果框一起变。
        /// </summary>
        private readonly Action<string> _setOutput;

        /// <summary>
        /// 多扫码出参槽变化后刷新中间的结果表。
        /// </summary>
        private readonly Action _slotsChanged;

        /// <summary>
        /// 写回 PLC 写入的变量名，让中间的写入值框一起变。
        /// </summary>
        private readonly Action<string> _setInput;

        /// <summary>
        /// 建立左右两列。variables 是本流程的变量名，按保存顺序。
        /// </summary>
        public ToolIoModel(
            FlowToolKind kind,
            PluginToolModel tool,
            IReadOnlyList<string> variables,
            IReadOnlySet<string> priorOutputs,
            Action<string> setOutput,
            Action<string> setInput,
            Action slotsChanged)
        {
            ArgumentNullException.ThrowIfNull(tool);
            ArgumentNullException.ThrowIfNull(variables);
            ArgumentNullException.ThrowIfNull(priorOutputs);
            ArgumentNullException.ThrowIfNull(setOutput);
            ArgumentNullException.ThrowIfNull(setInput);
            ArgumentNullException.ThrowIfNull(slotsChanged);
            _kind = kind;
            _tool = tool;
            _prior = new HashSet<string>(priorOutputs, StringComparer.Ordinal);
            _setOutput = setOutput;
            _setInput = setInput;
            _slotsChanged = slotsChanged;
            VariableNames = variables;
            Outputs = [];
            Inputs = [];
            SelectCommand = new DelegateCommand<ToolOutputRow>(Select);
            SelectInputCommand = new DelegateCommand<ToolInputRow>(SelectInput);
            Refresh();
        }

        /// <summary>
        /// 点选一个出参。
        /// </summary>
        public DelegateCommand<ToolOutputRow> SelectCommand { get; }

        /// <summary>
        /// 点选一个入参。
        /// </summary>
        public DelegateCommand<ToolInputRow> SelectInputCommand { get; }

        /// <summary>
        /// 右侧变量行。
        /// </summary>
        public ObservableCollection<ToolOutputRow> Outputs { get; }

        /// <summary>
        /// 左侧变量行。这种工具不读变量时为空。
        /// </summary>
        public ObservableCollection<ToolInputRow> Inputs { get; }

        /// <summary>
        /// 本流程的变量名。
        /// </summary>
        public IReadOnlyList<string> VariableNames { get; }

        /// <summary>
        /// 左侧没有入参时的说明。
        /// </summary>
        public string InputNote
        {
            get => _inputNote;
            private set => SetProperty(ref _inputNote, value);
        }

        /// <summary>
        /// 右侧补充说明。
        /// </summary>
        public string OutputNote
        {
            get => _outputNote;
            private set => SetProperty(ref _outputNote, value);
        }

        /// <summary>
        /// 右侧标题下的说明。PLC 写入用来提示点选后显示在写入值。
        /// </summary>
        public string OutputHint
        {
            get => _outputHint;
            private set => SetProperty(ref _outputHint, value);
        }

        /// <summary>
        /// 左侧说明。
        /// </summary>
        private string _inputNote = "";

        /// <summary>
        /// 右侧说明。
        /// </summary>
        private string _outputNote = "";

        /// <summary>
        /// 右侧标题下的说明。
        /// </summary>
        private string _outputHint = "点选本步写入的变量。前序已选用的不能再选";

        /// <summary>
        /// 条码数量、扫码枪或结果变了之后，按配置重画两侧。
        /// </summary>
        public void Refresh()
        {
            if (_tool is MultiScanToolEntity multi)
                ToolVars.FitSlots(multi, ToolVars.OutputSlotCount(multi));

            DropPriorOutputs();
            RebuildOutputs();
            RebuildInputs();
            if (_tool is MultiScanToolEntity)
                _slotsChanged();
        }

        /// <summary>
        /// 点一行。前序已选用的不能再选。再点本步已选的行则取消。
        /// PLC 写入没有写出参，点右侧是把变量名填进写入值。
        /// </summary>
        private void Select(ToolOutputRow row)
        {
            if (row is null)
                return;

            if (_kind == FlowToolKind.PlcWrite)
            {
                ApplyWriteVariable(row.Selected ? "" : row.Name);
                return;
            }

            if (row.UsedByPrior)
                return;

            if (ToolVars.HasManyOutputs(_kind) && _tool is MultiScanToolEntity multi)
            {
                var slots = ToolVars.Slots(multi);
                var index = IndexOf(slots, row.Name);
                if (index >= 0)
                    slots[index] = "";
                else
                {
                    var empty = IndexOf(slots, "");
                    if (empty < 0)
                        return;

                    slots[empty] = row.Name;
                }

                Refresh();
                return;
            }

            if (!ToolVars.HasOutput(_kind))
                return;

            var current = ToolVars.Output(_tool).Trim();
            _setOutput(current == row.Name ? "" : row.Name);
            Refresh();
        }

        /// <summary>
        /// 点一个入参。再点已选的则取消。前序出参也可以当入参读。
        /// </summary>
        private void SelectInput(ToolInputRow row)
        {
            if (row is null || !ToolVars.HasInputs(_kind))
                return;

            if (_kind == FlowToolKind.PlcWrite)
            {
                SelectWriteValue(row);
                return;
            }

            if (_kind == FlowToolKind.JsonParse)
            {
                ApplyJsonSource(row.Selected ? "" : row.Name);
                return;
            }

            if (_kind == FlowToolKind.Uph)
            {
                ApplyUph(row.Selected ? "" : row.Name);
                return;
            }

            var inputs = ToolVars.Inputs(_tool);
            if (inputs is null)
                return;

            var removed = false;
            for (var index = inputs.Count - 1; index >= 0; index--)
            {
                if (!string.Equals((inputs[index]?.Variable ?? "").Trim(), row.Name, StringComparison.Ordinal))
                    continue;

                inputs.RemoveAt(index);
                removed = true;
            }

            if (!removed)
                inputs.Add(new ToolInput { Variable = row.Name });

            RebuildInputs();
        }

        /// <summary>
        /// PLC 写入在变量模式下只留一个入参，并把名称填进写入值。
        /// </summary>
        private void SelectWriteValue(ToolInputRow row)
        {
            ApplyWriteVariable(row.Selected ? "" : row.Name);
        }

        /// <summary>
        /// JSON 解析只留一个来源，并把名称填进来源框。
        /// </summary>
        private void ApplyJsonSource(string name)
        {
            if (_tool is not JsonParseToolEntity json)
                return;

            _setInput(name);
            var inputs = ToolVars.Inputs(json);
            if (inputs is null)
                return;

            inputs.Clear();
            if (name.Length > 0)
                inputs.Add(new ToolInput { Variable = name });

            Refresh();
        }

        /// <summary>
        /// UPH 只留一个判定变量，并把名称填进判定框。
        /// </summary>
        private void ApplyUph(string name)
        {
            if (_tool is not UphToolEntity uph)
                return;

            _setInput(name);
            uph.Inputs ??= [];
            uph.Inputs.Clear();
            if (name.Length > 0)
                uph.Inputs.Add(new ToolInput { Variable = name });

            Refresh();
        }

        /// <summary>
        /// 把选中的变量名写进写入值。还在固定值时先改成变量，名称才会出现在框里。
        /// </summary>
        private void ApplyWriteVariable(string name)
        {
            if (_tool is not PlcWriteToolEntity write)
                return;

            _setInput(name);
            var inputs = ToolVars.Inputs(write);
            if (inputs is null)
                return;

            inputs.Clear();
            if (name.Length > 0)
                inputs.Add(new ToolInput { Variable = name });

            Refresh();
        }

        /// <summary>
        /// 按流程变量重画左侧。没勾选的旧槽位名仍留在配置里，只要变量名对得上就算已选。
        /// </summary>
        private void RebuildInputs()
        {
            Inputs.Clear();
            if (!ToolVars.HasInputs(_kind))
            {
                InputNote = "本步不引用变量。";
                return;
            }

            if (VariableNames.Count == 0)
            {
                InputNote = "本流程还没有变量。";
                return;
            }

            if (_tool is PlcWriteToolEntity write)
                InputNote = write.UseVariable ? "点选要写入的变量。" : "当前写入固定值。";
            else if (_tool is JsonParseToolEntity)
                InputNote = "点选来源变量。";
            else if (_tool is UphToolEntity)
                InputNote = "点选判定变量。";
            else
                InputNote = "点选本步读取的变量。";
            var selected = SelectedInputs();
            foreach (var name in VariableNames)
            {
                var on = selected.Contains(name);
                var status = "未选用";
                if (on)
                    status = _prior.Contains(name) ? "前序已出参" : "未出参";

                Inputs.Add(new ToolInputRow(name, on, status));
            }
        }

        /// <summary>
        /// 配置里已经点选的入参名。空白略过。旧槽位名不看。
        /// </summary>
        private HashSet<string> SelectedInputs()
        {
            var selected = new HashSet<string>(StringComparer.Ordinal);
            if (_tool is PlcWriteToolEntity write)
            {
                if (!write.UseVariable)
                    return selected;

                var chosen = (write.Value ?? "").Trim();
                if (chosen.Length > 0)
                    selected.Add(chosen);

                return selected;
            }

            if (_tool is JsonParseToolEntity json)
            {
                var source = (json.Source ?? "").Trim();
                if (source.Length > 0)
                    selected.Add(source);

                return selected;
            }

            if (_tool is UphToolEntity uph)
            {
                var variable = (uph.Variable ?? "").Trim();
                if (variable.Length > 0)
                    selected.Add(variable);

                return selected;
            }

            var inputs = ToolVars.Inputs(_tool);
            if (inputs is null)
                return selected;

            foreach (var input in inputs)
            {
                var name = (input?.Variable ?? "").Trim();
                if (name.Length > 0)
                    selected.Add(name);
            }

            return selected;
        }

        /// <summary>
        /// 本步如果选了前序已占用的名称，打开时清掉。取消窗口不会写回。
        /// </summary>
        private void DropPriorOutputs()
        {
            if (_tool is MultiScanToolEntity multi)
            {
                var slots = ToolVars.Slots(multi);
                for (var index = 0; index < slots.Count; index++)
                {
                    if (_prior.Contains((slots[index] ?? "").Trim()))
                        slots[index] = "";
                }

                return;
            }

            var current = ToolVars.Output(_tool).Trim();
            if (current.Length > 0 && _prior.Contains(current))
                _setOutput("");
        }

        /// <summary>
        /// 按流程变量重画右侧状态。
        /// </summary>
        private void RebuildOutputs()
        {
            var chosen = Chosen();
            Outputs.Clear();
            OutputHint = _kind == FlowToolKind.PlcWrite
                ? "点选一个变量，名称显示在写入值。"
                : "点选本步写入的变量。前序已选用的不能再选";
            foreach (var name in VariableNames)
            {
                chosen.TryGetValue(name, out var slot);
                var prior = _kind != FlowToolKind.PlcWrite && _prior.Contains(name);
                Outputs.Add(new ToolOutputRow(name, prior, Status(prior, slot), slot > 0));
            }

            OutputNote = Note(chosen);
        }

        /// <summary>
        /// 本步选中的名称到条码序号。单出参的序号用 1。
        /// </summary>
        private Dictionary<string, int> Chosen()
        {
            var chosen = new Dictionary<string, int>(StringComparer.Ordinal);
            if (_tool is MultiScanToolEntity multi)
            {
                var slots = ToolVars.Slots(multi);
                for (var index = 0; index < slots.Count; index++)
                {
                    var name = (slots[index] ?? "").Trim();
                    if (name.Length > 0)
                        chosen[name] = index + 1;
                }

                return chosen;
            }

            if (_tool is PlcWriteToolEntity write && write.UseVariable)
            {
                var picked = (write.Value ?? "").Trim();
                if (picked.Length > 0)
                    chosen[picked] = 1;

                return chosen;
            }

            var output = ToolVars.Output(_tool).Trim();
            if (output.Length > 0)
                chosen[output] = 1;

            return chosen;
        }

        /// <summary>
        /// 右侧状态文字。
        /// </summary>
        private string Status(bool prior, int slot)
        {
            if (prior)
                return "前序已选用";

            if (slot <= 0)
                return "未选用";

            if (ToolVars.HasManyOutputs(_kind))
                return "本步·条码" + slot.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return "本步选用";
        }

        /// <summary>
        /// 右侧一行说明。
        /// </summary>
        private string Note(Dictionary<string, int> chosen)
        {
            if (ToolVars.HasManyOutputs(_kind) && _tool is MultiScanToolEntity multi)
            {
                var slots = ToolVars.Slots(multi);
                for (var index = 0; index < slots.Count; index++)
                {
                    if (string.IsNullOrWhiteSpace(slots[index]))
                        return "条码 " + (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture) + " 尚未指定。";
                }

                return "";
            }

            if (_kind == FlowToolKind.PlcWrite)
                return "";

            if (!ToolVars.HasOutput(_kind) && !ToolVars.HasManyOutputs(_kind))
                return "本步不写出参。";

            return chosen.Count == 0 ? "" : "";
        }

        /// <summary>
        /// 名称在列表中的位置。找不到时返回 -1。空名称按空白项找。
        /// </summary>
        private static int IndexOf(IReadOnlyList<string> slots, string name)
        {
            for (var index = 0; index < slots.Count; index++)
            {
                if (string.Equals((slots[index] ?? "").Trim(), name, StringComparison.Ordinal))
                    return index;
            }

            return -1;
        }
    }

    /// <summary>
    /// 右侧的一个流程变量。
    /// </summary>
    public sealed class ToolOutputRow : BindableBase
    {
        /// <summary>
        /// 创建一行。
        /// </summary>
        public ToolOutputRow(string name, bool usedByPrior, string status, bool selected)
        {
            Name = name;
            UsedByPrior = usedByPrior;
            _status = status;
            _selected = selected;
        }

        /// <summary>
        /// 变量名。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 前序步骤已经把它当作出参。
        /// </summary>
        public bool UsedByPrior { get; }

        /// <summary>
        /// 状态文字。
        /// </summary>
        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// 是否为本步选中。
        /// </summary>
        public bool Selected
        {
            get => _selected;
            private set => SetProperty(ref _selected, value);
        }

        /// <summary>
        /// 状态文字。
        /// </summary>
        private string _status;

        /// <summary>
        /// 本步是否选中。
        /// </summary>
        private bool _selected;
    }

    /// <summary>
    /// 左侧的一个流程变量。
    /// </summary>
    public sealed class ToolInputRow : BindableBase
    {
        /// <summary>
        /// 创建一行。
        /// </summary>
        public ToolInputRow(string name, bool selected, string status)
        {
            Name = name;
            _selected = selected;
            _status = status;
        }

        /// <summary>
        /// 变量名。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 状态文字。未选用、未出参或前序已出参。
        /// </summary>
        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// 是否为本步读取。
        /// </summary>
        public bool Selected
        {
            get => _selected;
            private set => SetProperty(ref _selected, value);
        }

        /// <summary>
        /// 状态文字。
        /// </summary>
        private string _status;

        /// <summary>
        /// 本步是否读取。
        /// </summary>
        private bool _selected;
    }
}
