using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.Sys;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 文件日志一行的线程、业务和级别。
    /// </summary>
    public class LogLineTests
    {
        /// <summary>
        /// 有线程名时，线程号和名字都写在时间后面。
        /// </summary>
        [Fact]
        public void FormatLine_IncludesThreadName()
        {
            var time = new DateTime(2026, 10, 28, 14, 5, 1);
            var line = LogHelper.FormatLine(time, 12, "PlcHeartbeat", RunLogBiz.Plc, "异常", "心跳写入失败");
            Assert.Equal("2026-10-28 14:05:01 [线程 12 PlcHeartbeat] [Plc] [异常] 心跳写入失败", line);
        }

        /// <summary>
        /// 线程名空白时只留线程号。
        /// </summary>
        [Fact]
        public void FormatLine_OmitsBlankThreadName()
        {
            var time = new DateTime(2026, 10, 28, 14, 5, 2);
            var line = LogHelper.FormatLine(time, 8, "  ", RunLogBiz.Sys, "信息", "程序启动");
            Assert.Equal("2026-10-28 14:05:02 [线程 8] [Sys] [信息] 程序启动", line);
        }
    }
}
