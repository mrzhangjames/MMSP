namespace MMSP.Core.License
{
    /// <summary>
    /// 校验一份许可证字节。不读文件，不读注册表。
    /// </summary>
    public interface ILicenseValidator
    {
        /// <summary>
        /// 按固定顺序校验。空字节是格式错误。机器证在调用方没给出指纹时返回 <see cref="LicenseFailureCode.MachineUnreadable"/>。
        /// </summary>
        LicenseEvaluation Evaluate(
            ReadOnlySpan<byte> utf8File,
            string machineFingerprint,
            DateTimeOffset utcNow,
            DateTimeOffset? lastSeenUtc,
            TimeSpan clockRollbackTolerance);
    }
}
