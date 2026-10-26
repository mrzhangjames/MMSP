namespace MMSP.Service.Flow
{
    /// <summary>
    /// 一步在界面上的执行状态。数值和流程页步骤卡片的颜色对应，不要改顺序。
    /// </summary>
    public enum FlowStepRunState
    {
        /// <summary>
        /// 还没有跑过。
        /// </summary>
        None = 0,

        /// <summary>
        /// 正在执行。
        /// </summary>
        Running = 1,

        /// <summary>
        /// 成功。
        /// </summary>
        Ok = 2,

        /// <summary>
        /// 失败。
        /// </summary>
        Failed = 3,

        /// <summary>
        /// 前面的步骤失败或被停止后，这一步没有执行。
        /// </summary>
        Skipped = 4,

        /// <summary>
        /// 停止信号打断了这一步。
        /// </summary>
        Cancelled = 5
    }

    /// <summary>
    /// 一步执行过程中的通知。界面用来刷新步骤卡片，日志另走文本。
    /// </summary>
    public sealed class FlowStepProgress
    {
        /// <summary>
        /// 记下这一步的身份、状态、耗时和结果文本。
        /// </summary>
        public FlowStepProgress(Guid stepId, int sortIndex, FlowStepRunState state, long elapsedMs, string detail)
        {
            StepId = stepId;
            SortIndex = sortIndex;
            State = state;
            ElapsedMs = elapsedMs < 0 ? 0 : elapsedMs;
            Detail = detail ?? "";
        }

        /// <summary>
        /// 流程步主键。界面按它找到卡片。空主键时改用顺序号。
        /// </summary>
        public Guid StepId { get; }

        /// <summary>
        /// 执行顺序。从 1 开始。
        /// </summary>
        public int SortIndex { get; }

        /// <summary>
        /// 当前状态。
        /// </summary>
        public FlowStepRunState State { get; }

        /// <summary>
        /// 这一步已经用掉的毫秒。还在执行或被跳过时为 0。
        /// </summary>
        public long ElapsedMs { get; }

        /// <summary>
        /// 结果或失败原因。还没有结果时为空。
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// 耗时文本。和日志里的写法相同。
        /// </summary>
        public static string FormatElapsed(long elapsedMs)
        {
            if (elapsedMs < 0)
                elapsedMs = 0;

            return elapsedMs.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ms";
        }
    }
}
