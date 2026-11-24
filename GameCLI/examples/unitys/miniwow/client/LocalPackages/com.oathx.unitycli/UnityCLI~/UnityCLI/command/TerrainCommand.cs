using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI terrain command.
    /// </summary>
    internal sealed class TerrainCommand : ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name
        {
            get { return "terrain"; }
        }

        /// <summary>
        /// Sends the Terrain command request to the Unity Editor bridge.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        public async Task<int> ExecuteAsync(CliOptions options)
        {
            var action = (options.Action ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(action))
            {
                Console.Error.WriteLine("terrain requires an action: inspect, create-data, create-layer, assign-layer, bind-data.");
                return 1;
            }

            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/terrain", new
            {
                action = action,
                saveScenePath = options.SaveScenePath,
                objectPath = options.ObjectPath,
                dryRun = options.DryRun,
                terrainDataPath = options.TerrainDataPath,
                terrainLayerPath = options.TerrainLayerPath,
                terrainTexturePath = options.TerrainTexturePath,
                terrainNormalPath = options.TerrainNormalPath,
                terrainTileSize = options.TerrainTileSize,
                terrainTileOffset = options.TerrainTileOffset,
                terrainSize = options.TerrainSize,
                terrainHeightmapResolution = options.TerrainHeightmapResolution,
                terrainHeightmapResolutionSet = options.TerrainHeightmapResolutionSet,
                terrainAlphamapResolution = options.TerrainAlphamapResolution,
                terrainAlphamapResolutionSet = options.TerrainAlphamapResolutionSet,
                terrainBaseMapResolution = options.TerrainBaseMapResolution,
                terrainBaseMapResolutionSet = options.TerrainBaseMapResolutionSet,
                terrainLayerIndex = options.TerrainLayerIndex,
                terrainLayerIndexSet = options.TerrainLayerIndexSet,
                terrainAppendLayer = options.TerrainAppendLayer,
                terrainSmoothness = options.TerrainSmoothness,
                terrainSmoothnessSet = options.TerrainSmoothnessSet,
                terrainMetallic = options.TerrainMetallic,
                terrainMetallicSet = options.TerrainMetallicSet
            }, "POST");

            return ResponseFormatter.WriteJsonOrHuman(responseJson, options.JsonOutput, WriteHuman);
        }

        /// <summary>
        /// Writes human-readable terrain output.
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

            Console.WriteLine(root.TryGetProperty("message", out var message)
                ? message.GetString()
                : "terrain command completed");

            if (!root.TryGetProperty("data", out var data))
            {
                return;
            }

            WriteString(data, "action");
            WriteString(data, "scenePath");
            WriteString(data, "dataPath");
            WriteString(data, "layerPath");
            WriteString(data, "texturePath");
            WriteString(data, "objectPath");
            WriteBool(data, "dryRun");
            WriteBool(data, "assetModified");
            WriteBool(data, "sceneModified");
            WriteBool(data, "sceneSaved");
            WriteInt(data, "terrainCount");
            WriteInt(data, "layerCount");

            if (data.TryGetProperty("terrains", out var terrains) && terrains.ValueKind == JsonValueKind.Array)
            {
                foreach (var terrain in terrains.EnumerateArray())
                {
                    Console.WriteLine("  - terrain: " + GetString(terrain, "objectPath"));
                    WriteNestedString(terrain, "    ", "dataPath");
                    WriteNestedString(terrain, "    ", "size");
                    WriteNestedInt(terrain, "    ", "heightmapResolution");
                    WriteNestedInt(terrain, "    ", "alphamapResolution");
                    WriteNestedInt(terrain, "    ", "baseMapResolution");
                    WriteNestedInt(terrain, "    ", "layerCount");
                    WriteLayers(terrain, "    ");
                }
            }

            WriteLayers(data, "  ");

            if (data.TryGetProperty("details", out var details) && details.ValueKind == JsonValueKind.Array)
            {
                foreach (var detail in details.EnumerateArray())
                {
                    if (detail.ValueKind == JsonValueKind.String)
                    {
                        Console.WriteLine("  - " + detail.GetString());
                    }
                }
            }
        }

        static void WriteLayers(JsonElement element, string prefix)
        {
            if (!element.TryGetProperty("layers", out var layers) || layers.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var layer in layers.EnumerateArray())
            {
                Console.WriteLine(prefix + "- layer[" + GetInt(layer, "index") + "]: " + GetString(layer, "path"));
                WriteNestedString(layer, prefix + "  ", "texturePath");
                WriteNestedString(layer, prefix + "  ", "tileSize");
                WriteNestedString(layer, prefix + "  ", "tileOffset");
            }
        }

        static void WriteString(JsonElement data, string name)
        {
            var value = GetString(data, name);
            if (!string.IsNullOrEmpty(value))
            {
                Console.WriteLine("  " + name + ": " + value);
            }
        }

        static void WriteInt(JsonElement data, string name)
        {
            if (data.TryGetProperty(name, out var value) && value.TryGetInt32(out var number))
            {
                Console.WriteLine("  " + name + ": " + number);
            }
        }

        static void WriteBool(JsonElement data, string name)
        {
            if (data.TryGetProperty(name, out var value)
                && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
            {
                Console.WriteLine("  " + name + ": " + value.GetBoolean());
            }
        }

        static void WriteNestedString(JsonElement data, string prefix, string name)
        {
            var value = GetString(data, name);
            if (!string.IsNullOrEmpty(value))
            {
                Console.WriteLine(prefix + name + ": " + value);
            }
        }

        static void WriteNestedInt(JsonElement data, string prefix, string name)
        {
            if (data.TryGetProperty(name, out var value) && value.TryGetInt32(out var number))
            {
                Console.WriteLine(prefix + name + ": " + number);
            }
        }

        static string GetString(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }

        static int GetInt(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) && value.TryGetInt32(out var number)
                ? number
                : 0;
        }
    }
}
