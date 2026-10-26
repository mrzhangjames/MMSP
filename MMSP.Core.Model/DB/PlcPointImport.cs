using System.Globalization;
using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 点位 CSV 的一行。页面再换成自己的显示文字。
    /// </summary>
    public sealed class PlcPointImportRow
    {
        /// <summary>
        /// 点位名称。
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// 地址。
        /// </summary>
        public string Address { get; init; } = "";

        /// <summary>
        /// 数据类型原文。
        /// </summary>
        public string DataTypeText { get; init; } = "";

        /// <summary>
        /// 数据类型。
        /// </summary>
        public PlcDataType DataType { get; init; }

        /// <summary>
        /// 长度原文。
        /// </summary>
        public string LengthText { get; init; } = "";

        /// <summary>
        /// 长度。
        /// </summary>
        public int Length { get; init; }

        /// <summary>
        /// 读写原文。
        /// </summary>
        public string AccessText { get; init; } = "";

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool Enabled { get; init; }

        /// <summary>
        /// 点位类型。
        /// </summary>
        public PlcPointKind Kind { get; init; }

        /// <summary>
        /// Bit 位。空表示整字。16 位类型按 0 到 15 保存。
        /// </summary>
        public int? BitIndex { get; init; }

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; init; } = "";
    }

    /// <summary>
    /// 读点位 CSV。第一行长度不是正整数时当作表头跳过。
    /// </summary>
    public static class PlcPointImport
    {
        /// <summary>
        /// 读出点位。空文件、缺列、类型或启用文字无法识别时失败。
        /// 16 位或 32 位出现位宽本身、且文件里没有 Bit 0 时，按 1 起始收成 0 起始。
        /// </summary>
        public static IReadOnlyList<PlcPointImportRow> Read(IReadOnlyList<string[]> table)
        {
            if (table.Count == 0)
                throw new InvalidDataException();

            var start = 0;
            if (table[0].Length < 4 || !int.TryParse(table[0][3].Trim(), out var headerLength) || headerLength <= 0)
                start = 1;

            var origin = BitOrigin(table, start);
            var rows = new List<PlcPointImportRow>();
            for (var index = start; index < table.Count; index++)
            {
                var cells = table[index];
                if (cells.Length == 1 && cells[0].Trim().Length == 0)
                    continue;

                if (cells.Length < 6)
                    throw new InvalidDataException();

                var name = cells[0].Trim();
                var address = cells[1].Trim();
                var dataType = cells[2].Trim();
                var length = cells[3].Trim();
                if (name.Length == 0 || address.Length == 0 || !Enum.TryParse<PlcDataType>(dataType, out var parsedType) || !Enum.IsDefined(parsedType))
                    throw new InvalidDataException();

                if (!int.TryParse(length, out var lengthValue) || lengthValue <= 0)
                    throw new InvalidDataException();

                if (!PlcPointColumns.TryReadTail(ShiftBit(cells, origin), parsedType, lengthValue, out var kind, out var bitIndex, out var description))
                    throw new InvalidDataException();

                rows.Add(new PlcPointImportRow
                {
                    Name = name,
                    Address = address,
                    DataTypeText = dataType,
                    DataType = parsedType,
                    LengthText = length,
                    Length = lengthValue,
                    AccessText = cells[4],
                    Enabled = ParseEnabled(cells[5]),
                    Kind = kind,
                    BitIndex = bitIndex,
                    Description = description
                });
            }

            return rows;
        }

        /// <summary>
        /// 出现 16 或 32、且没有 0 时，Bit 从 1 计。否则从 0 计。
        /// </summary>
        private static int BitOrigin(IReadOnlyList<string[]> table, int start)
        {
            var sawZero = false;
            var sawWidth = false;
            for (var index = start; index < table.Count; index++)
            {
                var cells = table[index];
                if (cells.Length < 8 || cells[7].Trim().Length == 0)
                    continue;

                if (!int.TryParse(cells[7].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var bit))
                    continue;

                if (!Enum.TryParse(cells[2].Trim(), out PlcDataType dataType) || !Enum.IsDefined(dataType))
                    continue;

                var width = dataType switch
                {
                    PlcDataType.Int16 or PlcDataType.UInt16 => 16,
                    PlcDataType.Int32 => 32,
                    _ => 0
                };
                if (width == 0)
                    continue;

                if (bit == 0)
                    sawZero = true;
                if (bit == width)
                    sawWidth = true;
            }

            return sawWidth && !sawZero ? 1 : 0;
        }

        /// <summary>
        /// 把 1 起始的 Bit 减成 0 起始。空位和减数为 0 时不改原行。
        /// </summary>
        private static IReadOnlyList<string> ShiftBit(IReadOnlyList<string> cells, int origin)
        {
            if (origin == 0 || cells.Count < 8 || cells[7].Trim().Length == 0)
                return cells;

            if (!int.TryParse(cells[7].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var bit))
                return cells;

            var copy = cells.ToArray();
            copy[7] = (bit - origin).ToString(CultureInfo.InvariantCulture);
            return copy;
        }

        /// <summary>
        /// 启用文字。认不出时整份文件视为无效。
        /// </summary>
        private static bool ParseEnabled(string text)
        {
            var value = text.Trim();
            if (value.Equals("启用", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Enabled", StringComparison.OrdinalIgnoreCase)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("1", StringComparison.OrdinalIgnoreCase)
                || value.Equals("是", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.Equals("停用", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Disabled", StringComparison.OrdinalIgnoreCase)
                || value.Equals("false", StringComparison.OrdinalIgnoreCase)
                || value.Equals("0", StringComparison.OrdinalIgnoreCase)
                || value.Equals("否", StringComparison.OrdinalIgnoreCase)
                || value.Equals("no", StringComparison.OrdinalIgnoreCase))
                return false;

            throw new InvalidDataException();
        }
    }
}
