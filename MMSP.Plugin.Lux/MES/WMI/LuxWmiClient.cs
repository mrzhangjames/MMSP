using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MMSP.Common.Helper.Logger;

namespace MMSP.Plugin.Lux.WMI
{
    /// <summary>
    /// 调用 WMI01。失败写 MES 错误日志，不写密钥、账号密码和密文。
    /// </summary>
    public static class LuxWmiClient
    {
        /// <summary>
        /// 共用的 HTTP 客户端。超时由单次调用自己限制。
        /// </summary>
        private static readonly HttpClient Http = new()
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        /// <summary>
        /// 按目录里的方法调用一次。重试次数是第一次失败之后再试的次数。
        /// </summary>
        public static LuxWmiResult Call(
            LuxMesWmiConfig config,
            LuxWmiMethod method,
            IReadOnlyDictionary<string, object?>? parameters = null)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(method);
            return CallAsync(config, method, parameters).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步调用。日志仍然只在全部尝试结束且失败时写一次。
        /// </summary>
        public static async Task<LuxWmiResult> CallAsync(
            LuxMesWmiConfig config,
            LuxWmiMethod method,
            IReadOnlyDictionary<string, object?>? parameters = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(method);
            if (!config.IsEnable)
                return Fail(method.Name, "WMI 未启用");

            var address = LuxWmiEnvelope.AddressOf(config);
            if (address.Length == 0)
                return Fail(method.Name, "服务地址为空");

            string plain;
            try
            {
                plain = LuxWmiEnvelope.Plain(config.ServerName, method.Name, parameters, method.ParameterAsString);
            }
            catch (JsonException ex)
            {
                return Fail(method.Name, "参数无法编码", ex);
            }
            catch (NotSupportedException ex)
            {
                return Fail(method.Name, "参数无法编码", ex);
            }
            catch (InvalidOperationException ex)
            {
                return Fail(method.Name, "参数无法编码", ex);
            }

            if (!LuxWmiCrypto.TryEncrypt(plain, config.AesKey, out var cipher, out var cryptoError))
                return Fail(method.Name, cryptoError);

            var soap = LuxWmiEnvelope.Soap(cipher);
            var attempts = 1 + Math.Max(0, config.RetryCount);
            var timeoutMs = config.TimeoutMs > 0 ? config.TimeoutMs : 10000;
            LuxWmiResult? last = null;
            Exception? transport = null;
            for (var attempt = 0; attempt < attempts; attempt++)
            {
                var once = await OnceAsync(address, soap, timeoutMs, cancellationToken).ConfigureAwait(false);
                last = once.Result;
                transport = once.Exception;
                if (last.IsSuccess)
                    return last;
            }

            return Fail(method.Name, last?.Error ?? "WMI 调用失败", transport);
        }

        /// <summary>
        /// 发一次 SOAP。异常变成失败结果，不带上请求正文。
        /// </summary>
        private static async Task<(LuxWmiResult Result, Exception? Exception)> OnceAsync(
            string address,
            string soap,
            int timeoutMs,
            CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(timeoutMs);
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, address);
                request.Content = new StringContent(soap, Encoding.UTF8, "text/xml");
                request.Headers.TryAddWithoutValidation("SOAPAction", "\"http://tempuri.org/WMI01\"");
                using var response = await Http.SendAsync(request, timeout.Token).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false);
                if (!LuxWmiEnvelope.TryRead(body, out var state, out var content, out var error))
                    return (LuxWmiResult.Fail("响应无法解析"), null);

                if (!LuxWmiEnvelope.IsOk(state))
                    return (LuxWmiResult.Fail(string.IsNullOrWhiteSpace(error) ? content : error), null);

                return (LuxWmiResult.Ok(content), null);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return (LuxWmiResult.Fail("调用超时"), null);
            }
            catch (OperationCanceledException)
            {
                return (LuxWmiResult.Fail("调用已取消"), null);
            }
            catch (HttpRequestException ex)
            {
                return (LuxWmiResult.Fail("WMI 调用失败"), ex);
            }
            catch (SocketException ex)
            {
                return (LuxWmiResult.Fail("WMI 调用失败"), ex);
            }
            catch (IOException ex)
            {
                return (LuxWmiResult.Fail("WMI 调用失败"), ex);
            }
            catch (TimeoutException ex)
            {
                return (LuxWmiResult.Fail("WMI 调用失败"), ex);
            }
            catch (FormatException ex)
            {
                return (LuxWmiResult.Fail("WMI 调用失败"), ex);
            }
            catch (InvalidOperationException ex)
            {
                return (LuxWmiResult.Fail("WMI 调用失败"), ex);
            }
        }

        /// <summary>
        /// 记下失败。原因里如果夹带密钥或密文，就改成固定说法。
        /// </summary>
        private static LuxWmiResult Fail(string method, string reason, Exception? exception = null)
        {
            var text = Sanitize(reason);
            LogHelper.MesIoError("WMI " + method + " " + text, exception);
            return LuxWmiResult.Fail(text);
        }

        /// <summary>
        /// 日志只留短原因。
        /// </summary>
        private static string Sanitize(string? reason)
        {
            var text = (reason ?? "").Replace("sysuser", "", StringComparison.Ordinal).Trim();
            if (text.Length == 0 || LooksLikeCipher(text))
                return "WMI 调用失败";

            if (text.Length > 200)
                text = text[..200];

            return text;
        }

        /// <summary>
        /// 大段 Base64 视为密文，不写入日志。
        /// </summary>
        private static bool LooksLikeCipher(string text)
        {
            if (text.Length < 40)
                return false;

            foreach (var ch in text)
            {
                if (ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '+' or '/' or '=' or '\r' or '\n')
                    continue;

                return false;
            }

            return true;
        }
    }
}
