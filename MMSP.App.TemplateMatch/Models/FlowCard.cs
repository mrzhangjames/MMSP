using System.Collections.ObjectModel;

namespace MMSP.App.TemplateMatch.Models
{
    /// <summary>
    /// 一张流程卡片。内容来自客户库 biz_flows。
    /// </summary>
    public sealed class FlowCard : BindableBase
    {
        private bool _isEnabled;
        private bool _logOutput;
        private bool _isSelected;

        /// <summary>
        /// 主键。
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// 流程名称。
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// 左侧卡片从上到下的顺序。
        /// </summary>
        public int SortIndex { get; init; }

        /// <summary>
        /// 步骤。顺序即执行顺序。
        /// </summary>
        public ObservableCollection<FlowStep> Steps { get; init; } = [];

        /// <summary>
        /// 流程步数量。
        /// </summary>
        public int StepCount => Steps.Count;

        /// <summary>
        /// 步骤增删后刷新左侧数量。
        /// </summary>
        public void NotifyStepCount()
        {
            RaisePropertyChanged(nameof(StepCount));
        }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        /// <summary>
        /// 是否输出日志。
        /// </summary>
        public bool LogOutput
        {
            get => _logOutput;
            set => SetProperty(ref _logOutput, value);
        }

        /// <summary>
        /// 是否为当前选中的流程。
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
