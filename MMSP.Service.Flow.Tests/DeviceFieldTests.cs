using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Device.Plc;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 设备页字段：可编辑协议、读写超时分开、Bit 取值和旧 CSV。
    /// </summary>
    public class DeviceFieldTests
    {
        /// <summary>
        /// 现有列就能连上的网口协议可以编辑。串口、S7 和带槽号的 CIP 不能。
        /// </summary>
        [Fact]
        public void PlcLinkProtocols_EditsOnlySocketProtocols()
        {
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.ModbusTcp));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.ModbusRtuOverTcp));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.SiemensFetchWrite));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.SiemensPPIOverTcp));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.MelsecMcNet));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.KeyenceMcNet));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.OmronFinsNet));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.PanasonicMcNet));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.PanasonicMewtocolOverTcp));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.DeltaTcp));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.InovanceTcp));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.XinJETcp));
            Assert.True(PlcLinkProtocols.CanEdit(PlcProtocol.MegMeetTcp));

            Assert.False(PlcLinkProtocols.CanEdit(PlcProtocol.ModbusRtu));
            Assert.False(PlcLinkProtocols.CanEdit(PlcProtocol.SiemensS7));
            Assert.False(PlcLinkProtocols.CanEdit(PlcProtocol.SiemensPPI));
            Assert.False(PlcLinkProtocols.CanEdit(PlcProtocol.MelsecCipNet));
            Assert.False(PlcLinkProtocols.CanEdit(PlcProtocol.OmronCipNet));
            Assert.False(PlcLinkProtocols.CanEdit(PlcProtocol.AllenBradleyNet));
            Assert.False(PlcLinkProtocols.CanEdit(PlcProtocol.AllenBradleyMicroCip));

            foreach (PlcProtocol protocol in Enum.GetValues<PlcProtocol>())
            {
                var name = PlcLinkProtocols.NameOf(protocol);
                Assert.True(PlcLinkProtocols.TryParse(name, out var parsed));
                Assert.Equal(protocol, parsed);
                Assert.Equal(PlcLinkProtocols.CanEdit(protocol), PlcLinkProtocols.Editable.Contains(protocol));
            }

            Assert.Equal("Modbus TCP", PlcLinkProtocols.NameOf(PlcProtocol.ModbusTcp));
            Assert.DoesNotContain(PlcLinkProtocols.Editable, item => item == PlcProtocol.SiemensS7);
        }

        /// <summary>
        /// 字符串按寄存器低字节在前。没有字节变换时跳过。
        /// </summary>
        [Fact]
        public void PlcDeviceSetup_PutsStringLowByteFirst()
        {
            var device = new StringOrderDevice();
            PlcDeviceSetup.UseStringOrder(device);
            Assert.True(device.ByteTransform.IsStringReverseByteWord);

            PlcDeviceSetup.UseStringOrder(new object());
        }

        private sealed class StringOrderDevice
        {
            public StringOrderTransform ByteTransform { get; } = new();
        }

        private sealed class StringOrderTransform
        {
            public bool IsStringReverseByteWord { get; set; }
        }

        /// <summary>
        /// 汇川 H5U 的 D 元件号等于保持寄存器号。其它写法不改。
        /// </summary>
        [Fact]
        public void PlcModbusAddress_StripsOnlyDWordPrefix()
        {
            Assert.Equal("5244", PlcModbusAddress.Normalize("D5244"));
            Assert.Equal("5003", PlcModbusAddress.Normalize("d5003"));
            Assert.Equal(" 5244", PlcModbusAddress.Normalize(" 5244"));
            Assert.Equal("D5003.0", PlcModbusAddress.Normalize("D5003.0"));
            Assert.Equal("M0", PlcModbusAddress.Normalize("M0"));
            Assert.Equal("R0", PlcModbusAddress.Normalize("R0"));
            Assert.Equal("  ", PlcModbusAddress.Normalize("  "));
            Assert.Equal("", PlcModbusAddress.Normalize(null));
        }

        /// <summary>
        /// 地址为空不访问设备。整数、浮点和字数组的文本对不上时不发送报文。
        /// </summary>
        [Fact]
        public void PlcValueCodec_RejectsEmptyAddressAndMismatchedWrite()
        {
            var empty = PlcValueCodec.Read(null!, "  ", PlcDataType.Int16, 1);
            Assert.False(empty.IsSuccess);
            Assert.Equal("地址为空", empty.Message);

            var unknown = PlcValueCodec.Read(null!, "D1", (PlcDataType)99, 1);
            Assert.False(unknown.IsSuccess);
            Assert.Equal("不支持的数据类型", unknown.Message);

            Assert.Equal("写入值与数据类型不匹配", PlcValueCodec.Write(null!, "D1", PlcDataType.Int16, "1.5").Message);
            Assert.Equal("写入值与数据类型不匹配", PlcValueCodec.Write(null!, "D1", PlcDataType.UInt16, "-1").Message);
            Assert.Equal("写入值与数据类型不匹配", PlcValueCodec.Write(null!, "D1", PlcDataType.Int32, "x").Message);
            Assert.Equal("写入值与数据类型不匹配", PlcValueCodec.Write(null!, "D1", PlcDataType.Float, "abc").Message);
            Assert.Equal("写入值与数据类型不匹配", PlcValueCodec.Write(null!, "D1", PlcDataType.UInt16Array, "1,2,3").Message);
            Assert.Equal("地址为空", PlcValueCodec.Write(null!, "", PlcDataType.String, "SN").Message);
        }

        /// <summary>
        /// 读取超时给连接和接收。写入超时单独保存，小于等于 0 时按 1000 毫秒。
        /// </summary>
        [Fact]
        public void PlcConnections_KeepsWriteTimeoutApartFromRead()
        {
            var link = PlcConnections.Create(PlcProtocol.ModbusTcp, "10.0.0.8", 502, 7, "CDAB", 1500, 2500);
            Assert.Equal(PlcProtocol.ModbusTcp, link.Protocol);
            Assert.Equal(7, (int)link.Station);
            Assert.Equal("CDAB", link.DataFormat);
            Assert.Equal(1500, link.ConnectTimeout);
            Assert.Equal(1500, link.ReceiveTimeout);
            Assert.Equal(2500, link.WriteTimeout);

            var fallback = PlcConnections.Create(PlcProtocol.ModbusUdp, "10.0.0.8", 502, 300, "BAD", 0, -5);
            Assert.Equal(1000, fallback.ConnectTimeout);
            Assert.Equal(1000, fallback.ReceiveTimeout);
            Assert.Equal(1000, fallback.WriteTimeout);
            Assert.Equal(255, (int)fallback.Station);
            Assert.Equal("ABCD", fallback.DataFormat);
        }

        /// <summary>
        /// 按位取出 0 或 1。高位按有符号或无符号宽度写回，避免再写入时越界。
        /// </summary>
        [Fact]
        public void PlcBit_ReadsAndAppliesOneBit()
        {
            Assert.True(PlcBit.TryRead("5", 0, out var bit0));
            Assert.Equal(1, bit0);
            Assert.True(PlcBit.TryRead("5", 1, out var bit1));
            Assert.Equal(0, bit1);
            Assert.True(PlcBit.TryRead("5", 2, out var bit2));
            Assert.Equal(1, bit2);
            Assert.False(PlcBit.TryRead("5", 32, out _));
            Assert.False(PlcBit.TryRead("abc", 0, out _));

            Assert.True(PlcBit.TryApply("0", PlcDataType.Int16, 15, "1", out var signed, out _));
            Assert.Equal("-32768", signed);
            Assert.True(PlcBit.TryRead(signed, 15, out var signedBit));
            Assert.Equal(1, signedBit);

            Assert.True(PlcBit.TryApply("0", PlcDataType.UInt16, 15, "1", out var unsigned, out _));
            Assert.Equal("32768", unsigned);
            Assert.True(PlcBit.TryRead(unsigned, 15, out var unsignedBit));
            Assert.Equal(1, unsignedBit);

            Assert.True(PlcBit.TryApply("0", PlcDataType.Int32, 31, "1", out var wide, out _));
            Assert.Equal("-2147483648", wide);
            Assert.True(PlcBit.TryRead(wide, 31, out var wideBit));
            Assert.Equal(1, wideBit);

            Assert.True(PlcBit.TryApply("7", PlcDataType.Int16, 0, "0", out var cleared, out _));
            Assert.Equal("6", cleared);

            Assert.False(PlcBit.TryApply("1", PlcDataType.Int16, 0, "2", out _, out var valueError));
            Assert.Equal("写入值只能是 0 或 1", valueError);
            Assert.False(PlcBit.TryApply("1", PlcDataType.Int16, 16, "1", out _, out var rangeError));
            Assert.Equal("Bit 超出范围", rangeError);
            Assert.False(PlcBit.TryApply("1", PlcDataType.Float, 0, "1", out _, out _));
        }

        /// <summary>
        /// 旧 CSV 不足九列时按流程、空 Bit、空描述。填了但非法则拒绝。
        /// </summary>
        [Fact]
        public void PlcPointColumns_DefaultsMissingTail()
        {
            var oldFile = new[] { "完成", "D100", "Int16", "1", "读写", "启用" };
            Assert.True(PlcPointColumns.TryReadTail(oldFile, PlcDataType.Int16, 1, out var kind, out var bit, out var description));
            Assert.Equal(PlcPointKind.Flow, kind);
            Assert.Null(bit);
            Assert.Equal("", description);

            var blankTail = new[] { "完成", "D100", "Int16", "1", "读写", "启用", "", "", "  工位  " };
            Assert.True(PlcPointColumns.TryReadTail(blankTail, PlcDataType.Int16, 1, out kind, out bit, out description));
            Assert.Equal(PlcPointKind.Flow, kind);
            Assert.Null(bit);
            Assert.Equal("工位", description);

            var full = new[] { "完成", "D100", "UInt16", "1", "读写", "启用", "监控", "3", "说明" };
            Assert.True(PlcPointColumns.TryReadTail(full, PlcDataType.UInt16, 1, out kind, out bit, out description));
            Assert.Equal(PlcPointKind.Monitor, kind);
            Assert.Equal(3, bit);
            Assert.Equal("说明", description);

            var badBit = new[] { "完成", "D100", "Int16", "1", "读写", "启用", "Flow", "16", "" };
            Assert.False(PlcPointColumns.TryReadTail(badBit, PlcDataType.Int16, 1, out _, out _, out _));
            var badKind = new[] { "完成", "D100", "Int16", "1", "读写", "启用", "其它", "", "" };
            Assert.False(PlcPointColumns.TryReadTail(badKind, PlcDataType.Int16, 1, out _, out _, out _));
        }

        /// <summary>
        /// 状态整字 0 到 4 显示中文说明。对不上时保留原文本。
        /// </summary>
        [Fact]
        public void PlcStatusText_FormatsKnownValues()
        {
            Assert.Equal("运行", PlcStatusText.Format("0"));
            Assert.Equal("暂停", PlcStatusText.Format("1"));
            Assert.Equal("急停", PlcStatusText.Format("2"));
            Assert.Equal("调试", PlcStatusText.Format("3"));
            Assert.Equal("手动", PlcStatusText.Format("4"));
            Assert.Equal("9", PlcStatusText.Format("9"));
            Assert.Equal("abc", PlcStatusText.Format("abc"));
        }

        /// <summary>
        /// 6 个 UInt16 按年、月、日、时、分、秒收成时间。两位年份加 2000。
        /// </summary>
        [Fact]
        public void PlcTimeText_FormatsSixWordsAsClock()
        {
            Assert.Equal(5, (int)PlcDataType.UInt16Array);
            Assert.True(PlcTimeText.TryFormat([2026, 10, 26, 8, 9, 3], out var text));
            Assert.Equal("2026-10-26 08:09:03", text);
            Assert.True(PlcTimeText.TryFormat([26, 10, 26, 8, 9, 3], out var shortYear));
            Assert.Equal("2026-10-26 08:09:03", shortYear);
            Assert.False(PlcTimeText.TryFormat([2026, 13, 1, 0, 0, 0], out _));
            Assert.False(PlcTimeText.TryFormat([2026, 2, 29, 0, 0, 0], out _));
            Assert.False(PlcTimeText.TryFormat([2026, 10, 26, 8, 9], out _));
        }

        /// <summary>
        /// Bit 从 1 写到 16 时收成 0 到 15。文件里已有 0 时，16 仍拒绝。
        /// </summary>
        [Fact]
        public void PlcPointImport_ShiftsOneBasedBits()
        {
            var header = new[] { "点位名称", "地址", "数据类型", "长度", "读写", "启用", "点位类型", "Bit", "描述" };
            var oneBased = PlcPointImport.Read(
            [
                header,
                ["报警监控", "D470", "UInt16", "1", "只读", "启用", "监控", "1", "取 Bit 1。"],
                ["报警监控", "D470", "UInt16", "1", "只读", "启用", "监控", "16", "取 Bit 16。"]
            ]);
            Assert.Equal(0, oneBased[0].BitIndex);
            Assert.Equal(15, oneBased[1].BitIndex);
            Assert.Equal("取 Bit 16。", oneBased[1].Description);
            Assert.Equal(PlcPointKind.Monitor, oneBased[1].Kind);
            Assert.True(oneBased[1].Enabled);

            var zeroBased = PlcPointImport.Read(
            [
                header,
                ["完成", "D100", "Int16", "1", "读写", "启用", "流程", "0", ""],
                ["完成", "D101", "Int16", "1", "读写", "启用", "流程", "15", ""]
            ]);
            Assert.Equal(0, zeroBased[0].BitIndex);
            Assert.Equal(15, zeroBased[1].BitIndex);

            Assert.Throws<InvalidDataException>(() => PlcPointImport.Read(
            [
                header,
                ["完成", "D100", "Int16", "1", "读写", "启用", "流程", "0", ""],
                ["完成", "D101", "Int16", "1", "读写", "启用", "流程", "16", ""]
            ]));
        }
    }
}
