namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 标记工具界面对应的流程工具种类。一个类只能标一个，子类不继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class FlowToolAttribute : Attribute
    {
        /// <summary>
        /// 标上工具种类。
        /// </summary>
        public FlowToolAttribute(FlowToolKind kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// 对应的工具种类。
        /// </summary>
        public FlowToolKind Kind { get; }
    }
}
