namespace MMSP.Service.Device.Plc
{
    /// <summary>
    /// HSL 13.0.2 中的 PLC 客户端协议。每个值对应一台客户端，不含服务器和辅助类。
    /// </summary>
    public enum PlcProtocol
    {
        /// <summary>Modbus TCP。</summary>
        ModbusTcp,

        /// <summary>Modbus UDP。</summary>
        ModbusUdp,

        /// <summary>Modbus RTU 串口。</summary>
        ModbusRtu,

        /// <summary>Modbus RTU over TCP。</summary>
        ModbusRtuOverTcp,

        /// <summary>Modbus ASCII 串口。</summary>
        ModbusAscii,

        /// <summary>Modbus ASCII over TCP。</summary>
        ModbusAsciiOverTcp,

        /// <summary>西门子 S7。</summary>
        SiemensS7,

        /// <summary>西门子 Fetch/Write。</summary>
        SiemensFetchWrite,

        /// <summary>西门子 PPI 串口。</summary>
        SiemensPPI,

        /// <summary>西门子 PPI over TCP。</summary>
        SiemensPPIOverTcp,

        /// <summary>西门子 MPI 串口。</summary>
        SiemensMPI,

        /// <summary>三菱 MC 二进制。</summary>
        MelsecMcNet,

        /// <summary>三菱 MC ASCII。</summary>
        MelsecMcAsciiNet,

        /// <summary>三菱 MC UDP。</summary>
        MelsecMcUdp,

        /// <summary>三菱 MC ASCII UDP。</summary>
        MelsecMcAsciiUdp,

        /// <summary>三菱 MC R 系列。</summary>
        MelsecMcRNet,

        /// <summary>三菱 A-1E 二进制。</summary>
        MelsecA1ENet,

        /// <summary>三菱 A-1E ASCII。</summary>
        MelsecA1EAsciiNet,

        /// <summary>三菱 FX 串口。</summary>
        MelsecFxSerial,

        /// <summary>三菱 FX 串口 over TCP。</summary>
        MelsecFxSerialOverTcp,

        /// <summary>三菱 FX Links 串口。</summary>
        MelsecFxLinks,

        /// <summary>三菱 FX Links over TCP。</summary>
        MelsecFxLinksOverTcp,

        /// <summary>三菱 A-3C 串口。</summary>
        MelsecA3CNet,

        /// <summary>三菱 A-3C over TCP。</summary>
        MelsecA3CNetOverTcp,

        /// <summary>三菱 A-4C 串口。</summary>
        MelsecA4CNet,

        /// <summary>三菱 A-4C over TCP。</summary>
        MelsecA4CNetOverTcp,

        /// <summary>三菱 CIP。</summary>
        MelsecCipNet,

        /// <summary>欧姆龙 FINS TCP。</summary>
        OmronFinsNet,

        /// <summary>欧姆龙 FINS UDP。</summary>
        OmronFinsUdp,

        /// <summary>欧姆龙 HostLink 串口。</summary>
        OmronHostLink,

        /// <summary>欧姆龙 HostLink over TCP。</summary>
        OmronHostLinkOverTcp,

        /// <summary>欧姆龙 HostLink C-Mode 串口。</summary>
        OmronHostLinkCMode,

        /// <summary>欧姆龙 HostLink C-Mode over TCP。</summary>
        OmronHostLinkCModeOverTcp,

        /// <summary>欧姆龙 CIP。</summary>
        OmronCipNet,

        /// <summary>欧姆龙 Connected CIP。</summary>
        OmronConnectedCipNet,

        /// <summary>罗克韦尔 CIP。</summary>
        AllenBradleyNet,

        /// <summary>罗克韦尔 Connected CIP。</summary>
        AllenBradleyConnectedCipNet,

        /// <summary>罗克韦尔 Micro800 CIP。</summary>
        AllenBradleyMicroCip,

        /// <summary>罗克韦尔 PCCC。</summary>
        AllenBradleyPcccNet,

        /// <summary>罗克韦尔 SLC。</summary>
        AllenBradleySLCNet,

        /// <summary>罗克韦尔 DF1 串口。</summary>
        AllenBradleyDF1Serial,

        /// <summary>基恩士 MC 二进制。</summary>
        KeyenceMcNet,

        /// <summary>基恩士 MC ASCII。</summary>
        KeyenceMcAsciiNet,

        /// <summary>基恩士 Nano 串口。</summary>
        KeyenceNanoSerial,

        /// <summary>基恩士 Nano over TCP。</summary>
        KeyenceNanoSerialOverTcp,

        /// <summary>基恩士 KV 旧协议。</summary>
        KeyenceKvOld,

        /// <summary>松下 MC。</summary>
        PanasonicMcNet,

        /// <summary>松下 Mewtocol 串口。</summary>
        PanasonicMewtocol,

        /// <summary>松下 Mewtocol over TCP。</summary>
        PanasonicMewtocolOverTcp,

        /// <summary>台达 Modbus TCP。</summary>
        DeltaTcp,

        /// <summary>台达串口。</summary>
        DeltaSerial,

        /// <summary>台达串口 over TCP。</summary>
        DeltaSerialOverTcp,

        /// <summary>台达 ASCII 串口。</summary>
        DeltaSerialAscii,

        /// <summary>台达 ASCII over TCP。</summary>
        DeltaSerialAsciiOverTcp,

        /// <summary>汇川 Modbus TCP。</summary>
        InovanceTcp,

        /// <summary>汇川串口。</summary>
        InovanceSerial,

        /// <summary>汇川串口 over TCP。</summary>
        InovanceSerialOverTcp,

        /// <summary>汇川 ComputerLink 串口。</summary>
        InovanceComputerLink,

        /// <summary>汇川 Connected CIP。</summary>
        InovanceConnectedCip,

        /// <summary>汇川 Easy。</summary>
        InovanceEasyNet,

        /// <summary>信捷 Modbus TCP。</summary>
        XinJETcp,

        /// <summary>信捷串口。</summary>
        XinJESerial,

        /// <summary>信捷串口 over TCP。</summary>
        XinJESerialOverTcp,

        /// <summary>信捷内部协议。</summary>
        XinJEInternalNet,

        /// <summary>永宏串口。</summary>
        FatekProgram,

        /// <summary>永宏 over TCP。</summary>
        FatekProgramOverTcp,

        /// <summary>富士 SPB 串口。</summary>
        FujiSPB,

        /// <summary>富士 SPB over TCP。</summary>
        FujiSPBOverTcp,

        /// <summary>富士 SPH。</summary>
        FujiSPHNet,

        /// <summary>GE SRTP。</summary>
        GeSRTPNet,

        /// <summary>倍福 ADS。</summary>
        BeckhoffAdsNet,

        /// <summary>LS Cnet 串口。</summary>
        LSCnet,

        /// <summary>LS Cnet over TCP。</summary>
        LSCnetOverTcp,

        /// <summary>LS Cpu 串口。</summary>
        LSCpu,

        /// <summary>LS Fast Enet。</summary>
        LSFastEnet,

        /// <summary>麦格米特 Modbus TCP。</summary>
        MegMeetTcp,

        /// <summary>麦格米特串口。</summary>
        MegMeetSerial,

        /// <summary>麦格米特串口 over TCP。</summary>
        MegMeetSerialOverTcp,

        /// <summary>丰炜串口。</summary>
        VigorSerial,

        /// <summary>丰炜 over TCP。</summary>
        VigorSerialOverTcp,

        /// <summary>横河 Link TCP。</summary>
        YokogawaLinkTcp,

        /// <summary>安川 Memobus TCP。</summary>
        MemobusTcp,

        /// <summary>安川 Memobus UDP。</summary>
        MemobusUdp,

        /// <summary>山武 DigitronCPL 串口。</summary>
        DigitronCPL,

        /// <summary>山武 DigitronCPL over TCP。</summary>
        DigitronCPLOverTcp,

        /// <summary>丰田 ToyoPuc。</summary>
        ToyoPuc,

        /// <summary>Cimon HMI 协议。</summary>
        CimonHmiProtocol
    }
}
