using MMSP.Core.Model.Sys;

namespace MMSP.App.TemplateMatch.Models
{
    /// <summary>
    /// 一个流程步。内容来自客户库 biz_flow_steps，说明由工具配置拼出。
    /// </summary>
    public sealed class FlowStep : BindableBase
    {
        private int _number;
        private string _title = "";
        private string _detail = "";
        private bool _showStem;
        private bool _isSelected;
        private string _cfgValue = "";
        private int _runKind;
        private string _runStateText = "";
        private string _elapsedText = "";
        private string _runDetail = "";

        /// <summary>
        /// 主键。
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// 从 1 开始的序号。
        /// </summary>
        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        /// <summary>
        /// 工具种类。
        /// </summary>
        public FlowToolKind ToolKind { get; init; }

        /// <summary>
        /// 种类在步骤上的文字。
        /// </summary>
        public string Kind { get; init; } = "";

        /// <summary>
        /// 步骤名称。
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// 步骤说明。
        /// </summary>
        public string Detail
        {
            get => _detail;
            set => SetProperty(ref _detail, value);
        }

        /// <summary>
        /// 不是最后一步时画出向下的连线。
        /// </summary>
        public bool ShowStem
        {
            get => _showStem;
            set => SetProperty(ref _showStem, value);
        }

        /// <summary>
        /// 是否为当前选中的流程步。
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// 工具配置。Json 文本。
        /// </summary>
        public string CfgValue
        {
            get => _cfgValue;
            set => SetProperty(ref _cfgValue, value);
        }

        /// <summary>
        /// 执行状态。0 还没跑，其余和 <c>FlowStepRunState</c> 的数值一致。
        /// </summary>
        public int RunKind
        {
            get => _runKind;
            private set => SetProperty(ref _runKind, value);
        }

        /// <summary>
        /// 执行状态文字。还没跑时为空。
        /// </summary>
        public string RunStateText
        {
            get => _runStateText;
            private set => SetProperty(ref _runStateText, value);
        }

        /// <summary>
        /// 耗时。显示在步骤卡片右侧末尾。还在执行或被跳过时为空。
        /// </summary>
        public string ElapsedText
        {
            get => _elapsedText;
            private set => SetProperty(ref _elapsedText, value);
        }

        /// <summary>
        /// 这一步的结果或失败原因。给状态文字做提示。
        /// </summary>
        public string RunDetail
        {
            get => _runDetail;
            private set => SetProperty(ref _runDetail, value);
        }

        /// <summary>
        /// 写下这一轮的状态、耗时和结果。
        /// </summary>
        public void Mark(int kind, string stateText, string elapsedText, string detail)
        {
            RunKind = kind;
            RunStateText = stateText ?? "";
            ElapsedText = elapsedText ?? "";
            RunDetail = detail ?? "";
        }

        /// <summary>
        /// 清掉上一轮的状态和耗时。
        /// </summary>
        public void ClearRun()
        {
            Mark(0, "", "", "");
        }
    }
}
