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
