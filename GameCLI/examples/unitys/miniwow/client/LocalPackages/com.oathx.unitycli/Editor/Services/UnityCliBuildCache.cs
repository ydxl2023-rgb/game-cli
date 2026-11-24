using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Local derived build evidence; never grants authorization or replaces platform state.</summary>
    internal static class UnityCliBuildCache
    {
        internal static string Key(UnityCliBridgeRequest request, JObject manifest)
        {
            StringBuilder input = new StringBuilder("ui-build-cache/1\n" + Application.unityVersion + "\n" + request.screenshotPath + "\n");
            string[] paths = ((JArray)manifest["resources"]).Concat(manifest["assets"] as JArray ?? new JArray())
                .Select(a => (string)a["path"]).ToArray();
            string[] dependencies = AssetDatabase.GetDependencies(paths, true);
            string[] settings = Directory.Exists("ProjectSettings") ? Directory.GetFiles("ProjectSettings", "*.asset") : new string[] {};
            foreach (string path in dependencies.Concat(paths).Concat(settings)
                .Concat(new[] { request.jsonFile, request.assetManifestPath, typeof(UnityCliBuildCache).Assembly.Location })
                .Distinct().OrderBy(p => p, StringComparer.Ordinal))
            {
                input.Append(path).Append(':').Append(Hash(path)).Append(':').Append(Hash(path + ".meta")).Append('\n');
            }
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(input.ToString()))).Replace("-", "").ToLowerInvariant();
            }
        }

        private static string PathFor(string output)
        {
            using (SHA256 sha = SHA256.Create())
            {
                string id = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(output))).Replace("-", "");
                return Path.Combine("Library", "UnityCLI", "build-cache", id + ".json");
            }
        }

        internal static JObject Read(string output, string key)
        {
            try
            {
                string path = PathFor(output);
                if (!File.Exists(path))
                {
                    return null;
                }
                JObject cache = JObject.Parse(File.ReadAllText(path));
                if (!(cache["report"] is JObject cachedReport) || !(cachedReport["previews"] is JArray)
                    || cachedReport["nodeCount"]?.Type != JTokenType.Integer || cachedReport["componentCount"]?.Type != JTokenType.Integer)
                {
                    return null;
                }
                if ((string)cache["key"] != key || (string)cache["prefabHash"] != Hash(output)
                    || (string)cache["metaHash"] != Hash(output + ".meta") || !File.Exists(output))
                {
                    return null;
                }
                foreach (JObject preview in (JArray)cache["report"]["previews"])
                {
                    string file = (string)preview["path"];
                    if (!File.Exists(file) || (string)preview["sha256"] != Hash(file)
                        || (string)preview["metricsHash"] != Hash(file + ".text-metrics.json"))
                    {
                        return null;
                    }
                }
                return (JObject)cache["report"].DeepClone();
            }
            catch (Exception error) when (error is IOException || error is Newtonsoft.Json.JsonException || error is InvalidCastException || error is ArgumentException)
            {
                // Corrupt local derived evidence is a cache miss, not a successful build.
                return null;
            }
        }

        internal static void Save(string output, string key, JObject report)
        {
            string path = PathFor(output);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            JObject cache = new JObject { ["key"] = key, ["prefabHash"] = Hash(output), ["metaHash"] = Hash(output + ".meta"), ["report"] = report.DeepClone() };
            File.WriteAllText(path + ".tmp", cache.ToString());
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(path + ".tmp", path);
        }

        internal static string Hash(string path)
        {
            if (!File.Exists(path))
            {
                return "missing";
            }
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
