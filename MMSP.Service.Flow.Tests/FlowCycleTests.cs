using MMSP.Service.Flow;
using MMSP.Service.Flow.Steps;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 一轮的顺序、失败和日志。
    /// </summary>
    public class FlowCycleTests
    {
        /// <summary>
        /// 一步失败后，同流程后面的步骤不跑，下一条流程继续。
        /// </summary>
        [Fact]
        public void Failure_SkipsRestOfFlow_ContinuesNext()
        {
            var first = new FixedTool(FlowStepResult.Ok("a"));
            var failed = new FixedTool(FlowStepResult.Fail("断了"));
            var skipped = new FixedTool(FlowStepResult.Ok("b"));
            var next = new FixedTool(FlowStepResult.Ok("c"));
            var flows = new[]
            {
                new RunnableFlow("甲", true, new[]
                {
                    new RunnableStep(1, "一步", first),
                    new RunnableStep(2, "二步", failed),
                    new RunnableStep(3, "三步", skipped)
                }),
                new RunnableFlow("乙", true, new[]
                {
                    new RunnableStep(1, "四步", next)
                })
            };
            var infos = new List<string>();
            var errors = new List<string>();

            var stop = FlowCycle.Execute(flows, Context(), infos.Add, errors.Add);

            Assert.Equal(FlowCycleStop.None, stop);
            Assert.Equal(1, first.Calls);
            Assert.Equal(1, failed.Calls);
            Assert.Equal(0, skipped.Calls);
            Assert.Equal(1, next.Calls);
            FlowLogAssert.Lines(new[] { "甲 1 一步 a", "乙 1 四步 c" }, infos);
            FlowLogAssert.Lines(new[] { "甲 2 二步 断了" }, errors);
        }

        /// <summary>
        /// 关闭日志输出时不写成功行，失败仍然写。
        /// </summary>
        [Fact]
        public void LogOutputFalse_SkipsInfo_KeepsError()
        {
            var flows = new[]
            {
                new RunnableFlow("甲", false, new[]
                {
                    new RunnableStep(1, "一步", new FixedTool(FlowStepResult.Ok("a"))),
                    new RunnableStep(2, "二步", new FixedTool(FlowStepResult.Fail("断了")))
                })
            };
            var infos = new List<string>();
            var errors = new List<string>();

            FlowCycle.Execute(flows, Context(), infos.Add, errors.Add);

            Assert.Empty(infos);
            FlowLogAssert.Line("甲 2 二步 断了", errors[0]);
        }

        /// <summary>
        /// 没有步骤时让服务停掉，避免空转。
        /// </summary>
        [Fact]
        public void Empty_ReturnsEmpty()
        {
            var flows = new[] { new RunnableFlow("甲", true, Array.Empty<RunnableStep>()) };
            var stop = FlowCycle.Execute(flows, Context(), _ => { }, _ => { });
            Assert.Equal(FlowCycleStop.Empty, stop);
        }

        /// <summary>
        /// 取消不当成步骤失败。
        /// </summary>
        [Fact]
        public void Cancel_DoesNotLogFailure()
        {
            var flows = new[]
            {
                new RunnableFlow("甲", true, new[]
                {
                    new RunnableStep(1, "一步", new FixedTool(FlowStepResult.Cancelled()))
                })
            };
            var errors = new List<string>();
            var stop = FlowCycle.Execute(flows, Context(), _ => { }, errors.Add);
            Assert.Equal(FlowCycleStop.Cancelled, stop);
            Assert.Empty(errors);
        }

        /// <summary>
        /// 日志行带上流程名、顺序、标题和结果。
        /// </summary>
        [Fact]
        public void LogText_JoinsParts()
        {
            Assert.Equal("甲 2 扫码 15 ms BC", FlowLogText.Format("甲", 2, "扫码", 15, "BC"));
            Assert.Equal("甲 2 扫码 0 ms", FlowLogText.Format(" 甲 ", 2, " 扫码 ", 0, " "));
        }

        /// <summary>
        /// 每步先报执行中，结束后带耗时和结果。失败后面的步骤标成跳过。
        /// </summary>
        [Fact]
        public void Progress_ReportsStateAndElapsed_SkipsRest()
        {
            var flows = new[]
            {
                new RunnableFlow("甲", true, new[]
                {
                    new RunnableStep(1, "一步", new FixedTool(FlowStepResult.Ok("条码"))),
                    new RunnableStep(2, "二步", new FixedTool(FlowStepResult.Fail("断了"))),
                    new RunnableStep(3, "三步", new FixedTool(FlowStepResult.Ok("不会跑")))
                })
            };
            var infos = new List<string>();
            var errors = new List<string>();
            var marks = new List<FlowStepProgress>();

            FlowCycle.Execute(flows, Context(), infos.Add, errors.Add, progress: marks.Add);

            Assert.Equal(
                new[] { FlowStepRunState.Running, FlowStepRunState.Ok, FlowStepRunState.Running, FlowStepRunState.Failed, FlowStepRunState.Skipped },
                marks.Select(item => item.State).ToArray());
            Assert.Equal("条码", marks[1].Detail);
            Assert.Equal("断了", marks[3].Detail);
            Assert.Equal(3, marks[4].SortIndex);
            FlowLogAssert.Line("甲 1 一步 条码", infos[0]);
            FlowLogAssert.Line("甲 2 二步 断了", errors[0]);
            Assert.Contains(marks[1].ElapsedMs.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ms", infos[0]);
        }

        /// <summary>
        /// 做一个不访问设备的执行参数。
        /// </summary>
        private static FlowStepContext Context()
        {
            return new FlowStepContext(CancellationToken.None, new FakeDevices());
        }

        /// <summary>
        /// 测试里固定返回同一种结果，并记下调用次数。
        /// </summary>
        private sealed class FixedTool : FlowToolStep
        {
            private readonly FlowStepResult _result;

            /// <summary>
            /// 记下要返回的结果。
            /// </summary>
            public FixedTool(FlowStepResult result)
            {
                _result = result;
            }

            /// <summary>
            /// 被调用的次数。
            /// </summary>
            public int Calls { get; private set; }

            /// <summary>
            /// 返回构造时给定的结果。
            /// </summary>
            public override FlowStepResult Execute(FlowStepContext context)
            {
                Calls++;
                return _result;
            }
        }
    }
}
