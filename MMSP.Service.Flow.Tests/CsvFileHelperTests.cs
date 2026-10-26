using System.Text;
using MMSP.Common.Helper;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 带表头的 CSV 读写。
    /// </summary>
    public class CsvFileHelperTests
    {
        /// <summary>
        /// 表头和单元格往返。逗号、换行留在单元格里。
        /// </summary>
        [Fact]
        public void Table_RoundTrip_KeepsHeaderAndQuotedText()
        {
            var path = TempPath();
            try
            {
                var headers = new[] { "地址", "数据类型", "报警描述" };
                var rows = new IReadOnlyDictionary<string, string>[]
                {
                    new Dictionary<string, string>
                    {
                        ["地址"] = "D100",
                        ["数据类型"] = "UInt16",
                        ["报警描述"] = "急停, 一线"
                    },
                    new Dictionary<string, string>
                    {
                        ["地址"] = "D200",
                        ["数据类型"] = "String",
                        ["报警描述"] = "第二行\r\n继续"
                    }
                };

                CsvFileHelper.WriteTable(path, headers, rows);
                var table = CsvFileHelper.ReadTable(path);

                Assert.Equal(headers, table.Headers);
                Assert.Equal(2, table.Rows.Count);
                Assert.Equal("急停, 一线", table.Rows[0]["报警描述"]);
                Assert.Equal("第二行\r\n继续", table.Rows[1]["报警描述"]);
                Assert.Equal("D200", table.Rows[1]["地址"]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// 只有表头时没有数据行。空文件连表头也没有。
        /// </summary>
        [Fact]
        public void Table_HeaderOnlyAndEmptyFile()
        {
            var headerOnly = TempPath();
            var empty = TempPath();
            try
            {
                CsvFileHelper.WriteTable(headerOnly, ["地址", "数据类型"], []);
                var table = CsvFileHelper.ReadTable(headerOnly);
                Assert.Equal(["地址", "数据类型"], table.Headers);
                Assert.Empty(table.Rows);

                File.WriteAllText(empty, "");
                var blank = CsvFileHelper.ReadTable(empty);
                Assert.Empty(blank.Headers);
                Assert.Empty(blank.Rows);
            }
            finally
            {
                File.Delete(headerOnly);
                File.Delete(empty);
            }
        }

        /// <summary>
        /// 少写一列时该列是空，不把整份文件判失败。
        /// </summary>
        [Fact]
        public void Table_ShortRow_MissingCellIsEmpty()
        {
            var path = TempPath();
            try
            {
                File.WriteAllText(path, "地址,数据类型\r\nD1\r\n", new System.Text.UTF8Encoding(true));

                var table = CsvFileHelper.ReadTable(path);

                Assert.Equal("D1", table.Rows[0]["地址"]);
                Assert.Equal("", table.Rows[0]["数据类型"]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// 原来的无表头读写仍把第一行当数据。
        /// </summary>
        [Fact]
        public void PlainRead_FirstRowIsData()
        {
            var path = TempPath();
            try
            {
                CsvFileHelper.Write(path, [["地址", "D1"], ["Int16", "1"]]);

                var rows = CsvFileHelper.Read(path);

                Assert.Equal(["地址", "D1"], rows[0]);
                Assert.Equal(["Int16", "1"], rows[1]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Excel 中文 CSV 没有 BOM，按 GB18030 读回中文。
        /// </summary>
        [Fact]
        public void PlainRead_GbkWithoutBom_KeepsChinese()
        {
            var path = TempPath();
            try
            {
                CsvFileHelper.Write(path, [["名称"]]);
                var text = "点位名称,地址,数据类型,长度,读写,启用,点位类型,Bit,描述\r\n设备状态,D102,Int16,1,只读,启用,状态,,设备状态。0运行。\r\n";
                File.WriteAllBytes(path, Encoding.GetEncoding("GB18030").GetBytes(text));

                var rows = CsvFileHelper.Read(path);

                Assert.Equal("点位名称", rows[0][0]);
                Assert.Equal("设备状态", rows[1][0]);
                Assert.Equal("只读", rows[1][4]);
                Assert.Equal("启用", rows[1][5]);
                Assert.Equal("设备状态。0运行。", rows[1][8]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// 不带 BOM 的 UTF-8 中文仍按 UTF-8 读。
        /// </summary>
        [Fact]
        public void PlainRead_Utf8WithoutBom_KeepsChinese()
        {
            var path = TempPath();
            try
            {
                File.WriteAllBytes(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes("名称,D1\r\n"));

                var rows = CsvFileHelper.Read(path);

                Assert.Equal("名称", rows[0][0]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// 临时文件路径。
        /// </summary>
        private static string TempPath()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".csv");
        }
    }
}
