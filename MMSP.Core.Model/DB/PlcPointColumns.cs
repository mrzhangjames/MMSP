using System.Globalization;
using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 点位 CSV 末尾的类型、Bit 和描述。前六列仍由页面解释。
    /// </summary>
    public static class PlcPointColumns
    {
        /// <summary>
        /// 不足九列时类型为流程，Bit 和描述为空。填了但无法识别时返回 false。
        /// </summary>
        public static bool TryReadTail(
            IReadOnlyList<string> cells,
            PlcDataType dataType,
            int length,
            out PlcPointKind kind,
            out int? bitIndex,
            out string description)
        {
            kind = PlcPointKind.Flow;
            bitIndex = null;
            description = "";
            if (cells.Count >= 7 && cells[6].Trim().Length > 0 && !TryParseKind(cells[6], out kind))
                return false;

            if (cells.Count >= 8 && cells[7].Trim().Length > 0)
            {
                if (!int.TryParse(cells[7].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                    return false;

                bitIndex = PlcPointEntity.NormalizeBitIndex(dataType, length, index);
                if (bitIndex is null)
                    return false;
            }

            if (cells.Count >= 9)
                description = PlcPointEntity.NormalizeDescription(cells[8]);

            return true;
        }

        /// <summary>
        /// 认中文、英文和枚举名。空文本不是合法类型。
        /// </summary>
        public static bool TryParseKind(string? text, out PlcPointKind kind)
        {
            switch ((text ?? "").Trim())
            {
                case "流程":
                case "Flow":
                    kind = PlcPointKind.Flow;
                    return true;
                case "监控":
                case "Monitor":
                    kind = PlcPointKind.Monitor;
                    return true;
                case "心跳":
                case "Heartbeat":
                    kind = PlcPointKind.Heartbeat;
                    return true;
                case "状态":
                case "Status":
                    kind = PlcPointKind.Status;
                    return true;
                default:
                    kind = PlcPointKind.Flow;
                    return Enum.TryParse(text?.Trim(), out kind) && Enum.IsDefined(kind);
            }
        }
    }
}
