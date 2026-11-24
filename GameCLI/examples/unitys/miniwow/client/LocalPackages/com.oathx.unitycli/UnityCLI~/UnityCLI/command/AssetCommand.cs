using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `asset` command.
    /// </summary>
    internal sealed class AssetCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name => "asset";

        /// <summary>
        /// Sends the command request to the Unity Editor bridge.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        public async Task<int> ExecuteAsync(CliOptions options)
        {
            var action = (options.Action ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(action))
            {
                Console.Error.WriteLine(
                    "asset requires an action: search | create-folder | import | delete | move | copy.");
                return 1;
            }

            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/asset", new
            {
                action = action,
                search = options.Search,
                type = options.Type,
                count = options.Count,
                assetSrcPath = options.AssetSrcPath,
                assetDstPath = options.AssetDstPath
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
                        "If you expected \"POST /asset\", the Unity Editor is still running an older bridge. " +
                        "Switch focus to Unity, or trigger a recompile so Library/ScriptAssemblies " +
                        "contains the latest com.oathx.unitycli Editor assembly.");
                }
                return;
            }

            var message = root.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : "asset command completed";
            Console.WriteLine(message);

            if (!root.TryGetProperty("data", out var data))
            {
                return;
            }

            Console.WriteLine("  action: " + GetString(data, "action"));

            var src = GetString(data, "srcPath");
            var dst = GetString(data, "dstPath");
            var filter = GetString(data, "filter");
            if (filter.Length > 0) Console.WriteLine("  filter: \"" + filter + "\"");
            if (src.Length > 0)    Console.WriteLine("  src: " + src);
            if (dst.Length > 0)    Console.WriteLine("  dst: " + dst);

            var totalMatched = GetInt(data, "totalMatched");
            var returnedCount = GetInt(data, "returnedCount");
            if (totalMatched > 0 || returnedCount > 0)
            {
                Console.WriteLine("  matched: " + returnedCount + " of " + totalMatched);
            }

            if (data.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in assets.EnumerateArray())
                {
                    var path = GetString(entry, "path");
                    var typeName = GetString(entry, "mainAssetType");
                    var guid = GetString(entry, "guid");
                    var isFolder = GetBool(entry, "isFolder");

                    var prefix = isFolder ? "[D]" : "[F]";
                    var line = "  " + prefix + " " + path +
                               (typeName.Length > 0 ? "  (" + typeName + ")" : string.Empty) +
                               (guid.Length > 0 ? "  guid=" + guid : string.Empty);
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
        /// Reads an integer property from a JSON response object.
        /// </summary>
        /// <param name="element">The JSON object to inspect.</param>
        /// <param name="name">The JSON property name to read.</param>
        /// <returns>The property value, or zero when it is missing.</returns>
        static int GetInt(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
                ? value.GetInt32()
                : 0;
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
