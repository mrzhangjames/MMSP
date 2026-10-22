namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 流程变量的数据类型。文本与变量窗口上的四个值一致。
    /// </summary>
    public static class FlowVarType
    {
        /// <summary>
        /// 整数。
        /// </summary>
        public const string Int = "Int";

        /// <summary>
        /// 字符串。新增变量默认用它。
        /// </summary>
        public const string String = "string";

        /// <summary>
        /// 双精度浮点。
        /// </summary>
        public const string Double = "double";

        /// <summary>
        /// 布尔。
        /// </summary>
        public const string Bool = "bool";

        /// <summary>
        /// 界面下拉的全部取值，顺序固定。
        /// </summary>
        public static readonly IReadOnlyList<string> All = [Int, String, Double, Bool];

        /// <summary>
        /// 是否为四个取值之一。
        /// </summary>
        public static bool IsDefined(string? value)
        {
            return value is Int or String or Double or Bool;
        }
    }
}
