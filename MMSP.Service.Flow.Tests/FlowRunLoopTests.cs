using MMSP.Service.Flow;
using MMSP.Service.Flow.Steps;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 每条流程单独循环：并行、失败收拢、轮间等待。
    /// </summary>
    public class FlowRunLoopTests
    {
        /// <summary>
        /// 同一步、同一原因连续失败只写第一条。原因变了再写。
        /// </summary>
        [Fact]
        public void SameStep_SameReason_LogsOnce_ThenLogsChange()
        {
            var devices = new FakeDevices();
            var round = 0;
            devices.OnWait = _ => ++round < 3;
            var tool = new QueueTool(
                FlowStepResult.Fail("断了"),
                FlowStepResult.Fail("断了"),
                FlowStepResult.Fail("换了"));
            var errors = new List<string>();

            var cancelled = Run(new[] { Flow("甲", tool) }, devices, _ => { }, errors.Add);

            Assert.False(cancelled);
            FlowLogAssert.Lines(new[] { "甲 1 一步 断了", "甲 1 一步 换了" }, errors);
            Assert.Equal(new[] { FlowRunLoop.RoundIntervalMs, FlowRunLoop.RoundIntervalMs, FlowRunLoop.RoundIntervalMs }, devices.Waits);
            Assert.Equal(3, tool.Calls);
        }

        /// <summary>
        /// 中间成功过一次后，同样的失败原因重新写。
        /// </summary>
        [Fact]
        public void Success_AllowsSameReasonAgain()
        {
            var devices = new FakeDevices();
            var round = 0;
            devices.OnWait = _ => ++round < 3;
            var tool = new QueueTool(
                FlowStepResult.Fail("断了"),
                FlowStepResult.Ok("好"),
                FlowStepResult.Fail("断了"));
            var infos = new List<string>();
            var errors = new List<string>();

            Run(new[] { Flow("甲", true, tool) }, devices, infos.Add, errors.Add);

            FlowLogAssert.Lines(new[] { "甲 1 一步 好" }, infos);
            FlowLogAssert.Lines(new[] { "甲 1 一步 断了", "甲 1 一步 断了" }, errors);
        }

        /// <summary>
        /// 没有步骤的流程只退出自己，另一条流程继续跑。空流程不等待。
        /// </summary>
        [Fact]
        public void Empty_StopsOnlyThatFlow()
        {
            var devices = new FakeDevices();
            using var cts = new CancellationTokenSource();
            var calls = 0;
            var running = new DelegateTool(() =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                    cts.Cancel();

                return FlowStepResult.Ok("好");
            });
            var errors = new List<string>();

            var cancelled = Run(
                new[]
                {
                    new RunnableFlow("甲", true, Array.Empty<RunnableStep>()),
                    Flow("乙", running)
                },
                devices,
                _ => { },
                errors.Add,
                cts.Token);

            Assert.True(cancelled);
            Assert.Equal(new[] { "没有可执行的步骤" }, errors);
            Assert.True(calls >= 1);
            Assert.All(devices.Waits, wait => Assert.Equal(FlowRunLoop.RoundIntervalMs, wait));
        }

        /// <summary>
        /// 一步失败后另一条流程仍然执行。相同失败不重复写。
        /// </summary>
        [Fact]
        public void Failure_OtherFlowContinues()
        {
            using var cts = new CancellationTokenSource();
            var goodCalls = 0;
            var good = new DelegateTool(() =>
            {
                if (Interlocked.Increment(ref goodCalls) == 1)
                    cts.Cancel();

                return FlowStepResult.Ok("好");
            });
            var errors = new List<string>();

            Run(
                new[] { Flow("甲", new RepeatTool(FlowStepResult.Fail("断了"))), Flow("乙", good) },
                new FakeDevices(),
                _ => { },
                errors.Add,
                cts.Token);

            Assert.True(goodCalls >= 1);
            FlowLogAssert.Lines(new[] { "甲 1 一步 断了" }, errors);
        }

        /// <summary>
        /// 未预料异常只退出这一条流程。
        /// </summary>
        [Fact]
        public void Exception_StopsOnlyThatFlow()
        {
            using var cts = new CancellationTokenSource();
            var calls = 0;
            var running = new DelegateTool(() =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                    cts.Cancel();

                return FlowStepResult.Ok("好");
            });
            var errors = new List<string>();

            Run(
                new[] { Flow("甲", new DelegateTool(() => FlowStepResult.Ok("甲"))), Flow("乙", running) },
                new FakeDevices(),
                line =>
                {
                    if (line.StartsWith("甲", StringComparison.Ordinal))
                        throw new InvalidOperationException("坏了");
                },
                (message, _) => errors.Add(message),
                cts.Token);

            Assert.True(calls >= 1);
            Assert.Contains("流程执行失败", errors);
        }

        /// <summary>
        /// 停在当前步骤里的流程要等这一步返回，其它流程可以先跑完。
        /// </summary>
        [Fact]
        public async Task Stop_WaitsForCurrentStep()
        {
            var entered = new ManualResetEventSlim(false);
            var release = new ManualResetEventSlim(false);
            using var cts = new CancellationTokenSource();
            var calls = 0;
            var running = new DelegateTool(() =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                    cts.Cancel();

                return FlowStepResult.Ok("好");
            });
            var flows = new[] { Flow("甲", new HoldTool(entered, release)), Flow("乙", running) };
            var devices = new FakeDevices();
            var run = Task.Factory.StartNew(
                () => Run(flows, devices, _ => { }, _ => { }, cts.Token),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            try
            {
                Assert.True(entered.Wait(2000));
                Assert.True(SpinWait.SpinUntil(() => Volatile.Read(ref calls) >= 1, 2000));
                var finished = await Task.WhenAny(run, Task.Delay(200));
                Assert.NotSame(run, finished);
                release.Set();
                await run.WaitAsync(TimeSpan.FromSeconds(2));
            }
            finally
            {
                release.Set();
            }
        }

        /// <summary>
        /// 轮间等待被取消时立刻结束，不再进入下一轮。
        /// </summary>
        [Fact]
        public void RoundWait_Cancel_Exits()
        {
            using var cts = new CancellationTokenSource();
            var devices = new FakeDevices();
            devices.OnWait = _ =>
            {
                cts.Cancel();
                return false;
            };
            var tool = new RepeatTool(FlowStepResult.Ok("好"));

            var cancelled = Run(new[] { Flow("甲", tool) }, devices, _ => { }, _ => { }, cts.Token);

            Assert.True(cancelled);
            Assert.Equal(1, tool.Calls);
            Assert.Equal(new[] { FlowRunLoop.RoundIntervalMs }, devices.Waits);
        }

        /// <summary>
        /// 跑给定流程并收集日志。回调可能来自不同任务。
        /// </summary>
        private static bool Run(
            IReadOnlyList<RunnableFlow> flows,
            FakeDevices devices,
            Action<string> info,
            Action<string> error,
            CancellationToken token = default)
        {
            return Run(flows, devices, info, (message, _) => error(message), token);
        }

        /// <summary>
        /// 跑给定流程。错误回调可以看见异常。
        /// </summary>
        private static bool Run(
            IReadOnlyList<RunnableFlow> flows,
            FakeDevices devices,
            Action<string> info,
            FlowErrorLog error,
            CancellationToken token = default)
        {
            var gate = new object();
            var context = new FlowStepContext(token, devices);
            return FlowRunLoop.Execute(
                flows,
                context,
                line =>
                {
                    lock (gate)
                        info(line);
                },
                (message, exception) =>
                {
                    lock (gate)
                        error(message, exception);
                });
        }

        /// <summary>
        /// 一条只有一步的流程。默认写出成功日志。
        /// </summary>
        private static RunnableFlow Flow(string name, FlowToolStep tool)
        {
            return Flow(name, true, tool);
        }

        /// <summary>
        /// 一条只有一步的流程。
        /// </summary>
        private static RunnableFlow Flow(string name, bool logOutput, FlowToolStep tool)
        {
            return new RunnableFlow(name, logOutput, new[] { new RunnableStep(1, "一步", tool) });
        }

        /// <summary>
        /// 按给定顺序返回结果。
        /// </summary>
        private sealed class QueueTool : FlowToolStep
        {
            /// <summary>
            /// 还没返回的结果。
            /// </summary>
            private readonly Queue<FlowStepResult> _results;

            /// <summary>
            /// 记下要依次返回的结果。
            /// </summary>
            public QueueTool(params FlowStepResult[] results)
            {
                _results = new Queue<FlowStepResult>(results);
            }

            /// <summary>
            /// 被调用的次数。
            /// </summary>
            public int Calls { get; private set; }

            /// <summary>
            /// 返回下一个结果。队列空了就失败。
            /// </summary>
            public override FlowStepResult Execute(FlowStepContext context)
            {
                Calls++;
                if (_results.Count == 0)
                    return FlowStepResult.Fail("没有结果");

                return _results.Dequeue();
            }
        }

        /// <summary>
        /// 每次都返回同一个结果。
        /// </summary>
        private sealed class RepeatTool : FlowToolStep
        {
            /// <summary>
            /// 固定结果。
            /// </summary>
            private readonly FlowStepResult _result;

            /// <summary>
            /// 记下要重复返回的结果。
            /// </summary>
            public RepeatTool(FlowStepResult result)
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

        /// <summary>
        /// 把一步交给测试里的委托。
        /// </summary>
        private sealed class DelegateTool : FlowToolStep
        {
            /// <summary>
            /// 实际执行。
            /// </summary>
            private readonly Func<FlowStepResult> _run;

            /// <summary>
            /// 记下这一步要做的事。
            /// </summary>
            public DelegateTool(Func<FlowStepResult> run)
            {
                _run = run;
            }

            /// <summary>
            /// 调用委托。
            /// </summary>
            public override FlowStepResult Execute(FlowStepContext context)
            {
                return _run();
            }
        }

        /// <summary>
        /// 进入后停住，直到测试放行。用来表示当前通讯还没返回。
        /// </summary>
        private sealed class HoldTool : FlowToolStep
        {
            /// <summary>
            /// 已经进入这一步。
            /// </summary>
            private readonly ManualResetEventSlim _entered;

            /// <summary>
            /// 测试允许这一步返回。
            /// </summary>
            private readonly ManualResetEventSlim _release;

            /// <summary>
            /// 记下两个信号。
            /// </summary>
            public HoldTool(ManualResetEventSlim entered, ManualResetEventSlim release)
            {
                _entered = entered;
                _release = release;
            }

            /// <summary>
            /// 通知进入后一直等到放行。
            /// </summary>
            public override FlowStepResult Execute(FlowStepContext context)
            {
                _entered.Set();
                _release.Wait();
                return FlowStepResult.Ok("停住");
            }
        }
    }
}
