using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oathx.Widget;
using UnityEngine.Rendering.Universal;
using System.Linq;

namespace Oathx.Widget
{

    /// <summary>
    /// 
    /// </summary>
    public class UISystem : MonoBehaviour
    {
        public static UISystem instance;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UISystem GetSingleton()
        {
            if (!instance)
            {
                var root = GameObject.Find(typeof(UISystem).Name);
                if (root)
                {
                    instance = root.GetComponent<UISystem>();
                    if (!instance)
                        instance = root.AddComponent<UISystem>();

                    DontDestroyOnLoad(root);
                }
                else
                {
                    Debug.LogError($"Can't find GameObject {typeof(UISystem).Name}");
                }
            }

            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        protected readonly Dictionary<string, ConfigAttribute>
            customModleAttributes = new Dictionary<string, ConfigAttribute>();

#pragma warning disable IDE0090
        /// <summary>
        /// It has been loaded widget
        /// </summary>
        protected readonly Dictionary<string, IWidget> widgets = new Dictionary<string, IWidget>();

        /// <summary>
        /// Under loading widget
        /// </summary>
        protected readonly Dictionary<string, bool>
            loadingMap = new Dictionary<string, bool>();

        /// <summary>
        /// current opened widgets
        /// </summary>
        protected readonly List<IWidget> currents = new List<IWidget>();
#pragma warning restore IDE0090

        /// <summary>
        /// 每个界面的开合代际号：每次打开与每次关闭收尾各递增一次。
        /// 关闭动画的等待只在代际未变时才执行停用，避免把关闭中途重新打开的界面误关掉。
        /// </summary>
        protected readonly Dictionary<IWidget, int> openGenerations = new Dictionary<IWidget, int>();

        private static readonly int OpenStateHash = Animator.StringToHash("Open");

        private static readonly int CloseStateHash = Animator.StringToHash("Close");

        /// <summary>
        /// 等待关闭动画的硬超时上限，非缩放秒。
        /// </summary>
        private const float CloseAnimationTimeoutSeconds = 3f;

        [SerializeField]
        protected Camera uiCamera;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        protected Vector3 deltaSize;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            // init layer order value
            string[] layerNames = System.Enum.GetNames(typeof(WidgetLayer));
            for (int layer = 0; layer < layerNames.Length; layer++)
            {
                RectTransform layerGo = transform.Find(layerNames[layer]) as RectTransform;
                if (!layerGo)
                    throw new System.NullReferenceException($"Can't find layer {layerNames[layer]}");

                Canvas layerCanvas = layerGo.GetComponent<Canvas>();
                if (layerCanvas)
                    layerCanvas.sortingOrder = layer;

                if (layerNames[layer] == WidgetLayer.background.ToString())
                    deltaSize = layerGo.sizeDelta;
            }

            if (uiCamera)
            {
                var uiCameraData = uiCamera.GetUniversalAdditionalCameraData();
                uiCameraData.renderType = CameraRenderType.Overlay;

                var mainCameraData = Camera.main.GetUniversalAdditionalCameraData();
                if (mainCameraData)
                {
                    int index = mainCameraData.cameraStack.IndexOf(uiCamera);
                    if (index < 0)
                        mainCameraData.cameraStack.Add(uiCamera);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void SetUICamera(Camera camera)
        {
            uiCamera = camera;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="complete"></param>
        public void Startup(ResourceConfigure configure = null)
        {
            UIResourceManager.GetSingleton().Startup(configure ?? new ResourceConfigure()
            {
                loader = new UIDefaultLoader(),
                cacher = new UIDefaultCacher()
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="types"></param>
        public void RegisterSyncActions(System.Type[] types)
        {
            foreach (var type in types)
            {
                var action = (ISyncAction)System.Activator.CreateInstance(type);
                MetadataSyncFactory.GetSingleton().Register(type.Name, action);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Shutdown()
        {
            // clear all widgets
            Clearup();

            customModleAttributes.Clear();
            // clear all cache resources
            UIResourceManager.GetSingleton().Shutdown();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clearup()
        {
            // clear widget stacks
            currents.Clear();

            loadingMap.Clear();

            foreach (var widget in widgets.Values.ToList())
            {
                if (IsWidgetAlive(widget))
                {
                    try
                    {
                        CloseWidget(widget.GetType(), null, true);
                    }
                    catch (System.Exception ex)
                    {
                        string widgetName = widget ? widget.name : "DestroyedWidget";
                        Debug.LogError($"[UISystem] Error closing widget {widgetName}: {ex.Message}");
                    }
                }
            }

            // clear all
            widgets.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Camera GetUICamera()
        {
            return uiCamera;
        }

        /// <summary>
        /// push widget to stack top, open window auto push
        /// </summary>
        /// <param name="widget"></param>
        /// <returns></returns>
        public void AdjustWidget(IWidget widget)
        {
            if (currents.Count <= 0)
            {
                currents.Add(widget);

#if UNITY_EDITOR
                Debug.Log($"[UISystem] Adjust widget index {currents.Count - 1} {widget.name}");
#endif
            }
            else
            {
                var top = currents[currents.Count - 1];
                if (top != widget)
                {
                    var index = currents.FindLastIndex((v) => {
                        return v == widget;
                    });
                    if (index >= 0)
                    {
                        currents.RemoveAt(index);
#if UNITY_EDITOR
                        Debug.Log($"[UISystem] Adjust widget remove index {index} {widget.name}");
#endif
                    }

                    currents.Add(widget);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="widget"></param>
        public void SetTopWidget(IWidget widget)
        {
            AdjustWidget(widget);

            var trans = widget.transform;
            if (trans)
                trans.SetAsLastSibling();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IWidget GetTopWidget()
        {
            if (currents.Count <= 0)
                return null;

            return currents[currents.Count - 1];
        }

        /// <summary>
        /// get current stack top widget
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetTopWidget<T>() where T : IWidget
        {
            if (currents.Count <= 0)
                return null;

            return (T)currents[currents.Count - 1];
        }

        /// <summary>
        /// pop stack top widget
        /// </summary>
        /// <param name="widget"></param>
        /// <returns></returns>
        private bool RemoveWidget(IWidget widget)
        {
            if (currents.Count <= 0)
                return false;

            if (!IsWidgetAlive(widget))
            {
                currents.RemoveAll(v => !IsWidgetAlive(v));
                return false;
            }

            var top = currents[currents.Count - 1];
            if (top == widget)
            {
#if UNITY_EDITOR
                Debug.Log($"[UISystem] Remove widget {widget.name}");
#endif
                currents.Remove(widget);
            }

            return true;
        }

        /// <summary>
        /// create widget
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="styleId"></param>
        /// <param name="path"></param>
        /// <param name="prefab"></param>
        /// <param name="cache"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private T CreateWidget<T>(string name, string path, GameObject prefab, ConfigAttribute setting, Transform parent) where T : IWidget, new()
        {
            GameObject widgetGo = GameObject.Instantiate(prefab);
            if (!widgetGo)
                throw new System.NullReferenceException();

            widgetGo.transform.position = Vector3.zero;
            widgetGo.transform.SetParent(parent, false);

            // set widget type name
            widgetGo.name = name;

            RectTransform rt = widgetGo.GetComponent<RectTransform>();
            if (rt)
            {
                rt.offsetMax = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;
            }

            T widget = widgetGo.GetComponent<T>();
            if (!widget)
                widget = widgetGo.AddComponent<T>();

            BindWidget(widget, widgetGo, setting);

            if (setting.cache)
                widgets[name] = widget;

#if UNITY_EDITOR
            Debug.Log($"[UISystem] Create Widget name [{name}] cache [{setting.cache}] path [{path}]");
#endif

            return widget;
        }

        /// <summary>
        /// create widdget
        /// </summary>
        /// <param name="widgetType"></param>
        /// <param name="path"></param>
        /// <param name="prefab"></param>
        /// <param name="cache"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private IWidget CreateWidget(System.Type widgetType, string path, GameObject prefab, ConfigAttribute setting, Transform parent)
        {
            GameObject widgetGo = GameObject.Instantiate(prefab);
            if (!widgetGo)
                throw new System.NullReferenceException();

            widgetGo.transform.position = Vector3.zero;
            widgetGo.transform.SetParent(parent, false);

            string widgetName = widgetType.Name;
            // set widget type name
            widgetGo.name = widgetName;

            RectTransform rt = widgetGo.GetComponent<RectTransform>();
            if (rt)
            {
                rt.offsetMax = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;
            }

            var widget = (IWidget)widgetGo.GetComponent(widgetType);
            if (!widget)
                widget = (IWidget)widgetGo.AddComponent(widgetType);

            BindWidget(widget, widgetGo, setting);
            
            if (setting.cache)
                widgets[widgetName] = widget;

#if UNITY_EDITOR
            Debug.Log($"[UISystem] Create Widget {widgetName.ToLower()} cache {setting.cache} path {path}");
#endif

            return widget;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="complete"></param>
        /// <param name="cache"></param>
        /// <param name="parent"></param>
        public void LoadAsyncWidget<T>(string path, System.Action<T> complete, ConfigAttribute setting, Transform parent) where T : IWidget, new()
        {
            string widgetName = typeof(T).Name;
            if (widgets.ContainsKey(widgetName))
            {
                complete?.Invoke((T)widgets[widgetName]);
            }
            else
            {
                if (!loadingMap.ContainsKey(widgetName))
                {
                    loadingMap[widgetName] = true;

                    UIResourceManager.GetSingleton().LoadAsync<GameObject>(path, (prefab) =>
                    {
                        loadingMap.Remove(widgetName);

                        T widget = CreateWidget<T>(widgetName, path, prefab as GameObject, setting, parent);
#if UNITY_EDITOR
                        Debug.Log($"[UISystem] Load widget [{widgetName}]");
#endif
                        complete?.Invoke(widget);
                    });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="widgetType"></param>
        /// <param name="path"></param>
        /// <param name="complete"></param>
        /// <param name="cache"></param>
        /// <param name="parent"></param>
        public void LoadAsyncWidget(System.Type widgetType, string path, System.Action<IWidget> complete, ConfigAttribute setting, Transform parent)
        {
            string widgetName = widgetType.Name;
            if (widgets.ContainsKey(widgetName))
            {
                complete?.Invoke(widgets[widgetName]);
            }
            else
            {
                if (!loadingMap.ContainsKey(widgetName))
                {
                    loadingMap[widgetName] = true;

                    UIResourceManager.GetSingleton().LoadAsync<GameObject>(path, (prefab) =>
                    {
                        loadingMap.Remove(widgetName);

                        IWidget widget = CreateWidget(widgetType, path, prefab as GameObject, setting, parent);
#if UNITY_EDITOR
                        Debug.Log($"[UISystem] Load widget {widgetName}");
#endif
                        complete?.Invoke(widget);
                    });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="widgetType"></param>
        /// <param name="path"></param>
        /// <param name="cache"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public IWidget LoadWidget(System.Type widgetType, string path, ConfigAttribute setting, Transform parent)
        {
            string widgetName = widgetType.Name;
            if (widgets.ContainsKey(widgetName))
            {
                return widgets[widgetName];
            }

            GameObject prefab = Resources.Load<GameObject>(path);
            if (!prefab)
                throw new System.NullReferenceException(path);

            return CreateWidget(widgetType, path, prefab, setting, parent);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="cache"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public T LoadWidget<T>(string path, ConfigAttribute setting, Transform parent) where T : IWidget, new()
        {
            string widgetName = typeof(T).Name;
            if (widgets.ContainsKey(widgetName))
            {
                return (T)widgets[widgetName];
            }

            GameObject prefab = Resources.Load<GameObject>(path);
            if (!prefab)
                throw new System.NullReferenceException(path);

            return CreateWidget<T>(widgetName, path, prefab, setting, parent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="widget"></param>
        /// <param name="done"></param>
        /// <summary>
        /// Global widget-opened notification, fired after the widget is activated.
        /// The game layer can subscribe to this to play a unified popup-open sound.
        /// </summary>
        public static event System.Action<IWidget> OnWidgetOpened;

        /// <summary>
        /// Global widget-closed notification, fired when the widget starts closing.
        /// The game layer can subscribe to this to play a unified popup-close sound.
        /// </summary>
        public static event System.Action<IWidget> OnWidgetClosed;

        private void NotifyOpenWidget<T>(T widget, System.Action<T> done) where T : IWidget
        {
            SetTopWidget(widget);

            // 使该界面仍在等待中的关闭动画收尾失效（关闭中途重开即打断）
            openGenerations[widget] = GetCurrentGeneration(widget) + 1;

            widget.Active();

            // 关闭中途重开的恢复：Animator 拉回 Open 态并解锁交互
            var animator = widget.gameObject.GetComponent<Animator>();
            if (animator && animator.runtimeAnimatorController && animator.HasState(0, OpenStateHash))
            {
                animator.Play(OpenStateHash);
            }

            var group = widget.gameObject.GetComponent<CanvasGroup>();
            if (group)
            {
                group.interactable = true;
                group.blocksRaycasts = true;
            }

            OnWidgetOpened?.Invoke(widget);
            done?.Invoke(widget);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="complete"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool OpenWidget<T>(System.Action<T> complete, Transform parent = null) where T : IWidget
        {
            System.Type widgetType = typeof(T);

            var attrs = widgetType.GetCustomAttributes(true);
            if (!customModleAttributes.ContainsKey(widgetType.Name))
            {
                foreach (var attr in attrs)
                {
                    if (attr is ConfigAttribute c)
                    {
                        customModleAttributes.Add(widgetType.Name, c);
                        break;
                    }
                }
            }

            ConfigAttribute setting = customModleAttributes[widgetType.Name];
            if (widgets.ContainsKey(widgetType.Name))
            {
                T widget = (T)widgets[widgetType.Name];
                NotifyOpenWidget(widget, complete);
            }
            else
            {
                if (!parent)
                {
                    parent = transform.Find(setting.layer.ToString());
                }

                if (setting.async)
                {
#if UNITY_EDITOR
                    setting.createAt = System.DateTime.Now.Ticks;
                    int millisecond = System.DateTime.Now.Millisecond;
#endif
                    LoadAsyncWidget(widgetType, setting.path, (widget) => {
#if UNITY_EDITOR
                        setting.loadTime = System.DateTime.Now.Millisecond - millisecond;
#endif
                        NotifyOpenWidget((T)widget, complete);
                    }, setting, parent);
                }
                else
                {
#if UNITY_EDITOR
                    setting.createAt = System.DateTime.Now.Ticks;
                    int millisecond = System.DateTime.Now.Millisecond;
#endif
                    var widget = LoadWidget(widgetType, setting.path, setting, parent);
                    if (widget)
                    {
#if UNITY_EDITOR
                        setting.loadTime = System.DateTime.Now.Millisecond - millisecond;
#endif
                        NotifyOpenWidget((T)widget, complete);
                    }

                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector2 GetDeltaSize()
        {
            return deltaSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public Transform GetLayer(WidgetLayer layer)
        {
            return transform.Find(layer.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="setting"></param>
        private bool BindWidget(IWidget widget, GameObject vmGo, ConfigAttribute setting)
        {
            Metadata metadata = vmGo.GetComponent<Metadata>();
            if (!metadata)
                throw new System.NullReferenceException(vmGo.name);

            widget.setting = setting;

            // bind view model property infos
            VM vm = (VM)System.Activator.CreateInstance(widget.GetVMType());
            if (widget.Ref(vm))
            {
#if UNITY_EDITOR
                Debug.Log($"[UISystem] {widget.GetVMType().Name} binding to {widget.GetType().Name} in {vmGo.name} scucceed");
#endif
            }

            // set widget order
            if (setting.order != 0)
            {
                Canvas orderCanvas = vmGo.GetComponent<Canvas>();
                if (!orderCanvas)
                    orderCanvas = vmGo.gameObject.AddComponent<Canvas>();

                orderCanvas.overrideSorting = true;
                orderCanvas.sortingOrder = setting.order;
            }

            // If not displayed by default
            if (!setting.show)
            {
                widget.gameObject.SetActive(false);
            }
            else
            {
                if (!widget.gameObject.activeSelf)
                    widget.gameObject.SetActive(true);
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWidget<T>() where T : IWidget
        {
            string widgetName = typeof(T).Name;
            return (T)widgets[widgetName];
        }

        /// <summary>Find an opened widget, including widgets that are not cached.</summary>
        /// <typeparam name="T">Requested widget type.</typeparam>
        /// <param name="widget">Opened widget, or null when it is not available.</param>
        /// <returns>True when a live widget of the requested type is on the open stack.</returns>
        public bool TryGetWidget<T>(out T widget) where T : IWidget
        {
            for (int index = currents.Count - 1; index >= 0; index--)
            {
                if (IsWidgetAlive(currents[index]) && currents[index] is T current)
                {
                    widget = current;
                    return true;
                }
            }

            widget = null;
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="complete"></param>
        /// <param name="immediate">跳过关闭动画立即隐藏（强制/流程关闭路径用）</param>
        public void CloseWidget<T>(System.Action complete = null, bool immediate = false) where T : IWidget
        {
            UnloadWidget<T>(complete, immediate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="complete"></param>
        /// <param name="immediate"></param>
        public void CloseWidget(System.Type[] types, System.Action complete = null, bool immediate = false)
        {
            for(int index = 0; index<types.Length; index++)
            {
                CloseWidget(types[index], null, immediate);
            }

            complete?.Invoke();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <param name="complete"></param>
        /// <param name="immediate">跳过关闭动画立即隐藏（强制/流程关闭路径用）</param>
        public void CloseWidget(System.Type type, System.Action complete = null, bool immediate = false)
        {
            var widgetName = type.Name;
            if (widgets.ContainsKey(widgetName))
            {
                var widget = widgets[widgetName];
                if (!IsWidgetAlive(widget))
                {
                    widgets.Remove(widgetName);
                    complete?.Invoke();
                    return;
                }

                RemoveWidget(widget);
                CloseWidget(widget, widgetName, complete, immediate);
            }
            else
            {
                var widget = currents.Find(v => IsWidgetAlive(v) && v.name == widgetName);
                if (widget != null)
                {
                    RemoveWidget(widget);
                    CloseWidget(widget, widgetName, complete, immediate);
                }
            }

        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="complete"></param>
        /// <param name="immediate">跳过关闭动画立即隐藏（强制/流程关闭路径用）</param>
        public void UnloadWidget<T>(System.Action complete = null, bool immediate = false) where T : IWidget
        {
            var widgetName = typeof(T).Name;
            if (widgets.ContainsKey(widgetName))
            {
                var widget = widgets[widgetName];
                if (!IsWidgetAlive(widget))
                {
                    widgets.Remove(widgetName);
                    complete?.Invoke();
                    return;
                }

                RemoveWidget(widget);
                CloseWidget(widget, widgetName, complete, immediate);
            }
            else
            {
                var widget = currents.Find(v => IsWidgetAlive(v) && v.name == widgetName);
                if (widget != null)
                {
                    RemoveWidget(widget);
                    CloseWidget(widget, widgetName, complete, immediate);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="widgetName"></param>
        /// <param name="complete"></param>
        /// <param name="immediate">跳过关闭动画立即隐藏（强制/流程关闭路径用）</param>
        public virtual void CloseWidget(IWidget widget, string widgetName, System.Action complete = null, bool immediate = false)
        {
            if (!IsWidgetAlive(widget))
            {
                widgets.Remove(widgetName);
                complete?.Invoke();
                return;
            }

            OnWidgetClosed?.Invoke(widget);

            int generation = GetCurrentGeneration(widget);

            Animator animator = immediate ? null : widget.gameObject.GetComponent<Animator>();
            if (animator && animator.enabled && animator.runtimeAnimatorController
                && animator.HasState(0, CloseStateHash) && widget.gameObject.activeInHierarchy)
            {
                // 带动画关闭：先锁交互，播 Close 态，播完才真正停用/销毁
                SetWidgetInteraction(widget, false);
                animator.Play(CloseStateHash);
                UICoroutine.Run(WaitCloseAnimation(widget, widgetName, complete, generation));
            }
            else
            {
                widget.Close(() => FinalizeCloseWidget(widget, widgetName, complete, generation));
            }
        }

        /// <summary>
        /// 等待关闭动画播完（带硬超时兜底），再执行真正的停用/销毁收尾。
        /// </summary>
        private IEnumerator WaitCloseAnimation(IWidget widget, string widgetName, System.Action complete, int generation)
        {
            var animator = widget.gameObject.GetComponent<Animator>();

            // 等一帧让 Close 态真正成为当前状态，避免读到旧状态
            yield return null;

            float deadline = Time.unscaledTime + CloseAnimationTimeoutSeconds;
            while (Time.unscaledTime < deadline)
            {
                if (!IsWidgetAlive(widget) || !animator)
                {
                    break;
                }

                var info = animator.GetCurrentAnimatorStateInfo(0);
                if (info.shortNameHash == CloseStateHash && info.normalizedTime >= 1f)
                {
                    break;
                }

                yield return null;
            }

            FinalizeCloseWidget(widget, widgetName, complete, generation);
        }

        /// <summary>
        /// 真正停用或销毁界面。代际不一致说明等待期间界面已被重新打开（或已被另一次关闭收尾）：
        /// 保持打开，仅恢复交互。
        /// </summary>
        private void FinalizeCloseWidget(IWidget widget, string widgetName, System.Action complete, int generation)
        {
            if (!IsWidgetAlive(widget))
            {
                widgets.Remove(widgetName);
                openGenerations.Remove(widget);
                complete?.Invoke();
                return;
            }

            if (openGenerations.TryGetValue(widget, out int current) && current != generation)
            {
                SetWidgetInteraction(widget, true);
                complete?.Invoke();
                return;
            }

            // 收尾前递增代际，使同一界面仍挂着的其它关闭等待全部失效
            openGenerations[widget] = generation + 1;

            ConfigAttribute setting = customModleAttributes[widgetName];
            if (!setting.cache)
            {
                var vm = widget.GetVM();
                if (vm)
                {
                    widget.Unref(vm);
                }

                Destroy(widget.gameObject);

#if UNITY_EDITOR || UNITY_DEVELOP
                Debug.Log($"[UISystem] Destroy widget [{widgetName}]");
#endif
                widgets.Remove(widgetName);
                openGenerations.Remove(widget);
            }
            else
            {
                SetWidgetInteraction(widget, true);
                widget.Deactive();
            }

#if UNITY_EDITOR || UNITY_DEVELOP
            Debug.Log($"[UISystem] Unload widget [{widgetName}]");
#endif
            complete?.Invoke();
        }

        private int GetCurrentGeneration(IWidget widget)
        {
            return openGenerations.TryGetValue(widget, out int generation) ? generation : 0;
        }

        /// <summary>
        /// 关闭动画播放期间屏蔽（或恢复）界面根节点的射线交互。
        /// </summary>
        private static void SetWidgetInteraction(IWidget widget, bool value)
        {
            var group = widget.gameObject.GetComponent<CanvasGroup>();
            if (!group)
            {
                group = widget.gameObject.AddComponent<CanvasGroup>();
            }

            group.interactable = value;
            group.blocksRaycasts = value;
        }

        /// <summary>
        /// Checks whether the widget and its GameObject are still alive.
        /// </summary>
        /// <param name="widget">The widget to check.</param>
        /// <returns>True when the widget can still be safely accessed.</returns>
        private bool IsWidgetAlive(IWidget widget)
        {
            return widget && widget.gameObject;
        }
    }
}
