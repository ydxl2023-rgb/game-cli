using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Restores a resolved layout snapshot without imposing task-layout root geometry.</summary>
    internal static class UnityCliPrefabRestoreService
    {
        private static readonly Dictionary<string, Type> Types = new[]
        {
            typeof(Oathx.UnityCLI.UnityCliWhiteboxTabs), typeof(UnityEngine.EventSystems.EventTrigger), typeof(Canvas), typeof(CanvasRenderer), typeof(CanvasGroup), typeof(Image), typeof(RawImage), typeof(Animator),
            typeof(Text), typeof(Button), typeof(Toggle), typeof(ToggleGroup), typeof(Slider), typeof(Scrollbar),
            typeof(ScrollRect), typeof(RectMask2D), typeof(Mask), typeof(HorizontalLayoutGroup),
            typeof(VerticalLayoutGroup), typeof(GridLayoutGroup), typeof(LayoutElement), typeof(ContentSizeFitter),
            typeof(Shadow), typeof(Outline),
            typeof(AspectRatioFitter), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(Dropdown), typeof(InputField),
            typeof(ParticleSystem), typeof(ParticleSystemRenderer), typeof(UnityEngine.Rendering.SortingGroup)
        }.ToDictionary(type => type.FullName, type => type);

        public static PrefabData Execute(UnityCliBridgeRequest request)
        {
            return Execute(request, null);
        }

        internal static PrefabData Execute(UnityCliBridgeRequest request, JObject resolvedDocument)
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            string path = (request.prefabPath ?? "").Replace('\\', '/');
            string full = Path.GetFullPath(Path.Combine(project, path));
            bool previewOnly = request.action == "preview";
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || !path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                || path.Contains("..") || !full.StartsWith(Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Output must be a prefab under Assets.");
            }
            if (!previewOnly && File.Exists(full) && !(request.replaceExistingChildSet && request.replaceExistingChild))
            {
                throw new InvalidOperationException("Output already exists; use explicit --replace.");
            }
            JObject document = resolvedDocument;
            if (document == null)
            {
                string input = Path.GetFullPath(request.jsonFile);
                if (new FileInfo(input).Length > 8 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Snapshot exceeds 8 MiB.");
                }
                document = JObject.Parse(File.ReadAllText(input));
                document = UnityCliAssemblyInput.Resolve(document, path, request.whitebox);
            }
            if ((string)document["schemaVersion"] != "ui-template/1")
            {
                throw new InvalidOperationException("Expected ui-template/1 snapshot.");
            }
            JArray nodes = document["prefab"]?["nodes"] as JArray;
            if (nodes == null || nodes.Count == 0 || nodes.Count > 2048)
            {
                throw new InvalidOperationException("Invalid snapshot node count.");
            }
            string rootId = (string)document["prefab"]["rootNodeId"];
            // Guard resolution above always runs, including the whitebox stage.
            if (previewOnly && !(document["visualPreview"] is JObject))
            {
                throw new InvalidOperationException("Preview requires visualPreview settings.");
            }
            UnityCliSnapshotVisuals.Configure(document, request.dryRun || previewOnly, request.whitebox);
            JArray deferredBindings = new JArray();
            // Retain deferred curve requirements in the result; whitebox is not animation delivery.
            foreach (JObject animation in (document["deferredAnimations"] as JArray ?? new JArray()))
            {
                deferredBindings.Add(new JObject
                {
                    ["kind"] = "animationTracks",
                    ["animation"] = animation.DeepClone()
                });
            }
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
                Dictionary<string, Component> components = new Dictionary<string, Component>();
                foreach (JObject node in nodes)
                {
                    if ((string)node["kind"] != "Node")
                    {
                        throw new InvalidOperationException("Resolve nested prefab instances before restore.");
                    }
                    string id = (string)node["id"];
                    GameObject go = new GameObject((string)node["name"], node["rect"].Type == JTokenType.Null ? typeof(Transform) : typeof(RectTransform));
                    SceneManager.MoveGameObjectToScene(go, preview);
                    objects.Add(id, go);
                }
                if (nodes.Count(node => node["parentId"].Type == JTokenType.Null) != 1 || !objects.ContainsKey(rootId))
                {
                    throw new InvalidOperationException("Invalid snapshot root.");
                }
                if (nodes.First(node => (string)node["id"] == rootId)["parentId"].Type != JTokenType.Null)
                {
                    throw new InvalidOperationException("rootNodeId must identify the hierarchy root.");
                }
                foreach (JObject node in nodes)
                {
                    string id = (string)node["id"];
                    string parent = (string)node["parentId"];
                    HashSet<string> ancestors = new HashSet<string> { id };
                    string cursor = parent;
                    while (cursor != null)
                    {
                        if (!objects.ContainsKey(cursor) || !ancestors.Add(cursor))
                        {
                            throw new InvalidOperationException("Missing parent or hierarchy cycle.");
                        }
                        cursor = (string)nodes.First(item => (string)item["id"] == cursor)["parentId"];
                    }
                    if (parent != null)
                    {
                        objects[id].transform.SetParent(objects[parent].transform, false);
                    }
                }
                foreach (JObject node in nodes.OrderBy(node => (int)node["siblingIndex"]))
                {
                    string id = (string)node["id"];
                    GameObject go = objects[id];
                    go.transform.SetSiblingIndex((int)node["siblingIndex"]);
                    ApplyTransform(go.transform, node);
                    HashSet<string> componentTypes = new HashSet<string>();
                    foreach (JObject specification in node["components"])
                    {
                        string typeName = (string)specification["type"];
                        if (!componentTypes.Add(typeName))
                        {
                            throw new InvalidOperationException("Multiple components of the same type need an explicit adapter: " + typeName);
                        }
                        if (!Types.TryGetValue(typeName, out Type type))
                        {
                            throw new InvalidOperationException("Unsupported restore component: " + typeName);
                        }
                        Component component = go.GetComponent(type);
                        if (component == null)
                        {
                            component = go.AddComponent(type);
                        }
                        if (component == null)
                        {
                            throw new InvalidOperationException("Cannot create " + typeName + " on " + go.name);
                        }
                        components.Add(id + ":" + (string)specification["id"], component);
                    }
                }
                foreach (JObject node in nodes)
                {
                    string id = (string)node["id"];
                    foreach (JObject specification in node["components"])
                    {
                        Component component = components[id + ":" + (string)specification["id"]];
                        UnityCliNativeParticleData.Apply(component, specification["nativeSerialized"]);
                        SerializedObject serialized = new SerializedObject(component);
                        foreach (JProperty property in ((JObject)specification["properties"]).Properties().OrderBy(property => property.Name.EndsWith(".Array.size") ? 0 : 1))
                        {
                            // Unity YAML serialization metadata is not an editable component property.
                            if (property.Name == "m_Name" || property.Name == "m_EditorHideFlags" || property.Name == "serializedVersion")
                            {
                                continue;
                            }
                            if (property.Name.Contains("m_Script") || property.Name.Contains("m_PersistentCalls") || property.Name.StartsWith("m_GameObject"))
                            {
                                throw new InvalidOperationException("Script or ownership reference is forbidden.");
                            }
                            SerializedProperty destination = serialized.FindProperty(property.Name);
                            if (destination == null)
                            {
                                throw new InvalidOperationException(component.GetType().FullName + ": missing property " + property.Name);
                            }
                            SetValue(destination, property.Value, objects, request.dryRun, request.whitebox, previewOnly);
                            if ((request.whitebox || previewOnly && destination.name == "m_Controller") && property.Value is JObject reference && (reference["assetPath"] != null || reference["deferredSlot"] != null))
                            {
                                deferredBindings.Add(new JObject
                                {
                                    ["nodeId"] = id,
                                    ["componentId"] = (string)specification["id"],
                                    ["property"] = property.Name,
                                    ["reference"] = reference.DeepClone()
                                });
                            }
                            serialized.ApplyModifiedPropertiesWithoutUndo();
                        }
                        if (component is Text text && text.font == null)
                        {
                            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                        }
                    }
                    objects[id].SetActive((bool)node["active"]);
                }
                GameObject root = objects[rootId];
                foreach (Oathx.UnityCLI.UnityCliWhiteboxTabs tabs in root.GetComponentsInChildren<Oathx.UnityCLI.UnityCliWhiteboxTabs>(true))
                {
                    tabs.ValidateConfiguration();
                }
                if (!request.dryRun && !previewOnly)
                {
                    string folder = Path.GetDirectoryName(path).Replace('\\', '/');
                    EnsureFolder(folder);
                    if (PrefabUtility.SaveAsPrefabAsset(root, path) == null)
                    {
                        throw new InvalidOperationException("Failed to save restored prefab.");
                    }
                }
                if (!request.dryRun && !request.whitebox && document["visualPreview"] is JObject rendering)
                {
                    UnityCliSnapshotVisuals.Render(root, preview, rendering);
                }
                return new PrefabData
                {
                    whitebox = request.whitebox,
                    deferredBindingsJson = deferredBindings.ToString(Newtonsoft.Json.Formatting.None),
                    action = previewOnly ? "preview" : "restore", prefabPath = path, dryRun = request.dryRun, valid = true,
                    nodeCount = objects.Count, componentCount = components.Count, prefabModified = !request.dryRun && !previewOnly,
                    message = previewOnly ? "Static isolated preview only; controllers deferred, no prefab modified." : request.whitebox ? "Whitebox only; deferred bindings require final assembly and visual verification." : document["visualTextures"] != null ? "Layout and explicitly mapped visual assets restored; business scripts excluded." : "Layout snapshot restored; excluded source scripts and missing visual resources are not recreated."
                };
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        private static void SetValue(SerializedProperty property, JToken value, Dictionary<string, GameObject> objects, bool dryRun, bool whitebox, bool previewOnly)
        {
            // An explicitly empty serialized list carries no callback or source-code dependency.
            if (property.isArray && value is JArray empty && empty.Count == 0)
            {
                property.arraySize = 0;
                return;
            }
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    property.boolValue = value.Type == JTokenType.Boolean ? (bool)value : (double)value != 0;
                    break;
                case SerializedPropertyType.Integer:
                    // YAML also stores unsigned 32-bit masks (for example 4294967295).
                    // intValue narrows those values before Unity can preserve their bits.
                    property.longValue = (long)value;
                    break;
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.LayerMask:
                    property.intValue = (int)value;
                    break;
                case SerializedPropertyType.ArraySize:
                    property.intValue = (int)value;
                    break;
                case SerializedPropertyType.Character:
                    string character = value.Type == JTokenType.String ? (string)value : null;
                    if (character != null && character.Length != 1)
                    {
                        throw new InvalidOperationException("Expected one character: " + property.propertyPath);
                    }
                    property.intValue = character == null ? (int)value : character[0];
                    break;
                case SerializedPropertyType.Float:
                    float number = (float)value;
                    if (float.IsNaN(number) || float.IsInfinity(number))
                    {
                        throw new InvalidOperationException("Nonfinite number: " + property.propertyPath);
                    }
                    property.floatValue = number;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = (string)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    if (value.Type == JTokenType.Null)
                    {
                        property.objectReferenceValue = null;
                        break;
                    }
                    if (value["deferredSlot"] != null)
                    {
                        if (!whitebox || (property.name != "m_Sprite" && property.name != "m_Font" && property.name != "m_Controller" && property.name != "m_Texture"))
                        {
                            throw new InvalidOperationException("Deferred contract slot is only supported for whitebox visual references.");
                        }
                        property.objectReferenceValue = null;
                        break;
                    }
                    string assetPath = (string)value["assetPath"];
                    if (assetPath != null)
                    {
                        if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal) || assetPath.Contains(".."))
                        {
                            throw new InvalidOperationException("Invalid visual asset path.");
                        }
                        // Controllers are explicit project assets; source scripts remain excluded.
                        Type assetType = property.name == "m_Sprite" ? typeof(Sprite)
                            : property.name == "m_Font" ? typeof(Font)
                            : property.name == "m_Texture" ? typeof(Texture2D)
                            : property.name == "m_Controller" ? typeof(RuntimeAnimatorController)
                            : property.serializedObject.targetObject is ParticleSystemRenderer
                                && System.Text.RegularExpressions.Regex.IsMatch(property.propertyPath, @"^m_Materials\.Array\.data\[\d+\]$") ? typeof(Material)
                            : property.serializedObject.targetObject is ParticleSystemRenderer
                                && System.Text.RegularExpressions.Regex.IsMatch(property.propertyPath, @"^m_Mesh[1-3]?$") ? typeof(Mesh)
                            : property.serializedObject.targetObject is ParticleSystem && property.propertyPath == "ShapeModule.m_Mesh" ? typeof(Mesh)
                            : property.serializedObject.targetObject is ParticleSystem && property.propertyPath == "ShapeModule.m_Texture" ? typeof(Texture2D) : null;
                        if (assetType == null)
                        {
                            throw new InvalidOperationException("Unsupported visual reference: " + property.propertyPath);
                        }
                        if (whitebox || previewOnly && assetType == typeof(RuntimeAnimatorController))
                        {
                            // Do not let stale final assets silently change whitebox behavior.
                            property.objectReferenceValue = null;
                            break;
                        }
                        assetPath = UnityCliAssetAliases.Resolve(assetPath);
                        if (assetType == typeof(Mesh))
                        {
                            string expectedId = (string)value["localFileId"];
                            if (!long.TryParse(expectedId, out long identifier) || identifier == 0)
                            {
                                throw new InvalidOperationException("Native mesh bindings require an explicit local file ID.");
                            }
                            UnityEngine.Object match = null;
                            foreach (UnityEngine.Object candidate in AssetDatabase.LoadAllAssetsAtPath(assetPath))
                            {
                                if (candidate is Mesh mesh && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh, out string guid, out long localId)
                                    && localId == identifier)
                                {
                                    if (match != null || mesh.vertexCount > 100000 || mesh.subMeshCount > 16)
                                    {
                                        throw new InvalidOperationException("Native mesh binding is ambiguous or exceeds geometry limits.");
                                    }
                                    match = mesh;
                                }
                            }
                            property.objectReferenceValue = match;
                        }
                        else
                        {
                            property.objectReferenceValue = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
                        }
                        if (dryRun && property.objectReferenceValue == null && assetType == typeof(Sprite))
                        {
                            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                            if (texture != null)
                            {
                                // Dry-run validates the pending import without creating transient assets.
                                break;
                            }
                        }
                        if (property.objectReferenceValue == null)
                        {
                            throw new InvalidOperationException("Missing visual asset: " + assetPath);
                        }
                        break;
                    }
                    string id = (string)value["nodeId"];
                    if (id == null || !objects.TryGetValue(id, out GameObject target))
                    {
                        throw new InvalidOperationException("Invalid object reference: " + property.propertyPath);
                    }
                    string type = (string)value["componentType"];
                    if (type != null)
                    {
                        property.objectReferenceValue = type == "UnityEngine.RectTransform" ? target.transform : Types.TryGetValue(type, out Type componentType) ? target.GetComponent(componentType) : null;
                    }
                    else
                    {
                        // Offline 0.1 omits the target type. Resolve only unambiguous UGUI reference slots.
                        string name = property.name;
                        property.objectReferenceValue = name == "m_Content" || name == "m_Viewport" || name == "m_HandleRect" || name == "m_FillRect" ? target.transform
                            : name == "m_Group" ? target.GetComponent<ToggleGroup>()
                            : name == "m_TextComponent" ? target.GetComponent<UnityEngine.UI.Text>()
                            : name == "m_Placeholder" ? target.GetComponent<Graphic>()
                            : name == "m_HorizontalScrollbar" || name == "m_VerticalScrollbar" ? target.GetComponent<Scrollbar>()
                            : name == "m_TargetGraphic" || name == "graphic" ? target.GetComponent<Graphic>()
                            : name == "m_SelectOnUp" || name == "m_SelectOnDown" || name == "m_SelectOnLeft" || name == "m_SelectOnRight" ? target.GetComponent<Selectable>() : null;
                    }
                    if (property.objectReferenceValue == null)
                    {
                        throw new InvalidOperationException("Ambiguous component reference: " + property.propertyPath);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unsupported property: " + property.propertyPath);
            }
        }

        private static void ApplyTransform(Transform transform, JObject node)
        {
            foreach (string key in new[] { "localPosition", "localScale", "localRotation" })
            {
                JArray values = node[key] as JArray;
                if (values == null || values.Count != (key == "localRotation" ? 4 : 3)
                    || values.Any(value => float.IsNaN((float)value) || float.IsInfinity((float)value)))
                {
                    throw new InvalidOperationException("Invalid transform array: " + key);
                }
            }
            JArray scale = (JArray)node["localScale"];
            JArray rotation = (JArray)node["localRotation"];
            JArray position = (JArray)node["localPosition"];
            transform.localPosition = new Vector3((float)position[0], (float)position[1], (float)position[2]);
            transform.localRotation = new Quaternion((float)rotation[0], (float)rotation[1], (float)rotation[2], (float)rotation[3]);
            transform.localScale = new Vector3((float)scale[0], (float)scale[1], (float)scale[2]);
            if (transform is RectTransform rect)
            {
                JObject spec = (JObject)node["rect"];
                rect.anchorMin = Vector(spec["anchorMin"]);
                rect.anchorMax = Vector(spec["anchorMax"]);
                rect.pivot = Vector(spec["pivot"]);
                rect.sizeDelta = Vector(spec["sizeDelta"]);
                rect.anchoredPosition = Vector(spec["anchoredPosition"]);
            }
        }

        private static Vector2 Vector(JToken value)
        {
            if (!(value is JArray values) || values.Count != 2
                || values.Any(item => float.IsNaN((float)item) || float.IsInfinity((float)item)))
            {
                throw new InvalidOperationException("Invalid rect vector.");
            }
            return new Vector2((float)value[0], (float)value[1]);
        }

        private static void EnsureFolder(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
            }
        }
    }
}
