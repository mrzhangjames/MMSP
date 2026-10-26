using System.Text.RegularExpressions;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 核对带耗时的步骤日志。毫秒数随运行变化，只核对位置和其余正文。
    /// </summary>
    internal static class FlowLogAssert
    {
        /// <summary>
        /// 实际行应在期望正文的标题和结果之间插入「n ms」。期望正文不含耗时。
        /// </summary>
        public static void Line(string expected, string actual)
        {
            var match = Regex.Match(actual, @"^(.*) (\d+) ms(?: (.*))?$");
            Assert.True(match.Success, actual);
            var rebuilt = match.Groups[1].Value;
            if (match.Groups[3].Success && match.Groups[3].Value.Length > 0)
                rebuilt += " " + match.Groups[3].Value;

            Assert.Equal(expected, rebuilt);
        }

        /// <summary>
        /// 逐行核对。条数不一致时直接失败。
        /// </summary>
        public static void Lines(IReadOnlyList<string> expected, IReadOnlyList<string> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            for (var index = 0; index < expected.Count; index++)
                Line(expected[index], actual[index]);
        }
    }
}
