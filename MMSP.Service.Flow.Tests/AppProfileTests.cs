using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 应用程序安装身份的读取，以及客户特性和设备特性的筛选。
    /// </summary>
    public class AppProfileTests
    {
        /// <summary>
        /// 注释和枚举名可以读出立讯 PAD_001。
        /// </summary>
        [Fact]
        public void Parse_ReadsEnumNamesAndSkipsComments()
        {
            var profile = AppProfile.Parse("""
                // Customer：Lux 立讯，Zte 中兴
                // DeviceType：PAD_001
                {
                  "Customer": "Lux",
                  "DeviceType": "PAD_001"
                }
                """);

            Assert.Equal(Customer.Lux, profile.Customer);
            Assert.Equal(DeviceType.PAD_001, profile.DeviceType);
        }

        /// <summary>
        /// 仓库里的 app.cfg 能按注释后的枚举名读出。
        /// </summary>
        [Fact]
        public void Parse_ReadsRepositoryConfig()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Lib", "Config", "app.cfg")))
                dir = dir.Parent;

            Assert.NotNull(dir);
            var profile = AppProfile.Parse(File.ReadAllText(Path.Combine(dir.FullName, "Lib", "Config", "app.cfg")));
            Assert.Equal(Customer.Lux, profile.Customer);
            Assert.Equal(DeviceType.PAD_001, profile.DeviceType);
        }

        /// <summary>
        /// 数字、小写和无法识别的名称都不接受。
        /// </summary>
        [Theory]
        [InlineData("""{"Customer":0,"DeviceType":"PAD_001"}""")]
        [InlineData("""{"Customer":"0","DeviceType":"PAD_001"}""")]
        [InlineData("""{"Customer":"lux","DeviceType":"PAD_001"}""")]
        [InlineData("""{"Customer":"Lux","DeviceType":"PAD"}""")]
        [InlineData("""{"DeviceType":"PAD_001"}""")]
        public void Parse_RejectsUnknownValues(string json)
        {
            Assert.Throws<InvalidOperationException>(() => AppProfile.Parse(json));
        }

        /// <summary>
        /// 立讯 PAD_001 只留下标了这一档的 MES 配置和工具。
        /// </summary>
        [Fact]
        public void Pick_KeepsMatchingLuxTypes()
        {
            var pick = PluginScope.Pick(
                [typeof(PadWmi), typeof(ZteMes), typeof(NoDevice)],
                [typeof(AlarmTool), typeof(NoToolKind)],
                Customer.Lux,
                DeviceType.PAD_001);

            Assert.Equal(typeof(PadWmi), Assert.Single(pick.MesConfigs));
            var tool = Assert.Single(pick.Tools);
            Assert.Equal(typeof(AlarmTool), tool.ViewType);
            Assert.Equal(FlowToolKind.Lux_AlarmLogTool, tool.Kind);
            Assert.Contains(pick.Skipped, item => item.Contains(nameof(ZteMes)));
            Assert.Contains(pick.Skipped, item => item.Contains(nameof(NoDevice)));
            Assert.Contains(pick.Skipped, item => item.Contains(nameof(NoToolKind)));
        }

        /// <summary>
        /// 同一档有两个 MES 配置时失败。
        /// </summary>
        [Fact]
        public void Pick_RejectsTwoMesConfigs()
        {
            var error = Assert.Throws<InvalidOperationException>(() => PluginScope.Pick(
                [typeof(PadWmi), typeof(PadSfc)],
                [],
                Customer.Lux,
                DeviceType.PAD_001));

            Assert.Contains("一次运行只能使用一个", error.Message);
        }

        /// <summary>
        /// 设备特性里没有当前设备类型时不匹配。
        /// </summary>
        [Fact]
        public void Match_RejectsOtherDevice()
        {
            var result = PluginScope.Match(typeof(PadWmi), Customer.Lux, (DeviceType)9);
            Assert.Equal(ScopeResult.DeviceMismatch, result);
        }

        /// <summary>
        /// 立讯 WMI 配置。
        /// </summary>
        [Customer(Customer.Lux)]
        [DeviceType(DeviceType.PAD_001)]
        private sealed class PadWmi : MesCfgModel
        {
        }

        /// <summary>
        /// 同一档的另一个 MES 配置。
        /// </summary>
        [Customer(Customer.Lux)]
        [DeviceType(DeviceType.PAD_001)]
        private sealed class PadSfc : MesCfgModel
        {
        }

        /// <summary>
        /// 中兴的 MES 配置。
        /// </summary>
        [Customer(Customer.Zte)]
        [DeviceType(DeviceType.PAD_001)]
        private sealed class ZteMes : MesCfgModel
        {
        }

        /// <summary>
        /// 没有设备特性的配置。
        /// </summary>
        [Customer(Customer.Lux)]
        private sealed class NoDevice : MesCfgModel
        {
        }

        /// <summary>
        /// 标了工具种类的界面。
        /// </summary>
        [Customer(Customer.Lux)]
        [DeviceType(DeviceType.PAD_001)]
        [FlowTool(FlowToolKind.Lux_AlarmLogTool)]
        private sealed class AlarmTool
        {
        }

        /// <summary>
        /// 对上客户和设备，但没有工具特性。
        /// </summary>
        [Customer(Customer.Lux)]
        [DeviceType(DeviceType.PAD_001)]
        private sealed class NoToolKind
        {
        }
    }
}
