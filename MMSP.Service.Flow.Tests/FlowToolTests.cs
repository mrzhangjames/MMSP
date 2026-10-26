using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;
using MMSP.Service.Device.Plc;
using MMSP.Service.Device.Scan;
using MMSP.Service.Flow;
using MMSP.Service.Flow.Steps;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 工具子类的执行结果。
    /// </summary>
    public class FlowToolTests
    {
        /// <summary>
        /// 十一种已知工具都能从登记表创建出来。
        /// </summary>
        [Fact]
        public void Catalog_CreatesEachKind()
        {
            Assert.IsType<SingleScanFlowTool>(FlowToolCatalog.Create(FlowToolKind.SingleScan, ""));
            Assert.IsType<MultiScanFlowTool>(FlowToolCatalog.Create(FlowToolKind.MultiScan, ""));
            Assert.IsType<PlcReadFlowTool>(FlowToolCatalog.Create(FlowToolKind.PlcRead, ""));
            Assert.IsType<PlcWriteFlowTool>(FlowToolCatalog.Create(FlowToolKind.PlcWrite, ""));
            Assert.IsType<PlcMonitorFlowTool>(FlowToolCatalog.Create(FlowToolKind.PlcMonitor, ""));
            Assert.IsType<DelayFlowTool>(FlowToolCatalog.Create(FlowToolKind.Delay, ""));
            Assert.IsType<JsonParseFlowTool>(FlowToolCatalog.Create(FlowToolKind.JsonParse, ""));
            Assert.IsType<ConditionFlowTool>(FlowToolCatalog.Create(FlowToolKind.Condition, ""));
            Assert.IsType<ScriptFlowTool>(FlowToolCatalog.Create(FlowToolKind.Script, ""));
            Assert.IsType<UphFlowTool>(FlowToolCatalog.Create(FlowToolKind.Uph, ""));
            Assert.IsType<LuxAlarmLogFlowTool>(FlowToolCatalog.Create(FlowToolKind.Lux_AlarmLogTool, ""));
        }

        /// <summary>
        /// 登记表没有的种类返回未实现。
        /// </summary>
        [Fact]
        public void Catalog_UnknownKind_IsUnsupported()
        {
            var tool = FlowToolCatalog.Create((FlowToolKind)99, "");
            var result = tool.Execute(Context());
            Assert.IsType<UnsupportedFlowTool>(tool);
            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.StartsWith("未实现", result.Text);
        }

        /// <summary>
        /// 延时不绑定入参或出参。
        /// </summary>
        [Fact]
        public void Delay_HasNoIo()
        {
            Assert.False(ToolVars.HasInputs(FlowToolKind.Delay));
            Assert.False(ToolVars.HasOutput(FlowToolKind.Delay));
            var cfg = """{"Name":"等","Milliseconds":10}""";
            Assert.Empty(ToolVars.Referenced(FlowToolKind.Delay, cfg));
            Assert.Empty(ToolVars.OutputNames(FlowToolKind.Delay, cfg));
        }

        /// <summary>
        /// 延时 0 毫秒立刻成功。
        /// </summary>
        [Fact]
        public void Delay_Zero_Succeeds()
        {
            var result = new DelayFlowTool("""{"Milliseconds":0}""").Execute(Context());
            Assert.Equal(FlowStepStatus.Ok, result.Status);
        }

        /// <summary>
        /// 负数延时不进入等待。
        /// </summary>
        [Fact]
        public void Delay_Negative_Fails()
        {
            var result = new DelayFlowTool("""{"Milliseconds":-1}""").Execute(Context());
            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.Equal("配置无效", result.Text);
        }

        /// <summary>
        /// 空白配置不执行。
        /// </summary>
        [Fact]
        public void Delay_BlankConfig_Fails()
        {
            var result = new DelayFlowTool("  ").Execute(Context());
            Assert.Equal("配置无效", result.Text);
        }

        /// <summary>
        /// 已经取消时延时马上返回，不等待。
        /// </summary>
        [Fact]
        public void Delay_Cancelled_ReturnsCancelled()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();
            var result = new DelayFlowTool("""{"Milliseconds":60000}""").Execute(Context(source.Token));
            Assert.Equal(FlowStepStatus.Cancelled, result.Status);
        }

        /// <summary>
        /// 单扫码把条码原文返回。
        /// </summary>
        [Fact]
        public void SingleScan_ReturnsBarcode()
        {
            var devices = new FakeDevices();
            devices.ScanResults.Enqueue(ScanIoResult.Ok("BC001"));
            var result = new SingleScanFlowTool("""{"Scanner":"Gun","Trigger":"LON"}""").Execute(Context(devices: devices));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal("BC001", result.Text);
            Assert.Equal(("Gun", "LON"), devices.Triggers[0]);
        }

        /// <summary>
        /// 枪名为空时不触发。
        /// </summary>
        [Fact]
        public void SingleScan_MissingName_Fails()
        {
            var devices = new FakeDevices();
            var result = new SingleScanFlowTool("""{"Scanner":" ","Trigger":"LON"}""").Execute(Context(devices: devices));
            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.Empty(devices.Triggers);
        }

        /// <summary>
        /// 多扫码一次失败后不再触发后面的。
        /// </summary>
        [Fact]
        public void MultiScan_StopsAfterFailure()
        {
            var devices = new FakeDevices();
            devices.ScanResults.Enqueue(ScanIoResult.Ok("A1"));
            devices.ScanResults.Enqueue(ScanIoResult.Fail("接收超时"));
            var result = new MultiScanFlowTool("""{"Scanner":"Gun","Trigger":"LON","Count":2}""").Execute(Context(devices: devices));
            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.Equal("接收超时", result.Text);
            Assert.Equal(2, devices.Triggers.Count);
        }

        /// <summary>
        /// 多枪都成功时，条码按顿号拼起来。
        /// </summary>
        [Fact]
        public void MultiScan_JoinsBarcodes()
        {
            var devices = new FakeDevices();
            devices.ScanResults.Enqueue(ScanIoResult.Ok("A1"));
            devices.ScanResults.Enqueue(ScanIoResult.Ok("B1"));
            var result = new MultiScanFlowTool("""{"Scanner":"左、右","Trigger":"LON","Count":1}""").Execute(Context(devices: devices));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal("A1、B1", result.Text);
            Assert.Equal("左", devices.Triggers[0].Name);
            Assert.Equal("右", devices.Triggers[1].Name);
        }

        /// <summary>
        /// 两把枪、每枪两条时，出参按扫码顺序各写各的变量。
        /// </summary>
        [Fact]
        public void MultiScan_StoresEachBarcode()
        {
            var devices = new FakeDevices();
            devices.ScanResults.Enqueue(ScanIoResult.Ok("A1"));
            devices.ScanResults.Enqueue(ScanIoResult.Ok("A2"));
            devices.ScanResults.Enqueue(ScanIoResult.Ok("B1"));
            devices.ScanResults.Enqueue(ScanIoResult.Ok("B2"));
            var variables = FlowVariables.FromSeeds([("条码1", ""), ("条码2", ""), ("条码3", ""), ("条码4", "")]);
            var cfg = """{"Scanner":"左、右","Trigger":"LON","Count":2,"Outputs":["条码1","条码2","条码3","条码4"]}""";
            var result = new MultiScanFlowTool(cfg).Execute(new FlowStepContext(CancellationToken.None, devices, variables));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal("A1", variables.Get("条码1"));
            Assert.Equal("A2", variables.Get("条码2"));
            Assert.Equal("B1", variables.Get("条码3"));
            Assert.Equal("B2", variables.Get("条码4"));
        }

        /// <summary>
        /// 出参槽个数是枪数乘条码数量。没选枪时按一把算，空枪名段不算。
        /// </summary>
        [Fact]
        public void MultiScan_OutputSlotCount_FollowsGunsAndCount()
        {
            var tool = new MultiScanToolEntity { Scanner = "左、右", Count = 2 };
            Assert.Equal(4, ToolVars.OutputSlotCount(tool));
            tool.Scanner = "";
            tool.Count = 3;
            Assert.Equal(3, ToolVars.OutputSlotCount(tool));
            tool.Scanner = "左、、右";
            tool.Count = 1;
            Assert.Equal(2, ToolVars.OutputSlotCount(tool));
        }

        /// <summary>
        /// PLC 读取把长度和类型原样交出去。
        /// </summary>
        [Fact]
        public void PlcRead_ReturnsText()
        {
            var devices = new FakeDevices();
            devices.ReadResults.Enqueue(PlcIoResult.Ok("12"));
            var result = new PlcReadFlowTool("""{"Address":"D10","DataType":3,"Length":8}""").Execute(Context(devices: devices));
            Assert.Equal("12", result.Text);
            Assert.Equal(("D10", PlcDataType.String, 8), devices.Reads[0]);
        }

        /// <summary>
        /// 地址为空时不读 PLC。
        /// </summary>
        [Fact]
        public void PlcRead_EmptyAddress_Fails()
        {
            var devices = new FakeDevices();
            var result = new PlcReadFlowTool("""{"Address":""}""").Execute(Context(devices: devices));
            Assert.Equal("地址为空", result.Text);
            Assert.Empty(devices.Reads);
        }

        /// <summary>
        /// PLC 写入把配置里的值交出去。没有入参时仍用字面值。
        /// </summary>
        [Fact]
        public void PlcWrite_SendsValue()
        {
            var devices = new FakeDevices();
            var result = new PlcWriteFlowTool("""{"Address":"D10","DataType":0,"Value":"7"}""").Execute(Context(devices: devices));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal(("D10", PlcDataType.Int16, "7"), devices.Writes[0]);
        }

        /// <summary>
        /// 固定值不看来源变量。已声明的入参也不替换字面值。
        /// </summary>
        [Fact]
        public void PlcWrite_DeclaredInput_KeepsLiteral()
        {
            var devices = new FakeDevices();
            var variables = FlowVariables.FromSeeds([("工位状态", "3")]);
            var cfg = """{"Address":"D100","DataType":0,"Value":"1","Inputs":[{"Name":"写入值","Variable":"工位状态"}]}""";
            var result = new PlcWriteFlowTool(cfg).Execute(new FlowStepContext(CancellationToken.None, devices, variables));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal(("D100", PlcDataType.Int16, "1"), devices.Writes[0]);
        }

        /// <summary>
        /// 固定值不看入参。旧配置里的名称没声明时仍写字面值。
        /// </summary>
        [Fact]
        public void PlcWrite_Fixed_IgnoresUndeclaredInput()
        {
            var devices = new FakeDevices();
            var cfg = """{"Address":"D100","Value":"1","Inputs":[{"Name":"写入值","Variable":"工位状态"}]}""";
            var result = new PlcWriteFlowTool(cfg).Execute(Context(devices: devices));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal(("D100", PlcDataType.Int16, "1"), devices.Writes[0]);
        }

        /// <summary>
        /// 变量模式下写入该变量的当前文本。
        /// </summary>
        [Fact]
        public void PlcWrite_Variable_WritesCurrentText()
        {
            var devices = new FakeDevices();
            var variables = FlowVariables.FromSeeds([("工位状态", "3")]);
            var cfg = """{"Address":"D100","DataType":0,"Value":"工位状态","UseVariable":true}""";
            var result = new PlcWriteFlowTool(cfg).Execute(new FlowStepContext(CancellationToken.None, devices, variables));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal(("D100", PlcDataType.Int16, "3"), devices.Writes[0]);
        }

        /// <summary>
        /// 变量模式下来源没声明时不写 PLC。
        /// </summary>
        [Fact]
        public void PlcWrite_VariableMissing_DoesNotWrite()
        {
            var devices = new FakeDevices();
            var cfg = """{"Address":"D100","Value":"工位状态","UseVariable":true}""";
            var result = new PlcWriteFlowTool(cfg).Execute(Context(devices: devices));
            Assert.Equal("变量未声明", result.Text);
            Assert.Empty(devices.Writes);
        }

        /// <summary>
        /// 监控读到期望值后按重置值写回。
        /// </summary>
        [Fact]
        public void PlcMonitor_ResetWrites()
        {
            var devices = new FakeDevices();
            devices.ReadResults.Enqueue(PlcIoResult.Ok("1"));
            var cfg = """{"Address":"D1","DataType":0,"ExpectValue":"1","Reset":true,"ResetValue":"0","Interval":20}""";
            var result = new PlcMonitorFlowTool(cfg).Execute(Context(devices: devices));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal("1", result.Text);
            Assert.Equal(("D1", PlcDataType.Int16, "0"), devices.Writes[0]);
            Assert.Empty(devices.Waits);
        }

        /// <summary>
        /// 浮点期望值按数值比较，1 和 1.0 相等。
        /// </summary>
        [Fact]
        public void PlcMonitor_FloatMatch()
        {
            var devices = new FakeDevices();
            devices.ReadResults.Enqueue(PlcIoResult.Ok("1.0"));
            var cfg = """{"Address":"D1","DataType":2,"ExpectValue":"1","Reset":false,"Interval":20}""";
            var result = new PlcMonitorFlowTool(cfg).Execute(Context(devices: devices));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Empty(devices.Writes);
        }

        /// <summary>
        /// 还没读到期望值时，等待被取消就结束。
        /// </summary>
        [Fact]
        public void PlcMonitor_CancelDuringWait()
        {
            var devices = new FakeDevices();
            devices.ReadResults.Enqueue(PlcIoResult.Ok("0"));
            devices.WaitCompletes = false;
            var cfg = """{"Address":"D1","ExpectValue":"1","Interval":20}""";
            var result = new PlcMonitorFlowTool(cfg).Execute(Context(devices: devices));
            Assert.Equal(FlowStepStatus.Cancelled, result.Status);
            Assert.Empty(devices.Writes);
            Assert.Equal(20, devices.Waits[0]);
        }

        /// <summary>
        /// 监控不写出参。旧配置里的变量名不要求已声明，也不占用出参。
        /// </summary>
        [Fact]
        public void PlcMonitor_IgnoresOutput()
        {
            var devices = new FakeDevices();
            devices.ReadResults.Enqueue(PlcIoResult.Ok("1"));
            var cfg = """{"Address":"D1","ExpectValue":"1","Interval":20,"Output":"监控值"}""";
            var result = new PlcMonitorFlowTool(cfg).Execute(Context(devices: devices));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal("1", result.Text);
            Assert.False(ToolVars.HasInputs(FlowToolKind.PlcMonitor));
            Assert.False(ToolVars.HasOutput(FlowToolKind.PlcMonitor));
            Assert.Empty(ToolVars.OutputNames(FlowToolKind.PlcMonitor, cfg));
            Assert.Empty(ToolVars.Referenced(FlowToolKind.PlcMonitor, cfg));
        }

        /// <summary>
        /// 间隔无效时不读 PLC。
        /// </summary>
        [Fact]
        public void PlcMonitor_BadInterval_Fails()
        {
            var devices = new FakeDevices();
            var result = new PlcMonitorFlowTool("""{"Address":"D1","Interval":0}""").Execute(Context(devices: devices));
            Assert.Equal("监控间隔无效", result.Text);
            Assert.Empty(devices.Reads);
        }

        /// <summary>
        /// 脚本这一期不编译，成功时结果就是正文。
        /// </summary>
        [Fact]
        public void Script_ReturnsBody()
        {
            var result = new ScriptFlowTool("""{"Name":"记录","Script":"第一行\n第二行"}""").Execute(Context());
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal("第一行\n第二行", result.Text);
        }

        /// <summary>
        /// JSON 来源和结果都算已绑定。出参只看结果。
        /// </summary>
        [Fact]
        public void Json_SourceAndTarget_AreReferenced()
        {
            var cfg = """{"Source":"进站报文","Target":"解析值","Inputs":[{"Variable":"进站报文"}]}""";
            var names = ToolVars.Referenced(FlowToolKind.JsonParse, cfg).ToHashSet(StringComparer.Ordinal);
            Assert.Contains("进站报文", names);
            Assert.Contains("解析值", names);
            Assert.Equal(["解析值"], ToolVars.OutputNames(FlowToolKind.JsonParse, cfg).ToArray());
        }

        /// <summary>
        /// 做一个带假设备的执行参数。
        /// </summary>
        private static FlowStepContext Context(CancellationToken token = default, FakeDevices? devices = null)
        {
            return new FlowStepContext(token, devices ?? new FakeDevices());
        }
    }
}
