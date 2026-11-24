using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace UnityCLI
{
    /// <summary>Prepares existing generated images to explicit contract geometry; never creates artwork.</summary>
    internal sealed class UiPrepareCommand : ICommand
    {
        public string Name => "ui-prepare";

        public async Task<int> ExecuteAsync(CliOptions options)
        {
            string contractPath = Path.GetFullPath(options.JsonFile ?? throw new InvalidOperationException("--json required"));
            string receiptsPath = Path.GetFullPath(options.AssetManifestPath ?? throw new InvalidOperationException("--asset-manifest required"));
            string output = Path.GetFullPath(options.ScreenshotPath ?? throw new InvalidOperationException("--output required"));
            if (!output.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Resource mapping output must be a JSON file.");
            }
            if (output.Equals(contractPath, StringComparison.OrdinalIgnoreCase) || output.Equals(receiptsPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Output cannot replace contract or receipts.");
            }
            JsonObject contract = JsonNode.Parse(await File.ReadAllTextAsync(contractPath))!.AsObject();
            string? schema = contract["schemaVersion"]?.GetValue<string>();
            if (schema != "ui-task-input/1" && schema != "ui-production-contract/1")
            {
                throw new InvalidOperationException("Unsupported contract schema.");
            }
            JsonObject receipts = JsonNode.Parse(await File.ReadAllTextAsync(receiptsPath))!.AsObject();
            string contractHash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(contractPath))).ToLowerInvariant();
            if (receipts["contractFileSha256"]?.GetValue<string>() != contractHash)
            {
                throw new InvalidOperationException("Receipts contractFileSha256 mismatch.");
            }
            JsonObject[] windows = contract["schemaVersion"]?.GetValue<string>() == "ui-task-input/1"
                ? new[] { contract["window"]!.AsObject() }
                : contract["windows"]!.AsArray().OfType<JsonObject>().Where(w => string.IsNullOrEmpty(options.PrefabPath) || w["prefabPath"]!.GetValue<string>() == options.PrefabPath).ToArray();
            if (windows.Length != 1)
            {
                throw new InvalidOperationException("Select exactly one contract window.");
            }
            Dictionary<string, JsonObject> records = receipts["resources"]!.AsArray().OfType<JsonObject>().ToDictionary(r => r["slotId"]!.GetValue<string>());
            JsonArray slots = windows[0]["resourceSlots"]!.AsArray();
            if (records.Count != slots.Count)
            {
                throw new InvalidOperationException("Receipts must cover exactly all slots.");
            }
            string project = Path.GetFullPath(options.ProjectPath ?? Directory.GetCurrentDirectory());
            Dictionary<string, byte[]> pending = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            JsonArray resources = new JsonArray();
            foreach (JsonObject slot in slots.OfType<JsonObject>())
            {
                string id = slot["slotId"]!.GetValue<string>();
                JsonObject receipt = records[id];
                string path = receipt["path"]!.GetValue<string>();
                string target = AssetPath(project, path);
                byte[] bytes;
                if (receipt["sourcePath"] is JsonValue source)
                {
                    if (slot["generationPolicy"]?.GetValue<string>() != "imagegen" || slot["kind"]?.GetValue<string>() != "sprite")
                    {
                        throw new InvalidOperationException("Only generated sprite slots can transform source images: " + id);
                    }
                    string purpose = slot["purpose"]?.GetValue<string>() ?? throw new InvalidOperationException("Explicit purpose required: " + id);
                    if (!new[] { "background", "surface", "icon", "divider", "effect", "decoration" }.Contains(purpose))
                    {
                        throw new InvalidOperationException("Unknown purpose: " + id);
                    }
                    string strategy = slot["reskinStrategy"]?.GetValue<string>() ?? throw new InvalidOperationException("Explicit reskinStrategy required: " + id);
                    using Image<Rgba32> original = await Image.LoadAsync<Rgba32>(Path.GetFullPath(source.GetValue<string>()));
                    int width = slot["deliverySize"]!["width"]!.GetValue<int>();
                    int height = slot["deliverySize"]!["height"]!.GetValue<int>();
                    using Image<Rgba32> result = Prepare(original, slot, strategy, width, height);
                    Validate(result, slot, purpose);
                    using MemoryStream stream = new MemoryStream();
                    await result.SaveAsPngAsync(stream);
                    bytes = stream.ToArray();
                    if (File.Exists(target) && !options.ReplaceExistingChild)
                    {
                        throw new InvalidOperationException("Prepared output exists; use --replace: " + path);
                    }
                    if (!pending.TryAdd(target, bytes))
                    {
                        throw new InvalidOperationException("Duplicate destination: " + path);
                    }
                }
                else
                {
                    bytes = await File.ReadAllBytesAsync(target);
                }
                string hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                if (slot["sourceReference"]?["sha256"] is JsonValue expected && expected.GetValue<string>() != hash)
                {
                    throw new InvalidOperationException("Reuse source hash mismatch: " + id);
                }
                resources.Add(new JsonObject { ["slotId"] = id, ["path"] = path, ["sha256"] = hash });
            }
            JsonArray assets = new JsonArray();
            foreach (JsonObject asset in (receipts["assets"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
            {
                string path = asset["path"]!.GetValue<string>();
                string hash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(AssetPath(project, path)))).ToLowerInvariant();
                assets.Add(new JsonObject { ["path"] = path, ["sha256"] = hash, ["animationId"] = asset["animationId"]?.DeepClone() });
            }
            JsonObject manifest = new JsonObject { ["schemaVersion"] = "ui-contract-resources/1", ["contractFileSha256"] = contractHash, ["resources"] = resources, ["assets"] = assets };
            // Validate every slot before writing any production image. Preserve existing .meta files.
            foreach ((string path, byte[] bytes) in pending)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await File.WriteAllBytesAsync(path, bytes);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            await File.WriteAllTextAsync(output, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine(JsonSerializer.Serialize(new { ok = true, prepared = pending.Count, resourceCount = resources.Count, output }));
            return 0;
        }

        private static Image<Rgba32> Prepare(Image<Rgba32> source, JsonObject slot, string strategy, int width, int height)
        {
            if (width < 1 || height < 1 || width > 8192 || height > 8192)
            {
                throw new InvalidOperationException("Invalid delivery dimensions.");
            }
            if (strategy == "preserve")
            {
                if (source.Width != width || source.Height != height)
                {
                    throw new InvalidOperationException("preserve requires exact source dimensions.");
                }
                return source.Clone();
            }
            if (strategy != "align-visible")
            {
                throw new InvalidOperationException("Unknown reskin strategy.");
            }
            JsonObject bounds = slot["visibleBounds"]!.AsObject();
            int threshold = bounds["alphaThreshold"]!.GetValue<int>();
            Rectangle sourceBounds = Measure(source, threshold).Bounds;
            Rectangle target = new Rectangle(bounds["x"]!.GetValue<int>(), bounds["y"]!.GetValue<int>(), bounds["width"]!.GetValue<int>(), bounds["height"]!.GetValue<int>());
            if (target.X < 0 || target.Y < 0 || target.Width < 1 || target.Height < 1 || target.Right > width || target.Bottom > height)
            {
                throw new InvalidOperationException("Visible bounds exceed delivery canvas.");
            }
            using Image<Rgba32> cropped = source.Clone(c => c.Crop(sourceBounds));
            return UiImagePreparation.Render(cropped, slot, target, width, height);
        }

        private static void Validate(Image<Rgba32> image, JsonObject slot, string purpose)
        {
            JsonObject bounds = slot["visibleBounds"]!.AsObject();
            var measured = Measure(image, bounds["alphaThreshold"]!.GetValue<int>());
            int tolerance = bounds["tolerance"]!.GetValue<int>();
            if (Math.Abs(measured.Bounds.X - bounds["x"]!.GetValue<int>()) > tolerance || Math.Abs(measured.Bounds.Y - bounds["y"]!.GetValue<int>()) > tolerance
                || Math.Abs(measured.Bounds.Width - bounds["width"]!.GetValue<int>()) > tolerance || Math.Abs(measured.Bounds.Height - bounds["height"]!.GetValue<int>()) > tolerance)
            {
                throw new InvalidOperationException("Visible bounds mismatch: " + slot["slotId"]);
            }
            double minimum = bounds["minCoverage"]?.GetValue<double>() ?? 0;
            double center = bounds["minCenterCoverage"]?.GetValue<double>() ?? 0;
            if (purpose == "surface" && (minimum <= 0 || center <= 0))
            {
                throw new InvalidOperationException("surface requires explicit whole/center coverage floors.");
            }
            if (measured.Coverage < minimum || measured.Center < center)
            {
                throw new InvalidOperationException("Skin coverage mismatch; regenerate artwork rather than stretch missing content: " + slot["slotId"]);
            }
        }

        private static (Rectangle Bounds, double Coverage, double Center) Measure(Image<Rgba32> image, int threshold)
        {
            if (threshold < 1 || threshold > 255)
            {
                throw new InvalidOperationException("Alpha threshold must be between 1 and 255.");
            }
            int left = image.Width;
            int top = image.Height;
            int right = -1;
            int bottom = -1;
            int count = 0;
            int centerCount = 0;
            int centerVisible = 0;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    bool center = x >= image.Width / 4 && x < image.Width * 3 / 4 && y >= image.Height / 4 && y < image.Height * 3 / 4;
                    if (center)
                    {
                        centerCount++;
                    }
                    if (image[x, y].A < threshold)
                    {
                        continue;
                    }
                    count++;
                    if (center)
                    {
                        centerVisible++;
                    }
                    left = Math.Min(left, x);
                    right = Math.Max(right, x);
                    top = Math.Min(top, y);
                    bottom = Math.Max(bottom, y);
                }
            }
            if (right < 0)
            {
                throw new InvalidOperationException("Empty image silhouette.");
            }
            return (new Rectangle(left, top, right - left + 1, bottom - top + 1), (double)count / (image.Width * image.Height), centerCount == 0 ? 0 : (double)centerVisible / centerCount);
        }

        private static string AssetPath(string project, string path)
        {
            string root = Path.Combine(project, "Assets") + Path.DirectorySeparatorChar;
            string full = Path.GetFullPath(Path.Combine(project, path));
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.Contains('\\') || !full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid Assets destination: " + path);
            }
            return full;
        }
    }
}
