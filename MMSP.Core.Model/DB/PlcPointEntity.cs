using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using SqlSugar;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// PLC 点位配置。写在客户库。当前值和勾选状态不入库。
    /// </summary>
    [SugarTable("biz_plc_points")]
    public class PlcPointEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 点位名称。最长 64。
        /// </summary>
        [SugarColumn(ColumnName = "name", Length = 64)]
        public string Name { get; set; } = "";

        /// <summary>
        /// 地址。最长 64，原样保存。
        /// </summary>
        [SugarColumn(ColumnName = "address", Length = 64)]
        public string Address { get; set; } = "";

        /// <summary>
        /// 数据类型。
        /// </summary>
        [SugarColumn(ColumnName = "data_type")]
        public PlcDataType DataType { get; set; }

        /// <summary>
        /// 长度。保存时要求是正整数。
        /// </summary>
        [SugarColumn(ColumnName = "data_length")]
        public int DataLength { get; set; }

        /// <summary>
        /// 读写方式。
        /// </summary>
        [SugarColumn(ColumnName = "access")]
        public PlcAccess Access { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        [SugarColumn(ColumnName = "is_valid")]
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// 列表从上到下的顺序。
        /// </summary>
        [SugarColumn(ColumnName = "sort_index")]
        public int SortIndex { get; set; }

        /// <summary>
        /// 点位类型。流程和监控可有多条。心跳、状态各自只能有一条。
        /// </summary>
        [SugarColumn(ColumnName = "point_kind", DefaultValue = "1")]
        public PlcPointKind PointKind { get; set; } = PlcPointKind.Flow;

        /// <summary>
        /// Bit 位。空表示整字读写。Int16、UInt16 为 0 到 15，Int32 为 0 到 31。
        /// </summary>
        [SugarColumn(ColumnName = "bit_index", IsNullable = true)]
        public int? BitIndex { get; set; }

        /// <summary>
        /// 点位描述。最长 256，允许空。
        /// </summary>
        [SugarColumn(ColumnName = "description", Length = DescriptionMaxLength, DefaultValue = "''")]
        public string Description { get; set; } = "";

        /// <summary>
        /// 描述最长字符数。
        /// </summary>
        public const int DescriptionMaxLength = 256;

        /// <summary>
        /// 收成已定义的点位类型。未定义时用流程。已取消的触发值 0 也归流程。
        /// </summary>
        public static PlcPointKind NormalizeKind(PlcPointKind kind)
        {
            return Enum.IsDefined(kind) ? kind : PlcPointKind.Flow;
        }

        /// <summary>
        /// 心跳或状态。这两类整表只能有一条。
        /// </summary>
        public static bool IsSingleton(PlcPointKind kind)
        {
            return kind is PlcPointKind.Heartbeat or PlcPointKind.Status;
        }

        /// <summary>
        /// 找出重复的心跳或状态。没有重复时为空。
        /// </summary>
        public static PlcPointKind? ExtraSingleton(IEnumerable<PlcPointKind> kinds)
        {
            var heartbeat = 0;
            var status = 0;
            foreach (var kind in kinds)
            {
                if (kind == PlcPointKind.Heartbeat && ++heartbeat > 1)
                    return PlcPointKind.Heartbeat;
                if (kind == PlcPointKind.Status && ++status > 1)
                    return PlcPointKind.Status;
            }

            return null;
        }

        /// <summary>
        /// 按数据类型和长度收成合法的 Bit 位。长度不是 1，或类型不是 16 位、32 位整数时为空。
        /// </summary>
        public static int? NormalizeBitIndex(PlcDataType dataType, int dataLength, int? bitIndex)
        {
            if (bitIndex is null || dataLength != 1)
                return null;

            var max = dataType switch
            {
                PlcDataType.Int16 or PlcDataType.UInt16 => 15,
                PlcDataType.Int32 => 31,
                _ => -1
            };
            if (bitIndex.Value < 0 || bitIndex.Value > max)
                return null;

            return bitIndex;
        }

        /// <summary>
        /// 去掉描述两端空白，并截到最长 256。
        /// </summary>
        public static string NormalizeDescription(string? description)
        {
            var text = (description ?? "").Trim();
            return text.Length <= DescriptionMaxLength ? text : text[..DescriptionMaxLength];
        }
    }
}
