using System.Text.Json;
using MMSP.Core.Model.Tool;
using MMSP.Service.Flow;
using MMSP.Service.Flow.Steps;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 条件成立时写入 true。不成立、为空或无效时失败，并且不改出参。
    /// </summary>
    public class ConditionFlowToolTests
    {
        /// <summary>
        /// 成立时结果是 true，并写入出参。
        /// </summary>
        [Fact]
        public void Pass_StoresTrue()
        {
            var variables = FlowVariables.FromSeeds([("条码", "SN001234567"), ("合格", "")]);
            var result = Run(variables, "{条码} 长度 > 8");
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal("true", result.Text);
            Assert.Equal("true", variables.Get("合格"));
            Assert.Equal("SN001234567", variables.Get("条码"));
        }

        /// <summary>
        /// 不成立时本步失败，出参保持原值。
        /// </summary>
        [Fact]
        public void Fail_KeepsOutput()
        {
            var variables = FlowVariables.FromSeeds([("条码", "SN001"), ("合格", "旧")]);
            var result = Run(variables, "{条码} 长度 > 8");
            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.Equal("条件不成立", result.Text);
            Assert.Equal("旧", variables.Get("合格"));
        }

        /// <summary>
        /// 没写条件，或只写空白，这一步失败，出参不动。
        /// </summary>
        [Fact]
        public void Blank_Fails()
        {
            var variables = FlowVariables.FromSeeds([("合格", "旧")]);
            var empty = new ConditionFlowTool("""{"Name":"x","Output":"合格"}""").Execute(Context(variables));
            Assert.Equal("条件为空", empty.Text);
            Assert.Equal("旧", variables.Get("合格"));

            var spaces = Run(variables, "   ");
            Assert.Equal("条件为空", spaces.Text);
            Assert.Equal("旧", variables.Get("合格"));
        }

        /// <summary>
        /// 表达式拆不开时失败，出参不动。
        /// </summary>
        [Fact]
        public void Invalid_Fails()
        {
            var variables = FlowVariables.FromSeeds([("条码", "SN001234567"), ("合格", "旧")]);
            var result = Run(variables, "长度大于 8");
            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.Equal("条件无效", result.Text);
            Assert.Equal("旧", variables.Get("合格"));

            var open = Run(variables, "{条码");
            Assert.Equal("条件无效", open.Text);
            Assert.Equal("旧", variables.Get("合格"));
        }

        /// <summary>
        /// 入参未声明时还没计算表达式。表达式里的名称未声明同样失败，包括或的另一侧。
        /// </summary>
        [Fact]
        public void MissingNames_Fail()
        {
            var variables = FlowVariables.FromSeeds([("条码", "SN001234567"), ("工位状态", "10"), ("合格", "旧")]);
            var input = new ConditionToolEntity
            {
                Expression = "",
                Output = "合格",
                Inputs = [new ToolInput { Variable = "没有" }]
            };
            var missingInput = new ConditionFlowTool(JsonSerializer.Serialize(input)).Execute(Context(variables));
            Assert.Equal("变量未声明", missingInput.Text);
            Assert.Equal("旧", variables.Get("合格"));

            var missingName = Run(variables, "{没有} == 1");
            Assert.Equal("变量未声明", missingName.Text);
            Assert.Equal("旧", variables.Get("合格"));

            var otherSide = Run(variables, "{工位状态} == 10 || {没有} == 1");
            Assert.Equal("变量未声明", otherSide.Text);
            Assert.Equal("旧", variables.Get("合格"));
        }

        /// <summary>
        /// 出参为空时仍返回 true。出参未声明时失败，已有变量不动。
        /// </summary>
        [Fact]
        public void OutputOptional_UndeclaredFails()
        {
            var open = FlowVariables.FromSeeds([("条码", "SN001234567")]);
            var skipped = Run(open, "{条码} 长度 > 8", "  ");
            Assert.Equal(FlowStepStatus.Ok, skipped.Status);
            Assert.Equal("true", skipped.Text);
            Assert.False(open.Contains("合格"));

            var variables = FlowVariables.FromSeeds([("条码", "SN001234567")]);
            var failed = Run(variables, "{条码} 长度 > 8");
            Assert.Equal("变量未声明", failed.Text);
            Assert.Equal("SN001234567", variables.Get("条码"));
            Assert.False(variables.Contains("合格"));
        }

        /// <summary>
        /// 已经取消时不计算，也不写变量。
        /// </summary>
        [Fact]
        public void Cancelled_DoesNotWrite()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();
            var variables = FlowVariables.FromSeeds([("条码", "SN001234567"), ("合格", "旧")]);
            var cfg = JsonSerializer.Serialize(new ConditionToolEntity
            {
                Expression = "{条码} 长度 > 8",
                Output = "合格"
            });
            var result = new ConditionFlowTool(cfg).Execute(new FlowStepContext(source.Token, new FakeDevices(), variables));
            Assert.Equal(FlowStepStatus.Cancelled, result.Status);
            Assert.Equal("旧", variables.Get("合格"));
        }

        /// <summary>
        /// 配置损坏时不计算。
        /// </summary>
        [Fact]
        public void BadConfig_Fails()
        {
            var result = new ConditionFlowTool("{").Execute(Context(FlowVariables.Empty));
            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.Equal("配置无效", result.Text);
        }

        /// <summary>
        /// 文本相等区分大小写。10 大于 9 按数值。10 等于 10.0 不成立，因为相等不按数值。
        /// 条码大于 0 按码点。与优先于或。非作用在后面的整个比较上。
        /// </summary>
        [Theory]
        [InlineData("{条码} == SN001234567", true)]
        [InlineData("{条码} == sn001234567", false)]
        [InlineData("{条码} != SN001234567", false)]
        [InlineData("{工位状态} > 9", true)]
        [InlineData("{工位状态} > 8.0", true)]
        [InlineData("{工位状态} >= 10.0", true)]
        [InlineData("{工位状态} == 10", true)]
        [InlineData("{工位状态} == 10.0", false)]
        [InlineData("{工位状态} < 9", false)]
        [InlineData("{工位状态} <= 10", true)]
        [InlineData("{条码} > 0", true)]
        [InlineData("{条码} 长度 > 8", true)]
        [InlineData("{条码}长度>8", true)]
        [InlineData("{条码} 长度 == 11", true)]
        [InlineData("{条码} 长度 == 11.0", false)]
        [InlineData("{条码} 长度 >= 11.0", true)]
        [InlineData("{条码} 长度 > 20", false)]
        [InlineData("{条码} 包含 SN", true)]
        [InlineData("{条码} 包含 sn", false)]
        [InlineData("{条码} 包含 {工位状态}", false)]
        [InlineData("{条码} 长度 > 8 && {工位状态} == 10", true)]
        [InlineData("{工位状态} == 10 || {工位状态} == 0 && {条码} 长度 > 20", true)]
        [InlineData("!{工位状态} == 0", true)]
        [InlineData("!({条码} 包含 sn)", true)]
        public void Evaluates(string expression, bool passed)
        {
            var variables = FlowVariables.FromSeeds([("条码", "SN001234567"), ("工位状态", "10"), ("合格", "")]);
            var result = Run(variables, expression);
            if (passed)
            {
                Assert.Equal(FlowStepStatus.Ok, result.Status);
                Assert.Equal("true", result.Text);
                Assert.Equal("true", variables.Get("合格"));
                return;
            }

            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.Equal("条件不成立", result.Text);
            Assert.Equal("", variables.Get("合格"));
        }

        /// <summary>
        /// 用当前变量表跑一次条件。
        /// </summary>
        private static FlowStepResult Run(FlowVariables variables, string expression, string output = "合格")
        {
            var cfg = JsonSerializer.Serialize(new ConditionToolEntity
            {
                Expression = expression,
                Output = output
            });
            return new ConditionFlowTool(cfg).Execute(Context(variables));
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
