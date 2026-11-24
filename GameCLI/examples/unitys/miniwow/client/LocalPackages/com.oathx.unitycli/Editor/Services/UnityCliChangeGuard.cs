#nullable disable
using System;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Revalidates source and exact changes before texture imports or prefab writes.</summary>
    internal static class UnityCliChangeGuard
    {
        internal static JObject Resolve(JObject envelope, string output)
        {
            string baseline = (string)envelope["baselineJson"];
            string candidate = (string)envelope["snapshotJson"];
            string approvalText = (string)envelope["approvalJson"];
            if ((string)envelope["adapterVersion"] != "authorized-changes/1"
                || Hash(baseline) != (string)envelope["sourceSha256"]
                || Hash(candidate) != (string)envelope["snapshotSha256"]
                || Hash(approvalText) != (string)envelope["approvalSha256"])
            {
                throw new InvalidOperationException("Change guard content hash mismatch.");
            }
            JObject approval = Parse(approvalText);
            if ((string)approval["schemaVersion"] != "ui-change-approval/1"
                || (string)approval["sourceSha256"] != (string)envelope["sourceSha256"]
                || string.IsNullOrWhiteSpace((string)approval["authorizationReference"])
                || (string)approval["outputPrefab"] != output || (string)envelope["outputPrefab"] != output)
            {
                throw new InvalidOperationException("Change guard approval metadata mismatch.");
            }
            JObject source = Normalize(Parse(baseline));
            JObject target = Normalize(Parse(candidate));
            JArray changes = Diff(source, target, "");
            JArray effective = EffectiveChanges((JArray)approval["changes"], source, target);
            if (!Equal(changes, effective))
            {
                throw new InvalidOperationException("Unauthorized differences from baseline; no assets were changed.");
            }
            // The caller still validates Unity types, hierarchy and references. Approval evidence is external.
            JObject resolved = Parse(candidate);
            if (envelope["resourceManifestJson"] != null)
            {
                string manifest = (string)envelope["resourceManifestJson"];
                if (Hash(manifest) != (string)envelope["resourceManifestSha256"])
                {
                    throw new InvalidOperationException("Resource manifest hash mismatch.");
                }
                resolved = ProjectResources(resolved, Parse(manifest));
            }
            return resolved;
        }

        private static JObject ProjectResources(JObject target, JObject manifest)
        {
            JObject approvedPrefab = (JObject)Normalize(target)["prefab"];
            JObject manifestPrefab = (JObject)Normalize(manifest)["prefab"];
            // Expanded instance provenance is not part of the flat restore input.
            approvedPrefab.Remove("instances");
            manifestPrefab.Remove("instances");
            if (!Equal(approvedPrefab, manifestPrefab))
            {
                throw new InvalidOperationException("Resource manifest changes the approved prefab.");
            }
            var original = ((JArray)target["visualTextures"]).OfType<JObject>().ToDictionary(item => (string)item["path"], StringComparer.Ordinal);
            var selected = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            JArray textures = new JArray();
            foreach (JObject item in (JArray)manifest["visualTextures"])
            {
                string path = (string)item["path"];
                JObject approved;
                if (!selected.Add(path) || !original.TryGetValue(path, out approved) || !Equal(approved, item))
                {
                    throw new InvalidOperationException("Resource manifest adds or changes an approved texture.");
                }
                textures.Add(approved.DeepClone());
            }
            foreach (JProperty reference in ((JObject)target["prefab"]).Descendants().OfType<JProperty>().Where(property => property.Name == "assetPath"))
            {
                string path = (string)reference.Value;
                if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !selected.Contains(path))
                {
                    throw new InvalidOperationException("Resource manifest omits a bound texture: " + path);
                }
            }
            JObject result = (JObject)target.DeepClone();
            result["visualTextures"] = textures;
            return result;
        }

        private static JObject Parse(string text)
        {
            // Do not round authorization values through double before comparing them.
            // Values outside decimal range are rejected rather than silently authorized.
            using (JsonTextReader reader = new JsonTextReader(new StringReader(text)))
            {
                reader.FloatParseHandling = FloatParseHandling.Decimal;
                reader.DateParseHandling = DateParseHandling.None;
                JObject result = JObject.Load(reader);
                string[] numbers = Regex.Matches(text, "\"(?:\\\\.|[^\"\\\\])*\"|-?(?:0|[1-9][0-9]*)(?:\\.[0-9]+)?(?:[eE][+-]?[0-9]+)?")
                    .Cast<Match>().Where(match => !match.Value.StartsWith("\"", StringComparison.Ordinal)).Select(match => NumberKey(match.Value)).ToArray();
                string[] parsed = result.Descendants().OfType<JValue>().Where(IsNumber).Select(value => NumberKey(value.ToString(Formatting.None))).ToArray();
                if (!numbers.SequenceEqual(parsed))
                {
                    throw new InvalidOperationException("Guard numeric value cannot be represented exactly by this Unity adapter.");
                }
                return result;
            }
        }

        private static bool Equal(JToken left, JToken right)
        {
            if (left is JObject a && right is JObject b)
            {
                return a.Count == b.Count && a.Properties().All(property => b.Property(property.Name) != null && Equal(property.Value, b[property.Name]));
            }
            if (left is JArray x && right is JArray y)
            {
                return x.Count == y.Count && x.Select((item, index) => Equal(item, y[index])).All(equal => equal);
            }
            if (left != null && right != null && IsNumber(left) && IsNumber(right))
            {
                return NumberKey(left.ToString(Formatting.None)) == NumberKey(right.ToString(Formatting.None));
            }
            return JToken.DeepEquals(left, right);
        }

        private static bool IsNumber(JToken value)
        {
            return value.Type == JTokenType.Integer || value.Type == JTokenType.Float;
        }

        private static string NumberKey(string value)
        {
            string[] parts = value.ToLowerInvariant().Split('e');
            string mantissa = parts[0];
            int point = mantissa.IndexOf('.');
            BigInteger exponent = parts.Length == 2 ? BigInteger.Parse(parts[1], CultureInfo.InvariantCulture) : BigInteger.Zero;
            if (point >= 0)
            {
                exponent -= mantissa.Length - point - 1;
            }
            string sign = mantissa.StartsWith("-", StringComparison.Ordinal) ? "-" : "";
            string digits = mantissa.TrimStart('-').Replace(".", "").TrimStart('0');
            if (digits.Length == 0)
            {
                return "0";
            }
            int length = digits.Length;
            digits = digits.TrimEnd('0');
            exponent += length - digits.Length;
            return sign + digits + "e" + exponent.ToString(CultureInfo.InvariantCulture);
        }

        private static JArray EffectiveChanges(JArray allowed, JObject source, JObject target)
        {
            JArray result = new JArray();
            System.Collections.Generic.HashSet<string> paths = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            foreach (JObject change in allowed)
            {
                string path = (string)change["path"];
                if (change.Count != 5 || path == null || !paths.Add(path))
                {
                    throw new InvalidOperationException("Duplicate or invalid approval change.");
                }
                if ((bool)change["beforeExists"] && (bool)change["afterExists"] && Equal(change["before"], change["after"]))
                {
                    JToken before;
                    JToken after;
                    if (!TryResolve(source, path, out before) || !TryResolve(target, path, out after)
                        || !Equal(before, change["before"]) || !Equal(after, change["after"]))
                    {
                        throw new InvalidOperationException("No-op approval does not match the pinned snapshots.");
                    }
                    continue;
                }
                result.Add(change.DeepClone());
            }
            return result;
        }

        private static bool TryResolve(JToken root, string path, out JToken value)
        {
            value = root;
            if (!path.StartsWith("/", StringComparison.Ordinal))
            {
                return false;
            }
            foreach (string part in path.Substring(1).Split('/'))
            {
                string key = part.Replace("~1", "/").Replace("~0", "~");
                JObject obj = value as JObject;
                if (obj == null || obj.Property(key) == null)
                {
                    return false;
                }
                value = obj[key];
            }
            return true;
        }

        private static JObject Normalize(JObject value)
        {
            if ((string)value["schemaVersion"] != "ui-template/1")
            {
                throw new InvalidOperationException("Expected ui-template/1 baseline and candidate.");
            }
            JObject result = (JObject)value.DeepClone();
            JObject prefab = (JObject)result["prefab"];
            JObject nodes = Index((JArray)prefab["nodes"]);
            foreach (JProperty node in nodes.Properties())
            {
                node.Value["components"] = Index((JArray)node.Value["components"]);
            }
            prefab["nodes"] = nodes;
            return result;
        }

        private static JObject Index(JArray values)
        {
            JObject result = new JObject();
            foreach (JToken value in values)
            {
                string id = (string)value["id"];
                if (string.IsNullOrWhiteSpace(id) || result.Property(id) != null)
                {
                    throw new InvalidOperationException("Duplicate or missing stable ID.");
                }
                result[id] = value.DeepClone();
            }
            return result;
        }

        private static JArray Diff(JToken before, JToken after, string path)
        {
            JArray changes = new JArray();
            if (Equal(before, after))
            {
                return changes;
            }
            if (before is JObject left && after is JObject right)
            {
                foreach (string key in left.Properties().Select(item => item.Name).Union(right.Properties().Select(item => item.Name)).OrderBy(item => item, StringComparer.Ordinal))
                {
                    string next = path + "/" + key.Replace("~", "~0").Replace("/", "~1");
                    bool had = left.Property(key) != null;
                    bool has = right.Property(key) != null;
                    if (!had || !has)
                    {
                        changes.Add(Change(next, had, left[key], has, right[key]));
                    }
                    else
                    {
                        foreach (JToken change in Diff(left[key], right[key], next))
                        {
                            changes.Add(change.DeepClone());
                        }
                    }
                }
            }
            else
            {
                changes.Add(Change(path, true, before, true, after));
            }
            return changes;
        }

        private static JObject Change(string path, bool beforeExists, JToken before, bool afterExists, JToken after)
        {
            return new JObject
            {
                ["path"] = path,
                ["beforeExists"] = beforeExists,
                ["before"] = before?.DeepClone(),
                ["afterExists"] = afterExists,
                ["after"] = after?.DeepClone()
            };
        }

        private static string Hash(string text)
        {
            if (text == null)
            {
                throw new InvalidOperationException("Missing change guard content.");
            }
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
