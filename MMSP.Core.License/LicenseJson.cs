using System.Text.Json;

namespace MMSP.Core.License
{
    /// <summary>
    /// 许可证 JSON。未知属性、缺字段、重复字段和尾部多余字节都是格式错误。
    /// </summary>
    internal static class LicenseJson
    {
        /// <summary>
        /// 必填属性。多一个或少一个都不读。
        /// </summary>
        private static readonly string[] Names =
        [
            "format",
            "licenseId",
            "customerCode",
            "machineFingerprint",
            "notBeforeUtc",
            "notAfterUtc",
            "issuedAtUtc",
            "modules",
            "signature"
        ];

        /// <summary>
        /// 读入载荷。失败时 <paramref name="payload"/> 为空。
        /// </summary>
        public static bool TryRead(ReadOnlySpan<byte> utf8, out LicensePayload? payload, out LicenseFailureCode code)
        {
            payload = null;
            code = LicenseFailureCode.FormatInvalid;
            if (utf8.IsEmpty)
                return false;

            var reader = new Utf8JsonReader(utf8, new JsonReaderOptions
            {
                CommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });

            try
            {
                if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                    return false;

                var format = 0;
                var licenseId = Guid.Empty;
                string? customer = null;
                string? machine = null;
                DateTimeOffset notBefore = default;
                DateTimeOffset notAfter = default;
                DateTimeOffset issued = default;
                List<string>? modules = null;
                byte[]? signature = null;
                var seen = new HashSet<string>(StringComparer.Ordinal);

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        return false;

                    var name = reader.GetString();
                    if (name is null || !seen.Add(name) || !reader.Read())
                        return false;

                    switch (name)
                    {
                        case "format":
                            if (reader.TokenType != JsonTokenType.Number || !reader.ValueSpan.SequenceEqual("1"u8))
                                return false;
                            format = 1;
                            break;
                        case "licenseId":
                            if (!TryGuid(ref reader, out licenseId))
                                return false;
                            break;
                        case "customerCode":
                            if (!TryText(ref reader, LicenseRules.IsCustomer, out customer))
                                return false;
                            break;
                        case "machineFingerprint":
                            if (!TryText(ref reader, LicenseRules.IsFingerprint, out machine))
                                return false;
                            break;
                        case "notBeforeUtc":
                            if (!TryTime(ref reader, out notBefore))
                                return false;
                            break;
                        case "notAfterUtc":
                            if (!TryTime(ref reader, out notAfter))
                                return false;
                            break;
                        case "issuedAtUtc":
                            if (!TryTime(ref reader, out issued))
                                return false;
                            break;
                        case "modules":
                            if (!TryModules(ref reader, out modules))
                                return false;
                            break;
                        case "signature":
                            if (!TrySignature(ref reader, out signature))
                                return false;
                            break;
                        default:
                            return false;
                    }
                }

                if (reader.Read() || seen.Count != Names.Length || customer is null || machine is null || modules is null || signature is null)
                    return false;

                payload = new LicensePayload
                {
                    Format = format,
                    LicenseId = licenseId,
                    CustomerCode = customer,
                    MachineFingerprint = machine,
                    NotBeforeUtc = notBefore,
                    NotAfterUtc = notAfter,
                    IssuedAtUtc = issued,
                    Modules = modules,
                    Signature = signature
                };
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// 写出已签名的许可证。编码是不带 BOM 的 UTF-8。
        /// </summary>
        public static byte[] Write(LicenseDraft draft, byte[] signature)
        {
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();
                writer.WriteNumber("format", 1);
                writer.WriteString("licenseId", draft.LicenseId.ToString("D"));
                writer.WriteString("customerCode", draft.CustomerCode);
                writer.WriteString("machineFingerprint", draft.MachineFingerprint);
                writer.WriteString("notBeforeUtc", draft.NotBeforeUtc.ToString("O"));
                writer.WriteString("notAfterUtc", draft.NotAfterUtc.ToString("O"));
                writer.WriteString("issuedAtUtc", draft.IssuedAtUtc.ToString("O"));
                writer.WriteStartArray("modules");
                foreach (var module in draft.Modules)
                    writer.WriteStringValue(module);
                writer.WriteEndArray();
                writer.WriteString("signature", Convert.ToBase64String(signature));
                writer.WriteEndObject();
            }

            return stream.ToArray();
        }

        /// <summary>
        /// 读一个 Guid 字符串。
        /// </summary>
        private static bool TryGuid(ref Utf8JsonReader reader, out Guid value)
        {
            value = Guid.Empty;
            return reader.TokenType == JsonTokenType.String
                && Guid.TryParse(reader.GetString(), out value);
        }

        /// <summary>
        /// 读一个通过 <paramref name="accept"/> 的字符串。
        /// </summary>
        private static bool TryText(ref Utf8JsonReader reader, Func<string?, bool> accept, out string? value)
        {
            value = null;
            if (reader.TokenType != JsonTokenType.String)
                return false;

            var text = reader.GetString();
            if (!accept(text) || text is null)
                return false;

            value = text;
            return true;
        }

        /// <summary>
        /// 读一个时间字符串，并要求它和 O 格式往返一致。
        /// </summary>
        private static bool TryTime(ref Utf8JsonReader reader, out DateTimeOffset value)
        {
            value = default;
            if (reader.TokenType != JsonTokenType.String)
                return false;

            var text = reader.GetString();
            if (text is null || !DateTimeOffset.TryParse(text, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out value))
                return false;

            return value.ToString("O") == text;
        }

        /// <summary>
        /// 读模块数组。名字必须合法，也不能重复。
        /// </summary>
        private static bool TryModules(ref Utf8JsonReader reader, out List<string>? modules)
        {
            modules = null;
            if (reader.TokenType != JsonTokenType.StartArray)
                return false;

            var list = new List<string>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    modules = list;
                    return true;
                }

                if (reader.TokenType != JsonTokenType.String)
                    return false;

                var name = reader.GetString();
                if (!LicenseRules.IsModule(name) || name is null || !names.Add(name))
                    return false;

                list.Add(name);
            }

            return false;
        }

        /// <summary>
        /// 读标准 Base64 签名。空白和空串都不接受。
        /// </summary>
        private static bool TrySignature(ref Utf8JsonReader reader, out byte[]? signature)
        {
            signature = null;
            if (reader.TokenType != JsonTokenType.String)
                return false;

            var text = reader.GetString();
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (var c in text)
            {
                if (char.IsWhiteSpace(c))
                    return false;
            }

            try
            {
                signature = Convert.FromBase64String(text);
            }
            catch (FormatException)
            {
                return false;
            }

            return signature.Length > 0;
        }
    }
}
