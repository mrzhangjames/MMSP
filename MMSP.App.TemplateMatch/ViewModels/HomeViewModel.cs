using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using MMSP.App.WpfBase.Core;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.Sys;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 主页。只显示本次程序启动之后的运行日志和异常日志。
    /// </summary>
    public class HomeViewModel : BaseViewModel
    {
        /// <summary>
        /// 不超过这个条数时逐条插入。再多就整表换一次，避免界面连续重排。
        /// </summary>
        private const int IncrementalBatchLimit = 24;

        /// <summary>
        /// 已经订上本次启动后的日志。
        /// </summary>
        private bool _started;

        /// <summary>
        /// 已经排了一次界面刷新。同一轮里后来的日志只进队列。
        /// </summary>
        private bool _flushPosted;

        /// <summary>
        /// 已经上屏的日志主键。用来挡住快照和实时消息的重复。
        /// </summary>
        private readonly HashSet<Guid> _seen = new();

        /// <summary>
        /// 还没画到界面上的日志，旧的在前。
        /// </summary>
        private readonly Queue<RunLogMessage> _pending = new();

        /// <summary>
        /// 运行日志。新的在前。
        /// </summary>
        private ObservableCollection<RunLogMessage> _runLogs = new();

        /// <summary>
        /// 异常日志。只含报警和异常，新的在前。
        /// </summary>
        private ObservableCollection<RunLogMessage> _alarmLogs = new();

        /// <summary>
        /// 创建主页。
        /// </summary>
        public HomeViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator)
            : base(region, container, eventAggregator)
        {
        }

        /// <summary>
        /// 运行日志。新的在前。
        /// </summary>
        public ObservableCollection<RunLogMessage> RunLogs
        {
            get => _runLogs;
            private set => SetProperty(ref _runLogs, value);
        }

        /// <summary>
        /// 异常日志。只含报警和异常，新的在前。
        /// </summary>
        public ObservableCollection<RunLogMessage> AlarmLogs
        {
            get => _alarmLogs;
            private set => SetProperty(ref _alarmLogs, value);
        }

        /// <summary>
        /// 第一次进入时订阅新日志，并装入本次启动后已经写出的记录。
        /// </summary>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (_started)
                return;

            _started = true;
            EventAggregatorBase.GetEvent<RunLogEvent>()
                .Subscribe(OnLogArrived, ThreadOption.UIThread, true);
            ApplyBatch(LogHelper.LoadSession());
        }

        /// <summary>
        /// 收到一条新日志后先入队，同一轮只刷新一次界面。
        /// </summary>
        private void OnLogArrived(RunLogMessage message)
        {
            _pending.Enqueue(message);
            if (_flushPosted)
                return;

            _flushPosted = true;
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null || dispatcher.HasShutdownStarted)
            {
                Flush();
                return;
            }

            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(Flush));
        }

        /// <summary>
        /// 把队列里的日志一次性画到两个列表上。
        /// </summary>
        private void Flush()
        {
            _flushPosted = false;
            if (_pending.Count == 0)
                return;

            var batch = new List<RunLogMessage>(_pending.Count);
            while (_pending.Count > 0)
                batch.Add(_pending.Dequeue());
            ApplyBatch(batch);
        }

        /// <summary>
        /// 少量日志逐条插入。数量大时只保留最新的一段并整表替换。
        /// </summary>
        private void ApplyBatch(IReadOnlyList<RunLogMessage> batchOldestFirst)
        {
            if (batchOldestFirst.Count == 0)
                return;

            if (batchOldestFirst.Count > IncrementalBatchLimit)
            {
                Rebuild(batchOldestFirst);
                return;
            }

            foreach (var message in batchOldestFirst)
                AddOne(message);
        }

        /// <summary>
        /// 插入一条。报警和异常同时进入异常日志。同一条不放两次。
        /// </summary>
        private void AddOne(RunLogMessage message)
        {
            if (!_seen.Add(message.Id))
                return;

            RunLogs.Insert(0, message);
            Trim(RunLogs, AlarmLogs);
            if (message.Level is not (RunLogLevel.Alarm or RunLogLevel.Error))
                return;

            AlarmLogs.Insert(0, message);
            Trim(AlarmLogs, RunLogs);
        }

        /// <summary>
        /// 超过界面上限时丢掉最旧的一行。另一边还显示着这条时，主键继续留着。
        /// </summary>
        private void Trim(ObservableCollection<RunLogMessage> logs, ObservableCollection<RunLogMessage> other)
        {
            while (logs.Count > LogHelper.SessionRowLimit)
            {
                var old = logs[logs.Count - 1];
                logs.RemoveAt(logs.Count - 1);
                if (!other.Contains(old))
                    _seen.Remove(old.Id);
            }
        }

        /// <summary>
        /// 用最新的一段日志换掉两个列表，只通知界面一次。
        /// </summary>
        private void Rebuild(IReadOnlyList<RunLogMessage> batchOldestFirst)
        {
            var run = TakeNewest(batchOldestFirst, RunLogs);
            var alarmsInBatch = new List<RunLogMessage>();
            foreach (var message in batchOldestFirst)
            {
                if (message.Level is RunLogLevel.Alarm or RunLogLevel.Error)
                    alarmsInBatch.Add(message);
            }

            var alarm = TakeNewest(alarmsInBatch, AlarmLogs);
            _seen.Clear();
            foreach (var item in run)
                _seen.Add(item.Id);
            foreach (var item in alarm)
                _seen.Add(item.Id);
            RunLogs = new ObservableCollection<RunLogMessage>(run);
            AlarmLogs = new ObservableCollection<RunLogMessage>(alarm);
        }

        /// <summary>
        /// 新的在前，最多保留界面上限那么多条。
        /// </summary>
        private static List<RunLogMessage> TakeNewest(
            IReadOnlyList<RunLogMessage> batchOldestFirst,
            IEnumerable<RunLogMessage> existingNewestFirst)
        {
            var result = new List<RunLogMessage>(LogHelper.SessionRowLimit);
            var seen = new HashSet<Guid>();
            for (var i = batchOldestFirst.Count - 1; i >= 0 && result.Count < LogHelper.SessionRowLimit; i--)
            {
                var message = batchOldestFirst[i];
                if (seen.Add(message.Id))
                    result.Add(message);
            }

            if (result.Count >= LogHelper.SessionRowLimit)
                return result;

            foreach (var message in existingNewestFirst)
            {
                if (result.Count >= LogHelper.SessionRowLimit)
                    break;
                if (seen.Add(message.Id))
                    result.Add(message);
            }

            return result;
        }
    }
}
