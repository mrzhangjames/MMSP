using System.Text.Encodings.Web;
using System.Text.Json;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 全部流程配置的 JSON。含流程、流程步和变量，不含 MES，不含运行中的变量值。
    /// </summary>
    public static class FlowFile
    {
        /// <summary>
        /// 当前文件版本。
        /// </summary>
        public const int CurrentVersion = 1;

        /// <summary>
        /// 写出时用驼峰，中文不转义。
        /// </summary>
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// 根对象允许的字段。多出来的字段拒绝。
        /// </summary>
        private static readonly HashSet<string> RootNames = new(StringComparer.Ordinal) { "version", "flows" };

        /// <summary>
        /// 一条流程允许的字段。
        /// </summary>
        private static readonly HashSet<string> FlowNames = new(StringComparer.Ordinal)
        {
            "name", "isEnabled", "logOutput", "sortIndex", "steps", "vars"
        };

        /// <summary>
        /// 一个流程步允许的字段。
        /// </summary>
        private static readonly HashSet<string> StepNames = new(StringComparer.Ordinal)
        {
            "toolKind", "title", "cfgValue", "sortIndex"
        };

        /// <summary>
        /// 一个变量允许的字段。
        /// </summary>
        private static readonly HashSet<string> VarNames = new(StringComparer.Ordinal)
        {
            "name", "dataType", "initialValue", "sortIndex"
        };

        /// <summary>
        /// 把客户库里的三张表写成 JSON。步骤和变量挂到所属流程下。
        /// </summary>
        public static string Write(
            IEnumerable<FlowEntity> flows,
            IEnumerable<FlowStepEntity> steps,
            IEnumerable<FlowVarEntity> vars)
        {
            ArgumentNullException.ThrowIfNull(flows);
            ArgumentNullException.ThrowIfNull(steps);
            ArgumentNullException.ThrowIfNull(vars);
            var stepsByFlow = steps.GroupBy(item => item.FlowId)
                .ToDictionary(group => group.Key, group => group.OrderBy(item => item.SortIndex).ToList());
            var varsByFlow = vars.GroupBy(item => item.FlowId)
                .ToDictionary(group => group.Key, group => group.OrderBy(item => item.SortIndex).ToList());
            var body = new FileBody
            {
                Version = CurrentVersion,
                Flows = flows
                    .OrderBy(item => item.SortIndex)
                    .ThenBy(item => item.Name, StringComparer.Ordinal)
                    .Select(item => ToDto(item, stepsByFlow, varsByFlow))
                    .ToList()
            };
            return JsonSerializer.Serialize(body, Json);
        }

        /// <summary>
        /// 校验 JSON。失败时 error 是给界面看的原因，flows 为空。
        /// </summary>
        public static bool TryRead(string? json, out IReadOnlyList<FlowFileFlow> flows, out string error)
        {
            flows = [];
            error = "";
            if (string.IsNullOrWhiteSpace(json))
            {
                error = "文件不是流程配置";
                return false;
            }

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                error = "文件不是流程配置";
                return false;
            }

            using (document)
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object || !Known(root, RootNames, out error))
                {
                    error = string.IsNullOrEmpty(error) ? "文件不是流程配置" : error;
                    return false;
                }

                if (!TryInt(root, "version", out var version))
                {
                    error = "文件不是流程配置";
                    return false;
                }

                if (version != CurrentVersion)
                {
                    error = "版本不受支持";
                    return false;
                }

                if (!root.TryGetProperty("flows", out var flowArray) || flowArray.ValueKind != JsonValueKind.Array)
                {
                    error = "文件不是流程配置";
                    return false;
                }

                var list = new List<FlowFileFlow>();
                var index = 0;
                foreach (var item in flowArray.EnumerateArray())
                {
                    index++;
                    if (!TryFlow(item, index, out var flow, out error))
                        return false;

                    list.Add(flow);
                }

                flows = list;
                return true;
            }
        }

        /// <summary>
        /// 按文件内容生成新主键。调用方在一个事务里替换三张表。
        /// </summary>
        public static void CreateRows(
            IReadOnlyList<FlowFileFlow> flows,
            out List<FlowEntity> flowRows,
            out List<FlowStepEntity> stepRows,
            out List<FlowVarEntity> varRows)
        {
            ArgumentNullException.ThrowIfNull(flows);
            flowRows = [];
            stepRows = [];
            varRows = [];
            foreach (var flow in flows)
            {
                var id = Guid.NewGuid();
                flowRows.Add(new FlowEntity
                {
                    Id = id,
                    Name = flow.Name,
                    IsEnabled = flow.IsEnabled,
                    LogOutput = flow.LogOutput,
                    SortIndex = flow.SortIndex
                });
                foreach (var step in flow.Steps)
                {
                    stepRows.Add(new FlowStepEntity
                    {
                        Id = Guid.NewGuid(),
                        FlowId = id,
                        SortIndex = step.SortIndex,
                        ToolKind = step.ToolKind,
                        Title = step.Title,
                        CfgValue = step.CfgValue
                    });
                }

                foreach (var item in flow.Vars)
                {
                    varRows.Add(new FlowVarEntity
                    {
                        Id = Guid.NewGuid(),
                        FlowId = id,
                        Name = item.Name,
                        DataType = item.DataType,
                        InitialValue = item.InitialValue,
                        SortIndex = item.SortIndex
                    });
                }
            }
        }

        /// <summary>
        /// 一条流程及其步骤、变量。
        /// </summary>
        private static FileFlow ToDto(
            FlowEntity flow,
            Dictionary<Guid, List<FlowStepEntity>> stepsByFlow,
            Dictionary<Guid, List<FlowVarEntity>> varsByFlow)
        {
            stepsByFlow.TryGetValue(flow.Id, out var ownedSteps);
            varsByFlow.TryGetValue(flow.Id, out var ownedVars);
            return new FileFlow
            {
                Name = flow.Name ?? "",
                IsEnabled = flow.IsEnabled,
                LogOutput = flow.LogOutput,
                SortIndex = flow.SortIndex,
                Steps = (ownedSteps ?? []).Select(item => new FileStep
                {
                    ToolKind = (int)item.ToolKind,
                    Title = item.Title ?? "",
                    CfgValue = item.CfgValue ?? "",
                    SortIndex = item.SortIndex
                }).ToList(),
                Vars = (ownedVars ?? []).Select(item => new FileVar
                {
                    Name = item.Name ?? "",
                    DataType = item.DataType ?? "",
                    InitialValue = item.InitialValue ?? "",
                    SortIndex = item.SortIndex
                }).ToList()
            };
        }

        /// <summary>
        /// 读一条流程。名称、开关、顺序、步骤和变量都必须在。
        /// </summary>
        private static bool TryFlow(JsonElement item, int index, out FlowFileFlow flow, out string error)
        {
            flow = new FlowFileFlow();
            error = "";
            if (item.ValueKind != JsonValueKind.Object || !Known(item, FlowNames, out error))
            {
                error = Label(index, string.IsNullOrEmpty(error) ? "文件不是流程配置" : error);
                return false;
            }

            if (!TryString(item, "name", out var name)
                || !TryBool(item, "isEnabled", out var enabled)
                || !TryBool(item, "logOutput", out var logOutput)
                || !TryInt(item, "sortIndex", out var sortIndex)
                || !item.TryGetProperty("steps", out var steps) || steps.ValueKind != JsonValueKind.Array
                || !item.TryGetProperty("vars", out var vars) || vars.ValueKind != JsonValueKind.Array)
            {
                error = Label(index, "文件不是流程配置");
                return false;
            }

            name = name.Trim();
            if (name.Length == 0)
            {
                error = Label(index, "请填写流程名称");
                return false;
            }

            if (name.Length > FlowVarRules.NameLength)
            {
                error = Label(index, "名称最长 32");
                return false;
            }

            var stepRows = new List<FlowFileStep>();
            foreach (var step in steps.EnumerateArray())
            {
                if (!TryStep(step, out var row, out var stepError))
                {
                    error = Label(index, stepError);
                    return false;
                }

                stepRows.Add(row);
            }

            var drafts = new List<FlowVarDraft>();
            var varRows = new List<FlowFileVar>();
            foreach (var variable in vars.EnumerateArray())
            {
                if (!TryVar(variable, out var row, out var draft, out var varError))
                {
                    error = Label(index, varError);
                    return false;
                }

                drafts.Add(draft);
                varRows.Add(row);
            }

            var rule = FlowVarRules.Check(drafts, new HashSet<string>(StringComparer.Ordinal));
            if (rule.Length > 0)
            {
                error = Label(index, rule);
                return false;
            }

            flow = new FlowFileFlow
            {
                Name = name,
                IsEnabled = enabled,
                LogOutput = logOutput,
                SortIndex = sortIndex,
                Steps = stepRows,
                Vars = varRows
            };
            error = "";
            return true;
        }

        /// <summary>
        /// 读一个流程步。工具种类必须是已定义的枚举值。
        /// </summary>
        private static bool TryStep(JsonElement item, out FlowFileStep step, out string error)
        {
            step = new FlowFileStep();
            error = "";
            if (item.ValueKind != JsonValueKind.Object || !Known(item, StepNames, out error))
            {
                error = string.IsNullOrEmpty(error) ? "文件不是流程配置" : error;
                return false;
            }

            if (!TryInt(item, "toolKind", out var toolKind) || !TryInt(item, "sortIndex", out var sortIndex))
            {
                error = "文件不是流程配置";
                return false;
            }

            if (!Enum.IsDefined(typeof(FlowToolKind), toolKind))
            {
                error = "工具种类无效";
                return false;
            }

            var title = "";
            if (item.TryGetProperty("title", out var titleProp))
            {
                if (titleProp.ValueKind != JsonValueKind.String)
                {
                    error = "文件不是流程配置";
                    return false;
                }

                title = (titleProp.GetString() ?? "").Trim();
            }

            if (title.Length > FlowVarRules.NameLength)
            {
                error = "标题最长 32";
                return false;
            }

            var cfg = "";
            if (item.TryGetProperty("cfgValue", out var cfgProp))
            {
                if (cfgProp.ValueKind != JsonValueKind.String)
                {
                    error = "文件不是流程配置";
                    return false;
                }

                cfg = cfgProp.GetString() ?? "";
            }

            step = new FlowFileStep
            {
                ToolKind = (FlowToolKind)toolKind,
                Title = title,
                CfgValue = cfg,
                SortIndex = sortIndex
            };
            return true;
        }

        /// <summary>
        /// 读一个变量。名称和类型交给 FlowVarRules。
        /// </summary>
        private static bool TryVar(JsonElement item, out FlowFileVar variable, out FlowVarDraft draft, out string error)
        {
            variable = new FlowFileVar();
            draft = new FlowVarDraft();
            error = "";
            if (item.ValueKind != JsonValueKind.Object || !Known(item, VarNames, out error))
            {
                error = string.IsNullOrEmpty(error) ? "文件不是流程配置" : error;
                return false;
            }

            if (!TryString(item, "name", out var name)
                || !TryString(item, "dataType", out var dataType)
                || !TryInt(item, "sortIndex", out var sortIndex))
            {
                error = "文件不是流程配置";
                return false;
            }

            var initial = "";
            if (item.TryGetProperty("initialValue", out var initialProp))
            {
                if (initialProp.ValueKind != JsonValueKind.String)
                {
                    error = "文件不是流程配置";
                    return false;
                }

                initial = initialProp.GetString() ?? "";
            }

            draft = new FlowVarDraft
            {
                Name = name,
                DataType = dataType.Trim(),
                InitialValue = initial
            };
            variable = new FlowFileVar
            {
                Name = name.Trim(),
                DataType = dataType.Trim(),
                InitialValue = initial,
                SortIndex = sortIndex
            };
            return true;
        }

        /// <summary>
        /// 对象上不能出现约定以外的字段。用来挡住 MES 和其他表。
        /// </summary>
        private static bool Known(JsonElement item, HashSet<string> allowed, out string error)
        {
            foreach (var property in item.EnumerateObject())
            {
                if (allowed.Contains(property.Name))
                    continue;

                error = "文件含有不能导入的内容";
                return false;
            }

            error = "";
            return true;
        }

        /// <summary>
        /// 读取字符串字段。
        /// </summary>
        private static bool TryString(JsonElement item, string name, out string value)
        {
            value = "";
            if (!item.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
                return false;

            value = property.GetString() ?? "";
            return true;
        }

        /// <summary>
        /// 读取布尔字段。不接受 0 和 1。
        /// </summary>
        private static bool TryBool(JsonElement item, string name, out bool value)
        {
            value = false;
            if (!item.TryGetProperty(name, out var property))
                return false;

            if (property.ValueKind == JsonValueKind.True)
            {
                value = true;
                return true;
            }

            if (property.ValueKind == JsonValueKind.False)
                return true;

            return false;
        }

        /// <summary>
        /// 读取整数。小数和超出 int 的数都拒绝。
        /// </summary>
        private static bool TryInt(JsonElement item, string name, out int value)
        {
            value = 0;
            return item.TryGetProperty(name, out var property)
                && property.ValueKind == JsonValueKind.Number
                && property.TryGetInt32(out value);
        }

        /// <summary>
        /// 给错误加上流程序号。
        /// </summary>
        private static string Label(int index, string detail)
        {
            return "第 " + index + " 条流程：" + detail;
        }

        /// <summary>
        /// 写出的根对象。
        /// </summary>
        private sealed class FileBody
        {
            /// <summary>
            /// 文件版本。
            /// </summary>
            public int Version { get; set; }

            /// <summary>
            /// 全部流程。
            /// </summary>
            public List<FileFlow> Flows { get; set; } = [];
        }

        /// <summary>
        /// 写出的一条流程。
        /// </summary>
        private sealed class FileFlow
        {
            /// <summary>
            /// 流程名称。
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// 是否参与运行。
            /// </summary>
            public bool IsEnabled { get; set; }

            /// <summary>
            /// 是否记录这一轮的输出。
            /// </summary>
            public bool LogOutput { get; set; }

            /// <summary>
            /// 列表顺序。
            /// </summary>
            public int SortIndex { get; set; }

            /// <summary>
            /// 流程步。
            /// </summary>
            public List<FileStep> Steps { get; set; } = [];

            /// <summary>
            /// 变量。
            /// </summary>
            public List<FileVar> Vars { get; set; } = [];
        }

        /// <summary>
        /// 写出的一个流程步。
        /// </summary>
        private sealed class FileStep
        {
            /// <summary>
            /// 工具种类数值。
            /// </summary>
            public int ToolKind { get; set; }

            /// <summary>
            /// 步骤名称。
            /// </summary>
            public string Title { get; set; } = "";

            /// <summary>
            /// 工具配置。JSON 文本。
            /// </summary>
            public string CfgValue { get; set; } = "";

            /// <summary>
            /// 步骤顺序。
            /// </summary>
            public int SortIndex { get; set; }
        }

        /// <summary>
        /// 写出的一个变量。
        /// </summary>
        private sealed class FileVar
        {
            /// <summary>
            /// 变量名。
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// 变量表里的类型。
            /// </summary>
            public string DataType { get; set; } = "";

            /// <summary>
            /// 初始值。
            /// </summary>
            public string InitialValue { get; set; } = "";

            /// <summary>
            /// 变量顺序。
            /// </summary>
            public int SortIndex { get; set; }
        }
    }

    /// <summary>
    /// 校验通过的一条流程。
    /// </summary>
    public sealed class FlowFileFlow
    {
        /// <summary>
        /// 名称。已去掉两端空白。
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; init; }

        /// <summary>
        /// 是否输出日志。
        /// </summary>
        public bool LogOutput { get; init; }

        /// <summary>
        /// 列表顺序。
        /// </summary>
        public int SortIndex { get; init; }

        /// <summary>
        /// 流程步。
        /// </summary>
        public IReadOnlyList<FlowFileStep> Steps { get; init; } = [];

        /// <summary>
        /// 变量。
        /// </summary>
        public IReadOnlyList<FlowFileVar> Vars { get; init; } = [];
    }

    /// <summary>
    /// 校验通过的一个流程步。
    /// </summary>
    public sealed class FlowFileStep
    {
        /// <summary>
        /// 工具种类。
        /// </summary>
        public FlowToolKind ToolKind { get; init; }

        /// <summary>
        /// 标题。
        /// </summary>
        public string Title { get; init; } = "";

        /// <summary>
        /// 工具配置原文。
        /// </summary>
        public string CfgValue { get; init; } = "";

        /// <summary>
        /// 执行顺序。
        /// </summary>
        public int SortIndex { get; init; }
    }

    /// <summary>
    /// 校验通过的一个变量。
    /// </summary>
    public sealed class FlowFileVar
    {
        /// <summary>
        /// 名称。已去掉两端空白。
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// 数据类型。
        /// </summary>
        public string DataType { get; init; } = "";

        /// <summary>
        /// 初始文本。
        /// </summary>
        public string InitialValue { get; init; } = "";

        /// <summary>
        /// 显示顺序。
        /// </summary>
        public int SortIndex { get; init; }
    }
}
