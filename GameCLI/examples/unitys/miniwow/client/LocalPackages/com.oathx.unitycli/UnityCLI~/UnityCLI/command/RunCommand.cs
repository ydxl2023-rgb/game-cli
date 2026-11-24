using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `run` command.
    /// </summary>
    internal sealed class RunCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name => "run";

        /// <summary>
        /// Sends the command request to the Unity Editor bridge.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        public async Task<int> ExecuteAsync(CliOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Action))
            {
                Console.Error.WriteLine("run requires an action: play, pause, resume, or stop.");
                return 1;
            }

            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/run", new
            {
                action = options.Action
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
                    Console.Error.WriteLine("If you expected \"POST /run\", the Unity Editor is still running an older bridge. Save scripts, switch focus to Unity, or trigger a recompile so Library/ScriptAssemblies contains the latest com.oathx.unitycli Editor assembly.");
                }

                return;
            }

            var message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : "run command completed";
            Console.WriteLine(message);

            if (!root.TryGetProperty("data", out var data))
            {
                return;
            }

            Console.WriteLine("  action: " + GetString(data, "action"));
            Console.WriteLine("  isPlaying: " + GetBool(data, "isPlaying"));
            Console.WriteLine("  isPaused: " + GetBool(data, "isPaused"));
            Console.WriteLine("  isPlayingOrWillChangePlaymode: " + GetBool(data, "isPlayingOrWillChangePlaymode"));
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
