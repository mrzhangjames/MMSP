namespace MMSP.Core.License
{
    /// <summary>
    /// 从 JSON 读出的许可证。签名尚未判断。
    /// </summary>
    internal sealed class LicensePayload
    {
        /// <summary>
        /// 文件格式。第一版只有 1。
        /// </summary>
        public int Format { get; init; }

        /// <summary>
        /// 许可证编号。
        /// </summary>
        public Guid LicenseId { get; init; }

        /// <summary>
        /// 客户编码。
        /// </summary>
        public string CustomerCode { get; init; } = "";

        /// <summary>
        /// 通用证或机器指纹。
        /// </summary>
        public string MachineFingerprint { get; init; } = "";

        /// <summary>
        /// 生效时间。
        /// </summary>
        public DateTimeOffset NotBeforeUtc { get; init; }

        /// <summary>
        /// 到期时间。
        /// </summary>
        public DateTimeOffset NotAfterUtc { get; init; }

        /// <summary>
        /// 签发时间。
        /// </summary>
        public DateTimeOffset IssuedAtUtc { get; init; }

        /// <summary>
        /// 授权模块，顺序与文件一致。
        /// </summary>
        public IReadOnlyList<string> Modules { get; init; } = [];

        /// <summary>
        /// 规范字节的 RSA 签名。
        /// </summary>
        public byte[] Signature { get; init; } = [];
    }
}
