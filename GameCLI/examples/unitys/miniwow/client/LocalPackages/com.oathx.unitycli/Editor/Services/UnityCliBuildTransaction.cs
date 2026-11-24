using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Restores exact previous bytes on handled write failures, including importer metadata.</summary>
    internal sealed class UnityCliBuildTransaction : IDisposable
    {
        private readonly Dictionary<string, byte[]> originals = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool committed;

        internal UnityCliBuildTransaction(string prefab, JObject snapshot, JObject window, string previews, JObject manifest = null)
        {
            Capture(prefab);
            Capture(prefab + ".meta");
            foreach (JObject texture in snapshot["visualTextures"] as JArray ?? new JArray())
            {
                Capture((string)texture["path"] + ".meta");
            }
            if (manifest != null)
            {
                foreach (JObject resource in ((JArray)manifest["resources"]).Concat(manifest["assets"] as JArray ?? new JArray()))
                {
                    string resourcePath = (string)resource["path"];
                    if (resourcePath != null && resourcePath.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                    {
                        Capture(resourcePath);
                        Capture(resourcePath + ".meta");
                    }
                }
            }
            if (!string.IsNullOrEmpty(previews))
            {
                foreach (JObject state in window["previewStates"] as JArray ?? new JArray())
                {
                    string path = Path.Combine(previews, (string)state["id"] + ".png");
                    Capture(path);
                    Capture(path + ".text-metrics.json");
                    Capture(path + ".ui-metrics.json");
                    Capture(path + ".particle-framing.json");
                }
            }
        }

        private void Capture(string path)
        {
            string full = Path.GetFullPath(path);
            if (originals.ContainsKey(full))
            {
                return;
            }
            originals.Add(full, File.Exists(full) ? File.ReadAllBytes(full) : null);
            string parent = Path.GetDirectoryName(full);
            while (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
            {
                folders.Add(parent);
                parent = Path.GetDirectoryName(parent);
            }
        }

        internal void Commit()
        {
            committed = true;
        }

        public void Dispose()
        {
            if (committed)
            {
                return;
            }
            // Restore a snapshot, never synthesize or edit serialized prefab contents.
            foreach (KeyValuePair<string, byte[]> entry in originals)
            {
                if (entry.Value == null)
                {
                    if (File.Exists(entry.Key))
                    {
                        File.Delete(entry.Key);
                    }
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(entry.Key));
                    File.WriteAllBytes(entry.Key, entry.Value);
                }
            }
            foreach (string folder in folders.OrderByDescending(p => p.Length))
            {
                if (Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
                {
                    Directory.Delete(folder);
                    if (File.Exists(folder + ".meta"))
                    {
                        File.Delete(folder + ".meta");
                    }
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
    }
}
