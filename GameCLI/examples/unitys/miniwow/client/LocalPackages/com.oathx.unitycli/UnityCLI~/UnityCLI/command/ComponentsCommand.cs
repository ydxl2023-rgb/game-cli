using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `components` command.
    /// </summary>
    internal sealed class ComponentsCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name => "components";

        /// <summary>
        /// Sends the command request to the Unity Editor bridge.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        public async Task<int> ExecuteAsync(CliOptions options)
        {
            var action = (options.Action ?? string.Empty).Trim().ToLowerInvariant();
            if (action != "add" && action != "remove")
            {
                Console.Error.WriteLine("components requires an action: add or remove.");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(options.Component))
            {
                Console.Error.WriteLine("components requires --component <type>.");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(options.AssetPath)
                && options.InstanceId == 0
                && string.IsNullOrWhiteSpace(options.ObjectPath))
            {
                Console.Error.WriteLine("components requires --id <instanceId>, --path <hierarchy>, or --asset <prefab> --path <prefab hierarchy>.");
                return 1;
            }

            if (!string.IsNullOrWhiteSpace(options.AssetPath) && string.IsNullOrWhiteSpace(options.ObjectPath))
            {
                Console.Error.WriteLine("components with --asset requires --path <prefab hierarchy>.");
                return 1;
            }

            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/components", new
            {
                action = action,
                assetPath = options.AssetPath,
                instanceId = options.InstanceId,
                objectPath = options.ObjectPath,
                component = options.Component
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
                        "If you expected \"POST /components\", the Unity Editor is still running an older bridge. " +
                        "Switch focus to Unity, or trigger a recompile so Library/ScriptAssemblies " +
                        "contains the latest com.oathx.unitycli Editor assembly.");
                }
                return;
            }

            var message = root.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : "components command completed";
            Console.WriteLine(message);

            if (!root.TryGetProperty("data", out var data))
            {
                return;
            }

            Console.WriteLine("  action: " + GetString(data, "action"));
            var asset = GetString(data, "assetPath");
            if (!string.IsNullOrEmpty(asset))
            {
                Console.WriteLine("  asset: " + asset);
            }

            Console.WriteLine("  object: " + GetString(data, "objectPath") + " (" + GetInt(data, "instanceId") + ")");

            var requested = GetString(data, "requestedComponent");
            var resolved = GetString(data, "resolvedComponent");
            if (string.Equals(requested, resolved))
            {
                Console.WriteLine("  component: " + resolved);
            }
            else
            {
                Console.WriteLine("  component: " + requested + " -> " + resolved);
            }

            var componentInstanceId = GetInt(data, "componentInstanceId");
            if (componentInstanceId != 0)
            {
                Console.WriteLine("  componentInstanceId: " + componentInstanceId);
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
    }
}
