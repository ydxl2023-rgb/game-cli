using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `search` command.
    /// </summary>
    internal sealed class SearchCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name => "search";

        /// <summary>
        /// Sends the command request to the Unity Editor bridge.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        public async Task<int> ExecuteAsync(CliOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Search))
            {
                Console.Error.WriteLine("search requires a GameObject name or pattern.");
                return 1;
            }

            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/search", new
            {
                count = options.Count,
                search = options.Search
            });
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

            if (!root.TryGetProperty("data", out var data)
                || !data.TryGetProperty("objects", out var objects))
            {
                Console.WriteLine("no GameObjects found");
                return;
            }

            foreach (var gameObject in objects.EnumerateArray())
            {
                var name = GetString(gameObject, "name");
                var path = GetString(gameObject, "path");
                var scene = GetString(gameObject, "sceneName");
                var instanceId = GetInt(gameObject, "instanceId");
                var activeSelf = GetBool(gameObject, "activeSelf");
                var activeInHierarchy = GetBool(gameObject, "activeInHierarchy");

                Console.WriteLine(name + " (" + instanceId + ")");
                Console.WriteLine("  path: " + path);
                Console.WriteLine("  scene: " + scene);
                Console.WriteLine("  activeSelf: " + activeSelf + ", activeInHierarchy: " + activeInHierarchy);

                if (gameObject.TryGetProperty("components", out var components))
                {
                    Console.WriteLine("  components:");
                    foreach (var component in components.EnumerateArray())
                    {
                        var type = GetString(component, "type");
                        var enabled = component.TryGetProperty("hasEnabled", out var hasEnabledElement)
                            && hasEnabledElement.GetBoolean()
                            && component.TryGetProperty("enabled", out var enabledElement)
                            ? ", enabled: " + enabledElement.GetBoolean()
                            : string.Empty;
                        Console.WriteLine("    - " + type + " instanceId=" + GetInt(component, "instanceId") + enabled);
                        if (component.TryGetProperty("transform", out var transform))
                        {
                            WriteTransform(transform);
                        }

                        if (component.TryGetProperty("properties", out var properties))
                        {
                            WriteProperties(properties);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes the serialized Transform details for a search result.
        /// </summary>
        /// <param name="transform">The Transform JSON object to print.</param>
        static void WriteTransform(JsonElement transform)
        {
            Console.WriteLine("      transform:");
            Console.WriteLine("        localPosition: " + FormatVector3(transform, "localPosition"));
            Console.WriteLine("        localRotation: " + FormatQuaternion(transform, "localRotation"));
            Console.WriteLine("        localEulerAngles: " + FormatVector3(transform, "localEulerAngles"));
            Console.WriteLine("        localScale: " + FormatVector3(transform, "localScale"));
            Console.WriteLine("        position: " + FormatVector3(transform, "position"));
            Console.WriteLine("        rotation: " + FormatQuaternion(transform, "rotation"));
            Console.WriteLine("        eulerAngles: " + FormatVector3(transform, "eulerAngles"));
            Console.WriteLine("        lossyScale: " + FormatVector3(transform, "lossyScale"));
        }

        /// <summary>
        /// Writes serialized inspector properties for one component.
        /// </summary>
        /// <param name="properties">The inspector property array to print.</param>
        static void WriteProperties(JsonElement properties)
        {
            foreach (var property in properties.EnumerateArray())
            {
                var displayName = GetString(property, "displayName");
                var path = GetString(property, "path");
                var type = GetString(property, "type");
                var value = GetString(property, "value");
                var label = string.IsNullOrEmpty(displayName) ? path : displayName;
                Console.WriteLine("      " + label + " [" + type + "]: " + value);
            }
        }

        /// <summary>
        /// Formats a JSON Vector3 property as a compact tuple.
        /// </summary>
        /// <param name="parent">The JSON object that owns the nested property.</param>
        /// <param name="name">The JSON property name to read.</param>
        /// <returns>A formatted tuple, or an empty string when the property is missing.</returns>
        static string FormatVector3(JsonElement parent, string name)
        {
            if (!parent.TryGetProperty(name, out var value))
            {
                return string.Empty;
            }

            return "(" + GetFloat(value, "x") + ", " + GetFloat(value, "y") + ", " + GetFloat(value, "z") + ")";
        }

        /// <summary>
        /// Formats a JSON Quaternion property as a compact tuple.
        /// </summary>
        /// <param name="parent">The JSON object that owns the nested property.</param>
        /// <param name="name">The JSON property name to read.</param>
        /// <returns>A formatted tuple, or an empty string when the property is missing.</returns>
        static string FormatQuaternion(JsonElement parent, string name)
        {
            if (!parent.TryGetProperty(name, out var value))
            {
                return string.Empty;
            }

            return "(" + GetFloat(value, "x") + ", " + GetFloat(value, "y") + ", " + GetFloat(value, "z") + ", " + GetFloat(value, "w") + ")";
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
        /// Reads a floating-point property from a JSON response object.
        /// </summary>
        /// <param name="element">The JSON object to inspect.</param>
        /// <param name="name">The JSON property name to read.</param>
        /// <returns>The property value, or zero when it is missing.</returns>
        static float GetFloat(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) ? value.GetSingle() : 0f;
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

        /// <summary>
        /// Reads a boolean property from a JSON response object.
        /// </summary>
        /// <param name="element">The JSON object to inspect.</param>
        /// <param name="name">The JSON property name to read.</param>
        /// <returns>True when the property exists and is true.</returns>
        static bool GetBool(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) && value.GetBoolean();
        }
    }
}
