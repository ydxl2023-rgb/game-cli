using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UnityCLI
{
    /// <summary>Extracts contract slots and computes assembly mappings without another design document.</summary>
    internal sealed class UiResourcesCommand : ICommand
    {
        public string Name => "ui-resources";

        public async Task<int> ExecuteAsync(CliOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.JsonFile) || string.IsNullOrWhiteSpace(options.ScreenshotPath))
            {
                throw new InvalidOperationException("ui-resources requires --json <contract> --output <file>.");
            }
            string source = Path.GetFullPath(options.JsonFile);
            string output = Path.GetFullPath(options.ScreenshotPath);
            if (string.Equals(source, output, StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrEmpty(options.AssetManifestPath) && string.Equals(Path.GetFullPath(options.AssetManifestPath), output, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Output must not overwrite contract or receipts.");
            }
            JsonObject contract = Read(source);
            JsonObject window = SelectWindow(contract, options.PrefabPath);
            JsonArray slots = window["resourceSlots"] as JsonArray ?? throw new InvalidOperationException("Missing resourceSlots.");
            JsonObject document = string.IsNullOrWhiteSpace(options.AssetManifestPath)
                ? Extract(window, slots, await HashAsync(source))
                : await MapAsync(slots, Read(options.AssetManifestPath), await HashAsync(source), options.ProjectPath ?? Directory.GetCurrentDirectory());
            string json = document.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            await File.WriteAllTextAsync(output, json);
            Console.WriteLine(JsonSerializer.Serialize(new { ok = true, output, resourceCount = slots.Count, schemaVersion = document["schemaVersion"]?.GetValue<string>() }));
            return 0;
        }

        private static JsonObject SelectWindow(JsonObject contract, string? path)
        {
            string? schema = contract["schemaVersion"]?.GetValue<string>();
            if (schema == "ui-task-input/1")
            {
                JsonObject window = contract["window"] as JsonObject ?? throw new InvalidOperationException("Missing window.");
                if (!string.IsNullOrEmpty(path) && window["prefabPath"]?.GetValue<string>() != path)
                {
                    throw new InvalidOperationException("Window path mismatch.");
                }
                return window;
            }
            if (schema != "ui-production-contract/1")
            {
                throw new InvalidOperationException("Unsupported contract schema.");
            }
            JsonObject[] windows = (contract["windows"] as JsonArray ?? throw new InvalidOperationException("Missing windows.")).OfType<JsonObject>()
                .Where(w => string.IsNullOrEmpty(path) || w["prefabPath"]?.GetValue<string>() == path).ToArray();
            return windows.Length == 1 ? windows[0] : throw new InvalidOperationException("Select one window using --prefab.");
        }

        private static JsonObject Extract(JsonObject window, JsonArray slots, string hash)
        {
            JsonArray resources = new JsonArray();
            JsonArray nodes = window["nodes"] as JsonArray ?? throw new InvalidOperationException("Missing nodes.");
            foreach (JsonObject slot in slots.OfType<JsonObject>())
            {
                JsonObject item = (JsonObject)slot.DeepClone();
                JsonArray context = new JsonArray();
                foreach (JsonObject binding in (slot["bindings"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
                {
                    JsonObject node = nodes.OfType<JsonObject>().Single(n => n["id"]?.GetValue<string>() == binding["nodeId"]?.GetValue<string>());
                    context.Add(new JsonObject { ["nodeId"] = node["id"]?.DeepClone(), ["name"] = node["name"]?.DeepClone(), ["rect"] = node["rect"]?.DeepClone() });
                }
                item["usage"] = context;
                resources.Add(item);
            }
            return new JsonObject
            {
                ["schemaVersion"] = "ui-resource-info/1",
                ["contractFileSha256"] = hash,
                ["windowId"] = window["windowId"]?.DeepClone(),
                ["styleContract"] = window["styleContract"]?.DeepClone(),
                ["resources"] = resources,
                ["animations"] = window["animations"]?.DeepClone(),
                ["effects"] = window["effects"]?.DeepClone()
            };
        }

        private static async Task<JsonObject> MapAsync(JsonArray slots, JsonObject receipts, string hash, string project)
        {
            if (receipts["contractFileSha256"]?.GetValue<string>() != hash)
            {
                throw new InvalidOperationException("Receipts contractFileSha256 mismatch.");
            }
            Dictionary<string, JsonObject> mappings = (receipts["resources"] as JsonArray ?? throw new InvalidOperationException("Missing resource receipts."))
                .OfType<JsonObject>().ToDictionary(r => Required(r, "slotId"));
            if (mappings.Count != slots.Count)
            {
                throw new InvalidOperationException("Receipts must cover exactly the contract slots.");
            }
            JsonArray resources = new JsonArray();
            foreach (JsonObject slot in slots.OfType<JsonObject>())
            {
                string id = Required(slot, "slotId");
                if (!mappings.TryGetValue(id, out JsonObject? receipt))
                {
                    throw new InvalidOperationException("Missing receipt: " + id);
                }
                JsonObject entry = await AssetAsync(receipt, project);
                string policy = Required(slot, "generationPolicy");
                if (policy == "reuse" || policy == "preserve")
                {
                    string? expected = slot["sourceReference"]?["sha256"]?.GetValue<string>();
                    if (expected != null && expected != entry["sha256"]?.GetValue<string>())
                    {
                        throw new InvalidOperationException("Source hash mismatch: " + id);
                    }
                }
                entry["slotId"] = id;
                resources.Add(entry);
            }
            JsonArray assets = new JsonArray();
            foreach (JsonObject asset in (receipts["assets"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
            {
                assets.Add(await AssetAsync(asset, project));
            }
            return new JsonObject { ["schemaVersion"] = "ui-contract-resources/1", ["contractFileSha256"] = hash, ["resources"] = resources, ["assets"] = assets };
        }

        private static async Task<JsonObject> AssetAsync(JsonObject receipt, string project)
        {
            string path = Required(receipt, "path");
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.Contains('\\') || path.Split('/').Contains(".."))
            {
                throw new InvalidOperationException("Expected project-relative Assets path: " + path);
            }
            string physical = Path.GetFullPath(Path.Combine(project, path));
            string root = Path.GetFullPath(Path.Combine(project, "Assets")) + Path.DirectorySeparatorChar;
            if (!physical.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Asset escapes project: " + path);
            }
            JsonObject entry = new JsonObject { ["path"] = path, ["sha256"] = await HashAsync(physical) };
            if (receipt["animationId"] != null)
            {
                entry["animationId"] = receipt["animationId"]!.DeepClone();
            }
            return entry;
        }

        private static JsonObject Read(string path)
        {
            return JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? throw new InvalidOperationException("Expected JSON object.");
        }

        private static string Required(JsonObject value, string key)
        {
            return value[key]?.GetValue<string>() is string text && !string.IsNullOrWhiteSpace(text)
                ? text : throw new InvalidOperationException("Missing field: " + key);
        }

        private static async Task<string> HashAsync(string path)
        {
            await using FileStream stream = File.OpenRead(path);
            return Convert.ToHexString(await SHA256.HashDataAsync(stream)).ToLowerInvariant();
        }
    }
}
