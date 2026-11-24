using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Imports bounded native effect data without accepting scripts or foreign object identities.</summary>
    internal static class UnityCliNativeParticleData
    {
        internal static void Apply(Component target, JToken definition)
        {
            if (definition == null)
            {
                return;
            }
            RequireType(target);
            JObject normalized = NormalizeDefinition(definition);
            if (target is ParticleSystem)
            {
                double maximum = (double?)normalized["InitialModule"]?["maxNumParticles"] ?? 1000;
                double duration = (double?)normalized["lengthInSec"] ?? 5;
                double speed = (double?)normalized["simulationSpeed"] ?? 1;
                if (maximum < 1 || maximum > 10000 || duration < 0.01 || duration > 120 || speed <= 0 || speed > 10)
                {
                    throw new InvalidOperationException("Native particle capacity or timing exceeds the bounded import policy.");
                }
            }
            JObject schema = JObject.Parse(EditorJsonUtility.ToJson(target));
            JToken input = schema[target.GetType().Name] is JObject
                ? new JObject { [target.GetType().Name] = normalized }
                : normalized;
            EditorJsonUtility.FromJsonOverwrite(input.ToString(Formatting.None), target);
            SerializedObject serialized = new SerializedObject(target);
            ApplyScalars(serialized, normalized, string.Empty);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (target is ParticleSystem particles && normalized["cullingMode"] != null)
            {
                int mode = (int)normalized["cullingMode"];
                if (mode < 0 || mode > 3)
                {
                    throw new InvalidOperationException("Unsupported native particle culling mode.");
                }
                ParticleSystem.MainModule main = particles.main;
                main.cullingMode = (ParticleSystemCullingMode)mode;
            }
            if (target is ParticleSystem simulation && normalized["moveWithTransform"] != null)
            {
                int local = (int)normalized["moveWithTransform"];
                if (local != 0 && local != 1)
                {
                    throw new InvalidOperationException("Unsupported particle simulation-space policy.");
                }
                ParticleSystem.MainModule main = simulation.main;
                main.simulationSpace = (ParticleSystemSimulationSpace)local;
            }
            VerifyNormalized(target, normalized);
        }

        internal static void Verify(Component target, JToken definition)
        {
            if (definition == null)
            {
                return;
            }
            RequireType(target);
            VerifyNormalized(target, NormalizeDefinition(definition));
        }

        private static void RequireType(Component target)
        {
            if (!(target is ParticleSystem) && !(target is ParticleSystemRenderer) && !(target is SortingGroup))
            {
                throw new InvalidOperationException("Native serialized effect data is restricted to particles and sorting groups.");
            }
        }

        private static void ApplyScalars(SerializedObject target, JToken value, string path)
        {
            if (value is JObject map)
            {
                foreach (JProperty field in map.Properties())
                {
                    ApplyScalars(target, field.Value, path.Length == 0 ? field.Name : path + "." + field.Name);
                }
                return;
            }
            if (value is JArray array)
            {
                SerializedProperty list = target.FindProperty(path);
                if (list != null && list.isArray)
                {
                    list.arraySize = array.Count;
                }
                for (int index = 0; index < array.Count; index++)
                {
                    ApplyScalars(target, array[index], path + ".Array.data[" + index + "]");
                }
                return;
            }
            SerializedProperty property = target.FindProperty(path);
            if (property == null)
            {
                // Complete JSON readback below still rejects ignored or unsupported source fields.
                return;
            }
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    property.boolValue = value.Type == JTokenType.Boolean ? (bool)value : (long)value != 0;
                    break;
                case SerializedPropertyType.Integer:
                    property.longValue = (long)value;
                    break;
                case SerializedPropertyType.Enum:
                    property.intValue = (int)value;
                    break;
                case SerializedPropertyType.Float:
                    float number = (float)value;
                    if (float.IsNaN(number) || float.IsInfinity(number))
                    {
                        throw new InvalidOperationException("Nonfinite effect scalar: " + path);
                    }
                    property.floatValue = number;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = (string)value;
                    break;
            }
        }

        private static JObject NormalizeDefinition(JToken definition)
        {
            if (!(definition is JObject source) || definition.ToString(Formatting.None).Length > 1024 * 1024)
            {
                throw new InvalidOperationException("Native particle data must be an object within 1 MiB.");
            }
            return (JObject)Normalize(source, 0);
        }

        private static JToken Normalize(JToken value, int depth)
        {
            if (depth > 64)
            {
                throw new InvalidOperationException("Native effect data exceeds the nesting limit.");
            }
            if (value is JObject source)
            {
                if (source["fileID"] != null || source["instanceID"] != null)
                {
                    string identity = (string)(source["fileID"] ?? source["instanceID"]);
                    if (identity != "0" || source["guid"] != null)
                    {
                        throw new InvalidOperationException("Effect object references require explicit verified bindings.");
                    }
                    return new JObject { ["instanceID"] = 0 };
                }
                JObject output = new JObject();
                foreach (JProperty field in source.Properties())
                {
                    if (field.Name == "serializedVersion")
                    {
                        continue;
                    }
                    if (field.Name == "m_GameObject" || field.Name == "m_Script" || field.Name == "m_ObjectHideFlags"
                        || field.Name == "m_PrefabInstance" || field.Name == "m_PrefabAsset" || field.Name == "m_CorrespondingSourceObject"
                        || field.Name == "m_PersistentCalls" || field.Name == "guid")
                    {
                        throw new InvalidOperationException("Effect identity, script and callback fields are forbidden: " + field.Name);
                    }
                    output[field.Name] = Normalize(field.Value, depth + 1);
                }
                return output;
            }
            if (value is JArray array)
            {
                if (array.Count > 10000)
                {
                    throw new InvalidOperationException("Native effect array exceeds 10000 items.");
                }
                JArray output = new JArray();
                foreach (JToken child in array)
                {
                    output.Add(Normalize(child, depth + 1));
                }
                return output;
            }
            if (value.Type == JTokenType.Float)
            {
                double number = (double)value;
                if (double.IsNaN(number) || double.IsInfinity(number))
                {
                    throw new InvalidOperationException("Native effect numeric fields must be finite.");
                }
            }
            return value.DeepClone();
        }

        private static void VerifyNormalized(Component target, JObject expected)
        {
            JObject actual = JObject.Parse(EditorJsonUtility.ToJson(target));
            if (actual[target.GetType().Name] is JObject wrapped)
            {
                actual = wrapped;
            }
            Compare(expected, actual, target.GetType().Name, new SerializedObject(target), string.Empty);
        }

        private static void Compare(JToken expected, JToken actual, string path, SerializedObject serialized, string propertyPath)
        {
            if (expected is JObject map)
            {
                if (!(actual is JObject observed))
                {
                    throw new InvalidOperationException("Native effect object mismatch: " + path);
                }
                foreach (JProperty field in map.Properties())
                {
                    string childPath = propertyPath.Length == 0 ? field.Name : propertyPath + "." + field.Name;
                    if (observed[field.Name] == null)
                    {
                        // Editor JSON omits some null references; verify their native serialized slot instead.
                        SerializedProperty slot = serialized.FindProperty(childPath);
                        if (field.Value is JObject reference && (int?)reference["instanceID"] == 0
                            && slot != null && slot.propertyType == SerializedPropertyType.ObjectReference
                            && slot.objectReferenceValue == null)
                        {
                            continue;
                        }
                        throw new InvalidOperationException("Native effect field is unsupported: " + path + "/" + field.Name
                            + "; native keys: " + string.Join(",", observed.Properties().Select(property => property.Name).Take(100)));
                    }
                    Compare(field.Value, observed[field.Name], path + "/" + field.Name, serialized, childPath);
                }
                return;
            }
            if (expected is JArray array)
            {
                if (!(actual is JArray observed) || array.Count != observed.Count)
                {
                    throw new InvalidOperationException("Native effect array mismatch: " + path);
                }
                for (int index = 0; index < array.Count; index++)
                {
                    Compare(array[index], observed[index], path + "/" + index, serialized, propertyPath + ".Array.data[" + index + "]");
                }
                return;
            }
            bool same = JToken.DeepEquals(expected, actual);
            if (!same && expected.Type == JTokenType.Integer && actual.Type == JTokenType.Boolean)
            {
                long value = (long)expected;
                same = (value == 0 || value == 1) && (value == 1) == (bool)actual;
            }
            if (!same && (expected.Type == JTokenType.Integer || expected.Type == JTokenType.Float)
                && (actual.Type == JTokenType.Integer || actual.Type == JTokenType.Float))
            {
                same = expected.Type == JTokenType.Integer && actual.Type == JTokenType.Integer
                    ? (long)expected == (long)actual
                    : float.Parse(expected.ToString(), CultureInfo.InvariantCulture).Equals(float.Parse(actual.ToString(), CultureInfo.InvariantCulture));
            }
            if (!same)
            {
                throw new InvalidOperationException("Native effect value mismatch: " + path + " (" + expected + " vs " + actual + ")");
            }
        }
    }
}

