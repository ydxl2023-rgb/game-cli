using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `screenshot` command that captures the Unity Game view to a PNG file.
    /// </summary>
    internal sealed class ScreenshotCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name => "screenshot";

        /// <summary>
        /// Sends the command request to the Unity Editor bridge.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        public async Task<int> ExecuteAsync(CliOptions options)
        {
            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/screenshot", new
            {
                screenshotPath = FirstNonEmpty(options.ScreenshotPath, options.AssetDstPath),
                screenshotSuperSize = options.ScreenshotSuperSize
            }, "POST");

            return ResponseFormatter.WriteJsonOrHuman(responseJson, options.JsonOutput, WriteHuman);
        }

        /// <summary>
        /// Writes the human-readable command output.
        /// </summary>
        /// <param name="root">The response root element.</param>
        static void WriteHuman(JsonElement root)
        {
            var ok = root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean();
            if (!ok)
            {
                ResponseFormatter.WriteError(root);
                return;
            }

            var message = root.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : "screenshot captured";
            Console.WriteLine(message);

            if (!root.TryGetProperty("data", out var data))
            {
                return;
            }

            WriteString(data, "path");
            WriteString(data, "projectRelativePath");
            WriteString(data, "captureMethod");
            WriteInt(data, "width");
            WriteInt(data, "height");
            WriteInt64(data, "bytes");
            WriteInt(data, "superSize");
        }

        /// <summary>
        /// Writes a string property when it is present.
        /// </summary>
        /// <param name="data">The response data element.</param>
        /// <param name="name">The property name.</param>
        static void WriteString(JsonElement data, string name)
        {
            if (data.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrEmpty(text)) Console.WriteLine("  " + name + ": " + text);
            }
        }

        /// <summary>
        /// Writes an integer property when it is present.
        /// </summary>
        /// <param name="data">The response data element.</param>
        /// <param name="name">The property name.</param>
        static void WriteInt(JsonElement data, string name)
        {
            if (data.TryGetProperty(name, out var value) && value.TryGetInt32(out var number))
            {
                Console.WriteLine("  " + name + ": " + number);
            }
        }

        /// <summary>
        /// Writes a long integer property when it is present.
        /// </summary>
        /// <param name="data">The response data element.</param>
        /// <param name="name">The property name.</param>
        static void WriteInt64(JsonElement data, string name)
        {
            if (data.TryGetProperty(name, out var value) && value.TryGetInt64(out var number))
            {
                Console.WriteLine("  " + name + ": " + number);
            }
        }

        /// <summary>
        /// Returns the first non-empty value from two candidates.
        /// </summary>
        /// <param name="first">The preferred value.</param>
        /// <param name="second">The fallback value.</param>
        /// <returns>The first non-empty value.</returns>
        static string FirstNonEmpty(string first, string second)
        {
            return string.IsNullOrWhiteSpace(first) ? (second ?? string.Empty) : first;
        }
    }
}
