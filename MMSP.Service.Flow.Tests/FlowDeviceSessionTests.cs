using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Device.Plc;
using MMSP.Service.Device.Scan;
using MMSP.Service.Flow;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 设备会话里不碰网络的部分。
    /// </summary>
    public class FlowDeviceSessionTests
    {
        /// <summary>
        /// 同名枪用列表里靠前的那把。
        /// </summary>
        [Fact]
        public void FindScanner_DuplicateName_UsesFirst()
        {
            var first = new ScannerEntity { Id = Guid.NewGuid(), Name = " Gun " };
            var second = new ScannerEntity { Id = Guid.NewGuid(), Name = "Gun" };
            var found = FlowDeviceSession.FindScanner(new[] { first, second }, "Gun");
            Assert.Same(first, found);
        }

        /// <summary>
        /// 找不到枪时不创建客户端。
        /// </summary>
        [Fact]
        public void Trigger_MissingScanner_Fails()
        {
            var scanners = new[] { new ScannerEntity { Id = Guid.NewGuid(), Name = "Gun" } };
            using var session = new FlowDeviceSession(null, scanners);
            var result = session.Trigger("Other", "LON", CancellationToken.None);
            Assert.False(result.IsSuccess);
            Assert.Equal("找不到扫码枪", result.Message);
        }

        /// <summary>
        /// 没有 PLC 配置时不创建客户端。
        /// </summary>
        [Fact]
        public void Read_WithoutLink_Fails()
        {
            using var session = new FlowDeviceSession(null, Array.Empty<ScannerEntity>());
            var result = session.Read("D1", PlcDataType.Int16, 1, CancellationToken.None);
            Assert.False(result.IsSuccess);
            Assert.Equal("没有 PLC 连接", result.Message);
        }

        /// <summary>
        /// 两条流程同时第一次读 PLC，只创建一个客户端。
        /// </summary>
        [Fact]
        public async Task Read_ConcurrentFirstUse_OpensPlcOnce()
        {
            var opens = 0;
            var plc = new FakePlc();
            using var session = new FlowDeviceSession(
                Link(),
                Array.Empty<ScannerEntity>(),
                _ =>
                {
                    Interlocked.Increment(ref opens);
                    return plc;
                },
                _ => throw new InvalidOperationException("不应创建扫码枪"));
            var barrier = new Barrier(2);

            await Task.WhenAll(
                Task.Run(() => ReadOnce(session, barrier)),
                Task.Run(() => ReadOnce(session, barrier)));

            Assert.Equal(1, opens);
        }

        /// <summary>
        /// 两条流程同时触发同一把枪，只创建一个客户端。
        /// </summary>
        [Fact]
        public async Task Trigger_ConcurrentSameScanner_OpensOnce()
        {
            var opens = 0;
            var gun = new ScannerEntity { Id = Guid.NewGuid(), Name = "Gun", Host = "127.0.0.1", Port = 9000 };
            using var session = new FlowDeviceSession(
                null,
                new[] { gun },
                _ => throw new InvalidOperationException("不应创建 PLC"),
                _ =>
                {
                    Interlocked.Increment(ref opens);
                    return new FakeScan();
                });
            var barrier = new Barrier(2);

            await Task.WhenAll(
                Task.Run(() => TriggerOnce(session, barrier)),
                Task.Run(() => TriggerOnce(session, barrier)));

            Assert.Equal(1, opens);
        }

        /// <summary>
        /// PLC 还停在连接里时，另一条流程的等待可以返回。
        /// </summary>
        [Fact]
        public async Task Wait_DuringConnect_DoesNotWaitForPlc()
        {
            var entered = new ManualResetEventSlim(false);
            var release = new ManualResetEventSlim(false);
            var plc = new FakePlc(entered, release);
            using var session = new FlowDeviceSession(
                Link(),
                Array.Empty<ScannerEntity>(),
                _ => plc,
                _ => throw new InvalidOperationException("不应创建扫码枪"));
            var read = Task.Run(() => session.Read("D1", PlcDataType.Int16, 1, CancellationToken.None));

            try
            {
                Assert.True(entered.Wait(2000));
                var wait = Task.Run(() => session.Wait(30, CancellationToken.None));
                Assert.True(await wait.WaitAsync(TimeSpan.FromSeconds(2)));
            }
            finally
            {
                release.Set();
                await read.WaitAsync(TimeSpan.FromSeconds(2));
            }
        }

        /// <summary>
        /// 一条用来创建客户端的 PLC 连接。
        /// </summary>
        private static PlcLinkEntity Link()
        {
            return new PlcLinkEntity { Host = "127.0.0.1", Port = 502 };
        }

        /// <summary>
        /// 屏障对齐后读一次。
        /// </summary>
        private static void ReadOnce(FlowDeviceSession session, Barrier barrier)
        {
            barrier.SignalAndWait();
            var result = session.Read("D1", PlcDataType.Int16, 1, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        /// <summary>
        /// 屏障对齐后触发一次。
        /// </summary>
        private static void TriggerOnce(FlowDeviceSession session, Barrier barrier)
        {
            barrier.SignalAndWait();
            var result = session.Trigger("Gun", "LON", CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        /// <summary>
        /// 不连接真实 PLC。需要时把连接停住。
        /// </summary>
        private sealed class FakePlc : IPlcClient
        {
            /// <summary>
            /// 已经进入连接。
            /// </summary>
            private readonly ManualResetEventSlim? _entered;

            /// <summary>
            /// 允许连接返回。
            /// </summary>
            private readonly ManualResetEventSlim? _release;

            /// <summary>
            /// 连接立刻成功。
            /// </summary>
            public FakePlc()
            {
            }

            /// <summary>
            /// 连接停在两个信号之间。
            /// </summary>
            public FakePlc(ManualResetEventSlim entered, ManualResetEventSlim release)
            {
                _entered = entered;
                _release = release;
            }

            /// <summary>
            /// 协议。测试不区分。
            /// </summary>
            public PlcProtocol Protocol => PlcProtocol.ModbusTcp;

            /// <summary>
            /// 连接成功后为 true。
            /// </summary>
            public bool IsConnected { get; private set; }

            /// <summary>
            /// 按测试的信号连接。
            /// </summary>
            public PlcIoResult Connect()
            {
                _entered?.Set();
                _release?.Wait();
                IsConnected = true;
                return PlcIoResult.Ok("");
            }

            /// <summary>
            /// 返回一个固定读数。
            /// </summary>
            public PlcIoResult Read(string address, PlcDataType dataType, int length)
            {
                return PlcIoResult.Ok("1");
            }

            /// <summary>
            /// 写入直接成功。
            /// </summary>
            public PlcIoResult Write(string address, PlcDataType dataType, string value)
            {
                return PlcIoResult.Ok("");
            }

            /// <summary>
            /// 标成未连接。
            /// </summary>
            public void Close()
            {
                IsConnected = false;
            }

            /// <summary>
            /// 没有真实连接要关。
            /// </summary>
            public void Dispose()
            {
            }
        }

        /// <summary>
        /// 不连接真实扫码枪。
        /// </summary>
        private sealed class FakeScan : IScanClient
        {
            /// <summary>
            /// 传输方式。测试不区分。
            /// </summary>
            public ScanTransport Transport => ScanTransport.Tcp;

            /// <summary>
            /// 连接成功后为 true。
            /// </summary>
            public bool IsConnected { get; private set; }

            /// <summary>
            /// 直接标成已连接。
            /// </summary>
            public ScanIoResult Connect()
            {
                IsConnected = true;
                return ScanIoResult.Ok("");
            }

            /// <summary>
            /// 返回一个固定条码。
            /// </summary>
            public ScanIoResult Trigger(string command)
            {
                return ScanIoResult.Ok("BC");
            }

            /// <summary>
            /// 标成未连接。
            /// </summary>
            public void Close()
            {
                IsConnected = false;
            }

            /// <summary>
            /// 没有真实连接要关。
            /// </summary>
            public void Dispose()
            {
            }
        }
    }
}
