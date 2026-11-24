using System.IO.Pipes;
using System.Text.Json;
using Oathx.UnityCLI.Protocol;

namespace UnityCLI
{
    /// <summary>
    /// Connects the standalone CLI process to the Unity Editor bridge.
    /// </summary>
    internal static class CLIClient
    {
        /// <summary>
        /// Serializer settings shared with the editor endpoint file and request payloads.
        /// </summary>
        static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Sends a request to the editor bridge and returns the raw JSON response.
        /// </summary>
        /// <param name="projectPath">Optional Unity project root; when omitted the current directory is searched upward.</param>
        /// <param name="path">The bridge route path.</param>
        /// <param name="payload">The command payload to serialize.</param>
        /// <param name="method">The HTTP-like method expected by the bridge route.</param>
        /// <returns>A task that completes with the bridge response JSON.</returns>
        public static async Task<string> SendAsync(string? projectPath, string path, object payload, string method = "GET")
        {
            var endpointPath = FindEndpointPath(projectPath);
            if (string.IsNullOrEmpty(endpointPath))
            {
                throw new InvalidOperationException("UnityCLI endpoint was not found. Open the Unity project so the Editor bridge can start.");
            }

            var endpoint = ReadEndpoint(endpointPath);
            if (string.IsNullOrEmpty(endpoint.pipeName) || string.IsNullOrEmpty(endpoint.token))
            {
                throw new InvalidOperationException("UnityCLI endpoint is incomplete.");
            }

            int seconds = int.TryParse(Environment.GetEnvironmentVariable("UNITYCLI_REQUEST_TIMEOUT_SECONDS"), out int configured) ? Math.Clamp(configured, 5, 300) : 5;
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
            using var pipe = new NamedPipeClientStream(".", endpoint.pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            try
            {
                await pipe.ConnectAsync(timeout.Token);
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new InvalidOperationException("UnityCLI named-pipe connection denied for the current Windows account. Configure this project's Library/UnityCLI/pipe-access.json account allowlist and reload the Editor bridge.", exception);
            }

            var requestValues = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(payload), JsonOptions)
                ?? new Dictionary<string, object>();
            requestValues["method"] = method;
            requestValues["path"] = path;
            requestValues["token"] = endpoint.token;

            var request = JsonSerializer.Serialize(requestValues);
            await CLIPipeProtocol.WriteFrameAsync(pipe, request, timeout.Token);
            return await CLIPipeProtocol.ReadFrameAsync(pipe, timeout.Token);
        }

        /// <summary>
        /// Reads the endpoint metadata published by the Unity Editor.
        /// </summary>
        /// <param name="endpointPath">The endpoint.json file path.</param>
        /// <returns>The parsed endpoint metadata.</returns>
        static CLIEndpoint ReadEndpoint(string endpointPath)
        {
            var json = File.ReadAllText(endpointPath);
            return JsonSerializer.Deserialize<CLIEndpoint>(json, JsonOptions)
                ?? throw new InvalidOperationException("UnityCLI endpoint JSON is invalid.");
        }

        /// <summary>
        /// Finds the endpoint file for an explicit project root or a parent of the current directory.
        /// </summary>
        /// <param name="projectPath">Optional Unity project root.</param>
        /// <returns>The endpoint file path, or null when no live editor endpoint exists.</returns>
        static string? FindEndpointPath(string? projectPath)
        {
            if (!string.IsNullOrWhiteSpace(projectPath))
            {
                var endpoint = Path.Combine(Path.GetFullPath(projectPath), "Library", "UnityCLI", "endpoint.json");
                return File.Exists(endpoint) ? endpoint : null;
            }

            var current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current != null)
            {
                var endpoint = Path.Combine(current.FullName, "Library", "UnityCLI", "endpoint.json");
                if (File.Exists(endpoint))
                {
                    return endpoint;
                }

                current = current.Parent;
            }

            return null;
        }
    }
}
