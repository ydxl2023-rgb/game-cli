using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `modify` command.
    /// </summary>
    internal sealed class ModifyCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name => "modify";

        /// <summary>
        /// Sends the command request to the Unity Editor bridge.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        public async Task<int> ExecuteAsync(CliOptions options)
        {
            var error = Validate(options);
            if (!string.IsNullOrEmpty(error))
            {
                Console.Error.WriteLine(error);
                return 1;
            }

            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/modify", new
            {
                assetPath = options.AssetPath,
                instanceId = options.InstanceId,
                objectPath = options.ObjectPath,
                component = options.Component,
                property = options.Property,
                value = options.Value
            }, "POST");
            return ResponseFormatter.WriteJsonOrHuman(responseJson, options.JsonOutput, WriteHuman);
        }

        /// <summary>
        /// Validates command-specific required options.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>An error message, or an empty string when the options are valid.</returns>
        static string Validate(CliOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.AssetPath)
                && options.InstanceId == 0
                && string.IsNullOrWhiteSpace(options.ObjectPath))
            {
                return "modify requires --id <id>, --path <hierarchy path>, or --asset <prefab> --path <prefab hierarchy path>.";
            }

            if (!string.IsNullOrWhiteSpace(options.AssetPath) && string.IsNullOrWhiteSpace(options.ObjectPath))
            {
                return "modify with --asset requires --path <prefab hierarchy path>.";
            }

            if (string.IsNullOrWhiteSpace(options.Component))
            {
                return "modify requires --component <type>.";
            }

            if (string.IsNullOrWhiteSpace(options.Property))
            {
                return "modify requires --property <SerializedProperty path>.";
            }

            if (string.IsNullOrEmpty(options.Value))
            {
                return "modify requires --value <value>.";
            }

            return string.Empty;
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

            if (!root.TryGetProperty("data", out var data))
            {
                Console.WriteLine("modified");
                return;
            }

            Console.WriteLine("modified");
            var asset = GetString(data, "assetPath");
            if (!string.IsNullOrEmpty(asset))
            {
                Console.WriteLine("  asset: " + asset);
            }

            Console.WriteLine("  object: " + GetString(data, "objectPath") + " (" + GetInt(data, "instanceId") + ")");
            Console.WriteLine("  component: " + GetString(data, "component"));
            Console.WriteLine("  property: " + GetString(data, "property"));
            Console.WriteLine("  old: " + GetString(data, "oldValue"));
            Console.WriteLine("  new: " + GetString(data, "newValue"));
        }

        /// <summary>
        /// Reads a string property from a JSON response object.
        /// </summary>
        /// <param name="element">The JSON object to inspect.</param>
        /// <param name="name">The JSON property name to read.</param>
        /// <returns>The property value, or an empty string when it is missing.</returns>
        static string GetString(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) ? value.GetString() ?? string.Empty : string.Empty;
        }

        /// <summary>
        /// Reads an integer property from a JSON response object.
        /// </summary>
        /// <param name="element">The JSON object to inspect.</param>
        /// <param name="name">The JSON property name to read.</param>
        /// <returns>The property value, or zero when it is missing.</returns>
        static int GetInt(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) ? value.GetInt32() : 0;
        }
    }
}
