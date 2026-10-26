using System.Globalization;
using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 从整字文本取出或写回一个 Bit。不访问设备。
    /// </summary>
    public static class PlcBit
    {
        /// <summary>
        /// 取出 0 或 1。整字解析失败，或位号不在 0 到 31 时失败。
        /// </summary>
        public static bool TryRead(string? word, int bitIndex, out int bit)
        {
            bit = 0;
            if (bitIndex < 0 || bitIndex > 31)
                return false;

            if (!long.TryParse((word ?? "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                return false;

            bit = (int)((value >> bitIndex) & 1L);
            return true;
        }

        /// <summary>
        /// 把 0 或 1 写回整字文本。其它写入值不改原文本。
        /// </summary>
        public static bool TryApply(string? word, PlcDataType dataType, int bitIndex, string? bitText, out string updated, out string error)
        {
            updated = "";
            error = "";
            var width = Width(dataType);
            if (width == 0 || bitIndex < 0 || bitIndex >= width)
            {
                error = "Bit 超出范围";
                return false;
            }

            var text = (bitText ?? "").Trim();
            if (text is not ("0" or "1"))
            {
                error = "写入值只能是 0 或 1";
                return false;
            }

            if (!long.TryParse((word ?? "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                error = "Bit 无法读取";
                return false;
            }

            var mask = 1L << bitIndex;
            if (text == "1")
                value |= mask;
            else
                value &= ~mask;

            updated = dataType switch
            {
                PlcDataType.Int16 => unchecked((short)value).ToString(CultureInfo.InvariantCulture),
                PlcDataType.UInt16 => (value & 0xFFFFL).ToString(CultureInfo.InvariantCulture),
                _ => unchecked((int)value).ToString(CultureInfo.InvariantCulture)
            };
            return true;
        }

        /// <summary>
        /// 16 位或 32 位整数的位宽。其它类型不能按位写。
        /// </summary>
        private static int Width(PlcDataType dataType)
        {
            return dataType switch
            {
                PlcDataType.Int16 or PlcDataType.UInt16 => 16,
                PlcDataType.Int32 => 32,
                _ => 0
            };
        }
    }
}
