namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// Json 解析。这一期不执行解析。
    /// </summary>
    public sealed class JsonParseFlowTool : FlowToolStep
    {
        /// <summary>
        /// 留下配置文本，等这一期之后再读。
        /// </summary>
        public JsonParseFlowTool(string cfg)
        {
            _ = cfg;
        }

        /// <summary>
        /// 直接失败。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            return Fail("未实现");
        }
    }
}
