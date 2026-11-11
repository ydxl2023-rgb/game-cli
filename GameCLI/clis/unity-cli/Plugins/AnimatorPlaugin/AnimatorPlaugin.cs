using System.Globalization;
using System.Text.Json;
using UnityCLI.Abstractions;

namespace UnityCLI;

/// <summary>Edits UI animation assets through the Unity Editor animator bridge.</summary>
public sealed class AnimatorPlaugin : IPlugin
{
    public string Name => "AnimatorPlaugin";
    public IReadOnlyCollection<string> Commands => new[] { "animator" };
    public string Help => """
        animator controller create|import-json --controller Assets/...controller [--input file.json]
        animator clip create|set-curve|sample|import-json|export-json --clip Assets/...anim
        animator parameter add|set|remove --controller ... --parameter name [--type Float|Int|Bool|Trigger] [--default value]
        animator state add --controller ... --state name [--clip ...] [--layer name]
        animator transition add --controller ... --from name --to name [--condition "Param Greater 0.1"]
        animator bind --path "scene/object" --controller ...
        animator inspect --controller ...|--clip ...
        Curve: --path relative/child --component UnityEngine.RectTransform --property m_AnchoredPosition.x --keys 0:0,1:100
        Clip: --duration seconds --loop true|false; sample: --path scene/object --time seconds
        Common: --project path --dry-run --json; import-json: --input file.json
        """;

    /// <summary>Parses animator options and sends a single Editor request.</summary>
    /// <param name="command">Registered command name.</param>
    /// <param name="args">Action, operation and options.</param>
    /// <returns>Zero on success, one on bridge failure, two on invalid arguments.</returns>
    public async Task<int> ExecuteAsync(string command, string[] args)
    {
        bool json = args.Contains("--json");
        try
        {
            if (args.Contains("--help") || args.Contains("-h")) { Console.WriteLine(Help); return 0; }
            if (args.Length == 0) throw new ArgumentException(Help);
            var operations = new Dictionary<string, string[]> {
                ["controller"] = new[] { "create", "import-json" },
                ["clip"] = new[] { "create", "set-curve", "sample", "import-json", "export-json" },
                ["parameter"] = new[] { "add", "set", "remove" },
                ["state"] = new[] { "add" }, ["transition"] = new[] { "add" },
                ["bind"] = Array.Empty<string>(), ["inspect"] = Array.Empty<string>() };
            if (!operations.TryGetValue(args[0], out var allowed)) throw new ArgumentException("Unknown animator action.");
            var payload = new Dictionary<string, object> { ["action"] = args[0] };
            int index = 1;
            if (allowed.Length > 0) {
                if (args.Length < 2 || !allowed.Contains(args[1])) throw new ArgumentException("Expected operation: " + string.Join(", ", allowed));
                payload["animatorOperation"] = args[index++];
            }
            var names = new Dictionary<string, string> {
                ["--controller"]="animatorControllerPath", ["--clip"]="animationClipPath", ["--path"]="objectPath",
                ["--component"]="component", ["--property"]="property", ["--layer"]="animatorLayer",
                ["--parameter"]="animatorParameterName", ["--type"]="animatorParameterType", ["--default"]="animatorDefaultValue",
                ["--state"]="animatorStateName", ["--from"]="animatorFromState", ["--to"]="animatorToState",
                ["--condition"]="animatorCondition", ["--keys"]="animatorKeys", ["--input"]="jsonFile" };
            string? project = null;
            var seen = new HashSet<string>();
            for (; index < args.Length; index++) {
                string option = args[index];
                if (!seen.Add(option)) throw new ArgumentException("Duplicate option: " + option);
                if (option == "--json") continue;
                if (option == "--dry-run") { payload["dryRun"] = true; continue; }
                if (++index >= args.Length || args[index].StartsWith("--")) throw new ArgumentException("Missing value: " + option);
                string value = args[index];
                if (option == "--project") { project = value; continue; }
                if (option == "--loop") {
                    if (!bool.TryParse(value, out var loop)) throw new ArgumentException("--loop requires true or false.");
                    payload["animatorLoop"] = loop; payload["animatorLoopSet"] = true; continue;
                }
                if (option is "--time" or "--duration") {
                    if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) || !float.IsFinite(number) || number < 0)
                        throw new ArgumentException(option + " requires finite nonnegative seconds.");
                    string field = option == "--time" ? "animatorSampleTime" : "animatorDuration";
                    payload[field] = number; payload[field + "Set"] = true; continue;
                }
                if (!names.TryGetValue(option, out var key)) throw new ArgumentException("Unknown option: " + option);
                payload[key] = option == "--input" ? Path.GetFullPath(value) : value;
            }
            string response = await CLIClient.SendAsync(project, "/animator", payload, "POST");
            return ResponseFormatter.WriteJsonOrHuman(response, json, root => {
                if (!root.GetProperty("ok").GetBoolean()) { ResponseFormatter.WriteError(root); return; }
                Console.WriteLine(root.GetProperty("message").GetString());
                if (root.TryGetProperty("data", out var data)) Console.WriteLine(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            });
        }
        catch (Exception error) {
            if (json) Console.WriteLine(JsonSerializer.Serialize(new { ok = false, error = new { message = error.Message } }));
            else Console.Error.WriteLine(error.Message);
            return error is ArgumentException ? 2 : 1;
        }
    }
}
