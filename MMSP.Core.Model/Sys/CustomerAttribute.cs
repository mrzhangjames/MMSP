namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 标记类型所属客户。一个类只能标一个，子类不继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomerAttribute : Attribute
    {
        /// <summary>
        /// 标上所属客户。
        /// </summary>
        public CustomerAttribute(Customer customer)
        {
            Customer = customer;
        }

        /// <summary>
        /// 所属客户。
        /// </summary>
        public Customer Customer { get; }
    }
}
