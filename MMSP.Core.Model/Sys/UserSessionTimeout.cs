using System;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 会话超时。0 不注销。正数的单位是分钟，到点注销。与流程无关。
    /// </summary>
    public static class UserSessionTimeout
    {
        /// <summary>
        /// 是否到了该注销的时刻。只看登录时间和分钟数。
        /// </summary>
        public static bool Due(int minutes, DateTime loginTime, DateTime now)
        {
            if (minutes <= 0)
                return false;

            return now >= loginTime.AddMinutes(minutes);
        }
    }
}
