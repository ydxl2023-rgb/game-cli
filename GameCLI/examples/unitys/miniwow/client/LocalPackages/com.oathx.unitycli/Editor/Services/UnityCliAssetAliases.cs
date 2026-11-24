using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Resolves explicit, identity-preserving UI renames after approval verification.</summary>
    internal static class UnityCliAssetAliases
    {
        internal static string Resolve(string path)
        {
            const string registry = "ProjectSettings/UnityCliAssetAliases.json";
            if (!File.Exists(registry))
            {
                return path;
            }
            JObject document = JObject.Parse(File.ReadAllText(registry));
            if ((string)document["schemaVersion"] != "unity-asset-path-aliases/1" || !(document["assets"] is JArray entries))
            {
                throw new InvalidOperationException("Invalid asset alias registry.");
            }
            foreach (JObject entry in entries)
            {
                if ((string)entry["from"] != path)
                {
                    continue;
                }
                string target = (string)entry["to"];
                if (!Regex.IsMatch(path, @"^Assets/UI/(?:[A-Za-z0-9_-]+/)+[a-f0-9]{64}\.(?:png|ttf|otf|ttc)$") ||
                    !Regex.IsMatch(target ?? "", @"^Assets/UI/(?:[A-Za-z0-9_-]+/)+[A-Za-z][A-Za-z0-9_]{0,63}\.(?:png|ttf|otf|ttc)$") ||
                    Path.GetExtension(path) != Path.GetExtension(target) || Path.GetDirectoryName(path) != Path.GetDirectoryName(target) || File.Exists(path))
                {
                    throw new InvalidOperationException("Invalid or recreated legacy asset alias: " + path);
                }
                if (AssetDatabase.AssetPathToGUID(target) != (string)entry["guid"] ||
                    Hash(target) != (string)entry["sha256"] || Hash(target + ".meta") != (string)entry["metaSha256"])
                {
                    throw new InvalidOperationException("Migrated asset identity mismatch: " + target);
                }
                return target;
            }
            return path;
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream input = File.OpenRead(path))
            {
                return BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
