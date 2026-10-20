using System.Security.Cryptography;

namespace MMSP.Core.License
{
    /// <summary>
    /// 用私钥签发许可证文件字节。不写磁盘。
    /// </summary>
    public static class LicenseIssuer
    {
        /// <summary>
        /// 签出一份许可证。字段不合法或私钥无效时抛出 <see cref="ArgumentException"/>。
        /// </summary>
        public static byte[] Sign(LicenseDraft draft, ReadOnlySpan<byte> pkcs8PrivateKey)
        {
            ArgumentNullException.ThrowIfNull(draft);
            Validate(draft);
            if (pkcs8PrivateKey.IsEmpty)
                throw new ArgumentException("私钥为空。", nameof(pkcs8PrivateKey));

            var canonical = LicenseCanonical.Build(
                1,
                draft.LicenseId,
                draft.CustomerCode,
                draft.MachineFingerprint,
                draft.NotBeforeUtc,
                draft.NotAfterUtc,
                draft.IssuedAtUtc,
                draft.Modules);

            byte[] signature;
            using (var rsa = RSA.Create())
            {
                try
                {
                    rsa.ImportPkcs8PrivateKey(pkcs8PrivateKey, out _);
                }
                catch (CryptographicException ex)
                {
                    throw new ArgumentException("私钥无效。", nameof(pkcs8PrivateKey), ex);
                }

                signature = rsa.SignData(canonical, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }

            return LicenseJson.Write(draft, signature);
        }

        /// <summary>
        /// 签发前检查字段。这里失败就不产生文件。
        /// </summary>
        private static void Validate(LicenseDraft draft)
        {
            if (!LicenseRules.IsCustomer(draft.CustomerCode))
                throw new ArgumentException("客户编码必须是 1 到 64 个可见 ASCII。", nameof(draft));

            if (!LicenseRules.IsFingerprint(draft.MachineFingerprint))
                throw new ArgumentException("绑定值只能是 universal 或 64 位小写十六进制。", nameof(draft));

            if (draft.NotBeforeUtc > draft.NotAfterUtc)
                throw new ArgumentException("生效时间晚于到期时间。", nameof(draft));

            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var module in draft.Modules)
            {
                if (!LicenseRules.IsModule(module) || !names.Add(module))
                    throw new ArgumentException("模块名无效或重复。", nameof(draft));
            }
        }
    }
}
