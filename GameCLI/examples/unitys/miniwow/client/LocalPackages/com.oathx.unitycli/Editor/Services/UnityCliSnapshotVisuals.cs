using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Oathx.UnityCLI.Editor
{
    internal static class UnityCliSnapshotVisuals
    {
        internal static PrefabData ConfigureManifest(UnityCliBridgeRequest request)
        {
            string input = Path.GetFullPath(request.jsonFile);
            if (new FileInfo(input).Length > 8 * 1024 * 1024)
            {
                throw new InvalidOperationException("Manifest exceeds 8 MiB.");
            }
            JObject document = JObject.Parse(File.ReadAllText(input));
            if ((string)document["schemaVersion"] == "ui-native-mesh-resources/1")
            {
                return UnityCliNativeMeshResources.Configure(document, request.dryRun);
            }
            if ((string)document["schemaVersion"] == "ui-particle-resources/1")
            {
                return UnityCliParticleResources.Configure(document, request.dryRun);
            }
            if ((string)document["schemaVersion"] != "ui-template/1" || !(document["visualTextures"] is JArray textures) || textures.Count == 0)
            {
                throw new InvalidOperationException("Expected ui-template/1 with nonempty visualTextures.");
            }
            Configure(document, request.dryRun);
            JArray evidence = new JArray();
            foreach (JObject item in textures)
            {
                string path = UnityCliAssetAliases.Resolve((string)item["path"]);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                JObject settings = (JObject)item["settings"];
                JObject edges = (JObject)settings["spriteBorder"];
                Vector4 border = new Vector4((float)edges["x"], (float)edges["y"], (float)edges["z"], (float)edges["w"]);
                bool matches = importer != null && importer.textureType == TextureImporterType.Sprite
                    && importer.spriteImportMode == SpriteImportMode.Single
                    && Mathf.Approximately(importer.spritePixelsPerUnit, (float)settings["spritePixelsToUnits"])
                    && importer.spriteBorder == border
                    && (int)importer.wrapMode == ((int?)settings["wrapMode"] ?? 1);
                if (!request.dryRun && !matches)
                {
                    throw new InvalidOperationException("Importer readback mismatch: " + path);
                }
                evidence.Add(new JObject
                {
                    ["path"] = path,
                    ["matches"] = matches,
                    ["textureType"] = importer.textureType.ToString(),
                    ["spriteMode"] = importer.spriteImportMode.ToString(),
                    ["pixelsPerUnit"] = importer.spritePixelsPerUnit,
                    ["wrapMode"] = importer.wrapMode.ToString(),
                    ["border"] = new JArray(importer.spriteBorder.x, importer.spriteBorder.y, importer.spriteBorder.z, importer.spriteBorder.w)
                });
            }
            return new PrefabData
            {
                action = "configure-textures",
                dryRun = request.dryRun,
                valid = true,
                prefabModified = false,
                textureSettingsJson = evidence.ToString(Newtonsoft.Json.Formatting.None),
                message = request.dryRun ? "Texture manifest validated; no importer changed." : "Texture import settings applied and read back; no prefab changed."
            };
        }

        private static bool IsNativeShapeTextureReference(JToken reference)
        {
            JObject binding = reference.Parent?.Parent as JObject;
            JProperty property = binding?.Parent as JProperty;
            JObject properties = property?.Parent as JObject;
            JProperty propertiesMember = properties?.Parent as JProperty;
            JObject component = propertiesMember?.Parent as JObject;
            if (property?.Name != "ShapeModule.m_Texture" || propertiesMember?.Name != "properties"
                || (string)component?["type"] != "UnityEngine.ParticleSystem"
                || !long.TryParse((string)binding?["localFileId"], out long localId))
            {
                return false;
            }

            // Native sampling masks retain Texture2D import settings; sprite configuration would change emission.
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>((string)reference);
            return texture != null
                && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(texture, out string guid, out long importedId)
                && importedId == localId;
        }

        internal static void Configure(JObject document, bool dryRun, bool whitebox = false)
        {
            HashSet<string> listed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JObject resource in document["visualTextures"] as JArray ?? new JArray())
            {
                string resourcePath = (string)resource["path"];
                if (string.IsNullOrEmpty(resourcePath) || !listed.Add(resourcePath))
                {
                    throw new InvalidOperationException("Missing or duplicate texture path: " + resourcePath);
                }
            }
            foreach (JToken reference in document.SelectTokens("$..assetPath"))
            {
                string resourcePath = (string)reference;
                if (resourcePath != null && resourcePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                    && !listed.Contains(resourcePath) && !IsNativeShapeTextureReference(reference))
                {
                    throw new InvalidOperationException("Unlisted texture binding: " + resourcePath);
                }
            }
            // Validate every source before changing any importer or writing the prefab.
            foreach (JObject item in document["visualTextures"] as JArray ?? new JArray())
            {
                string target = UnityCliAssetAliases.Resolve((string)item["path"]);
                int expectedWidth = (int?)item["deliveryWidth"] ?? 0;
                int expectedHeight = (int?)item["deliveryHeight"] ?? 0;
                if (expectedWidth <= 0 || expectedHeight <= 0 || expectedWidth > 8192 || expectedHeight > 8192)
                {
                    throw new InvalidOperationException("Missing or invalid delivery dimensions: " + target);
                }
                if (target == null || !target.StartsWith("Assets/", StringComparison.Ordinal) || target.Contains("..") || target.Contains("\\"))
                {
                    throw new InvalidOperationException("Invalid texture path: " + target);
                }
                if (!whitebox)
                {
                    using (SHA256 sha = SHA256.Create())
                    using (FileStream input = File.OpenRead(target))
                    {
                        string actualHash = BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "").ToLowerInvariant();
                        if (!string.Equals(actualHash, (string)item["deliverySha256"], StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("Missing or mismatched deliverySha256: " + target);
                        }
                    }
                }
                JObject importSettings = item["settings"] as JObject;
                float pixelsPerUnit = (float?)importSettings?["spritePixelsToUnits"] ?? 0;
                JObject edges = importSettings?["spriteBorder"] as JObject;
                float left = (float?)edges?["x"] ?? -1;
                float bottom = (float?)edges?["y"] ?? -1;
                float right = (float?)edges?["z"] ?? -1;
                float top = (float?)edges?["w"] ?? -1;
                if (!(pixelsPerUnit > 0) || float.IsInfinity(pixelsPerUnit) || (int?)importSettings?["spriteMode"] != 1
                    || (importSettings?["wrapMode"] != null && importSettings["wrapMode"].Type != JTokenType.Integer)
                    || ((int?)importSettings?["wrapMode"] ?? 1) < 0 || ((int?)importSettings?["wrapMode"] ?? 1) > 1
                    || !(left >= 0 && right >= 0 && bottom >= 0 && top >= 0)
                    || left + right > expectedWidth || bottom + top > expectedHeight)
                {
                    throw new InvalidOperationException("Invalid sprite settings or nine-slice bounds: " + target);
                }
                if (whitebox)
                {
                    continue;
                }
                TextureImporter source = AssetImporter.GetAtPath(target) as TextureImporter;
                if (source == null)
                {
                    throw new InvalidOperationException("Missing imported texture: " + target);
                }
                source.GetSourceTextureWidthAndHeight(out int width, out int height);
                if (width != expectedWidth || height != expectedHeight)
                {
                    throw new InvalidOperationException($"Texture size mismatch: {target}; expected {expectedWidth}x{expectedHeight}, actual {width}x{height}.");
                }
            }
            if (whitebox)
            {
                return;
            }
            foreach (JObject item in document["visualTextures"] as JArray ?? new JArray())
            {
                string path = UnityCliAssetAliases.Resolve((string)item["path"]);
                if (path == null || !path.StartsWith("Assets/", StringComparison.Ordinal) || path.Contains(".."))
                {
                    throw new InvalidOperationException("Invalid texture target.");
                }
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException("Missing imported texture: " + path);
                }
                if (dryRun)
                {
                    continue;
                }
                JObject settings = (JObject)item["settings"];
                if ((int?)settings["spriteMode"] != 1)
                {
                    throw new InvalidOperationException("Only single-sprite evidence is supported.");
                }
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = (float?)settings["spritePixelsToUnits"] ?? 100;
                JObject border = (JObject)settings["spriteBorder"];
                importer.spriteBorder = new Vector4((float)border["x"], (float)border["y"], (float)border["z"], (float)border["w"]);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 8192;
                importer.wrapMode = (TextureWrapMode)((int?)settings["wrapMode"] ?? 1);
                importer.SaveAndReimport();
            }
        }

        internal static void Render(GameObject root, Scene scene, JObject settings, Action beforeRender = null)
        {
            int width = (int)settings["width"];
            int height = (int)settings["height"];
            string output = Path.GetFullPath((string)settings["output"]);
            if (width < 64 || height < 64 || width > 4096 || height > 4096 || !output.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid preview output.");
            }
            GameObject host = new GameObject("SnapshotPreview", typeof(RectTransform), typeof(Canvas));
            SceneManager.MoveGameObjectToScene(host, scene);
            Canvas canvas = host.GetComponent<Canvas>();
            // Keep the design canvas independent of the inspection camera. Zooming
            // must never resize stretch anchors or change approved node positions.
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRect = host.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(width, height);
            float viewHeight = (float?)settings["viewHeight"] ?? height;
            float centerX = (float?)settings["centerX"] ?? 0f;
            float centerY = (float?)settings["centerY"] ?? 0f;
            if (float.IsNaN(viewHeight) || float.IsInfinity(viewHeight) || viewHeight <= 0 || viewHeight > 100000
                || float.IsNaN(centerX) || float.IsInfinity(centerX) || float.IsNaN(centerY) || float.IsInfinity(centerY))
            {
                throw new InvalidOperationException("Invalid preview camera view.");
            }
            GameObject cameraObject = new GameObject("SnapshotCamera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.scene = scene;
            camera.transform.position = new Vector3(0, 0, -100);
            camera.orthographic = true;
            camera.orthographicSize = viewHeight / 2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = (string)settings["previewBackground"] == "light"
                ? new Color(0.88f, 0.88f, 0.88f, 1f) : new Color(0.08f, 0.09f, 0.13f, 1);
            canvas.worldCamera = camera;
            canvas.planeDistance = 10;
            RenderTexture target = new RenderTexture(width, height, 24);
            Texture2D pixels = new Texture2D(width, height, TextureFormat.RGBA32, false);
            RenderTexture previous = RenderTexture.active;
            try
            {
                camera.targetTexture = target;
                // A prefab with its own overlay Canvas must not resize itself to the
                // current editor GameView while rendered under an isolated world canvas.
                Canvas rootCanvas = root.GetComponent<Canvas>();
                if (rootCanvas != null)
                {
                    CanvasScaler scaler = root.GetComponent<CanvasScaler>();
                    if (scaler != null)
                    {
                        scaler.enabled = false;
                    }
                    rootCanvas.renderMode = RenderMode.WorldSpace;
                    rootCanvas.worldCamera = camera;
                }
                root.transform.SetParent(host.transform, false);
                if (rootCanvas != null && root.transform is RectTransform rootRect)
                {
                    rootRect.anchorMin = Vector2.zero;
                    rootRect.anchorMax = Vector2.one;
                    rootRect.anchoredPosition = Vector2.zero;
                    rootRect.sizeDelta = Vector2.zero;
                    rootRect.localScale = Vector3.one;
                }
                Canvas.ForceUpdateCanvases();
                // Include negative and positive UI depth without flattening nodes.
                float minDepth = 0f;
                float maxDepth = 0f;
                Vector3[] corners = new Vector3[4];
                foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>())
                {
                    rect.GetWorldCorners(corners);
                    foreach (Vector3 corner in corners)
                    {
                        minDepth = Mathf.Min(minDepth, corner.z);
                        maxDepth = Mathf.Max(maxDepth, corner.z);
                    }
                }
                camera.transform.position = new Vector3(centerX, centerY, minDepth - 100f);
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = maxDepth - minDepth + 200f;
                // Effects must simulate only after the isolated host has finalized its transforms.
                beforeRender?.Invoke();
                if ((bool?)settings["hideUiGraphics"] == true)
                {
                    // Change only the isolated draw state after layout and animation sampling.
                    // Disabling Graphic components would invalidate layout ownership and could
                    // move effect anchors; CanvasRenderer alpha leaves geometry and assets intact.
                    foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
                    {
                        graphic.canvasRenderer.SetAlpha(0f);
                    }
                }
                if ((bool?)settings["frameParticles"] == true)
                {
                    Bounds bounds = new Bounds();
                    bool found = false;
                    foreach (ParticleSystemRenderer renderer in root.GetComponentsInChildren<ParticleSystemRenderer>())
                    {
                        if ((int?)settings["particleFramingVersion"] == 3 || (int?)settings["particleFramingVersion"] == 4)
                        {
                            continue;
                        }
                        ParticleSystem particles = renderer.GetComponent<ParticleSystem>();
                        if (!renderer.enabled || particles == null || particles.particleCount == 0)
                        {
                            continue;
                        }
                        Bounds observed = renderer.bounds;
                        if (!found)
                        {
                            bounds = observed;
                            found = true;
                        }
                        else
                        {
                            bounds.Encapsulate(observed);
                        }
                    }
                    if ((int?)settings["particleFramingVersion"] == 2 || (int?)settings["particleFramingVersion"] == 4)
                    {
                        foreach (ParticleSystem particles in root.GetComponentsInChildren<ParticleSystem>())
                        {
                            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
                            if (renderer == null || !renderer.enabled || particles.particleCount == 0)
                            {
                                continue;
                            }
                            ParticleSystem.Particle[] sample = new ParticleSystem.Particle[particles.particleCount];
                            int count = particles.GetParticles(sample);
                            ParticleSystem.MainModule main = particles.main;
                            Vector3 scale = main.scalingMode == ParticleSystemScalingMode.Shape ? Vector3.one
                                : main.scalingMode == ParticleSystemScalingMode.Local ? particles.transform.localScale : particles.transform.lossyScale;
                            float maximumScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                            for (int index = 0; index < count; index++)
                            {
                                Vector3 position = main.simulationSpace == ParticleSystemSimulationSpace.World
                                    ? sample[index].position : particles.transform.TransformPoint(sample[index].position);
                                Vector3 size = sample[index].GetCurrentSize3D(particles);
                                float diameter = Mathf.Max(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z)) * maximumScale;
                                Bounds particleBounds = new Bounds(position, Vector3.one * diameter);
                                if (!found)
                                {
                                    bounds = particleBounds;
                                    found = true;
                                }
                                else
                                {
                                    bounds.Encapsulate(particleBounds);
                                }
                            }
                        }
                    }
                    if ((int?)settings["particleFramingVersion"] == 3)
                    {
                        AccumulateBakedParticleBounds(root, camera, ref bounds, ref found);
                    }
                    if ((bool?)settings["frameContent"] == true)
                    {
                        // Only visible graphic bounds belong to the overview; source transforms remain untouched.
                        foreach (UnityEngine.UI.Graphic graphic in root.GetComponentsInChildren<UnityEngine.UI.Graphic>())
                        {
                            if (!graphic.isActiveAndEnabled || graphic.color.a <= 0f || graphic.canvasRenderer.GetAlpha() <= 0f)
                            {
                                continue;
                            }
                            graphic.rectTransform.GetWorldCorners(corners);
                            foreach (Vector3 corner in corners)
                            {
                                if (!found)
                                {
                                    bounds = new Bounds(corner, Vector3.zero);
                                    found = true;
                                }
                                else
                                {
                                    bounds.Encapsulate(corner);
                                }
                            }
                        }
                    }
                    if (found)
                    {
                        float extent = Mathf.Max(bounds.size.y, bounds.size.x * height / width);
                        if (float.IsNaN(extent) || float.IsInfinity(extent) || extent > 100000)
                        {
                            throw new InvalidOperationException("Particle preview bounds exceed the camera policy.");
                        }
                        camera.orthographicSize = Mathf.Max(0.01f, extent * 0.6f);
                        camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, bounds.min.z - 100f);
                        camera.farClipPlane = Mathf.Max(200f, bounds.size.z + 200f);
                    }
                }
                if (((int?)settings["particleFramingVersion"] == 3 || (int?)settings["particleFramingVersion"] == 4) && (bool?)settings["frameParticles"] == true)
                {
                    FitParticlePixels(camera, target, pixels, width, height, output, (int)settings["particleFramingVersion"]);
                }
                else
                {
                    camera.Render();
                    RenderTexture.active = target;
                    pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    pixels.Apply();
                }
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllBytes(output, pixels.EncodeToPNG());
                // Record actual generated text metrics to distinguish source styling
                // from importer or preview rendering faults without changing content.
                JArray textMetrics = new JArray();
                foreach (UnityEngine.UI.Text text in root.GetComponentsInChildren<UnityEngine.UI.Text>())
                {
                    textMetrics.Add(new JObject
                    {
                        ["node"] = text.name,
                        ["fontSize"] = text.fontSize,
                        ["bestFit"] = text.resizeTextForBestFit,
                        ["generatedFontSize"] = text.cachedTextGenerator.fontSizeUsedForBestFit,
                        ["horizontalOverflow"] = text.horizontalOverflow.ToString(),
                        ["width"] = text.rectTransform.rect.width,
                        ["height"] = text.rectTransform.rect.height,
                        ["lineCount"] = text.cachedTextGenerator.lineCount,
                        ["font"] = text.font == null ? "" : text.font.name
                    });
                }
                File.WriteAllText(output + ".text-metrics.json", textMetrics.ToString());
                UnityCliPreviewUiMetrics.Write(root, host, camera, output);
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                UnityEngine.Object.DestroyImmediate(pixels);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void AccumulateBakedParticleBounds(GameObject root, Camera camera, ref Bounds bounds, ref bool found)
        {
            int totalVertices = 0;
            foreach (ParticleSystemRenderer renderer in root.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                ParticleSystem particles = renderer.GetComponent<ParticleSystem>();
                if (!renderer.enabled || particles == null || particles.particleCount == 0)
                {
                    continue;
                }
                int verticesPerParticle = 4;
                if (renderer.renderMode == ParticleSystemRenderMode.Mesh)
                {
                    Mesh[] sources = new Mesh[renderer.meshCount];
                    renderer.GetMeshes(sources);
                    foreach (Mesh source in sources)
                    {
                        if (source != null)
                        {
                            verticesPerParticle = Mathf.Max(verticesPerParticle, source.vertexCount);
                        }
                    }
                }
                if ((long)verticesPerParticle * particles.particleCount > 500000)
                {
                    throw new InvalidOperationException("Particle geometry sampling exceeds 500000 vertices per renderer.");
                }
                Mesh mesh = new Mesh();
                try
                {
                    // Bake world coordinates explicitly; applying the owner transform again would shift the camera.
                    renderer.BakeMesh(mesh, camera, ParticleSystemBakeMeshOptions.BakeRotationAndScale | ParticleSystemBakeMeshOptions.BakePosition);
                    IncludeBakedVertices(mesh, ref bounds, ref found, ref totalVertices);
                    if (particles.trails.enabled)
                    {
                        mesh.Clear();
                        renderer.BakeTrailsMesh(mesh, camera, ParticleSystemBakeMeshOptions.BakeRotationAndScale | ParticleSystemBakeMeshOptions.BakePosition);
                        IncludeBakedVertices(mesh, ref bounds, ref found, ref totalVertices);
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
            }
        }

        private static void IncludeBakedVertices(Mesh mesh, ref Bounds bounds, ref bool found, ref int totalVertices)
        {
            totalVertices += mesh.vertexCount;
            if (mesh.vertexCount > 500000 || totalVertices > 1000000)
            {
                throw new InvalidOperationException("Particle preview geometry exceeds its bounded sampling policy.");
            }
            Vector3[] vertices = mesh.vertices;
            Color32[] colors = mesh.colors32;
            for (int index = 0; index < vertices.Length; index++)
            {
                if (colors.Length == vertices.Length && colors[index].a == 0)
                {
                    continue;
                }
                Vector3 world = vertices[index];
                if (!found)
                {
                    bounds = new Bounds(world, Vector3.zero);
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(world);
                }
            }
        }

        private static void FitParticlePixels(Camera camera, RenderTexture target, Texture2D pixels, int width, int height, string output, int framingVersion)
        {
            int mask = camera.cullingMask;
            Color32 background;
            try
            {
                // Sample the actual clear color through the same render target and color space.
                camera.cullingMask = 0;
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                pixels.Apply();
                background = pixels.GetPixels32()[0];
            }
            finally
            {
                camera.cullingMask = mask;
            }
            JObject metrics = new JObject
            {
                ["algorithm"] = framingVersion == 4 ? "native-position-and-pixel-frame-v4" : "baked-geometry-and-pixel-frame-v3"
            };
            bool complete = false;
            for (int pass = 0; pass < 10; pass++)
            {
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                pixels.Apply();
                Color32[] data = pixels.GetPixels32();
                int minX = width;
                int minY = height;
                int maxX = -1;
                int maxY = -1;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color32 color = data[y * width + x];
                        if (Mathf.Abs(color.r - background.r) <= 2 && Mathf.Abs(color.g - background.g) <= 2 && Mathf.Abs(color.b - background.b) <= 2)
                        {
                            continue;
                        }
                        minX = Mathf.Min(minX, x);
                        minY = Mathf.Min(minY, y);
                        maxX = Mathf.Max(maxX, x);
                        maxY = Mathf.Max(maxY, y);
                    }
                }
                metrics["passes"] = pass + 1;
                metrics["visiblePixelsFound"] = maxX >= 0;
                metrics["pixelBounds"] = new JArray(minX, minY, maxX, maxY);
                if (maxX < 0)
                {
                    complete = true;
                    break;
                }
                float extent = camera.orthographicSize;
                if (minX < 3 || minY < 3 || maxX >= width - 3 || maxY >= height - 3)
                {
                    camera.orthographicSize *= 1.8f;
                }
                else
                {
                    // Native screen-size clamps can prevent enlargement; a fully contained final sample remains faithful.
                    if (pass == 9)
                    {
                        metrics["nativeProjectionLimitedFill"] = true;
                        complete = true;
                        break;
                    }
                    float fraction = Mathf.Max((maxX - minX + 1f) / width, (maxY - minY + 1f) / height);
                    float deltaX = ((minX + maxX + 1f) / width - 1f) * extent * width / height;
                    float deltaY = ((minY + maxY + 1f) / height - 1f) * extent;
                    if (fraction >= 0.72f && fraction <= 0.88f && Mathf.Abs(deltaX) <= extent * 0.02f && Mathf.Abs(deltaY) <= extent * 0.02f)
                    {
                        complete = true;
                        break;
                    }
                    camera.transform.position += new Vector3(deltaX, deltaY, 0f);
                    camera.orthographicSize = Mathf.Max(0.01f, extent * fraction / 0.8f);
                }
                if (float.IsNaN(camera.orthographicSize) || float.IsInfinity(camera.orthographicSize) || camera.orthographicSize > 100000f)
                {
                    throw new InvalidOperationException("Particle pixel framing exceeds its camera policy.");
                }
            }
            if (!complete)
            {
                throw new InvalidOperationException("Particle pixel framing did not converge within ten isolated renders.");
            }
            metrics["orthographicSize"] = camera.orthographicSize;
            metrics["cameraPosition"] = new JArray(camera.transform.position.x, camera.transform.position.y, camera.transform.position.z);
            metrics["sourceTransformsModified"] = false;
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output + ".particle-framing.json", metrics.ToString());
        }
    }
}
