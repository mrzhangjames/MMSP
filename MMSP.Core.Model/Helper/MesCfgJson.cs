using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using MMSP.Core.Model.Base;

namespace MMSP.Core.Model.Helper
{
    /// <summary>
    /// 一个 MES 配置属性在界面上的名称、文字和编辑方式。
    /// </summary>
    public sealed class MesCfgField
    {
        /// <summary>
        /// 记下属性名、标题、当前文字，以及是否用勾选框。
        /// </summary>
        public MesCfgField(string name, string label, string text, bool isCheck = false)
        {
            Name = name;
            Label = label;
            Text = text;
            IsCheck = isCheck;
        }

        /// <summary>
        /// 属性名。保存时按它写回。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 界面标题。有说明时用说明，否则用属性名。
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// 当前文字。
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 为 true 时界面用勾选框。只有布尔属性为 true。
        /// </summary>
        public bool IsCheck { get; }
    }

    /// <summary>
    /// 把 MES 配置的公共属性写成 JSON，再从 JSON 读回。
    /// </summary>
    public static class MesCfgJson
    {
        /// <summary>
        /// 属性名按类上的写法保存，读的时候不分大小写。
        /// </summary>
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// 写出这个实例的公共可写属性。
        /// </summary>
        public static string Write(MesCfgModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            return JsonSerializer.Serialize(model, model.GetType(), Json);
        }

        /// <summary>
        /// 把 JSON 读进已有实例。空文本恢复成新实例的默认值。
        /// </summary>
        public static void Read(MesCfgModel target, string json)
        {
            ArgumentNullException.ThrowIfNull(target);
            var type = target.GetType();
            object? source = string.IsNullOrWhiteSpace(json)
                ? Activator.CreateInstance(type)
                : JsonSerializer.Deserialize(json, type, Json);
            if (source is null)
                throw new InvalidOperationException("MES 配置无法读取。");

            Copy(source, target, type);
        }

        /// <summary>
        /// 列出界面要显示的属性。
        /// </summary>
        public static IReadOnlyList<MesCfgField> Fields(MesCfgModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            var fields = new List<MesCfgField>();
            foreach (var property in Properties(model.GetType()))
            {
                var text = TextOf(property.GetValue(model), property.PropertyType);
                var check = property.PropertyType == typeof(bool);
                fields.Add(new MesCfgField(property.Name, Label(property), text, check));
            }

            return fields;
        }

        /// <summary>
        /// 把界面文字写回实例。有一项无法识别时不改原实例，并返回该项的标题。
        /// </summary>
        public static string? Apply(MesCfgModel target, IReadOnlyList<MesCfgField> fields)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(fields);
            var type = target.GetType();
            var draft = Activator.CreateInstance(type);
            if (draft is null)
                throw new InvalidOperationException("MES 配置无法创建。");

            Copy(target, draft, type);
            var edited = fields.ToDictionary(item => item.Name, item => item.Text);
            foreach (var property in Properties(type))
            {
                if (!edited.TryGetValue(property.Name, out var text))
                    continue;

                if (!TrySet(draft, property, text))
                    return Label(property);

                edited.Remove(property.Name);
            }

            Copy(draft, target, type);
            return null;
        }

        /// <summary>
        /// 公共实例属性，并且有公共的读和写。
        /// </summary>
        private static IEnumerable<PropertyInfo> Properties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.GetMethod is { IsPublic: true }
                    && property.SetMethod is { IsPublic: true }
                    && property.GetIndexParameters().Length == 0);
        }

        /// <summary>
        /// 把源对象的公共属性抄到目标。
        /// </summary>
        private static void Copy(object source, object target, Type type)
        {
            foreach (var property in Properties(type))
                property.SetValue(target, property.GetValue(source));
        }

        /// <summary>
        /// 属性标题。
        /// </summary>
        private static string Label(PropertyInfo property)
        {
            var text = property.GetCustomAttribute<DescriptionAttribute>()?.Description;
            return string.IsNullOrWhiteSpace(text) ? property.Name : text;
        }

        /// <summary>
        /// 属性值变成界面文字。字符串原样显示，其余按 JSON。
        /// </summary>
        private static string TextOf(object? value, Type type)
        {
            if (value is null)
                return "";

            if (type == typeof(string))
                return (string)value;

            return JsonSerializer.Serialize(value, type, Json);
        }

        /// <summary>
        /// 把界面文字写进草稿。字符串去掉两端空白，其余按 JSON 读。
        /// </summary>
        private static bool TrySet(object draft, PropertyInfo property, string text)
        {
            if (property.PropertyType == typeof(string))
            {
                property.SetValue(draft, text.Trim());
                return true;
            }

            try
            {
                var value = JsonSerializer.Deserialize(text, property.PropertyType, Json);
                if (value is null && property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) is null)
                    return false;

                property.SetValue(draft, value);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}