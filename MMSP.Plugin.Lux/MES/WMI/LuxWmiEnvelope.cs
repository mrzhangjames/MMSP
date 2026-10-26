using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace MMSP.Plugin.Lux.WMI
{
    /// <summary>
    /// 组装 WMI01 的明文、SOAP，并解析 ASMX 的 string 返回。
    /// </summary>
    public static class LuxWmiEnvelope
    {
        /// <summary>
        /// 协议固定账号。不要写入日志。
        /// </summary>
        private const string ProtocolUser = "sys";

        /// <summary>
        /// 协议固定密码。不要写入日志。
        /// </summary>
        private const string ProtocolPassword = "sysuser";

        private static readonly JsonSerializerOptions Json = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly Regex StringBody = new(
            "<string\\b[^>]*>(.*)</string>",
            RegexOptions.Singleline | RegexOptions.CultureInvariant);

        /// <summary>
        /// 服务地址。ServiceUrl 有值时用它，否则用 URL 路径。
        /// </summary>
        public static string AddressOf(LuxMesWmiConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);
            var service = (config.ServiceUrl ?? "").Trim();
            if (service.Length > 0)
                return service;

            return (config.UrlPath ?? "").Trim();
        }

        /// <summary>
        /// 加密前的 JSON。methodparameter 默认是对象。
        /// </summary>
        public static string Plain(string? serverName, string method, IReadOnlyDictionary<string, object?>? parameters, bool parameterAsString)
        {
            var node = new JsonObject
            {
                ["user"] = ProtocolUser,
                ["password"] = ProtocolPassword,
                ["servername"] = serverName ?? "",
                ["method"] = method ?? ""
            };
            var values = parameters ?? new Dictionary<string, object?>();
            if (parameterAsString)
                node["methodparameter"] = JsonSerializer.Serialize(values, Json);
            else
                node["methodparameter"] = JsonSerializer.SerializeToNode(values, Json);

            return node.ToJsonString(Json);
        }

        /// <summary>
        /// 把已经加密的 Par 放进 WMI01。
        /// </summary>
        public static string Soap(string par)
        {
            var body = WebUtility.HtmlEncode(par ?? "");
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                + "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">"
                + "<soap:Body><WMI01 xmlns=\"http://tempuri.org/\"><Par>"
                + body
                + "</Par></WMI01></soap:Body></soap:Envelope>";
        }

        /// <summary>
        /// 从 ASMX 正文取出 state、content 和 error。取不到时返回 false。
        /// </summary>
        public static bool TryRead(string? body, out string state, out string content, out string error)
        {
            state = "";
            content = "";
            error = "";
            if (string.IsNullOrWhiteSpace(body))
                return false;

            var json = body.Trim();
            var match = StringBody.Match(json);
            if (match.Success)
                json = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                return false;
            }

            using (document)
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    return false;

                if (document.RootElement.TryGetProperty("state", out var stateProp) && stateProp.ValueKind == JsonValueKind.String)
                    state = stateProp.GetString() ?? "";

                if (document.RootElement.TryGetProperty("error", out var errorProp))
                    error = TextOf(errorProp);

                if (document.RootElement.TryGetProperty("content", out var contentProp))
                    content = TextOf(contentProp);

                return state.Length > 0;
            }
        }

        /// <summary>
        /// state 为 true 才算成功。比较不分大小写。
        /// </summary>
        public static bool IsOk(string? state)
        {
            return string.Equals(state, "true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 字符串用原值，对象和数组保留 JSON。
        /// </summary>
        private static string TextOf(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.String)
                return value.GetString() ?? "";

            if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                return "";

            return value.GetRawText();
        }
    }
}
