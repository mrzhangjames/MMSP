namespace MMSP.Core.License
{
    /// <summary>
    /// 许可证时间。永久不是单独字段，到期时间写成这个时刻。
    /// </summary>
    public static class LicenseDates
    {
        /// <summary>
        /// 勾选永久时写入的到期时间。
        /// </summary>
        public static readonly DateTimeOffset PerpetualNotAfter = new(9999, 12, 31, 23, 59, 59, TimeSpan.Zero);

        /// <summary>
        /// 这个时刻是不是永久到期时间。
        /// </summary>
        public static bool IsPerpetual(DateTimeOffset value) => value == PerpetualNotAfter;
    }
}
