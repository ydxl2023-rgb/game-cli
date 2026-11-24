using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if !USE_OLD_TEXT
using TMPro;
#endif

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Creates task-defined whitebox prefabs without the Web visual-ui pipeline.</summary>
    internal static class UnityCliTaskPrefabService
    {
        [Serializable]
        private sealed class Layout
        {
            public int schemaVersion;
            public string taskId;
            public string source;
            public float width;
            public float height;
            public Node root;
        }

        [Serializable]
        private sealed class Node
        {
            public string id;
            public string name;
            public bool active = true;
            public bool interactable = true;
            public bool viewportImage = true;
            public bool safeArea;
            public float[] anchorMin;
            public float[] anchorMax;
            public float[] pivot;
            public string alignment = "MiddleCenter";
            public string overflow = "Overflow";
            public bool wordWrap = true;
            public bool autoSize;
            public int minFontSize = 12;
            public int maxFontSize = 72;
            public string type;
            public float x;
            public float y;
            public float width;
            public float height;
            public string color = "#FFFFFFFF";
            public bool raycastTarget;
            public string targetGraphic;
            public string text = "";
            public string font;
            public int fontSize = 24;
            public string sprite;
            public bool sliced;
            public string direction = "vertical";
            public string viewport;
            public string content;
            public bool blocksRaycasts = true;
            public Node[] children = Array.Empty<Node>();
        }

        public static PrefabData Execute(UnityCliBridgeRequest request)
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            string path = (request.prefabPath ?? "").Replace('\\', '/');
            string full = Path.GetFullPath(Path.Combine(project, path));
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || !path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                || !full.StartsWith(Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || path.Contains(".."))
            {
                throw new InvalidOperationException("Output must be a .prefab under Assets without traversal.");
            }
            if (string.IsNullOrWhiteSpace(request.jsonFile))
            {
                throw new InvalidOperationException("Task layout JSON is required.");
            }
            string input = Path.GetFullPath(Path.Combine(project, request.jsonFile));
            if (new FileInfo(input).Length > 1024 * 1024)
            {
                throw new InvalidOperationException("Task layout exceeds 1 MiB.");
            }
            Layout layout = JsonConvert.DeserializeObject<Layout>(File.ReadAllText(input), new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                MaxDepth = 64
            });
            if (layout == null || (layout.schemaVersion != 1 && layout.schemaVersion != 2) || string.IsNullOrWhiteSpace(layout.taskId)
                || string.IsNullOrWhiteSpace(layout.source) || !Positive(layout.width) || !Positive(layout.height)
                || layout.root == null || layout.root.type != "Panel"
                || layout.root.safeArea
                || layout.root.width != layout.width || layout.root.height != layout.height)
            {
                throw new InvalidOperationException("Require schemaVersion=1 or 2, taskId, source, finite design size and Panel root.");
            }
            Dictionary<string, Node> specs = new Dictionary<string, Node>();
            Validate(layout.root, specs, 0);
            foreach (Node node in specs.Values)
            {
                if (node.targetGraphic != null)
                {
                    // A separate button background must belong to this button, never another control.
                    Node background = Array.Find(node.children, child => child.id == node.targetGraphic);
                    if (node.type != "Button" || background == null || background.type != "Image" || !background.raycastTarget)
                    {
                        throw new InvalidOperationException(node.id + ": targetGraphic must reference a direct child Image with raycastTarget=true.");
                    }
                }
                if (node.type == "ScrollRect")
                {
                    Node viewport = Array.Find(node.children, child => child.id == node.viewport);
                    Node content = viewport == null ? null : Array.Find(viewport.children, child => child.id == node.content);
                    if (viewport == null || viewport.type != "Viewport" || content == null || content.type != "Container")
                    {
                        throw new InvalidOperationException(node.id + ": require direct ScrollRect -> Viewport -> Content Container references.");
                    }
                }
            }
            if (File.Exists(full) && !(request.replaceExistingChildSet && request.replaceExistingChild))
            {
                throw new InvalidOperationException("Prefab exists. Use --replace explicitly to update it.");
            }
            PrefabData data = new PrefabData
            {
                action = "create",
                prefabPath = path,
                dryRun = request.dryRun,
                nodeCount = specs.Count,
                message = request.dryRun ? "Task prefab dry-run passed." : "Task prefab created."
            };
            // Build in a preview scene even for dry-run so component/configuration failures precede writes.
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
                GameObject root = Build(layout.root, null, preview, objects, layout.width, layout.height);
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                foreach (Node node in specs.Values)
                {
                    if (node.type == "Button" && node.targetGraphic != null)
                    {
                        objects[node.id].GetComponent<Button>().targetGraphic = objects[node.targetGraphic].GetComponent<Image>();
                    }
                    if (node.type == "ScrollRect")
                    {
                        ScrollRect scroll = objects[node.id].GetComponent<ScrollRect>();
                        scroll.viewport = objects[node.viewport].GetComponent<RectTransform>();
                        scroll.content = objects[node.content].GetComponent<RectTransform>();
                        scroll.horizontal = node.direction == "horizontal";
                        scroll.vertical = node.direction == "vertical";
                        scroll.movementType = ScrollRect.MovementType.Clamped;
                    }
                }
                if (!request.dryRun)
                {
                    string folder = Path.GetDirectoryName(path).Replace('\\', '/');
                    EnsureFolder(folder);
                    GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
                    if (!success || saved == null)
                    {
                        throw new InvalidOperationException("Unity failed to save prefab.");
                    }
                    data.prefabModified = true;
                }
                return data;
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        private static bool Positive(float value)
        {
            return value > 0 && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void Validate(Node node, Dictionary<string, Node> nodes, int depth)
        {
            if (node == null || depth > 24 || nodes.Count >= 512 || string.IsNullOrWhiteSpace(node.id)
                || node.id.IndexOfAny(new[] {'\n', '\r'}) >= 0 || nodes.ContainsKey(node.id))
            {
                throw new InvalidOperationException("Invalid/duplicate node ID or excessive depth/node count.");
            }
            string objectName = node.name ?? node.id;
            if (string.IsNullOrWhiteSpace(objectName) || objectName.IndexOfAny(new[] {'/', '\\', '\n', '\r'}) >= 0)
            {
                throw new InvalidOperationException(node.id + ": invalid GameObject name; supply name separately from source ID.");
            }
            ValidateVector(node.anchorMin);
            ValidateVector(node.anchorMax);
            ValidateVector(node.pivot);
            if ((node.anchorMin == null) != (node.anchorMax == null)
                || (node.anchorMin != null && (node.anchorMin[0] > node.anchorMax[0] || node.anchorMin[1] > node.anchorMax[1]))
                || !Enum.TryParse(node.alignment, out TextAnchor alignment) || !Enum.IsDefined(typeof(TextAnchor), alignment)
                || Array.IndexOf(new[] {"Overflow", "Truncate", "Ellipsis"}, node.overflow) < 0
                || node.minFontSize < 1 || node.maxFontSize > 300 || node.minFontSize > node.maxFontSize)
            {
                throw new InvalidOperationException(node.id + ": invalid anchors or text settings.");
            }
#if USE_OLD_TEXT
            if (node.overflow == "Ellipsis")
            {
                throw new InvalidOperationException(node.id + ": Ellipsis requires TMP; legacy Text cannot represent it.");
            }
#endif
            if (Array.IndexOf(new[] {"Panel", "Container", "Image", "Text", "Button", "ScrollRect", "Viewport"}, node.type) < 0
                || !Positive(node.width) || !Positive(node.height) || float.IsNaN(node.x) || float.IsInfinity(node.x)
                || float.IsNaN(node.y) || float.IsInfinity(node.y) || node.fontSize < 1 || node.fontSize > 300
                || node.children == null || !ColorUtility.TryParseHtmlString(node.color, out _)
                || (node.type == "ScrollRect" && node.direction != "horizontal" && node.direction != "vertical"))
            {
                throw new InvalidOperationException(node.id + ": invalid type, rect, color or scrolling direction.");
            }
            if (!string.IsNullOrEmpty(node.sprite) && AssetDatabase.LoadAssetAtPath<Sprite>(node.sprite) == null)
            {
                throw new InvalidOperationException(node.id + ": Sprite asset not found.");
            }
            if (!string.IsNullOrEmpty(node.font))
            {
#if USE_OLD_TEXT
                bool found = AssetDatabase.LoadAssetAtPath<Font>(node.font) != null;
#else
                bool found = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(node.font) != null;
#endif
                if (node.type != "Text" || !found)
                {
                    throw new InvalidOperationException(node.id + ": font must reference a compatible imported font asset on Text.");
                }
            }
            nodes.Add(node.id, node);
            HashSet<string> childNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (Node child in node.children)
            {
                if (child != null && !childNames.Add(child.name ?? child.id))
                {
                    throw new InvalidOperationException(node.id + ": duplicate sibling names make hierarchy paths ambiguous.");
                }
                Validate(child, nodes, depth + 1);
            }
        }

        private static void ValidateVector(float[] value)
        {
            if (value != null && (value.Length != 2 || !FiniteUnit(value[0]) || !FiniteUnit(value[1])))
            {
                throw new InvalidOperationException("Anchors and pivot require two finite numbers in [0,1].");
            }
        }

        private static bool FiniteUnit(float value)
        {
            return !float.IsNaN(value) && value >= 0 && value <= 1;
        }

        private static GameObject Build(Node node, Transform parent, Scene scene, Dictionary<string, GameObject> nodes, float parentWidth, float parentHeight)
        {
            GameObject go = new GameObject(node.name ?? node.id, typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(go, scene);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            nodes.Add(node.id, go);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = node.anchorMin == null ? new Vector2(0, 1) : new Vector2(node.anchorMin[0], node.anchorMin[1]);
            rect.anchorMax = node.anchorMax == null ? rect.anchorMin : new Vector2(node.anchorMax[0], node.anchorMax[1]);
            rect.pivot = node.pivot == null ? new Vector2(0, 1) : new Vector2(node.pivot[0], node.pivot[1]);
            // Preserve the design rectangle at the reference size; anchors govern subsequent resizing.
            rect.offsetMin = new Vector2(node.x - parentWidth * rect.anchorMin.x, parentHeight - node.y - node.height - parentHeight * rect.anchorMin.y);
            rect.offsetMax = new Vector2(node.x + node.width - parentWidth * rect.anchorMax.x, parentHeight - node.y - parentHeight * rect.anchorMax.y);
            ColorUtility.TryParseHtmlString(node.color, out Color color);
            if (node.type == "Panel")
            {
                CanvasGroup group = go.AddComponent<CanvasGroup>();
                group.blocksRaycasts = node.blocksRaycasts;
            }
            if (node.type == "Image" || (node.type == "Button" && node.targetGraphic == null) || (node.type == "Viewport" && node.viewportImage))
            {
                Image image = go.AddComponent<Image>();
                image.color = color;
                image.raycastTarget = node.raycastTarget;
                image.sprite = string.IsNullOrEmpty(node.sprite) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(node.sprite);
                image.type = node.sliced ? Image.Type.Sliced : Image.Type.Simple;
            }
            if (node.type == "Viewport")
            {
                go.AddComponent<RectMask2D>();
            }
            if (node.type == "Button")
            {
                // Child-backed buttons deliberately have no extra root Image.
                go.AddComponent<Button>().targetGraphic = go.GetComponent<Image>();
                go.GetComponent<Button>().interactable = node.interactable;
            }
            if (node.type == "Text")
            {
#if USE_OLD_TEXT
                Text text = go.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (!string.IsNullOrEmpty(node.font))
                {
                    text.font = AssetDatabase.LoadAssetAtPath<Font>(node.font);
                }
                text.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), node.alignment);
                text.resizeTextForBestFit = node.autoSize;
                text.resizeTextMinSize = node.minFontSize;
                text.resizeTextMaxSize = node.maxFontSize;
                text.horizontalOverflow = node.wordWrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
                text.verticalOverflow = node.overflow == "Overflow" ? VerticalWrapMode.Overflow : VerticalWrapMode.Truncate;
#else
                TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
                if (!string.IsNullOrEmpty(node.font))
                {
                    text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(node.font);
                }
                TextAlignmentOptions[] alignments = { TextAlignmentOptions.TopLeft, TextAlignmentOptions.Top, TextAlignmentOptions.TopRight,
                    TextAlignmentOptions.Left, TextAlignmentOptions.Center, TextAlignmentOptions.Right,
                    TextAlignmentOptions.BottomLeft, TextAlignmentOptions.Bottom, TextAlignmentOptions.BottomRight };
                text.alignment = alignments[(int)Enum.Parse(typeof(TextAnchor), node.alignment)];
                text.enableAutoSizing = node.autoSize;
                text.fontSizeMin = node.minFontSize;
                text.fontSizeMax = node.maxFontSize;
                text.enableWordWrapping = node.wordWrap;
                text.overflowMode = (TextOverflowModes)Enum.Parse(typeof(TextOverflowModes), node.overflow);
#endif
                text.text = node.text;
                text.fontSize = node.fontSize;
                text.color = color;
                text.raycastTarget = node.raycastTarget;
            }
            if (node.type == "ScrollRect")
            {
                go.AddComponent<ScrollRect>();
            }
            // Input child order is authoritative draw/sibling order.
            foreach (Node child in node.children)
            {
                Build(child, go.transform, scene, nodes, node.width, node.height);
            }
            if (node.safeArea)
            {
                go.AddComponent<Oathx.UnityCLI.UnityCliSafeArea>();
            }
            go.SetActive(node.active);
            return go;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}

