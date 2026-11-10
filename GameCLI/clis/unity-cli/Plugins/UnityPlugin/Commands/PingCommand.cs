using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `ping` command.
    /// </summary>
    internal sealed class PingCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name => "ping";

        /// <summary>
        /// Sends the command request to the Unity Editor bridge.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        public async Task<int> ExecuteAsync(CliOptions options)
        {
            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/ping", new { });
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

            var message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : string.Empty;
            Console.WriteLine(message);

            if (!root.TryGetProperty("data", out var data))
            {
                return;
            }

            if (data.TryGetProperty("respondedAtUtc", out var respondedAtUtc))
            {
                Console.WriteLine("time: " + respondedAtUtc.GetString());
            }

            if (data.TryGetProperty("endpoint", out var endpoint)
                && endpoint.TryGetProperty("pipeName", out var pipeName))
            {
                Console.WriteLine("pipe: " + pipeName.GetString());
            }

            if (data.TryGetProperty("supportedRoutes", out var supportedRoutes)
                && supportedRoutes.ValueKind == JsonValueKind.String)
            {
                var routes = supportedRoutes.GetString();
                if (!string.IsNullOrEmpty(routes))
                {
                    Console.WriteLine("routes: " + routes);
                }
            }
        }
    }
}
