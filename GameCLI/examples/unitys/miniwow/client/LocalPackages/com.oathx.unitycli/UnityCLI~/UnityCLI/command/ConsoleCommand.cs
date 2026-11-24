using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `console` command.
    /// </summary>
    internal sealed class ConsoleCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name => "console";

        /// <summary>
        /// Sends the command request to the Unity Editor bridge.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        public async Task<int> ExecuteAsync(CliOptions options)
        {
            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/console", new
            {
                count = options.Count,
                type = options.Type,
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
                || !data.TryGetProperty("entries", out var entries))
            {
                Console.WriteLine("no console entries");
                return;
            }

            foreach (var entry in entries.EnumerateArray())
            {
                var type = entry.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : "log";
                var message = entry.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : string.Empty;
                var stackTrace = entry.TryGetProperty("stackTrace", out var stackTraceElement) ? stackTraceElement.GetString() : string.Empty;
                var file = entry.TryGetProperty("file", out var fileElement) ? fileElement.GetString() : string.Empty;
                var line = entry.TryGetProperty("line", out var lineElement) ? lineElement.GetInt32() : 0;

                Console.WriteLine("[" + type + "] " + message);
                if (!string.IsNullOrEmpty(stackTrace))
                {
                    Console.WriteLine(stackTrace);
                }

                if (!string.IsNullOrEmpty(file))
                {
                    Console.WriteLine("  at " + file + ":" + line);
                }
            }
        }
    }
}
