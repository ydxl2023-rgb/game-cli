using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `compile` command.
    /// </summary>
    internal sealed class CompileCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name => "compile";

        /// <summary>
        /// Sends the command request to the Unity Editor bridge.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        public async Task<int> ExecuteAsync(CliOptions options)
        {
            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/compile", new
            {
                compileWait = false
            }, "POST");

            if (options.CompileWait && ResponseWasSuccessful(responseJson))
            {
                responseJson = await WaitForCompilationAsync(options.ProjectPath);
            }

            return ResponseFormatter.WriteJsonOrHuman(responseJson, options.JsonOutput, WriteHuman);
        }

        /// <summary>
        /// Polls the editor for compilation state after a request that may trigger domain reload.
        /// </summary>
        /// <param name="projectPath">The Unity project root, when explicitly supplied.</param>
        /// <returns>The final compile status response.</returns>
        static async Task<string> WaitForCompilationAsync(string? projectPath)
        {
            var deadline = DateTime.UtcNow.AddMinutes(5);
            string lastError = string.Empty;
            await Task.Delay(250);

            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var responseJson = await CLIClient.SendAsync(projectPath, "/compile", new
                    {
                        compileWait = true,
                        compileCheckOnly = true
                    }, "POST");

                    if (!ResponseWasSuccessful(responseJson))
                    {
                        lastError = ReadResponseMessage(responseJson);
                    }
                    else if (!IsCompiling(responseJson))
                    {
                        return responseJson;
                    }
                }
                catch (Exception exception)
                {
                    lastError = exception.Message;
                }

                await Task.Delay(500);
            }

            return JsonSerializer.Serialize(new
            {
                ok = false,
                status = "error",
                message = string.IsNullOrWhiteSpace(lastError)
                    ? "Compilation did not finish within 5 minutes."
                    : "Compilation did not finish within 5 minutes. Last bridge error: " + lastError,
                error = new
                {
                    code = "compile_failed",
                    message = "Compilation did not finish within 5 minutes."
                }
            });
        }

        /// <summary>
        /// Checks whether a bridge response is successful.
        /// </summary>
        /// <param name="responseJson">The raw bridge response JSON.</param>
        /// <returns>True when the response contains ok=true.</returns>
        static bool ResponseWasSuccessful(string responseJson)
        {
            using var document = JsonDocument.Parse(responseJson);
            return document.RootElement.TryGetProperty("ok", out var okElement) && okElement.GetBoolean();
        }

        /// <summary>
        /// Reads the message from a bridge response for retry diagnostics.
        /// </summary>
        /// <param name="responseJson">The raw bridge response JSON.</param>
        /// <returns>The response message, or an empty string.</returns>
        static string ReadResponseMessage(string responseJson)
        {
            using var document = JsonDocument.Parse(responseJson);
            return document.RootElement.TryGetProperty("message", out var message)
                ? message.GetString() ?? string.Empty
                : string.Empty;
        }

        /// <summary>
        /// Reads the compile state from a bridge response.
        /// </summary>
        /// <param name="responseJson">The raw bridge response JSON.</param>
        /// <returns>True when Unity reports that it is still compiling.</returns>
        static bool IsCompiling(string responseJson)
        {
            using var document = JsonDocument.Parse(responseJson);
            if (!document.RootElement.TryGetProperty("data", out var data)
                || !data.TryGetProperty("isCompiling", out var isCompiling)
                || isCompiling.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                throw new InvalidDataException("Compile response is missing a valid isCompiling state.");
            return isCompiling.GetBoolean();
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

            var message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : "compile command completed";
            Console.WriteLine(message);

            if (!root.TryGetProperty("data", out var data))
            {
                return;
            }

            Console.WriteLine("  waited: " + GetBool(data, "waited"));
            Console.WriteLine("  isCompiling: " + GetBool(data, "isCompiling"));
            if (data.TryGetProperty("respondedAtUtc", out var respondedAtUtc))
            {
                Console.WriteLine("  respondedAtUtc: " + respondedAtUtc.GetString());
            }
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
