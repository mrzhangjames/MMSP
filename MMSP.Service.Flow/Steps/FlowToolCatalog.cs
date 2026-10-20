using MMSP.Core.Model.Sys;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// 按工具种类创建子类。这里是唯一知道枚举和子类对应关系的地方。
    /// </summary>
    public static class FlowToolCatalog
    {
        /// <summary>
        /// 种类到构造函数。每种工具一行。
        /// </summary>
        private static readonly Dictionary<FlowToolKind, Func<string, FlowToolStep>> Map = new()
        {
            [FlowToolKind.SingleScan] = cfg => new SingleScanFlowTool(cfg),
            [FlowToolKind.MultiScan] = cfg => new MultiScanFlowTool(cfg),
            [FlowToolKind.PlcRead] = cfg => new PlcReadFlowTool(cfg),
            [FlowToolKind.PlcWrite] = cfg => new PlcWriteFlowTool(cfg),
            [FlowToolKind.PlcMonitor] = cfg => new PlcMonitorFlowTool(cfg),
            [FlowToolKind.Delay] = cfg => new DelayFlowTool(cfg),
            [FlowToolKind.JsonParse] = cfg => new JsonParseFlowTool(cfg),
            [FlowToolKind.Condition] = cfg => new ConditionFlowTool(cfg),
            [FlowToolKind.Script] = cfg => new ScriptFlowTool(cfg),
            [FlowToolKind.Lux_AlarmLogTool] = cfg => new LuxAlarmLogFlowTool(cfg)
        };

        /// <summary>
        /// 创建工具。不认识的种类返回未实现工具，不中断快照。
        /// </summary>
        public static FlowToolStep Create(FlowToolKind kind, string cfg)
        {
            if (Map.TryGetValue(kind, out var create))
                return create(cfg ?? "");

            return new UnsupportedFlowTool(kind);
        }
    }
}
