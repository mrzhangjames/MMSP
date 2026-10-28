using System.Globalization;
using HslCommunication;
using HslCommunication.Core;
using MMSP.Core.Model.Sys;

namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 把 PlcDataType 对应到 HSL 的读写方法。所有协议共用。
    /// 整数、浮点、字符串、字数组各自封装。
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

            return dataType switch
            {
                PlcDataType.Int16 or PlcDataType.UInt16 or PlcDataType.Int32 => Integer.Read(device, address, dataType),
                PlcDataType.Float => Real.Read(device, address),
                PlcDataType.String => Text.Read(device, address, length),
                PlcDataType.UInt16Array => Words.Read(device, address, length),
                _ => PlcIoResult.Fail("不支持的数据类型")
            };
        }

        /// <summary>
        /// 写入一个点位。文本和类型对不上时不发送报文。
        /// </summary>
        public static PlcIoResult Write(IReadWriteNet device, string address, PlcDataType dataType, string value)
        {
            if (string.IsNullOrWhiteSpace(address))
                return PlcIoResult.Fail("地址为空");

            var text = value ?? "";
            return dataType switch
            {
                PlcDataType.Int16 or PlcDataType.UInt16 or PlcDataType.Int32 => Integer.Write(device, address, dataType, text),
                PlcDataType.Float => Real.Write(device, address, text),
                PlcDataType.String => Text.Write(device, address, text),
                PlcDataType.UInt16Array => Words.Write(),
                _ => Mismatch()
            };
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
        /// 写入值和数据类型对不上。
        /// </summary>
        private static PlcIoResult Mismatch()
        {
            return PlcIoResult.Fail("写入值与数据类型不匹配");
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
        /// Int16、UInt16、Int32。
        /// </summary>
        private static class Integer
        {
            /// <summary>
            /// 按整数宽度读取一个字或双字。
            /// </summary>
            public static PlcIoResult Read(IReadWriteNet device, string address, PlcDataType dataType)
            {
                return dataType switch
                {
                    PlcDataType.Int16 => PlcValueCodec.From(device.ReadInt16(address)),
                    PlcDataType.UInt16 => PlcValueCodec.From(device.ReadUInt16(address)),
                    PlcDataType.Int32 => PlcValueCodec.From(device.ReadInt32(address)),
                    _ => PlcIoResult.Fail("不支持的数据类型")
                };
            }

            /// <summary>
            /// 文本必须是不变文化的整数，否则不发送报文。
            /// </summary>
            public static PlcIoResult Write(IReadWriteNet device, string address, PlcDataType dataType, string text)
            {
                OperateResult result = dataType switch
                {
                    PlcDataType.Int16 when short.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var int16) => device.Write(address, int16),
                    PlcDataType.UInt16 when ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uint16) => device.Write(address, uint16),
                    PlcDataType.Int32 when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var int32) => device.Write(address, int32),
                    _ => new OperateResult("写入值与数据类型不匹配")
                };
                return PlcValueCodec.From(result);
            }
        }

        /// <summary>
        /// Float。长度不参与读取。
        /// </summary>
        private static class Real
        {
            /// <summary>
            /// 读取一个单精度浮点。
            /// </summary>
            public static PlcIoResult Read(IReadWriteNet device, string address)
            {
                return PlcValueCodec.From(device.ReadFloat(address));
            }

            /// <summary>
            /// 先按不变文化解析，再按当前文化。解析失败时不发送报文。
            /// </summary>
            public static PlcIoResult Write(IReadWriteNet device, string address, string text)
            {
                if (!TryFloat(text, out var value))
                    return Mismatch();

                return PlcValueCodec.From(device.Write(address, value));
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

        /// <summary>
        /// String。长度是字符数。
        /// </summary>
        private static class Text
        {
            /// <summary>
            /// 按长度读取字符串。
            /// </summary>
            public static PlcIoResult Read(IReadWriteNet device, string address, int length)
            {
                return PlcValueCodec.From(device.ReadString(address, ToLength(length)));
            }

            /// <summary>
            /// 按原文写入字符串。
            /// </summary>
            public static PlcIoResult Write(IReadWriteNet device, string address, string text)
            {
                return PlcValueCodec.From(device.Write(address, text));
            }
        }

        /// <summary>
        /// UInt16Array。6 个字收成时间，其它长度保留逗号分隔的原值。写入未实现。
        /// </summary>
        private static class Words
        {
            /// <summary>
            /// 读取一组无符号字。
            /// </summary>
            public static PlcIoResult Read(IReadWriteNet device, string address, int length)
            {
                return FromArray(device.ReadUInt16(address, ToLength(length)));
            }

            /// <summary>
            /// 字数组写入还没有对应报文。
            /// </summary>
            public static PlcIoResult Write()
            {
                return Mismatch();
            }

            /// <summary>
            /// 6 元按年/月/日/时/分/秒收成时间。其它长度保留逗号分隔的原值。
            /// </summary>
            private static PlcIoResult FromArray(OperateResult<ushort[]> result)
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
        }
    }
}
