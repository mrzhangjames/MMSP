using MMSP.Core.Model.Base;
using MMSP.Core.Model.Helper;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// MES 配置按公共属性读写 JSON。
    /// </summary>
    public class MesCfgJsonTests
    {
        /// <summary>
        /// 写出的 JSON 只含这个类的公共属性，读回后值不变。
        /// </summary>
        [Fact]
        public void WriteRead_RoundTripsPublicProperties()
        {
            var model = new SampleMes { UrlPath = "/wmi", Count = 3 };
            var json = MesCfgJson.Write(model);

            Assert.Contains("\"UrlPath\":\"/wmi\"", json);
            Assert.Contains("\"Count\":3", json);

            var target = new SampleMes();
            MesCfgJson.Read(target, json);
            Assert.Equal("/wmi", target.UrlPath);
            Assert.Equal(3, target.Count);
        }

        /// <summary>
        /// 空文本把已有值恢复成默认值。
        /// </summary>
        [Fact]
        public void Read_EmptyJsonResetsDefaults()
        {
            var model = new SampleMes { UrlPath = "/old", Count = 8 };
            MesCfgJson.Read(model, "");
            Assert.Equal("", model.UrlPath);
            Assert.Equal(0, model.Count);
        }

        /// <summary>
        /// 字段标题用说明，没有说明时用属性名。
        /// </summary>
        [Fact]
        public void Fields_UsesDescriptionThenName()
        {
            var fields = MesCfgJson.Fields(new SampleMes { UrlPath = "/wmi", Count = 2 });
            Assert.Equal("URL 路径", fields.Single(item => item.Name == nameof(MesCfgModel.UrlPath)).Label);
            Assert.Equal("/wmi", fields.Single(item => item.Name == nameof(MesCfgModel.UrlPath)).Text);
            Assert.False(fields.Single(item => item.Name == nameof(MesCfgModel.UrlPath)).IsCheck);
            Assert.Equal(nameof(SampleMes.Count), fields.Single(item => item.Name == nameof(SampleMes.Count)).Label);
            Assert.Equal("2", fields.Single(item => item.Name == nameof(SampleMes.Count)).Text);
            Assert.False(fields.Single(item => item.Name == nameof(SampleMes.Count)).IsCheck);
            var enabled = fields.Single(item => item.Name == nameof(MesCfgModel.IsEnable));
            Assert.Equal("是否启用", enabled.Label);
            Assert.Equal("true", enabled.Text);
            Assert.True(enabled.IsCheck);
        }

        /// <summary>
        /// 有一项无法识别时不改原实例。
        /// </summary>
        [Fact]
        public void Apply_RejectsBadValueWithoutChangingTarget()
        {
            var model = new SampleMes { UrlPath = "/wmi", Count = 2 };
            var error = MesCfgJson.Apply(model,
            [
                new MesCfgField(nameof(SampleMes.UrlPath), "URL 路径", "/next"),
                new MesCfgField(nameof(SampleMes.Count), nameof(SampleMes.Count), "no")
            ]);

            Assert.Equal(nameof(SampleMes.Count), error);
            Assert.Equal("/wmi", model.UrlPath);
            Assert.Equal(2, model.Count);
        }

        /// <summary>
        /// 合法文字写回全部属性。
        /// </summary>
        [Fact]
        public void Apply_WritesRecognizedValues()
        {
            var model = new SampleMes();
            var error = MesCfgJson.Apply(model,
            [
                new MesCfgField(nameof(SampleMes.UrlPath), "URL 路径", " /next "),
                new MesCfgField(nameof(SampleMes.Count), nameof(SampleMes.Count), "4")
            ]);

            Assert.Null(error);
            Assert.Equal("/next", model.UrlPath);
            Assert.Equal(4, model.Count);
        }

        /// <summary>
        /// 测试用配置。比基类多一个整数。
        /// </summary>
        private sealed class SampleMes : MesCfgModel
        {
            /// <summary>
            /// 计数。
            /// </summary>
            public int Count { get; set; }
        }
    }
}