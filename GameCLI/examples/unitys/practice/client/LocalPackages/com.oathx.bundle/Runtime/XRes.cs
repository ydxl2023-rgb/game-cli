using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Oathx.Bundle.Assets
{
    /// <summary>
    /// hotfix status
    /// </summary>
    public enum HotfixStatus
    {
        /// <summary>
        /// fetch remote bundle sheet file, get version number succeed
        /// </summary>
        FetchVersionSucceed,
        /// <summary>
        /// fetch remote bundle sheet file, get version number failure
        /// </summary>
        FetchVersionFailure,
        /// <summary>
        /// init app version failure
        /// </summary>
        InitVersionFailure,
        /// <summary>
        /// init app version succeed
        /// </summary>
        InitVersionSucceed,
        /// <summary>
        /// notify hotfix download progress
        /// </summary>
        DownloadProgress,
        /// <summary>
        /// download hotfix entry failure
        /// </summary>
        DownloadFailure,
        /// <summary>
        /// download all entrys succeed
        /// </summary>
        DownloadSucceed,
        /// <summary>
        /// app hotfix succeed
        /// </summary>
        HotfixSucceed,
        /// <summary>
        /// app hotfix failure
        /// </summary>
        HotfixFailure,
        /// <summary>
        /// current app need force update, exit and open force url
        /// </summary>
        ForeceUpdate,
        /// <summary>
        /// hotfix update start
        /// </summary>
        HotfixBundles
    }

    /// <summary>
    /// resource manager
    /// </summary>
    public static class XRes
    {
        static readonly XBundleManager bundleManager = new XBundleManager();

        /// <summary>
        /// current local version
        /// </summary>
        static XHotfixVersion localVersion
        { get; set; }
        /// <summary>
        /// remote version
        /// </summary>
        static XHotfixVersion remoteVersion
        { get; set; }

        /// <summary>
        /// current remote bundle sheet info
        /// </summary>
        static BundleSheetInfo remoteSheet
        { get; set; }

        /// <summary>
        /// remote config object, you must be impl interface or use default remote config [DefaultRemoteConfig]
        /// </summary>
        static XHotfixConfig config;
        static XBundleSource source;
        static IAssetsCacher cacher;

        /// <summary>
        /// do hotfix update
        /// </summary>
        /// <param name="url"></param>
        /// <param name="hotfixConfig"></param>
        /// <param name="progress"></param>
        /// <param name="window"></param>
        /// <param name="assetsCacher"></param>
        /// <returns></returns>
        public static IEnumerator DoUpdate(XHotfixVersion remoteVersion, System.Action<HotfixStatus, float, string> progress, IHotfixWindow window=null, IAssetsCacher assetsCacher=null)
        {
            // init current local version
            yield return DoInitVersion(remoteVersion, progress);

            // check init version succeed
            if (!CheckInitVersionSucceed())
            {
                progress?.Invoke(HotfixStatus.InitVersionFailure, 0f, localVersion.ToString());
                yield break;
            }
            else
            {
                // compare local version and remote version
                var flag = CompareVersion(localVersion, remoteVersion);
                if (flag == UpdateFlag.App)
                {
                    // if need force update app
                    DoUpdateApp(progress);
                }
                else
                {
                    // update asset bundles
                    if (flag == UpdateFlag.AssetBundles)
                    {
                        yield return DoUpdateAssetBundles(progress, window);
                    }
                    else
                    {
                        // nothing to update
                        progress?.Invoke(HotfixStatus.HotfixSucceed,
                            1.0f, remoteVersion.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// init current local version and fetch remote version
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        static IEnumerator DoInitVersion(XHotfixVersion remoteAppVersion, System.Action<HotfixStatus, float, string> progress)
        {
            // init remote version
            remoteVersion = remoteAppVersion;

#if UNITY_EDITOR
            // if enable debug, clear all local data
            if (config.debugEnabled)
                PlayerPrefs.DeleteAll();
#endif

            var packageVersion = config.GetPackageVersion();

            // read local version
            var localVersionCode = ReadLocalVersion();
            if (string.IsNullOrEmpty(localVersionCode))
                localVersionCode = packageVersion.ToString();

            if (XHotfixVersion.TryParse(localVersionCode, out XHotfixVersion version))
            {
                localVersion = version;

#if UNITY_EDITOR
                if (config.debugEnabled)
                {
                    localVersion.resVersion = System.DateTime.Now.Millisecond;
                    localVersion.srcVersion = System.DateTime.Now.Hour;
                }
#endif
                // init local version succeed
                progress?.Invoke(HotfixStatus.InitVersionSucceed,
                    0, localVersion.ToString());
            }
                
#if UNITY_HOTFIXLOG
            Debug.Log("[XRes.DoInitVersion] current version " + localVersion.ToString());
#endif
            yield return DoInitRemoteSheet(progress);
        }

        /// <summary>
        /// do init remote sheet and set hotfix cdn url
        /// </summary>
        /// <returns></returns>
        static IEnumerator DoInitRemoteSheet(System.Action<HotfixStatus, float, string> progress)
        {
            var cdns = remoteVersion.cdns ?? new string[0] { };

            // fetch remote bundle sheet
            foreach (var cdn in cdns)
            {
                var sheetUrl = config.GetBundleSheetURL(cdn, remoteVersion);
#if UNITY_HOTFIXLOG
                Debug.Log($"[XRes.DoInitRemoteSheet] {sheetUrl}");
#endif
                UnityWebRequest reqSheet = UnityWebRequest.Get(sheetUrl);
                yield return reqSheet.SendWebRequest();

                if (reqSheet.result == UnityWebRequest.Result.Success)
                {
                    config.SetCDNUrl(cdn);
#if UNITY_HOTFIXLOG
                    Debug.Log("[XRes.FeatchRemoteVersion] set CDN url " + cdn);
#endif
                    // parse remote bundle sheet text, get version number
                    if (BundleSheetInfo.TryParse(reqSheet.downloadHandler.text, out var sheet))
                    {
                        remoteSheet = sheet;
                        break;
                    }
                }
                else
                {
                    Debug.LogError($"Can't find cdn url {sheetUrl}");
                }
            }

            if (!remoteSheet)
            {
                progress?.Invoke(HotfixStatus.InitVersionFailure,
                            1.0f, remoteVersion.ToString());
                yield break;
            }
        }

        /// <summary>
        /// init resource manager
        /// </summary>
        /// <param name="bundleSource"></param>
        /// <param name="autoReleaseTime"></param>
        /// <param name="remoteConfig"></param>
        /// <param name="assetsCacher"></param>
        /// <returns></returns>
        public static bool Initialize(XBundleSource bundleSource, float autoReleaseTime,
            XHotfixConfig remoteConfig, IAssetsCacher assetsCacher=null)
        {
            config = remoteConfig;
            source = bundleSource;
            cacher = assetsCacher;

#if UNITY_EDITOR
            if (bundleSource == XBundleSource.Local)
                return InitLocalSourceFromDirectory();
#endif

            string text = config.GetPackageBundleSheet();
            return bundleManager.Initialize(text, autoReleaseTime, (versionStr)=> {

            });
        }

        /// <summary>
        /// init local source
        /// </summary>
        /// <returns></returns>
        private static bool InitLocalSourceFromDirectory()
        {
            var sheet = bundleManager.GetBundleSheet();
            sheet.Clearup();

            var resources = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in resources)
            {
                var ext = Path.GetExtension(file);
                if (IsValid(ext))
                {
                    var path = file.Replace("\\", "/").ToLower().Substring(Application.dataPath.Length - 6);   
                    sheet.AddAssetPath(path, new AssetInfo() {
                        path = path, locationType = LocationType.Resource, name = path
                    });   
                }
            }

            return true;
        }

        /// <summary>
        /// check file valid
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        private static bool IsValid(string ext)
        {
            return ext != ".meta" && ext != ".manifest" && ext != ".cs";
        }

        /// <summary>
        /// update bundle manager, start auto release unused bundle
        /// </summary>
        public static void Update()
        {
            bundleManager.Update();
        }

        /// <summary>
        /// shutdown close resource mananger, free all assets
        /// </summary>
        public static void Shutdown()
        {
            bundleManager.Shutdown();
        }

        /// <summary>
        /// get current version
        /// </summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            if (remoteVersion)
                return remoteVersion.ToString();

            if (localVersion)
                return localVersion.ToString();

            var version = config.GetPackageVersion();
            return version.ToString();
        }

        /// <summary>
        /// Loads the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="path">Path.</param>
        /// <param name="callback">Callback.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static Coroutine LoadAsync<T>(string path, System.Action<Object> callback) where T : Object
        {
            return LoadAsync(path, typeof(T), callback);
        }

        /// <summary>
        /// Loads the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="path">Path.</param>
        /// <param name="type">Type.</param>
        /// <param name="callback">Callback.</param>
        public static Coroutine LoadAsync(string path, System.Type type, System.Action<Object> callback)
        {
            return XBundleCoroutine.Run(DoLoadAsync(path, type, callback));
        }

        /// <summary>
        /// load mutli assets
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paths"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Coroutine LoadMultiAsync<T>(string[] paths, System.Action<T[]> callback) where T : Object
        {
            return XBundleCoroutine.Run(DoLoadMutliAsync(paths, callback));
        }

        /// <summary>
        /// load mutli assets
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Coroutine LoadMutliAsync(string[] paths, System.Action<Object[]> callback)
        {
            return XBundleCoroutine.Run(DoLoadMutliAsync(paths, callback));
        }

        /// <summary>
        /// async load scene
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Coroutine LoadSceneAsync(string name, System.Action<Scene> callback)
        {
            return XBundleCoroutine.Run(DoLoadSceneAsync(name, callback));
        }

        /// <summary>
        /// do load scene
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        static IEnumerator DoLoadSceneAsync(string name, System.Action<Scene> callback)
        {
#if UNITY_EDITOR
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.StackFrame[] frames = stackTrace.GetFrames();

            float loadingStartTime = Time.realtimeSinceStartup;
#endif

            var oldAcitveScene = SceneManager.GetActiveScene();
            if (!bundleManager.TryGetAssetInfo(name.ToLower(), out var assetInfo))
            {
                Debug.LogError($"[XRes.DoLoadSceneAsync] Load scene failure, Can't find scene {name}");

                callback?.Invoke(oldAcitveScene);
                yield break;
            }
            else
            {
                switch (source)
                {
                    case XBundleSource.Local:
                        yield return SceneManager.LoadSceneAsync(assetInfo.path);
                        break;

                    case XBundleSource.Bundle:
                        yield return bundleManager.LoadSceneAsync(assetInfo);
                        break;
                }

                yield return new WaitForEndOfFrame();

                // force gc and unload all unused assets
                System.GC.Collect();
                Resources.UnloadUnusedAssets();

                var newScene = SceneManager.GetActiveScene();
                callback?.Invoke(newScene);

#if UNITY_EDITOR
                if (bundleManager.TryGetAssetBundle(assetInfo.name, out var assetBundleInfo))
                {
                    float loadingTime = Time.realtimeSinceStartup - loadingStartTime;
                    BundleAnalysis.AddAssetLoadInfo(assetBundleInfo,
                        assetInfo.name, name, newScene.GetType().Name, assetInfo.size, loadingTime, loadingStartTime, frames);
                }
#endif

                Debug.Log($"[XRes.DoLoadSceneAsync] Load scene {newScene.name} succeed");
            }
        }

        /// <summary>
        /// do load mutli assets
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paths"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private static IEnumerator DoLoadMutliAsync<T>(string[] paths,
            System.Action<T[]> callback) where T : Object
        {
            int mutliln = paths.Length;
            T[] objects = new T[paths.Length];
            for (int index = 0; index < paths.Length; index++)
            {
                LoadAsync(paths[index], typeof(T), (obj, idx) => {
                    objects[idx] = (T)obj;
                    mutliln--;
                }, index);
            }

            while (mutliln > 0)
                yield return null;

            callback?.Invoke(objects);
        }

        /// <summary>
        /// load asset, need setting param
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        static Coroutine LoadAsync(string path,
            System.Type type, System.Action<Object, int> callback, int param)
        {
            return XBundleCoroutine.Run(DoLoadAsync(path, type, (obj) => {
                callback(obj, param);
            }));
        }

        /// <summary>
        /// do load bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        static IEnumerator DoLoadAssetBundle(string bundleName, System.Action<AssetBundle> callback)
        {
            string filepath = bundleName.ToLower();
            if (bundleManager.TryGetAssetBundle(filepath, out var bundleInfo))
            {
                if (!bundleInfo.isDone)
                    yield return bundleManager.LoadAssetBundleAsync(bundleInfo);

                if (bundleInfo.isDone)
                {
                    bundleInfo.IncRef(Time.realtimeSinceStartup);
                    callback?.Invoke(bundleInfo.bundle);
                    bundleInfo.DecRef();
                }
            }
            else
            {
                Debug.LogError($"Can't find assetbundle {filepath}");
            }
        }

        /// <summary>
        /// load a bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Coroutine LoadBundleAsync(string bundleName, System.Action<AssetBundle> callback)
        {
            return XBundleCoroutine.Run(DoLoadAssetBundle(bundleName, callback));
        }

        /// <summary>
        /// do load bundle
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        static IEnumerator DoLoadAsync(string path, System.Type type, System.Action<Object> callback)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[XRes::Load] sent empty/null path!");
                yield break;
            }

#if UNITY_EDITOR
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.StackFrame[] frames = stackTrace.GetFrames();
#endif

            float loadingStartTime = Time.realtimeSinceStartup;
            string assetName = path.ToLower();
            Object resultObj = null;

            if (bundleManager.TryGetAssetInfo(assetName, out var assetInfo))
            {
                if (source == XBundleSource.Bundle)
                {
                    switch (assetInfo.locationType)
                    {
                        // in resource dirtory, must be remove ext
                        case LocationType.Resource:
                            var resname = assetInfo.path;
                            if (Path.HasExtension(path))
                                resname = Path.ChangeExtension(path, null);

                            ResourceRequest req = Resources.LoadAsync(resname, type);
                            yield return req;

                            resultObj = req.asset;
                            break;

                            // load asset from bundle, the asset is native package
                        case LocationType.Bundle:
                            yield return bundleManager.LoadAssetAsync(assetInfo,
                                type, delegate (Object result) { resultObj = result; });
                            break;

                            // load asset from remote, the asset is hotfix asset
                        case LocationType.Remote:
                            string fileUrl = GetLocalFileUrl(Path.Combine(Application.persistentDataPath, assetInfo.name));

                            UnityWebRequest fileLoader = UnityWebRequest.Get(fileUrl);
                            yield return fileLoader.SendWebRequest();

                            if (fileLoader.result == UnityWebRequest.Result.Success)
                                resultObj = GetRequestResult(fileLoader, type);
                            break;
                    }

                    if (!resultObj)
                        Debug.LogError($"[XRes.Load] Can't find {assetName} path {assetInfo.path} in AssetInfo {assetInfo.locationType}");
                }
                else
                {
#if UNITY_EDITOR
                    resultObj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetInfo.path, type);
                    yield return new WaitForEndOfFrame();
#else
                    throw new System.NotSupportedException($"Can't load resource {assetInfo.path}");
#endif
                }
            }

            if (resultObj)
            {
                if (cacher != null)
                    cacher.Add(assetInfo.path, resultObj);

#if UNITY_EDITOR
                bundleManager.TryGetAssetBundle(assetInfo.name, out var assetBundleInfo);    
                
                float loadingTime = Time.realtimeSinceStartup - loadingStartTime;
                BundleAnalysis.AddAssetLoadInfo(assetBundleInfo,
                    assetInfo.name, assetName, type.Name, assetInfo.size, loadingTime, loadingStartTime, frames);
#endif
            }
            else
            {
                Debug.LogError($"[XRes.Load] Can't find {assetName}");
            }

            callback?.Invoke(resultObj);
        }

        /// <summary>
        /// get local file url
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetLocalFileUrl(string path)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "file:///" + path;
                default:
                    return "file://" + path;
            }
        }

        /// <summary>
        /// get request asset result
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Object GetRequestResult(UnityWebRequest request, System.Type type)
        {
            XBundleAssetBuffer asset = ScriptableObject.CreateInstance<XBundleAssetBuffer>();
            asset.Init(request.downloadHandler.data);

            if (type == typeof(Texture2D))
            {
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(asset.bytes);

                return texture;
            }

            return asset;
        }

        /// <summary>
        /// clear bundle sheet
        /// </summary>
        public static void ResetBundleSheet()
        {
            var sheet = bundleManager.GetBundleSheet();
            if (sheet != null)
                sheet.Clearup();
        }

        /// <summary>
        /// add bundle entry
        /// </summary>
        /// <param name="entry"></param>
        public static void AddEntry(Entry entry)
        {
            var sheet = bundleManager.GetBundleSheet();
            if (sheet != null)
                sheet.AddEntry(entry);
        }

        /// <summary>
        /// remove bundle entry
        /// </summary>
        /// <param name="entry"></param>
        public static void RemoveEntry(Entry entry)
        {
            var sheet = bundleManager.GetBundleSheet();
            if (sheet != null)
                sheet.RemoveEntry(entry);
        }

        /// <summary>
        /// save bundle entry sheet file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pretty"></param>
        /// <param name="version"></param>
        public static void Save(string path, bool pretty = true, string version = "")
        {
            var sheet = bundleManager.GetBundleSheet();
            if (sheet != null)
                sheet.Save(path, pretty, version);
        }

        /// <summary>
        /// check entry exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Exists(string name)
        {
            var sheet = bundleManager.GetBundleSheet();
            if (sheet != null)
                return sheet.Exists(name);

            return false;
        }

        /// <summary>
        /// get all bundles 
        /// </summary>
        /// <returns></returns>
        public static List<XBundle> GetAllAssetBundles()
        {
            return bundleManager.GetAllAssetBundles();
        }

#region HOTFIX

        /// <summary>
        /// check hotfix condition
        /// </summary>
        /// <returns></returns>
        static bool CheckInitVersionSucceed()
        {
            return remoteVersion && localVersion;
        }

        /// <summary>
        /// do force update app
        /// </summary>
        /// <param name="progress"></param>
        static void DoUpdateApp(System.Action<HotfixStatus, float, string> progress)
        {
            switch(Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.Android:
                    break;

                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.IPhonePlayer:
                    break;
            }

            progress?.Invoke(HotfixStatus.ForeceUpdate, 0f, remoteVersion.ToString());
        }

        /// <summary>
        /// hotfix update flag
        /// </summary>
        enum UpdateFlag
        {
            None,
            App,
            AssetBundles,
        }

        /// <summary>
        /// compre local version and remote version
        /// </summary>
        /// <param name="local"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        static UpdateFlag CompareVersion(XHotfixVersion local, XHotfixVersion remote)
        {
            if (local.appVersion != remote.appVersion || local.aotVersion != remote.aotVersion)
                return UpdateFlag.App;

            if (local.srcVersion != remote.srcVersion || 
                local.resVersion != remote.resVersion)
            {
                return UpdateFlag.AssetBundles;
            }
                
            return UpdateFlag.None;
        }

        /// <summary>
        /// start update asset bundles
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        static IEnumerator DoUpdateAssetBundles(System.Action<HotfixStatus, float, string> progress, IHotfixWindow window)
        {
            // if remote sheet is null, fetch remote sheet failure
            if (!remoteSheet)
            {
                progress?.Invoke(HotfixStatus.HotfixFailure,
                    0f, remoteVersion.ToString());
                yield break;
            }

            // diff bundle sheet, get need download entrys
            var entryDiffs = bundleManager.GetBundleSheet().GenerateSheetDiff(remoteSheet);
            yield return DownloadEntrys(entryDiffs, progress, window);
        }

        /// <summary>
        /// do download entrys
        /// </summary>
        /// <param name="entryDiffs"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        static IEnumerator DownloadEntrys(Queue<EntryDiff> entryDiffs, 
            System.Action<HotfixStatus, float, string> progress, IHotfixWindow window)
        {
            const int maxDownloadAttempts = 3;
            const float initialRetryDelaySeconds = 1f;

            long totalBytes = CalcDownloadSize(entryDiffs);

#if UNITY_HOTFIXLOG
            Debug.Log($"[XRes.DownloadEntrys] entryDiffs count {entryDiffs.Count} size {totalBytes}");
#endif

            float totalMegabyte = totalBytes / (float)(1024 * 1024);
            if (window != null)
                yield return OpenDownloadSize(window, totalMegabyte);

            long downloadedBytes = 0;
            long entrySize = 0;

            float lastNodeTime = Time.realtimeSinceStartup;
            float timeThreshold = 0.1f;
            string downloadSpeedFormat = string.Empty;
            int averageCount = 0;

            const int maxAverageCount = 10;
            float[] averageList = new float[maxAverageCount];

            float downloadProgress = 0f;
            float lastNodeProcess = 0f;

            XHotfixDownloader http = new XHotfixDownloader((int percent) => {
                downloadProgress = XHotfixRetryPolicy.CalculateProgress(
                    downloadedBytes,
                    entrySize,
                    percent,
                    totalBytes);
            });

            if (totalMegabyte > 0)
                progress?.Invoke(HotfixStatus.HotfixBundles, 0, string.Format("{0:0.00}Mb", totalMegabyte));

            while (entryDiffs.Count > 0)
            {
                EntryDiff entry = entryDiffs.Dequeue();

                string fullPath = GetFullPath(entry.remote);
                string tempPath = Path.ChangeExtension(fullPath,
                    entry.remote.checksum.ToString());

                entrySize = entry.remote.size;
                var retryPolicy = new XHotfixRetryPolicy(maxDownloadAttempts);
                string failureReason = "Unknown download failure.";

                while (retryPolicy.TryBeginAttempt(out int attempt))
                {
                    downloadProgress = XHotfixRetryPolicy.CalculateProgress(
                        downloadedBytes,
                        0,
                        0,
                        totalBytes);
                    lastNodeProcess = totalMegabyte * downloadProgress;
                    lastNodeTime = Time.realtimeSinceStartup;

                    string url = config.MakeDownloadURL(remoteVersion, entry.remote.name);
                    http.Download(url, tempPath);
                    
                    float lasetProgress = downloadProgress;
                    while(http.Downloading)
                    {
                        if (downloadProgress > lasetProgress)
                        {
                            float downloadSpeed = 0f;
                            if (Time.realtimeSinceStartup - lastNodeProcess > timeThreshold)
                            {
                                downloadSpeed = (totalMegabyte * downloadProgress - lastNodeProcess) / (Time.realtimeSinceStartup - lastNodeTime);
                                downloadSpeed = CalcDownloadAverage(averageCount, 10, averageList, downloadSpeed);

                                lastNodeProcess = totalMegabyte * downloadProgress;
                                lastNodeTime = Time.realtimeSinceStartup;

                                string speedUnit = (downloadSpeed < 1) ? "kb/s" : "mb/s";
                                if (downloadSpeed < 1)
                                    downloadSpeed *= 1000f;

                                downloadSpeedFormat = string.Format("{0} {1:0.0}{2}", XHotfixUtility.Md5Sum(entry.remote.name), downloadSpeed, speedUnit);
                                averageCount++;
                            }

                            // download speed more than 10kb/s, show download speed
                            if (downloadSpeed > 10)
                            {
                                var text = string.Format("{0:0.00}Mb {1}", (totalBytes - downloadedBytes) / (float)(1024 * 1024), downloadSpeedFormat);
                                progress?.Invoke(HotfixStatus.DownloadProgress, downloadProgress, text);
                            }
                        }

                        yield return null;
                    }

                    bool retryable = true;
                    if (http.Finish && File.Exists(tempPath))
                    {
                        uint checksum = XHotfixUtility.ComputeFileChecksum(tempPath);
                        if (checksum == entry.remote.checksum)
                        {
                            FileInfo fileInfo = new FileInfo(tempPath);
                            if (File.Exists(fullPath))
                                File.Delete(fullPath);

                            fileInfo.MoveTo(fullPath);
#if UNITY_HOTFIXLOG
                            Debug.Log($"[XRes.DownloaEntrys] move {tempPath} to {fullPath} succeed");
#endif
                            
                            UpdateLocalBundleSheet(entry);
                            downloadedBytes += entrySize;
                            downloadProgress = XHotfixRetryPolicy.CalculateProgress(
                                downloadedBytes,
                                0,
                                0,
                                totalBytes);
                            retryPolicy.MarkSucceeded();
                            break;
                        }

                        failureReason =
                            $"Checksum mismatch. Expected {entry.remote.checksum}, actual {checksum}.";
                    }
                    else
                    {
                        failureReason = http.Finish
                            ? "Download completed without a temporary file."
                            : http.ExceptionError?.Message ?? "Downloader failed without an exception.";
                        retryable = !http.Finish &&
                            XHotfixRetryPolicy.IsRetryable(http.ExceptionError);
                    }

                    if (File.Exists(tempPath))
                        File.Delete(tempPath);

#if UNITY_HOTFIXLOG
                    Debug.LogError(
                        $"[XRes.DownloadEntrys] download {entry.remote.name} " +
                        $"attempt {attempt}/{maxDownloadAttempts} failed: {failureReason}");
#endif

                    if (!retryable || retryPolicy.IsExhausted)
                        break;

                    float retryDelay = retryPolicy.GetRetryDelaySeconds(
                        initialRetryDelaySeconds);
                    yield return new WaitForSecondsRealtime(retryDelay);
                }

                if (!retryPolicy.Succeeded)
                {
                    string failureMessage =
                        $"{remoteVersion} | {entry.remote.name} | " +
                        $"attempt {retryPolicy.AttemptCount}/{maxDownloadAttempts} | " +
                        failureReason;
                    progress?.Invoke(
                        HotfixStatus.HotfixFailure,
                        downloadProgress,
                        failureMessage);
                    config.Report(remoteVersion.ToString());
                    yield break;
                }
            }

            var newVersionCode = remoteVersion.ToString();
            SaveLocalVersion(newVersionCode);

            // hotfix finish, reinstall bundle sheet
            ReInitialize();

            progress?.Invoke(HotfixStatus.HotfixSucceed, 1.0f, remoteVersion.ToString());

            config.Report(remoteVersion.ToString());
        }

        /// <summary>
        /// update and save local bundle sheet info
        /// </summary>
        /// <param name="entry"></param>
        static void UpdateLocalBundleSheet(EntryDiff entry)
        {
            // update bundle sheet
            var bundleSheet = bundleManager.GetBundleSheet();
            if (entry.local != null)
                bundleSheet.RemoveEntry(entry.local);

            entry.remote.locationType = LocationType.Remote;
            entry.remote.entryType = EntryType.None;

#if UNITY_HOTFIXLOG
            Debug.Log($"[XRes.UpdateLocalBundleSheet] update {entry.remote.name} succeed");
#endif
            bundleSheet.AddEntry(entry.remote);

            var bundleSheetPath = config.GetBundleSheetPath();
            bundleSheet.Save(bundleSheetPath, true,
                remoteVersion.ToString());

#if UNITY_HOTFIXLOG
            Debug.Log($"[XRes.UpdateLocalBundleSheet] save sheet {bundleSheetPath} succeed");
#endif
        }

        /// <summary>
        /// re init bundle manager
        /// </summary>
        static void ReInitialize()
        {
            var sheet = bundleManager.GetBundleSheet();
            bundleManager.Initialize(sheet.GetEntrys());
        }

        /// <summary>
        /// save local version number
        /// </summary>
        /// <param name="versionCode"></param>
        static void SaveLocalVersion(string versionCode)
        {
#if UNITY_HOTFIXLOG
            Debug.Log($"Save local version {versionCode} succeed");
#endif
            PlayerPrefs.SetString(typeof(XHotfixVersion).Name.ToLower(), versionCode);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// read local version number
        /// </summary>
        /// <returns></returns>
        private static string ReadLocalVersion()
        {
            return PlayerPrefs.GetString(typeof(XHotfixVersion).Name.ToLower());
        }

        /// <summary>
        /// calc download average speed
        /// </summary>
        static float CalcDownloadAverage(int currentCount, int maxCount, float[] averageList, float currentSpeed)
        {
            averageList[currentCount % maxCount] = currentSpeed;

            float total = 0;
            for (int i = 0; i < maxCount; i++)
            {
                total += averageList[i];
            }

            return total / maxCount;
        }

        /// <summary>
        /// get real full path
        /// </summary>
        static string GetFullPath(Entry entry)
        {
            string directoy = Path.Combine(Application.persistentDataPath, Path.GetDirectoryName(entry.name));
            if (!Directory.Exists(directoy))
                Directory.CreateDirectory(directoy);
            
            return Path.Combine(Application.persistentDataPath, entry.name);
        }

        /// <summary>
        /// open download window, show hotfix download size
        /// </summary>
        /// <param name="window"></param>
        /// <param name="totalMegabyte"></param>
        /// <returns></returns>
        static IEnumerator OpenDownloadSize(IHotfixWindow window, float totalMegabyte)
        {
            bool confirm = false;
            bool cancle = false;

            window.MessageBox(HotfixMessageType.GetDownloadSizeSucceed, totalMegabyte.ToString("0.000Mb"), (no, yes) => {
                cancle = no; confirm = yes;
            });
       
            while (!confirm && !cancle)
                yield return null;

            if (cancle)
            {
                Application.Quit();
            }
        }

        /// <summary>
        /// calc download total size
        /// </summary>
        /// <param name="entryDiffs"></param>
        /// <returns></returns>

        static long CalcDownloadSize(Queue<EntryDiff> entryDiffs)
        {
            long totalBytes = 0;
            foreach(var entry in entryDiffs)
            {
                totalBytes += entry.remote.size;
            }

            return totalBytes;
        }

#endregion
    }
}
