using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// JSON 路径试算。不跑流程。
    /// </summary>
    public class JsonPathValueTests
    {
        /// <summary>
        /// 点号和方括号能取到字符串，引号去掉。
        /// </summary>
        [Fact]
        public void ReadsNestedString()
        {
            var read = JsonPathValue.Read("""{"data":{"items":[{"name":"笔"}]}}""", "$.data.items[0].name");
            Assert.True(read.Ok);
            Assert.Equal("笔", read.Text);
        }

        /// <summary>
        /// 没有 $ 时从根开始。数组下标可以连续。
        /// </summary>
        [Fact]
        public void ReadsArrayWithoutDollar()
        {
            var read = JsonPathValue.Read("""[[10,20]]""", "[0][1]");
            Assert.True(read.Ok);
            Assert.Equal("20", read.Text);
        }

        /// <summary>
        /// 空路径返回整份原文。对象和布尔保留 JSON 原文。
        /// </summary>
        [Fact]
        public void EmptyPathAndRawValues()
        {
            var root = JsonPathValue.Read("""{"a":1}""", "  ");
            Assert.True(root.Ok);
            Assert.Equal("""{"a":1}""", root.Text);

            var obj = JsonPathValue.Read("""{"a":{"b":1}}""", "a");
            Assert.Equal("""{"b":1}""", obj.Text);

            var flag = JsonPathValue.Read("""{"ok":true,"n":null}""", "ok");
            Assert.Equal("true", flag.Text);

            var nothing = JsonPathValue.Read("""{"ok":true,"n":null}""", "$.n");
            Assert.Equal("null", nothing.Text);
        }

        /// <summary>
        /// 样例、路径和取值失败时给出固定原因。
        /// </summary>
        [Theory]
        [InlineData("", "a", "样例为空")]
        [InlineData("  ", "a", "样例为空")]
        [InlineData("{", "a", "JSON 无效")]
        [InlineData("{}", "a..b", "路径无效")]
        [InlineData("{}", "a[", "路径无效")]
        [InlineData("{}", "a[-1]", "路径无效")]
        [InlineData("{}", "a[0].", "路径无效")]
        [InlineData("{}", "missing", "路径不存在")]
        [InlineData("""{"a":[]}""", "a[0]", "路径不存在")]
        [InlineData("""{"a":1}""", "a.b", "路径不存在")]
        public void Rejects(string json, string path, string error)
        {
            var read = JsonPathValue.Read(json, path);
            Assert.False(read.Ok);
            Assert.Equal(error, read.Text);
        }
    }
}
