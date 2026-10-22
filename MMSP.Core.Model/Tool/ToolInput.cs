namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 本步读取的一个流程变量。运行时只认变量名。
    /// </summary>
    public class ToolInput
    {
        /// <summary>
        /// 旧配置里的槽位名。不参与点选，留着是为了旧报文还能读出来。
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 本步读取的流程变量。空表示这条不参与。
        /// </summary>
        public string Variable { get; set; } = "";
    }
}
