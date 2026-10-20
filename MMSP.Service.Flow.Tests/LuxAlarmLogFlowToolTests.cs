using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;
using MMSP.Service.Device.Plc;
using MMSP.Service.Device.Scan;
using MMSP.Service.Flow;
using MMSP.Service.Flow.Steps;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 立讯报警日志的上升沿和失败继续。
    /// </summary>
    public class LuxAlarmLogFlowToolTests
    {
        /// <summary>
        /// 相等时记一次。值不变不记。离开后再回来再记一次。
        /// </summary>
        [Fact]
        public void RisingEdge_LogsOnlyWhenValueBecomesTrigger()
        {
            var infos = new List<string>();
            var errors = new List<string>();
            var tool = Create(infos, errors, Row(" D1 ", "1", "急停"));
            var devices = new FakeDevices();
            devices.ReadResults.Enqueue(PlcIoResult.Ok("1"));
            devices.ReadResults.Enqueue(PlcIoResult.Ok("1"));
            devices.ReadResults.Enqueue(PlcIoResult.Ok("0"));
            devices.ReadResults.Enqueue(PlcIoResult.Ok("1"));

            var first = tool.Execute(new FlowStepContext(CancellationToken.None, devices));
            var second = tool.Execute(new FlowStepContext(CancellationToken.None, devices));
            var third = tool.Execute(new FlowStepContext(CancellationToken.None, devices));
            var fourth = tool.Execute(new FlowStepContext(CancellationToken.None, devices));

            Assert.Equal(FlowStepStatus.Ok, first.Status);
            Assert.Equal(FlowStepStatus.Ok, second.Status);
            Assert.Equal(FlowStepStatus.Ok, third.Status);
            Assert.Equal(FlowStepStatus.Ok, fourth.Status);
            Assert.Equal(["急停 D1 1", "急停 D1 1"], infos);
            Assert.Empty(errors);
            Assert.Equal(
                [("D1", PlcDataType.UInt16, 1), ("D1", PlcDataType.UInt16, 1), ("D1", PlcDataType.UInt16, 1), ("D1", PlcDataType.UInt16, 1)],
                devices.Reads);
        }

        /// <summary>
        /// 不勾选记录日志时记住状态，但不写日志。
        /// </summary>
        [Fact]
        public void LogUnchecked_RemembersMatchWithoutLogging()
        {
            var infos = new List<string>();
            var errors = new List<string>();
            var tool = Create(infos, errors, false, Row("D1", "1", "急停"));
            var devices = new FakeDevices();
            devices.ReadResults.Enqueue(PlcIoResult.Ok("1"));
            devices.ReadResults.Enqueue(PlcIoResult.Ok("1"));

            var first = tool.Execute(new FlowStepContext(CancellationToken.None, devices));
            var second = tool.Execute(new FlowStepContext(CancellationToken.None, devices));

            Assert.Equal(FlowStepStatus.Ok, first.Status);
            Assert.Equal(FlowStepStatus.Ok, second.Status);
            Assert.Empty(infos);
            Assert.Empty(errors);
            Assert.Equal(2, devices.Reads.Count);
        }

        /// <summary>
        /// 地址空白的行不读。
        /// </summary>
        [Fact]
        public void BlankAddress_SkipsRead()
        {
            var infos = new List<string>();
            var errors = new List<string>();
            var tool = Create(infos, errors, Row("  ", "1", "急停"));
            var devices = new FakeDevices();

            var result = tool.Execute(new FlowStepContext(CancellationToken.None, devices));

            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Empty(devices.Reads);
            Assert.Empty(infos);
            Assert.Empty(errors);
        }

        /// <summary>
        /// 一行读失败只记这一行，其它行继续，整步成功。
        /// </summary>
        [Fact]
        public void OneRowFailure_ContinuesAndStepSucceeds()
        {
            var infos = new List<string>();
            var errors = new List<string>();
            var tool = Create(infos, errors, Row("D100", "1", "急停"), Row("D200", "1", "过站"));
            var devices = new FakeDevices();
            devices.ReadResults.Enqueue(PlcIoResult.Fail("超时"));
            devices.ReadResults.Enqueue(PlcIoResult.Ok("1"));

            var result = tool.Execute(new FlowStepContext(CancellationToken.None, devices));

            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal("", result.Text);
            Assert.Equal(["D100 超时"], errors);
            Assert.Equal(["过站 D200 1"], infos);
            Assert.Equal(2, devices.Reads.Count);
        }

        /// <summary>
        /// 读失败不改这一行的沿状态。后面仍相等时不再记一次。
        /// </summary>
        [Fact]
        public void ReadFailure_DoesNotClearPreviousMatch()
        {
            var infos = new List<string>();
            var errors = new List<string>();
            var tool = Create(infos, errors, Row("D1", "1", "急停"));

            Run(tool, PlcIoResult.Ok("1"));
            Run(tool, PlcIoResult.Fail("超时"));
            Run(tool, PlcIoResult.Ok("1"));

            Assert.Equal(["急停 D1 1"], infos);
            Assert.Equal(["D1 超时"], errors);
        }

        /// <summary>
        /// 损坏的配置让这一步失败。
        /// </summary>
        [Fact]
        public void BadJson_Fails()
        {
            var tool = new LuxAlarmLogFlowTool("{", _ => { }, _ => { });

            var result = tool.Execute(new FlowStepContext(CancellationToken.None, new FakeDevices()));

            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.Equal("配置无效", result.Text);
        }

        /// <summary>
        /// 空表成功，不读设备。
        /// </summary>
        [Theory]
        [InlineData("{}")]
        [InlineData("{\"Rows\":[]}")]
        public void EmptyRows_Succeeds(string cfg)
        {
            var devices = new FakeDevices();
            var tool = new LuxAlarmLogFlowTool(cfg, _ => { }, _ => { });

            var result = tool.Execute(new FlowStepContext(CancellationToken.None, devices));

            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Empty(devices.Reads);
        }

        /// <summary>
        /// 01 和 1 按 UInt16 相等。
        /// </summary>
        [Fact]
        public void UInt16_LeadingZero_Matches()
        {
            var infos = new List<string>();
            var tool = Create(infos, [], Row("D1", "1", "急停"));

            var result = Run(tool, PlcIoResult.Ok("01"));

            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal(["急停 D1 01"], infos);
        }

        /// <summary>
        /// 读的过程中取消，不当成这一行的通讯失败。
        /// </summary>
        [Fact]
        public void CancelledRead_ReturnsCancelled()
        {
            var errors = new List<string>();
            var tool = Create([], errors, Row("D1", "1", "急停"));
            using var source = new CancellationTokenSource();
            var devices = new CancelOnRead(source);

            var result = tool.Execute(new FlowStepContext(source.Token, devices));

            Assert.Equal(FlowStepStatus.Cancelled, result.Status);
            Assert.Empty(errors);
        }

        /// <summary>
        /// 做一行报警。
        /// </summary>
        private static LuxAlarmLogRow Row(string address, string trigger, string description)
        {
            return new LuxAlarmLogRow
            {
                Address = address,
                DataType = PlcDataType.UInt16,
                TriggerValue = trigger,
                Description = description
            };
        }

        /// <summary>
        /// 用测试日志创建工具。记录日志默认勾选。
        /// </summary>
        private static LuxAlarmLogFlowTool Create(List<string> infos, List<string> errors, params LuxAlarmLogRow[] rows)
        {
            return Create(infos, errors, true, rows);
        }

        /// <summary>
        /// 用测试日志创建工具，并指定整份工具是否记录日志。
        /// </summary>
        private static LuxAlarmLogFlowTool Create(List<string> infos, List<string> errors, bool log, params LuxAlarmLogRow[] rows)
        {
            var cfg = System.Text.Json.JsonSerializer.Serialize(new LuxAlarmLogToolEntity
            {
                Log = log,
                Rows = rows.ToList()
            });
            return new LuxAlarmLogFlowTool(cfg, infos.Add, errors.Add);
        }

        /// <summary>
        /// 跑一轮。
        /// </summary>
        private static FlowStepResult Run(LuxAlarmLogFlowTool tool, PlcIoResult read)
        {
            var devices = new FakeDevices();
            devices.ReadResults.Enqueue(read);
            return tool.Execute(new FlowStepContext(CancellationToken.None, devices));
        }

        /// <summary>
        /// 读取时取消标记。用来确认取消不记成行错误。
        /// </summary>
        private sealed class CancelOnRead : IFlowDeviceSession
        {
            /// <summary>
            /// 读取时取消的来源。
            /// </summary>
            private readonly CancellationTokenSource _source;

            /// <summary>
            /// 记下要取消的来源。
            /// </summary>
            public CancelOnRead(CancellationTokenSource source)
            {
                _source = source;
            }

            /// <summary>
            /// 读完立刻取消，让结果走取消而不是成功。
            /// </summary>
            public PlcIoResult Read(string address, PlcDataType dataType, int length, CancellationToken token)
            {
                _source.Cancel();
                return PlcIoResult.Ok("1");
            }

            /// <summary>
            /// 这个测试不触发扫码枪。
            /// </summary>
            public ScanIoResult Trigger(string scannerName, string command, CancellationToken token)
            {
                return ScanIoResult.Ok("");
            }

            /// <summary>
            /// 这个测试不写 PLC。
            /// </summary>
            public PlcIoResult Write(string address, PlcDataType dataType, string value, CancellationToken token)
            {
                return PlcIoResult.Ok("");
            }

            /// <summary>
            /// 这个测试不等待。
            /// </summary>
            public bool Wait(int milliseconds, CancellationToken token)
            {
                return false;
            }

            /// <summary>
            /// 没有连接要关。
            /// </summary>
            public void Dispose()
            {
            }
        }
    }
}
