using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Creates deterministic whitebox particle atlases from an existing, hash-verified control sprite.</summary>
    internal static class UnityCliParticleResources
    {
        internal static PrefabData Configure(JObject document, bool dryRun)
        {
            bool verifyExisting = (bool?)document["verifyExisting"] ?? false;
            JArray definitions = document["atlases"] as JArray ?? throw new InvalidOperationException("Particle atlas definitions are required.");
            if (definitions.Count == 0 || definitions.Count > 32)
            {
                throw new InvalidOperationException("Particle atlas count must be in 1..32.");
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
            {
                throw new InvalidOperationException("URP unlit particle shader is unavailable.");
            }
            List<Plan> plans = new List<Plan>();
            HashSet<string> targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            long totalPixels = 0;
            try
            {
                // Validate and calculate all output bytes before writing any asset.
                foreach (JObject definition in definitions)
                {
                    string source = AssetPath(definition, "sourcePath", ".png");
                    string texturePath = AssetPath(definition, "texturePath", ".png");
                    string materialPath = AssetPath(definition, "materialPath", ".mat");
                    int columns = (int?)definition["columns"] ?? 0;
                    int rows = (int?)definition["rows"] ?? 0;
                    if (definition["cpuReadable"] != null && definition["cpuReadable"].Type != JTokenType.Boolean)
                    {
                        throw new InvalidOperationException("Particle texture cpuReadable must be boolean.");
                    }
                    if (columns < 1 || columns > 16 || rows < 1 || rows > 16 || source == texturePath
                        || !targets.Add(texturePath) || !targets.Add(materialPath))
                    {
                        throw new InvalidOperationException("Invalid or duplicate particle atlas output.");
                    }
                    byte[] bytes = File.ReadAllBytes(source);
                    if (bytes.Length > 4 * 1024 * 1024 || Hash(bytes) != (string)definition["sourceSha256"])
                    {
                        throw new InvalidOperationException("Particle source sprite digest or size mismatch.");
                    }
                    Texture2D input = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    Texture2D atlas = null;
                    try
                    {
                        if (!input.LoadImage(bytes, false) || input.width > 512 || input.height > 512
                            || input.width * columns > 4096 || input.height * rows > 4096)
                        {
                            throw new InvalidOperationException("Particle atlas dimensions exceed the bounded resource policy.");
                        }
                        JObject region = definition["sourceRegion"] as JObject;
                        int left = (int?)region?["x"] ?? 0;
                        int bottom = (int?)region?["y"] ?? 0;
                        int tileWidth = (int?)region?["width"] ?? input.width;
                        int tileHeight = (int?)region?["height"] ?? input.height;
                        if (left < 0 || bottom < 0 || tileWidth < 1 || tileHeight < 1
                            || left + tileWidth > input.width || bottom + tileHeight > input.height)
                        {
                            throw new InvalidOperationException("Particle atlas source region is outside the verified sprite.");
                        }
                        totalPixels += (long)tileWidth * tileHeight * columns * rows;
                        if (totalPixels > 16 * 1024 * 1024)
                        {
                            throw new InvalidOperationException("Particle atlas plan exceeds the aggregate pixel budget.");
                        }
                        atlas = new Texture2D(tileWidth * columns, tileHeight * rows, TextureFormat.RGBA32, false);
                        Color32[] sourcePixels = input.GetPixels32();
                        Color32[] output = new Color32[atlas.width * atlas.height];
                        for (int y = 0; y < atlas.height; y++)
                        {
                            for (int x = 0; x < atlas.width; x++)
                            {
                                output[y * atlas.width + x] = sourcePixels[(bottom + y % tileHeight) * input.width + left + x % tileWidth];
                            }
                        }
                        atlas.SetPixels32(output);
                        atlas.Apply();
                        byte[] png = atlas.EncodeToPNG();
                        if (!verifyExisting && (File.Exists(texturePath) || File.Exists(materialPath)))
                        {
                            throw new InvalidOperationException("Use fresh particle resource paths; existing assets are immutable.");
                        }
                        plans.Add(new Plan
                        {
                            TexturePath = texturePath,
                            MaterialPath = materialPath,
                            Bytes = png,
                            Width = atlas.width,
                            Height = atlas.height,
                            Columns = columns,
                            Rows = rows,
                            SourceHash = Hash(bytes),
                            CpuReadable = (bool?)definition["cpuReadable"],
                            SourceRegion = new JObject
                            {
                                ["x"] = left,
                                ["y"] = bottom,
                                ["width"] = tileWidth,
                                ["height"] = tileHeight
                            }
                        });
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(input);
                        if (atlas != null)
                        {
                            UnityEngine.Object.DestroyImmediate(atlas);
                        }
                    }
                }
                JArray evidence = new JArray();
                foreach (Plan plan in plans)
                {
                    if (verifyExisting)
                    {
                        VerifyExisting(plan, shader);
                    }
                    else if (!dryRun)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(plan.TexturePath));
                        Directory.CreateDirectory(Path.GetDirectoryName(plan.MaterialPath));
                        File.WriteAllBytes(plan.TexturePath, plan.Bytes);
                        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                        AssetDatabase.ImportAsset(plan.TexturePath, ImportAssetOptions.ForceSynchronousImport);
                        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(plan.TexturePath);
                        importer.textureType = TextureImporterType.Default;
                        importer.mipmapEnabled = false;
                        importer.isReadable = plan.CpuReadable ?? false;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.alphaIsTransparency = true;
                        importer.wrapMode = TextureWrapMode.Clamp;
                        importer.filterMode = FilterMode.Bilinear;
                        importer.maxTextureSize = 4096;
                        importer.SaveAndReimport();
                        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(plan.TexturePath);
                        if (texture == null || texture.isReadable != (plan.CpuReadable ?? false))
                        {
                            throw new InvalidOperationException("Particle texture CPU readability differs from its declared plan.");
                        }
                        Material material = new Material(shader);
                        material.SetTexture("_BaseMap", texture);
                        if (material.HasProperty("_MainTex"))
                        {
                            material.SetTexture("_MainTex", texture);
                        }
                        Set(material, "_Surface", 1);
                        Set(material, "_Blend", 0);
                        Set(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
                        Set(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                        Set(material, "_SrcBlendAlpha", (float)BlendMode.One);
                        Set(material, "_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
                        Set(material, "_ZWrite", 0);
                        Set(material, "_AlphaClip", 0);
                        // Mirrored source transforms must remain visible without changing their geometry.
                        Set(material, "_Cull", (float)CullMode.Off);
                        material.SetColor("_BaseColor", Color.white);
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.renderQueue = (int)RenderQueue.Transparent;
                        Canonicalize(material);
                        AssetDatabase.CreateAsset(material, plan.MaterialPath);
                        AssetDatabase.SaveAssets();
                        string canonicalHash = Hash(File.ReadAllBytes(plan.MaterialPath));
                        Canonicalize(material);
                        EditorUtility.SetDirty(material);
                        AssetDatabase.SaveAssets();
                        if (Hash(File.ReadAllBytes(plan.MaterialPath)) != canonicalHash)
                        {
                            throw new InvalidOperationException("Particle material does not have stable canonical bytes.");
                        }
                        Material saved = AssetDatabase.LoadAssetAtPath<Material>(plan.MaterialPath);
                        if (saved.shader != shader || saved.GetTexture("_BaseMap") != texture || saved.GetFloat("_Cull") != 0
                            || Hash(File.ReadAllBytes(plan.TexturePath)) != Hash(plan.Bytes))
                        {
                            throw new InvalidOperationException("Particle atlas or material readback mismatch.");
                        }
                    }
                    evidence.Add(new JObject
                    {
                        ["texturePath"] = plan.TexturePath,
                        ["materialPath"] = plan.MaterialPath,
                        ["sourceSha256"] = plan.SourceHash,
                        ["sourceRegion"] = plan.SourceRegion,
                        ["cpuReadable"] = plan.CpuReadable.HasValue ? JToken.FromObject(plan.CpuReadable.Value) : JValue.CreateNull(),
                        ["textureSha256"] = Hash(plan.Bytes),
                        ["materialSha256"] = dryRun && !verifyExisting ? null : Hash(File.ReadAllBytes(plan.MaterialPath)),
                        ["verifiedExisting"] = verifyExisting,
                        ["width"] = plan.Width,
                        ["height"] = plan.Height,
                        ["columns"] = plan.Columns,
                        ["rows"] = plan.Rows,
                        ["tilePixelsUnchanged"] = true,
                        ["doubleSided"] = true
                    });
                }
                return new PrefabData { action = "configure-particle-resources", valid = true, dryRun = dryRun, textureSettingsJson = evidence.ToString(), message = verifyExisting ? "Particle atlas pixels and native material settings verified; no assets modified." : dryRun ? "Particle atlas plans validated; no assets modified." : "Particle atlases and double-sided materials created and read back." };
            }
            finally
            {
                plans.Clear();
            }
        }

        private static void VerifyExisting(Plan plan, Shader shader)
        {
            TextureImporter importer = AssetImporter.GetAtPath(plan.TexturePath) as TextureImporter;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(plan.MaterialPath);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(plan.TexturePath);
            if (!File.Exists(plan.TexturePath) || Hash(File.ReadAllBytes(plan.TexturePath)) != Hash(plan.Bytes)
                || importer == null || importer.textureType != TextureImporterType.Default || importer.mipmapEnabled
                || importer.textureCompression != TextureImporterCompression.Uncompressed || importer.wrapMode != TextureWrapMode.Clamp
                || importer.filterMode != FilterMode.Bilinear || (plan.CpuReadable.HasValue && importer.isReadable != plan.CpuReadable.Value)
                || material == null || material.shader != shader
                || material.GetTexture("_BaseMap") != texture || material.GetColor("_BaseColor") != Color.white
                || material.GetFloat("_Cull") != 0 || material.GetFloat("_ZWrite") != 0 || material.GetFloat("_Surface") != 1
                || material.GetFloat("_SrcBlend") != (float)BlendMode.SrcAlpha
                || material.GetFloat("_DstBlend") != (float)BlendMode.OneMinusSrcAlpha
                || !material.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT") || material.renderQueue != (int)RenderQueue.Transparent)
            {
                throw new InvalidOperationException("Existing particle resource does not match its immutable plan: " + plan.MaterialPath);
            }
        }

        private static void Canonicalize(Material material)
        {
            MaterialEditor editor = UnityEditor.Editor.CreateEditor(material) as MaterialEditor;
            try
            {
                if (editor == null || editor.customShaderGUI == null)
                {
                    throw new InvalidOperationException("The particle shader has no native material validator.");
                }
                // URP derives keywords, passes and fade uniforms. Freeze them before hashing resources.
                editor.customShaderGUI.ValidateMaterial(material);
            }
            finally
            {
                if (editor != null)
                {
                    UnityEngine.Object.DestroyImmediate(editor);
                }
            }
        }

        private static void Set(Material material, string property, float value)
        {
            if (material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }

        private static string AssetPath(JObject value, string field, string extension)
        {
            string path = (string)value[field];
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/", StringComparison.Ordinal)
                || path.Contains("\\") || !path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid particle resource path: " + field);
            }
            foreach (string part in path.Split('/'))
            {
                if (part == "." || part == ".." || part.Length == 0)
                {
                    throw new InvalidOperationException("Particle resource path is not normalized.");
                }
            }
            return path;
        }

        private static string Hash(byte[] bytes)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
            }
        }

        private sealed class Plan
        {
            internal string TexturePath;
            internal string MaterialPath;
            internal byte[] Bytes;
            internal int Width;
            internal int Height;
            internal int Columns;
            internal int Rows;
            internal string SourceHash;
            internal JObject SourceRegion;
            internal bool? CpuReadable;
        }
    }
}
