using System.Globalization;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 把 UInt16 数组收成时间文本。6 元按年、月、日、时、分、秒。
    /// </summary>
    public static class PlcTimeText
    {
        /// <summary>
        /// 正好 6 个字且能组成时间时写出 yyyy-MM-dd HH:mm:ss。年份小于 100 时加 2000。
        /// </summary>
        public static bool TryFormat(IReadOnlyList<ushort> words, out string text)
        {
            text = "";
            if (words is null || words.Count != 6)
                return false;

            var year = words[0] < 100 ? words[0] + 2000 : words[0];
            try
            {
                var time = new DateTime(year, words[1], words[2], words[3], words[4], words[5]);
                text = time.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }
    }
}
