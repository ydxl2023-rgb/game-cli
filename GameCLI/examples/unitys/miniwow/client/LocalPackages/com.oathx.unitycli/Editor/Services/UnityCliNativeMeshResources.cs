using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Normalizes geometry-only model imports and captures verified built-in meshes through fixed native APIs.</summary>
    internal static class UnityCliNativeMeshResources
    {
        internal static PrefabData Configure(JObject document, bool dryRun)
        {
            JArray evidence = new JArray();
            foreach (JObject item in document["models"] as JArray ?? new JArray())
            {
                string path = AssetPath(item);
                if (!File.Exists(path) || Hash(File.ReadAllBytes(path)) != (string)item["sha256"])
                {
                    throw new InvalidOperationException("Native geometry source digest mismatch.");
                }
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null || AssetDatabase.AssetPathToGUID(path) != (string)item["sourceGuid"])
                {
                    throw new InvalidOperationException("Native model importer or GUID differs from source evidence.");
                }
                Dictionary<long, string> before = MeshFingerprints(path);
                foreach (JToken expected in item["localFileIds"] as JArray ?? new JArray())
                {
                    if (!long.TryParse((string)expected, out long localId) || !before.ContainsKey(localId))
                    {
                        throw new InvalidOperationException("Expected source mesh local ID is missing: " + path);
                    }
                }
                if (!dryRun)
                {
                    byte[] metadata = File.ReadAllBytes(path + ".meta");
                    importer.materialImportMode = ModelImporterMaterialImportMode.None;
                    importer.importAnimation = false;
                    importer.SaveAndReimport();
                    Dictionary<long, string> after = MeshFingerprints(path);
                    if (before.Count != after.Count || before.Any(pair => !after.TryGetValue(pair.Key, out string observed) || observed != pair.Value))
                    {
                        // Restore the exact imported metadata; never synthesize a replacement mesh on mismatch.
                        File.WriteAllBytes(path + ".meta", metadata);
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                        throw new InvalidOperationException("Geometry changed while disabling unused model materials or animations: " + path);
                    }
                }
                evidence.Add(new JObject { ["path"] = path, ["sha256"] = Hash(File.ReadAllBytes(path)), ["metaSha256"] = Hash(File.ReadAllBytes(path + ".meta")), ["geometryUnchanged"] = !dryRun, ["meshCount"] = before.Count });
            }
            foreach (JObject item in document["builtins"] as JArray ?? new JArray())
            {
                string path = AssetPath(item);
                if ((string)item["primitive"] != "Cube" || (string)item["sourceGuid"] != "0000000000000000e000000000000000" || (string)item["sourceLocalFileId"] != "10202" || File.Exists(path))
                {
                    throw new InvalidOperationException("Only a fresh, explicitly identified built-in Cube is supported.");
                }
                Mesh original = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                if (original == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(original, out string guid, out long localId)
                    || guid != (string)item["sourceGuid"] || localId.ToString() != (string)item["sourceLocalFileId"])
                {
                    throw new InvalidOperationException("Target Unity built-in mesh identity differs from source evidence.");
                }
                long targetId = 0;
                if (!dryRun)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    Mesh copy = UnityEngine.Object.Instantiate(original);
                    AssetDatabase.CreateAsset(copy, path);
                    AssetDatabase.SaveAssets();
                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(copy, out string targetGuid, out targetId))
                    {
                        throw new InvalidOperationException("Created primitive mesh has no stable native local ID.");
                    }
                }
                evidence.Add(new JObject { ["path"] = path, ["sourceGuid"] = guid, ["sourceLocalFileId"] = localId.ToString(), ["targetLocalFileId"] = targetId.ToString(), ["sha256"] = dryRun ? null : Hash(File.ReadAllBytes(path)), ["metaSha256"] = dryRun ? null : Hash(File.ReadAllBytes(path + ".meta")), ["vertexCount"] = original.vertexCount, ["unityVersion"] = Application.unityVersion });
            }
            return new PrefabData { action = "configure-native-mesh-resources", valid = true, dryRun = dryRun, textureSettingsJson = evidence.ToString(), message = dryRun ? "Native geometry plans validated without modifying assets." : "Native geometry imports verified and source mesh identities retained." };
        }

        private static Dictionary<long, string> MeshFingerprints(string path)
        {
            Dictionary<long, string> result = new Dictionary<long, string>();
            foreach (Mesh mesh in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>())
            {
                if (mesh.vertexCount > 100000 || mesh.subMeshCount > 16 || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh, out string guid, out long localId))
                {
                    throw new InvalidOperationException("Source mesh exceeds bounded geometry limits.");
                }
                result.Add(localId, Hash(Encoding.UTF8.GetBytes(EditorJsonUtility.ToJson(mesh))));
            }
            if (result.Count == 0)
            {
                throw new InvalidOperationException("Model has no native mesh subassets.");
            }
            return result;
        }

        private static string AssetPath(JObject item)
        {
            string path = (string)item["path"];
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/", StringComparison.Ordinal) || path.Contains("\\")
                || path.Split('/').Any(part => part == "." || part == ".." || part.Length == 0))
            {
                throw new InvalidOperationException("Normalized native geometry asset path is required.");
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
    }
}
