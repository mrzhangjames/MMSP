namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 标记类型适用的设备类型。可以标多个，子类不继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class DeviceTypeAttribute : Attribute
    {
        /// <summary>
        /// 标上一种设备类型。
        /// </summary>
        public DeviceTypeAttribute(DeviceType deviceType)
        {
            DeviceType = deviceType;
        }

        /// <summary>
        /// 适用的设备类型。
        /// </summary>
        public DeviceType DeviceType { get; }
    }
}
