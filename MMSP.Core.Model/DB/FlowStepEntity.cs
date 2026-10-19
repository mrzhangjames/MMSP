using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 一个流程步。写在客户库。序号和说明由页面按顺序生成。
    /// </summary>
    [SugarTable("biz_flow_steps")]
    public class FlowStepEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 所属流程。
        /// </summary>
        [SugarColumn(ColumnName = "flow_id")]
        public Guid FlowId { get; set; }

        /// <summary>
        /// 执行顺序。从 1 开始，小的先执行。
        /// </summary>
        [SugarColumn(ColumnName = "sort_index")]
        public int SortIndex { get; set; }

        /// <summary>
        /// 工具种类。
        /// </summary>
        [SugarColumn(ColumnName = "tool_kind")]
        public FlowToolKind ToolKind { get; set; }

        /// <summary>
        /// 标题。最长 32。
        /// </summary>
        [SugarColumn(ColumnName = "title", Length = 32)]
        public string Title { get; set; } = "";

        /// <summary>
        /// 扫码枪名、PLC 地址或变量名。最长 64。
        /// </summary>
        [SugarColumn(ColumnName = "target", Length = 64)]
        public string Target { get; set; } = "";

        /// <summary>
        /// 数据类型。只有 PLC 读取和 PLC 写入使用。
        /// </summary>
        [SugarColumn(ColumnName = "data_type")]
        public PlcDataType DataType { get; set; }

        /// <summary>
        /// 读取长度。只有 PLC 读取使用。
        /// </summary>
        [SugarColumn(ColumnName = "data_length")]
        public int DataLength { get; set; }

        /// <summary>
        /// PLC 写入值或变量赋值。最长 64。
        /// </summary>
        [SugarColumn(ColumnName = "write_value", Length = 64)]
        public string WriteValue { get; set; } = "";

        /// <summary>
        /// 延时毫秒。
        /// </summary>
        [SugarColumn(ColumnName = "number_value")]
        public int NumberValue { get; set; }

        /// <summary>
        /// 触发字符串、条件、日志或脚本摘要。最长 256。
        /// </summary>
        [SugarColumn(ColumnName = "text_value", Length = 256)]
        public string TextValue { get; set; } = "";

        /// <summary>
        /// 工具配置。Json 文本，由工具种类解释。
        /// </summary>
        [SugarColumn(ColumnName = "cfg_value", ColumnDataType = "text")]
        public string CfgValue { get; set; } = "";
    }
}
