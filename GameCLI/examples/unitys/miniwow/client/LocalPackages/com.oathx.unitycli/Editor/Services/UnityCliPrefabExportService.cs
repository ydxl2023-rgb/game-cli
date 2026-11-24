using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Exports read-only layout packages; unsupported data is explicit diagnostic evidence.</summary>
    internal sealed class UnityCliPrefabExportService
    {
        private readonly Dictionary<string, JObject> entries = new Dictionary<string, JObject>();
        private readonly HashSet<string> active = new HashSet<string>();
        private readonly string output;
        private static readonly HashSet<string> Supported = new HashSet<string>(new[]
        {
            "UnityEngine.Canvas", "UnityEngine.CanvasRenderer", "UnityEngine.CanvasGroup",
            "UnityEngine.UI.Image", "UnityEngine.UI.RawImage", "UnityEngine.UI.Text",
            "UnityEngine.UI.Button", "UnityEngine.UI.Toggle", "UnityEngine.UI.ToggleGroup",
            "UnityEngine.UI.Slider", "UnityEngine.UI.Scrollbar", "UnityEngine.UI.ScrollRect",
            "UnityEngine.UI.RectMask2D", "UnityEngine.UI.Mask", "UnityEngine.UI.HorizontalLayoutGroup",
            "UnityEngine.UI.VerticalLayoutGroup", "UnityEngine.UI.GridLayoutGroup",
            "UnityEngine.UI.LayoutElement", "UnityEngine.UI.ContentSizeFitter", "UnityEngine.UI.AspectRatioFitter",
            "UnityEngine.UI.CanvasScaler", "UnityEngine.UI.GraphicRaycaster", "UnityEngine.UI.Dropdown",
            "UnityEngine.UI.InputField", "TMPro.TextMeshProUGUI", "TMPro.TMP_InputField", "TMPro.TMP_Dropdown"
        });

        private UnityCliPrefabExportService(string output)
        {
            this.output = output;
        }

        public static PrefabData Execute(UnityCliBridgeRequest request)
        {
            string path = (request.prefabPath ?? string.Empty).Replace('\\', '/').TrimEnd('/');
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.Split('/').Any(part => part == ".."))
            {
                throw new ArgumentException("--prefab must identify an Assets/ prefab or folder.");
            }
            if (string.IsNullOrWhiteSpace(request.screenshotPath) || string.IsNullOrWhiteSpace(request.exportSourceKey))
            {
                throw new ArgumentException("export requires --output and --source-key.");
            }
            string output = Path.GetFullPath(request.screenshotPath);
            string assets = Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar;
            if (output.StartsWith(assets, StringComparison.OrdinalIgnoreCase) || Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())
            {
                throw new ArgumentException("Use a new empty export directory outside Assets.");
            }
            string[] paths = AssetDatabase.IsValidFolder(path)
                ? AssetDatabase.FindAssets("t:Prefab", new[] { path }).Select(AssetDatabase.GUIDToAssetPath).OrderBy(value => value, StringComparer.Ordinal).ToArray()
                : new[] { path };
            if (paths.Length == 0)
            {
                throw new InvalidOperationException("No prefabs found.");
            }
            Directory.CreateDirectory(Path.Combine(output, "prefabs"));
            UnityCliPrefabExportService exporter = new UnityCliPrefabExportService(output);
            foreach (string asset in paths)
            {
                exporter.Export(asset);
            }
            JObject manifest = new JObject
            {
                ["schemaVersion"] = "ui-export/1",
                ["sourceKey"] = request.exportSourceKey,
                ["unityVersion"] = Application.unityVersion,
                ["exporterVersion"] = "layout-snapshot/0.1",
                ["entries"] = new JArray(exporter.entries.Values.OrderBy(entry => (string)entry["guid"], StringComparer.Ordinal))
            };
            File.WriteAllText(Path.Combine(output, "manifest.json"), manifest.ToString(Formatting.Indented));
            return new PrefabData
            {
                action = "export",
                prefabPath = path,
                outputPrefabPath = output,
                valid = true,
                message = "Exported " + exporter.entries.Count + " templates. Check manifest diagnostics before reconstruction."
            };
        }

        private void Export(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (entries.ContainsKey(guid))
            {
                return;
            }
            if (!active.Add(guid))
            {
                throw new InvalidOperationException("Cyclic prefab dependency: " + path);
            }
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(path);
                JObject resources = new JObject();
                JArray nodes = new JArray();
                JArray diagnostics = new JArray();
                Dictionary<Transform, string> ids = new Dictionary<Transform, string>();
                Collect(root.transform, "root", ids);
                foreach (KeyValuePair<Transform, string> pair in ids)
                {
                    Transform transform = pair.Key;
                    string nested = transform != root.transform && PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject)
                        ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(transform.gameObject) : null;
                    JObject node = new JObject
                    {
                        ["id"] = pair.Value,
                        ["name"] = transform.name,
                        ["active"] = transform.gameObject.activeSelf,
                        ["kind"] = string.IsNullOrEmpty(nested) ? "Node" : "PrefabInstance",
                        ["parentId"] = transform == root.transform ? null : ids[transform.parent],
                        ["siblingIndex"] = transform == root.transform ? 0 : transform.GetSiblingIndex(),
                        ["rect"] = Rect(transform as RectTransform),
                        ["localPosition"] = Vector(transform.localPosition),
                        ["localRotation"] = JArray.FromObject(new[] { transform.localRotation.x, transform.localRotation.y, transform.localRotation.z, transform.localRotation.w }),
                        ["localScale"] = Vector(transform.localScale),
                        ["components"] = Components(transform, ids, diagnostics)
                    };
                    if (!string.IsNullOrEmpty(nested))
                    {
                        Export(nested);
                        string nestedGuid = AssetDatabase.AssetPathToGUID(nested);
                        resources[nestedGuid] = new JObject
                        {
                            ["kind"] = "prefab",
                            ["id"] = nestedGuid,
                            ["version"] = entries[nestedGuid]["sourceRevision"].DeepClone()
                        };
                        node["resourceRef"] = nestedGuid;
                        node["overrides"] = new JArray();
                        // Keep unsupported overrides as evidence, never claim a lossless rebuild.
                        PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(transform.gameObject);
                        diagnostics.Add(new JObject
                        {
                            ["nodeId"] = pair.Value,
                            ["code"] = "nested-instance-requires-adapter",
                            ["message"] = "Nested template is exported. Instance overrides/added objects require the next-stage adapter.",
                            ["overrides"] = new JArray((modifications ?? new PropertyModification[0]).Select(modification => new JObject
                            {
                                ["property"] = modification.propertyPath,
                                ["value"] = modification.value,
                                ["target"] = modification.target == null ? null : modification.target.name
                            }))
                        });
                    }
                    nodes.Add(node);
                }
                JObject document = new JObject
                {
                    ["schemaVersion"] = "ui-template/1",
                    ["resources"] = resources,
                    ["widgetBindings"] = UnityCliWidgetBindingEvidence.Export(root, ids, path),
                    ["prefab"] = new JObject { ["name"] = root.name, ["rootNodeId"] = "root", ["nodes"] = nodes }
                };
                string file = "prefabs/" + guid + ".json";
                File.WriteAllText(Path.Combine(output, file), document.ToString(Formatting.Indented));
                entries.Add(guid, new JObject
                {
                    ["guid"] = guid,
                    ["assetPath"] = path,
                    ["sourceRevision"] = AssetDatabase.GetAssetDependencyHash(path).ToString(),
                    ["file"] = file,
                    ["grade"] = diagnostics.Count == 0 ? "supported" : "reference-only",
                    ["diagnostics"] = diagnostics
                });
            }
            finally
            {
                active.Remove(guid);
                if (root != null)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void Collect(Transform transform, string id, Dictionary<Transform, string> ids)
        {
            ids.Add(transform, id);
            if (id != "root" && PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject))
            {
                return;
            }
            for (int index = 0; index < transform.childCount; index++)
            {
                // Positional IDs are stable within this immutable version, not across hierarchy edits.
                Collect(transform.GetChild(index), id + "/" + index, ids);
            }
        }

        private static JToken Rect(RectTransform rect)
        {
            if (rect == null)
            {
                return JValue.CreateNull();
            }
            return new JObject
            {
                ["anchorMin"] = Vector(rect.anchorMin),
                ["anchorMax"] = Vector(rect.anchorMax),
                ["pivot"] = Vector(rect.pivot),
                ["anchoredPosition"] = Vector(rect.anchoredPosition),
                ["sizeDelta"] = Vector(rect.sizeDelta)
            };
        }

        private static JArray Vector(Vector2 value)
        {
            return new JArray(value.x, value.y);
        }

        private static JArray Vector(Vector3 value)
        {
            return new JArray(value.x, value.y, value.z);
        }

        private static JArray Components(Transform transform, Dictionary<Transform, string> ids, JArray diagnostics)
        {
            JArray result = new JArray();
            foreach (Component component in transform.GetComponents<Component>())
            {
                if (component is Transform)
                {
                    continue;
                }
                string type = component == null ? "MissingScript" : component.GetType().FullName;
                if (!Supported.Contains(type))
                {
                    diagnostics.Add(new JObject { ["nodeId"] = ids[transform], ["code"] = "unsupported-component", ["type"] = type });
                    continue;
                }
                JObject properties = new JObject();
                SerializedObject serialized = new SerializedObject(component);
                SerializedProperty property = serialized.GetIterator();
                bool descend = true;
                while (property.Next(descend))
                {
                    descend = true;
                    string key = property.propertyPath;
                    if (key == "m_Script" || key == "m_GameObject" || key.StartsWith("m_CorrespondingSourceObject") || key.StartsWith("m_Prefab") || key == "m_ObjectHideFlags")
                    {
                        descend = false;
                        continue;
                    }
                    if (key.EndsWith("m_PersistentCalls", StringComparison.Ordinal))
                    {
                        descend = false;
                        if (property.FindPropertyRelative("m_Calls")?.arraySize > 0)
                        {
                            diagnostics.Add(new JObject { ["nodeId"] = ids[transform], ["code"] = "business-callback-excluded", ["property"] = key });
                        }
                        continue;
                    }
                    if (property.propertyType == SerializedPropertyType.Generic)
                    {
                        continue;
                    }
                    descend = false;
                    JToken value = Value(property, ids);
                    if (value == null)
                    {
                        diagnostics.Add(new JObject { ["nodeId"] = ids[transform], ["code"] = "unsupported-property", ["component"] = type, ["property"] = key, ["valueType"] = property.propertyType.ToString() });
                    }
                    else
                    {
                        properties[key] = value;
                    }
                }
                result.Add(new JObject { ["id"] = "c" + result.Count, ["type"] = type, ["properties"] = properties });
            }
            return result;
        }

        private static JToken Value(SerializedProperty property, Dictionary<Transform, string> ids)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return new JValue(property.boolValue);
                case SerializedPropertyType.Integer:
                    return new JValue(property.longValue);
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.LayerMask:
                    return new JValue(property.intValue);
                case SerializedPropertyType.Float:
                    return new JValue(property.doubleValue);
                case SerializedPropertyType.String:
                    return new JValue(property.stringValue);
                case SerializedPropertyType.Vector2:
                    return Vector(property.vector2Value);
                case SerializedPropertyType.Vector3:
                    return Vector(property.vector3Value);
                case SerializedPropertyType.Vector4:
                    Vector4 vector = property.vector4Value;
                    return new JArray(vector.x, vector.y, vector.z, vector.w);
                case SerializedPropertyType.Color:
                    Color color = property.colorValue;
                    return new JArray(color.r, color.g, color.b, color.a);
                case SerializedPropertyType.ObjectReference:
                    UnityEngine.Object reference = property.objectReferenceValue;
                    if (reference == null)
                    {
                        return JValue.CreateNull();
                    }
                    Transform target = reference is GameObject gameObject ? gameObject.transform : (reference as Component)?.transform;
                    if (target != null && ids.TryGetValue(target, out string id))
                    {
                        if (reference is Component && !(reference is Transform) && !Supported.Contains(reference.GetType().FullName))
                        {
                            return null;
                        }
                        return new JObject { ["nodeId"] = id, ["componentType"] = reference is Component ? reference.GetType().FullName : null };
                    }
                    return null;
                default:
                    return null;
            }
        }
    }
}
