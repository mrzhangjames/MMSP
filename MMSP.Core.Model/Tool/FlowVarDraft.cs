using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 变量窗口里的一行草稿。保存前用它校验，不直接写库。
    /// </summary>
    public sealed class FlowVarDraft
    {
        /// <summary>
        /// 打开窗口时的名称。新行是空。用来判断改名和删除。
        /// </summary>
        public string OriginalName { get; init; } = "";

        /// <summary>
        /// 当前名称。
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// 数据类型。
        /// </summary>
        public string DataType { get; init; } = FlowVarType.String;

        /// <summary>
        /// 初始文本。
        /// </summary>
        public string InitialValue { get; init; } = "";
    }
}
