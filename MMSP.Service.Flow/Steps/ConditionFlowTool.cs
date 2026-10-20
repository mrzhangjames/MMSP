namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// 条件判断。这一期不计算表达式。
    /// </summary>
    public sealed class ConditionFlowTool : FlowToolStep
    {
        /// <summary>
        /// 留下配置文本，等这一期之后再读。
        /// </summary>
        public ConditionFlowTool(string cfg)
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
