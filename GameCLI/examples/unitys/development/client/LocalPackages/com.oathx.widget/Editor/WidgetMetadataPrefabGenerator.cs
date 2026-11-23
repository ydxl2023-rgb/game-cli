using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace Oathx.Widget.Editor
{
    /// <summary>
    /// 为 UnityCLI 提供 Prefab Metadata 自动生成能力。
    /// </summary>
    public static class WidgetMetadataPrefabGenerator
    {
        /// <summary>
        /// 根据 VM 的 Binding 字段和 Prefab 节点命名生成 Metadata。
        /// </summary>
        /// <param name="prefabPath">目标 Prefab 的 Unity 资产路径</param>
        /// <param name="uiName">UI 类型名称</param>
        /// <param name="vmName">ViewModel 类型名称</param>
        /// <param name="dryRun">是否只校验而不保存 Prefab</param>
        /// <returns>可供 UnityCLI 序列化的生成结果</returns>
        public static object Generate(string prefabPath, string uiName, string vmName, bool dryRun)
        {
            var result = CreateResult(prefabPath, uiName, vmName, dryRun);
            if (string.IsNullOrEmpty(prefabPath) || !prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                result.data.issues.Add("prefabPath 必须指向 .prefab 资产");
                return Complete(result, false);
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                result.data.issues.Add($"找不到 Prefab：{prefabPath}");
                return Complete(result, false);
            }

            var vmType = ResolveType(vmName);
            if (vmType == null || !typeof(VM).IsAssignableFrom(vmType))
            {
                result.data.issues.Add($"找不到 VM 类型或类型未继承 VM：{vmName}");
                return Complete(result, false);
            }

            var vmScript = FindMonoScript(vmType);
            if (vmScript == null)
            {
                result.data.issues.Add($"找不到 VM MonoScript：{vmType.FullName}");
                return Complete(result, false);
            }

            var bindingFields = vmType
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(field => field.GetCustomAttribute<BindingAttribute>() != null)
                .ToArray();
            result.data.fieldCount = bindingFields.Length;

            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(prefabPath);
                var generatedFields = BuildFields(root, bindingFields, result.data.issues);
                result.data.boundTargetCount = generatedFields.Count;
                result.data.metadataAdded = root.GetComponent<Metadata>() == null;

                if (result.data.issues.Count > 0)
                {
                    return Complete(result, false);
                }

                if (!dryRun)
                {
                    var metadata = root.GetComponent<Metadata>() ?? root.AddComponent<Metadata>();
                    metadata.className = vmType.FullName;
                    metadata.classGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(vmScript));
                    metadata.metadatas = generatedFields;
                    EditorUtility.SetDirty(metadata);
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                    AssetDatabase.SaveAssets();
                }

                return Complete(result, true);
            }
            catch (Exception exception)
            {
                result.data.issues.Add(exception.Message);
                return Complete(result, false);
            }
            finally
            {
                if (root != null)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        /// <summary>
        /// 将 VM Binding 字段解析为 Prefab Metadata 字段，并收集未匹配问题。
        /// </summary>
        /// <param name="root">Prefab 根节点</param>
        /// <param name="bindingFields">VM 中声明 Binding 的字段</param>
        /// <param name="issues">生成过程中发现的问题</param>
        /// <returns>已成功解析的 Metadata 字段</returns>
        private static List<MetadataField> BuildFields(GameObject root, FieldInfo[] bindingFields,
            List<string> issues)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            var fields = new List<MetadataField>(bindingFields.Length);
            foreach (var fieldInfo in bindingFields)
            {
                string syncTypeName = ResolveSyncTypeName(fieldInfo);
                var syncType = ResolveType(syncTypeName);
                if (syncType == null || !typeof(ISyncAction).IsAssignableFrom(syncType))
                {
                    issues.Add($"字段 {fieldInfo.Name} 找不到 SyncAction：{syncTypeName}");
                    continue;
                }

                var target = FindTarget(transforms, fieldInfo);
                if (target == null)
                {
                    issues.Add($"字段 {fieldInfo.Name} 未匹配到 Prefab 节点");
                    continue;
                }

                var syncBinding = syncType.GetCustomAttribute<BindingAttribute>();
                var componentType = ResolveComponentType(syncBinding?.name);
                if (componentType == null)
                {
                    issues.Add($"字段 {fieldInfo.Name} 无法解析绑定组件：{syncBinding?.name}");
                    continue;
                }

                var component = target.gameObject.GetComponent(componentType);
                if (component == null)
                {
                    issues.Add($"节点 {target.name} 缺少组件 {componentType.Name}（字段 {fieldInfo.Name}）");
                    continue;
                }

                fields.Add(new MetadataField
                {
                    name = fieldInfo.Name,
                    type = syncType.Name,
                    hash = syncType.Name.GetHashCode(),
                    targets = new List<GameObject> { target.gameObject },
                    components = new List<Component> { component }
                });
            }

            return fields;
        }

        private static string ResolveSyncTypeName(FieldInfo fieldInfo)
        {
            var binding = fieldInfo.GetCustomAttribute<BindingAttribute>();
            if (!string.IsNullOrEmpty(binding?.name))
            {
                return binding.name;
            }

            if (fieldInfo.FieldType == typeof(ButtonClick))
            {
                return typeof(SyncClick).Name;
            }

            return typeof(SyncVisible).Name;
        }

        private static Transform FindTarget(Transform[] transforms, FieldInfo fieldInfo)
        {
            foreach (string candidate in BuildTargetCandidates(fieldInfo))
            {
                var matches = transforms
                    .Where(transform => string.Equals(transform.name, candidate, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (matches.Length == 1)
                {
                    return matches[0];
                }
            }

            return null;
        }

        /// <summary>
        /// 根据字段名和绑定类型生成按优先级排列的节点候选名称。
        /// </summary>
        /// <param name="fieldInfo">VM Binding 字段</param>
        /// <returns>节点候选名称序列</returns>
        private static IEnumerable<string> BuildTargetCandidates(FieldInfo fieldInfo)
        {
            string fieldName = ToPascalCase(fieldInfo.Name);
            yield return fieldName;

            string baseName = fieldName;
            if (baseName.EndsWith("Click", StringComparison.Ordinal))
            {
                baseName = baseName.Substring(0, baseName.Length - "Click".Length);
                yield return baseName;
            }

            if (baseName.EndsWith("Interactable", StringComparison.Ordinal))
            {
                baseName = baseName.Substring(0, baseName.Length - "Interactable".Length);
                yield return baseName;
            }

            if (baseName.StartsWith("Can", StringComparison.Ordinal) && baseName.Length > 3)
            {
                baseName = baseName.Substring(3);
                yield return baseName;
            }

            bool isButtonBinding = fieldInfo.FieldType == typeof(ButtonClick)
                || string.Equals(ResolveSyncTypeName(fieldInfo), typeof(SyncInteractable).Name,
                    StringComparison.Ordinal);
            if (isButtonBinding && !baseName.EndsWith("Button", StringComparison.Ordinal))
            {
                yield return baseName + "Button";
            }
        }

        /// <summary>
        /// 将 SyncAction 声明的组件名称解析为 Unity 组件类型。
        /// </summary>
        /// <param name="typeName">组件类型短名或完整名称</param>
        /// <returns>解析出的组件类型；未找到时返回 null</returns>
        private static Type ResolveComponentType(string typeName)
        {
            if (string.Equals(typeName, nameof(RectTransform), StringComparison.Ordinal))
            {
                return typeof(RectTransform);
            }

            if (string.Equals(typeName, nameof(UnityEngine.UI.Button), StringComparison.Ordinal))
            {
                return typeof(UnityEngine.UI.Button);
            }

            if (string.Equals(typeName, "TMP_Text", StringComparison.Ordinal))
            {
                return Type.GetType("TMPro.TMP_Text, Unity.TextMeshPro");
            }

            if (string.Equals(typeName, "TextMeshProUGUI", StringComparison.Ordinal))
            {
                return Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            }

            var type = ResolveType(typeName);
            if (type != null && typeof(Component).IsAssignableFrom(type))
            {
                return type;
            }

            // 通用 fallback：按短名从所有 MonoBehaviour 派生类型里查找，
            // 覆盖 Image / FancyView / RawImage 等未在上面显式枚举的 Unity 组件。
            return TypeCache.GetTypesDerivedFrom<MonoBehaviour>()
                .FirstOrDefault(c => string.Equals(c.Name, typeName, StringComparison.Ordinal));
        }

        /// <summary>
        /// 优先从已知类型和 Unity TypeCache 中解析 VM 或 SyncAction 类型。
        /// </summary>
        /// <param name="typeName">类型短名或完整名称</param>
        /// <returns>解析出的类型；未找到时返回 null</returns>
        private static Type ResolveType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            Type knownType = ResolveKnownType(typeName);
            if (knownType != null)
            {
                return knownType;
            }

            return TypeCache.GetTypesDerivedFrom<VM>().FirstOrDefault(candidate =>
                    string.Equals(candidate.Name, typeName, StringComparison.Ordinal)
                    || string.Equals(candidate.FullName, typeName, StringComparison.Ordinal))
                ?? TypeCache.GetTypesDerivedFrom<ISyncAction>().FirstOrDefault(candidate =>
                    string.Equals(candidate.Name, typeName, StringComparison.Ordinal)
                    || string.Equals(candidate.FullName, typeName, StringComparison.Ordinal));
        }

        /// <summary>
        /// 解析 Metadata 生成流程常用的内置 SyncAction 类型。
        /// </summary>
        /// <param name="typeName">SyncAction 类型名称</param>
        /// <returns>匹配的已知类型；未匹配时返回 null</returns>
        private static Type ResolveKnownType(string typeName)
        {
            if (string.Equals(typeName, typeof(SyncTextMeshPro).Name, StringComparison.Ordinal))
            {
                return typeof(SyncTextMeshPro);
            }

            if (string.Equals(typeName, typeof(SyncSizeDelta).Name, StringComparison.Ordinal))
            {
                return typeof(SyncSizeDelta);
            }

            if (string.Equals(typeName, typeof(SyncInteractable).Name, StringComparison.Ordinal))
            {
                return typeof(SyncInteractable);
            }

            if (string.Equals(typeName, typeof(SyncClick).Name, StringComparison.Ordinal))
            {
                return typeof(SyncClick);
            }

            if (string.Equals(typeName, typeof(SyncVisible).Name, StringComparison.Ordinal))
            {
                return typeof(SyncVisible);
            }

            return null;
        }

        private static MonoScript FindMonoScript(Type vmType)
        {
            foreach (string guid in AssetDatabase.FindAssets($"{vmType.Name} t:MonoScript"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == vmType)
                {
                    return script;
                }
            }

            return null;
        }

        private static string ToPascalCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        /// <summary>
        /// 创建带有输入回显和空问题列表的生成结果。
        /// </summary>
        /// <param name="prefabPath">目标 Prefab 路径</param>
        /// <param name="uiName">UI 类型名称</param>
        /// <param name="vmName">ViewModel 类型名称</param>
        /// <param name="dryRun">是否只校验而不保存</param>
        /// <returns>初始化后的生成结果</returns>
        private static WidgetMetadataGenerationResult CreateResult(string prefabPath, string uiName, string vmName,
            bool dryRun)
        {
            return new WidgetMetadataGenerationResult
            {
                data = new WidgetMetadataGenerationData
                {
                    prefabPath = prefabPath ?? string.Empty,
                    uiName = uiName ?? string.Empty,
                    vmName = vmName ?? string.Empty,
                    dryRun = dryRun,
                    issues = new List<string>()
                },
                error = new WidgetMetadataGenerationError()
            };
        }

        /// <summary>
        /// 写入最终状态和错误信息后返回生成结果。
        /// </summary>
        /// <param name="result">待完成的生成结果</param>
        /// <param name="success">生成是否成功</param>
        /// <returns>已写入最终状态的生成结果</returns>
        private static WidgetMetadataGenerationResult Complete(WidgetMetadataGenerationResult result, bool success)
        {
            result.ok = success;
            result.status = success ? (result.data.dryRun ? "dry-run" : "generated") : "failed";
            result.message = success
                ? $"Widget metadata {(result.data.dryRun ? "dry-run" : "generation")} passed"
                : "Widget metadata generation failed";
            result.error.code = success ? string.Empty : "widget_metadata_failed";
            result.error.message = success ? string.Empty : string.Join("; ", result.data.issues);
            return result;
        }
    }

    /// <summary>
    /// UnityCLI Widget Metadata 生成结果。
    /// </summary>
    [Serializable]
    public sealed class WidgetMetadataGenerationResult
    {
        /// <summary>
        /// 生成是否成功。
        /// </summary>
        public bool ok;

        /// <summary>
        /// 当前生成状态。
        /// </summary>
        public string status;

        /// <summary>
        /// 面向调用方的结果说明。
        /// </summary>
        public string message;

        /// <summary>
        /// Metadata 生成详情。
        /// </summary>
        public WidgetMetadataGenerationData data;

        /// <summary>
        /// 失败时的错误信息。
        /// </summary>
        public WidgetMetadataGenerationError error;
    }

    /// <summary>
    /// UnityCLI Widget Metadata 生成详情。
    /// </summary>
    [Serializable]
    public sealed class WidgetMetadataGenerationData
    {
        /// <summary>
        /// UI 类型名称。
        /// </summary>
        public string uiName;

        /// <summary>
        /// ViewModel 类型名称。
        /// </summary>
        public string vmName;

        /// <summary>
        /// 目标 Prefab 资产路径。
        /// </summary>
        public string prefabPath;

        /// <summary>
        /// 是否仅执行校验。
        /// </summary>
        public bool dryRun;

        /// <summary>
        /// 本次生成是否新增 Metadata 组件。
        /// </summary>
        public bool metadataAdded;

        /// <summary>
        /// VM Binding 字段数量。
        /// </summary>
        public int fieldCount;

        /// <summary>
        /// 已绑定的目标数量。
        /// </summary>
        public int boundTargetCount;

        /// <summary>
        /// 生成或校验过程中发现的问题。
        /// </summary>
        public List<string> issues;
    }

    /// <summary>
    /// UnityCLI Widget Metadata 生成错误。
    /// </summary>
    [Serializable]
    public sealed class WidgetMetadataGenerationError
    {
        /// <summary>
        /// 机器可读的错误码。
        /// </summary>
        public string code;

        /// <summary>
        /// 面向调用方的错误说明。
        /// </summary>
        public string message;
    }
}
