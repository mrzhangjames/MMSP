using MMSP.Service.Flow;
using MMSP.Service.Flow.Steps;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// Json 解析按来源变量和路径取值，并把转换后的文本写入出参。
    /// </summary>
    public class JsonParseFlowToolTests
    {
        /// <summary>
        /// 字符串去掉引号。来源原文保留，结果写入出参。
        /// </summary>
        [Fact]
        public void StoresConvertedString()
        {
            var variables = FlowVariables.FromSeeds([("进站报文", """{"barcode":"SN001"}"""), ("解析值", "")]);
            var cfg = """{"Source":"进站报文","JsonPath":"$.barcode","Target":"解析值","Inputs":[{"Variable":"进站报文"}]}""";
            var result = new JsonParseFlowTool(cfg).Execute(Context(variables));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal("SN001", result.Text);
            Assert.Equal("SN001", variables.Get("解析值"));
            Assert.Equal("""{"barcode":"SN001"}""", variables.Get("进站报文"));
        }

        /// <summary>
        /// 数字、布尔和对象保留 JSON 原文。空路径返回整份文本。
        /// </summary>
        [Fact]
        public void KeepsRawValues()
        {
            var json = """{"ok":true,"n":20,"row":{"b":1}}""";
            var variables = FlowVariables.FromSeeds([("进站报文", json), ("解析值", "")]);
            var number = Run(variables, "$.n");
            Assert.Equal("20", number.Text);
            Assert.Equal("20", variables.Get("解析值"));

            var flag = Run(variables, "$.ok");
            Assert.Equal("true", flag.Text);

            var row = Run(variables, "$.row");
            Assert.Equal("""{"b":1}""", row.Text);

            var root = Run(variables, "  ");
            Assert.Equal(json, root.Text);
        }

        /// <summary>
        /// 路径里的 {变量名} 换成当前文本后再取值。
        /// </summary>
        [Fact]
        public void ExpandsPathVariable()
        {
            var variables = FlowVariables.FromSeeds(
            [
                ("进站报文", """{"rows":[{"code":"A"},{"code":"B"}]}"""),
                ("工位状态", "1"),
                ("字段", "code"),
                ("解析值", "旧")
            ]);
            var cfg = """{"Source":"进站报文","JsonPath":"$.rows[{工位状态}].{字段}","Target":"解析值","Inputs":[{"Variable":"进站报文"},{"Variable":"工位状态"}]}""";
            var result = new JsonParseFlowTool(cfg).Execute(Context(variables));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal("B", result.Text);
            Assert.Equal("B", variables.Get("解析值"));
        }

        /// <summary>
        /// 来源没有文本、JSON 损坏或路径不存在时失败，出参保持原值。
        /// </summary>
        [Theory]
        [InlineData("", "$.a", "来源为空")]
        [InlineData("  ", "$.a", "来源为空")]
        [InlineData("{", "$.a", "JSON 无效")]
        [InlineData("""{"a":1}""", "a..b", "路径无效")]
        [InlineData("""{"a":1}""", "missing", "路径不存在")]
        public void RejectsSourceOrPath(string json, string path, string error)
        {
            var variables = FlowVariables.FromSeeds([("进站报文", json), ("解析值", "旧")]);
            var result = Run(variables, path);
            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.Equal(error, result.Text);
            Assert.Equal("旧", variables.Get("解析值"));
        }

        /// <summary>
        /// 没选来源、入参未声明、路径里的名称未声明，都不取值。
        /// </summary>
        [Fact]
        public void MissingNames_Fail()
        {
            var declared = FlowVariables.FromSeeds([("进站报文", """{"a":1}"""), ("解析值", "旧")]);
            var noSource = new JsonParseFlowTool("""{"JsonPath":"$.a","Target":"解析值"}""").Execute(Context(declared));
            Assert.Equal("来源为空", noSource.Text);
            Assert.Equal("旧", declared.Get("解析值"));

            var unknownInput = """{"Source":"进站报文","JsonPath":"$.a","Inputs":[{"Variable":"没有"}]}""";
            var missing = new JsonParseFlowTool(unknownInput).Execute(Context(declared));
            Assert.Equal("变量未声明", missing.Text);

            var unknownPath = """{"Source":"进站报文","JsonPath":"$.{没有}","Target":"解析值","Inputs":[{"Variable":"进站报文"}]}""";
            var path = new JsonParseFlowTool(unknownPath).Execute(Context(declared));
            Assert.Equal("变量未声明", path.Text);
            Assert.Equal("旧", declared.Get("解析值"));

            var open = """{"Source":"进站报文","JsonPath":"$.{a","Target":"解析值"}""";
            var invalid = new JsonParseFlowTool(open).Execute(Context(declared));
            Assert.Equal("路径无效", invalid.Text);
        }

        /// <summary>
        /// 出参为空时仍返回转换结果。出参未声明时失败，来源不动。
        /// </summary>
        [Fact]
        public void TargetOptional_UndeclaredFails()
        {
            var json = """{"barcode":"SN001"}""";
            var open = FlowVariables.FromSeeds([("进站报文", json)]);
            var skipped = new JsonParseFlowTool("""{"Source":"进站报文","JsonPath":"$.barcode","Target":"  "}""")
                .Execute(Context(open));
            Assert.Equal(FlowStepStatus.Ok, skipped.Status);
            Assert.Equal("SN001", skipped.Text);

            var variables = FlowVariables.FromSeeds([("进站报文", json)]);
            var failed = new JsonParseFlowTool("""{"Source":"进站报文","JsonPath":"$.barcode","Target":"解析值"}""")
                .Execute(Context(variables));
            Assert.Equal("变量未声明", failed.Text);
            Assert.Equal(json, variables.Get("进站报文"));
        }

        /// <summary>
        /// 已经取消时不写变量。
        /// </summary>
        [Fact]
        public void Cancelled_DoesNotWrite()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();
            var variables = FlowVariables.FromSeeds([("进站报文", """{"barcode":"SN001"}"""), ("解析值", "旧")]);
            var cfg = """{"Source":"进站报文","JsonPath":"$.barcode","Target":"解析值"}""";
            var result = new JsonParseFlowTool(cfg).Execute(new FlowStepContext(source.Token, new FakeDevices(), variables));
            Assert.Equal(FlowStepStatus.Cancelled, result.Status);
            Assert.Equal("旧", variables.Get("解析值"));
        }

        /// <summary>
        /// 按当前变量表跑一次解析。
        /// </summary>
        private static FlowStepResult Run(FlowVariables variables, string path)
        {
            var cfg = "{\"Source\":\"进站报文\",\"JsonPath\":" + System.Text.Json.JsonSerializer.Serialize(path) + ",\"Target\":\"解析值\"}";
            return new JsonParseFlowTool(cfg).Execute(Context(variables));
        }

        /// <summary>
        /// 做一个带变量表的执行参数。
        /// </summary>
        private static FlowStepContext Context(FlowVariables variables)
        {
            return new FlowStepContext(CancellationToken.None, new FakeDevices(), variables);
        }
    }
}
