using System.Security.Cryptography;

namespace MMSP.Core.License
{
    /// <summary>
    /// 用一把 RSA 公钥校验许可证。构造时公钥就必须可用。
    /// </summary>
    public sealed class LicenseValidator : ILicenseValidator
    {
        /// <summary>
        /// SubjectPublicKeyInfo 字节。
        /// </summary>
        private readonly byte[] _publicKey;

        /// <summary>
        /// 装入公钥。空字节或不是 RSA 公钥时抛出。
        /// </summary>
        public LicenseValidator(ReadOnlySpan<byte> subjectPublicKeyInfo)
        {
            if (subjectPublicKeyInfo.IsEmpty)
                throw new ArgumentException("公钥为空。", nameof(subjectPublicKeyInfo));

            _publicKey = subjectPublicKeyInfo.ToArray();
            using var rsa = RSA.Create();
            try
            {
                rsa.ImportSubjectPublicKeyInfo(_publicKey, out _);
            }
            catch (CryptographicException ex)
            {
                throw new ArgumentException("公钥无效。", nameof(subjectPublicKeyInfo), ex);
            }
        }

        /// <summary>
        /// 校验顺序：格式、签名、机器、容差、回拨、生效、到期。
        /// </summary>
        public LicenseEvaluation Evaluate(
            ReadOnlySpan<byte> utf8File,
            string machineFingerprint,
            DateTimeOffset utcNow,
            DateTimeOffset? lastSeenUtc,
            TimeSpan clockRollbackTolerance)
        {
            if (!LicenseJson.TryRead(utf8File, out var payload, out _) || payload is null)
                return LicenseEvaluation.Only(LicenseFailureCode.FormatInvalid);

            if (!Verify(payload))
                return LicenseEvaluation.Only(LicenseFailureCode.SignatureInvalid);

            if (!MachineFingerprintFormat.IsUniversal(payload.MachineFingerprint))
            {
                if (!MachineFingerprintFormat.IsMachineHex(machineFingerprint))
                    return LicenseEvaluation.FromPayload(LicenseFailureCode.MachineUnreadable, payload);

                if (!string.Equals(payload.MachineFingerprint, machineFingerprint, StringComparison.Ordinal))
                    return LicenseEvaluation.FromPayload(LicenseFailureCode.MachineMismatch, payload);
            }

            if (clockRollbackTolerance < TimeSpan.Zero)
                return LicenseEvaluation.FromPayload(LicenseFailureCode.ToleranceInvalid, payload);

            if (lastSeenUtc is { } seen && utcNow < seen - clockRollbackTolerance)
                return LicenseEvaluation.FromPayload(LicenseFailureCode.ClockRollback, payload);

            if (utcNow < payload.NotBeforeUtc)
                return LicenseEvaluation.FromPayload(LicenseFailureCode.NotYetValid, payload);

            if (utcNow > payload.NotAfterUtc)
                return LicenseEvaluation.FromPayload(LicenseFailureCode.Expired, payload);

            return LicenseEvaluation.FromPayload(LicenseFailureCode.None, payload);
        }

        /// <summary>
        /// 用公钥核对规范字节。签名坏了就返回 false，不把异常抛出去。
        /// </summary>
        private bool Verify(LicensePayload payload)
        {
            var canonical = LicenseCanonical.Build(
                payload.Format,
                payload.LicenseId,
                payload.CustomerCode,
                payload.MachineFingerprint,
                payload.NotBeforeUtc,
                payload.NotAfterUtc,
                payload.IssuedAtUtc,
                payload.Modules);

            try
            {
                using var rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(_publicKey, out _);
                return rsa.VerifyData(canonical, payload.Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch (CryptographicException)
            {
                return false;
            }
        }
    }
}
