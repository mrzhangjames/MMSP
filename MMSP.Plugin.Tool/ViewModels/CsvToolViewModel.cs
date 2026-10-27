using System.Collections.ObjectModel;
using MMSP.Core.Model.Tool;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// CSV 工具的编辑模型。文件名默认按运行当天生成，数据来源从本流程变量里选，可以重复使用。
    /// </summary>
    public sealed class CsvToolViewModel : ToolEditViewModel<CsvToolEntity>
    {
        /// <summary>
        /// 当前流程名。用来预览文件名和存放目录。
        /// </summary>
        private string _flowName = "";

        /// <summary>
        /// 本流程的变量名。
        /// </summary>
        private IReadOnlyList<string> _variables = [];

        /// <summary>
        /// 批量改行时先不要把每一格都写回配置。
        /// </summary>
        private bool _mute;

        /// <summary>
        /// 用一份空配置创建。文件名等流程名到达后再填。
        /// </summary>
        public CsvToolViewModel()
            : this(new CsvToolEntity())
        {
        }

        /// <summary>
        /// 编辑一份已有配置。行直接可点，同一时间只编辑一行。
        /// </summary>
        public CsvToolViewModel(CsvToolEntity tool)
            : base(tool)
        {
            Tool.Rows ??= [];
            Rows = new ObservableCollection<CsvEditRow>();
            foreach (var row in Tool.Rows)
            {
                if (row is null)
                    continue;

                Rows.Add(new CsvEditRow(this, row.Column, row.Source));
            }

            Sync();
        }

        /// <summary>
        /// 导出列。
        /// </summary>
        public ObservableCollection<CsvEditRow> Rows { get; }

        /// <summary>
        /// 数据来源下拉。
        /// </summary>
        public IReadOnlyList<string> Variables => _variables;

        /// <summary>
        /// 当前选中的行。没有选中时为空。
        /// </summary>
        public CsvEditRow? Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        /// <summary>
        /// 当前选中的行。
        /// </summary>
        private CsvEditRow? _selected;

        /// <summary>
        /// 文件名。清空或改回当天默认时，运行仍按日期生成。
        /// </summary>
        public string FileName
        {
            get => Tool.FileName ?? "";
            set
            {
                var text = (value ?? "").Trim();
                var today = CsvPaths.DefaultFile(_flowName, DateTime.Now);
                if (text.Length == 0 || (today.Length > 0 && text == today))
                {
                    Tool.NameFromDate = true;
                    text = today;
                }
                else
                {
                    Tool.NameFromDate = false;
                }

                SetText(Tool.FileName, text, item => Tool.FileName = item);
            }
        }

        /// <summary>
        /// 存放目录。只显示，不能改。
        /// </summary>
        public string DirectoryText => CsvPaths.DisplayDirectory(_flowName);

        /// <summary>
        /// 填上流程名和变量。按日期生成的文件名刷新成今天。
        /// </summary>
        public void Bind(string? flowName, IReadOnlyList<string>? variables)
        {
            _flowName = (flowName ?? "").Trim();
            _variables = Distinct(variables);
            foreach (var row in Rows)
                row.RefreshChoices();

            RaisePropertyChanged(nameof(Variables));
            RaisePropertyChanged(nameof(DirectoryText));
            if (Tool.NameFromDate || string.IsNullOrWhiteSpace(Tool.FileName))
            {
                Tool.NameFromDate = true;
                Tool.FileName = CsvPaths.DefaultFile(_flowName, DateTime.Now);
                RaisePropertyChanged(nameof(FileName));
            }
        }

        /// <summary>
        /// 在末尾加一空行并进入编辑。
        /// </summary>
        public void AddRow()
        {
            _mute = true;
            StopEditing();
            var row = new CsvEditRow(this, "", "");
            row.IsEditing = true;
            Rows.Add(row);
            Selected = row;
            _mute = false;
            Sync();
        }

        /// <summary>
        /// 让选中行进入编辑。没有选中时不动作。
        /// </summary>
        public void EditSelected()
        {
            if (Selected is null)
                return;

            EditAt(Rows.IndexOf(Selected));
        }

        /// <summary>
        /// 让指定行进入编辑。左侧入参点的就是这一行。
        /// </summary>
        public void EditAt(int index)
        {
            if (index < 0 || index >= Rows.Count)
                return;

            if (Rows[index].IsEditing)
                return;

            _mute = true;
            StopEditing();
            Rows[index].IsEditing = true;
            Selected = Rows[index];
            _mute = false;
            Sync();
        }

        /// <summary>
        /// 去掉选中行。没有选中时不动作。
        /// </summary>
        public void DeleteSelected()
        {
            if (Selected is null)
                return;

            _mute = true;
            Rows.Remove(Selected);
            Selected = null;
            _mute = false;
            Sync();
        }

        /// <summary>
        /// 一格改了之后写回配置，好让左侧入参跟着变。
        /// </summary>
        internal void OnRowEdited()
        {
            if (_mute)
                return;

            Sync();
        }

        /// <summary>
        /// 结束其它行的编辑。
        /// </summary>
        private void StopEditing()
        {
            foreach (var row in Rows)
                row.IsEditing = false;
        }

        /// <summary>
        /// 把表写回工具。空白列名和空白来源仍保留，运行时再判失败。
        /// </summary>
        private void Sync()
        {
            var editing = -1;
            Tool.Rows = [];
            Tool.Inputs = [];
            for (var index = 0; index < Rows.Count; index++)
            {
                var row = Rows[index];
                var column = (row.Column ?? "").Trim();
                var source = (row.Source ?? "").Trim();
                Tool.Rows.Add(new CsvColumn { Column = column, Source = source });
                if (source.Length > 0)
                    Tool.Inputs.Add(new ToolInput { Variable = source });

                if (row.IsEditing)
                    editing = index;
            }

            Tool.EditingIndex = editing;
            RaisePropertyChanged("CsvIo");
        }

        /// <summary>
        /// 去掉空白和重复的变量名。顺序保留。
        /// </summary>
        private static List<string> Distinct(IReadOnlyList<string>? variables)
        {
            var list = new List<string>();
            if (variables is null)
                return list;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var name in variables)
            {
                var text = (name ?? "").Trim();
                if (text.Length == 0 || !seen.Add(text))
                    continue;

                list.Add(text);
            }

            return list;
        }
    }

    /// <summary>
    /// 数据列表里的一行。
    /// </summary>
    public sealed class CsvEditRow : Prism.Mvvm.BindableBase
    {
        /// <summary>
        /// 所属编辑模型。格子变化时通知它写回配置。
        /// </summary>
        private readonly CsvToolViewModel _owner;

        /// <summary>
        /// 列名。
        /// </summary>
        private string _column;

        /// <summary>
        /// 数据来源。
        /// </summary>
        private string _source;

        /// <summary>
        /// 是否正在编辑这一行。
        /// </summary>
        private bool _isEditing;

        /// <summary>
        /// 用已保存的列名和来源创建。
        /// </summary>
        public CsvEditRow(CsvToolViewModel owner, string? column, string? source)
        {
            _owner = owner;
            _column = column ?? "";
            _source = source ?? "";
        }

        /// <summary>
        /// 列名。
        /// </summary>
        public string Column
        {
            get => _column;
            set
            {
                var text = value ?? "";
                if (_column == text)
                    return;

                _column = text;
                RaisePropertyChanged();
                _owner.OnRowEdited();
            }
        }

        /// <summary>
        /// 数据来源。从本流程的变量里选，入参可以重复使用。
        /// </summary>
        public string Source
        {
            get => _source;
            set
            {
                var text = value ?? "";
                if (_source == text)
                    return;

                _source = text;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SourceChoices));
                _owner.OnRowEdited();
            }
        }

        /// <summary>
        /// 下拉项。当前来源不在变量表里时仍留在列表中，避免一打开就被清空。
        /// </summary>
        public IReadOnlyList<string> SourceChoices
        {
            get
            {
                var list = _owner.Variables;
                var source = (_source ?? "").Trim();
                if (source.Length == 0 || list.Contains(source, StringComparer.Ordinal))
                    return list;

                var copy = new List<string>(list) { source };
                return copy;
            }
        }

        /// <summary>
        /// 是否正在编辑这一行。
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (!SetProperty(ref _isEditing, value))
                    return;

                _owner.OnRowEdited();
            }
        }

        /// <summary>
        /// 变量名单变了之后重画下拉。
        /// </summary>
        public void RefreshChoices()
        {
            RaisePropertyChanged(nameof(SourceChoices));
        }
    }
}
