namespace MMSP.Plugin.Lux.WMI
{
    /// <summary>
    /// 一个 WMI 方法。字段名来自说明文档里的请求示例，调用时仍以传入的字典为准。
    /// </summary>
    public sealed class LuxWmiMethod
    {
        /// <summary>
        /// 记下方法名、说明、状态、文档字段，以及 methodparameter 是否要写成转义字符串。
        /// </summary>
        public LuxWmiMethod(string name, string remark, string status, string[] fields, bool parameterAsString)
        {
            Name = name;
            Remark = remark;
            Status = status;
            Fields = fields;
            ParameterAsString = parameterAsString;
        }

        /// <summary>
        /// 报文里的 method。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 目录上的说明。
        /// </summary>
        public string Remark { get; }

        /// <summary>
        /// 已取消、未启用、已停产、孟加拉国内不用，或仅 5.0.3 有。空表示文档没有这种标记。
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// 文档示例里的 methodparameter 字段名。没有示例时为空。
        /// </summary>
        public IReadOnlyList<string> Fields { get; }

        /// <summary>
        /// 为 true 时，methodparameter 写成 JSON 字符串，而不是 JSON 对象。
        /// </summary>
        public bool ParameterAsString { get; }
    }
}
