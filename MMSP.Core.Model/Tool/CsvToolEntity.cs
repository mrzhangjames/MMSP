using System.Text.Json.Serialization;
using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// CSV 导出的一列。列名写入表头，数据来源是流程变量。
    /// </summary>
    public class CsvColumn
    {
        /// <summary>
        /// 表头。运行时去掉首尾空白，本步内不能重复。
        /// </summary>
        public string Column { get; set; } = "";

        /// <summary>
        /// 取值的流程变量。空表示这一列还没选。
        /// </summary>
        public string Source { get; set; } = "";
    }

    /// <summary>
    /// CSV 工具。把每一列的变量值追加到当天文件，不写出参。
    /// </summary>
    public class CsvToolEntity : PluginToolModel
    {
        /// <summary>
        /// 文件名。按日期生成时这里只是打开窗口时的预览。
        /// </summary>
        public string FileName { get; set; } = "";

        /// <summary>
        /// 为 true 时运行当天用流程名加年月日，不使用 <see cref="FileName"/>。
        /// </summary>
        public bool NameFromDate { get; set; } = true;

        /// <summary>
        /// 导出列。顺序就是表头顺序。
        /// </summary>
        public List<CsvColumn> Rows { get; set; } = [];

        /// <summary>
        /// 本步读取的流程变量。与各列的数据来源保持一致。
        /// </summary>
        public List<ToolInput> Inputs { get; set; } = [];

        /// <summary>
        /// 正在编辑的行。不写入配置。没有编辑时为 -1。
        /// </summary>
        [JsonIgnore]
        public int EditingIndex { get; set; } = -1;
    }
}
