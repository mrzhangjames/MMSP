namespace MMSP.Core.License
{
    /// <summary>
    /// 一次校验的结果。验签没通过时，客户、时间和模块都空着。
    /// </summary>
    public sealed class LicenseEvaluation
    {
        /// <summary>
        /// 失败码。有效时是 <see cref="LicenseFailureCode.None"/>。
        /// </summary>
        public LicenseFailureCode Code { get; init; }

        /// <summary>
        /// 这次结果能不能放行。
        /// </summary>
        public bool IsValid => Code == LicenseFailureCode.None;

        /// <summary>
        /// 许可证编号。验签通过后才有。
        /// </summary>
        public Guid? LicenseId { get; init; }

        /// <summary>
        /// 客户编码。验签通过后才有。
        /// </summary>
        public string? CustomerCode { get; init; }

        /// <summary>
        /// 通用证为 true，机器证为 false。验签通过前是 null。
        /// </summary>
        public bool? IsUniversal { get; init; }

        /// <summary>
        /// 生效时间。验签通过后才有。
        /// </summary>
        public DateTimeOffset? NotBeforeUtc { get; init; }

        /// <summary>
        /// 到期时间。验签通过后才有。
        /// </summary>
        public DateTimeOffset? NotAfterUtc { get; init; }

        /// <summary>
        /// 签发时间。验签通过后才有。
        /// </summary>
        public DateTimeOffset? IssuedAtUtc { get; init; }

        /// <summary>
        /// 授权模块。验签通过前是空名单。
        /// </summary>
        public IReadOnlyList<string> Modules { get; init; } = [];

        /// <summary>
        /// 只有失败码、没有证书内容的结果。
        /// </summary>
        public static LicenseEvaluation Only(LicenseFailureCode code) => new()
        {
            Code = code
        };

        /// <summary>
        /// 日志用的一行。不包含指纹、签名和文件路径。
        /// </summary>
        public string ToLogText()
        {
            var bind = IsUniversal switch
            {
                true => "universal",
                false => "machine",
                _ => ""
            };
            var id = LicenseId?.ToString("D") ?? "";
            var customer = CustomerCode ?? "";
            return $"代码={Code} licenseId={id} customerCode={customer} 绑定={bind}";
        }

        /// <summary>
        /// 验签已经通过时，带上证书内容。
        /// </summary>
        internal static LicenseEvaluation FromPayload(LicenseFailureCode code, LicensePayload payload) => new()
        {
            Code = code,
            LicenseId = payload.LicenseId,
            CustomerCode = payload.CustomerCode,
            IsUniversal = MachineFingerprintFormat.IsUniversal(payload.MachineFingerprint),
            NotBeforeUtc = payload.NotBeforeUtc,
            NotAfterUtc = payload.NotAfterUtc,
            IssuedAtUtc = payload.IssuedAtUtc,
            Modules = payload.Modules
        };
    }
}
