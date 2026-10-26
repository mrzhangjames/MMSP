using System;
using System.Globalization;

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

        /// <summary>
        /// 把弹窗文字收成分钟。只接受 0 和正整数。空、小数和负数都不收。
        /// </summary>
        public static bool TryParse(string? text, out int minutes)
        {
            minutes = 0;
            var value = (text ?? "").Trim();
            if (value.Length == 0)
                return false;

            return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out minutes);
        }
    }
}
