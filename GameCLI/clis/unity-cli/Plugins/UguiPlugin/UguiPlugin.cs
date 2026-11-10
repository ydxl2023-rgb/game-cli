using System.Text.Json;
using System.Text.RegularExpressions;
using UnityCLI.Abstractions;

namespace UnityCLI;

public sealed class UguiPlugin : IPlugin
{
    public string Name => "UguiPlugin";
    public IReadOnlyCollection<string> Commands => new[] { "ugui" };
    public string Help => "ugui (--template <database ID> | --input <record-or-payload.json>) --project <Unity root> --prefab <Assets/...prefab> [--assets-dir <folder>] [--integration <codex folder>] [--dry-run] [--overwrite] [--json]";

    public async Task<int> ExecuteAsync(string command, string[] args)
    {
        if (args.Contains("--help")) { Console.WriteLine(Help); return 0; }
        string? studio = null, project = null, prefab = null, template = null, assetsDir = null, integration = null;
        
        bool dryRun = false, overwrite = false;
        for (int i = 0; i < args.Length; i++)
        {
            string Value() => ++i < args.Length ? args[i] : throw new ArgumentException("Missing option value.");
            switch (args[i])
            {
                case "--template": template = Value(); break;
                case "--assets-dir": assetsDir = Path.GetFullPath(Value()); break;
                case "--integration": integration = Path.GetFullPath(Value()); break;
                case "--input": studio = Path.GetFullPath(Value()); break;
                case "--project": project = Path.GetFullPath(Value()); break;
                case "--prefab": prefab = Value().Replace('\\', '/'); break;
                case "--dry-run": dryRun = true; break;
                case "--overwrite": overwrite = true; break;
                case "--json": break;
                default: throw new ArgumentException("Unknown ugui option: " + args[i]);
            }
        }
        if ((studio == null) == (template == null) || project == null || prefab == null) throw new ArgumentException(Help);
        if (!Directory.Exists(Path.Combine(project, "ProjectSettings"))) throw new ArgumentException("Not a Unity project.");
        if (!prefab.StartsWith("Assets/") || !prefab.EndsWith(".prefab") || prefab.Split('/').Any(p => p is ".." or "." or ""))
            throw new ArgumentException("Expected an Assets/.../*.prefab path without traversal.");
        var dir = Path.Combine(project, "Library", "GameCLI", "UGUI", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        if (template != null)
        {
            if (integration == null)
            {
                for (var parent = new DirectoryInfo(project); parent != null; parent = parent.Parent)
                {
                    var candidate = Path.Combine(parent.FullName, "integrations", "codex");
                    if (File.Exists(Path.Combine(candidate, "server", "database.mjs"))) { integration = candidate; break; }
                }
            }
            if (integration == null) throw new ArgumentException("Specify --integration pointing to the configured integrations/codex directory.");
            var start = new System.Diagnostics.ProcessStartInfo("node") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
            foreach (var value in new[] { Path.Combine(AppContext.BaseDirectory, "Plugins/UguiPlugin/read-template.mjs"), integration, template, dir }) start.ArgumentList.Add(value);
            using var process = System.Diagnostics.Process.Start(start)!;
            var error = process.StandardError.ReadToEndAsync(); var output = process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync(); await output;
            if (process.ExitCode != 0) throw new InvalidOperationException("Database template read failed: " + await error);
            studio = Path.Combine(dir, "template.json"); assetsDir = dir;
        }
        var json = await File.ReadAllTextAsync(studio!);
        using var document = JsonDocument.Parse(json);
        var payload = document.RootElement.TryGetProperty("payload", out var stored) ? stored : document.RootElement;
        if (!payload.TryGetProperty("root", out _) || !payload.TryGetProperty("design", out var design) || design.GetProperty("schemaVersion").GetInt32() != 1)
            throw new ArgumentException("Expected database template record or payload { root, design: { schemaVersion: 1, nodes } }.");
        assetsDir ??= Path.Combine(Path.GetDirectoryName(studio!)!, "assets");
        foreach (var id in design.GetProperty("nodes").EnumerateArray().Where(n => n.TryGetProperty("image", out _)).Select(n => n.GetProperty("image").GetString()!).Distinct())
        {
            if (!Regex.IsMatch(id, "^[a-f0-9]{64}$")) throw new ArgumentException("Invalid image ID.");
            var bytes = await File.ReadAllBytesAsync(Path.Combine(assetsDir, id + ".png"));
            if (!Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).Equals(id, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Image hash mismatch: " + id);
            await File.WriteAllBytesAsync(Path.Combine(dir, id + ".png"), bytes);
        }
        var namesPath = Path.Combine(assetsDir, "asset-names.json");
        if (File.Exists(namesPath) && Path.GetFullPath(namesPath) != Path.Combine(dir, "asset-names.json"))
            File.Copy(namesPath, Path.Combine(dir, "asset-names.json"), true);
        var designPath = Path.Combine(dir, "source.json");
        await File.WriteAllTextAsync(designPath, json); // Preserve the raw database envelope and all metadata.
        async Task<JsonDocument> Send(bool preview)
        {
            var requestDir = Path.Combine(dir, preview ? "preview" : "build");
            Directory.CreateDirectory(requestDir);
            var request = new { designPath, assetsDirectory = dir, prefabPath = prefab, dryRun = preview, overwrite, expiresUtc = DateTime.UtcNow.AddSeconds(90).ToString("O") };
            var pending = Path.Combine(requestDir, "request.tmp");
            await File.WriteAllTextAsync(pending, JsonSerializer.Serialize(request));
            File.Move(pending, Path.Combine(requestDir, "request.json"));
            var response = Path.Combine(requestDir, "result.json");
            for (int i = 0; i < 900; i++)
            {
                if (File.Exists(response)) return JsonDocument.Parse(await File.ReadAllTextAsync(response));
                await Task.Delay(100);
            }
            var waiting = Path.Combine(requestDir, "request.json");
            if (File.Exists(waiting)) File.Delete(waiting);
            throw new TimeoutException("UGUI Editor importer did not respond. Refresh Unity. A claimed build may still finish; inspect the prefab before retrying.");
        }
        using var previewResult = await Send(true);
        var result = previewResult;
        JsonDocument? buildResult = null;
        if (!dryRun && previewResult.RootElement.GetProperty("ok").GetBoolean()) result = buildResult = await Send(false);
        try
        {
            Console.WriteLine(args.Contains("--json") ? result.RootElement.GetRawText() : result.RootElement.GetProperty("message").GetString());
            if (!args.Contains("--json") && result.RootElement.TryGetProperty("warnings", out var warnings))
                foreach (var warning in warnings.EnumerateArray()) Console.WriteLine("warning: " + warning.GetString());
            return result.RootElement.GetProperty("ok").GetBoolean() ? 0 : 1;
        }
        finally { buildResult?.Dispose(); }
    }
}
