namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// 扫码枪下拉项。已保存但不在设备列表里的名称仍保留。
    /// </summary>
    internal static class ScannerList
    {
        /// <summary>
        /// 设备里的枪在前，配置里多出来的名称补在后面。
        /// </summary>
        public static IReadOnlyList<string> Merge(IReadOnlyList<string>? names, string? saved)
        {
            var list = new List<string>();
            if (names is not null)
            {
                foreach (var name in names)
                    Add(list, name);
            }

            foreach (var name in Split(saved))
                Add(list, name);

            return list;
        }

        /// <summary>
        /// 按顿号拆开已保存的枪名。
        /// </summary>
        public static IReadOnlyList<string> Split(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return [];

            return text.Split('、', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        /// <summary>
        /// 去掉空白和重复名称。
        /// </summary>
        private static void Add(List<string> list, string? name)
        {
            var text = name?.Trim() ?? "";
            if (text.Length == 0 || list.Contains(text))
                return;

            list.Add(text);
        }
    }
}
