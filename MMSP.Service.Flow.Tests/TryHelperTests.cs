using System.ComponentModel;
using System.Security;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using MMSP.Common.Helper;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.Sys;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// TryHelper 按异常类型写日志。不替换业务里的 try。
    /// </summary>
    public class TryHelperTests
    {
        /// <summary>
        /// 每种已分出的异常都有自己的短说明。未知类型用操作失败。
        /// </summary>
        [Fact]
        public void Describe_UsesTypeText()
        {
            Assert.Equal("文本编码无效", TryHelper.Describe(new DecoderFallbackException()));
            Assert.Equal("参数为空", TryHelper.Describe(new ArgumentNullException("name")));
            Assert.Equal("参数无效", TryHelper.Describe(new ArgumentException("bad")));
            Assert.Equal("没有访问权限", TryHelper.Describe(new UnauthorizedAccessException()));
            Assert.Equal("安全检查未通过", TryHelper.Describe(new SecurityException()));
            Assert.Equal("文件读写失败", TryHelper.Describe(new IOException("secret")));
            Assert.Equal("对象已释放", TryHelper.Describe(new ObjectDisposedException("gate")));
            Assert.Equal("当前状态不能执行", TryHelper.Describe(new InvalidOperationException()));
            Assert.Equal("JSON 无法解析", TryHelper.Describe(new JsonException("raw")));
            Assert.Equal("内容格式无效", TryHelper.Describe(new FormatException("raw")));
            Assert.Equal("校验失败", TryHelper.Describe(new CryptographicException()));
            Assert.Equal("系统调用失败", TryHelper.Describe(new Win32Exception(5)));
            Assert.Equal("操作超时", TryHelper.Describe(new TimeoutException()));
            Assert.Equal("不支持该操作", TryHelper.Describe(new NotSupportedException()));
            Assert.Equal("操作失败", TryHelper.Describe(new Exception("other")));
        }

        /// <summary>
        /// 成功不写日志。失败只写类型说明，不写异常原文。
        /// </summary>
        [Fact]
        public void Run_LogsTypeWithoutExceptionText()
        {
            var marker = "TryHelper" + Guid.NewGuid().ToString("N");
            Assert.True(TryHelper.Run(marker, () => { }));
            Assert.DoesNotContain(LogHelper.LoadSession(), item => item.Message.Contains(marker));

            Assert.False(TryHelper.Run(marker, () => throw new IOException("secret-path")));
            var row = LogHelper.LoadSession().Last(item => item.Message.Contains(marker));
            Assert.Equal(marker + "：文件读写失败", row.Message);
            Assert.Equal(RunLogBiz.Sys, row.BizType);
            Assert.Equal(RunLogLevel.Error, row.Level);
            Assert.DoesNotContain("secret-path", row.Message);
        }

        /// <summary>
        /// 操作取消返回 false，不进入异常日志。
        /// </summary>
        [Fact]
        public void Run_CancellationIsNotAnError()
        {
            var marker = "TryHelperCancel" + Guid.NewGuid().ToString("N");
            Assert.False(TryHelper.Run(marker, () => throw new OperationCanceledException("stop")));
            Assert.DoesNotContain(LogHelper.LoadSession(), item => item.Message.Contains(marker));
        }

        /// <summary>
        /// 没有分出的异常继续抛出，不写成“操作失败”。
        /// </summary>
        [Fact]
        public void Run_UnknownExceptionPropagates()
        {
            var thrown = Assert.Throws<Exception>(() => TryHelper.Run("未分类", () => throw new Exception("other")));
            Assert.Equal("other", thrown.Message);
        }

        /// <summary>
        /// 指定业务时写入对应类型。
        /// </summary>
        [Fact]
        public void Run_UsesRequestedBiz()
        {
            var marker = "TryHelperPlc" + Guid.NewGuid().ToString("N");
            Assert.False(TryHelper.Run(marker, () => throw new TimeoutException("late"), RunLogBiz.Plc));
            var row = LogHelper.LoadSession().Last(item => item.Message.Contains(marker));
            Assert.Equal(marker + "：操作超时", row.Message);
            Assert.Equal(RunLogBiz.Plc, row.BizType);
        }
    }
}
