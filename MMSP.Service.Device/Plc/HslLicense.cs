namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// HSL 商业授权。整个进程调用一次即可。
    /// </summary>
    public static class HslLicense
    {
        /// <summary>
        /// 激活码。
        /// </summary>
        private const string Code = "db25afe0-dd1f-4a7b-a9cf-35cbd840cf08";

        private static bool _applied;
        private static bool _authorized;

        /// <summary>
        /// 最近一次授权是否成功。还没调用过时为 false。
        /// </summary>
        public static bool IsAuthorized => _applied && _authorized;

        /// <summary>
        /// 写入授权码。整个进程只向 HSL 提交一次，重复调用返回第一次的结果。
        /// </summary>
        public static bool Apply()
        {
            if (_applied)
                return _authorized;

            _authorized = HslCommunication.Authorization.SetAuthorizationCode(Code);
            _applied = true;
            return _authorized;
        }
    }
}
