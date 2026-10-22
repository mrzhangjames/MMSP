using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 流程工具里可选的一个 PLC 点位。地址仍写在步骤配置里，这里只供下拉。
    /// </summary>
    public sealed class PlcPointChoice
    {
        /// <summary>
        /// 点位名称。可以空。
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// PLC 地址。
        /// </summary>
        public string Address { get; init; } = "";

        /// <summary>
        /// 点位上的数据类型。
        /// </summary>
        public PlcDataType DataType { get; init; }

        /// <summary>
        /// 点位上的长度。
        /// </summary>
        public int Length { get; init; }

        /// <summary>
        /// 点位类型。默认流程。
        /// </summary>
        public PlcPointKind Kind { get; init; } = PlcPointKind.Flow;

        /// <summary>
        /// 下拉里的文字。有名称时名称和地址都显示。
        /// </summary>
        public string Text
        {
            get
            {
                var address = (Address ?? "").Trim();
                var name = (Name ?? "").Trim();
                if (name.Length == 0)
                    return address;

                return name + "  " + address;
            }
        }
    }

    /// <summary>
    /// 从点位列表里按地址找下拉项。同一地址有多条时，先看数据类型，再优先流程。
    /// </summary>
    public static class PlcPointChoices
    {
        /// <summary>
        /// 对上已保存的地址。对不上时为空。
        /// </summary>
        public static PlcPointChoice? Match(IReadOnlyList<PlcPointChoice>? points, string? address, PlcDataType dataType)
        {
            var text = (address ?? "").Trim();
            if (text.Length == 0 || points is null)
                return null;

            var same = new List<PlcPointChoice>();
            foreach (var point in points)
            {
                if (string.Equals((point?.Address ?? "").Trim(), text, StringComparison.Ordinal))
                    same.Add(point!);
            }

            if (same.Count == 0)
                return null;

            var typed = new List<PlcPointChoice>();
            foreach (var point in same)
            {
                if (point.DataType == dataType)
                    typed.Add(point);
            }

            var pool = typed.Count > 0 ? typed : same;
            foreach (var point in pool)
            {
                if (point.Kind == PlcPointKind.Flow)
                    return point;
            }

            return pool[0];
        }

        /// <summary>
        /// 打开窗口时的数据源。对不上地址时用流程。
        /// </summary>
        public static PlcPointKind SourceKind(IReadOnlyList<PlcPointChoice>? points, string? address, PlcDataType dataType)
        {
            return Match(points, address, dataType)?.Kind ?? PlcPointKind.Flow;
        }

        /// <summary>
        /// 一种点位类型下、地址不为空的项。顺序与传入列表相同。
        /// </summary>
        public static IReadOnlyList<PlcPointChoice> OfKind(IReadOnlyList<PlcPointChoice>? points, PlcPointKind kind)
        {
            var found = new List<PlcPointChoice>();
            if (points is null)
                return found;

            foreach (var point in points)
            {
                if (point is null || point.Kind != kind)
                    continue;

                if (string.IsNullOrWhiteSpace(point.Address))
                    continue;

                found.Add(point);
            }

            return found;
        }
    }
}
