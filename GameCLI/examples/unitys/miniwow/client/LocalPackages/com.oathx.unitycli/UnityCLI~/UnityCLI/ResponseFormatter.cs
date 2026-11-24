using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Formats bridge responses for the CLI process.
    /// </summary>
    internal static class ResponseFormatter
    {
        /// <summary>
        /// Writes either raw JSON or command-specific human-readable output.
        /// </summary>
        /// <param name="responseJson">The raw response JSON returned by the bridge.</param>
        /// <param name="jsonOutput">True to print raw JSON instead of formatted text.</param>
        /// <param name="writeHuman">The command-specific formatter for human-readable output.</param>
        /// <returns>Zero when the bridge response reports success; otherwise one.</returns>
        public static int WriteJsonOrHuman(string responseJson, bool jsonOutput, Action<JsonElement> writeHuman)
        {
            if (jsonOutput)
            {
                Console.WriteLine(responseJson);
            }
            else
            {
                using var document = JsonDocument.Parse(responseJson);
                writeHuman(document.RootElement);
            }

            return IsOk(responseJson) ? 0 : 1;
        }

        /// <summary>
        /// Reads the common response success flag.
        /// </summary>
        /// <param name="responseJson">The raw response JSON returned by the bridge.</param>
        /// <returns>True when the response contains <c>ok: true</c>.</returns>
        public static bool IsOk(string responseJson)
        {
            using var document = JsonDocument.Parse(responseJson);
            return document.RootElement.TryGetProperty("ok", out var okElement) && okElement.GetBoolean();
        }

        /// <summary>
        /// Writes a standard error response in human-readable mode.
        /// </summary>
        /// <param name="root">The parsed response root element.</param>
        public static void WriteError(JsonElement root)
        {
            Console.WriteLine("error");
            if (root.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var errorMessage))
            {
                Console.Error.WriteLine(errorMessage.GetString());
            }
        }
    }
}
