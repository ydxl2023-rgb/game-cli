using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Composes prefab assets by nesting one prefab under a node inside another prefab.
    /// </summary>
    internal static class UnityCliPrefabService
    {
        /// <summary>
        /// Executes a prefab command.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <returns>The prefab command result data.</returns>
        public static PrefabData Execute(UnityCliBridgeRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("Empty request.");
            }

            var action = (request.action ?? string.Empty).Trim().ToLowerInvariant();
            if (action == "motion-preview")
            {
                return UnityCliMotionPreviewService.Execute(request);
            }
            if (action == "configure-textures")
            {
                return UnityCliSnapshotVisuals.ConfigureManifest(request);
            }
            if (action == "restore" || action == "preview")
            {
                return UnityCliPrefabRestoreService.Execute(request);
            }
            if (action == "export")
            {
                return UnityCliPrefabExportService.Execute(request);
            }
            if (action == "create")
            {
                return UnityCliTaskPrefabService.Execute(request);
            }

            if (action == "extract")
            {
                return ExecuteExtract(request);
            }

            if (action == "inspect")
            {
                return UnityCliPrefabInspectService.Execute(request);
            }

            if (action == "visual-import")
            {
                return ExecuteVisualImport(request);
            }

            if (action == "group")
            {
                return ExecuteGroup(request);
            }

            if (action != "attach")
            {
                throw new InvalidOperationException("prefab requires action attach, group, visual-import, or inspect.");
            }

            var parentPrefabPath = NormalizeAssetPath(request.parentPrefabPath);
            var childPrefabPath = NormalizeAssetPath(request.childPrefabPath);
            RequirePrefabAsset(parentPrefabPath, "--parent-prefab");
            RequirePrefabAsset(childPrefabPath, "--child-prefab");

            var parentPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(parentPrefabPath);
            var childPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(childPrefabPath);
            var parentObjectPath = NormalizeHierarchyPath(request.parentObjectPath);
            var childName = SanitizeName(request.childName);
            if (string.IsNullOrEmpty(childName))
            {
                childName = childPrefabAsset.name;
            }

            var replaceExisting = request.replaceExistingChildSet ? request.replaceExistingChild : true;
            var transformSettings = BuildTransformSettings(request);
            var layerSettings = BuildLayerSettings(request);
            var issues = new List<string>();

            ValidateParentPath(parentPrefabAsset, parentObjectPath, layerSettings, issues);
            var data = BuildData(request, parentPrefabPath, parentObjectPath, childPrefabPath, childName, replaceExisting, transformSettings, layerSettings, issues);
            if (request.dryRun)
            {
                data.message = issues.Count == 0 ? "prefab attach dry-run passed" : "prefab attach dry-run completed with issues";
                return data;
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(string.Join("; ", issues.ToArray()));
            }

            AttachToPrefab(parentPrefabPath, parentObjectPath, childPrefabAsset, childName, replaceExisting, transformSettings, layerSettings);
            data.prefabModified = true;

            if (request.replaceSceneInstance)
            {
                data.sceneInstanceReplaced = AttachToOpenSceneInstance(parentPrefabAsset.name, parentObjectPath, childPrefabAsset, childName, replaceExisting, transformSettings, layerSettings);
            }

            AssetDatabase.SaveAssets();
            data.message = "prefab child attached";
            return data;
        }

        /// <summary>
        /// Imports a prefab as a visual-only prefab and optionally places it in a scene.
        /// </summary>
        /// <param name="request">The bridge request containing the prefab import settings.</param>
        /// <returns>The visual import result data.</returns>
        static PrefabData ExecuteVisualImport(UnityCliBridgeRequest request)
        {
            var sourcePrefabPath = NormalizeAssetPath(request.prefabPath);
            var outputPrefabPath = NormalizeAssetPath(request.assetDstPath);
            RequirePrefabAsset(sourcePrefabPath, "--prefab");
            RequireOutputPrefabPath(outputPrefabPath, "--dst");

            AssetDatabase.Refresh();

            var rootName = SanitizeName(request.objectPath);
            if (string.IsNullOrEmpty(rootName))
            {
                rootName = Path.GetFileNameWithoutExtension(outputPrefabPath);
            }

            var settings = BuildTransformSettings(request);
            var issues = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.saveScenePath))
            {
                ValidateScenePath(NormalizeAssetPath(request.saveScenePath), issues);
            }

            var data = BuildVisualImportData(request, sourcePrefabPath, outputPrefabPath, rootName, settings, issues);
            if (request.dryRun)
            {
                data.message = issues.Count == 0 ? "prefab visual-import dry-run passed" : "prefab visual-import dry-run completed with issues";
                return data;
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(string.Join("; ", issues.ToArray()));
            }

            SaveVisualPrefab(sourcePrefabPath, outputPrefabPath, data);
            data.prefabModified = true;

            if (request.replaceSceneInstance && !string.IsNullOrWhiteSpace(request.saveScenePath))
            {
                data.sceneInstanceReplaced = PlaceVisualPrefabInScene(outputPrefabPath, NormalizeAssetPath(request.saveScenePath), rootName, settings);
            }

            AssetDatabase.SaveAssets();
            data.message = "prefab visual-import completed";
            return data;
        }

        /// <summary>
        /// 在 prefab 指定节点下新建容器节点，并把列出的直接子节点按给定顺序迁入容器。
        /// 容器默认与父节点同矩形的全屏 Stretch，子节点本地变换保持不变，用于插入动画容器等中间层级。
        /// </summary>
        /// <param name="request">命令请求。</param>
        /// <returns>prefab 命令结果数据。</returns>
        static PrefabData ExecuteGroup(UnityCliBridgeRequest request)
        {
            var prefabPath = NormalizeAssetPath(request.prefabPath);
            RequirePrefabAsset(prefabPath, "--prefab");

            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var parentObjectPath = NormalizeHierarchyPath(request.parentObjectPath);
            var groupName = SanitizeName(request.groupName);
            var childNames = ParseNameList(request.groupChildren);
            var rectSettings = BuildGroupRectSettings(request);
            var siblingIndex = request.groupSiblingIndex;
            var issues = new List<string>();

            ValidateGroupRequest(prefabAsset, parentObjectPath, groupName, childNames, rectSettings, siblingIndex, issues);
            var data = BuildGroupData(request, prefabPath, parentObjectPath, groupName, childNames, rectSettings, siblingIndex, issues);
            if (request.dryRun)
            {
                data.message = issues.Count == 0 ? "prefab group dry-run passed" : "prefab group dry-run completed with issues";
                return data;
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(string.Join("; ", issues.ToArray()));
            }

            GroupInPrefab(prefabPath, parentObjectPath, groupName, childNames, rectSettings, siblingIndex);
            data.prefabModified = true;

            if (request.replaceSceneInstance)
            {
                data.sceneInstanceReplaced = GroupOpenSceneInstance(prefabAsset.name, parentObjectPath, groupName, childNames, rectSettings, siblingIndex);
            }

            AssetDatabase.SaveAssets();
            data.message = "prefab group created";
            return data;
        }

        /// <summary>
        /// 校验 group 请求：父路径存在、容器名不冲突、子节点均为直接子节点且不重复、Rect 与兄弟序号参数合法。
        /// </summary>
        /// <param name="prefabAsset">目标 prefab 资产对象。</param>
        /// <param name="parentObjectPath">容器挂靠的 prefab 内部节点路径。</param>
        /// <param name="groupName">新容器节点名。</param>
        /// <param name="childNames">待迁入的子节点名列表。</param>
        /// <param name="rectSettings">容器 RectTransform 参数。</param>
        /// <param name="siblingIndex">容器插入的兄弟序号，-1 表示自动取被迁入子节点的最小序号。</param>
        /// <param name="issues">校验问题输出列表。</param>
        static void ValidateGroupRequest(GameObject prefabAsset, string parentObjectPath, string groupName, List<string> childNames, GroupRectSettings rectSettings, int siblingIndex, List<string> issues)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                issues.Add("--group-name is required.");
                return;
            }

            var parent = ResolveTransformInRoot(prefabAsset.transform, parentObjectPath);
            if (parent == null)
            {
                issues.Add("Parent path was not found in prefab: " + parentObjectPath);
                return;
            }

            if (parent.Find(groupName) != null)
            {
                issues.Add("Group name already exists under parent: " + groupName);
            }

            if (childNames.Count == 0)
            {
                issues.Add("--children requires at least one node name.");
            }

            var seenNames = new HashSet<string>();
            foreach (var childName in childNames)
            {
                if (childName.IndexOf('/') >= 0)
                {
                    issues.Add("Child name must be a direct child without '/': " + childName);
                    continue;
                }

                if (!seenNames.Add(childName))
                {
                    issues.Add("Duplicate child name: " + childName);
                    continue;
                }

                if (parent.Find(childName) == null)
                {
                    issues.Add("Child was not found under parent: " + childName);
                }
            }

            if (!(parent is RectTransform) && (rectSettings.hasAnchor || rectSettings.hasPivot || rectSettings.hasSize))
            {
                issues.Add("Parent is not a RectTransform; --anchor/--pivot/--group-size do not apply.");
            }

            if (siblingIndex < -1 || siblingIndex >= parent.childCount)
            {
                issues.Add("--sibling-index out of range: " + siblingIndex);
            }
        }

        /// <summary>
        /// 在 prefab 资产内执行 group 操作：新建容器、按列表顺序迁入子节点并保存。
        /// </summary>
        /// <param name="prefabPath">目标 prefab 资产路径。</param>
        /// <param name="parentObjectPath">容器挂靠的 prefab 内部节点路径。</param>
        /// <param name="groupName">新容器节点名。</param>
        /// <param name="childNames">待迁入的子节点名列表。</param>
        /// <param name="rectSettings">容器 RectTransform 参数。</param>
        /// <param name="siblingIndex">容器插入的兄弟序号，-1 表示自动。</param>
        static void GroupInPrefab(string prefabPath, string parentObjectPath, string groupName, List<string> childNames, GroupRectSettings rectSettings, int siblingIndex)
        {
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(prefabPath);
                var parent = ResolveTransformInRoot(root.transform, parentObjectPath);
                if (parent == null)
                {
                    throw new InvalidOperationException("Parent path was not found in prefab: " + parentObjectPath);
                }

                ApplyGroup(parent, groupName, childNames, rectSettings, siblingIndex);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
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
        /// 把 group 操作同步到当前打开场景中的同名 prefab 实例。
        /// </summary>
        /// <param name="rootName">prefab 根节点名。</param>
        /// <param name="parentObjectPath">容器挂靠的实例内部节点路径。</param>
        /// <param name="groupName">新容器节点名。</param>
        /// <param name="childNames">待迁入的子节点名列表。</param>
        /// <param name="rectSettings">容器 RectTransform 参数。</param>
        /// <param name="siblingIndex">容器插入的兄弟序号，-1 表示自动。</param>
        /// <returns>是否同步到至少一个场景实例。</returns>
        static bool GroupOpenSceneInstance(string rootName, string parentObjectPath, string groupName, List<string> childNames, GroupRectSettings rectSettings, int siblingIndex)
        {
            var roots = FindSceneRoots(rootName);
            foreach (var root in roots)
            {
                var parent = ResolveTransformInRoot(root.transform, parentObjectPath);
                if (parent == null)
                {
                    continue;
                }

                ApplyGroup(parent, groupName, childNames, rectSettings, siblingIndex);
                EditorSceneManager.MarkSceneDirty(parent.gameObject.scene);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 在父节点下新建容器并迁入子节点：UI 父节点创建 RectTransform 容器并应用 Rect 参数，普通父节点创建空 Transform 节点。
        /// </summary>
        /// <param name="parent">容器挂靠的父节点。</param>
        /// <param name="groupName">新容器节点名。</param>
        /// <param name="childNames">待迁入的子节点名列表。</param>
        /// <param name="rectSettings">容器 RectTransform 参数。</param>
        /// <param name="siblingIndex">容器插入的兄弟序号，-1 表示自动取被迁入子节点的最小序号。</param>
        static void ApplyGroup(Transform parent, string groupName, List<string> childNames, GroupRectSettings rectSettings, int siblingIndex)
        {
            var children = new List<Transform>();
            var insertIndex = int.MaxValue;
            foreach (var childName in childNames)
            {
                var child = parent.Find(childName);
                children.Add(child);
                insertIndex = Mathf.Min(insertIndex, child.GetSiblingIndex());
            }

            var parentRect = parent as RectTransform;
            var groupObject = parentRect != null
                ? new GameObject(groupName, typeof(RectTransform))
                : new GameObject(groupName);
            groupObject.layer = parent.gameObject.layer;
            var groupTransform = groupObject.transform;
            groupTransform.SetParent(parent, false);

            if (parentRect != null)
            {
                ApplyGroupRect((RectTransform)groupTransform, rectSettings);
            }
            else if (rectSettings.hasPosition)
            {
                groupTransform.localPosition = rectSettings.position;
            }

            var targetIndex = siblingIndex >= 0 ? siblingIndex : insertIndex;
            groupTransform.SetSiblingIndex(Mathf.Clamp(targetIndex, 0, parent.childCount - 1));

            foreach (var child in children)
            {
                child.SetParent(groupTransform, false);
            }
        }

        /// <summary>
        /// 应用容器 RectTransform 参数；未指定的项使用与父节点同矩形的全屏 Stretch 默认值。
        /// </summary>
        /// <param name="groupRect">新容器的 RectTransform。</param>
        /// <param name="rectSettings">解析后的容器 Rect 参数。</param>
        static void ApplyGroupRect(RectTransform groupRect, GroupRectSettings rectSettings)
        {
            groupRect.anchorMin = rectSettings.hasAnchor ? rectSettings.anchorMin : Vector2.zero;
            groupRect.anchorMax = rectSettings.hasAnchor ? rectSettings.anchorMax : Vector2.one;
            groupRect.pivot = rectSettings.hasPivot ? rectSettings.pivot : new Vector2(0.5f, 0.5f);
            groupRect.anchoredPosition = rectSettings.hasPosition
                ? new Vector2(rectSettings.position.x, rectSettings.position.y)
                : Vector2.zero;
            groupRect.sizeDelta = rectSettings.hasSize ? rectSettings.size : Vector2.zero;
        }

        /// <summary>
        /// 从请求解析容器 RectTransform 参数；锚点 4 个分量、轴心与尺寸各 2 个分量，格式错误直接抛错。
        /// </summary>
        /// <param name="request">命令请求。</param>
        /// <returns>解析后的容器 Rect 参数。</returns>
        static GroupRectSettings BuildGroupRectSettings(UnityCliBridgeRequest request)
        {
            var settings = new GroupRectSettings
            {
                hasPosition = !string.IsNullOrWhiteSpace(request.localPosition),
                position = ParseVector3(request.localPosition, Vector3.zero, "--position")
            };

            if (!string.IsNullOrWhiteSpace(request.groupAnchor))
            {
                var values = ParseFloatList(request.groupAnchor, 4, "--anchor");
                settings.hasAnchor = true;
                settings.anchorMin = new Vector2(values[0], values[1]);
                settings.anchorMax = new Vector2(values[2], values[3]);
            }

            if (!string.IsNullOrWhiteSpace(request.groupPivot))
            {
                var values = ParseFloatList(request.groupPivot, 2, "--pivot");
                settings.hasPivot = true;
                settings.pivot = new Vector2(values[0], values[1]);
            }

            if (!string.IsNullOrWhiteSpace(request.groupSize))
            {
                var values = ParseFloatList(request.groupSize, 2, "--group-size");
                settings.hasSize = true;
                settings.size = new Vector2(values[0], values[1]);
            }

            return settings;
        }

        /// <summary>
        /// 构建 group 命令的结果数据。
        /// </summary>
        /// <param name="request">命令请求。</param>
        /// <param name="prefabPath">目标 prefab 资产路径。</param>
        /// <param name="parentObjectPath">容器挂靠的 prefab 内部节点路径。</param>
        /// <param name="groupName">新容器节点名。</param>
        /// <param name="childNames">待迁入的子节点名列表。</param>
        /// <param name="rectSettings">容器 RectTransform 参数。</param>
        /// <param name="siblingIndex">容器插入的兄弟序号。</param>
        /// <param name="issues">校验问题列表。</param>
        /// <returns>结果数据。</returns>
        static PrefabData BuildGroupData(UnityCliBridgeRequest request, string prefabPath, string parentObjectPath, string groupName, List<string> childNames, GroupRectSettings rectSettings, int siblingIndex, List<string> issues)
        {
            return new PrefabData
            {
                action = "group",
                prefabPath = prefabPath,
                parentObjectPath = parentObjectPath,
                groupName = groupName,
                movedChildren = string.Join(",", childNames.ToArray()),
                movedChildCount = childNames.Count,
                groupRect = FormatGroupRect(rectSettings),
                groupSiblingIndex = siblingIndex,
                dryRun = request.dryRun,
                issues = issues.ToArray(),
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };
        }

        /// <summary>
        /// 把容器 Rect 参数格式化为可读摘要。
        /// </summary>
        /// <param name="rectSettings">容器 Rect 参数。</param>
        /// <returns>形如 anchor=0,0,1,1 pivot=0.5,0.5 size=stretch 的摘要。</returns>
        static string FormatGroupRect(GroupRectSettings rectSettings)
        {
            var anchor = rectSettings.hasAnchor
                ? string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", rectSettings.anchorMin.x, rectSettings.anchorMin.y, rectSettings.anchorMax.x, rectSettings.anchorMax.y)
                : "0,0,1,1";
            var pivot = rectSettings.hasPivot
                ? string.Format(CultureInfo.InvariantCulture, "{0},{1}", rectSettings.pivot.x, rectSettings.pivot.y)
                : "0.5,0.5";
            var size = rectSettings.hasSize
                ? string.Format(CultureInfo.InvariantCulture, "{0},{1}", rectSettings.size.x, rectSettings.size.y)
                : "stretch";
            return "anchor=" + anchor + " pivot=" + pivot + " size=" + size;
        }

        /// <summary>
        /// 解析逗号分隔的节点名列表，忽略空白项。
        /// </summary>
        /// <param name="csv">逗号分隔的节点名。</param>
        /// <returns>去重前的节点名列表。</returns>
        static List<string> ParseNameList(string csv)
        {
            var names = new List<string>();
            if (string.IsNullOrWhiteSpace(csv))
            {
                return names;
            }

            foreach (var part in csv.Split(','))
            {
                var name = SanitizeName(part);
                if (name.Length > 0)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        /// <summary>
        /// 解析固定分量数量的逗号分隔浮点列表，格式错误直接抛错。
        /// </summary>
        /// <param name="text">逗号分隔的数值文本。</param>
        /// <param name="expectedCount">期望的分量数量。</param>
        /// <param name="optionName">报错用的命令行选项名。</param>
        /// <returns>解析后的浮点数组。</returns>
        static float[] ParseFloatList(string text, int expectedCount, string optionName)
        {
            var parts = text.Split(',');
            if (parts.Length != expectedCount)
            {
                throw new InvalidOperationException(optionName + " requires " + expectedCount + " numbers: " + text);
            }

            var values = new float[expectedCount];
            for (var index = 0; index < expectedCount; index++)
            {
                values[index] = ParseFloat(parts[index], optionName);
            }

            return values;
        }

        /// <summary>
        /// Saves a source prefab as a visual-only prefab.
        /// </summary>
        /// <param name="sourcePrefabPath">The source prefab asset path.</param>
        /// <param name="outputPrefabPath">The output prefab asset path.</param>
        /// <param name="data">The result data that receives cleanup counts.</param>
        static void SaveVisualPrefab(string sourcePrefabPath, string outputPrefabPath, PrefabData data)
        {
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(sourcePrefabPath);
                var originalMaterials = BuildRendererMaterialMap(root);
                data.prefabUnpackedCount = UnpackNestedPrefabInstances(root);
                data.missingScriptRemovedCount = RemoveMissingScriptsRecursive(root);
                data.monoBehaviourRemovedCount = RemoveMonoBehavioursRecursive(root);
                data.materialRepairedCount = RepairDefaultMaterials(root, sourcePrefabPath, outputPrefabPath, originalMaterials);
                EnsureAssetFolder(Path.GetDirectoryName(outputPrefabPath)?.Replace('\\', '/') ?? "Assets");

                var saved = PrefabUtility.SaveAsPrefabAsset(root, outputPrefabPath);
                if (saved == null)
                {
                    throw new InvalidOperationException("PrefabUtility.SaveAsPrefabAsset failed: " + outputPrefabPath);
                }

                AssetDatabase.ImportAsset(outputPrefabPath, ImportAssetOptions.ForceUpdate);
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
        /// Places a visual prefab as a root object in the requested scene.
        /// </summary>
        /// <param name="prefabPath">The visual prefab asset path.</param>
        /// <param name="scenePath">The scene asset path.</param>
        /// <param name="rootName">The scene root name.</param>
        /// <param name="settings">The root transform settings.</param>
        /// <returns>True when the scene instance was created.</returns>
        static bool PlaceVisualPrefabInScene(string prefabPath, string scenePath, string rootName, TransformSettings settings)
        {
            var scene = OpenScene(scenePath);
            RemoveSceneRoots(scene, rootName);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("Prefab was not found after save: " + prefabPath);
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException("PrefabUtility.InstantiatePrefab failed: " + prefabPath);
            }

            instance.name = rootName;
            ApplyTransform(instance.transform, settings);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return true;
        }

        /// <summary>
        /// Opens or resolves a scene by asset path.
        /// </summary>
        /// <param name="scenePath">The scene asset path.</param>
        /// <returns>The loaded scene.</returns>
        static Scene OpenScene(string scenePath)
        {
            for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);
                if (scene.isLoaded && string.Equals(scene.path, scenePath, StringComparison.Ordinal))
                {
                    return scene;
                }
            }

            return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        /// <summary>
        /// Removes matching root objects from a scene.
        /// </summary>
        /// <param name="scene">The target scene.</param>
        /// <param name="rootName">The root name to remove.</param>
        static void RemoveSceneRoots(Scene scene, string rootName)
        {
            var roots = scene.GetRootGameObjects();
            for (var index = roots.Length - 1; index >= 0; index--)
            {
                if (roots[index].name == rootName)
                {
                    UnityEngine.Object.DestroyImmediate(roots[index]);
                }
            }
        }

        /// <summary>
        /// Removes missing script components from a prefab hierarchy.
        /// </summary>
        /// <param name="root">The hierarchy root.</param>
        /// <returns>The number of missing script components removed.</returns>
        static int RemoveMissingScriptsRecursive(GameObject root)
        {
            var count = 0;
            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var transform in transforms)
            {
                count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
            }

            return count;
        }

        /// <summary>
        /// Completely unpacks nested prefab instances so the visual prefab owns its renderer data directly.
        /// </summary>
        /// <param name="root">The loaded prefab root.</param>
        /// <returns>The number of unpacked prefab instance roots.</returns>
        static int UnpackNestedPrefabInstances(GameObject root)
        {
            var instanceRoots = new HashSet<GameObject>();
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var gameObject = transform.gameObject;
                var instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
                if (instanceRoot == null)
                {
                    continue;
                }

                instanceRoots.Add(instanceRoot);
            }

            var count = 0;
            foreach (var instanceRoot in instanceRoots)
            {
                if (instanceRoot == null)
                {
                    continue;
                }

                PrefabUtility.UnpackPrefabInstance(instanceRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                count++;
            }

            return count;
        }

        /// <summary>
        /// Removes MonoBehaviour components from a prefab hierarchy.
        /// </summary>
        /// <param name="root">The hierarchy root.</param>
        /// <returns>The number of MonoBehaviour components removed.</returns>
        static int RemoveMonoBehavioursRecursive(GameObject root)
        {
            var count = 0;
            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var transform in transforms)
            {
                var components = transform.GetComponents<Component>();
                for (var index = components.Length - 1; index >= 0; index--)
                {
                    if (components[index] is MonoBehaviour monoBehaviour && !ShouldPreserveMonoBehaviour(monoBehaviour))
                    {
                        UnityEngine.Object.DestroyImmediate(components[index], true);
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Checks whether a MonoBehaviour should be kept during visual prefab import.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour component to inspect.</param>
        /// <returns>True when the component contains level data required by the migrated scene.</returns>
        static bool ShouldPreserveMonoBehaviour(MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null)
            {
                return false;
            }

            var type = monoBehaviour.GetType();
            return string.Equals(type.Name, "LevelConfig", StringComparison.Ordinal);
        }

        /// <summary>
        /// Repairs renderers that still use the package default material by matching imported template prefabs by mesh.
        /// </summary>
        /// <param name="root">The visual prefab root being saved.</param>
        /// <param name="sourcePrefabPath">The source prefab path.</param>
        /// <param name="outputPrefabPath">The output prefab path.</param>
        /// <param name="originalMaterials">The original renderer material map captured before unpacking.</param>
        /// <returns>The number of renderer material arrays repaired.</returns>
        static int RepairDefaultMaterials(GameObject root, string sourcePrefabPath, string outputPrefabPath, Dictionary<string, Material[]> originalMaterials)
        {
            var importedRoot = ResolveImportedRoot(sourcePrefabPath);
            if (string.IsNullOrEmpty(importedRoot))
            {
                return 0;
            }

            var materialMap = BuildMaterialMap(importedRoot, sourcePrefabPath, outputPrefabPath);
            var fallbackMaterials = BuildMaterialAssetMap(importedRoot);
            if ((originalMaterials == null || originalMaterials.Count == 0) && materialMap.Count == 0 && fallbackMaterials.Count == 0)
            {
                return 0;
            }

            var repaired = 0;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!ShouldRepairMaterials(renderer.sharedMaterials))
                {
                    continue;
                }

                var rendererKey = ResolveRendererKey(root.transform, renderer);
                if (!string.IsNullOrEmpty(rendererKey)
                    && originalMaterials != null
                    && originalMaterials.TryGetValue(rendererKey, out var originalRendererMaterials))
                {
                    renderer.sharedMaterials = CloneMaterialArray(originalRendererMaterials);
                    repaired++;
                    continue;
                }

                var mesh = ResolveRendererMesh(renderer);
                var key = ResolveMeshKey(mesh);
                if (!string.IsNullOrEmpty(key) && materialMap.TryGetValue(key, out var materials))
                {
                    renderer.sharedMaterials = CloneMaterialArray(materials);
                    repaired++;
                    continue;
                }

                var fallbackMaterial = ResolveFallbackMaterial(root.transform, renderer, fallbackMaterials);
                if (fallbackMaterial == null)
                {
                    continue;
                }

                renderer.sharedMaterials = BuildSingleMaterialArray(renderer.sharedMaterials, fallbackMaterial);
                repaired++;
            }

            return repaired;
        }

        /// <summary>
        /// Builds a material lookup keyed by renderer hierarchy path before prefab unpacking changes material bindings.
        /// </summary>
        /// <param name="root">The loaded prefab root.</param>
        /// <returns>The renderer material lookup.</returns>
        static Dictionary<string, Material[]> BuildRendererMaterialMap(GameObject root)
        {
            var map = new Dictionary<string, Material[]>(StringComparer.Ordinal);
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0 || ShouldRepairMaterials(materials))
                {
                    continue;
                }

                var key = ResolveRendererKey(root.transform, renderer);
                if (!string.IsNullOrEmpty(key) && !map.ContainsKey(key))
                {
                    map.Add(key, CloneMaterialArray(materials));
                }
            }

            return map;
        }

        /// <summary>
        /// Resolves a stable renderer key from hierarchy path and renderer type.
        /// </summary>
        /// <param name="root">The prefab root transform.</param>
        /// <param name="renderer">The renderer to key.</param>
        /// <returns>The renderer key, or an empty string when it cannot be resolved.</returns>
        static string ResolveRendererKey(Transform root, Renderer renderer)
        {
            if (root == null || renderer == null)
            {
                return string.Empty;
            }

            var path = ResolveTransformPath(root, renderer.transform);
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return path + "|" + renderer.GetType().Name;
        }

        /// <summary>
        /// Resolves a child transform path relative to a root transform.
        /// </summary>
        /// <param name="root">The root transform.</param>
        /// <param name="target">The target transform.</param>
        /// <returns>The relative transform path.</returns>
        static string ResolveTransformPath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return string.Empty;
            }

            if (target == root)
            {
                return root.name;
            }

            var parts = new List<string>();
            var current = target;
            while (current != null)
            {
                parts.Add(current.name);
                if (current == root)
                {
                    break;
                }

                current = current.parent;
            }

            if (current != root)
            {
                return string.Empty;
            }

            parts.Reverse();
            return string.Join("/", parts);
        }

        /// <summary>
        /// Builds a material lookup keyed by mesh from imported template prefabs.
        /// </summary>
        /// <param name="importedRoot">The imported asset root folder.</param>
        /// <param name="sourcePrefabPath">The source prefab path to exclude.</param>
        /// <param name="outputPrefabPath">The output prefab path to exclude.</param>
        /// <returns>The mesh-to-material lookup.</returns>
        static Dictionary<string, Material[]> BuildMaterialMap(string importedRoot, string sourcePrefabPath, string outputPrefabPath)
        {
            var map = new Dictionary<string, Material[]>(StringComparer.Ordinal);
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { importedRoot });
            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(path, sourcePrefabPath, StringComparison.Ordinal)
                    || string.Equals(path, outputPrefabPath, StringComparison.Ordinal))
                {
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                {
                    var mesh = ResolveRendererMesh(renderer);
                    var key = ResolveMeshKey(mesh);
                    var materials = renderer.sharedMaterials;
                    if (string.IsNullOrEmpty(key) || materials == null || materials.Length == 0 || ShouldRepairMaterials(materials))
                    {
                        continue;
                    }

                    if (!map.ContainsKey(key))
                    {
                        map.Add(key, CloneMaterialArray(materials));
                    }
                }
            }

            return map;
        }

        /// <summary>
        /// Builds a material lookup from all material assets under the imported folder.
        /// </summary>
        /// <param name="importedRoot">The imported asset root folder.</param>
        /// <returns>The material lookup keyed by material name and file name.</returns>
        static Dictionary<string, Material> BuildMaterialAssetMap(string importedRoot)
        {
            var map = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            var materialGuids = AssetDatabase.FindAssets("t:Material", new[] { importedRoot });
            foreach (var guid in materialGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    continue;
                }

                AddMaterialAlias(map, material.name, material);
                AddMaterialAlias(map, Path.GetFileNameWithoutExtension(path), material);
            }

            return map;
        }

        /// <summary>
        /// Adds a material alias when the alias is usable.
        /// </summary>
        /// <param name="map">The material lookup to update.</param>
        /// <param name="alias">The material alias.</param>
        /// <param name="material">The material asset.</param>
        static void AddMaterialAlias(Dictionary<string, Material> map, string alias, Material material)
        {
            if (string.IsNullOrWhiteSpace(alias) || material == null || map.ContainsKey(alias))
            {
                return;
            }

            map.Add(alias, material);
        }

        /// <summary>
        /// Resolves a fallback material from renderer hierarchy and mesh asset names.
        /// </summary>
        /// <param name="root">The prefab root transform.</param>
        /// <param name="renderer">The renderer that needs a fallback material.</param>
        /// <param name="materials">The material lookup.</param>
        /// <returns>The resolved fallback material, or null when no rule matches.</returns>
        static Material ResolveFallbackMaterial(Transform root, Renderer renderer, Dictionary<string, Material> materials)
        {
            if (renderer == null || materials == null || materials.Count == 0)
            {
                return null;
            }

            var mesh = ResolveRendererMesh(renderer);
            var meshPath = mesh == null ? string.Empty : AssetDatabase.GetAssetPath(mesh);
            var rendererPath = ResolveTransformPath(root, renderer.transform);
            var text = renderer.name + "|" + rendererPath + "|" + meshPath;

            if (ContainsAny(text, "mianfenji"))
            {
                return FindMaterial(materials, "\u9762\u7C89\u751F\u4EA7\u673A\u5668");
            }

            if (ContainsAny(text, "youzaguo"))
            {
                return FindMaterial(materials, "\u6CB9\u70B8\u9505");
            }

            if (ContainsAny(text, "\u9762\u5305\u70E4\u7089"))
            {
                return FindMaterial(materials, "\u9762\u5305\u70E4\u7089");
            }

            if (ContainsAny(text, "\u70E4\u7BB1"))
            {
                return FindMaterial(materials, "\u70E4\u7BB1");
            }

            if (ContainsAny(text, "jiazi"))
            {
                return FindMaterial(materials, "\u67B6\u5B50");
            }

            if (ContainsAny(text, "\u6728\u5934\u7F6E\u7269\u6846", "changjing03:polySurface14"))
            {
                return FindMaterial(materials, "\u6728\u5934\u7F6E\u7269\u6846");
            }

            if (renderer is ParticleSystemRenderer)
            {
                return ResolveParticleFallbackMaterial(text, materials);
            }

            return null;
        }

        /// <summary>
        /// Resolves a fallback material for imported particle systems.
        /// </summary>
        /// <param name="text">The renderer context text.</param>
        /// <param name="materials">The material lookup.</param>
        /// <returns>The resolved particle material, or null when no rule matches.</returns>
        static Material ResolveParticleFallbackMaterial(string text, Dictionary<string, Material> materials)
        {
            if (ContainsAny(text, "fire"))
            {
                return FindMaterial(materials, "Mat_Fire_01");
            }

            if (ContainsAny(text, "yan", "vfx_ren_smoke", "qipao"))
            {
                return FindMaterial(materials, "Mat_Yan_01");
            }

            if (ContainsAny(text, "glow", "VFX_MianBao", "TianTianQuan", "\u7279\u6548"))
            {
                return FindMaterial(materials, "Mat_Glow_01");
            }

            return FindMaterial(materials, "Mat_Glow_01");
        }

        /// <summary>
        /// Finds a material by name from the lookup.
        /// </summary>
        /// <param name="materials">The material lookup.</param>
        /// <param name="name">The requested material name.</param>
        /// <returns>The material asset, or null when no material exists.</returns>
        static Material FindMaterial(Dictionary<string, Material> materials, string name)
        {
            return !string.IsNullOrEmpty(name) && materials.TryGetValue(name, out var material) ? material : null;
        }

        /// <summary>
        /// Checks whether a text contains any token using ordinal ignore-case matching.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <param name="tokens">The candidate tokens.</param>
        /// <returns>True when at least one token exists in the text.</returns>
        static bool ContainsAny(string text, params string[] tokens)
        {
            if (string.IsNullOrEmpty(text) || tokens == null)
            {
                return false;
            }

            foreach (var token in tokens)
            {
                if (!string.IsNullOrEmpty(token) && text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Builds a material array filled with one fallback material.
        /// </summary>
        /// <param name="currentMaterials">The current renderer material array.</param>
        /// <param name="material">The fallback material.</param>
        /// <returns>The replacement material array.</returns>
        static Material[] BuildSingleMaterialArray(Material[] currentMaterials, Material material)
        {
            var count = currentMaterials == null || currentMaterials.Length == 0 ? 1 : currentMaterials.Length;
            var replacements = new Material[count];
            for (var i = 0; i < replacements.Length; i++)
            {
                replacements[i] = material;
            }

            return replacements;
        }

        /// <summary>
        /// Resolves the mesh rendered by a MeshRenderer or SkinnedMeshRenderer.
        /// </summary>
        /// <param name="renderer">The renderer to inspect.</param>
        /// <returns>The renderer mesh, or null when no mesh is available.</returns>
        static Mesh ResolveRendererMesh(Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                return skinnedMeshRenderer.sharedMesh;
            }

            var meshFilter = renderer.GetComponent<MeshFilter>();
            return meshFilter != null ? meshFilter.sharedMesh : null;
        }

        /// <summary>
        /// Resolves a stable material-repair key for an imported mesh.
        /// </summary>
        /// <param name="mesh">The mesh to inspect.</param>
        /// <returns>The stable mesh key.</returns>
        static string ResolveMeshKey(Mesh mesh)
        {
            return mesh == null || string.IsNullOrWhiteSpace(mesh.name) ? string.Empty : mesh.name.Trim();
        }

        /// <summary>
        /// Checks whether a material array should be repaired from a template prefab.
        /// </summary>
        /// <param name="materials">The material array to inspect.</param>
        /// <returns>True when the array is empty, null, or only uses the default Lit material.</returns>
        static bool ShouldRepairMaterials(Material[] materials)
        {
            if (materials == null || materials.Length == 0)
            {
                return true;
            }

            foreach (var material in materials)
            {
                if (material == null)
                {
                    continue;
                }

                if (!IsDefaultLitMaterial(material))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks whether a material is the package default Lit material.
        /// </summary>
        /// <param name="material">The material to inspect.</param>
        /// <returns>True when the material is the URP default Lit material.</returns>
        static bool IsDefaultLitMaterial(Material material)
        {
            var path = AssetDatabase.GetAssetPath(material);
            return string.Equals(path, "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat", StringComparison.Ordinal)
                || (string.Equals(material.name, "Lit", StringComparison.Ordinal) && path.IndexOf("com.unity.render-pipelines.universal", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// Clones a material array so renderer assignments do not share the array instance.
        /// </summary>
        /// <param name="materials">The source material array.</param>
        /// <returns>The cloned material array.</returns>
        static Material[] CloneMaterialArray(Material[] materials)
        {
            var clone = new Material[materials.Length];
            Array.Copy(materials, clone, materials.Length);
            return clone;
        }

        /// <summary>Extract an authored subtree without changing its source prefab or component references.</summary>
        /// <param name="request">Source prefab, subtree path, new destination and dry-run flag.</param>
        /// <returns>The validated extraction result.</returns>
        private static PrefabData ExecuteExtract(UnityCliBridgeRequest request)
        {
            string sourcePath = NormalizeAssetPath(request.prefabPath);
            string destination = NormalizeAssetPath(request.assetDstPath);
            RequirePrefabAsset(sourcePath, "--prefab");
            if (string.IsNullOrWhiteSpace(destination) || !destination.StartsWith("Assets/", StringComparison.Ordinal)
                || !destination.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                || destination.Contains("..") || File.Exists(destination))
            {
                throw new InvalidOperationException("Extraction requires a new Assets/*.prefab destination.");
            }

            GameObject source = PrefabUtility.LoadPrefabContents(sourcePath);
            GameObject copy = null;
            try
            {
                Transform subtree = ResolveTransformInRoot(source.transform, request.objectPath);
                if (!subtree)
                {
                    throw new InvalidOperationException("Extraction subtree was not found: " + request.objectPath);
                }

                if (!request.dryRun)
                {
                    copy = UnityEngine.Object.Instantiate(subtree.gameObject);
                    copy.name = Path.GetFileNameWithoutExtension(destination);
                    copy.transform.localPosition = subtree.localPosition;
                    copy.transform.localRotation = subtree.localRotation;
                    copy.transform.localScale = subtree.localScale;
                    if (copy.transform is RectTransform copiedRect && subtree is RectTransform sourceRect)
                    {
                        copiedRect.anchoredPosition3D = sourceRect.anchoredPosition3D;
                    }
                    EnsureAssetFolder(Path.GetDirectoryName(destination).Replace('\\', '/'));
                    PrefabUtility.SaveAsPrefabAsset(copy, destination);
                }

                return new PrefabData
                {
                    action = "extract",
                    prefabPath = destination,
                    dryRun = request.dryRun,
                    valid = true,
                    prefabModified = !request.dryRun,
                    message = request.dryRun ? "Subtree extraction validated; source unchanged." : "Subtree extracted; source unchanged."
                };
            }
            finally
            {
                if (copy)
                {
                    UnityEngine.Object.DestroyImmediate(copy);
                }

                PrefabUtility.UnloadPrefabContents(source);
            }
        }

        /// <summary>
        /// Resolves the imported asset root from a copied asset path.
        /// </summary>
        /// <param name="assetPath">The asset path inside the imported folder.</param>
        /// <returns>The imported root folder, or an empty string when it cannot be inferred.</returns>
        static string ResolveImportedRoot(string assetPath)
        {
            var normalized = NormalizeAssetPath(assetPath);
            const string marker = "/Resources/";
            var markerIndex = normalized.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex > 0)
            {
                return normalized.Substring(0, markerIndex);
            }

            var prefabsIndex = normalized.IndexOf("/Prefabs/", StringComparison.Ordinal);
            if (prefabsIndex > 0)
            {
                return normalized.Substring(0, prefabsIndex);
            }

            return string.Empty;
        }

        static void AttachToPrefab(string parentPrefabPath, string parentObjectPath, GameObject childPrefabAsset, string childName, bool replaceExisting, TransformSettings transformSettings, LayerSettings layerSettings)
        {
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(parentPrefabPath);
                var parent = ResolveTransformInRoot(root.transform, parentObjectPath);
                if (parent == null)
                {
                    throw new InvalidOperationException("Parent path was not found in prefab: " + parentObjectPath);
                }

                AttachChild(parent, childPrefabAsset, childName, replaceExisting, transformSettings, layerSettings);
                PrefabUtility.SaveAsPrefabAsset(root, parentPrefabPath);
            }
            finally
            {
                if (root != null)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        static bool AttachToOpenSceneInstance(string parentRootName, string parentObjectPath, GameObject childPrefabAsset, string childName, bool replaceExisting, TransformSettings transformSettings, LayerSettings layerSettings)
        {
            var roots = FindSceneRoots(parentRootName);
            foreach (var root in roots)
            {
                var parent = ResolveTransformInRoot(root.transform, parentObjectPath);
                if (parent == null)
                {
                    continue;
                }

                AttachChild(parent, childPrefabAsset, childName, replaceExisting, transformSettings, layerSettings);
                EditorSceneManager.MarkSceneDirty(parent.gameObject.scene);
                return true;
            }

            return false;
        }

        static void AttachChild(Transform parent, GameObject childPrefabAsset, string childName, bool replaceExisting, TransformSettings transformSettings, LayerSettings layerSettings)
        {
            // Replacing a UI child must preserve its draw and raycast order.
            var previous = parent.Find(childName);
            var siblingIndex = previous ? previous.GetSiblingIndex() : parent.childCount;
            var existingCount = CountDirectChildren(parent, childName);
            if (existingCount > 0)
            {
                if (!replaceExisting)
                {
                    throw new InvalidOperationException("Child already exists under parent: " + childName);
                }

                RemoveDirectChildren(parent, childName);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(childPrefabAsset, parent);
            instance.name = childName;
            instance.transform.SetSiblingIndex(siblingIndex);
            ApplyTransform(instance.transform, transformSettings);
            ApplyLayerRecursive(instance, ResolveLayer(parent.gameObject, layerSettings));
            EditorUtility.SetDirty(instance);
        }

        static void ValidateParentPath(GameObject parentPrefabAsset, string parentObjectPath, LayerSettings layerSettings, List<string> issues)
        {
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(parentPrefabAsset));
                if (ResolveTransformInRoot(root.transform, parentObjectPath) == null)
                {
                    issues.Add("parent path was not found in parent prefab: " + parentObjectPath);
                }

                if (!layerSettings.keepLayer && layerSettings.hasExplicitLayer && ResolveExplicitLayer(layerSettings.explicitLayer, false) < 0)
                {
                    issues.Add("layer was not found: " + layerSettings.explicitLayer);
                }
            }
            finally
            {
                if (root != null)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        static int CountDirectChildren(Transform parent, string childName)
        {
            var count = 0;
            for (var index = 0; index < parent.childCount; index++)
            {
                if (parent.GetChild(index).name == childName)
                {
                    count++;
                }
            }

            return count;
        }

        static void RemoveDirectChildren(Transform parent, string childName)
        {
            for (var index = parent.childCount - 1; index >= 0; index--)
            {
                var child = parent.GetChild(index);
                if (child.name == childName)
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        static Transform ResolveTransformInRoot(Transform root, string path)
        {
            var normalized = NormalizeHierarchyPath(path);
            if (string.IsNullOrEmpty(normalized) || normalized == "." || normalized == root.name)
            {
                return root;
            }

            if (normalized.StartsWith(root.name + "/", StringComparison.Ordinal))
            {
                normalized = normalized.Substring(root.name.Length + 1);
            }

            return root.Find(normalized);
        }

        static List<GameObject> FindSceneRoots(string rootName)
        {
            var matches = new List<GameObject>();
            for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded) continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    FindSceneRootsRecursive(root.transform, rootName, matches);
                }
            }

            return matches;
        }

        static void FindSceneRootsRecursive(Transform current, string rootName, List<GameObject> matches)
        {
            if (current.name == rootName)
            {
                matches.Add(current.gameObject);
            }

            for (var index = 0; index < current.childCount; index++)
            {
                FindSceneRootsRecursive(current.GetChild(index), rootName, matches);
            }
        }

        static TransformSettings BuildTransformSettings(UnityCliBridgeRequest request)
        {
            return new TransformSettings
            {
                hasLocalPosition = !string.IsNullOrWhiteSpace(request.localPosition),
                localPosition = ParseVector3(request.localPosition, Vector3.zero, "--local-position"),
                hasLocalRotation = !string.IsNullOrWhiteSpace(request.localRotation),
                localRotation = ParseVector3(request.localRotation, Vector3.zero, "--local-rotation"),
                hasLocalScale = !string.IsNullOrWhiteSpace(request.localScale),
                localScale = ParseVector3(request.localScale, Vector3.one, "--local-scale")
            };
        }

        static void ApplyTransform(Transform target, TransformSettings settings)
        {
            if (settings.hasLocalPosition)
            {
                target.localPosition = settings.localPosition;
            }

            if (settings.hasLocalRotation)
            {
                target.localRotation = Quaternion.Euler(settings.localRotation);
            }

            if (settings.hasLocalScale)
            {
                target.localScale = settings.localScale;
            }
        }

        static LayerSettings BuildLayerSettings(UnityCliBridgeRequest request)
        {
            return new LayerSettings
            {
                keepLayer = request.keepLayer,
                hasExplicitLayer = !string.IsNullOrWhiteSpace(request.layer),
                explicitLayer = string.IsNullOrWhiteSpace(request.layer) ? string.Empty : request.layer.Trim()
            };
        }

        static int ResolveLayer(GameObject parent, LayerSettings settings)
        {
            if (settings.keepLayer)
            {
                return -1;
            }

            if (settings.hasExplicitLayer)
            {
                return ResolveExplicitLayer(settings.explicitLayer, true);
            }

            return parent.layer;
        }

        static int ResolveExplicitLayer(string layer, bool throwOnMissing)
        {
            if (int.TryParse(layer, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
            {
                if (index >= 0 && index <= 31)
                {
                    return index;
                }

                if (throwOnMissing)
                {
                    throw new InvalidOperationException("--layer index must be between 0 and 31: " + layer);
                }

                return -1;
            }

            var named = LayerMask.NameToLayer(layer);
            if (named >= 0)
            {
                return named;
            }

            if (throwOnMissing)
            {
                throw new InvalidOperationException("Layer was not found: " + layer);
            }

            return -1;
        }

        static void ApplyLayerRecursive(GameObject root, int layer)
        {
            if (layer < 0)
            {
                return;
            }

            root.layer = layer;
            var transform = root.transform;
            for (var index = 0; index < transform.childCount; index++)
            {
                ApplyLayerRecursive(transform.GetChild(index).gameObject, layer);
            }
        }

            static int ReadInt(SerializedObject serializedObject, string propertyName)
            {
                var property = serializedObject.FindProperty(propertyName);
                return property == null ? 0 : property.intValue;
            }

            static float ReadFloat(SerializedObject serializedObject, string propertyName)
            {
                var property = serializedObject.FindProperty(propertyName);
                return property == null ? 0f : property.floatValue;
            }

            static bool ReadBool(SerializedObject serializedObject, string propertyName)
            {
                var property = serializedObject.FindProperty(propertyName);
                return property != null && property.boolValue;
            }

            static Vector3 ReadVector3(SerializedObject serializedObject, string propertyName)
            {
                var property = serializedObject.FindProperty(propertyName);
                return property == null ? Vector3.zero : property.vector3Value;
            }

            static UnityEngine.Object ReadObject(SerializedObject serializedObject, string propertyName)
            {
                var property = serializedObject.FindProperty(propertyName);
                return property == null ? null : property.objectReferenceValue;
            }

            static int ReadLayerMask(SerializedObject serializedObject, string propertyName)
            {
                var property = serializedObject.FindProperty(propertyName);
                var bits = property == null ? null : property.FindPropertyRelative("m_Bits");
                return bits == null ? 0 : bits.intValue;
            }

            static int ReadYamlInt(string line)
            {
                return int.TryParse(ReadYamlValue(line), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
            }

            static float ReadYamlFloat(string line)
            {
                return float.TryParse(ReadYamlValue(line), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0f;
            }

            static bool ReadYamlBool(string line)
            {
                return ReadYamlInt(line) != 0;
            }

            static Vector3 ReadYamlVector3(string line)
            {
                var value = ReadYamlValue(line).Trim().Trim('{', '}');
                var parts = value.Split(',');
                var result = Vector3.zero;
                foreach (var part in parts)
                {
                    var pair = part.Split(':');
                    if (pair.Length != 2)
                    {
                        continue;
                    }

                    var key = pair[0].Trim();
                    var number = float.TryParse(pair[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0f;
                    if (key == "x")
                    {
                        result.x = number;
                    }
                    else if (key == "y")
                    {
                        result.y = number;
                    }
                    else if (key == "z")
                    {
                        result.z = number;
                    }
                }

                return result;
            }

            static string ReadYamlValue(string line)
            {
                var separator = line.IndexOf(':');
                return separator < 0 ? string.Empty : line.Substring(separator + 1).Trim();
            }

            static string FormatYamlBool(bool value)
            {
                return value ? "1" : "0";
            }

            static string FormatYamlVector3(Vector3 value)
            {
                return "{x: " + value.x.ToString("R", CultureInfo.InvariantCulture)
                    + ", y: " + value.y.ToString("R", CultureInfo.InvariantCulture)
                    + ", z: " + value.z.ToString("R", CultureInfo.InvariantCulture)
                    + "}";
            }

            static void WriteInt(SerializedObject serializedObject, string propertyName, int value)
            {
                var property = serializedObject.FindProperty(propertyName);
                if (property != null)
                {
                    property.intValue = value;
                }
            }

            static void WriteFloat(SerializedObject serializedObject, string propertyName, float value)
            {
                var property = serializedObject.FindProperty(propertyName);
                if (property != null)
                {
                    property.floatValue = value;
                }
            }

            static void WriteBool(SerializedObject serializedObject, string propertyName, bool value)
            {
                var property = serializedObject.FindProperty(propertyName);
                if (property != null)
                {
                    property.boolValue = value;
                }
            }

            static void WriteVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
            {
                var property = serializedObject.FindProperty(propertyName);
                if (property != null)
                {
                    property.vector3Value = value;
                }
            }

            static void WriteObject(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
            {
                var property = serializedObject.FindProperty(propertyName);
                if (property != null)
                {
                    property.objectReferenceValue = value;
                }
            }

            static void WriteLayerMask(SerializedObject serializedObject, string propertyName, int value)
            {
                var property = serializedObject.FindProperty(propertyName);
                if (property != null)
                {
                    property.FindPropertyRelative("m_Bits").intValue = value;
                }
            }
        static PrefabData BuildData(UnityCliBridgeRequest request, string parentPrefabPath, string parentObjectPath, string childPrefabPath, string childName, bool replaceExisting, TransformSettings settings, LayerSettings layerSettings, List<string> issues)
        {
            return new PrefabData
            {
                action = "attach",
                parentPrefabPath = parentPrefabPath,
                parentObjectPath = parentObjectPath,
                childPrefabPath = childPrefabPath,
                childName = childName,
                localPosition = settings.hasLocalPosition ? FormatVector3(settings.localPosition) : string.Empty,
                localRotation = settings.hasLocalRotation ? FormatVector3(settings.localRotation) : string.Empty,
                localScale = settings.hasLocalScale ? FormatVector3(settings.localScale) : string.Empty,
                layerName = layerSettings.keepLayer ? string.Empty : (layerSettings.hasExplicitLayer ? layerSettings.explicitLayer : "inherit"),
                layerMode = layerSettings.keepLayer ? "keep" : (layerSettings.hasExplicitLayer ? "explicit" : "inherit"),
                replaceExistingChild = replaceExisting,
                dryRun = request.dryRun,
                issues = issues.ToArray(),
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };
        }

        /// <summary>
        /// Builds the result data for a visual prefab import.
        /// </summary>
        /// <param name="request">The source request.</param>
        /// <param name="sourcePrefabPath">The source prefab asset path.</param>
        /// <param name="outputPrefabPath">The output prefab asset path.</param>
        /// <param name="rootName">The scene root name.</param>
        /// <param name="settings">The transform settings.</param>
        /// <param name="issues">The validation issues.</param>
        /// <returns>The result data.</returns>
        static PrefabData BuildVisualImportData(UnityCliBridgeRequest request, string sourcePrefabPath, string outputPrefabPath, string rootName, TransformSettings settings, List<string> issues)
        {
            return new PrefabData
            {
                action = "visual-import",
                sourcePrefabPath = sourcePrefabPath,
                outputPrefabPath = outputPrefabPath,
                scenePath = NormalizeAssetPath(request.saveScenePath),
                sceneRootName = rootName,
                localPosition = settings.hasLocalPosition ? FormatVector3(settings.localPosition) : string.Empty,
                localRotation = settings.hasLocalRotation ? FormatVector3(settings.localRotation) : string.Empty,
                localScale = settings.hasLocalScale ? FormatVector3(settings.localScale) : string.Empty,
                dryRun = request.dryRun,
                issues = issues.ToArray(),
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };
        }

        static string NormalizeAssetPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            return path.Replace('\\', '/').Trim();
        }

        static string NormalizeHierarchyPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            return path.Replace('\\', '/').Trim().Trim('/');
        }

        static string SanitizeName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
        }

        static void RequirePrefabAsset(string path, string optionName)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException(optionName + " is required.");
            }

            if (!path.StartsWith("Assets/", StringComparison.Ordinal) && !path.StartsWith("Packages/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(optionName + " must be under Assets/ or Packages/: " + path);
            }

            if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(optionName + " must end with .prefab: " + path);
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                throw new InvalidOperationException(optionName + " was not found: " + path);
            }
        }

        /// <summary>
        /// Validates an output prefab asset path.
        /// </summary>
        /// <param name="path">The asset path to validate.</param>
        /// <param name="optionName">The option name used for diagnostics.</param>
        static void RequireOutputPrefabPath(string path, string optionName)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException(optionName + " is required.");
            }

            if (!path.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(optionName + " must be under Assets/: " + path);
            }

            if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(optionName + " must end with .prefab: " + path);
            }
        }

        /// <summary>
        /// Validates a scene asset path and appends issues instead of throwing.
        /// </summary>
        /// <param name="scenePath">The scene path to validate.</param>
        /// <param name="issues">The validation issue list.</param>
        static void ValidateScenePath(string scenePath, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                return;
            }

            if (!scenePath.StartsWith("Assets/", StringComparison.Ordinal) || !scenePath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add("--scene must be an Assets/*.unity path: " + scenePath);
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                issues.Add("--scene was not found: " + scenePath);
            }
        }

        /// <summary>
        /// Ensures an asset folder exists.
        /// </summary>
        /// <param name="folderPath">The asset folder path.</param>
        static void EnsureAssetFolder(string folderPath)
        {
            folderPath = NormalizeAssetPath(folderPath);
            if (string.IsNullOrEmpty(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var index = 1; index < parts.Length; index++)
            {
                var next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        static Vector3 ParseVector3(string text, Vector3 fallback, string optionName)
        {
            if (string.IsNullOrWhiteSpace(text)) return fallback;
            var parts = text.Split(',');
            if (parts.Length != 3)
            {
                throw new InvalidOperationException(optionName + " requires x,y,z.");
            }

            return new Vector3(
                ParseFloat(parts[0], optionName),
                ParseFloat(parts[1], optionName),
                ParseFloat(parts[2], optionName));
        }

        static float ParseFloat(string text, string optionName)
        {
            if (!float.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                throw new InvalidOperationException(optionName + " contains a non-number value: " + text);
            }

            return value;
        }

        static string FormatVector3(Vector3 value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", value.x, value.y, value.z);
        }

        struct TransformSettings
        {
            public bool hasLocalPosition;
            public Vector3 localPosition;
            public bool hasLocalRotation;
            public Vector3 localRotation;
            public bool hasLocalScale;
            public Vector3 localScale;
        }

        struct GroupRectSettings
        {
            public bool hasAnchor;
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public bool hasPivot;
            public Vector2 pivot;
            public bool hasSize;
            public Vector2 size;
            public bool hasPosition;
            public Vector3 position;
        }

        struct LayerSettings
        {
            public bool keepLayer;
            public bool hasExplicitLayer;
            public string explicitLayer;
        }
    }
}
