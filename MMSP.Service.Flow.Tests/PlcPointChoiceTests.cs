using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 流程工具按地址对上点位列表。
    /// </summary>
    public class PlcPointChoiceTests
    {
        /// <summary>
        /// 同一地址既有流程又有监控时，数据类型相同则用流程。
        /// </summary>
        [Fact]
        public void Match_PrefersFlowWhenTypeAgrees()
        {
            var points = new List<PlcPointChoice>
            {
                Point("监控点", "D1", PlcDataType.Int16, PlcPointKind.Monitor),
                Point("流程点", "D1", PlcDataType.Int16, PlcPointKind.Flow)
            };

            var match = PlcPointChoices.Match(points, "D1", PlcDataType.Int16);
            Assert.Equal("流程点", match?.Name);
            Assert.Equal(PlcPointKind.Flow, PlcPointChoices.SourceKind(points, "D1", PlcDataType.Int16));
        }

        /// <summary>
        /// 流程点类型对不上、监控点对得上时，用监控点。
        /// </summary>
        [Fact]
        public void Match_PrefersSavedType()
        {
            var points = new List<PlcPointChoice>
            {
                Point("流程点", "D1", PlcDataType.Int16, PlcPointKind.Flow),
                Point("监控点", "D1", PlcDataType.String, PlcPointKind.Monitor)
            };

            var match = PlcPointChoices.Match(points, " D1 ", PlcDataType.String);
            Assert.Equal("监控点", match?.Name);
            Assert.Equal(PlcPointKind.Monitor, PlcPointChoices.SourceKind(points, "D1", PlcDataType.String));
        }

        /// <summary>
        /// 列表里没有这个地址时，数据源仍是流程。空地址也没有下拉项。
        /// </summary>
        [Fact]
        public void MissingAddress_UsesFlow()
        {
            var points = new List<PlcPointChoice>
            {
                Point("流程点", "D1", PlcDataType.Int16, PlcPointKind.Flow),
                Point("空地址", " ", PlcDataType.Int16, PlcPointKind.Monitor)
            };

            Assert.Null(PlcPointChoices.Match(points, "D9", PlcDataType.Int16));
            Assert.Equal(PlcPointKind.Flow, PlcPointChoices.SourceKind(points, "", PlcDataType.Int16));
            Assert.Equal(["流程点"], PlcPointChoices.OfKind(points, PlcPointKind.Flow).Select(item => item.Name).ToArray());
        }

        /// <summary>
        /// 做一个点位选项。
        /// </summary>
        private static PlcPointChoice Point(string name, string address, PlcDataType dataType, PlcPointKind kind)
        {
            return new PlcPointChoice
            {
                Name = name,
                Address = address,
                DataType = dataType,
                Length = 1,
                Kind = kind
            };
        }
    }
}
