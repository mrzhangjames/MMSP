namespace MMSP.Core.License
{
    /// <summary>
    /// 失败码对应的语言资源键。界面用资源显示，枚举名另放一行。
    /// </summary>
    public static class LicensePhrases
    {
        /// <summary>
        /// 返回这个失败码的资源键。
        /// </summary>
        public static string Key(LicenseFailureCode code) => code switch
        {
            LicenseFailureCode.None => "Config_StatusValid",
            LicenseFailureCode.FileMissing => "License_FileMissing",
            LicenseFailureCode.FileUnreadable => "License_FileUnreadable",
            LicenseFailureCode.FormatInvalid => "License_FormatInvalid",
            LicenseFailureCode.SignatureInvalid => "License_SignatureInvalid",
            LicenseFailureCode.MachineMismatch => "License_MachineMismatch",
            LicenseFailureCode.NotYetValid => "License_NotYetValid",
            LicenseFailureCode.Expired => "License_Expired",
            LicenseFailureCode.ClockRollback => "License_ClockRollback",
            LicenseFailureCode.MachineUnreadable => "License_MachineUnreadable",
            LicenseFailureCode.ToleranceInvalid => "License_ToleranceInvalid",
            LicenseFailureCode.PublicKeyMissing => "License_PublicKeyMissing",
            LicenseFailureCode.PublicKeyInvalid => "License_PublicKeyInvalid",
            _ => "License_FormatInvalid"
        };
    }
}
