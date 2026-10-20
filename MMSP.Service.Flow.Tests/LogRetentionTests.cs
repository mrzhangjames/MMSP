using MMSP.Core.Model.Sys;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 日志保留天数的判断。不碰数据库和磁盘。
    /// </summary>
    public class LogRetentionTests
    {
        /// <summary>
        /// 用来比较的“今天”。
        /// </summary>
        private static readonly DateTime Today = new(2026, 10, 20);

        /// <summary>
        /// 0、上限和普通整数可以保存。
        /// </summary>
        [Fact]
        public void TryParseDays_AcceptsZeroAndUpperBound()
        {
            Assert.True(LogRetention.TryParseDays("0", out var zero));
            Assert.Equal(0, zero);
            Assert.True(LogRetention.TryParseDays("90", out var days));
            Assert.Equal(90, days);
            Assert.True(LogRetention.TryParseDays("3650", out var max));
            Assert.Equal(3650, max);
        }

        /// <summary>
        /// 空、负数、小数和超过上限的天数不接受。
        /// </summary>
        [Fact]
        public void TryParseDays_RejectsBlankNegativeAndTooLarge()
        {
            Assert.False(LogRetention.TryParseDays("  ", out _));
            Assert.False(LogRetention.TryParseDays("-1", out _));
            Assert.False(LogRetention.TryParseDays("1.5", out _));
            Assert.False(LogRetention.TryParseDays("3651", out _));
            Assert.False(LogRetention.TryParseDays("30天", out _));
        }

        /// <summary>
        /// 0 天不产生截止日期。30 天的截止日是今天往前 30 天。
        /// </summary>
        [Fact]
        public void CutoffDate_ZeroKeepsForever()
        {
            Assert.Null(LogRetention.CutoffDate(Today, 0));
            Assert.Equal(new DateTime(2026, 9, 20), LogRetention.CutoffDate(Today, 30));
        }

        /// <summary>
        /// 早于截止日的按日文件要删。截止日当天和认不出的文件名留下。
        /// </summary>
        [Fact]
        public void IsExpiredFile_UsesDayName()
        {
            Assert.True(LogRetention.IsExpiredFile(@"D:\Data\Log\20260919.log", Today, 30));
            Assert.False(LogRetention.IsExpiredFile(@"D:\Data\Log\20260920.log", Today, 30));
            Assert.False(LogRetention.IsExpiredFile(@"D:\Data\Log\20261020.log", Today, 30));
            Assert.False(LogRetention.IsExpiredFile(@"D:\Data\Log\NLog.config", Today, 30));
            Assert.False(LogRetention.IsExpiredFile(@"D:\Data\Log\20260919.log", Today, 0));
        }
    }
}
