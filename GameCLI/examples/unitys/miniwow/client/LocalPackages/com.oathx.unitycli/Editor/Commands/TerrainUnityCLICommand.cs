using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Handles editor bridge requests for Unity Terrain assets and scene bindings.
    /// </summary>
    internal sealed class TerrainUnityCLICommand : IUnityCLICommand
    {
        public string Path
        {
            get { return "/terrain"; }
        }

        public string Method
        {
            get { return "POST"; }
        }

        public async Task<object> ExecuteAsync(UnityCliBridgeRequest request, UnityCLICommandContext context, CancellationToken cancellationToken)
        {
            try
            {
                var data = await context.RunOnMainThreadAsync(() => ExecuteTerrain(request));
                return TerrainResult.Ok(data);
            }
            catch (Exception exception)
            {
                return PingResult.Fail("terrain_failed", exception.Message);
            }
        }

        static TerrainCommandData ExecuteTerrain(UnityCliBridgeRequest request)
        {
            if (request == null) throw new InvalidOperationException("Empty request.");

            var action = Normalize(request.action);
            var data = new TerrainCommandData
            {
                action = action,
                scenePath = request.saveScenePath ?? string.Empty,
                objectPath = request.objectPath ?? string.Empty,
                dataPath = NormalizeAssetPath(request.terrainDataPath),
                layerPath = NormalizeAssetPath(request.terrainLayerPath),
                texturePath = NormalizeAssetPath(request.terrainTexturePath),
                dryRun = request.dryRun,
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };

            switch (action)
            {
                case "inspect":
                    Inspect(request, data);
                    break;
                case "createdata":
                case "create-data":
                    CreateData(request, data);
                    break;
                case "createlayer":
                case "create-layer":
                    CreateLayer(request, data);
                    break;
                case "assignlayer":
                case "assign-layer":
                    AssignLayer(request, data);
                    break;
                case "binddata":
                case "bind-data":
                    BindData(request, data);
                    break;
                default:
                    throw new InvalidOperationException("terrain action must be inspect, create-data, create-layer, assign-layer, or bind-data.");
            }

            return data;
        }

        static void Inspect(UnityCliBridgeRequest request, TerrainCommandData data)
        {
            if (!string.IsNullOrEmpty(data.dataPath))
            {
                var terrainData = LoadAsset<TerrainData>(data.dataPath, "--data");
                data.layers = DescribeLayers(terrainData.terrainLayers);
                data.layerCount = data.layers.Length;
                data.terrains = new[]
                {
                    DescribeTerrainData(terrainData, data.dataPath, string.Empty)
                };
                data.terrainCount = 1;
                data.message = "terrain data inspected";
                return;
            }

            var scene = ResolveScene(request.saveScenePath, false);
            data.scenePath = scene.path;
            var terrains = ResolveTerrains(scene, request.objectPath);
            data.terrains = terrains.Select(DescribeTerrain).ToArray();
            data.terrainCount = data.terrains.Length;
            data.layerCount = data.terrains.Sum(entry => entry.layerCount);
            data.message = "terrain inspected";
        }

        static void CreateData(UnityCliBridgeRequest request, TerrainCommandData data)
        {
            RequireAssetPath(data.dataPath, "--data", ".asset");
            data.details = new[] { "create TerrainData " + data.dataPath };
            data.message = data.dryRun ? "terrain data dry-run passed" : "terrain data created";
            if (data.dryRun) return;

            EnsureParentFolder(data.dataPath);
            var terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(data.dataPath);
            if (terrainData == null)
            {
                terrainData = new TerrainData();
                AssetDatabase.CreateAsset(terrainData, data.dataPath);
            }

            terrainData.size = string.IsNullOrWhiteSpace(request.terrainSize)
                ? new Vector3(100f, 20f, 100f)
                : ParseVector3(request.terrainSize, "--terrain-size");

            if (request.terrainHeightmapResolutionSet)
            {
                terrainData.heightmapResolution = request.terrainHeightmapResolution;
            }

            if (request.terrainAlphamapResolutionSet)
            {
                terrainData.alphamapResolution = request.terrainAlphamapResolution;
            }

            if (request.terrainBaseMapResolutionSet)
            {
                terrainData.baseMapResolution = request.terrainBaseMapResolution;
            }

            EditorUtility.SetDirty(terrainData);
            data.assetModified = true;
            data.terrains = new[] { DescribeTerrainData(terrainData, data.dataPath, string.Empty) };
            data.terrainCount = 1;
            AssetDatabase.SaveAssets();
        }

        static void CreateLayer(UnityCliBridgeRequest request, TerrainCommandData data)
        {
            RequireAssetPath(data.layerPath, "--layer", ".terrainlayer");
            var texture = LoadAsset<Texture2D>(data.texturePath, "--texture");
            var normal = string.IsNullOrWhiteSpace(request.terrainNormalPath)
                ? null
                : LoadAsset<Texture2D>(NormalizeAssetPath(request.terrainNormalPath), "--normal");

            data.details = new[] { "create TerrainLayer " + data.layerPath };
            data.message = data.dryRun ? "terrain layer dry-run passed" : "terrain layer created";
            if (data.dryRun) return;

            EnsureParentFolder(data.layerPath);
            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(data.layerPath);
            if (layer == null)
            {
                layer = new TerrainLayer();
                AssetDatabase.CreateAsset(layer, data.layerPath);
            }

            layer.diffuseTexture = texture;
            layer.normalMapTexture = normal;
            layer.tileSize = string.IsNullOrWhiteSpace(request.terrainTileSize)
                ? new Vector2(1f, 1f)
                : ParseVector2(request.terrainTileSize, "--tile-size");
            layer.tileOffset = string.IsNullOrWhiteSpace(request.terrainTileOffset)
                ? Vector2.zero
                : ParseVector2(request.terrainTileOffset, "--tile-offset");
            layer.smoothness = request.terrainSmoothnessSet ? Mathf.Clamp01(request.terrainSmoothness) : 0f;
            layer.metallic = request.terrainMetallicSet ? Mathf.Clamp01(request.terrainMetallic) : 0f;

            EditorUtility.SetDirty(layer);
            data.assetModified = true;
            data.layers = new[] { DescribeLayer(layer, 0) };
            data.layerCount = 1;
            AssetDatabase.SaveAssets();
        }

        static void AssignLayer(UnityCliBridgeRequest request, TerrainCommandData data)
        {
            var terrainData = LoadAsset<TerrainData>(data.dataPath, "--data");
            var layer = LoadAsset<TerrainLayer>(data.layerPath, "--layer");
            var layers = terrainData.terrainLayers == null
                ? new List<TerrainLayer>()
                : terrainData.terrainLayers.ToList();
            var index = request.terrainAppendLayer
                ? layers.Count
                : request.terrainLayerIndexSet ? request.terrainLayerIndex : 0;
            if (index < 0) throw new InvalidOperationException("--index must be >= 0.");

            while (layers.Count <= index)
            {
                layers.Add(null);
            }

            layers[index] = layer;
            data.details = new[] { "assign " + data.layerPath + " to " + data.dataPath + " at index " + index.ToString(CultureInfo.InvariantCulture) };
            data.message = data.dryRun ? "terrain layer assignment dry-run passed" : "terrain layer assigned";
            if (!data.dryRun)
            {
                terrainData.terrainLayers = layers.ToArray();
                EditorUtility.SetDirty(terrainData);
                data.assetModified = true;
                AssetDatabase.SaveAssets();
            }

            data.layers = DescribeLayers(layers.ToArray());
            data.layerCount = data.layers.Length;
            data.terrains = new[] { DescribeTerrainData(terrainData, data.dataPath, string.Empty, data.layers) };
            data.terrainCount = 1;
        }

        static void BindData(UnityCliBridgeRequest request, TerrainCommandData data)
        {
            var terrainData = LoadAsset<TerrainData>(data.dataPath, "--data");
            var scene = ResolveScene(request.saveScenePath, true);
            data.scenePath = scene.path;
            var terrains = ResolveTerrains(scene, request.objectPath);
            if (terrains.Count != 1)
            {
                throw new InvalidOperationException("bind-data requires exactly one Terrain target. Provide --path <terrain object>.");
            }

            var terrain = terrains[0];
            data.details = new[] { "bind " + data.dataPath + " to " + UnityCliSceneGameObjectSearcher.GetHierarchyPath(terrain.gameObject) };
            data.message = data.dryRun ? "terrain data bind dry-run passed" : "terrain data bound";
            if (!data.dryRun)
            {
                Undo.RecordObject(terrain, "Bind TerrainData");
                terrain.terrainData = terrainData;
                var collider = terrain.GetComponent<TerrainCollider>();
                if (collider != null)
                {
                    Undo.RecordObject(collider, "Bind TerrainData Collider");
                    collider.terrainData = terrainData;
                    EditorUtility.SetDirty(collider);
                }

                EditorUtility.SetDirty(terrain);
                EditorSceneManager.MarkSceneDirty(scene);
                data.sceneModified = true;
                data.sceneSaved = EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
            }

            data.terrains = new[] { DescribeTerrain(terrain, terrainData) };
            data.terrainCount = 1;
            data.layerCount = data.terrains[0].layerCount;
        }

        static Scene ResolveScene(string scenePath, bool requireScenePath)
        {
            var requestedPath = NormalizeAssetPath(scenePath);
            if (!string.IsNullOrEmpty(requestedPath))
            {
                if (!File.Exists(requestedPath))
                {
                    throw new FileNotFoundException("Scene asset was not found: " + requestedPath, requestedPath);
                }

                return EditorSceneManager.OpenScene(requestedPath, OpenSceneMode.Single);
            }

            if (requireScenePath)
            {
                throw new InvalidOperationException("Provide --scene <Assets/...unity>.");
            }

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isLoaded)
            {
                return activeScene;
            }

            throw new InvalidOperationException("No active scene is loaded. Provide --scene <Assets/...unity>.");
        }

        static List<Terrain> ResolveTerrains(Scene scene, string objectPath)
        {
            var path = (objectPath ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(path))
            {
                var gameObject = FindSceneObject(scene, path);
                if (gameObject == null)
                {
                    throw new InvalidOperationException("Terrain object was not found: " + path);
                }

                var terrain = gameObject.GetComponent<Terrain>();
                if (terrain == null)
                {
                    throw new InvalidOperationException("GameObject has no Terrain component: " + path);
                }

                return new List<Terrain> { terrain };
            }

            return UnityEngine.Object.FindObjectsOfType<Terrain>(true)
                .Where(terrain => terrain != null && terrain.gameObject.scene == scene)
                .ToList();
        }

        static GameObject FindSceneObject(Scene scene, string path)
        {
            var normalized = path.Trim().Trim('/');
            if (string.IsNullOrEmpty(normalized))
            {
                return null;
            }

            var parts = normalized.Split('/');
            foreach (var root in scene.GetRootGameObjects())
            {
                if (!string.Equals(root.name, parts[0], StringComparison.Ordinal))
                {
                    continue;
                }

                var current = root.transform;
                for (var index = 1; index < parts.Length && current != null; index++)
                {
                    current = current.Find(parts[index]);
                }

                if (current != null)
                {
                    return current.gameObject;
                }
            }

            return null;
        }

        static TerrainEntry DescribeTerrain(Terrain terrain)
        {
            return DescribeTerrain(terrain, terrain == null ? null : terrain.terrainData);
        }

        static TerrainEntry DescribeTerrain(Terrain terrain, TerrainData terrainData)
        {
            return DescribeTerrainData(
                terrainData,
                terrainData == null ? string.Empty : AssetDatabase.GetAssetPath(terrainData),
                terrain == null ? string.Empty : UnityCliSceneGameObjectSearcher.GetHierarchyPath(terrain.gameObject));
        }

        static TerrainEntry DescribeTerrainData(TerrainData terrainData, string assetPath, string objectPath)
        {
            return DescribeTerrainData(terrainData, assetPath, objectPath, null);
        }

        static TerrainEntry DescribeTerrainData(TerrainData terrainData, string assetPath, string objectPath, TerrainLayerEntry[] overrideLayers)
        {
            if (terrainData == null)
            {
                return new TerrainEntry
                {
                    objectPath = objectPath,
                    dataPath = assetPath
                };
            }

            var layers = overrideLayers ?? DescribeLayers(terrainData.terrainLayers);
            return new TerrainEntry
            {
                objectPath = objectPath,
                dataPath = assetPath,
                size = FormatVector3(terrainData.size),
                heightmapResolution = terrainData.heightmapResolution,
                alphamapResolution = terrainData.alphamapResolution,
                baseMapResolution = terrainData.baseMapResolution,
                layerCount = layers.Length,
                layers = layers
            };
        }

        static TerrainLayerEntry[] DescribeLayers(TerrainLayer[] layers)
        {
            if (layers == null)
            {
                return Array.Empty<TerrainLayerEntry>();
            }

            var entries = new TerrainLayerEntry[layers.Length];
            for (var index = 0; index < layers.Length; index++)
            {
                entries[index] = DescribeLayer(layers[index], index);
            }

            return entries;
        }

        static TerrainLayerEntry DescribeLayer(TerrainLayer layer, int index)
        {
            if (layer == null)
            {
                return new TerrainLayerEntry { index = index };
            }

            return new TerrainLayerEntry
            {
                index = index,
                path = AssetDatabase.GetAssetPath(layer),
                name = layer.name,
                texturePath = layer.diffuseTexture == null ? string.Empty : AssetDatabase.GetAssetPath(layer.diffuseTexture),
                normalPath = layer.normalMapTexture == null ? string.Empty : AssetDatabase.GetAssetPath(layer.normalMapTexture),
                tileSize = FormatVector2(layer.tileSize),
                tileOffset = FormatVector2(layer.tileOffset),
                metallic = layer.metallic,
                smoothness = layer.smoothness
            };
        }

        static T LoadAsset<T>(string path, string optionName) where T : UnityEngine.Object
        {
            var normalized = NormalizeAssetPath(path);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new InvalidOperationException(optionName + " is required.");
            }

            var asset = AssetDatabase.LoadAssetAtPath<T>(normalized);
            if (asset == null)
            {
                throw new InvalidOperationException(optionName + " asset was not found or has the wrong type: " + normalized);
            }

            return asset;
        }

        static void RequireAssetPath(string path, string optionName, string extension)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException(optionName + " is required.");
            }

            if (!path.StartsWith("Assets/", StringComparison.Ordinal) && !path.StartsWith("Packages/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(optionName + " must be under Assets/ or Packages/: " + path);
            }

            if (!path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(optionName + " must end with " + extension + ": " + path);
            }
        }

        static void EnsureParentFolder(string assetPath)
        {
            var folder = System.IO.Path.GetDirectoryName(assetPath).Replace('\\', '/');
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parts = folder.Split('/');
            var current = parts[0];
            for (var index = 1; index < parts.Length; index++)
            {
                var next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        static Vector2 ParseVector2(string value, string optionName)
        {
            var parts = (value ?? string.Empty).Split(',');
            if (parts.Length != 2)
            {
                throw new InvalidOperationException(optionName + " requires x,y.");
            }

            return new Vector2(ParseFloat(parts[0], optionName), ParseFloat(parts[1], optionName));
        }

        static Vector3 ParseVector3(string value, string optionName)
        {
            var parts = (value ?? string.Empty).Split(',');
            if (parts.Length != 3)
            {
                throw new InvalidOperationException(optionName + " requires x,y,z.");
            }

            return new Vector3(ParseFloat(parts[0], optionName), ParseFloat(parts[1], optionName), ParseFloat(parts[2], optionName));
        }

        static float ParseFloat(string value, string optionName)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                throw new InvalidOperationException(optionName + " contains an invalid number: " + value);
            }

            return result;
        }

        static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        static string NormalizeAssetPath(string path)
        {
            return (path ?? string.Empty).Trim().Replace('\\', '/');
        }

        static string FormatVector2(Vector2 value)
        {
            return value.x.ToString("0.###", CultureInfo.InvariantCulture) + "," + value.y.ToString("0.###", CultureInfo.InvariantCulture);
        }

        static string FormatVector3(Vector3 value)
        {
            return value.x.ToString("0.###", CultureInfo.InvariantCulture)
                + "," + value.y.ToString("0.###", CultureInfo.InvariantCulture)
                + "," + value.z.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
