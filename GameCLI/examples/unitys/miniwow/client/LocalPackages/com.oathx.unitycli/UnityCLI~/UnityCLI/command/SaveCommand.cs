using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `save` command.
    /// </summary>
    internal sealed class SaveCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name => "save";

        /// <summary>
        /// Sends the command request to the Unity Editor bridge.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        public async Task<int> ExecuteAsync(CliOptions options)
        {
            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/save", new
            {
                saveScenePath = options.SaveScenePath
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
                if (root.TryGetProperty("error", out var error)
                    && error.TryGetProperty("code", out var code)
                    && code.GetString() == "not_found")
                {
                    Console.Error.WriteLine(
                        "If you expected \"POST /save\", the Unity Editor is still running an older bridge. " +
                        "Switch focus to Unity, or trigger a recompile so Library/ScriptAssemblies " +
                        "contains the latest com.oathx.unitycli Editor assembly.");
                }
                return;
            }

            var message = root.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : "save command completed";
            Console.WriteLine(message);

            if (!root.TryGetProperty("data", out var data))
            {
                return;
            }

            var saved = data.TryGetProperty("scenesSavedCount", out var savedCount) ? savedCount.GetInt32() : 0;
            var skipped = data.TryGetProperty("scenesSkippedCount", out var skippedCount) ? skippedCount.GetInt32() : 0;
            var assets = data.TryGetProperty("assetsSaved", out var assetsEl) && assetsEl.GetBoolean();

            Console.WriteLine("  scenes saved: " + saved);
            Console.WriteLine("  scenes skipped: " + skipped);
            Console.WriteLine("  AssetDatabase.SaveAssets: " + assets);

            if (data.TryGetProperty("scenes", out var scenes) && scenes.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in scenes.EnumerateArray())
                {
                    var name = GetString(entry, "name");
                    var path = GetString(entry, "path");
                    var wasDirty = GetBool(entry, "wasDirty");
                    var entrySaved = GetBool(entry, "saved");
                    var skipReason = GetString(entry, "skipReason");

                    var statusLabel = entrySaved
                        ? "SAVED"
                        : (wasDirty ? "SKIPPED" : "clean");

                    var line = "  - [" + statusLabel + "] " + name +
                               (string.IsNullOrEmpty(path) ? string.Empty : "  (" + path + ")");
                    if (!string.IsNullOrEmpty(skipReason))
                    {
                        line += "  reason=" + skipReason;
                    }
                    Console.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Reads a string property from a JSON response object.
        /// </summary>
        /// <param name="element">The JSON object to inspect.</param>
        /// <param name="name">The JSON property name to read.</param>
        /// <returns>The property value, or an empty string when it is missing.</returns>
        static string GetString(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }

        /// <summary>
        /// Reads a boolean property from a JSON response object.
        /// </summary>
        /// <param name="element">The JSON object to inspect.</param>
        /// <param name="name">The JSON property name to read.</param>
        /// <returns>True when the property exists and is true.</returns>
        static bool GetBool(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value)
                   && value.ValueKind == JsonValueKind.True;
        }
    }
}
