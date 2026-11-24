#nullable disable
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Resolves bounded, hash-verified native data references before normal contract validation.</summary>
    internal static class UnityCliContractNativeReferences
    {
        internal static void Expand(JObject contract)
        {
            JObject definitions = contract["nativeSerializedDefinitions"] as JObject;
            string encoding = (string)contract["nativeSerializedEncoding"];
            if (definitions == null && encoding == null)
            {
                RejectUnencodedReferences(contract);
                return;
            }
            if (encoding != "sha256-table/1" || definitions == null || definitions.Count > 128)
            {
                throw new InvalidOperationException("Invalid shared native-data encoding or definition count.");
            }
            Dictionary<string, JObject> blocks = new Dictionary<string, JObject>(StringComparer.Ordinal);
            Dictionary<string, int> lengths = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (JProperty property in definitions.Properties())
            {
                if (property.Value.Type != JTokenType.String)
                {
                    throw new InvalidOperationException("Shared native data must contain canonical JSON strings.");
                }
                string json = (string)property.Value;
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                string hash;
                using (SHA256 sha = SHA256.Create())
                {
                    hash = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
                }
                if (hash != property.Name || bytes.Length > 2 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Shared native parameter hash mismatch or oversized block.");
                }
                blocks.Add(hash, JObject.Parse(json));
                lengths.Add(hash, bytes.Length);
            }
            long expanded = 0;
            int references = 0;
            foreach (JToken token in contract.SelectTokens("$.windows[*].nodes[*].components[*]"))
            {
                JObject component = (JObject)token;
                JToken reference = component["nativeSerializedRef"];
                if (reference == null)
                {
                    continue;
                }
                string hash = reference.Type == JTokenType.String ? (string)reference : null;
                if (hash == null || !blocks.TryGetValue(hash, out JObject data) || component["nativeSerialized"] != null)
                {
                    throw new InvalidOperationException("Missing, conflicting or invalid native parameter reference.");
                }
                expanded += lengths[hash];
                references++;
                if (expanded > 64L * 1024 * 1024 || references > 8192)
                {
                    throw new InvalidOperationException("Shared native expansion exceeds its bounded production budget.");
                }
                // Each component owns its expanded object; applying one instance cannot mutate another.
                component["nativeSerialized"] = data.DeepClone();
                component.Remove("nativeSerializedRef");
            }
        }

        private static void RejectUnencodedReferences(JObject contract)
        {
            foreach (JToken token in contract.SelectTokens("$.windows[*].nodes[*].components[*].nativeSerializedRef"))
            {
                throw new InvalidOperationException("Native parameter references require an explicit supported encoding.");
            }
        }
    }
}
