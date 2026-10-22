namespace MMSP.Service.Flow
{
    /// <summary>
    /// 一条流程在一次运行里的变量值。启动时用初始值建立，各轮共用，停止后丢掉。
    /// 只能读写已声明的名称。运行中的值不写回数据库。
    /// </summary>
    public sealed class FlowVariables
    {
        /// <summary>
        /// 没有变量的表。任何名称都未声明。
        /// </summary>
        public static FlowVariables Empty { get; } = new(new Dictionary<string, string>(StringComparer.Ordinal));

        /// <summary>
        /// 名称到当前文本。比较区分大小写。
        /// </summary>
        private readonly Dictionary<string, string> _values;

        /// <summary>
        /// 用已有的名称和初始值创建。同名后者覆盖前者。
        /// </summary>
        public FlowVariables(IEnumerable<KeyValuePair<string, string>> values)
        {
            ArgumentNullException.ThrowIfNull(values);
            _values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in values)
            {
                var name = (pair.Key ?? "").Trim();
                if (name.Length == 0)
                    continue;

                _values[name] = pair.Value ?? "";
            }
        }

        /// <summary>
        /// 按启动时抄下的名称和初始值创建。
        /// </summary>
        public static FlowVariables FromSeeds(IEnumerable<(string Name, string Value)> seeds)
        {
            ArgumentNullException.ThrowIfNull(seeds);
            return new FlowVariables(seeds.Select(item => new KeyValuePair<string, string>(item.Name, item.Value)));
        }

        /// <summary>
        /// 这个名称是否在名单里。
        /// </summary>
        public bool Contains(string name)
        {
            return _values.ContainsKey((name ?? "").Trim());
        }

        /// <summary>
        /// 当前文本。未声明时返回空，不写入。
        /// </summary>
        public string Get(string name)
        {
            return _values.TryGetValue((name ?? "").Trim(), out var value) ? value : "";
        }

        /// <summary>
        /// 写入当前文本。未声明的名称不写。
        /// </summary>
        public void Set(string name, string value)
        {
            var key = (name ?? "").Trim();
            if (!_values.ContainsKey(key))
                return;

            _values[key] = value ?? "";
        }
    }
}
