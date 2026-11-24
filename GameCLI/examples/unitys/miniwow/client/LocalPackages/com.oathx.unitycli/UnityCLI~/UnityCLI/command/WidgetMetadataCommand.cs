using System.Text.Json;

namespace UnityCLI;

/// <summary>
/// Implements the standalone CLI `widget-metadata` command.
/// </summary>
internal sealed class WidgetMetadataCommand : ICommand
{
    /// <summary>
    /// Gets the command name.
    /// </summary>
    public string Name => "widget-metadata";

    /// <summary>
    /// Sends the command request to the Unity Editor bridge.
    /// </summary>
    /// <param name="options">The parsed command-line options.</param>
    /// <returns>A task that completes with the process exit code.</returns>
    public async Task<int> ExecuteAsync(CliOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.UiName) && string.IsNullOrWhiteSpace(options.VmName))
        {
            Console.Error.WriteLine("widget-metadata requires a UIName such as UIHome or --vm <VMName>.");
            return 1;
        }

        if (options.ValidateExisting && string.IsNullOrWhiteSpace(options.PrefabPath))
        {
            Console.Error.WriteLine("widget-metadata --validate-existing requires --prefab.");
            return 1;
        }

        var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/widget-metadata", new
        {
            uiName = options.UiName,
            vmName = options.VmName,
            prefabPath = options.PrefabPath,
            objectPath = options.ObjectPath,
            dryRun = options.DryRun,
            validateExisting = options.ValidateExisting
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
        }

        var message = root.TryGetProperty("message", out var messageElement)
            ? messageElement.GetString()
            : "widget metadata command completed";
        if (ok) Console.WriteLine(message);

        if (!root.TryGetProperty("data", out var data))
        {
            return;
        }

        WriteString(data, "prefabPath");
        WriteString(data, "uiName");
        WriteString(data, "vmName");
        WriteString(data, "widgetName");
        WriteString(data, "expectedClassName");
        WriteString(data, "expectedClassGuid");
        WriteString(data, "expectedScriptPath");
        WriteString(data, "actualClassName");
        WriteString(data, "actualClassGuid");
        WriteString(data, "actualScriptPath");
        WriteString(data, "actualScriptClassName");
        WriteInt(data, "metadataComponentCount");
        WriteInt(data, "metadataCount");
        WriteInt(data, "boundTargetCount");
        WriteInt(data, "missingReferenceCount");
        WriteBool(data, "dryRun");

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
        if (data.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            if (!string.IsNullOrEmpty(text))
            {
                Console.WriteLine("  " + name + ": " + text);
            }
        }
    }

    static void WriteInt(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value) && value.TryGetInt32(out var number))
        {
            Console.WriteLine("  " + name + ": " + number);
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
}
