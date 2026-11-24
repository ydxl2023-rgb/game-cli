using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `call` command.
    /// </summary>
    internal sealed class CallCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name => "call";

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

            var argsJson = string.IsNullOrWhiteSpace(options.InvokeArgsJson) ? "[]" : options.InvokeArgsJson;
            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/call", new
            {
                invokeType = options.InvokeType,
                invokeMember = options.InvokeMember,
                invokeArgsJson = argsJson,
                invokeArgs = options.InvokeArgList.ToArray()
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
            if (string.IsNullOrWhiteSpace(options.InvokeType))
            {
                return "call requires --invoke-type <full CLR type name>.";
            }

            if (string.IsNullOrWhiteSpace(options.InvokeMember))
            {
                return "call requires --invoke-member <method name>.";
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
                if (root.TryGetProperty("error", out var error)
                    && error.TryGetProperty("code", out var code)
                    && code.GetString() == "not_found")
                {
                    Console.Error.WriteLine("If you expected \"POST /call\", reload scripts in the Unity Editor so the bridge includes the latest com.oathx.unitycli Editor assembly.");
                }

                return;
            }

            if (!root.TryGetProperty("data", out var data))
            {
                Console.WriteLine("call completed");
                return;
            }

            Console.WriteLine("call completed");
            Console.WriteLine("  invokeType: " + GetString(data, "invokeType"));
            Console.WriteLine("  invokeMember: " + GetString(data, "invokeMember"));
            Console.WriteLine("  resolved: " + GetString(data, "resolvedMember"));
            Console.WriteLine("  returnType: " + GetString(data, "returnType"));
            if (data.TryGetProperty("returnIsVoid", out var voidEl) && voidEl.GetBoolean())
            {
                Console.WriteLine("  return: (void)");
            }
            else
            {
                Console.WriteLine("  return: " + GetString(data, "returnSummary"));
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
            return element.TryGetProperty(name, out var value) ? value.GetString() ?? string.Empty : string.Empty;
        }
    }
}
