using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// 校验现有 Widget Metadata 的类型身份和序列化绑定。
    /// </summary>
    internal static class UnityCliWidgetMetadataValidationService
    {
        const string MetadataTypeName = "Oathx.Widget.Metadata";
        const string VmTypeName = "Oathx.Widget.VM";

        /// <summary>
        /// 在不修改 Prefab 资产的情况下校验 Metadata。
        /// </summary>
        /// <param name="request">桥接请求</param>
        /// <returns>校验响应</returns>
        public static WidgetMetadataValidationResult Execute(UnityCliBridgeRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("Empty request.");
            }

            var prefabPath = NormalizePrefabPath(request.prefabPath);
            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                throw new InvalidOperationException("Prefab asset was not found: " + prefabPath);
            }

            var issues = new List<string>();
            var data = new WidgetMetadataValidationData
            {
                prefabPath = prefabPath,
                uiName = request.uiName ?? string.Empty,
                vmName = request.vmName ?? string.Empty,
                respondedAtUtc = DateTime.UtcNow.ToString("O")
            };

            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(prefabPath);
                ValidateLoadedPrefab(root, request, data, issues);
            }
            finally
            {
                if (root != null)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            data.issues = issues.ToArray();
            return WidgetMetadataValidationResult.Create(data);
        }

        /// <summary>
        /// 校验已加载 Prefab 根 Metadata 的类型身份与绑定。
        /// </summary>
        static void ValidateLoadedPrefab(
            GameObject root,
            UnityCliBridgeRequest request,
            WidgetMetadataValidationData data,
            List<string> issues)
        {
            var metadataComponents = root.GetComponentsInChildren<Component>(true)
                .Where(component => component != null && string.Equals(component.GetType().FullName, MetadataTypeName, StringComparison.Ordinal))
                .ToArray();
            data.metadataComponentCount = metadataComponents.Length;
            var rootMetadataComponents = root.GetComponents<Component>()
                .Where(component => component != null && string.Equals(component.GetType().FullName, MetadataTypeName, StringComparison.Ordinal))
                .ToArray();
            if (rootMetadataComponents.Length == 0)
            {
                issues.Add("Prefab root does not contain Oathx.Widget.Metadata.");
                return;
            }

            if (rootMetadataComponents.Length > 1)
            {
                issues.Add("Prefab root contains multiple Metadata components; expected exactly one.");
            }

            var metadata = rootMetadataComponents[0];
            var serializedMetadata = new SerializedObject(metadata);
            var classNameProperty = serializedMetadata.FindProperty("className");
            var classGuidProperty = serializedMetadata.FindProperty("classGuid");
            var metadatasProperty = serializedMetadata.FindProperty("metadatas");

            data.actualClassName = classNameProperty == null ? string.Empty : classNameProperty.stringValue;
            data.actualClassGuid = classGuidProperty == null ? string.Empty : classGuidProperty.stringValue;
            ReadBindings(metadatasProperty, data, issues);

            var expectedVmName = ResolveExpectedVmName(request, root);
            data.vmName = expectedVmName;
            var expectedVmType = ResolveType(expectedVmName);
            if (expectedVmType == null)
            {
                issues.Add("Expected VM type could not be resolved: " + expectedVmName);
                InspectActualGuid(data, issues);
                return;
            }

            data.expectedClassName = expectedVmType.Name;
            if (!InheritsFrom(expectedVmType, VmTypeName))
            {
                issues.Add("Expected type does not inherit Oathx.Widget.VM: " + expectedVmType.FullName);
            }

            if (!TryFindMonoScript(expectedVmType, out var expectedScriptPath, out var expectedClassGuid))
            {
                issues.Add("MonoScript asset could not be resolved for VM type: " + expectedVmType.FullName);
            }
            else
            {
                data.expectedScriptPath = expectedScriptPath;
                data.expectedClassGuid = expectedClassGuid;
            }

            if (!string.Equals(data.actualClassName, data.expectedClassName, StringComparison.Ordinal))
            {
                issues.Add("Metadata.className is '" + data.actualClassName + "' but expected '" + data.expectedClassName + "'.");
            }

            if (!string.IsNullOrEmpty(data.expectedClassGuid)
                && !string.Equals(data.actualClassGuid, data.expectedClassGuid, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add("Metadata.classGuid does not match the expected VM MonoScript GUID.");
            }

            InspectActualGuid(data, issues);
            if (!string.IsNullOrEmpty(data.actualScriptClassName)
                && !string.Equals(data.actualScriptClassName, data.expectedClassName, StringComparison.Ordinal))
            {
                issues.Add("Metadata.classGuid resolves to '" + data.actualScriptClassName + "' instead of '" + data.expectedClassName + "'.");
            }
        }

        /// <summary>
        /// 读取 Metadata 字段并汇总绑定问题。
        /// </summary>
        static void ReadBindings(SerializedProperty metadatasProperty, WidgetMetadataValidationData data, List<string> issues)
        {
            if (metadatasProperty == null || !metadatasProperty.isArray)
            {
                issues.Add("Metadata.metadatas could not be read.");
                return;
            }

            data.metadataCount = metadatasProperty.arraySize;
            for (var index = 0; index < metadatasProperty.arraySize; index++)
            {
                var entry = metadatasProperty.GetArrayElementAtIndex(index);
                var nameProperty = entry.FindPropertyRelative("name");
                var typeProperty = entry.FindPropertyRelative("type");
                var entryName = nameProperty == null ? string.Empty : nameProperty.stringValue;
                if (string.IsNullOrWhiteSpace(entryName))
                {
                    issues.Add("Metadata.metadatas[" + index + "].name is empty.");
                    entryName = "metadatas[" + index + "]";
                }

                if (typeProperty == null || string.IsNullOrWhiteSpace(typeProperty.stringValue))
                {
                    issues.Add("Metadata field '" + entryName + "' has an empty type.");
                }

                ReadReferenceArray(entry.FindPropertyRelative("targets"), entryName + ".targets", data, issues);
                ReadReferenceArray(entry.FindPropertyRelative("components"), entryName + ".components", data, issues);
            }
        }

        /// <summary>
        /// 读取单个 Metadata 引用数组并统计空引用。
        /// </summary>
        static void ReadReferenceArray(
            SerializedProperty references,
            string label,
            WidgetMetadataValidationData data,
            List<string> issues)
        {
            if (references == null || !references.isArray)
            {
                issues.Add("Metadata reference array is missing: " + label);
                return;
            }

            for (var index = 0; index < references.arraySize; index++)
            {
                var reference = references.GetArrayElementAtIndex(index);
                if (reference.objectReferenceValue == null)
                {
                    data.missingReferenceCount++;
                    issues.Add("Metadata reference is null or missing: " + label + "[" + index + "].");
                }
                else
                {
                    data.boundTargetCount++;
                }
            }
        }

        /// <summary>
        /// 反解现有 classGuid 并校验实际脚本类型。
        /// </summary>
        static void InspectActualGuid(WidgetMetadataValidationData data, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(data.actualClassGuid))
            {
                issues.Add("Metadata.classGuid is empty; it must be the VM MonoScript .meta GUID.");
                return;
            }

            data.actualScriptPath = AssetDatabase.GUIDToAssetPath(data.actualClassGuid);
            if (string.IsNullOrEmpty(data.actualScriptPath))
            {
                issues.Add("Metadata.classGuid does not resolve to an asset: " + data.actualClassGuid);
                return;
            }

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(data.actualScriptPath);
            var scriptType = script == null ? null : script.GetClass();
            if (scriptType == null)
            {
                issues.Add("Metadata.classGuid does not resolve to a compiled MonoScript class: " + data.actualScriptPath);
                return;
            }

            data.actualScriptClassName = scriptType.Name;
            if (!InheritsFrom(scriptType, VmTypeName))
            {
                issues.Add("Metadata.classGuid resolves to a class that is not a VM: " + scriptType.FullName);
            }
        }

        /// <summary>
        /// 按显式参数、UI 泛型参数和命名约定解析预期 VM 名称。
        /// </summary>
        static string ResolveExpectedVmName(UnityCliBridgeRequest request, GameObject root)
        {
            if (!string.IsNullOrWhiteSpace(request.vmName))
            {
                return request.vmName.Trim();
            }

            var uiName = (request.uiName ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(uiName))
            {
                var uiType = ResolveType(uiName);
                var vmType = FindUiWidgetVmType(uiType);
                if (vmType != null)
                {
                    return vmType.FullName ?? vmType.Name;
                }

                if (uiName.StartsWith("UI", StringComparison.Ordinal) && uiName.Length > 2)
                {
                    return "VM" + uiName.Substring(2);
                }
            }

            var widgetComponent = root.GetComponentsInChildren<Component>(true)
                .FirstOrDefault(component => component != null && FindUiWidgetVmType(component.GetType()) != null);
            var inferredVmType = widgetComponent == null ? null : FindUiWidgetVmType(widgetComponent.GetType());
            return inferredVmType == null ? string.Empty : inferredVmType.FullName ?? inferredVmType.Name;
        }

        static Type FindUiWidgetVmType(Type type)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                if (!current.IsGenericType)
                {
                    continue;
                }

                var definition = current.GetGenericTypeDefinition();
                if (string.Equals(definition.FullName, "Oathx.Widget.UIWidget`1", StringComparison.Ordinal))
                {
                    return current.GetGenericArguments()[0];
                }
            }

            return null;
        }

        /// <summary>
        /// 从当前 AppDomain 解析唯一匹配的类型。
        /// </summary>
        static Type ResolveType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            var simpleNameMatches = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types.Where(type => type != null).ToArray();
                }
                catch
                {
                    continue;
                }

                var fullNameMatch = types.FirstOrDefault(type => string.Equals(type.FullName, typeName, StringComparison.Ordinal));
                if (fullNameMatch != null)
                {
                    return fullNameMatch;
                }
                simpleNameMatches.AddRange(types.Where(type => string.Equals(type.Name, typeName, StringComparison.Ordinal)));
            }

            return simpleNameMatches.Count == 1 ? simpleNameMatches[0] : null;
        }

        static bool TryFindMonoScript(Type type, out string scriptPath, out string guid)
        {
            foreach (var candidateGuid in AssetDatabase.FindAssets("t:MonoScript " + type.Name))
            {
                var candidatePath = AssetDatabase.GUIDToAssetPath(candidateGuid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(candidatePath);
                if (script != null && script.GetClass() == type)
                {
                    scriptPath = candidatePath;
                    guid = candidateGuid;
                    return true;
                }
            }

            scriptPath = string.Empty;
            guid = string.Empty;
            return false;
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
