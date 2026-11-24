using System.Text.Json;

namespace UnityCLI;

/// <summary>
/// Implements the standalone CLI `ugui` command that builds UGUI prefabs from visual UI data.
/// </summary>
internal sealed class UguiCommand : ICommand
{
    /// <summary>
    /// Gets the command name.
    /// </summary>
    public string Name => "ugui";

    /// <summary>
    /// Sends the command request to the Unity Editor bridge.
    /// </summary>
    /// <param name="options">The parsed command-line options.</param>
    /// <returns>A task that completes with the process exit code.</returns>
    public async Task<int> ExecuteAsync(CliOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.UiName) && string.IsNullOrWhiteSpace(options.JsonFile))
        {
            Console.Error.WriteLine("ugui requires a UIName such as UILogin or --json <visual-ui.json>.");
            return 1;
        }

        var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/ugui", new
        {
            uiName = options.UiName,
            jsonFile = string.IsNullOrWhiteSpace(options.JsonFile) ? options.JsonFile : Path.GetFullPath(options.JsonFile),
            prefabPath = options.PrefabPath,
            assetManifestPath = string.IsNullOrWhiteSpace(options.AssetManifestPath) ? options.AssetManifestPath : Path.GetFullPath(options.AssetManifestPath),
            screenshotPath = string.IsNullOrWhiteSpace(options.ScreenshotPath) ? options.ScreenshotPath : Path.GetFullPath(options.ScreenshotPath),
            replaceExistingChild = options.ReplaceExistingChild,
            replaceExistingChildSet = options.ReplaceExistingChildSet,
            dryRun = options.DryRun,
            replaceSceneInstance = options.ReplaceSceneInstance
        }, "POST");

        return ResponseFormatter.WriteJsonOrHuman(responseJson, options.JsonOutput, WriteHuman);
    }

    /// <summary>
    /// Writes the human-readable command output.
    /// </summary>
    /// <param name="root">The parsed bridge response root.</param>
    static void WriteHuman(JsonElement root)
    {
        var ok = root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean();
        if (!ok)
        {
            ResponseFormatter.WriteError(root);
            return;
        }

        var message = root.TryGetProperty("message", out var messageElement)
            ? messageElement.GetString()
            : "ugui prefab workflow completed";
        Console.WriteLine(message);

        if (!root.TryGetProperty("data", out var data))
        {
            return;
        }

        WriteString(data, "uiName");
        WriteString(data, "jsonFile");
        WriteString(data, "assetManifestPath");
        WriteString(data, "prefabPath");
        WriteString(data, "buildReportPath");
        WriteString(data, "assetsFolder");
        WriteInt(data, "nodeCount");
        WriteInt(data, "assetsCopied");
        WriteInt(data, "spritesReferenced");
        WriteBool(data, "dryRun");
        WriteBool(data, "sceneInstanceReplaced");

        if (data.TryGetProperty("issues", out var issues) && issues.ValueKind == JsonValueKind.Array)
        {
            foreach (var issue in issues.EnumerateArray())
            {
                Console.WriteLine("  issue: " + (issue.GetString() ?? string.Empty));
            }
        }
    }

    /// <summary>
    /// Writes a string response property when it is present.
    /// </summary>
    /// <param name="data">The command response data object.</param>
    /// <param name="name">The JSON property name to read.</param>
    static void WriteString(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            if (!string.IsNullOrEmpty(text)) Console.WriteLine("  " + name + ": " + text);
        }
    }

    /// <summary>
    /// Writes an integer response property when it is present.
    /// </summary>
    /// <param name="data">The command response data object.</param>
    /// <param name="name">The JSON property name to read.</param>
    static void WriteInt(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value) && value.TryGetInt32(out var number))
        {
            Console.WriteLine("  " + name + ": " + number);
        }
    }

    /// <summary>
    /// Writes a boolean response property when it is present.
    /// </summary>
    /// <param name="data">The command response data object.</param>
    /// <param name="name">The JSON property name to read.</param>
    static void WriteBool(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value)
            && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
        {
            Console.WriteLine("  " + name + ": " + value.GetBoolean());
        }
    }
}
