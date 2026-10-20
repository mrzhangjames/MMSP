using MMSP.Core.Model.Sys;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// 登记表里没有的种类。执行时失败，不中断其它流程的创建。
    /// </summary>
    public sealed class UnsupportedFlowTool : FlowToolStep
    {
        /// <summary>
        /// 不认识的种类。
        /// </summary>
        private readonly FlowToolKind _kind;

        /// <summary>
        /// 记下种类，便于日志看出是哪一个。
        /// </summary>
        public UnsupportedFlowTool(FlowToolKind kind)
        {
            _kind = kind;
        }

        /// <summary>
        /// 直接失败。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            return Fail("未实现 " + _kind);
        }
    }
}
