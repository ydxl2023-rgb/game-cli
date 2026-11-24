using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
#if !USE_OLD_TEXT
using TMPro;
#endif
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Builds Unity UGUI prefabs from the unity-cli visual-ui JSON and asset manifest workflow.
    /// </summary>
    internal static class UnityCliWebVisualUguiPrefabService
    {
        /// <summary>
        /// Default workflow folder searched when only a view name is supplied.
        /// </summary>
        const string WorkflowRoot = "UNITY-CLI";
        /// <summary>
        /// Default project folder for generated prefabs and copied sprite assets.
        /// </summary>
        const string DefaultOutputRoot = "Assets/Resources/UI";

        /// <summary>
        /// 判断视觉矩形是否覆盖完整设计区域时允许的边缘误差。
        /// </summary>
        const float GlobalBackgroundEdgeTolerance = 0.5f;

        /// <summary>
        /// Executes the full visual-ui to UGUI prefab workflow.
        /// </summary>
        /// <param name="request">The bridge request containing UI name, JSON, manifest, and prefab options.</param>
        /// <returns>The generated prefab workflow result.</returns>
        public static UguiData Execute(UnityCliBridgeRequest request)
        {
            if (request == null) throw new InvalidOperationException("Empty request.");

            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var jsonFullPath = ResolveJsonPath(projectRoot, request.uiName, request.jsonFile);
            var visual = LoadJson<WebVisualDocument>(jsonFullPath);
            if (visual == null) throw new InvalidOperationException("Failed to parse visual-ui json: " + jsonFullPath);

            var uiName = FirstNonEmpty(request.uiName, visual.uiName, Path.GetFileNameWithoutExtension(jsonFullPath));
            uiName = SanitizeName(uiName);
            RequireUiName(uiName);

            var manifestFullPath = ResolveManifestPath(projectRoot, jsonFullPath, request.assetManifestPath);
            var manifest = File.Exists(manifestFullPath) ? LoadJson<UguiAssetManifest>(manifestFullPath) : null;
            var prefabPath = ResolvePrefabPath(uiName, request.prefabPath);
            var defaultAssetsFolder = DefaultOutputRoot + "/" + uiName;
            var assetsFolder = ResolveAssetsFolder(manifest, defaultAssetsFolder);

            var issues = ValidateInputs(visual, manifest, jsonFullPath, manifestFullPath, projectRoot, prefabPath);
            var nodeCount = CountNodes(visual.nodes);
            if (request.dryRun)
            {
                return BuildData(uiName, jsonFullPath, manifestFullPath, prefabPath, assetsFolder, true, false, nodeCount, 0, 0, issues,
                    issues.Count == 0 ? "ugui dry-run passed" : "ugui dry-run found " + issues.Count + " issue(s)");
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException("ugui input validation failed: " + string.Join("; ", issues));
            }

            var stage = "copy manifest assets";
            int copied;
            Dictionary<string, Sprite> spriteMap;
            GameObject prefab;
            bool replaced;
            try
            {
                copied = CopyManifestAssets(manifest, projectRoot, Path.GetDirectoryName(jsonFullPath), defaultAssetsFolder);
                stage = "refresh after asset copy";
                AssetDatabase.Refresh();
                stage = "configure sprite importers";
                ConfigureSpriteImporters(manifest, defaultAssetsFolder);
                stage = "refresh after sprite import";
                AssetDatabase.Refresh();

                stage = "load sprites";
                spriteMap = LoadSprites(manifest, defaultAssetsFolder);
                stage = "build prefab";
                prefab = BuildPrefab(visual, uiName, prefabPath, spriteMap);
                stage = "replace scene instance";
                replaced = request.replaceSceneInstance && prefab != null && ReplaceOpenSceneInstance(prefab, uiName);

                stage = "save generated assets";
                AssetDatabase.SaveAssets();
                stage = "final asset refresh";
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("ugui failed during " + stage + ": " + ex.Message, ex);
            }

            return BuildData(uiName, jsonFullPath, manifestFullPath, prefabPath, assetsFolder, false, replaced, nodeCount, copied, spriteMap.Count, issues,
                "Created prefab " + prefabPath + " from " + MakeProjectRelative(projectRoot, jsonFullPath) + " (" + nodeCount + " node(s), " + copied + " asset(s)).");
        }

        /// <summary>
        /// Builds the serializable result payload returned to the CLI.
        /// </summary>
        /// <returns>The UGUI workflow result data.</returns>
        static UguiData BuildData(string uiName, string jsonFile, string manifestPath, string prefabPath, string assetsFolder,
            bool dryRun, bool replaced, int nodeCount, int copied, int sprites, List<string> issues, string message)
        {
            return new UguiData
            {
                uiName = uiName,
                jsonFile = MakeProjectRelative(Directory.GetParent(Application.dataPath).FullName, jsonFile),
                assetManifestPath = File.Exists(manifestPath) ? MakeProjectRelative(Directory.GetParent(Application.dataPath).FullName, manifestPath) : string.Empty,
                prefabPath = prefabPath,
                assetsFolder = assetsFolder,
                dryRun = dryRun,
                sceneInstanceReplaced = replaced,
                nodeCount = nodeCount,
                assetsCopied = copied,
                spritesReferenced = sprites,
                issues = issues.ToArray(),
                message = message,
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };
        }

        /// <summary>
        /// Resolves the visual-ui JSON path from an explicit file or default workflow location.
        /// </summary>
        /// <param name="projectRoot">The Unity project root directory.</param>
        /// <param name="uiName">The UI name used for workflow path lookup.</param>
        /// <param name="jsonFile">The optional explicit JSON file path.</param>
        /// <returns>The absolute visual-ui JSON file path.</returns>
        static string ResolveJsonPath(string projectRoot, string uiName, string jsonFile)
        {
            if (!string.IsNullOrWhiteSpace(jsonFile))
            {
                return FullPath(projectRoot, jsonFile);
            }

            if (string.IsNullOrWhiteSpace(uiName))
            {
                throw new InvalidOperationException("Provide a UIName such as UILogin or --json <visual-ui.json>.");
            }

            RequireUiName(SanitizeName(uiName));
            var uiDir = Path.Combine(projectRoot, WorkflowRoot, uiName.Trim());
            var recut = Path.Combine(uiDir, uiName.Trim() + ".visual-ui.recut.json");
            if (File.Exists(recut)) return recut;
            var visual = Path.Combine(uiDir, uiName.Trim() + ".visual-ui.json");
            if (File.Exists(visual)) return visual;
            throw new FileNotFoundException("visual-ui json not found. Looked for: " + recut + " and " + visual);
        }

        /// <summary>
        /// Resolves the asset manifest path next to the JSON or from an explicit request path.
        /// </summary>
        /// <param name="projectRoot">The Unity project root directory.</param>
        /// <param name="jsonFullPath">The resolved visual-ui JSON file path.</param>
        /// <param name="manifestPath">The optional explicit asset manifest path.</param>
        /// <returns>The absolute asset manifest path.</returns>
        static string ResolveManifestPath(string projectRoot, string jsonFullPath, string manifestPath)
        {
            if (!string.IsNullOrWhiteSpace(manifestPath))
            {
                return FullPath(projectRoot, manifestPath);
            }

            return Path.Combine(Path.GetDirectoryName(jsonFullPath), "asset-manifest.json");
        }

        /// <summary>
        /// Resolves and validates the destination prefab asset path.
        /// </summary>
        /// <param name="uiName">The generated UI name.</param>
        /// <param name="requested">The optional explicit prefab path.</param>
        /// <returns>The Assets-relative prefab path.</returns>
        static string ResolvePrefabPath(string uiName, string requested)
        {
            var prefabPath = string.IsNullOrWhiteSpace(requested)
                ? DefaultOutputRoot + "/" + uiName + "/" + uiName + ".prefab"
                : requested.Trim().Replace('\\', '/');
            if (!prefabPath.StartsWith("Assets/", StringComparison.Ordinal) || !prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("prefab path must be an Assets/*.prefab path: " + prefabPath);
            }

            return prefabPath;
        }

        static void RequireUiName(string uiName)
        {
            if (string.IsNullOrEmpty(uiName))
            {
                throw new InvalidOperationException("UIName is required.");
            }

            if (!uiName.StartsWith("UI", StringComparison.Ordinal) || uiName.Length <= 2)
            {
                throw new InvalidOperationException("UIName must use the UI<Name> form, for example UILogin.");
            }

            if (uiName.EndsWith("View", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("UIName must not use the xxxView form. Use UI<Name>, for example UILogin.");
            }
        }

        /// <summary>
        /// Validates the visual document, manifest, copied assets, and prefab destination.
        /// </summary>
        /// <param name="visual">The parsed visual-ui document.</param>
        /// <param name="manifest">The parsed asset manifest.</param>
        /// <param name="jsonFullPath">The resolved visual-ui JSON file path.</param>
        /// <param name="manifestFullPath">The resolved asset manifest file path.</param>
        /// <param name="projectRoot">The Unity project root directory.</param>
        /// <param name="prefabPath">The resolved destination prefab path.</param>
        /// <returns>A list of validation issues.</returns>
        static List<string> ValidateInputs(WebVisualDocument visual, UguiAssetManifest manifest, string jsonFullPath, string manifestFullPath, string projectRoot, string prefabPath)
        {
            var issues = new List<string>();
            if (!File.Exists(jsonFullPath)) issues.Add("visual-ui json not found: " + jsonFullPath);
            if (visual.referenceResolution == null || visual.referenceResolution.width <= 0 || visual.referenceResolution.height <= 0)
            {
                issues.Add("visual-ui referenceResolution is required.");
            }
            if (visual.nodes == null || visual.nodes.Length == 0)
            {
                issues.Add("visual-ui nodes are empty.");
            }
            if (manifest == null || manifest.assets == null || manifest.assets.Length == 0)
            {
                issues.Add("asset-manifest.json is required and must contain assets.");
            }
            else
            {
                var manifestDir = Path.GetDirectoryName(manifestFullPath);
                foreach (var asset in manifest.assets)
                {
                    if (asset == null) continue;
                    var file = FirstNonEmpty(asset.repairedAsset, asset.file);
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        issues.Add("asset manifest entry has no file: " + asset.id);
                        continue;
                    }
                    var src = FullPath(manifestDir, file);
                    if (!File.Exists(src)) issues.Add("asset source not found: " + MakeProjectRelative(projectRoot, src));
                }
            }
            if (!prefabPath.StartsWith("Assets/", StringComparison.Ordinal)) issues.Add("prefab path must be under Assets/: " + prefabPath);
            return issues;
        }

        /// <summary>
        /// 返回结果中展示的资源目录；manifest 提供显式路径时优先使用其目录。
        /// </summary>
        /// <param name="manifest">资源清单。</param>
        /// <param name="defaultAssetsFolder">未提供显式路径时使用的默认目录。</param>
        /// <returns>显式资源目录或默认目录。</returns>
        static string ResolveAssetsFolder(UguiAssetManifest manifest, string defaultAssetsFolder)
        {
            if (manifest == null)
            {
                return defaultAssetsFolder;
            }

            foreach (var asset in manifest.assets ?? Array.Empty<UguiAssetEntry>())
            {
                var assetPath = NormalizeAssetPath(asset != null ? asset.path : string.Empty);
                if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
                {
                    continue;
                }

                var assetFolder = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
                if (!string.IsNullOrWhiteSpace(assetFolder))
                {
                    return assetFolder;
                }
            }

            return defaultAssetsFolder;
        }

        /// <summary>
        /// 解析单个资源的 Unity 目标路径，显式路径有效时优先使用，否则回退默认目录。
        /// </summary>
        /// <param name="asset">资源清单项。</param>
        /// <param name="defaultAssetsFolder">未提供显式路径时使用的默认目录。</param>
        /// <returns>Assets 相对路径。</returns>
        static string ResolveManifestAssetPath(UguiAssetEntry asset, string defaultAssetsFolder)
        {
            var assetPath = NormalizeAssetPath(asset.path);
            if (!string.IsNullOrWhiteSpace(assetPath) && assetPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return assetPath;
            }

            var sourcePath = FirstNonEmpty(asset.repairedAsset, asset.file);
            return defaultAssetsFolder.TrimEnd('/') + "/" + Path.GetFileName(sourcePath);
        }

        /// <summary>
        /// Copies manifest assets into the Unity project output folder.
        /// </summary>
        /// <param name="manifest">The parsed asset manifest.</param>
        /// <param name="projectRoot">The Unity project root directory.</param>
        /// <param name="manifestDir">The directory containing the manifest and source assets.</param>
        /// <param name="assetsFolder">The Assets-relative destination folder.</param>
        /// <returns>The number of files copied.</returns>
        static int CopyManifestAssets(UguiAssetManifest manifest, string projectRoot, string manifestDir, string assetsFolder)
        {
            var copied = 0;
            foreach (var asset in manifest.assets ?? Array.Empty<UguiAssetEntry>())
            {
                if (asset == null) continue;
                var sourceRel = FirstNonEmpty(asset.repairedAsset, asset.file);
                if (string.IsNullOrWhiteSpace(sourceRel)) continue;

                var src = FullPath(manifestDir, sourceRel);
                if (!File.Exists(src)) continue;

                var dstAssetPath = ResolveManifestAssetPath(asset, assetsFolder);

                EnsureAssetFolder(Path.GetDirectoryName(dstAssetPath).Replace('\\', '/'));
                var dstFullPath = FullPath(projectRoot, dstAssetPath);
                Directory.CreateDirectory(Path.GetDirectoryName(dstFullPath));
                File.Copy(src, dstFullPath, true);
                copied++;
            }
            return copied;
        }

        /// <summary>
        /// Configures copied PNG files as single Sprite assets and applies manifest borders.
        /// </summary>
        /// <param name="manifest">The parsed asset manifest.</param>
        /// <param name="assetsFolder">The Assets-relative folder containing copied sprites.</param>
        static void ConfigureSpriteImporters(UguiAssetManifest manifest, string assetsFolder)
        {
            foreach (var asset in manifest.assets ?? Array.Empty<UguiAssetEntry>())
            {
                if (asset == null)
                {
                    continue;
                }

                var assetPath = ResolveManifestAssetPath(asset, assetsFolder);
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.spriteBorder = asset.spriteBorder != null && asset.spriteBorder.Length >= 4
                    ? new Vector4(asset.spriteBorder[0], asset.spriteBorder[1], asset.spriteBorder[2], asset.spriteBorder[3])
                    : Vector4.zero;
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// Loads copied sprites and indexes them by node ids, asset ids, paths, and file names.
        /// </summary>
        /// <param name="manifest">The parsed asset manifest.</param>
        /// <param name="assetsFolder">The Assets-relative folder containing copied sprites.</param>
        /// <returns>A sprite lookup table for prefab construction.</returns>
        static Dictionary<string, Sprite> LoadSprites(UguiAssetManifest manifest, string assetsFolder)
        {
            var map = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            foreach (var asset in manifest.assets ?? Array.Empty<UguiAssetEntry>())
            {
                if (asset == null)
                {
                    continue;
                }

                var assetPath = ResolveManifestAssetPath(asset, assetsFolder);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite == null) continue;
                AddSprite(map, asset.id, sprite);
                AddSprite(map, asset.layerPlanId, sprite);
                AddSprite(map, assetPath, sprite);
                AddSprite(map, Path.GetFileName(assetPath), sprite);
                foreach (var usedBy in asset.usedBy ?? Array.Empty<string>()) AddSprite(map, usedBy, sprite);
            }
            return map;
        }

        /// <summary>
        /// Builds and saves the Unity prefab from the visual node tree.
        /// </summary>
        /// <param name="visual">The parsed visual-ui document.</param>
        /// <param name="uiName">The generated prefab root name.</param>
        /// <param name="prefabPath">The Assets-relative prefab path.</param>
        /// <param name="sprites">The sprite lookup table.</param>
        /// <returns>The saved prefab asset.</returns>
        static GameObject BuildPrefab(WebVisualDocument visual, string uiName, string prefabPath, IReadOnlyDictionary<string, Sprite> sprites)
        {
            var stage = "ensure prefab folder";
            GameObject rootGo = null;
            try
            {
                EnsureAssetFolder(Path.GetDirectoryName(prefabPath).Replace('\\', '/'));

                stage = "read reference resolution";
                var width = visual.referenceResolution.width;
                var height = visual.referenceResolution.height;
                stage = "resolve root node";
                var rootNode = visual.nodes.FirstOrDefault(n => n != null && string.Equals(n.id, uiName, StringComparison.OrdinalIgnoreCase))
                               ?? visual.nodes.FirstOrDefault();

                stage = "create root game object";
                rootGo = new GameObject(uiName, typeof(RectTransform));
                stage = "resolve ui layer";
                var uiLayer = ResolveUiLayer();
                rootGo.layer = uiLayer;
                stage = "configure root rect transform";
                var rootRect = rootGo.GetComponent<RectTransform>();
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.pivot = new Vector2(0.5f, 0.5f);
                rootRect.sizeDelta = Vector2.zero;
                rootRect.anchoredPosition = Vector2.zero;

                stage = "resolve build context";
                var ctx = new BuildContext
                {
                    Sprites = sprites,
                    Font = ResolveFont(),
                    UiLayer = uiLayer,
                    AssetsFolder = DefaultOutputRoot + "/" + uiName,
                    NodeObjects = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase)
                };

                stage = "build child nodes";
                var parentRect = new Rect(0f, 0f, width, height);
                if (rootNode != null && rootNode.children != null && rootNode.children.Length > 0)
                {
                    foreach (var child in rootNode.children) BuildNodeWithContext(child, rootRect, ctx, parentRect, true);
                }
                else
                {
                    foreach (var node in visual.nodes ?? Array.Empty<WebVisualNode>()) BuildNodeWithContext(node, rootRect, ctx, parentRect, true);
                }

                stage = "set ui layer recursively";
                SetLayerRecursively(rootGo, uiLayer);
                stage = "save prefab asset";
                var prefab = PrefabUtility.SaveAsPrefabAsset(rootGo, prefabPath);
                UnityEngine.Object.DestroyImmediate(rootGo);
                return prefab;
            }
            catch (Exception ex)
            {
                if (rootGo != null) UnityEngine.Object.DestroyImmediate(rootGo);
                throw new InvalidOperationException("BuildPrefab failed during " + stage + ": " + ex.Message, ex);
            }
        }

        static void BuildNodeWithContext(WebVisualNode node, RectTransform parent, BuildContext ctx, Rect parentVisualRect, bool isRootChild)
        {
            try
            {
                BuildNode(node, parent, ctx, parentVisualRect, isRootChild);
            }
            catch (Exception ex)
            {
                var nodeId = node != null ? FirstNonEmpty(node.id, node.name, node.element, node.webType, "unknown") : "null";
                throw new InvalidOperationException("failed to build visual node '" + nodeId + "': " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Recursively converts one visual node into a UGUI GameObject.
        /// </summary>
        /// <param name="node">The visual node being converted.</param>
        /// <param name="parent">The parent transform that receives the generated child.</param>
        /// <param name="ctx">The active prefab build context.</param>
        /// <param name="parentVisualRect">The parent rectangle in visual document coordinates.</param>
        /// <param name="isRootChild">节点是否为 Prefab 根节点的直接子级。</param>
        static void BuildNode(WebVisualNode node, RectTransform parent, BuildContext ctx, Rect parentVisualRect, bool isRootChild)
        {
            if (node == null || node.rect == null) return;
            var go = new GameObject(SanitizeName(FirstNonEmpty(node.name, node.id, "Node")), typeof(RectTransform));
            go.layer = ctx.UiLayer;
            AddNodeObject(ctx, node.id, go);
            AddNodeObject(ctx, node.name, go);
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            var type = FirstNonEmpty(node.element, node.webType).Trim().ToLowerInvariant();
            var visualRect = node.rect.ToRect();
            if (IsRootLevelGlobalBackground(node, ctx, type, visualRect, parentVisualRect, isRootChild))
            {
                ApplyStretchRect(rect);
            }
            else
            {
                ApplyRect(rect, visualRect, parentVisualRect);
            }

            switch (type)
            {
                case "text":
                case "textelement":
                case "label":
                    SetupText(go, node, ctx, false);
                    break;
                case "button":
                case "buttonelement":
                    SetupImage(go, node, ctx, true);
                    SetupButton(go, node, ctx);
                    break;
                case "inputfield":
                case "inputfieldelement":
                case "input":
                    SetupImage(go, node, ctx, true);
                    SetupInputField(go, node, ctx);
                    break;
                case "dropdown":
                case "dropdownelement":
                    SetupImage(go, node, ctx, true);
                    SetupDropdown(go, node, ctx);
                    break;
                case "toggle":
                case "toggleelement":
                case "checkbox":
                    SetupImage(go, node, ctx, true);
                    SetupToggle(go);
                    break;
                case "scrollview":
                    SetupImage(go, node, ctx, true);
                    go.AddComponent<ScrollRect>();
                    break;
                case "listview":
                    SetupListLayout(go, node);
                    break;
                case "container":
                    break;
                case "panel":
                case "image":
                case "imageelement":
                default:
                    SetupImage(go, node, ctx, true);
                    break;
            }

            foreach (var child in node.children ?? Array.Empty<WebVisualNode>())
            {
                BuildNodeWithContext(child, rect, ctx, visualRect, false);
            }

            if (type == "scrollview")
            {
                ConfigureScrollView(go, node);
            }
        }

        /// <summary>
        /// Converts a visual document rectangle into RectTransform anchors and offsets.
        /// </summary>
        /// <param name="rect">The RectTransform being positioned.</param>
        /// <param name="visual">The node rectangle in visual document coordinates.</param>
        /// <param name="parentVisual">The parent rectangle in visual document coordinates.</param>
        static void ApplyRect(RectTransform rect, Rect visual, Rect parentVisual)
        {
            var localX = visual.x - parentVisual.x;
            var localY = visual.y - parentVisual.y;
            var width = visual.width;
            var height = visual.height;

            var anchor = ChooseAnchor(localX, localY, width, height, parentVisual.width, parentVisual.height);
            var pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(
                localX + width * pivot.x - parentVisual.width * anchor.x,
                -localY - height * (1f - pivot.y) + parentVisual.height * (1f - anchor.y));

            var actualLocalX = rect.anchoredPosition.x + parentVisual.width * anchor.x - width * pivot.x;
            var actualLocalY = -rect.anchoredPosition.y + parentVisual.height * (1f - anchor.y) - height * (1f - pivot.y);
            Debug.Assert(Mathf.Approximately(actualLocalX, localX) && Mathf.Approximately(actualLocalY, localY),
                "RectTransform anchor conversion must preserve the visual rectangle.");
        }

        /// <summary>
        /// 判断节点是否为根层级中覆盖完整设计区域且不使用 Sprite 的全局背景。
        /// </summary>
        /// <param name="node">待转换的视觉节点。</param>
        /// <param name="ctx">当前 Prefab 构建上下文。</param>
        /// <param name="type">规范化后的节点类型。</param>
        /// <param name="visual">节点在视觉文档中的矩形。</param>
        /// <param name="parentVisual">父节点在视觉文档中的矩形。</param>
        /// <param name="isRootChild">节点是否为 Prefab 根节点的直接子级。</param>
        /// <returns>需要使用全局拉伸布局时返回 true。</returns>
        static bool IsRootLevelGlobalBackground(WebVisualNode node, BuildContext ctx, string type, Rect visual, Rect parentVisual, bool isRootChild)
        {
            if (!isRootChild)
            {
                return false;
            }

            if (type != "panel" && type != "image" && type != "imageelement")
            {
                return false;
            }

            if (ResolveSprite(node, ctx) != null)
            {
                return false;
            }

            return Mathf.Abs(visual.xMin - parentVisual.xMin) <= GlobalBackgroundEdgeTolerance
                   && Mathf.Abs(visual.yMin - parentVisual.yMin) <= GlobalBackgroundEdgeTolerance
                   && Mathf.Abs(visual.xMax - parentVisual.xMax) <= GlobalBackgroundEdgeTolerance
                   && Mathf.Abs(visual.yMax - parentVisual.yMax) <= GlobalBackgroundEdgeTolerance;
        }

        /// <summary>
        /// 将 RectTransform 设置为相对父节点四边偏移均为零的全局拉伸布局。
        /// </summary>
        /// <param name="rect">待设置的 RectTransform。</param>
        static void ApplyStretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Infers a stable RectTransform anchor from a node position within its parent.
        /// </summary>
        /// <param name="x">The node x position relative to its parent.</param>
        /// <param name="y">The node y position relative to its parent.</param>
        /// <param name="w">The node width.</param>
        /// <param name="h">The node height.</param>
        /// <param name="parentW">The parent width.</param>
        /// <param name="parentH">The parent height.</param>
        /// <returns>The inferred fixed anchor value.</returns>
        static Vector2 ChooseAnchor(float x, float y, float w, float h, float parentW, float parentH)
        {
            if (parentW <= 0f || parentH <= 0f)
            {
                return new Vector2(0f, 1f);
            }

            var centerX = x + w * 0.5f;
            var centerY = y + h * 0.5f;
            var left = x;
            var right = parentW - (x + w);
            var top = y;
            var bottom = parentH - (y + h);
            float ax = Mathf.Abs(centerX - parentW * 0.5f) <= Math.Max(8f, parentW * 0.03f) ? 0.5f : (right < left * 0.5f ? 1f : 0f);
            float ay = Mathf.Abs(centerY - parentH * 0.5f) <= Math.Max(8f, parentH * 0.03f) ? 0.5f : (bottom < top * 0.5f ? 0f : 1f);
            return new Vector2(ax, ay);
        }

        /// <summary>
        /// Adds or updates the Image component for a generated node.
        /// </summary>
        /// <param name="go">The GameObject being configured.</param>
        /// <param name="node">The visual node being converted.</param>
        /// <param name="ctx">The active prefab build context.</param>
        /// <param name="raycast">Whether the generated Image should receive raycasts.</param>
        static void SetupImage(GameObject go, WebVisualNode node, BuildContext ctx, bool raycast)
        {
            var image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
            image.raycastTarget = raycast;
            image.color = ParseColor(FirstNonEmpty(node.style != null ? node.style.color : null, "#FFFFFF"), node.style != null ? node.style.alpha * node.style.opacity : 1f);
            image.sprite = ResolveSprite(node, ctx);
            image.type = ResolveImageType(node, image.sprite);
            if (image.type == Image.Type.Filled)
            {
                image.fillMethod = Image.FillMethod.Horizontal;
                image.fillOrigin = (int)Image.OriginHorizontal.Left;
                image.fillAmount = 1f;
            }
        }

        /// <summary>
        /// Adds or updates a text component from visual text metadata.
        /// </summary>
        /// <param name="go">The GameObject being configured.</param>
        /// <param name="node">The visual node being converted.</param>
        /// <param name="ctx">The active prefab build context.</param>
        /// <param name="raycast">Whether the generated text should receive raycasts.</param>
        static void SetupText(GameObject go, WebVisualNode node, BuildContext ctx, bool raycast)
        {
#if USE_OLD_TEXT
            var text = go.GetComponent<Text>() ?? go.AddComponent<Text>();
            text.raycastTarget = raycast;
            text.font = ctx.Font;
            text.text = node.text != null ? node.text.content ?? string.Empty : string.Empty;
            text.fontSize = Mathf.RoundToInt(node.text != null && node.text.fontSize > 0 ? node.text.fontSize : 18);
            text.color = ParseColor(node.text != null ? node.text.color : null, 1f);
            text.alignment = ResolveAlignment(node.text != null ? node.text.alignment : null);
            text.fontStyle = IsBold(node.text != null ? node.text.fontWeight : null) ? FontStyle.Bold : FontStyle.Normal;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
#else
            var text = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
            text.raycastTarget = raycast;
            text.font = ctx.Font;
            text.text = node.text != null ? node.text.content ?? string.Empty : string.Empty;
            text.fontSize = node.text != null && node.text.fontSize > 0 ? node.text.fontSize : 18;
            text.color = ParseColor(node.text != null ? node.text.color : null, 1f);
            text.alignment = ResolveAlignment(node.text != null ? node.text.alignment : null);
            text.fontStyle = IsBold(node.text != null ? node.text.fontWeight : null) ? FontStyles.Bold : FontStyles.Normal;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
#endif
        }

        /// <summary>
        /// Adds a Button component and optional label text to a generated node.
        /// </summary>
        /// <param name="go">The GameObject being configured.</param>
        /// <param name="node">The visual node being converted.</param>
        /// <param name="ctx">The active prefab build context.</param>
        static void SetupButton(GameObject go, WebVisualNode node, BuildContext ctx)
        {
            var button = go.GetComponent<Button>() ?? go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Graphic>();
            var label = node.text != null ? node.text.content : string.Empty;
            if (!string.IsNullOrWhiteSpace(label))
            {
#if USE_OLD_TEXT
                CreateTextChild(go.transform, "Label", label, node.text.fontSize > 0 ? node.text.fontSize : 22, TextAnchor.MiddleCenter, ctx);
#else
                CreateTextChild(go.transform, "Label", label, node.text.fontSize > 0 ? node.text.fontSize : 22, TextAlignmentOptions.Center, ctx);
#endif
            }
        }

        static void AddNodeObject(BuildContext ctx, string key, GameObject go)
        {
            if (ctx.NodeObjects == null || string.IsNullOrWhiteSpace(key) || go == null) return;
            if (!ctx.NodeObjects.ContainsKey(key)) ctx.NodeObjects.Add(key, go);
        }

        /// <summary>
        /// Creates a native input field hierarchy for a visual input node.
        /// </summary>
        /// <param name="go">The GameObject being configured.</param>
        /// <param name="node">The visual node being converted.</param>
        /// <param name="ctx">The active prefab build context.</param>
        static void SetupInputField(GameObject go, WebVisualNode node, BuildContext ctx)
        {
#if USE_OLD_TEXT
            var input = go.GetComponent<InputField>() ?? go.AddComponent<InputField>();
            input.targetGraphic = go.GetComponent<Graphic>();
            input.lineType = InputField.LineType.SingleLine;
            if (IsPassword(node))
            {
                input.contentType = InputField.ContentType.Password;
                input.inputType = InputField.InputType.Password;
            }

            var area = CreateStretchChild(go.transform, "Text Area", new Vector2(12, 8), new Vector2(-12, -8));
            area.gameObject.AddComponent<RectMask2D>();
            var placeholder = CreateTextChild(area, "Placeholder", node.text != null ? node.text.content : string.Empty, node.text != null && node.text.fontSize > 0 ? node.text.fontSize : 18, TextAnchor.MiddleLeft, ctx);
            placeholder.color = new Color(0.65f, 0.75f, 0.85f, 0.7f);
            var text = CreateTextChild(area, "Text", string.Empty, node.text != null && node.text.fontSize > 0 ? node.text.fontSize : 18, TextAnchor.MiddleLeft, ctx);
            input.textComponent = text;
            input.placeholder = placeholder;
#else
            var input = go.GetComponent<TMP_InputField>() ?? go.AddComponent<TMP_InputField>();
            input.targetGraphic = go.GetComponent<Graphic>();
            input.lineType = TMP_InputField.LineType.SingleLine;
            if (IsPassword(node))
            {
                input.contentType = TMP_InputField.ContentType.Password;
                input.inputType = TMP_InputField.InputType.Password;
            }

            var area = CreateStretchChild(go.transform, "Text Area", new Vector2(12, 8), new Vector2(-12, -8));
            area.gameObject.AddComponent<RectMask2D>();
            var placeholder = CreateTextChild(area, "Placeholder", node.text != null ? node.text.content : string.Empty, node.text != null && node.text.fontSize > 0 ? node.text.fontSize : 18, TextAlignmentOptions.Left, ctx);
            placeholder.color = new Color(0.65f, 0.75f, 0.85f, 0.7f);
            var text = CreateTextChild(area, "Text", string.Empty, node.text != null && node.text.fontSize > 0 ? node.text.fontSize : 18, TextAlignmentOptions.Left, ctx);
            input.textViewport = area;
            input.placeholder = placeholder;
            input.textComponent = text;
#endif
        }

        /// <summary>
        /// Creates a native dropdown hierarchy for a visual dropdown node.
        /// </summary>
        /// <param name="go">The GameObject being configured.</param>
        /// <param name="node">The visual node being converted.</param>
        /// <param name="ctx">The active prefab build context.</param>
        static void SetupDropdown(GameObject go, WebVisualNode node, BuildContext ctx)
        {
#if USE_OLD_TEXT
            var dropdown = go.GetComponent<Dropdown>() ?? go.AddComponent<Dropdown>();
            dropdown.targetGraphic = go.GetComponent<Graphic>();
            var options = SplitOptions(node.dropdown != null ? node.dropdown.options : string.Empty);
            dropdown.options.Clear();
            foreach (var option in options) dropdown.options.Add(new Dropdown.OptionData(option));
            dropdown.value = node.dropdown != null ? Math.Max(0, node.dropdown.value) : 0;

            var label = CreateStretchChild(go.transform, "Label", new Vector2(18, 6), new Vector2(-42, -6));
            var labelText = label.gameObject.AddComponent<Text>();
            labelText.font = ctx.Font;
            labelText.text = options.Length > 0 ? options[Math.Min(dropdown.value, options.Length - 1)] : (node.text != null ? node.text.content : string.Empty);
            labelText.fontSize = Mathf.RoundToInt(node.text != null && node.text.fontSize > 0 ? node.text.fontSize : 18);
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.color = ParseColor(node.text != null ? node.text.color : null, 1f);
            labelText.raycastTarget = false;
            dropdown.captionText = labelText;
            CreateDropdownTemplate(go.transform, dropdown, ctx);
#else
            var dropdown = go.GetComponent<TMP_Dropdown>() ?? go.AddComponent<TMP_Dropdown>();
            dropdown.targetGraphic = go.GetComponent<Graphic>();
            var options = SplitOptions(node.dropdown != null ? node.dropdown.options : string.Empty);
            dropdown.options.Clear();
            foreach (var option in options) dropdown.options.Add(new TMP_Dropdown.OptionData(option));
            dropdown.value = node.dropdown != null ? Math.Max(0, node.dropdown.value) : 0;

            var label = CreateStretchChild(go.transform, "Label", new Vector2(18, 6), new Vector2(-42, -6));
            var labelText = label.gameObject.AddComponent<TextMeshProUGUI>();
            labelText.font = ctx.Font;
            labelText.text = options.Length > 0 ? options[Math.Min(dropdown.value, options.Length - 1)] : (node.text != null ? node.text.content : string.Empty);
            labelText.fontSize = node.text != null && node.text.fontSize > 0 ? node.text.fontSize : 18;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            labelText.color = ParseColor(node.text != null ? node.text.color : null, 1f);
            labelText.raycastTarget = false;
            dropdown.captionText = labelText;
            CreateDropdownTemplate(go.transform, dropdown, ctx);
#endif
        }

#if USE_OLD_TEXT
        /// <summary>
        /// Creates the hidden Dropdown template required by Unity.
        /// </summary>
        /// <param name="parent">The parent transform that receives the generated child.</param>
        /// <param name="dropdown">The dropdown component being configured.</param>
        /// <param name="ctx">The active prefab build context.</param>
        static void CreateDropdownTemplate(Transform parent, Dropdown dropdown, BuildContext ctx)
        {
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.layer = ctx.UiLayer;
            var tr = template.GetComponent<RectTransform>();
            tr.SetParent(parent, false);
            tr.anchorMin = new Vector2(0, 0);
            tr.anchorMax = new Vector2(1, 0);
            tr.pivot = new Vector2(0.5f, 1);
            tr.anchoredPosition = new Vector2(0, -2);
            tr.sizeDelta = new Vector2(0, 150);
            template.SetActive(false);
            template.GetComponent<Image>().color = new Color(1, 1, 1, 0.96f);

            var viewport = CreateStretchChild(template.transform, "Viewport", new Vector2(6, 6), new Vector2(-6, -6));
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = CreateStretchChild(viewport, "Content", Vector2.zero, Vector2.zero);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = new Vector2(0, 96);

            var item = CreateStretchChild(content, "Item", Vector2.zero, Vector2.zero);
            item.anchorMin = new Vector2(0, 1);
            item.anchorMax = new Vector2(1, 1);
            item.pivot = new Vector2(0.5f, 1);
            item.sizeDelta = new Vector2(0, 32);
            item.gameObject.AddComponent<Toggle>();
            item.gameObject.AddComponent<Image>().color = new Color(0.86f, 0.96f, 1f, 0.72f);
            var itemLabel = CreateTextChild(item, "Item Label", "Option", 18, TextAnchor.MiddleLeft, ctx);

            var scroll = template.GetComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            dropdown.template = tr;
            dropdown.itemText = itemLabel;
        }
#else
        /// <summary>
        /// Creates the hidden TMP_Dropdown template required by Unity.
        /// </summary>
        /// <param name="parent">The parent transform that receives the generated child.</param>
        /// <param name="dropdown">The dropdown component being configured.</param>
        /// <param name="ctx">The active prefab build context.</param>
        static void CreateDropdownTemplate(Transform parent, TMP_Dropdown dropdown, BuildContext ctx)
        {
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.layer = ctx.UiLayer;
            var tr = template.GetComponent<RectTransform>();
            tr.SetParent(parent, false);
            tr.anchorMin = new Vector2(0, 0);
            tr.anchorMax = new Vector2(1, 0);
            tr.pivot = new Vector2(0.5f, 1);
            tr.anchoredPosition = new Vector2(0, -2);
            tr.sizeDelta = new Vector2(0, 150);
            template.SetActive(false);
            template.GetComponent<Image>().color = new Color(1, 1, 1, 0.96f);

            var viewport = CreateStretchChild(template.transform, "Viewport", new Vector2(6, 6), new Vector2(-6, -6));
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = CreateStretchChild(viewport, "Content", Vector2.zero, Vector2.zero);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = new Vector2(0, 96);

            var item = CreateStretchChild(content, "Item", Vector2.zero, Vector2.zero);
            item.anchorMin = new Vector2(0, 1);
            item.anchorMax = new Vector2(1, 1);
            item.pivot = new Vector2(0.5f, 1);
            item.sizeDelta = new Vector2(0, 32);
            item.gameObject.AddComponent<Toggle>();
            item.gameObject.AddComponent<Image>().color = new Color(0.86f, 0.96f, 1f, 0.72f);
            var itemLabel = CreateTextChild(item, "Item Label", "Option", 18, TextAlignmentOptions.MidlineLeft, ctx);

            var scroll = template.GetComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            dropdown.template = tr;
            dropdown.itemText = itemLabel;
        }
#endif

        /// <summary>
        /// Adds a Toggle component to a generated checkbox node.
        /// </summary>
        /// <param name="go">The GameObject being configured.</param>
        static void SetupToggle(GameObject go)
        {
            var toggle = go.GetComponent<Toggle>() ?? go.AddComponent<Toggle>();
            toggle.targetGraphic = go.GetComponent<Graphic>();
            toggle.graphic = go.GetComponent<Graphic>();
            toggle.isOn = true;
        }

        /// <summary>
        /// Sets up the list layout component described by the web visual node.
        /// </summary>
        /// <param name="go">The list container game object.</param>
        /// <param name="node">The source web visual node.</param>
        static void SetupListLayout(GameObject go, WebVisualNode node)
        {
            var list = node.list;
            var layout = FirstNonEmpty(list != null ? list.layout : null, string.Empty).ToLowerInvariant();
            if (layout != "grid" && layout != "vertical" && layout != "horizontal" && list != null && list.cellWidth <= 0 && list.cellHeight <= 0)
            {
                return;
            }

            if (layout == "vertical")
            {
                RemoveLayoutGroup<GridLayoutGroup>(go);
                RemoveLayoutGroup<HorizontalLayoutGroup>(go);
                var vertical = go.GetComponent<VerticalLayoutGroup>() ?? go.AddComponent<VerticalLayoutGroup>();
                vertical.spacing = list != null ? list.spacingY : 0f;
                vertical.padding = new RectOffset(
                    Mathf.RoundToInt(list != null ? list.paddingLeft : 0f),
                    Mathf.RoundToInt(list != null ? list.paddingRight : 0f),
                    Mathf.RoundToInt(list != null ? list.paddingTop : 0f),
                    Mathf.RoundToInt(list != null ? list.paddingBottom : 0f));
                vertical.childAlignment = TextAnchor.UpperLeft;
                vertical.childControlWidth = false;
                vertical.childControlHeight = false;
                vertical.childForceExpandWidth = false;
                vertical.childForceExpandHeight = false;

                var verticalFitter = go.GetComponent<ContentSizeFitter>() ?? go.AddComponent<ContentSizeFitter>();
                verticalFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                verticalFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                return;
            }

            if (layout == "horizontal")
            {
                RemoveLayoutGroup<GridLayoutGroup>(go);
                RemoveLayoutGroup<VerticalLayoutGroup>(go);
                var horizontal = go.GetComponent<HorizontalLayoutGroup>() ?? go.AddComponent<HorizontalLayoutGroup>();
                horizontal.spacing = list != null ? list.spacingX : 0f;
                horizontal.padding = new RectOffset(
                    Mathf.RoundToInt(list != null ? list.paddingLeft : 0f),
                    Mathf.RoundToInt(list != null ? list.paddingRight : 0f),
                    Mathf.RoundToInt(list != null ? list.paddingTop : 0f),
                    Mathf.RoundToInt(list != null ? list.paddingBottom : 0f));
                horizontal.childAlignment = TextAnchor.UpperLeft;
                horizontal.childControlWidth = false;
                horizontal.childControlHeight = false;
                horizontal.childForceExpandWidth = false;
                horizontal.childForceExpandHeight = false;

                var horizontalFitter = go.GetComponent<ContentSizeFitter>() ?? go.AddComponent<ContentSizeFitter>();
                horizontalFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                horizontalFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                return;
            }

            RemoveLayoutGroup<HorizontalLayoutGroup>(go);
            RemoveLayoutGroup<VerticalLayoutGroup>(go);
            var grid = go.GetComponent<GridLayoutGroup>() ?? go.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(
                list != null && list.cellWidth > 0 ? list.cellWidth : 96f,
                list != null && list.cellHeight > 0 ? list.cellHeight : 96f);
            grid.spacing = new Vector2(
                list != null ? list.spacingX : 0f,
                list != null ? list.spacingY : 0f);
            grid.padding = new RectOffset(
                Mathf.RoundToInt(list != null ? list.paddingLeft : 0f),
                Mathf.RoundToInt(list != null ? list.paddingRight : 0f),
                Mathf.RoundToInt(list != null ? list.paddingTop : 0f),
                Mathf.RoundToInt(list != null ? list.paddingBottom : 0f));

            if (list != null && list.gridCount > 0)
            {
                var constraint = FirstNonEmpty(list.gridConstraint, "fixedColumnCount").ToLowerInvariant();
                grid.constraint = constraint == "fixedrowcount"
                    ? GridLayoutGroup.Constraint.FixedRowCount
                    : GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = list.gridCount;
            }

            var fitter = go.GetComponent<ContentSizeFitter>() ?? go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        static void RemoveLayoutGroup<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            if (component == null) return;
            UnityEngine.Object.DestroyImmediate(component);
        }

        /// <summary>
        /// Configures a ScrollRect by binding its viewport and content children.
        /// </summary>
        /// <param name="go">The scroll view game object.</param>
        /// <param name="node">The source web visual node.</param>
        static void ConfigureScrollView(GameObject go, WebVisualNode node)
        {
            var scroll = go.GetComponent<ScrollRect>();
            if (scroll == null) return;

            var viewport = FindChildRect(go.transform, "Viewport")
                           ?? (go.transform.childCount > 0 ? go.transform.GetChild(0) as RectTransform : null);
            if (viewport != null)
            {
                scroll.viewport = viewport;
                if (viewport.GetComponent<RectMask2D>() == null)
                {
                    viewport.gameObject.AddComponent<RectMask2D>();
                }
            }

            var content = viewport != null
                ? FindChildRect(viewport, "Content") ?? FindChildRect(viewport, "ItemGrid")
                : null;
            if (content == null)
            {
                content = FindChildRect(go.transform, "Content") ?? FindChildRect(go.transform, "ItemGrid");
            }

            if (content != null)
            {
                content.anchorMin = new Vector2(0f, 1f);
                content.anchorMax = new Vector2(0f, 1f);
                content.pivot = new Vector2(0f, 1f);
                content.anchoredPosition = Vector2.zero;
                scroll.content = content;
            }

            var direction = FirstNonEmpty(node.list != null ? node.list.scrollDirection : null, "vertical").ToLowerInvariant();
            scroll.horizontal = direction == "horizontal" || direction == "both";
            scroll.vertical = direction != "horizontal";
        }

        /// <summary>
        /// Finds a child RectTransform by name under the given parent.
        /// </summary>
        /// <param name="parent">The transform to search.</param>
        /// <param name="name">The child name to find.</param>
        /// <returns>The matching child RectTransform, or null when not found.</returns>
        static RectTransform FindChildRect(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (string.Equals(child.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return child as RectTransform;
                }
            }

            foreach (Transform child in parent)
            {
                var nested = FindChildRect(child, name);
                if (nested != null) return nested;
            }

            return null;
        }

        /// <summary>
        /// Creates a stretched text child under a generated UI node.
        /// </summary>
        /// <param name="parent">The parent transform that receives the generated child.</param>
        /// <param name="name">The generated child object name.</param>
        /// <param name="value">The initial text content.</param>
        /// <param name="size">The font size.</param>
        /// <param name="alignment">The text alignment.</param>
        /// <param name="ctx">The active prefab build context.</param>
        /// <returns>The generated text component.</returns>
#if USE_OLD_TEXT
        static Text CreateTextChild(Transform parent, string name, string value, float size, TextAnchor alignment, BuildContext ctx)
        {
            var rect = CreateStretchChild(parent, name, Vector2.zero, Vector2.zero);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = ctx.Font;
            text.text = value ?? string.Empty;
            text.fontSize = Mathf.RoundToInt(size);
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }
#else
        static TextMeshProUGUI CreateTextChild(Transform parent, string name, string value, float size, TextAlignmentOptions alignment, BuildContext ctx)
        {
            var rect = CreateStretchChild(parent, name, Vector2.zero, Vector2.zero);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = ctx.Font;
            text.text = value ?? string.Empty;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }
#endif

        /// <summary>
        /// Creates a RectTransform child that stretches to its parent with explicit offsets.
        /// </summary>
        /// <param name="parent">The parent transform that receives the generated child.</param>
        /// <param name="name">The generated child object name.</param>
        /// <param name="offsetMin">The lower-left stretch offset.</param>
        /// <param name="offsetMax">The upper-right stretch offset.</param>
        /// <returns>The generated RectTransform.</returns>
        static RectTransform CreateStretchChild(Transform parent, string name, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent == null ? ResolveUiLayer() : parent.gameObject.layer;
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        /// <summary>
        /// Resolves Unity's built-in UI layer id.
        /// </summary>
        /// <returns>The UI layer id.</returns>
        static int ResolveUiLayer()
        {
            var layer = LayerMask.NameToLayer("UI");
            return layer >= 0 ? layer : 5;
        }

        /// <summary>
        /// Applies the selected layer to every object in the generated prefab hierarchy.
        /// </summary>
        /// <param name="root">The hierarchy root.</param>
        /// <param name="layer">The layer id to apply.</param>
        static void SetLayerRecursively(GameObject root, int layer)
        {
            if (root == null) return;
            root.layer = layer;
            var transform = root.transform;
            for (var index = 0; index < transform.childCount; index++)
            {
                SetLayerRecursively(transform.GetChild(index).gameObject, layer);
            }
        }

        /// <summary>
        /// Resolves the sprite referenced by a visual node using ids, file names, or asset paths.
        /// </summary>
        /// <param name="node">The visual node being converted.</param>
        /// <param name="ctx">The active prefab build context.</param>
        /// <returns>The resolved sprite, or null when no matching asset exists.</returns>
        static Sprite ResolveSprite(WebVisualNode node, BuildContext ctx)
        {
            Sprite sprite;
            if (ctx.Sprites.TryGetValue(node.id ?? string.Empty, out sprite)) return sprite;
            var styleSprite = node.style != null ? node.style.sprite : string.Empty;
            if (!string.IsNullOrWhiteSpace(styleSprite))
            {
                if (ctx.Sprites.TryGetValue(styleSprite, out sprite)) return sprite;
                if (ctx.Sprites.TryGetValue(Path.GetFileName(styleSprite), out sprite)) return sprite;
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(NormalizeAssetPath(styleSprite));
                if (sprite != null) return sprite;
            }
            return null;
        }

        /// <summary>
        /// Resolves the font used by generated native text controls.
        /// </summary>
        /// <returns>The project-specific font, or Unity's default font.</returns>
#if USE_OLD_TEXT
        static Font ResolveFont()
        {
            var guids = AssetDatabase.FindAssets("UnityCLI_Chinese t:Font");
            foreach (var guid in guids)
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(guid));
                if (font != null) return font;
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
#else
        static TMP_FontAsset ResolveFont()
        {
            var guids = AssetDatabase.FindAssets("UnityCLI_Chinese_TMP t:TMP_FontAsset");
            foreach (var guid in guids)
            {
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (font != null) return font;
            }
            try
            {
                return TMP_Settings.defaultFontAsset;
            }
            catch (Exception)
            {
                return null;
            }
        }
#endif

        /// <summary>
        /// Replaces the matching open-scene instance with a fresh instance of the generated prefab.
        /// </summary>
        /// <param name="prefab">The prefab asset to instantiate into the open scene.</param>
        /// <param name="rootName">The scene child name to replace under the Canvas.</param>
        /// <returns>True when a scene instance was replaced or created.</returns>
        static bool ReplaceOpenSceneInstance(GameObject prefab, string rootName)
        {
            var canvas = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(c => c != null && c.name == "Canvas")
                ?? UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
            if (canvas == null) return false;
            var existing = FindDeepChild(canvas.transform, rootName);
            var parent = existing != null
                ? existing.parent
                : FirstNonEmptyTransform(canvas.transform.Find("center"), canvas.transform);
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
            var instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null) return false;
            instance.name = rootName;
            var rect = instance.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.SetAsLastSibling();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            EditorUtility.SetDirty(canvas.gameObject);
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            return true;
        }

        static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null || string.IsNullOrWhiteSpace(childName)) return null;
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform != null && transform.name == childName) return transform;
            }
            return null;
        }

        static Transform FirstNonEmptyTransform(params Transform[] values)
        {
            foreach (var value in values)
            {
                if (value != null) return value;
            }
            return null;
        }

        /// <summary>
        /// Ensures that a Unity asset folder and all missing parent folders exist.
        /// </summary>
        /// <param name="assetFolder">The Assets-relative folder path to create.</param>
        static void EnsureAssetFolder(string assetFolder)
        {
            assetFolder = NormalizeAssetPath(assetFolder);
            if (AssetDatabase.IsValidFolder(assetFolder)) return;
            var parts = assetFolder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        /// <summary>
        /// Resolves a path against a root directory when it is not already absolute.
        /// </summary>
        /// <param name="root">The base directory for relative paths.</param>
        /// <param name="path">The JSON file path to read.</param>
        /// <returns>The absolute normalized path, or an empty string for blank input.</returns>
        static string FullPath(string root, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            return Path.IsPathRooted(path) ? Path.GetFullPath(path) : Path.GetFullPath(Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar)));
        }

        /// <summary>
        /// Normalizes a Unity asset path to forward-slash separators.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The normalized Unity asset path.</returns>
        static string NormalizeAssetPath(string path)
        {
            return (path ?? string.Empty).Trim().Replace('\\', '/');
        }

        /// <summary>
        /// Converts an absolute file path to a project-relative path for result reporting.
        /// </summary>
        /// <param name="projectRoot">The Unity project root directory.</param>
        /// <param name="fullPath">The absolute path to make relative.</param>
        /// <returns>A forward-slash project-relative path when possible.</returns>
        static string MakeProjectRelative(string projectRoot, string fullPath)
        {
            try { return Path.GetRelativePath(projectRoot, fullPath).Replace('\\', '/'); }
            catch { return fullPath.Replace('\\', '/'); }
        }

        /// <summary>
        /// Loads and deserializes a JSON file into the requested schema type.
        /// </summary>
        /// <typeparam name="T">The schema type to deserialize.</typeparam>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The deserialized schema object.</returns>
        static T LoadJson<T>(string path)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path), new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        /// <summary>
        /// Adds a sprite lookup entry when both the key and sprite are valid.
        /// </summary>
        /// <param name="map">The sprite lookup table being populated.</param>
        /// <param name="key">The lookup key to register.</param>
        /// <param name="sprite">The sprite to store.</param>
        static void AddSprite(Dictionary<string, Sprite> map, string key, Sprite sprite)
        {
            if (!string.IsNullOrWhiteSpace(key) && sprite != null) map[key] = sprite;
        }

        /// <summary>
        /// Returns the first non-empty string from a priority-ordered list.
        /// </summary>
        /// <param name="values">Candidate values in priority order.</param>
        /// <returns>The trimmed first non-empty value, or an empty string.</returns>
        static string FirstNonEmpty(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
            }
            return string.Empty;
        }

        /// <summary>
        /// Removes invalid file-name characters from a generated Unity object or asset name.
        /// </summary>
        /// <param name="value">The candidate object or asset name.</param>
        /// <returns>A safe trimmed name, or an empty string.</returns>
        static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return value.Trim();
        }

        /// <summary>
        /// Counts all visual nodes recursively.
        /// </summary>
        /// <param name="nodes">The node array to count.</param>
        /// <returns>The total number of non-null nodes.</returns>
        static int CountNodes(WebVisualNode[] nodes)
        {
            if (nodes == null) return 0;
            var count = 0;
            foreach (var node in nodes) count += node == null ? 0 : 1 + CountNodes(node.children);
            return count;
        }

        /// <summary>
        /// Splits dropdown option text into individual display values.
        /// </summary>
        /// <param name="value">The option string from the visual document.</param>
        /// <returns>The non-empty option labels.</returns>
        static string[] SplitOptions(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? Array.Empty<string>()
                : value.Split(new[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).Where(v => v.Length > 0).ToArray();
        }

        /// <summary>
        /// Resolves the explicitly authored UGUI Image rendering type.
        /// </summary>
        /// <param name="node">The visual node being converted.</param>
        /// <param name="sprite">The resolved sprite assigned to the Image.</param>
        /// <returns>The Image type matching the node declaration and sprite import settings.</returns>
        static Image.Type ResolveImageType(WebVisualNode node, Sprite sprite)
        {
            var imageType = node.style != null ? node.style.imageType : string.Empty;
            if (string.Equals(imageType, "filled", StringComparison.OrdinalIgnoreCase))
            {
                return Image.Type.Filled;
            }

            if (string.Equals(imageType, "sliced", StringComparison.OrdinalIgnoreCase)
                && sprite != null
                && sprite.border != Vector4.zero)
            {
                return Image.Type.Sliced;
            }

            return Image.Type.Simple;
        }

        /// <summary>
        /// Detects password input fields from node metadata and visible text.
        /// </summary>
        /// <param name="node">The visual node being converted.</param>
        /// <returns>True when the node should use password masking.</returns>
        static bool IsPassword(WebVisualNode node)
        {
            var text = (node.id + " " + node.name + " " + (node.text != null ? node.text.content : string.Empty)).ToLowerInvariant();
            return text.Contains("password") || text.Contains("密码");
        }

        /// <summary>
        /// Converts CSS-style font weight values into a TextMeshPro bold flag.
        /// </summary>
        /// <param name="weight">The CSS font-weight value.</param>
        /// <returns>True when the supplied weight represents bold text.</returns>
        static bool IsBold(string weight)
        {
            int numeric;
            return string.Equals(weight, "bold", StringComparison.OrdinalIgnoreCase)
                   || (int.TryParse(weight, out numeric) && numeric >= 600);
        }

#if USE_OLD_TEXT
        /// <summary>
        /// Maps CSS-style text alignment to Unity UI Text alignment options.
        /// </summary>
        /// <param name="alignment">The CSS-style alignment string.</param>
        /// <returns>The Unity UI Text alignment option.</returns>
        static TextAnchor ResolveAlignment(string alignment)
        {
            if (string.Equals(alignment, "center", StringComparison.OrdinalIgnoreCase)) return TextAnchor.MiddleCenter;
            if (string.Equals(alignment, "end", StringComparison.OrdinalIgnoreCase) || string.Equals(alignment, "right", StringComparison.OrdinalIgnoreCase)) return TextAnchor.MiddleRight;
            return TextAnchor.MiddleLeft;
        }
#else
        /// <summary>
        /// Maps CSS-style text alignment to TextMeshPro alignment options.
        /// </summary>
        /// <param name="alignment">The CSS-style alignment string.</param>
        /// <returns>The TextMeshPro alignment option.</returns>
        static TextAlignmentOptions ResolveAlignment(string alignment)
        {
            if (string.Equals(alignment, "center", StringComparison.OrdinalIgnoreCase)) return TextAlignmentOptions.Center;
            if (string.Equals(alignment, "end", StringComparison.OrdinalIgnoreCase) || string.Equals(alignment, "right", StringComparison.OrdinalIgnoreCase)) return TextAlignmentOptions.Right;
            return TextAlignmentOptions.Left;
        }
#endif

        /// <summary>
        /// Parses a CSS hex color and applies visual-node opacity.
        /// </summary>
        /// <param name="value">The CSS color string to parse.</param>
        /// <param name="alpha">The opacity multiplier to apply.</param>
        /// <returns>The parsed Unity color, or white with the requested opacity.</returns>
        static Color ParseColor(string value, float alpha)
        {
            Color color;
            if (!string.IsNullOrWhiteSpace(value) && ColorUtility.TryParseHtmlString(value, out color))
            {
                color.a *= alpha <= 0f ? 1f : alpha;
                return color;
            }
            return new Color(1f, 1f, 1f, alpha <= 0f ? 1f : alpha);
        }

        /// <summary>
        /// Shared data used while building one prefab.
        /// </summary>
        sealed class BuildContext
        {
            /// <summary>
            /// Sprites addressable by node id, layer id, file name, and asset path.
            /// </summary>
            public IReadOnlyDictionary<string, Sprite> Sprites;
            /// <summary>
            /// Font used by generated text elements.
            /// </summary>
#if USE_OLD_TEXT
            public Font Font;
#else
            public TMP_FontAsset Font;
#endif
            /// <summary>
            /// Layer assigned to every generated UI object.
            /// </summary>
            public int UiLayer;
            /// <summary>
            /// Assets folder used for generated UI resources.
            /// </summary>
            public string AssetsFolder;
            public Dictionary<string, GameObject> NodeObjects;
        }
    }

    /// <summary>
    /// Root schema produced by the Web visual-ui extraction step.
    /// </summary>
    [Serializable]
    public sealed class WebVisualDocument
    {
        /// <summary>
        /// Schema version for compatibility checks.
        /// </summary>
        public string schemaVersion;
        /// <summary>
        /// Name of the Unity UI prefab being generated.
        /// </summary>
        public string uiName;
        /// <summary>
        /// Pixel resolution used by all node rectangles.
        /// </summary>
        public WebVisualResolution referenceResolution;
        /// <summary>
        /// Top-level visual nodes.
        /// </summary>
        public WebVisualNode[] nodes = Array.Empty<WebVisualNode>();
    }

    /// <summary>
    /// Pixel dimensions for a visual-ui document.
    /// </summary>
    [Serializable]
    public sealed class WebVisualResolution
    {
        /// <summary>
        /// Width in pixels.
        /// </summary>
        public int width;
        /// <summary>
        /// Height in pixels.
        /// </summary>
        public int height;
    }

    /// <summary>
    /// Serializable visual node that maps to one generated RectTransform.
    /// </summary>
    [Serializable]
    public sealed class WebVisualNode
    {
        /// <summary>
        /// Stable semantic identifier used for sprite lookup and generated object names.
        /// </summary>
        public string id;
        /// <summary>
        /// Display name used for the generated GameObject when present.
        /// </summary>
        public string name;
        /// <summary>
        /// Original web extraction type.
        /// </summary>
        public string webType;
        /// <summary>
        /// Semantic UGUI element type, such as Image, Button, Text, InputField, or ScrollView.
        /// </summary>
        public string element;
        /// <summary>
        /// Node rectangle in reference-resolution top-left pixels.
        /// </summary>
        public WebVisualRect rect;
        /// <summary>
        /// Visual style and sprite metadata.
        /// </summary>
        public WebVisualStyle style;
        /// <summary>
        /// Text metadata for generated TextMeshProUGUI children or labels.
        /// </summary>
        public WebVisualText text;
        /// <summary>
        /// Dropdown options and selected value.
        /// </summary>
        public WebVisualDropdown dropdown;
        /// <summary>
        /// List, grid, and scroll settings.
        /// </summary>
        public WebVisualList list;
        /// <summary>
        /// Child nodes nested under this node.
        /// </summary>
        public WebVisualNode[] children = Array.Empty<WebVisualNode>();
    }

    /// <summary>
    /// Rectangle in top-left pixel coordinates from the design reference resolution.
    /// </summary>
    [Serializable]
    public sealed class WebVisualRect
    {
        /// <summary>
        /// Left position in pixels.
        /// </summary>
        public float x;
        /// <summary>
        /// Top position in pixels.
        /// </summary>
        public float y;
        /// <summary>
        /// Width in pixels.
        /// </summary>
        public float width;
        /// <summary>
        /// Height in pixels.
        /// </summary>
        public float height;
        /// <summary>
        /// Converts the serializable rectangle into a Unity Rect.
        /// </summary>
        /// <returns>A Rect with the same x, y, width, and height values.</returns>
        public Rect ToRect() { return new Rect(x, y, width, height); }
    }

    /// <summary>
    /// Serializable style information used by generated Image and Graphic components.
    /// </summary>
    [Serializable]
    public sealed class WebVisualStyle
    {
        /// <summary>
        /// Image tint color in HTML hex format.
        /// </summary>
        public string color;
        /// <summary>
        /// Text color in HTML hex format.
        /// </summary>
        public string textColor;
        /// <summary>
        /// Sprite asset path or manifest lookup key.
        /// </summary>
        public string sprite;
        /// <summary>
        /// Image rendering mode, such as simple or sliced.
        /// </summary>
        public string imageType;
        /// <summary>
        /// Alpha multiplier extracted from the source style.
        /// </summary>
        public float alpha = 1f;
        /// <summary>
        /// Opacity multiplier extracted from the source style.
        /// </summary>
        public float opacity = 1f;
    }

    /// <summary>
    /// Serializable text metadata used by generated TextMeshProUGUI components.
    /// </summary>
    [Serializable]
    public sealed class WebVisualText
    {
        /// <summary>
        /// Text content.
        /// </summary>
        public string content;
        /// <summary>
        /// Font size in design pixels.
        /// </summary>
        public float fontSize;
        /// <summary>
        /// Font weight token, such as 400 or 700.
        /// </summary>
        public string fontWeight;
        /// <summary>
        /// Text color in HTML hex format.
        /// </summary>
        public string color;
        /// <summary>
        /// Horizontal/vertical alignment token.
        /// </summary>
        public string alignment;
    }

    /// <summary>
    /// Dropdown-specific metadata.
    /// </summary>
    [Serializable]
    public sealed class WebVisualDropdown
    {
        /// <summary>
        /// Pipe-delimited option text.
        /// </summary>
        public string options;
        /// <summary>
        /// Initially selected option index.
        /// </summary>
        public int value;
    }

    /// <summary>
    /// Grid and scroll layout metadata for list-like nodes.
    /// </summary>
    [Serializable]
    public sealed class WebVisualList
    {
        /// <summary>
        /// Layout mode, such as grid.
        /// </summary>
        public string layout;
        /// <summary>
        /// Scroll direction: vertical, horizontal, or both.
        /// </summary>
        public string scrollDirection;
        /// <summary>
        /// Grid constraint mode for Unity GridLayoutGroup.
        /// </summary>
        public string gridConstraint;
        /// <summary>
        /// Row or column count used by the grid constraint.
        /// </summary>
        public int gridCount;
        /// <summary>
        /// Grid cell width.
        /// </summary>
        public float cellWidth;
        /// <summary>
        /// Grid cell height.
        /// </summary>
        public float cellHeight;
        /// <summary>
        /// Horizontal spacing between grid cells.
        /// </summary>
        public float spacingX;
        /// <summary>
        /// Vertical spacing between grid cells.
        /// </summary>
        public float spacingY;
        /// <summary>
        /// Left padding for the grid layout.
        /// </summary>
        public float paddingLeft;
        /// <summary>
        /// Right padding for the grid layout.
        /// </summary>
        public float paddingRight;
        /// <summary>
        /// Top padding for the grid layout.
        /// </summary>
        public float paddingTop;
        /// <summary>
        /// Bottom padding for the grid layout.
        /// </summary>
        public float paddingBottom;
    }

    /// <summary>
    /// Manifest that maps visual-ui nodes to production PNG assets.
    /// </summary>
    [Serializable]
    public sealed class UguiAssetManifest
    {
        /// <summary>
        /// Manifest schema version.
        /// </summary>
        public string schemaVersion;
        /// <summary>
        /// UI name this manifest belongs to.
        /// </summary>
        public string uiName;
        /// <summary>
        /// Reference resolution used by the manifest assets.
        /// </summary>
        public WebVisualResolution referenceResolution;
        /// <summary>
        /// Asset entries copied and imported before prefab generation.
        /// </summary>
        public UguiAssetEntry[] assets = Array.Empty<UguiAssetEntry>();
    }

    /// <summary>
    /// Single asset mapping in the UGUI asset manifest.
    /// </summary>
    [Serializable]
    public sealed class UguiAssetEntry
    {
        /// <summary>
        /// Stable asset id.
        /// </summary>
        public string id;
        /// <summary>
        /// Optional layer plan id used as an additional sprite lookup key.
        /// </summary>
        public string layerPlanId;
        /// <summary>
        /// Source PNG path relative to the manifest directory.
        /// </summary>
        public string file;
        /// <summary>
        /// Preferred repaired PNG path relative to the manifest directory.
        /// </summary>
        public string repairedAsset;
        /// <summary>
        /// Destination Unity asset path.
        /// </summary>
        public string path;
        /// <summary>
        /// Node ids that should use this asset.
        /// </summary>
        public string[] usedBy = Array.Empty<string>();
        /// <summary>
        /// Sprite border values used for sliced images.
        /// </summary>
        public float[] spriteBorder = Array.Empty<float>();
    }
}
