using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace MMSP.Common.Helper
{
    /// <summary>
    /// CSV 文件读写。调用方只传行列文本，不直接使用 CsvHelper。
    /// </summary>
    public static class CsvFileHelper
    {
        /// <summary>
        /// 读出全部行。没有表头概念，第一行也是数据。空文件返回空列表。
        /// </summary>
        public static IReadOnlyList<string[]> Read(string filePath)
        {
            using var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            using var csv = new CsvReader(reader, CreateConfig());
            var rows = new List<string[]>();
            while (csv.Read())
            {
                var cells = new List<string>();
                for (var index = 0; csv.TryGetField<string>(index, out var field); index++)
                    cells.Add(field ?? "");

                rows.Add(cells.ToArray());
            }

            return rows;
        }

        /// <summary>
        /// 按给定顺序写出。文件使用 UTF-8，并带 BOM。单元格里的逗号和换行由 CsvHelper 加引号。
        /// </summary>
        public static void Write(string filePath, IEnumerable<IReadOnlyList<string>> rows)
        {
            using var writer = new StreamWriter(filePath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            using var csv = new CsvWriter(writer, CreateConfig());
            foreach (var row in rows)
            {
                foreach (var field in row)
                    csv.WriteField(field);
                csv.NextRecord();
            }
        }

        /// <summary>
        /// 逗号分隔，不把第一行当成表头。
        /// </summary>
        private static CsvConfiguration CreateConfig()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                DetectDelimiter = false,
                NewLine = "\r\n"
            };
        }
    }
}
