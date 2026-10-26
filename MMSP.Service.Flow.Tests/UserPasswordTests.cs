using MMSP.Core.Model.Sys;
using MMSP.Service.Database;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 口令哈希和会话超时。
    /// </summary>
    public class UserPasswordTests
    {
        /// <summary>
        /// 测试用迭代。不低于拒绝线，又比默认的 600000 快。
        /// </summary>
        private const int Iterations = UserPassword.MinimumIterations;

        /// <summary>
        /// 哈希能通过校验，明文库存不能。
        /// </summary>
        [Fact]
        public void Verify_AcceptsHashAndRejectsPlaintext()
        {
            var stored = UserPassword.Hash("123456", Iterations);

            Assert.StartsWith("pbkdf2$" + Iterations + "$", stored);
            Assert.True(UserPassword.IsHashed(stored));
            Assert.True(UserPassword.Verify(stored, "123456"));
            Assert.False(UserPassword.Verify(stored, "123457"));
            Assert.False(UserPassword.Verify("123456", "123456"));
            Assert.False(UserPassword.IsHashed("123456"));
        }

        /// <summary>
        /// 同一口令两次入库盐不同。损坏的迭代和 Base64 都当失败。
        /// </summary>
        [Fact]
        public void Verify_RejectsAnotherSaltAndADamagedRow()
        {
            var first = UserPassword.Hash("123456", Iterations);
            var second = UserPassword.Hash("123456", Iterations);
            Assert.NotEqual(first, second);
            Assert.True(UserPassword.Verify(second, "123456"));

            var parts = first.Split('$');
            Assert.False(UserPassword.Verify("pbkdf2$99999$" + parts[2] + "$" + parts[3], "123456"));
            Assert.False(UserPassword.Verify("pbkdf2$" + Iterations + "$****$" + parts[3], "123456"));
        }

        /// <summary>
        /// 低于拒绝线的次数不能写入。
        /// </summary>
        [Fact]
        public void Hash_RejectsIterationsBelowMinimum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => UserPassword.Hash("123456", Iterations - 1));
        }

        /// <summary>
        /// 0 不注销。分钟到点才注销。
        /// </summary>
        [Fact]
        public void Timeout_DueWhenMinutesElapse()
        {
            var login = new DateTime(2026, 10, 26, 8, 0, 0);

            Assert.False(UserSessionTimeout.Due(0, login, login.AddHours(1)));
            Assert.False(UserSessionTimeout.Due(5, login, login.AddMinutes(5).AddTicks(-1)));
            Assert.True(UserSessionTimeout.Due(5, login, login.AddMinutes(5)));
        }

        /// <summary>
        /// 弹窗只接受 0 和正整数分钟。
        /// </summary>
        [Fact]
        public void Timeout_ParseAcceptsWholeMinutes()
        {
            Assert.True(UserSessionTimeout.TryParse("0", out var zero));
            Assert.Equal(0, zero);
            Assert.True(UserSessionTimeout.TryParse(" 15 ", out var minutes));
            Assert.Equal(15, minutes);
            Assert.False(UserSessionTimeout.TryParse("", out _));
            Assert.False(UserSessionTimeout.TryParse("-1", out _));
            Assert.False(UserSessionTimeout.TryParse("1.5", out _));
            Assert.False(UserSessionTimeout.TryParse("abc", out _));
        }
    }
}
