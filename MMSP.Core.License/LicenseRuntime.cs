namespace MMSP.Core.License
{
    /// <summary>
    /// 把目录里的公钥、许可证和水位合成一次校验。
    /// </summary>
    public static class LicenseRuntime
    {
        /// <summary>
        /// 时钟回拨容差。这个范围内仍算有效，水位保持原来的较晚值。
        /// </summary>
        public static readonly TimeSpan ClockTolerance = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 校验目录里当前的许可证。没有公钥时先返回公钥失败，不再把许可证说成缺失。
        /// </summary>
        public static LicenseEvaluation Check(LicenseDirectoryStore files, string machineFingerprint, DateTimeOffset utcNow)
        {
            ArgumentNullException.ThrowIfNull(files);
            if (!files.TryReadPublicKey(out _, out var keyCode))
                return LicenseEvaluation.Only(keyCode);

            try
            {
                if (!files.TryReadLicense(out var utf8))
                    return LicenseEvaluation.Only(LicenseFailureCode.FileMissing);

                return CheckBytes(files, utf8, machineFingerprint, utcNow);
            }
            catch (UnauthorizedAccessException ex)
            {
                LicenseCaughtLog.Report("读取许可证失败", ex);
                return LicenseEvaluation.Only(LicenseFailureCode.FileUnreadable);
            }
            catch (IOException ex)
            {
                LicenseCaughtLog.Report("读取许可证失败", ex);
                return LicenseEvaluation.Only(LicenseFailureCode.FileUnreadable);
            }
        }

        /// <summary>
        /// 用给出的公钥校验许可证字节。不读磁盘上的旧公钥。
        /// </summary>
        public static LicenseEvaluation CheckPair(
            byte[] subjectPublicKeyInfo,
            byte[] licenseUtf8,
            string machineFingerprint,
            DateTimeOffset? lastSeenUtc,
            DateTimeOffset utcNow)
        {
            ArgumentNullException.ThrowIfNull(subjectPublicKeyInfo);
            ArgumentNullException.ThrowIfNull(licenseUtf8);
            LicenseValidator validator;
            try
            {
                validator = new LicenseValidator(subjectPublicKeyInfo);
            }
            catch (ArgumentException)
            {
                // 内部加密异常可能带上密钥材料，日志不附带原来的异常。
                LicenseCaughtLog.Report("公钥无效", new ArgumentException("公钥无效"));
                return LicenseEvaluation.Only(LicenseFailureCode.PublicKeyInvalid);
            }

            return validator.Evaluate(licenseUtf8, machineFingerprint ?? "", utcNow, lastSeenUtc, ClockTolerance);
        }

        /// <summary>
        /// 用目录里的公钥校验一份尚未落盘的许可证字节。
        /// </summary>
        public static LicenseEvaluation CheckBytes(
            LicenseDirectoryStore files,
            byte[] utf8,
            string machineFingerprint,
            DateTimeOffset utcNow)
        {
            ArgumentNullException.ThrowIfNull(files);
            ArgumentNullException.ThrowIfNull(utf8);
            if (!files.TryReadPublicKey(out var publicKey, out var keyCode))
                return LicenseEvaluation.Only(keyCode);

            LicenseValidator validator;
            try
            {
                validator = new LicenseValidator(publicKey);
            }
            catch (ArgumentException)
            {
                LicenseCaughtLog.Report("公钥无效", new ArgumentException("公钥无效"));
                return LicenseEvaluation.Only(LicenseFailureCode.PublicKeyInvalid);
            }

            DateTimeOffset? seen;
            try
            {
                seen = files.ReadSeen();
            }
            catch (UnauthorizedAccessException ex)
            {
                LicenseCaughtLog.Report("读取许可证水位失败", ex);
                seen = null;
            }
            catch (IOException ex)
            {
                LicenseCaughtLog.Report("读取许可证水位失败", ex);
                seen = null;
            }

            return validator.Evaluate(utf8, machineFingerprint ?? "", utcNow, seen, ClockTolerance);
        }

        /// <summary>
        /// 校验成功后回写水位。当前时间更早但还在容差内时，保持原来的较晚值。
        /// </summary>
        public static void Remember(LicenseDirectoryStore files, DateTimeOffset utcNow)
        {
            ArgumentNullException.ThrowIfNull(files);
            var seen = files.ReadSeen();
            var stored = seen is { } previous && previous > utcNow ? previous : utcNow.ToUniversalTime();
            files.WriteSeen(stored);
        }
    }
}
