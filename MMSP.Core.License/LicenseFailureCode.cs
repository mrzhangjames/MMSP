namespace MMSP.Core.License
{
    /// <summary>
    /// 许可证校验结果。前一步失败就返回，后面的步骤不再做。
    /// </summary>
    public enum LicenseFailureCode
    {
        /// <summary>
        /// 有效。
        /// </summary>
        None = 0,

        /// <summary>
        /// 许可证文件不存在。由读取文件的一方返回，校验器不返回这个码。
        /// </summary>
        FileMissing = 1,

        /// <summary>
        /// 许可证文件读不出来。
        /// </summary>
        FileUnreadable = 2,

        /// <summary>
        /// JSON 或字段不符合约定。
        /// </summary>
        FormatInvalid = 3,

        /// <summary>
        /// 签名对不上。
        /// </summary>
        SignatureInvalid = 4,

        /// <summary>
        /// 机器证和本机指纹不一致。
        /// </summary>
        MachineMismatch = 5,

        /// <summary>
        /// 还没到生效时间。
        /// </summary>
        NotYetValid = 6,

        /// <summary>
        /// 已经过了到期时间。
        /// </summary>
        Expired = 7,

        /// <summary>
        /// 系统时钟早于上次成功水位减去容差。
        /// </summary>
        ClockRollback = 8,

        /// <summary>
        /// 机器证需要本机指纹，但调用方没有给出 64 位小写十六进制。
        /// </summary>
        MachineUnreadable = 9,

        /// <summary>
        /// 时钟容差小于 0。
        /// </summary>
        ToleranceInvalid = 10,

        /// <summary>
        /// 主程序目录里没有公钥文件。校验器不返回这个码。
        /// </summary>
        PublicKeyMissing = 11,

        /// <summary>
        /// 公钥文件不是可用的 RSA 公钥。校验器不返回这个码。
        /// </summary>
        PublicKeyInvalid = 12
    }
}
