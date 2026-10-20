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
        /// 按表头读出。第一行是列名，空文件返回空表。列名去掉首尾空白。
        /// </summary>
        public static CsvTable ReadTable(string filePath)
        {
            using var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            using var csv = new CsvReader(reader, CreateHeaderConfig());
            if (!csv.Read())
                return new CsvTable([], []);

            csv.ReadHeader();
            var headers = (csv.HeaderRecord ?? [])
                .Select(item => (item ?? "").Trim())
                .ToArray();
            var rows = new List<IReadOnlyDictionary<string, string>>();
            while (csv.Read())
            {
                var row = new Dictionary<string, string>(StringComparer.Ordinal);
                for (var index = 0; index < headers.Length; index++)
                {
                    if (headers[index].Length == 0)
                        continue;

                    csv.TryGetField<string>(index, out var field);
                    row[headers[index]] = field ?? "";
                }

                rows.Add(row);
            }

            return new CsvTable(headers, rows);
        }

        /// <summary>
        /// 先写表头，再按表头顺序写每一行。文件使用 UTF-8，并带 BOM。缺列写空。
        /// </summary>
        public static void WriteTable(string filePath, IReadOnlyList<string> headers, IEnumerable<IReadOnlyDictionary<string, string>> rows)
        {
            using var writer = new StreamWriter(filePath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            using var csv = new CsvWriter(writer, CreateHeaderConfig());
            foreach (var header in headers)
                csv.WriteField(header ?? "");

            csv.NextRecord();
            foreach (var row in rows)
            {
                foreach (var header in headers)
                {
                    var name = header ?? "";
                    var value = "";
                    if (row is not null && name.Length > 0 && row.TryGetValue(name, out var cell))
                        value = cell ?? "";

                    csv.WriteField(value);
                }

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

        /// <summary>
        /// 逗号分隔，第一行是表头。少列时不抛缺失列异常。
        /// </summary>
        private static CsvConfiguration CreateHeaderConfig()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                DetectDelimiter = false,
                MissingFieldFound = null,
                NewLine = "\r\n"
            };
        }
    }

    /// <summary>
    /// 带表头的 CSV。列名按文件顺序，数据行用列名取值。
    /// </summary>
    public sealed class CsvTable
    {
        /// <summary>
        /// 读出一张表。
        /// </summary>
        public CsvTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyDictionary<string, string>> rows)
        {
            Headers = headers;
            Rows = rows;
        }

        /// <summary>
        /// 列名。
        /// </summary>
        public IReadOnlyList<string> Headers { get; }

        /// <summary>
        /// 数据行。
        /// </summary>
        public IReadOnlyList<IReadOnlyDictionary<string, string>> Rows { get; }
    }
}
