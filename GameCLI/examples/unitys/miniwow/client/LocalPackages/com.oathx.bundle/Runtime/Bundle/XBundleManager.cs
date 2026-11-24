using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Bundle.Assets
{
#if UNITY_EDITOR
    /// <summary>
    /// asset load info
    /// </summary>
    [Serializable]
    public class AssetLoadFrame
    {
        /// <summary>
        /// method file name
        /// </summary>
        public string fileName
        { get; private set; }

        /// <summary>
        /// method line number
        /// </summary>
        public int lineNumber
        { get; private set; }

        /// <summary>
        /// method column number
        /// </summary>
        public int columnNumber
        { get; private set; }

        /// <summary>
        /// method name
        /// </summary>
        public string methodName
        { get; private set; }

        /// <summary>
        /// class name
        /// </summary>
        public string className
        { get; private set; }

        /// <summary>
        /// asset load info constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <param name="method"></param>
        /// <param name="classTypeName"></param>
        public AssetLoadFrame(string name, int line, int column, string typeName, string method)
        {
            fileName = name;
            lineNumber = line;
            columnNumber = column;
            methodName = method;
            className = typeName;
        }
    }

    [Serializable]
    public class AssetLoadInfo
    {
        /// <summary>
        /// bundle info
        /// </summary>
        public XBundle bundleInfo
        { get; private set; }

        /// <summary>
        /// load frames
        /// </summary>
        public int usedCount
        { get; private set; } = 1;

        /// <summary>
        /// last read time
        /// </summary>
        public float lastReadTime
        { get; private set; }

        /// <summary>
        /// loading time
        /// </summary>
        public float loadingTime
        { get; private set; }

        /// <summary>
        /// asset path
        /// </summary>
        public string path
        { get; private set; }

        /// <summary>
        /// asset name
        /// </summary>
        public string name
        { get; private set; }

        /// <summary>
        /// asset type
        /// </summary>
        public string type
        { get; private set; }

        /// <summary>
        /// asset size
        /// </summary>
        public long size
        { get; private set; }

        /// <summary>
        /// load frames
        /// </summary>
        public List<AssetLoadFrame> frames
        { get; private set; }

        /// <summary>
        /// asset load info constructor
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="assetPath"></param>
        /// <param name="assetName"></param>
        /// <param name="assetType"></param>
        /// <param name="loadTime"></param>
        public AssetLoadInfo(XBundle bundle, string assetPath, string assetName, string assetType, long sizeOnDisk, List<AssetLoadFrame> loadFrames)
        {
            bundleInfo = bundle;
            path = assetPath;
            name = assetName;
            type = assetType;
            size = sizeOnDisk;
            loadingTime = 0;
            usedCount = 0;
            lastReadTime = 0f;
            frames = loadFrames;
        }

        /// <summary>
        /// add use count
        /// </summary>
        /// <param name="readTime"></param>
        public int Used()
        {
            usedCount++;
            return usedCount;
        }

        /// <summary>
        /// unuse count
        /// </summary>
        public void Unused()
        {
            usedCount--;
        }

        /// <summary>
        /// update time
        /// </summary>
        /// <param name="readTime"></param>
        /// <param name="loadTime"></param>
        public void UpdateTime(float readTime, float loadTime)
        {
            lastReadTime = readTime;
            loadingTime = loadingTime;
        }
    }

    /// <summary>
    /// asset load info handler
    /// </summary>
    /// <param name="info"></param>
    public delegate void AssetLoadInfoHandler(AssetLoadInfo info);
    /// <summary>
    /// bundle load info handler
    /// </summary>
    /// <param name="bundle"></param>
    public delegate void BundleLoadInfoHandler(XBundle bundle);

    /// <summary>
    /// bundle analysis
    /// </summary>
    public static class BundleAnalysis
    {
        /// <summary>
        /// load infos
        /// </summary>
        static readonly Dictionary<string, AssetLoadInfo> 
            loadInfos = new Dictionary<string, AssetLoadInfo>();

        /// <summary>
        /// float queue
        /// </summary>
        public static event Action<AssetLoadInfo> assetLoadHander;
        /// <summary>
        /// bundle load handler
        /// </summary>
        public static event BundleLoadInfoHandler bundleLoadHander;

        /// <summary>
        /// get method infos from stack frames
        /// </summary>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static List<AssetLoadFrame> GetLoadFrames(System.Diagnostics.StackFrame[] frames)
        {
            List<AssetLoadFrame> methods = new List<AssetLoadFrame>();

            int depth = 0;
            int total = 0;
            while (total < frames.Length)
            {
                var frame = frames[depth];
                if (frame != null)
                {
                    var fileName = frame.GetFileName();
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var method = frame.GetMethod();

                        var loadInfo = new AssetLoadFrame(fileName, frame.GetFileLineNumber(), frame.GetFileColumnNumber(),
                            method.DeclaringType.Name, method.Name);
                        methods.Add(loadInfo);
                    }
                }

                total++;
                depth++;
            }

            return methods;
        }

        /// <summary>
        /// add asset load info
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="loadingTime"></param>
        /// <param name="readTime"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static bool AddAssetLoadInfo(XBundle bundle, string name, string path, string type, long size, 
            float loadingTime, float readTime, System.Diagnostics.StackFrame[] frames)
        {
            var loadFrames = GetLoadFrames(frames);
            if (!loadInfos.TryGetValue(path, out var loadInfo))
            {
                loadInfo = new AssetLoadInfo(bundle,
                    path, name, type, size, loadFrames);
                loadInfos.Add(path, loadInfo);
            }

            // update load info
            loadInfo.UpdateTime(readTime, loadingTime);
            loadInfo.Used();

            assetLoadHander?.Invoke(loadInfo);
            return true;
        }

        /// <summary>
        /// get all load infos
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, AssetLoadInfo> GetAllLoadInfos()
        {
            return loadInfos;
        }

        /// <summary>
        /// notify bundle load
        /// </summary>
        /// <param name="bundle"></param>
        public static void NotifyBundleLoad(XBundle bundle)
        {
            bundleLoadHander?.Invoke(bundle);
        }
    }
#endif

    /// <summary>
    /// bundle source type
    /// </summary>
    public enum XBundleSource
    {
        Local,
        Bundle,
    }

    /// <summary>
    /// bundle manager
    /// </summary>
    public class XBundleManager
    {
        /// <summary>
        /// all bundle info
        /// </summary>
        protected readonly Dictionary<string, XBundle> bundles = new Dictionary<string, XBundle>();

        /// <summary>
        /// all asset info
        /// </summary>
        protected readonly List<XBundle> allInfo = new List<XBundle>();

        /// <summary>
        /// 
        /// </summary>
        protected XBundleSheet bundleSheet = new XBundleSheet();

        /// <summary>
        /// auto release bundle time
        /// </summary>
        protected float autoReleaseTime = 0.5f;

        /// <summary>
        /// inti bundle info
        /// </summary>
        /// <returns></returns>
        public bool Initialize(string jsonText, float autoReleaseCacheTime, Action<string> complete)
        {
            autoReleaseTime = autoReleaseCacheTime;

            bool checkOk = bundleSheet.Initialize(jsonText, complete);
            if (!checkOk)
                return false;
            
            var entrys = bundleSheet.GetEntrys();
            return Initialize(entrys);
        }

        /// <summary>
        /// init bundle info
        /// </summary>
        /// <param name="entrys"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool Initialize(Dictionary<string, Entry> entrys)
        {
            foreach(var bundle in bundles)
            {
                XBundle assetBundle = bundle.Value;
                if (assetBundle.isDone)
                    assetBundle.bundle.Unload(false);
            }

            bundles.Clear();
            allInfo.Clear();

            foreach (KeyValuePair<string, Entry> item in entrys)
            {
                XBundle info = new XBundle(item.Value, autoReleaseTime);
                bundles.Add(item.Key, info);

#if UNITY_EDITOR
                Debug.Log($"[AssetManager.Initialize] Add bundle {item.Key}");
#endif
                allInfo.Add(info);
            }

            Resources.UnloadUnusedAssets();

#if UNITY_EDITOR
            Debug.Log($"[AssetManager.Initialize] Succeed");
#endif
            return true;
        }

        /// <summary>
        /// shutdown bundle manager
        /// </summary>
        public void Shutdown()
        {
            ResetBundleSheet();
            ReleaseBundle();

            GC();
        }

        /// <summary>
        /// force gc collect
        /// </summary>
        public void GC()
        {
            Resources.UnloadUnusedAssets();

            // force gc
            System.GC.Collect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assetInfo"></param>
        /// <returns></returns>
        public bool TryGetAssetInfo(string name, out AssetInfo assetInfo)
        {
            return bundleSheet.TryGetAssetInfo(name, out assetInfo);
        }

        /// <summary>
        /// update bundle manager, auto release unused bundle
        /// </summary>
        public void Update()
        {
            if (autoReleaseTime >= 0)
                ReleaseBundle();
        }

        /// <summary>
        /// release unused bundle
        /// </summary>
        public void ReleaseBundle()
        {
            float now = Time.realtimeSinceStartup;
            for (int i = 0; i < allInfo.Count; i++)
            {
                XBundle info = allInfo[i];
                if (info.isDone && info.Unused(now))
                {
#if UNITY_EDITOR
                    if (info.Ref() < 0)
                        Debug.LogError($"[XAssetManager.ReleaseBundle] bundle {info.entry.name} refCount({info.Ref()}) incorrect");
#endif

                    info.ResetRef();

                    // 注意：不在此处注销依赖登记。包只是闲置（壳还在、资产仍可能被加载），
                    // 提前注销会让依赖包被误判为无人使用而卸载，导致依赖包反复卸载/重载。
                }
            }

            for (int i = 0; i < allInfo.Count; i++)
            {
                XBundle info = allInfo[i];
                if (info.isDone && info.Unused(now))
                {
                    if (info.DependenciesCount() == 0)
                    {
                        info.bundle.Unload(false);
                        info.bundle = null;
                        info.isDone = false;

                        // 包真正卸载时才注销它对各依赖包的登记：
                        // 从此没有任何存活包需要这些依赖，依赖包下一轮即可被正常释放。
                        for (int depIndex = 0; depIndex < info.entry.dependencies.Length; depIndex++)
                        {
                            string dependence = info.entry.dependencies[depIndex];
                            if (TryGetAssetBundle(dependence, out var dependenceInfo))
                                dependenceInfo.RemoveDependencies(info.entry.name);
                        }
#if UNITY_EDITOR
                        Debug.Log($"[AssetManager.ReleaseBundle] bundle {info.entry.name} unloaded ref:{info.Ref()}");
#endif
                    }
                }
            }
        }

        /// <summary>
        /// try get bundle info
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetBundle"></param>
        /// <returns></returns>
        public bool TryGetAssetBundle(string bundleName, out XBundle assetBundle)
        {
            if (bundles.TryGetValue(bundleName, out assetBundle))
                return true;

            return false;
        }

        /// <summary>
        /// get all bundle info
        /// </summary>
        /// <returns></returns>
        public List<XBundle> GetAllAssetBundles()
        {
            return allInfo;
        }

        /// <summary>
        /// load entry dependencies
        /// </summary>
        /// <param name="entiry"></param>
        /// <returns></returns>
        private IEnumerator LoadDependenciesAsync(Entry entiry)
        {
            // StartCoroutine 不会立即执行协程体（首个 MoveNext 要等下一帧），
            // 因此不能用 isLoading 判断依赖是否已开始加载——必须持有协程句柄逐个 yield 等待真正完成，
            // 否则本包会抢在依赖包加载完成前就绪，跨包引用（如 AnimatorController）解析为 null。
            var pending = new List<Coroutine>();
            for (int i = 0; i < entiry.dependencies.Length; i++)
            {
                if (TryGetAssetBundle(entiry.dependencies[i], out var bundle))
                {
                    if (!bundle.isDone)
                        pending.Add(XBundleCoroutine.Run(LoadAssetBundleAsync(bundle, false)));

                    bundle.AddDependencies(entiry.name);
                }
            }

            for (int i = 0; i < pending.Count; i++)
            {
                yield return pending[i];
            }
        }

        /// <summary>
        /// internal load asset bundle
        /// </summary>
        /// <param name="bundleInfo"></param>
        /// <param name="loadDependence"></param>
        /// <returns></returns>
        public IEnumerator LoadAssetBundleAsync(XBundle bundleInfo, bool loadDependence = true)
        {
            while (bundleInfo.isLoading)
                yield return null;

            if (bundleInfo.isDone)
                yield break;

#if UNITY_EDITOR
            Debug.Log($"[XBundleManager.LoadAssetBundleAsync] {bundleInfo.url}");
#endif

            bundleInfo.StartLoad();
            bundleInfo.isLoading = true;
            if (loadDependence)
                yield return XBundleCoroutine.Run(LoadDependenciesAsync(bundleInfo.entry));

            string bundleName = bundleInfo.entry.name;
            AssetBundleCreateRequest loader = AssetBundle.LoadFromFileAsync(bundleInfo.url);
            yield return loader;
            bundleInfo.LoadDone();

            if (!loader.isDone)
            {
                bundleInfo.isLoading = false;

                Debug.LogError($"[XBundleManager.LoadAssetBundleAsync] can't async load bundle {bundleInfo.url} reason: NOT FOUND");
            }
            else
            {
                bundleInfo.isLoading = false;
                if (loader.assetBundle != null)
                {
                    bundleInfo.bundle = loader.assetBundle;
                    bundleInfo.isDone = true;
#if UNITY_EDITOR
                    Debug.Log($"[XBundleManager.LoadAssetBundleAsync] async load bundle {bundleInfo.url} {bundleInfo.startLoadingTime} succeed");
#endif
                }
                else
                {
                    Debug.LogError($"[XBundleManager.LoadAssetBundle] can't async load bundle: {bundleInfo.url}");
                }
            }
        }

        /// <summary>
        /// load asset in bundle
        /// </summary>
        /// <param name="info"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Coroutine LoadAssetAsync(AssetInfo info, Type type, Action<UnityEngine.Object> callback)
        {
            return XBundleCoroutine.Run(DoLoadAssetAsync(info, type, callback));
        }

        /// <summary>
        /// do load asset in bundle
        /// </summary>
        /// <param name="info"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator DoLoadAssetAsync(AssetInfo info, Type type, Action<UnityEngine.Object> callback)
        {
#if UNITY_EDITOR
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.StackFrame[] frames = stackTrace.GetFrames();
#endif

            if (TryGetAssetBundle(info.name, out var assetBundleInfo))
            {
                if (!assetBundleInfo.isDone)
                {
                    yield return XBundleCoroutine.Run(LoadAssetBundleAsync(assetBundleInfo));
                }
                else
                {
                    // bundle 已加载不代表依赖链还在：自动释放只按引用计数卸载依赖包，
                    // 本包存活期间依赖可能已被卸载，资产加载前必须重新确认依赖链就绪，
                    // 否则跨包引用（如 AnimatorController）会解析为 null。
                    // 等待依赖期间持有引用计数，防止本包自身被自动释放卸载导致加载中断。
                    assetBundleInfo.IncRef(Time.realtimeSinceStartup);
                    yield return XBundleCoroutine.Run(LoadDependenciesAsync(assetBundleInfo.entry));
                    assetBundleInfo.DecRef();
                }
            }

            if (assetBundleInfo.isDone)
            {
                float loadingStartTime = Time.realtimeSinceStartup;
                assetBundleInfo.IncRef(loadingStartTime);

                AssetBundleRequest request = assetBundleInfo.bundle.LoadAssetAsync(info.path, type);
                yield return request;

                assetBundleInfo.DecRef();

                // load error
                UnityEngine.Object asset = request.asset;
                if (!asset)
                    Debug.LogError($"load asset {info.path} failure");

                callback?.Invoke(asset);
            }
        }

        /// <summary>
        /// load scene in bundle
        /// </summary>
        /// <param name="assetInfo"></param>
        /// <returns></returns>
        public Coroutine LoadSceneAsync(AssetInfo assetInfo)
        {
            return XBundleCoroutine.Run(DoLoadSceneAsync(assetInfo));
        }

        /// <summary>
        /// do load scene in bundle
        /// </summary>
        /// <param name="assetInfo"></param>
        /// <returns></returns>
        private IEnumerator DoLoadSceneAsync(AssetInfo assetInfo)
        {
            if (!TryGetAssetBundle(assetInfo.name, out var bundleInfo))
                throw new NullReferenceException(assetInfo.name);

            if (!bundleInfo.isDone)
            {
                yield return XBundleCoroutine.Run(LoadAssetBundleAsync(bundleInfo));
            }
            else
            {
                // 与资产加载同理：bundle 已加载时依赖包可能已被自动释放卸载，需重新确认依赖链就绪；
                // 等待依赖期间持有引用计数，防止本包自身被自动释放卸载。
                bundleInfo.IncRef(Time.realtimeSinceStartup);
                yield return XBundleCoroutine.Run(LoadDependenciesAsync(bundleInfo.entry));
                bundleInfo.DecRef();
            }

            if (bundleInfo.isDone)
            {
                bundleInfo.IncRef(Time.realtimeSinceStartup);
                // sheet 注册键统一小写，而 AssetBundle 内场景路径保留原始大小写且按大小写敏感匹配，
                // 必须换回 bundle 内真实路径再交给 SceneManager，否则场景加载静默失败
                var scenePath = assetInfo.path;
                foreach (var path in bundleInfo.bundle.GetAllScenePaths())
                {
                    if (path.ToLower() == assetInfo.path)
                    {
                        scenePath = path;
                        break;
                    }
                }

                yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenePath);
                bundleInfo.DecRef();
            }
        }

        /// <summary>
        /// do load asset bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private IEnumerator DoLoadAsyncAssetBundle(string bundleName, Action<AssetBundle> result)
        {
            if (!TryGetAssetBundle(bundleName, out var bundleInfo))
                throw new NullReferenceException(bundleName);

            if (!bundleInfo.isDone)
                yield return XBundleCoroutine.Run(LoadAssetBundleAsync(bundleInfo));

            bundleInfo.IncRef(Time.realtimeSinceStartup);
            result?.Invoke(bundleInfo.bundle);
            bundleInfo.DecRef();
        }

        /// <summary>
        /// do load asset bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public Coroutine LoadAsyncAssetBundle(string bundleName, Action<AssetBundle> result)
        {
            return XBundleCoroutine.Run(DoLoadAsyncAssetBundle(bundleName, result));
        }

        /// <summary>
        /// reset bundle sheet
        /// </summary>
        public void ResetBundleSheet()
        {
            bundleSheet.Clearup();
        }

        /// <summary>
        /// get current bundle sheet
        /// </summary>
        /// <returns></returns>
        public XBundleSheet GetBundleSheet()
        {
            return bundleSheet;
        }
    }
}
