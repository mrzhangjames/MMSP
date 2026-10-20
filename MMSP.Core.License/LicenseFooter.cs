namespace MMSP.Core.License
{
    /// <summary>
    /// 底栏许可证状态。到期前七天改成提醒，过期单独显示。
    /// </summary>
    public static class LicenseFooter
    {
        /// <summary>
        /// 到期提醒提前量。剩余时间小于或等于这个值时提醒。
        /// </summary>
        public static readonly TimeSpan Reminder = TimeSpan.FromDays(7);

        /// <summary>
        /// 按当前时间判断底栏该显示哪一种状态。
        /// </summary>
        public static LicenseFooterState StateOf(LicenseEvaluation evaluation, DateTimeOffset utcNow)
        {
            ArgumentNullException.ThrowIfNull(evaluation);
            if (evaluation.Code == LicenseFailureCode.Expired)
                return LicenseFooterState.Expired;

            if (!evaluation.IsValid || evaluation.NotAfterUtc is not { } notAfter)
                return LicenseFooterState.Unauthorized;

            if (LicenseDates.IsPerpetual(notAfter))
                return LicenseFooterState.Authorized;

            if (notAfter - utcNow <= Reminder)
                return LicenseFooterState.Expiring;

            return LicenseFooterState.Authorized;
        }
    }

    /// <summary>
    /// 底栏上的四种许可证状态。
    /// </summary>
    public enum LicenseFooterState
    {
        /// <summary>
        /// 没有可用许可证。
        /// </summary>
        Unauthorized = 0,

        /// <summary>
        /// 已授权，且距到期超过七天。
        /// </summary>
        Authorized = 1,

        /// <summary>
        /// 七天内到期。
        /// </summary>
        Expiring = 2,

        /// <summary>
        /// 已经过了到期时间。
        /// </summary>
        Expired = 3
    }
}
