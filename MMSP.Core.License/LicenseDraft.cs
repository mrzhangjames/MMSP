namespace MMSP.Core.License
{
    /// <summary>
    /// 待签发的许可证。签名由 <see cref="LicenseIssuer"/> 填写。
    /// </summary>
    public sealed class LicenseDraft
    {
        /// <summary>
        /// 许可证编号。
        /// </summary>
        public Guid LicenseId { get; init; }

        /// <summary>
        /// 客户编码。1 到 64 个可见 ASCII。
        /// </summary>
        public string CustomerCode { get; init; } = "";

        /// <summary>
        /// <see cref="MachineFingerprintFormat.Universal"/> 或 64 位小写十六进制。
        /// </summary>
        public string MachineFingerprint { get; init; } = "";

        /// <summary>
        /// 生效时间。
        /// </summary>
        public DateTimeOffset NotBeforeUtc { get; init; }

        /// <summary>
        /// 到期时间。永久用 <see cref="LicenseDates.PerpetualNotAfter"/>。
        /// </summary>
        public DateTimeOffset NotAfterUtc { get; init; }

        /// <summary>
        /// 签发时间。
        /// </summary>
        public DateTimeOffset IssuedAtUtc { get; init; }

        /// <summary>
        /// 授权模块。可以是空名单，顺序会原样签进去。
        /// </summary>
        public IReadOnlyList<string> Modules { get; init; } = [];
    }
}
