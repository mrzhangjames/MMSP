namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// 设备页连接类型能选择的协议。只用 IP、端口、站号、字节序和超时就能创建的网口协议可以编辑。
    /// 串口、西门子 S7 和带槽号的 CIP 只显示已保存的名字，不放进常规下拉。
    /// </summary>
    public static class PlcLinkProtocols
    {
        /// <summary>
        /// 协议、中文名、设备页能否编辑。
        /// </summary>
        private static readonly Entry[] Entries =
        [
            new(PlcProtocol.ModbusTcp, "Modbus TCP", true),
            new(PlcProtocol.ModbusUdp, "Modbus UDP", true),
            new(PlcProtocol.ModbusRtu, "Modbus RTU 串口", false),
            new(PlcProtocol.ModbusRtuOverTcp, "Modbus RTU over TCP", true),
            new(PlcProtocol.ModbusAscii, "Modbus ASCII 串口", false),
            new(PlcProtocol.ModbusAsciiOverTcp, "Modbus ASCII over TCP", true),
            new(PlcProtocol.SiemensS7, "西门子 S7", false),
            new(PlcProtocol.SiemensFetchWrite, "西门子 Fetch/Write", true),
            new(PlcProtocol.SiemensPPI, "西门子 PPI 串口", false),
            new(PlcProtocol.SiemensPPIOverTcp, "西门子 PPI over TCP", true),
            new(PlcProtocol.SiemensMPI, "西门子 MPI 串口", false),
            new(PlcProtocol.MelsecMcNet, "三菱 MC 二进制", true),
            new(PlcProtocol.MelsecMcAsciiNet, "三菱 MC ASCII", true),
            new(PlcProtocol.MelsecMcUdp, "三菱 MC UDP", true),
            new(PlcProtocol.MelsecMcAsciiUdp, "三菱 MC ASCII UDP", true),
            new(PlcProtocol.MelsecMcRNet, "三菱 MC R 系列", true),
            new(PlcProtocol.MelsecA1ENet, "三菱 A-1E 二进制", true),
            new(PlcProtocol.MelsecA1EAsciiNet, "三菱 A-1E ASCII", true),
            new(PlcProtocol.MelsecFxSerial, "三菱 FX 串口", false),
            new(PlcProtocol.MelsecFxSerialOverTcp, "三菱 FX 串口 over TCP", true),
            new(PlcProtocol.MelsecFxLinks, "三菱 FX Links 串口", false),
            new(PlcProtocol.MelsecFxLinksOverTcp, "三菱 FX Links over TCP", true),
            new(PlcProtocol.MelsecA3CNet, "三菱 A-3C 串口", false),
            new(PlcProtocol.MelsecA3CNetOverTcp, "三菱 A-3C over TCP", true),
            new(PlcProtocol.MelsecA4CNet, "三菱 A-4C 串口", false),
            new(PlcProtocol.MelsecA4CNetOverTcp, "三菱 A-4C over TCP", true),
            new(PlcProtocol.MelsecCipNet, "三菱 CIP", false),
            new(PlcProtocol.OmronFinsNet, "欧姆龙 FINS TCP", true),
            new(PlcProtocol.OmronFinsUdp, "欧姆龙 FINS UDP", true),
            new(PlcProtocol.OmronHostLink, "欧姆龙 HostLink 串口", false),
            new(PlcProtocol.OmronHostLinkOverTcp, "欧姆龙 HostLink over TCP", true),
            new(PlcProtocol.OmronHostLinkCMode, "欧姆龙 HostLink C-Mode 串口", false),
            new(PlcProtocol.OmronHostLinkCModeOverTcp, "欧姆龙 HostLink C-Mode over TCP", true),
            new(PlcProtocol.OmronCipNet, "欧姆龙 CIP", false),
            new(PlcProtocol.OmronConnectedCipNet, "欧姆龙 Connected CIP", true),
            new(PlcProtocol.AllenBradleyNet, "罗克韦尔 CIP", false),
            new(PlcProtocol.AllenBradleyConnectedCipNet, "罗克韦尔 Connected CIP", true),
            new(PlcProtocol.AllenBradleyMicroCip, "罗克韦尔 Micro800 CIP", false),
            new(PlcProtocol.AllenBradleyPcccNet, "罗克韦尔 PCCC", true),
            new(PlcProtocol.AllenBradleySLCNet, "罗克韦尔 SLC", true),
            new(PlcProtocol.AllenBradleyDF1Serial, "罗克韦尔 DF1 串口", false),
            new(PlcProtocol.KeyenceMcNet, "基恩士 MC 二进制", true),
            new(PlcProtocol.KeyenceMcAsciiNet, "基恩士 MC ASCII", true),
            new(PlcProtocol.KeyenceNanoSerial, "基恩士 Nano 串口", false),
            new(PlcProtocol.KeyenceNanoSerialOverTcp, "基恩士 Nano over TCP", true),
            new(PlcProtocol.KeyenceKvOld, "基恩士 KV 旧协议", true),
            new(PlcProtocol.PanasonicMcNet, "松下 MC", true),
            new(PlcProtocol.PanasonicMewtocol, "松下 Mewtocol 串口", false),
            new(PlcProtocol.PanasonicMewtocolOverTcp, "松下 Mewtocol over TCP", true),
            new(PlcProtocol.DeltaTcp, "台达 Modbus TCP", true),
            new(PlcProtocol.DeltaSerial, "台达串口", false),
            new(PlcProtocol.DeltaSerialOverTcp, "台达串口 over TCP", true),
            new(PlcProtocol.DeltaSerialAscii, "台达 ASCII 串口", false),
            new(PlcProtocol.DeltaSerialAsciiOverTcp, "台达 ASCII over TCP", true),
            new(PlcProtocol.InovanceTcp, "汇川 Modbus TCP", true),
            new(PlcProtocol.InovanceSerial, "汇川串口", false),
            new(PlcProtocol.InovanceSerialOverTcp, "汇川串口 over TCP", true),
            new(PlcProtocol.InovanceComputerLink, "汇川 ComputerLink 串口", false),
            new(PlcProtocol.InovanceConnectedCip, "汇川 Connected CIP", true),
            new(PlcProtocol.InovanceEasyNet, "汇川 Easy", true),
            new(PlcProtocol.XinJETcp, "信捷 Modbus TCP", true),
            new(PlcProtocol.XinJESerial, "信捷串口", false),
            new(PlcProtocol.XinJESerialOverTcp, "信捷串口 over TCP", true),
            new(PlcProtocol.XinJEInternalNet, "信捷内部协议", true),
            new(PlcProtocol.FatekProgram, "永宏串口", false),
            new(PlcProtocol.FatekProgramOverTcp, "永宏 over TCP", true),
            new(PlcProtocol.FujiSPB, "富士 SPB 串口", false),
            new(PlcProtocol.FujiSPBOverTcp, "富士 SPB over TCP", true),
            new(PlcProtocol.FujiSPHNet, "富士 SPH", true),
            new(PlcProtocol.GeSRTPNet, "GE SRTP", true),
            new(PlcProtocol.BeckhoffAdsNet, "倍福 ADS", true),
            new(PlcProtocol.LSCnet, "LS Cnet 串口", false),
            new(PlcProtocol.LSCnetOverTcp, "LS Cnet over TCP", true),
            new(PlcProtocol.LSCpu, "LS Cpu 串口", false),
            new(PlcProtocol.LSFastEnet, "LS Fast Enet", true),
            new(PlcProtocol.MegMeetTcp, "麦格米特 Modbus TCP", true),
            new(PlcProtocol.MegMeetSerial, "麦格米特串口", false),
            new(PlcProtocol.MegMeetSerialOverTcp, "麦格米特串口 over TCP", true),
            new(PlcProtocol.VigorSerial, "丰炜串口", false),
            new(PlcProtocol.VigorSerialOverTcp, "丰炜 over TCP", true),
            new(PlcProtocol.YokogawaLinkTcp, "横河 Link TCP", true),
            new(PlcProtocol.MemobusTcp, "安川 Memobus TCP", true),
            new(PlcProtocol.MemobusUdp, "安川 Memobus UDP", true),
            new(PlcProtocol.DigitronCPL, "山武 DigitronCPL 串口", false),
            new(PlcProtocol.DigitronCPLOverTcp, "山武 DigitronCPL over TCP", true),
            new(PlcProtocol.ToyoPuc, "丰田 ToyoPuc", true),
            new(PlcProtocol.CimonHmiProtocol, "Cimon HMI 协议", true)
        ];

        /// <summary>
        /// 设备页下拉里的协议。顺序与登记表一致。
        /// </summary>
        public static IReadOnlyList<PlcProtocol> Editable { get; } = Entries.Where(item => item.Editable).Select(item => item.Protocol).ToArray();

        /// <summary>
        /// 当前表单的 IP、端口、站号、字节序和超时够不够创建这个协议。
        /// </summary>
        public static bool CanEdit(PlcProtocol protocol)
        {
            return Find(protocol)?.Editable == true;
        }

        /// <summary>
        /// 中文名。没有登记时用枚举名。
        /// </summary>
        public static string NameOf(PlcProtocol protocol)
        {
            return Find(protocol)?.Name ?? protocol.ToString();
        }

        /// <summary>
        /// 按中文名找回协议。
        /// </summary>
        public static bool TryParse(string? name, out PlcProtocol protocol)
        {
            var text = (name ?? "").Trim();
            foreach (var entry in Entries)
            {
                if (entry.Name == text)
                {
                    protocol = entry.Protocol;
                    return true;
                }
            }

            protocol = default;
            return false;
        }

        /// <summary>
        /// 按协议找登记。没有时为空。
        /// </summary>
        private static Entry? Find(PlcProtocol protocol)
        {
            foreach (var entry in Entries)
            {
                if (entry.Protocol == protocol)
                    return entry;
            }

            return null;
        }

        /// <summary>
        /// 一条协议登记。
        /// </summary>
        private readonly record struct Entry(PlcProtocol Protocol, string Name, bool Editable);
    }
}
