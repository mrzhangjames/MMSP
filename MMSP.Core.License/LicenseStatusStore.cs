namespace MMSP.Core.License
{
    /// <summary>
    /// 当前进程已经接受的许可证。配置页只读这一份。
    /// </summary>
    public sealed class LicenseStatusStore
    {
        /// <summary>
        /// 最近一次结果。还没校验时按许可证文件缺失。
        /// </summary>
        public LicenseEvaluation Current { get; private set; } = LicenseEvaluation.Only(LicenseFailureCode.FileMissing);

        /// <summary>
        /// 只有已经过期才限制功能。没有许可证、校验失败和尚未生效都不在这里。
        /// </summary>
        public bool IsExpired => Current.Code == LicenseFailureCode.Expired;

        /// <summary>
        /// 结果被替换时通知。底栏用它立刻改字。
        /// </summary>
        public event EventHandler? Changed;

        /// <summary>
        /// 换成新的校验结果。
        /// </summary>
        public void Replace(LicenseEvaluation evaluation)
        {
            ArgumentNullException.ThrowIfNull(evaluation);
            Current = evaluation;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
