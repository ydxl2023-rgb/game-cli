using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// 对 Prefab 资产执行只读结构检查。
    /// </summary>
    internal static class UnityCliPrefabInspectService
    {
        const string MetadataTypeName = "Oathx.Widget.Metadata";

        /// <summary>
        /// 检查 Prefab 层级、组件、引用以及可选档案约束。
        /// </summary>
        /// <param name="request">桥接请求</param>
        /// <returns>Prefab 检查数据</returns>
        public static PrefabData Execute(UnityCliBridgeRequest request)
        {
            var prefabPath = NormalizePrefabPath(request.prefabPath);
            var profile = NormalizeProfile(request.prefabProfile);
            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                throw new InvalidOperationException("Prefab asset was not found: " + prefabPath);
            }

            var issues = new List<string>();
            var data = new PrefabData
            {
                action = "inspect",
                prefabPath = prefabPath,
                profile = profile,
                recursive = request.recursive,
                dryRun = true,
                respondedAtUtc = DateTime.UtcNow.ToString("O")
            };

            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(prefabPath);
                var nodes = request.recursive
                    ? root.GetComponentsInChildren<Transform>(true).Select(transform => transform.gameObject).ToArray()
                    : new[] { root };
                data.nodes = nodes.Select(node => InspectNode(root, node, data, issues)).ToArray();
                data.nodeCount = data.nodes.Length;
                ValidateProfile(root, profile, issues);
            }
            finally
            {
                if (root != null)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            data.issues = issues.Distinct().ToArray();
            data.valid = data.issues.Length == 0;
            data.message = data.valid ? "prefab inspection passed" : "prefab inspection found issues";
            return data;
        }

        /// <summary>
        /// 检查单个层级节点并更新聚合计数。
        /// </summary>
        static PrefabNodeData InspectNode(GameObject root, GameObject node, PrefabData aggregate, List<string> issues)
        {
            var path = GetHierarchyPath(root.transform, node.transform);
            var missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node);
            aggregate.missingScriptCount += missingScriptCount;
            if (missingScriptCount > 0)
            {
                issues.Add(path + " contains " + missingScriptCount + " missing MonoBehaviour script(s).");
            }

            var components = new List<PrefabComponentData>();
            foreach (var component in node.GetComponents<Component>())
            {
                if (component == null)
                {
                    continue;
                }

                var references = InspectObjectReferences(root, component, aggregate, issues);
                components.Add(new PrefabComponentData
                {
                    type = component.GetType().FullName ?? component.GetType().Name,
                    objectReferences = references.ToArray()
                });
            }

            aggregate.componentCount += components.Count;
            var rectTransform = node.transform as RectTransform;
            return new PrefabNodeData
            {
                path = path,
                activeSelf = node.activeSelf,
                layer = node.layer,
                layerName = LayerMask.LayerToName(node.layer),
                rectTransform = rectTransform == null ? string.Empty : FormatRectTransform(rectTransform),
                missingScriptCount = missingScriptCount,
                components = components.ToArray()
            };
        }

        /// <summary>
        /// 枚举组件的序列化对象引用并报告失效引用。
        /// </summary>
        static List<PrefabObjectReferenceData> InspectObjectReferences(
            GameObject root,
            Component component,
            PrefabData aggregate,
            List<string> issues)
        {
            var result = new List<PrefabObjectReferenceData>();
            SerializedObject serializedObject;
            try
            {
                serializedObject = new SerializedObject(component);
            }
            catch (Exception exception)
            {
                issues.Add("Could not inspect " + component.GetType().FullName + ": " + exception.Message);
                return result;
            }

            var iterator = serializedObject.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyType != SerializedPropertyType.ObjectReference
                    || string.Equals(iterator.propertyPath, "m_Script", StringComparison.Ordinal))
                {
                    continue;
                }

                var value = iterator.objectReferenceValue;
                var missing = value == null && iterator.objectReferenceInstanceIDValue != 0;
                if (value == null && !missing)
                {
                    continue;
                }

                var reference = new PrefabObjectReferenceData
                {
                    propertyPath = iterator.propertyPath,
                    missing = missing
                };
                if (missing)
                {
                    aggregate.missingReferenceCount++;
                    issues.Add(GetHierarchyPath(root.transform, component.transform) + "." + iterator.propertyPath + " is a missing object reference.");
                }
                else
                {
                    reference.target = GetObjectTarget(root, value);
                    reference.assetPath = AssetDatabase.GetAssetPath(value) ?? string.Empty;
                    aggregate.objectReferenceCount++;
                }

                result.Add(reference);
            }

            return result;
        }

        static void ValidateProfile(GameObject root, string profile, List<string> issues)
        {
            if (string.IsNullOrEmpty(profile))
            {
                return;
            }

            var components = root.GetComponentsInChildren<Component>(true).Where(component => component != null).ToArray();
            switch (profile)
            {
                case "widget":
                    ValidateWidgetProfile(root, issues);
                    break;
                case "fancy-view":
                    ValidateFancyViewProfile(components, issues);
                    break;
                case "fancy-view-cell":
                    ValidateFancyViewCellProfile(root, components, issues);
                    break;
            }
        }

        /// <summary>
        /// 校验 Widget Prefab 的根 Metadata 约束。
        /// </summary>
        static void ValidateWidgetProfile(GameObject root, List<string> issues)
        {
            var metadata = root.GetComponents<Component>()
                .FirstOrDefault(component => component != null && string.Equals(component.GetType().FullName, MetadataTypeName, StringComparison.Ordinal));
            if (metadata == null)
            {
                issues.Add("widget profile requires Oathx.Widget.Metadata on the prefab root.");
                return;
            }

            var serializedMetadata = new SerializedObject(metadata);
            var className = serializedMetadata.FindProperty("className")?.stringValue ?? string.Empty;
            var classGuid = serializedMetadata.FindProperty("classGuid")?.stringValue ?? string.Empty;
            if (string.IsNullOrWhiteSpace(className))
            {
                issues.Add("widget profile requires a non-empty Metadata.className.");
            }

            ValidateMetadataGuid(className, classGuid, issues);
            ValidateMetadataReferences(serializedMetadata.FindProperty("metadatas"), issues);

        }

        /// <summary>
        /// 校验 FancyView 的 Prefab、容器、尺寸和 Scroller 引用。
        /// </summary>
        static void ValidateFancyViewProfile(Component[] components, List<string> issues)
        {
            var fancyView = components.FirstOrDefault(component => InheritsFrom(component.GetType(), "Oathx.Widget.FancyView"));
            if (fancyView == null)
            {
                issues.Add("fancy-view profile could not find Oathx.Widget.FancyView or a subclass.");
                return;
            }

            var serializedView = new SerializedObject(fancyView);
            RequireObjectReference(serializedView, "cellPrefab", "FancyView.cellPrefab", issues);
            RequireObjectReference(serializedView, "cellContainer", "FancyView.cellContainer", issues);
            var cellSize = serializedView.FindProperty("cellSize");
            if (cellSize == null || cellSize.floatValue <= 0f)
            {
                issues.Add("FancyView.cellSize must be greater than zero.");
            }

            if (!components.Any(component => string.Equals(component.GetType().Name, "Scroller", StringComparison.Ordinal)))
            {
                issues.Add("fancy-view profile requires a Scroller component.");
            }
        }

        static void ValidateFancyViewCellProfile(GameObject root, Component[] components, List<string> issues)
        {
            if (!components.Any(component => InheritsFrom(component.GetType(), "Oathx.Widget.FancyViewCell")))
            {
                issues.Add("fancy-view-cell profile could not find Oathx.Widget.FancyViewCell or a subclass.");
            }

            if (!root.GetComponents<Component>().Any(component => component != null && string.Equals(component.GetType().FullName, MetadataTypeName, StringComparison.Ordinal)))
            {
                issues.Add("fancy-view-cell profile requires Oathx.Widget.Metadata on the prefab root.");
            }

            var rectTransform = root.transform as RectTransform;
            if (rectTransform == null || rectTransform.rect.width <= 0f || rectTransform.rect.height <= 0f)
            {
                issues.Add("fancy-view-cell profile requires a root RectTransform with positive width and height.");
            }
        }

        /// <summary>
        /// 校验 Metadata GUID 是否解析为同名 VM MonoScript。
        /// </summary>
        static void ValidateMetadataGuid(string className, string classGuid, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(classGuid))
            {
                issues.Add("Metadata.classGuid is empty; it must be the VM MonoScript .meta GUID.");
                return;
            }

            var scriptPath = AssetDatabase.GUIDToAssetPath(classGuid);
            var script = string.IsNullOrEmpty(scriptPath) ? null : AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            var scriptType = script == null ? null : script.GetClass();
            if (scriptType == null)
            {
                issues.Add("Metadata.classGuid does not resolve to a compiled MonoScript: " + classGuid);
                return;
            }

            if (!string.Equals(scriptType.Name, className, StringComparison.Ordinal))
            {
                issues.Add("Metadata.classGuid resolves to '" + scriptType.Name + "' but className is '" + className + "'.");
            }

            if (!InheritsFrom(scriptType, "Oathx.Widget.VM"))
            {
                issues.Add("Metadata.classGuid resolves to a class that does not inherit Oathx.Widget.VM: " + scriptType.FullName);
            }
        }

        static void ValidateMetadataReferences(SerializedProperty entries, List<string> issues)
        {
            if (entries == null || !entries.isArray)
            {
                issues.Add("Metadata.metadatas could not be read.");
                return;
            }

            for (var index = 0; index < entries.arraySize; index++)
            {
                var entry = entries.GetArrayElementAtIndex(index);
                var name = entry.FindPropertyRelative("name")?.stringValue ?? "metadatas[" + index + "]";
                ValidateReferenceArray(entry.FindPropertyRelative("targets"), name + ".targets", issues);
                ValidateReferenceArray(entry.FindPropertyRelative("components"), name + ".components", issues);
            }
        }

        static void ValidateReferenceArray(SerializedProperty references, string label, List<string> issues)
        {
            if (references == null || !references.isArray)
            {
                issues.Add("Metadata reference array is missing: " + label);
                return;
            }

            for (var index = 0; index < references.arraySize; index++)
            {
                if (references.GetArrayElementAtIndex(index).objectReferenceValue == null)
                {
                    issues.Add("Metadata reference is null or missing: " + label + "[" + index + "].");
                }
            }
        }

        static void RequireObjectReference(SerializedObject serializedObject, string propertyName, string label, List<string> issues)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
            {
                issues.Add(label + " must reference an object.");
            }
        }

        static bool InheritsFrom(Type type, string baseTypeFullName)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                if (string.Equals(current.FullName, baseTypeFullName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        static string GetObjectTarget(GameObject root, UnityEngine.Object value)
        {
            if (value is Component component)
            {
                return GetHierarchyPath(root.transform, component.transform) + " (" + component.GetType().Name + ")";
            }

            if (value is GameObject gameObject && gameObject.transform.IsChildOf(root.transform))
            {
                return GetHierarchyPath(root.transform, gameObject.transform);
            }

            return value.name;
        }

        static string GetHierarchyPath(Transform root, Transform target)
        {
            var names = new Stack<string>();
            for (var current = target; current != null; current = current.parent)
            {
                names.Push(current.name);
                if (current == root)
                {
                    break;
                }
            }

            return string.Join("/", names.ToArray());
        }

        static string FormatRectTransform(RectTransform rectTransform)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "anchorMin=({0:0.###},{1:0.###}) anchorMax=({2:0.###},{3:0.###}) pivot=({4:0.###},{5:0.###}) anchoredPosition=({6:0.###},{7:0.###}) sizeDelta=({8:0.###},{9:0.###})",
                rectTransform.anchorMin.x,
                rectTransform.anchorMin.y,
                rectTransform.anchorMax.x,
                rectTransform.anchorMax.y,
                rectTransform.pivot.x,
                rectTransform.pivot.y,
                rectTransform.anchoredPosition.x,
                rectTransform.anchoredPosition.y,
                rectTransform.sizeDelta.x,
                rectTransform.sizeDelta.y);
        }

        static string NormalizeProfile(string profile)
        {
            var normalized = (profile ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(normalized)
                || normalized == "widget"
                || normalized == "fancy-view"
                || normalized == "fancy-view-cell")
            {
                return normalized;
            }

            throw new InvalidOperationException("--profile must be widget, fancy-view, or fancy-view-cell.");
        }

        static string NormalizePrefabPath(string path)
        {
            var normalized = (path ?? string.Empty).Trim().Replace('\\', '/');
            if (!normalized.StartsWith("Assets/", StringComparison.Ordinal)
                && !normalized.StartsWith("Packages/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("--prefab must be a project asset path under Assets/ or Packages/.");
            }

            if (!normalized.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("--prefab must end with .prefab.");
            }

            return normalized;
        }
    }
}
