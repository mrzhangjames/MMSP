using System.Globalization;
using System.Text.Json;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 从 JSON 文本按路径取出一个值。供编辑窗口试算，不执行流程。
    /// </summary>
    public static class JsonPathValue
    {
        /// <summary>
        /// 取出路径上的文本。失败时 Ok 为 false，Text 是原因。
        /// 空路径或 $ 表示整份文本。字符串去掉引号，其余保留原文。
        /// 路径用点号和方括号，例如 $.data.items[0].name。
        /// </summary>
        public static JsonPathRead Read(string? json, string? path)
        {
            if (string.IsNullOrWhiteSpace(json))
                return JsonPathRead.Fail("样例为空");

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                return JsonPathRead.Fail("JSON 无效");
            }

            using (document)
            {
                if (!TrySplit(path, out var tokens))
                    return JsonPathRead.Fail("路径无效");

                var node = document.RootElement;
                foreach (var token in tokens)
                {
                    if (!TryStep(node, token, out node))
                        return JsonPathRead.Fail("路径不存在");
                }

                return JsonPathRead.Found(TextOf(node));
            }
        }

        /// <summary>
        /// 把路径拆成属性名和下标。拆不开时返回 false。
        /// </summary>
        private static bool TrySplit(string? path, out List<PathToken> tokens)
        {
            tokens = [];
            var text = (path ?? "").Trim();
            if (text.Length == 0)
                return true;

            if (text[0] == '$')
                text = text[1..];

            if (text.StartsWith('.'))
                text = text[1..];

            var index = 0;
            while (index < text.Length)
            {
                if (text[index] == '[')
                {
                    if (!TryIndex(text, ref index, out var number))
                        return false;

                    tokens.Add(PathToken.At(number));
                    if (!SkipDot(text, ref index))
                        return false;

                    continue;
                }

                var start = index;
                while (index < text.Length && text[index] != '.' && text[index] != '[')
                    index++;

                if (index == start)
                    return false;

                tokens.Add(PathToken.Property(text[start..index]));
                if (!SkipDot(text, ref index))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 读一个方括号下标，并把位置停在右括号之后。
        /// </summary>
        private static bool TryIndex(string text, ref int index, out int number)
        {
            number = 0;
            index++;
            var start = index;
            if (index >= text.Length || !char.IsDigit(text[index]))
                return false;

            while (index < text.Length && char.IsDigit(text[index]))
                index++;

            if (index >= text.Length || text[index] != ']')
                return false;

            if (!int.TryParse(text[start..index], NumberStyles.None, CultureInfo.InvariantCulture, out number))
                return false;

            index++;
            if (index < text.Length && text[index] != '.' && text[index] != '[')
                return false;

            return true;
        }

        /// <summary>
        /// 段后面的一个点号可以略过。点号后面没有下一段时路径无效。
        /// </summary>
        private static bool SkipDot(string text, ref int index)
        {
            if (index >= text.Length || text[index] != '.')
                return true;

            index++;
            return index < text.Length;
        }

        /// <summary>
        /// 从当前节点走一步。类型对不上或名称不存在时失败。
        /// </summary>
        private static bool TryStep(JsonElement node, PathToken token, out JsonElement next)
        {
            next = default;
            if (token.IsIndex)
            {
                if (node.ValueKind != JsonValueKind.Array || token.Index < 0 || token.Index >= node.GetArrayLength())
                    return false;

                next = node[token.Index];
                return true;
            }

            if (node.ValueKind != JsonValueKind.Object || !node.TryGetProperty(token.Name, out next))
                return false;

            return true;
        }

        /// <summary>
        /// 字符串返回内容。数字、布尔、空值和对象保留 JSON 原文。
        /// </summary>
        private static string TextOf(JsonElement node)
        {
            if (node.ValueKind == JsonValueKind.String)
                return node.GetString() ?? "";

            return node.GetRawText();
        }

        /// <summary>
        /// 路径上的一段。下标段不使用名称。
        /// </summary>
        private readonly record struct PathToken(bool IsIndex, string Name, int Index)
        {
            /// <summary>
            /// 数组下标。
            /// </summary>
            public static PathToken At(int index)
            {
                return new PathToken(true, "", index);
            }

            /// <summary>
            /// 对象属性名。
            /// </summary>
            public static PathToken Property(string name)
            {
                return new PathToken(false, name, 0);
            }
        }
    }

    /// <summary>
    /// 路径读取的结果。失败时 Text 是原因。
    /// </summary>
    public readonly record struct JsonPathRead(bool Ok, string Text)
    {
        /// <summary>
        /// 取到的文本。
        /// </summary>
        public static JsonPathRead Found(string text)
        {
            return new JsonPathRead(true, text ?? "");
        }

        /// <summary>
        /// 失败原因。
        /// </summary>
        public static JsonPathRead Fail(string reason)
        {
            return new JsonPathRead(false, reason ?? "");
        }
    }
}
