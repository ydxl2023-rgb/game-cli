using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Checks immutable assembly envelopes before any Unity asset operation.</summary>
    internal static class UnityCliAssemblyInput
    {
        public static JObject Resolve(JObject document, string outputPath, bool whitebox = false)
        {
            if ((string)document["schemaVersion"] == "ui-task-input/1")
            {
                return ResolveWhitebox(document, outputPath, whitebox);
            }
            if ((string)document["schemaVersion"] == "ui-assembly/2")
            {
                return UnityCliChangeGuard.Resolve(document, outputPath);
            }
            if ((string)document["schemaVersion"] != "ui-assembly/1")
            {
                return document;
            }
            if ((string)document["adapterVersion"] != "layout-edits/1")
            {
                throw new InvalidOperationException("Unsupported assembly adapter.");
            }
            if ((string)document["outputPrefab"] != outputPath)
            {
                throw new InvalidOperationException("Assembly output differs from the approved plan.");
            }
            string snapshot = RequiredString(document, "snapshotJson");
            string planText = RequiredString(document, "planJson");
            if (Hash(snapshot) != (string)document["snapshotSha256"] || Hash(planText) != (string)document["planSha256"])
            {
                throw new InvalidOperationException("Assembly content hash mismatch.");
            }
            JObject plan = JObject.Parse(planText);
            if ((string)plan["schemaVersion"] != "ui-assembly-plan/1"
                || (string)plan["outputPrefab"] != outputPath
                || (string)plan["sourceSha256"] != RequiredString(document, "sourceSha256"))
            {
                throw new InvalidOperationException("Assembly plan metadata mismatch.");
            }
            // These hashes detect corruption; they are not signatures or authorization grants.
            return JObject.Parse(snapshot);
        }

        // This adapter consumes the server-frozen window, never historical snapshots.
        // Content hashes are provenance, not signatures or a replacement for platform approval.
        private static JObject ResolveWhitebox(JObject input, string outputPath, bool whitebox)
        {
            if (!whitebox || (string)input["type"] != "prefab-whitebox")
            {
                throw new InvalidOperationException("Frozen task input currently requires prefab-whitebox and --whitebox; final assembly needs resolved delivery resources.");
            }
            JObject window = input["window"] as JObject;
            if (window == null || (string)window["prefabPath"] != outputPath
                || (string)window["placeholderPolicy"]?["image"] != "placeholder"
                || (string)window["placeholderPolicy"]?["font"] != "default")
            {
                throw new InvalidOperationException("Frozen window output or placeholder policy mismatch.");
            }
            JObject snapshot = new JObject
            {
                ["schemaVersion"] = "ui-template/1",
                ["prefab"] = new JObject
                {
                    ["rootNodeId"] = window["rootNodeId"].DeepClone(),
                    ["nodes"] = window["nodes"].DeepClone()
                }
            };
            JArray nodes = (JArray)snapshot["prefab"]["nodes"];
            Dictionary<string, JObject> byId = nodes.Cast<JObject>().ToDictionary(n => RequiredString(n, "id"));
            foreach (JObject node in nodes)
            {
                node["kind"] = "Node";
            }
            JObject root = byId[(string)window["rootNodeId"]];
            if ((string)root["name"] != System.IO.Path.GetFileNameWithoutExtension(outputPath))
            {
                throw new InvalidOperationException("Frozen root name must match output prefab.");
            }
            HashSet<string> bindings = new HashSet<string>();
            foreach (JObject slot in (JArray)window["resourceSlots"])
            {
                foreach (JObject binding in (JArray)slot["bindings"])
                {
                    DeferBinding(byId, bindings, binding, "resourceSlot", RequiredString(slot, "slotId"));
                }
            }
            foreach (JObject animation in (JArray)window["animations"])
            {
                if (!(animation["specification"]?["bindings"] is JArray targets))
                {
                    DeferAnimationTracks(snapshot, byId, animation);
                    continue;
                }
                foreach (JObject binding in targets)
                {
                    DeferBinding(byId, bindings, binding, "animationSlot", RequiredString(animation, "id"));
                }
            }
            return snapshot;
        }

        // Track declarations describe curves, not asset reference properties. Whitebox keeps
        // their complete specification for Art/assembly without inventing Animator bindings.
        private static void DeferAnimationTracks(JObject snapshot, Dictionary<string, JObject> nodes, JObject animation)
        {
            JObject specification = animation["specification"] as JObject;
            JArray tracks = specification?["tracks"] as JArray;
            JArray targetIds = animation["targetNodeIds"] as JArray;
            if (tracks == null || tracks.Count == 0 || targetIds == null)
            {
                throw new InvalidOperationException("Animation requires component bindings or nonempty tracks and targetNodeIds.");
            }
            foreach (JObject track in tracks)
            {
                string nodeId = RequiredString(track, "targetNodeId");
                if (!nodes.TryGetValue(nodeId, out JObject node) || !targetIds.Values<string>().Contains(nodeId))
                {
                    throw new InvalidOperationException("Animation track target is absent from the frozen window: " + nodeId);
                }
                string property = (string)(track["targetProperty"] ?? track["property"]);
                string componentId = (string)(track["targetComponentId"] ?? track["componentId"]);
                if (string.IsNullOrWhiteSpace(property))
                {
                    throw new InvalidOperationException("Animation track property is required.");
                }
                if (!string.IsNullOrEmpty(componentId)
                    && !((JArray)node["components"]).Cast<JObject>().Any(c => (string)c["id"] == componentId))
                {
                    throw new InvalidOperationException("Animation track component is absent: " + componentId);
                }
            }
            JArray deferred = snapshot["deferredAnimations"] as JArray;
            if (deferred == null)
            {
                deferred = new JArray();
                snapshot["deferredAnimations"] = deferred;
            }
            deferred.Add(animation.DeepClone());
        }

        private static void DeferBinding(Dictionary<string, JObject> nodes, HashSet<string> seen, JObject binding, string kind, string slot)
        {
            string nodeId = RequiredString(binding, "nodeId");
            string componentId = RequiredString(binding, "componentId");
            string property = RequiredString(binding, "property");
            if (!seen.Add(nodeId + ":" + componentId + ":" + property))
            {
                throw new InvalidOperationException("Duplicate frozen slot binding.");
            }
            JObject component = ((JArray)nodes[nodeId]["components"]).Cast<JObject>().Single(c => (string)c["id"] == componentId);
            JObject properties = (JObject)component["properties"];
            if (properties[property] == null || properties[property].Type != JTokenType.Null)
            {
                throw new InvalidOperationException("Frozen slot must bind an explicit null reference.");
            }
            properties[property] = new JObject { ["deferredSlot"] = slot, ["kind"] = kind };
        }

        private static string RequiredString(JObject document, string key)
        {
            if (document[key]?.Type != JTokenType.String || string.IsNullOrEmpty((string)document[key]))
            {
                throw new InvalidOperationException("Missing assembly field: " + key);
            }
            return (string)document[key];
        }

        private static string Hash(string text)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
