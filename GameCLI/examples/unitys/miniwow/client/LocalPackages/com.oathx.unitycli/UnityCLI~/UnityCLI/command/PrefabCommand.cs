using System.Text.Json;

namespace UnityCLI;

internal sealed class PrefabCommand : ICommand
{
    public string Name => "prefab";

    public async Task<int> ExecuteAsync(CliOptions options)
    {
        if (options.Whitebox && options.Action != "restore")
        {
            Console.Error.WriteLine("--whitebox is supported only by prefab restore.");
            return 1;
        }
        if (options.Action == "motion-preview" || options.Action == "verify")
        {
            var result = await CLIClient.SendAsync(options.ProjectPath, "/prefab", new
            {
                action = options.Action, prefabPath = options.PrefabPath,
                jsonFile = Path.GetFullPath(options.JsonFile), dryRun = options.DryRun
            }, "POST");
            return ResponseFormatter.WriteJsonOrHuman(result, options.JsonOutput, WriteHuman);
        }
        if (options.Action == "configure-textures")
        {
            if (string.IsNullOrWhiteSpace(options.JsonFile))
            {
                Console.Error.WriteLine("prefab configure-textures requires --json <manifest.json>.");
                return 1;
            }
            var configured = await CLIClient.SendAsync(options.ProjectPath, "/prefab", new
            {
                action = options.Action,
                jsonFile = Path.GetFullPath(options.JsonFile),
                dryRun = options.DryRun
            }, "POST");
            return ResponseFormatter.WriteJsonOrHuman(configured, options.JsonOutput, WriteHuman);
        }
        if (options.Action == "restore" || options.Action == "preview")
        {
            if (string.IsNullOrWhiteSpace(options.JsonFile) || string.IsNullOrWhiteSpace(options.PrefabPath))
            {
                Console.Error.WriteLine("prefab restore requires --json <snapshot.json> --prefab <Assets/...prefab>");
                return 1;
            }
            var restored = await CLIClient.SendAsync(options.ProjectPath, "/prefab", new
            {
                action = options.Action, jsonFile = Path.GetFullPath(options.JsonFile), prefabPath = options.PrefabPath,
                dryRun = options.DryRun, whitebox = options.Whitebox,
                replaceExistingChild = options.ReplaceExistingChild, replaceExistingChildSet = options.ReplaceExistingChildSet
            }, "POST");
            return ResponseFormatter.WriteJsonOrHuman(restored, options.JsonOutput, WriteHuman);
        }
        if (options.Action == "export")
        {
            if (string.IsNullOrWhiteSpace(options.PrefabPath) || string.IsNullOrWhiteSpace(options.ScreenshotPath) || string.IsNullOrWhiteSpace(options.ExportSourceKey))
            {
                Console.Error.WriteLine("prefab export requires --prefab <Assets/file.prefab or folder> --output <empty directory> --source-key <stable project ID>");
                return 1;
            }
            var exported = await CLIClient.SendAsync(options.ProjectPath, "/prefab", new
            {
                action = "export",
                prefabPath = options.PrefabPath,
                screenshotPath = Path.GetFullPath(options.ScreenshotPath),
                exportSourceKey = options.ExportSourceKey
            }, "POST");
            return ResponseFormatter.WriteJsonOrHuman(exported, options.JsonOutput, WriteHuman);
        }
        if (string.Equals(options.Action, "extract", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(options.PrefabPath) || string.IsNullOrWhiteSpace(options.AssetDstPath)
                || string.IsNullOrWhiteSpace(options.ObjectPath))
            {
                Console.Error.WriteLine("prefab extract requires --prefab, --path and --dst.");
                return 1;
            }
            return await ExecuteVisualImportAsync(options);
        }

        if (string.IsNullOrWhiteSpace(options.Action))
        {
            Console.Error.WriteLine("prefab requires an action: create, attach, group, visual-import, or inspect.");
            return 1;
        }

        if (!string.Equals(options.Action, "create", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.Action, "attach", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.Action, "group", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.Action, "visual-import", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.Action, "inspect", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Unsupported prefab action: " + options.Action);
            return 1;
        }

        if (string.Equals(options.Action, "create", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(options.JsonFile) || string.IsNullOrWhiteSpace(options.PrefabPath))
            {
                Console.Error.WriteLine("prefab create requires --json <task-layout.json> and --prefab <Assets/...prefab>.");
                return 1;
            }
            var response = await CLIClient.SendAsync(options.ProjectPath, "/prefab", new
            {
                action = "create",
                jsonFile = options.JsonFile,
                prefabPath = options.PrefabPath,
                dryRun = options.DryRun,
                replaceExistingChild = options.ReplaceExistingChild,
                replaceExistingChildSet = options.ReplaceExistingChildSet
            }, "POST");
            return ResponseFormatter.WriteJsonOrHuman(response, options.JsonOutput, WriteHuman);
        }

        if (string.Equals(options.Action, "inspect", StringComparison.OrdinalIgnoreCase))
        {
            return await ExecuteInspectAsync(options);
        }

        if (string.Equals(options.Action, "visual-import", StringComparison.OrdinalIgnoreCase))
        {
            return await ExecuteVisualImportAsync(options);
        }

        if (string.Equals(options.Action, "group", StringComparison.OrdinalIgnoreCase))
        {
            return await ExecuteGroupAsync(options);
        }

        if (string.IsNullOrWhiteSpace(options.ParentPrefabPath))
        {
            Console.Error.WriteLine("prefab attach requires --parent-prefab.");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(options.ChildPrefabPath))
        {
            Console.Error.WriteLine("prefab attach requires --child-prefab.");
            return 1;
        }

        var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/prefab", new
        {
            action = options.Action,
            parentPrefabPath = options.ParentPrefabPath,
            childPrefabPath = options.ChildPrefabPath,
            parentObjectPath = options.ParentObjectPath,
            childName = options.ChildName,
            localPosition = options.LocalPosition,
            localRotation = options.LocalRotation,
            localScale = options.LocalScale,
            layer = options.Layer,
            keepLayer = options.KeepLayer,
            replaceExistingChild = options.ReplaceExistingChild,
            replaceExistingChildSet = options.ReplaceExistingChildSet,
            dryRun = options.DryRun,
            replaceSceneInstance = options.ReplaceSceneInstance
        }, "POST");

        return ResponseFormatter.WriteJsonOrHuman(responseJson, options.JsonOutput, WriteHuman);
    }

    /// <summary>
    /// 执行只读 Prefab 检查。
    /// </summary>
    /// <param name="options">已解析的命令选项</param>
    /// <returns>命令退出码</returns>
    static async Task<int> ExecuteInspectAsync(CliOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PrefabPath))
        {
            Console.Error.WriteLine("prefab inspect requires --prefab.");
            return 1;
        }

        var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/prefab", new
        {
            action = options.Action,
            prefabPath = options.PrefabPath,
            recursive = options.Recursive,
            prefabProfile = options.PrefabProfile
        }, "POST");

        return ResponseFormatter.WriteJsonOrHuman(responseJson, options.JsonOutput, WriteHuman);
    }

    /// <summary>
    /// Executes the visual prefab import action.
    /// </summary>
    /// <param name="options">The parsed command options.</param>
    /// <returns>The command exit code.</returns>
    static async Task<int> ExecuteVisualImportAsync(CliOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PrefabPath))
        {
            Console.Error.WriteLine("prefab visual-import requires --prefab.");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(options.AssetDstPath))
        {
            Console.Error.WriteLine("prefab visual-import requires --dst.");
            return 1;
        }

        var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/prefab", new
        {
            action = options.Action,
            prefabPath = options.PrefabPath,
            assetDstPath = options.AssetDstPath,
            saveScenePath = options.SaveScenePath,
            objectPath = options.ObjectPath,
            localPosition = options.LocalPosition,
            localRotation = options.LocalRotation,
            localScale = options.LocalScale,
            dryRun = options.DryRun,
            replaceSceneInstance = options.ReplaceSceneInstance
        }, "POST");

        return ResponseFormatter.WriteJsonOrHuman(responseJson, options.JsonOutput, WriteHuman);
    }

    /// <summary>
    /// 执行 prefab group 动作：新建容器节点并把列出的子节点迁入其中。
    /// </summary>
    /// <param name="options">已解析的命令选项。</param>
    /// <returns>命令退出码。</returns>
    static async Task<int> ExecuteGroupAsync(CliOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PrefabPath))
        {
            Console.Error.WriteLine("prefab group requires --prefab.");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(options.GroupName))
        {
            Console.Error.WriteLine("prefab group requires --group-name.");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(options.GroupChildren))
        {
            Console.Error.WriteLine("prefab group requires --children.");
            return 1;
        }

        var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/prefab", new
        {
            action = options.Action,
            prefabPath = options.PrefabPath,
            parentObjectPath = options.ParentObjectPath,
            groupName = options.GroupName,
            groupChildren = options.GroupChildren,
            groupAnchor = options.GroupAnchor,
            groupPivot = options.GroupPivot,
            groupSize = options.GroupSize,
            localPosition = options.LocalPosition,
            groupSiblingIndex = options.GroupSiblingIndex,
            dryRun = options.DryRun,
            replaceSceneInstance = options.ReplaceSceneInstance
        }, "POST");

        return ResponseFormatter.WriteJsonOrHuman(responseJson, options.JsonOutput, WriteHuman);
    }

    static void WriteHuman(JsonElement root)
    {
        var ok = root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean();
        if (!ok)
        {
            ResponseFormatter.WriteError(root);
        }

        if (ok)
        {
            Console.WriteLine(root.TryGetProperty("message", out var message) ? message.GetString() : "prefab command completed");
        }
        if (!root.TryGetProperty("data", out var data))
        {
            return;
        }
        WriteString(data, "action");
        WriteString(data, "prefabPath");
        WriteString(data, "profile");
        WriteString(data, "parentPrefabPath");
        WriteString(data, "parentObjectPath");
        WriteString(data, "childPrefabPath");
        WriteString(data, "childName");
        WriteString(data, "groupName");
        WriteString(data, "movedChildren");
        WriteString(data, "groupRect");
        WriteString(data, "sourcePrefabPath");
        WriteString(data, "outputPrefabPath");
        WriteString(data, "scenePath");
        WriteString(data, "sceneRootName");
        WriteString(data, "localPosition");
        WriteString(data, "localRotation");
        WriteString(data, "localScale");
        WriteString(data, "layerName");
        WriteString(data, "layerMode");
        WriteBool(data, "replaceExistingChild");
        WriteBool(data, "recursive");
        WriteBool(data, "valid");
        WriteBool(data, "dryRun");
        WriteBool(data, "prefabModified");
        WriteBool(data, "sceneInstanceReplaced");
        WriteInt(data, "missingScriptRemovedCount");
        WriteInt(data, "monoBehaviourRemovedCount");
        WriteInt(data, "materialRepairedCount");
        WriteInt(data, "prefabUnpackedCount");
        WriteInt(data, "nodeCount");
        WriteInt(data, "componentCount");
        WriteInt(data, "objectReferenceCount");
        WriteInt(data, "missingScriptCount");
        WriteInt(data, "missingReferenceCount");
        WriteInt(data, "movedChildCount");

        if (data.TryGetProperty("nodes", out var nodes) && nodes.ValueKind == JsonValueKind.Array)
        {
            foreach (var node in nodes.EnumerateArray())
            {
                var path = node.TryGetProperty("path", out var pathValue) ? pathValue.GetString() : string.Empty;
                var componentCount = node.TryGetProperty("components", out var components) && components.ValueKind == JsonValueKind.Array
                    ? components.GetArrayLength()
                    : 0;
                Console.WriteLine("  node: " + path + " (components=" + componentCount + ")");
                WriteString(node, "rectTransform", "    ");
            }
        }

        if (data.TryGetProperty("issues", out var issues) && issues.ValueKind == JsonValueKind.Array)
        {
            foreach (var issue in issues.EnumerateArray())
            {
                Console.WriteLine("  issue: " + (issue.GetString() ?? string.Empty));
            }
        }
    }

    static void WriteString(JsonElement data, string name)
    {
        WriteString(data, name, "  ");
    }

    static void WriteString(JsonElement data, string name, string indent)
    {
        if (data.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            if (!string.IsNullOrEmpty(text)) Console.WriteLine(indent + name + ": " + text);
        }
    }

    static void WriteBool(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value)
            && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
        {
            Console.WriteLine("  " + name + ": " + value.GetBoolean());
        }
    }

    /// <summary>
    /// Writes an integer data property when it exists.
    /// </summary>
    /// <param name="data">The data object.</param>
    /// <param name="name">The property name.</param>
    static void WriteInt(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number)
        {
            Console.WriteLine("  " + name + ": " + value.GetInt32());
        }
    }
}
