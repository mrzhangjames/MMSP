namespace MMSP.Service.Flow
{
    /// <summary>
    /// 一步的执行结果。
    /// </summary>
    public enum FlowStepStatus
    {
        /// <summary>
        /// 成功。
        /// </summary>
        Ok,

        /// <summary>
        /// 失败。
        /// </summary>
        Failed,

        /// <summary>
        /// 停止信号打断了等待。这不是步骤错误。
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// 一步的结果。成功时文本是条码或读数，失败时是原因，取消时为空。
    /// </summary>
    public sealed class FlowStepResult
    {
        private FlowStepResult(FlowStepStatus status, string text)
        {
            Status = status;
            Text = text;
        }

        /// <summary>
        /// 结果状态。
        /// </summary>
        public FlowStepStatus Status { get; }

        /// <summary>
        /// 结果文本。
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 成功。
        /// </summary>
        public static FlowStepResult Ok(string text)
        {
            return new FlowStepResult(FlowStepStatus.Ok, text ?? "");
        }

        /// <summary>
        /// 失败。
        /// </summary>
        public static FlowStepResult Fail(string reason)
        {
            return new FlowStepResult(FlowStepStatus.Failed, reason ?? "");
        }

        /// <summary>
        /// 已取消。
        /// </summary>
        public static FlowStepResult Cancelled()
        {
            return new FlowStepResult(FlowStepStatus.Cancelled, "");
        }
    }
}
