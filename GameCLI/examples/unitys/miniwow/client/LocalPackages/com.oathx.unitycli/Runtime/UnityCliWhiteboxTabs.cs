using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Oathx.UnityCLI
{
    /// <summary>Runs explicit local sample tabs; source callbacks and inventory services are never executed.</summary>
    public sealed class UnityCliWhiteboxTabs : MonoBehaviour
    {
        [Serializable]
        public sealed class Tab
        {
            public string path;
            public int category;
        }

        [Serializable]
        public sealed class Item
        {
            public string path;
            public int category;
            public string title;
            public string description;
        }

        [Serializable]
        public sealed class Configuration
        {
            public Tab[] tabs;
            public Item[] items;
            public string itemButtonPath;
            public string selectionPath;
            public string titlePath;
            public string descriptionPath;
            public bool ensureEventSystem;
            public int referenceWidth;
            public int referenceHeight;
        }

        [SerializeField] private string configuration;
        private Configuration data;
        private readonly List<Button> listeners = new List<Button>();
        private readonly List<UnityAction> actions = new List<UnityAction>();
        private bool bound;
        private GameObject ownedCanvas;

        public int SelectedTab
        {
            get;
            private set;
        }

        public int SelectedItem
        {
            get;
            private set;
        }

        public int VisibleItemCount
        {
            get;
            private set;
        }

        public int SelectionRevision
        {
            get;
            private set;
        }

        public Configuration ValidateConfiguration()
        {
            Configuration parsed = JsonUtility.FromJson<Configuration>(configuration ?? "");
            if (parsed == null || parsed.tabs == null || parsed.tabs.Length < 2 || parsed.items == null || parsed.items.Length == 0)
            {
                throw new InvalidOperationException("Whitebox tabs require explicit tabs and sample items.");
            }
            HashSet<string> paths = new HashSet<string>(StringComparer.Ordinal);
            foreach (Tab tab in parsed.tabs)
            {
                if (!paths.Add(tab.path) || Find(tab.path).GetComponent<Button>() == null)
                {
                    throw new InvalidOperationException("Whitebox tab must resolve a unique Button: " + tab.path);
                }
            }
            paths.Clear();
            foreach (Item item in parsed.items)
            {
                Transform cell = Find(item.path);
                if (!paths.Add(item.path) || cell.Find(parsed.itemButtonPath)?.GetComponent<Button>() == null || cell.Find(parsed.selectionPath) == null)
                {
                    throw new InvalidOperationException("Whitebox item binding is incomplete: " + item.path);
                }
            }
            if (Find(parsed.titlePath).GetComponent<Text>() == null || Find(parsed.descriptionPath).GetComponent<Text>() == null)
            {
                throw new InvalidOperationException("Whitebox detail text bindings are incomplete.");
            }
            return parsed;
        }

        private void OnEnable()
        {
            TryBind();
        }

        private void Start()
        {
            // Live CLI assembly may assign serialized fields after AddComponent invokes OnEnable.
            TryBind();
        }

        private void TryBind()
        {
            if (bound || !Application.isPlaying || string.IsNullOrEmpty(configuration))
            {
                return;
            }
            data = ValidateConfiguration();
            EnsureInputHost();
            if (data.ensureEventSystem && EventSystem.current == null)
            {
                // The scene owns this fallback. Existing host input systems always take precedence.
                GameObject events = new GameObject("WhiteboxEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                SceneManager.MoveGameObjectToScene(events, gameObject.scene);
            }
            for (int i = 0; i < data.tabs.Length; i++)
            {
                int tabIndex = i;
                Listen(Find(data.tabs[i].path).GetComponent<Button>(), () => SelectTab(tabIndex));
            }
            for (int i = 0; i < data.items.Length; i++)
            {
                int itemIndex = i;
                Listen(Find(data.items[i].path).Find(data.itemButtonPath).GetComponent<Button>(), () => SelectItem(itemIndex));
            }
            bound = true;
            SelectTab(0);
        }

        private void EnsureInputHost()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Canvas previousCanvas = canvas == null ? null : canvas.rootCanvas;
            CanvasScaler existingScaler = previousCanvas == null ? null : previousCanvas.GetComponent<CanvasScaler>();
            bool incompatibleHost = previousCanvas != null && data.referenceWidth > 0 && data.referenceHeight > 0
                && (existingScaler == null || existingScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                    || Mathf.Abs(existingScaler.referenceResolution.x - data.referenceWidth) > 0.5f
                    || Mathf.Abs(existingScaler.referenceResolution.y - data.referenceHeight) > 0.5f);
            if (canvas == null || incompatibleHost)
            {
                if (data.referenceWidth <= 0 || data.referenceHeight <= 0)
                {
                    throw new InvalidOperationException("Standalone whitebox tabs require an explicit reference resolution.");
                }
                // Keep the prefab root stretched under a host; never turn it into a driven root Canvas.
                ownedCanvas = new GameObject("WhiteboxCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                SceneManager.MoveGameObjectToScene(ownedCanvas, gameObject.scene);
                canvas = ownedCanvas.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                if (previousCanvas != null)
                {
                    canvas.sortingLayerID = previousCanvas.sortingLayerID;
                    canvas.sortingOrder = Mathf.Min(32767, previousCanvas.sortingOrder + 1);
                    canvas.targetDisplay = previousCanvas.targetDisplay;
                }
                CanvasScaler scaler = ownedCanvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(data.referenceWidth, data.referenceHeight);
                scaler.matchWidthOrHeight = 0;
                transform.SetParent(ownedCanvas.transform, false);
            }
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                if (listeners[i] != null)
                {
                    listeners[i].onClick.RemoveListener(actions[i]);
                }
            }
            listeners.Clear();
            actions.Clear();
            bound = false;
        }

        private void OnDestroy()
        {
            if (ownedCanvas != null && gameObject.scene.isLoaded)
            {
                Destroy(ownedCanvas);
            }
        }

        private void Listen(Button button, UnityAction action)
        {
            button.interactable = true;
            button.onClick.AddListener(action);
            listeners.Add(button);
            actions.Add(action);
        }

        private void SelectTab(int index)
        {
            if (!bound)
            {
                return;
            }
            SelectedTab = index;
            SelectionRevision++;
            int category = data.tabs[index].category;
            for (int i = 0; i < data.tabs.Length; i++)
            {
                Image background = Find(data.tabs[i].path).GetComponent<Image>();
                if (background != null)
                {
                    background.color = new Color(0.78f, 0.78f, 0.78f, i == index ? 1 : 0);
                }
            }
            int first = -1;
            VisibleItemCount = 0;
            for (int i = 0; i < data.items.Length; i++)
            {
                bool visible = category == 0 || data.items[i].category == category;
                Find(data.items[i].path).gameObject.SetActive(visible);
                if (visible)
                {
                    VisibleItemCount++;
                    if (first < 0)
                    {
                        first = i;
                    }
                }
            }
            SelectItem(first);
            Canvas.ForceUpdateCanvases();
        }

        private void SelectItem(int index)
        {
            if (!bound || index < 0 || index >= data.items.Length || !Find(data.items[index].path).gameObject.activeInHierarchy)
            {
                return;
            }
            SelectedItem = index;
            for (int i = 0; i < data.items.Length; i++)
            {
                Find(data.items[i].path).Find(data.selectionPath).gameObject.SetActive(i == index);
            }
            Find(data.titlePath).GetComponent<Text>().text = data.items[index].title;
            Find(data.descriptionPath).GetComponent<Text>().text = data.items[index].description;
        }

        private Transform Find(string path)
        {
            if (string.IsNullOrEmpty(path) || path.StartsWith("/") || path.Contains(".."))
            {
                throw new InvalidOperationException("Whitebox bindings require relative paths.");
            }
            Transform found = transform.Find(path);
            if (found == null)
            {
                throw new InvalidOperationException("Missing whitebox binding: " + path);
            }
            return found;
        }
    }
}
