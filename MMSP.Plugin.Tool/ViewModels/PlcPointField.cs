using System.Collections.ObjectModel;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;
using Prism.Mvvm;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// 数据源和点位下拉。数据源默认流程。选中点位后由调用方写回地址和数据类型。
    /// </summary>
    public sealed class PlcPointField : BindableBase
    {
        /// <summary>
        /// 全部点位。含打开时补上的旧地址。
        /// </summary>
        private readonly List<PlcPointChoice> _all;

        /// <summary>
        /// 选中点位后写回工具。
        /// </summary>
        private readonly Action<PlcPointChoice> _apply;

        /// <summary>
        /// 当前数据源。
        /// </summary>
        private PlcSourceOption _source;

        /// <summary>
        /// 当前点位。切换数据源滤掉它时只清空显示，不改已保存的地址。
        /// </summary>
        private PlcPointChoice? _choice;

        /// <summary>
        /// 用点位列表和已保存的地址创建。对得上的点位会选中，但不会立刻写回。
        /// </summary>
        public PlcPointField(
            IReadOnlyList<PlcPointChoice>? points,
            string? address,
            PlcDataType dataType,
            int length,
            Action<PlcPointChoice> apply)
        {
            ArgumentNullException.ThrowIfNull(apply);
            _apply = apply;
            _all = points is null ? [] : [..points];
            var matched = PlcPointChoices.Match(_all, address, dataType);
            if (matched is null && !string.IsNullOrWhiteSpace(address))
            {
                matched = new PlcPointChoice
                {
                    Address = address.Trim(),
                    DataType = dataType,
                    Length = length,
                    Kind = PlcPointKind.Flow
                };
                _all.Add(matched);
            }

            var kind = matched?.Kind ?? PlcPointKind.Flow;
            _source = Sources.First(item => item.Kind == kind);
            Fill();
            _choice = matched is not null && Choices.Contains(matched) ? matched : null;
        }

        /// <summary>
        /// 数据源下拉。顺序固定，默认用流程这一项。
        /// </summary>
        public IReadOnlyList<PlcSourceOption> Sources { get; } =
        [
            new(PlcPointKind.Flow, "流程"),
            new(PlcPointKind.Monitor, "监控"),
            new(PlcPointKind.Heartbeat, "心跳"),
            new(PlcPointKind.Status, "状态")
        ];

        /// <summary>
        /// 当前数据源下的点位。
        /// </summary>
        public ObservableCollection<PlcPointChoice> Choices { get; } = [];

        /// <summary>
        /// 当前数据源。换源只过滤点位。
        /// </summary>
        public PlcSourceOption Source
        {
            get => _source;
            set
            {
                if (value is null || value.Kind == _source.Kind)
                    return;

                _source = value;
                RaisePropertyChanged();
                Fill();
                if (_choice is not null && !Choices.Contains(_choice))
                {
                    _choice = null;
                    RaisePropertyChanged(nameof(Choice));
                }
            }
        }

        /// <summary>
        /// 当前点位。点选后写回地址和数据类型。空选不写回。
        /// </summary>
        public PlcPointChoice? Choice
        {
            get => _choice;
            set
            {
                if (value is null || ReferenceEquals(_choice, value))
                    return;

                _choice = value;
                RaisePropertyChanged();
                _apply(value);
            }
        }

        /// <summary>
        /// 按当前数据源重填下拉。
        /// </summary>
        private void Fill()
        {
            Choices.Clear();
            foreach (var point in PlcPointChoices.OfKind(_all, _source.Kind))
                Choices.Add(point);
        }
    }

    /// <summary>
    /// 数据源下拉的一项。文字是点位类型。
    /// </summary>
    public sealed record PlcSourceOption(PlcPointKind Kind, string Text);
}
