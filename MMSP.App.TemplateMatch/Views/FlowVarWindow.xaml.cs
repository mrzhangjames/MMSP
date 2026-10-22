using System.Collections.ObjectModel;
using System.Windows;
using MahApps.Metro.Controls;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;

namespace MMSP.App.TemplateMatch.Views
{
    /// <summary>
    /// 一条流程的变量列表。增删改在表里完成，保存才写库。
    /// </summary>
    public partial class FlowVarWindow : MetroWindow
    {
        /// <summary>
        /// 打开时步骤已经绑定的名称。
        /// </summary>
        private readonly IReadOnlySet<string> _bound;

        /// <summary>
        /// 打开一份变量名单。bound 是本流程步骤已经用到的名称。
        /// </summary>
        public FlowVarWindow(string flowName, IReadOnlyList<FlowVarEntity> rows, IReadOnlySet<string> bound)
        {
            ArgumentNullException.ThrowIfNull(rows);
            ArgumentNullException.ThrowIfNull(bound);
            InitializeComponent();
            Title = (flowName ?? "").Trim() + " · 变量";
            _bound = bound;
            Rows = new ObservableCollection<FlowVarRow>(rows
                .OrderBy(item => item.SortIndex)
                .Select(item => new FlowVarRow(item.Name, item.DataType, item.InitialValue)));
            DataContext = this;
        }

        /// <summary>
        /// 表中的行。
        /// </summary>
        public ObservableCollection<FlowVarRow> Rows { get; }

        /// <summary>
        /// 当前选中的行。
        /// </summary>
        public FlowVarRow? Selected { get; set; }

        /// <summary>
        /// 数据类型下拉。
        /// </summary>
        public IReadOnlyList<string> Types { get; } = FlowVarType.All;

        /// <summary>
        /// 确定后的名单。取消时为空。
        /// </summary>
        public IReadOnlyList<FlowVarDraft>? Result { get; private set; }

        /// <summary>
        /// 在末尾加一行，数据类型为 string，并进入编辑。
        /// </summary>
        private void AddRow(object sender, RoutedEventArgs e)
        {
            StopEditing();
            var row = new FlowVarRow("", FlowVarType.String, "") { IsEditing = true };
            Rows.Add(row);
            Selected = row;
            Grid.SelectedItem = row;
            Grid.ScrollIntoView(row);
        }

        /// <summary>
        /// 让选中行进入编辑。没有选中时不动作。
        /// </summary>
        private void EditRow(object sender, RoutedEventArgs e)
        {
            if (Grid.SelectedItem is not FlowVarRow row)
                return;

            StopEditing();
            row.IsEditing = true;
        }

        /// <summary>
        /// 从名单里去掉选中行。没有选中时不动作。是否能删要到保存时再看绑定。
        /// </summary>
        private void DeleteRow(object sender, RoutedEventArgs e)
        {
            if (Grid.SelectedItem is not FlowVarRow row)
                return;

            Rows.Remove(row);
        }

        /// <summary>
        /// 校验后关闭。不通过时留在窗口里。
        /// </summary>
        private void Accept(object sender, RoutedEventArgs e)
        {
            var drafts = Rows.Select(item => item.Draft()).ToList();
            var error = FlowVarRules.Check(drafts, _bound);
            if (error.Length > 0)
            {
                ErrorText.Text = error;
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            Result = drafts;
            DialogResult = true;
        }

        /// <summary>
        /// 结束其它行的编辑。同一时间只编辑一行。
        /// </summary>
        private void StopEditing()
        {
            foreach (var row in Rows)
                row.IsEditing = false;
        }
    }

    /// <summary>
    /// 变量窗口里的一行。
    /// </summary>
    public sealed class FlowVarRow : Prism.Mvvm.BindableBase
    {
        /// <summary>
        /// 打开时的名称。新行是空。
        /// </summary>
        private readonly string _originalName;

        /// <summary>
        /// 当前名称。
        /// </summary>
        private string _name;

        /// <summary>
        /// 数据类型。
        /// </summary>
        private string _dataType;

        /// <summary>
        /// 初始文本。
        /// </summary>
        private string _initialValue;

        /// <summary>
        /// 是否正在这一行里改。
        /// </summary>
        private bool _isEditing;

        /// <summary>
        /// 用库里的一行创建。名称会去掉首尾空白后当作原名。
        /// </summary>
        public FlowVarRow(string name, string dataType, string initialValue)
        {
            _originalName = (name ?? "").Trim();
            _name = _originalName;
            _dataType = FlowVarType.IsDefined(dataType) ? dataType : FlowVarType.String;
            _initialValue = initialValue ?? "";
        }

        /// <summary>
        /// 名称。
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value ?? "");
        }

        /// <summary>
        /// 数据类型。
        /// </summary>
        public string DataType
        {
            get => _dataType;
            set => SetProperty(ref _dataType, value ?? "");
        }

        /// <summary>
        /// 初始文本。
        /// </summary>
        public string InitialValue
        {
            get => _initialValue;
            set => SetProperty(ref _initialValue, value ?? "");
        }

        /// <summary>
        /// 是否正在编辑这一行。
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        /// <summary>
        /// 收成校验用的草稿。
        /// </summary>
        public FlowVarDraft Draft()
        {
            return new FlowVarDraft
            {
                OriginalName = _originalName,
                Name = Name,
                DataType = DataType,
                InitialValue = InitialValue ?? ""
            };
        }
    }
}
