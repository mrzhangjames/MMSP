using System.Text.Json;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 应用程序安装身份。一次运行只读一次，决定加载哪个客户插件。
    /// </summary>
    public sealed class AppProfile
    {
        /// <summary>
        /// 已经读入的身份。启动完成之前读取会失败。
        /// </summary>
        private static AppProfile? _current;

        /// <summary>
        /// 用已经核对过的枚举值创建。
        /// </summary>
        public AppProfile(Customer customer, DeviceType deviceType)
        {
            if (!Enum.IsDefined(customer))
                throw new ArgumentOutOfRangeException(nameof(customer));
            if (!Enum.IsDefined(deviceType))
                throw new ArgumentOutOfRangeException(nameof(deviceType));

            Customer = customer;
            DeviceType = deviceType;
        }

        /// <summary>
        /// 当前客户。
        /// </summary>
        public Customer Customer { get; }

        /// <summary>
        /// 当前设备类型。
        /// </summary>
        public DeviceType DeviceType { get; }

        /// <summary>
        /// 本次运行的安装身份。尚未加载时抛出。
        /// </summary>
        public static AppProfile Current => _current ?? throw new InvalidOperationException("应用程序配置尚未加载。");

        /// <summary>
        /// 记下本次运行的安装身份。
        /// </summary>
        public static void Use(AppProfile profile)
        {
            _current = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        /// <summary>
        /// 从 JSON 文本读出客户和设备类型。允许注释。枚举必须写名称，不接受数字。
        /// </summary>
        public static AppProfile Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("应用程序配置是空的。");

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(json, new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip
                });
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("应用程序配置不是合法的 JSON。", ex);
            }

            using (document)
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    throw new InvalidOperationException("应用程序配置必须是 JSON 对象。");

                var customer = ReadEnum<Customer>(document.RootElement, "Customer");
                var deviceType = ReadEnum<DeviceType>(document.RootElement, "DeviceType");
                return new AppProfile(customer, deviceType);
            }
        }

        /// <summary>
        /// 读取一个枚举属性。缺字段、非字符串或名称无法识别时失败。
        /// </summary>
        private static TEnum ReadEnum<TEnum>(JsonElement root, string name)
            where TEnum : struct, Enum
        {
            if (!root.TryGetProperty(name, out var property))
                throw new InvalidOperationException($"应用程序配置缺少 {name}。");
            if (property.ValueKind != JsonValueKind.String)
                throw new InvalidOperationException($"应用程序配置的 {name} 必须是枚举名。");

            var text = property.GetString() ?? "";
            if (!IsEnumName(text) || !Enum.TryParse<TEnum>(text, ignoreCase: false, out var value) || !Enum.IsDefined(value))
                throw new InvalidOperationException($"应用程序配置的 {name} 无法识别：{text}。");

            return value;
        }

        /// <summary>
        /// 枚举名由字母或下划线开头。数字字符串不算名称。
        /// </summary>
        private static bool IsEnumName(string text)
        {
            if (text.Length == 0)
                return false;

            var first = text[0];
            return char.IsLetter(first) || first == '_';
        }
    }
}
