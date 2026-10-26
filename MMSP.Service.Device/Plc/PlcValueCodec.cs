using System.Globalization;
using HslCommunication;
using HslCommunication.Core;
using MMSP.Core.Model.Sys;

namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 把 PlcDataType 对应到 HSL 的读写方法。所有协议共用。
    /// </summary>
    internal static class PlcValueCodec
    {
        /// <summary>
        /// 读取一个点位。地址为空或类型不支持时不访问设备。
        /// </summary>
        public static PlcIoResult Read(IReadWriteNet device, string address, PlcDataType dataType, int length)
        {
            if (string.IsNullOrWhiteSpace(address))
                return PlcIoResult.Fail("地址为空");

            try
            {
                return dataType switch
                {
                    PlcDataType.Int16 => From(device.ReadInt16(address)),
                    PlcDataType.UInt16 => From(device.ReadUInt16(address)),
                    PlcDataType.Int32 => From(device.ReadInt32(address)),
                    PlcDataType.Float => From(device.ReadFloat(address)),
                    PlcDataType.String => From(device.ReadString(address, ToLength(length))),
                    PlcDataType.UInt16Array => FromUInt16Array(device.ReadUInt16(address, ToLength(length))),
                    _ => PlcIoResult.Fail("不支持的数据类型")
                };
            }
            catch (Exception ex)
            {
                return PlcIoResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 写入一个点位。文本和类型对不上时不发送报文。
        /// </summary>
        public static PlcIoResult Write(IReadWriteNet device, string address, PlcDataType dataType, string value)
        {
            if (string.IsNullOrWhiteSpace(address))
                return PlcIoResult.Fail("地址为空");

            var text = value ?? "";
            try
            {
                OperateResult result = dataType switch
                {
                    PlcDataType.Int16 when short.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var int16) => device.Write(address, int16),
                    PlcDataType.UInt16 when ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uint16) => device.Write(address, uint16),
                    PlcDataType.Int32 when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var int32) => device.Write(address, int32),
                    PlcDataType.Float when TryFloat(text, out var real) => device.Write(address, real),
                    PlcDataType.String => device.Write(address, text),
                    _ => new OperateResult("写入值与数据类型不匹配")
                };
                return From(result);
            }
            catch (Exception ex)
            {
                return PlcIoResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 字符串长度限制在 1 到 ushort 最大值。
        /// </summary>
        public static ushort ToLength(int length)
        {
            if (length <= 0)
                return 1;

            if (length > ushort.MaxValue)
                return ushort.MaxValue;

            return (ushort)length;
        }

        /// <summary>
        /// 把 HSL 结果收成文本结果。
        /// </summary>
        public static PlcIoResult From(OperateResult result)
        {
            return result.IsSuccess
                ? PlcIoResult.Ok("")
                : PlcIoResult.Fail(string.IsNullOrWhiteSpace(result.Message) ? "通讯失败" : result.Message);
        }

        /// <summary>
        /// 6 元按年/月/日/时/分/秒收成时间。其它长度保留逗号分隔的原值。
        /// </summary>
        private static PlcIoResult FromUInt16Array(OperateResult<ushort[]> result)
        {
            if (!result.IsSuccess)
                return PlcIoResult.Fail(string.IsNullOrWhiteSpace(result.Message) ? "通讯失败" : result.Message);

            var values = result.Content;
            if (values is null || values.Length == 0)
                return PlcIoResult.Fail("通讯失败");

            if (values.Length == 6)
            {
                return PlcTimeText.TryFormat(values, out var time)
                    ? PlcIoResult.Ok(time)
                    : PlcIoResult.Fail("时间无效");
            }

            return PlcIoResult.Ok(string.Join(",", values.Select(item => item.ToString(CultureInfo.InvariantCulture))));
        }

        /// <summary>
        /// 把读取值收成不变文化的文本。
        /// </summary>
        private static PlcIoResult From<T>(OperateResult<T> result)
        {
            if (!result.IsSuccess)
                return PlcIoResult.Fail(string.IsNullOrWhiteSpace(result.Message) ? "通讯失败" : result.Message);

            var text = result.Content switch
            {
                null => "",
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => result.Content.ToString() ?? ""
            };
            return PlcIoResult.Ok(text);
        }

        /// <summary>
        /// 先按不变文化解析浮点，再按当前文化。
        /// </summary>
        private static bool TryFloat(string text, out float value)
        {
            return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
        }
    }
}
