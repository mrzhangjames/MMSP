using HslCommunication.Core;
using HslCommunication.Profinet.Beckhoff;
using HslCommunication.Profinet.Cimon;
using HslCommunication.Profinet.FATEK;
using HslCommunication.Profinet.Fuji;
using HslCommunication.Profinet.GE;
using HslCommunication.Profinet.LSIS;
using HslCommunication.Profinet.MegMeet;
using HslCommunication.Profinet.Toyota;
using HslCommunication.Profinet.Vigor;
using HslCommunication.Profinet.Yamatake;
using HslCommunication.Profinet.YASKAWA;
using HslCommunication.Profinet.Yokogawa;

namespace MMSP.Service.Device.Plc.Drivers
{
    /// <summary>
    /// 永宏、富士、GE、倍福、LS、麦格米特、丰炜、横河、安川、山武、丰田和 Cimon。
    /// 网口使用 Host、Port 和超时。串口使用 Host 作为 COM 口、Station 和 BaudRate。麦格米特另用 DataFormat。
    /// </summary>
    internal static class OtherPlcDrivers
    {
        /// <summary>
        /// 登记其余品牌的客户端。
        /// </summary>
        public static void Register(Action<PlcProtocol, Func<PlcConnection, HslPlcDriver>> add)
        {
            add(PlcProtocol.FatekProgram, link => Serial(link, static item => new FatekProgram()));
            add(PlcProtocol.FatekProgramOverTcp, link => Tcp(link, static item => WithStation(new FatekProgramOverTcp(item.Host, item.Port), item)));
            add(PlcProtocol.FujiSPB, link => Serial(link, static item => new FujiSPB()));
            add(PlcProtocol.FujiSPBOverTcp, link => Tcp(link, static item => WithStation(new FujiSPBOverTcp(item.Host, item.Port), item)));
            add(PlcProtocol.FujiSPHNet, link => Tcp(link, static item => new FujiSPHNet(item.Host, item.Port)));
            add(PlcProtocol.GeSRTPNet, link => Tcp(link, static item => new GeSRTPNet(item.Host, item.Port)));
            add(PlcProtocol.BeckhoffAdsNet, link => Tcp(link, static item => new BeckhoffAdsNet(item.Host, item.Port)));
            add(PlcProtocol.LSCnet, link => Serial(link, static item => new LSCnet()));
            add(PlcProtocol.LSCnetOverTcp, link => Tcp(link, static item => WithStation(new LSCnetOverTcp(item.Host, item.Port), item)));
            add(PlcProtocol.LSCpu, link => Serial(link, static item => new LSCpu()));
            add(PlcProtocol.LSFastEnet, link => Tcp(link, static item => new LSFastEnet(item.Host, item.Port)));
            add(PlcProtocol.MegMeetTcp, link => new TcpPlcDriver(link, static item => Modbus(new MegMeetTcpNet(item.Host, item.Port, item.Station), item, serial: false)));
            add(PlcProtocol.MegMeetSerial, link => new SerialPortPlcDriver(link, static item => Modbus(new MegMeetSerial(item.Station), item, serial: true)));
            add(PlcProtocol.MegMeetSerialOverTcp, link => new TcpPlcDriver(link, static item => Modbus(new MegMeetSerialOverTcp(item.Host, item.Port, item.Station), item, serial: false)));
            add(PlcProtocol.VigorSerial, link => Serial(link, static item => new VigorSerial()));
            add(PlcProtocol.VigorSerialOverTcp, link => Tcp(link, static item => WithStation(new VigorSerialOverTcp(item.Host, item.Port), item)));
            add(PlcProtocol.YokogawaLinkTcp, link => Tcp(link, static item => new YokogawaLinkTcp(item.Host, item.Port)));
            add(PlcProtocol.MemobusTcp, link => Tcp(link, static item => new MemobusTcpNet(item.Host, item.Port)));
            add(PlcProtocol.MemobusUdp, link => Tcp(link, static item => new MemobusUdpNet(item.Host, item.Port)));
            add(PlcProtocol.DigitronCPL, link => Serial(link, static item => new DigitronCPL()));
            add(PlcProtocol.DigitronCPLOverTcp, link => Tcp(link, static item => WithStation(new DigitronCPLOverTcp(item.Host, item.Port, item.Station), item)));
            add(PlcProtocol.ToyoPuc, link => Tcp(link, static item => new ToyoPuc(item.Host, item.Port)));
            add(PlcProtocol.CimonHmiProtocol, link => Tcp(link, static item => new CimonHmiProtocol(item.Host, item.Port)));
        }

        private static HslPlcDriver Tcp(PlcConnection link, Func<PlcConnection, IReadWriteNet> create)
        {
            return new TcpPlcDriver(link, item =>
            {
                var plc = create(item);
                PlcDeviceSetup.UseTimeouts(plc, item);
                return plc;
            });
        }

        private static HslPlcDriver Serial(PlcConnection link, Func<PlcConnection, IReadWriteNet> create)
        {
            return new SerialPortPlcDriver(link, item =>
            {
                var plc = create(item);
                PlcDeviceSetup.UseSerial(plc, item);
                return plc;
            });
        }

        private static IReadWriteNet WithStation(IReadWriteNet plc, PlcConnection item)
        {
            PlcDeviceSetup.SetStation(plc, item.Station);
            return plc;
        }

        private static T Modbus<T>(T plc, PlcConnection item, bool serial)
        {
            PlcDeviceSetup.UseModbus(plc!, item);
            if (serial)
                PlcDeviceSetup.UseSerial(plc!, item);
            return plc;
        }
    }
}
