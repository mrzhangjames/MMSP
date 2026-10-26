using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 状态点整字对应的中文说明。对不上时保留原文本。
    /// </summary>
    public static class PlcStatusText
    {
        /// <summary>
        /// 0 到 4 显示运行、暂停、急停、调试、手动。
        /// </summary>
        public static string Format(string? raw)
        {
            var text = (raw ?? "").Trim();
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
                return raw ?? "";

            if (!Enum.IsDefined(typeof(PlcStatusValue), number))
                return raw ?? "";

            var value = (PlcStatusValue)number;
            var field = typeof(PlcStatusValue).GetField(value.ToString());
            var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
            return string.IsNullOrEmpty(description) ? text : description;
        }
    }
}
