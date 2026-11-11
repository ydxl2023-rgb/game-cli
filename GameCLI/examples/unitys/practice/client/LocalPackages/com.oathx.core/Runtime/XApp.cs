using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oathx.Bundle.Assets;
using System.Reflection;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;
using Oathx.Core.Sdk;
using UnityEngine.Rendering.Universal;
using System.IO;

namespace Oathx.Core
{
    [Serializable]
    public class APIVersion
    {
        /// <summary>
        /// version id
        /// </summary>
        public int id
        { get; set; }

        /// <summary>
        /// current client version
        /// </summary>
        public string version
        { get; set; }

        /// <summary>
        /// runtime platform
        /// </summary>
        public string platform
        { get; set; }

        /// <summary>
        /// version create time
        /// </summary>
        public string createTime
        { get; set; }

        /// <summary>
        /// version update time
        /// </summary>
        public string updateTime
        { get; set; }

        /// <summary>
        /// client auth url
        /// </summary>
        public string authUrl
        { get; set; }

        /// <summary>
        /// hotfix cdn list
        /// </summary>
        public string[] cdns
        { get; set; }

        /// <summary>
        /// client pay url
        /// </summary>
        public string payUrl
        { get; set; }

        /// <summary>
        /// version status
        /// </summary>
        public int status
        { get; set; }
    }

    /// <summary>
    /// remote version info
    /// </summary>
    [Serializable]
    public class AppVersion
    {
        /// <summary>
        /// get version is success
        /// </summary>
        public bool success
        { get; set; }

        /// <summary>
        /// version data
        /// </summary>
        public APIVersion data
        { get; set; }

        /// <summary>
        /// error message
        /// </summary>
        public string message
        { get; set; }

        /// <summary>
        /// error code
        /// </summary>
        public int errorCode
        { get; set; }
    }

    /// <summary>
    /// XApp Main Entry
    /// </summary>
    public class XApp : XMonoSingleton<XApp>
    {
        /// <summary>
        /// The app configure.
        /// </summary>
        [SerializeField]
        private XAppConfigure conf;

        /// <summary>
        /// Is breaking update
        /// </summary>
        [SerializeField]
        private bool isBreaking;

        /// <summary>
        /// The auth URL.
        /// </summary>
        [SerializeField]
        private string authUrl;

        /// <summary>
        /// main camera
        /// </summary>
        [SerializeField]
        private Camera mainCamera;

        /// <summary>
        /// Launch view assigned from the scene's UI hierarchy.
        /// </summary>
        [SerializeField]
        private XLauncher launcher;

        /// <summary>
        /// Hide the launch view when the game UI is ready.
        /// </summary>
        public void HideLaunch()
        {
            if (launcher)
                launcher.Hide();
        }

        /// <summary>
        /// Keep only the launch background visible behind the game UI.
        /// </summary>
        public void ShowLaunchBackground()
        {
            if (launcher)
                launcher.ShowBackground();
        }

        /// <summary>
        /// Whether runtime systems have already been shut down.
        /// </summary>
        private bool runtimeShutdown;

        /// <summary>
        /// Awake is called when the script instance is being loaded
        /// </summary>
        private void Awake()
        {
            instance = this;

            Application.targetFrameRate = 60;
            GameObject.DontDestroyOnLoad(this);

            if (mainCamera)
                GameObject.DontDestroyOnLoad(mainCamera);

#if !UNITY_EDITOR
            // Update to the maximum screen resolution
            UpdateResolution();
#endif

            Debug.Log(System.IO.Path.GetTempPath());
        }

        /// <summary>
        /// Get the XAppConfigure ScriptableObject
        /// </summary>
        /// <returns></returns>
        private IEnumerator Start()
        {
            if (!launcher)
            {
                Debug.LogError("Assign the Launch component to XApp.launcher.", this);
                yield break;
            }

            // show the assigned launch view
            launcher.Show();
            launcher.ShowLoading();

            // load app configure
            var req = Resources.LoadAsync<XAppConfigure>(typeof(XAppConfigure).Name);
            yield return req;

            if (req.isDone)
            {
                // init app configure
                conf = req.asset as XAppConfigure;
                if (conf)
                {
                    ClearLocalEntityArchiveIfNeeded();

                    // init app tracking
                    InitAppTracking();

                    yield return DoCheckAppVersion();
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"load app configure failure");
#endif
            }

            yield return new WaitForEndOfFrame();

            // if current no need to break, startup hotfix assemblies
            if (!isBreaking)
            {
                Startup();
            }
        }

        /// <summary>
        /// Clears local entity archive files when the app configure requests a development reset.
        /// </summary>
        private void ClearLocalEntityArchiveIfNeeded()
        {
            if (!conf || !conf.clearLocalEntityArchiveOnStartup)
                return;

            ClearPersistentDataPath();
        }

        /// <summary>
        /// Deletes local persistent archive files under the app persistent data root.
        /// </summary>
        private void ClearPersistentDataPath()
        {
            string path = Application.persistentDataPath;
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return;

            DirectoryInfo directory = new DirectoryInfo(path);
            foreach (FileInfo file in directory.GetFiles())
                file.Delete();

#if UNITY_EDITOR
            Debug.Log($"Clear local entity archive path {path}");
#endif
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            XRes.Update();

            // update plugins
            XPluginManager.GetSingleton().Update(Time.deltaTime);
        }

        /// <summary>
        /// OnDestroy is called when the MonoBehaviour will be destroyed
        /// </summary>
        private void OnDestroy()
        {
            ShutdownRuntime();
        }

        /// <summary>
        /// OnApplicationQuit is called before Unity destroys scene objects.
        /// </summary>
        private void OnApplicationQuit()
        {
            ShutdownRuntime();
        }

        /// <summary>
        /// Shuts down loaded plugins before shared runtime objects are destroyed.
        /// </summary>
        private void ShutdownRuntime()
        {
            if (!runtimeShutdown)
            {
                runtimeShutdown = true;

                // unload all plugin
                XPluginManager.GetSingleton().UnloadAllPlugin();

                // close all coroutine
                XCoroutine.StopAll();
            }
        }

        /// <summary>
        /// Initialize app tracking system, we can use it to track user behavior, such as page view, click event, etc.
        /// </summary>
        private void InitAppTracking()
        {
            var api = new InternalTrackingAPI(conf);
            XAppTracking.GetSingleton().RegisterAPI(api);

            // track init tracking succeed event
            XAppTracking.GetSingleton().Login((succeed, message) => {});
        }

        /// <summary>
        /// Fetch remote version info
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoCheckAppVersion()
        {
            XAppTracking.GetSingleton().Track(TrackingEvent.CheckAppVersion);

            // convert bundle source
            XBundleSource source = XBundleSource.Local;
            switch (conf.bundleSource)
            {
                case AppBundleSource.Local:
                    source = XBundleSource.Local;
                    break;

                case AppBundleSource.Bundle:
                    source = XBundleSource.Bundle;
                    break;
            }

            // initialize hotfix system, we must be initialize it before do update
            XHotfixConfig config = new XHotfixConfig(conf.hotfixDebugEnabled);

            // bundle 自动释放延迟由 XAppConfigure.bundleAutoReleaseTime 控制（默认 60s）：
            // 传 0 会在每次加载后立刻卸载 bundle 壳，下一波加载又被迫重新 LoadFromFile，
            // 造成高频卸载/重载抖动；壳本身只占少量内存，驻留换取加载平稳。
            XRes.Initialize(source, conf.bundleAutoReleaseTime, config);

            if (conf.bundleSource == AppBundleSource.Local)
            {
                launcher.UpdateProgress(1.0f,
                    HotfixStatus.HotfixSucceed.ToString());
                yield break;
            }

            // first, fetch remote version info, we need to do hotfix update base on remote version info
            UnityWebRequest req = UnityWebRequest.Get(conf.VersionURL);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
#if UNITY_EDITOR
                Debug.LogError($"Fetch remote version from {conf.VersionURL} failed {req.error}");
#endif
            }
            else
            {
                // get remote version info from backup server
                AppVersion appVersion = JsonConvert.DeserializeObject<AppVersion>(req.downloadHandler.text);
                if (appVersion.success)
                {
                    // we need to update server url and pay url
                    conf.UpdateServerURL(appVersion.data.authUrl, appVersion.data.payUrl);

                    // convert remote version info to hotfix version info
                    if (TryConvertRemoteVersion(appVersion, out var remoteVersion))
                    {
                        XAppTracking.GetSingleton().Track(TrackingEvent.DoUpdate);
                        yield return DoUpdate(remoteVersion);
                    }
                }
                else
                {
                    Debug.LogError(req.downloadHandler.text);
                }
            }
        }

        /// <summary>
        /// Try convert remote version info
        /// </summary>
        /// <param name="result"></param>
        /// <param name="remoteVersion"></param>
        /// <returns></returns>
        private bool TryConvertRemoteVersion(AppVersion result, out XHotfixVersion remoteVersion)
        {
            remoteVersion = new XHotfixVersion(result.data.version) {
                payUrl = result.data.payUrl, authUrl = result.data.authUrl, cdns = result.data.cdns, platform = result.data.platform
            };

            return true;
        }

        /// <summary>
        /// Do hotfix update
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoUpdate(XHotfixVersion remoteVersion)
        {
            // if need to do hotfix update, perform hotfix update, must be set hotfix enable in app configure
            if (conf.hotfix)
            {
                yield return XRes.DoUpdate(remoteVersion, (status, progress, message) => {
                    switch (status)
                    {

                        // if start to download hotfix bundles
                        case HotfixStatus.HotfixBundles:
                            launcher.ShowUpdate();
                            break;

                        // if need force update app
                        case HotfixStatus.ForeceUpdate:
                            isBreaking = true;
                            break;

                        // if hotfix update succeed
                        case HotfixStatus.HotfixSucceed:
                            launcher.ShowLoading();
                            break;

                        // update download progress
                        case HotfixStatus.DownloadProgress:
                            launcher.UpdateProgress(progress, message);
                            break;
                    }
                });
            }
            else
            {
                launcher.UpdateProgress(1.0f,
                    HotfixStatus.HotfixSucceed.ToString());
            }
        }

        /// <summary>
        /// Startup hotfix assemblies
        /// </summary>
        private void Startup()
        {
            XAppTracking.GetSingleton().Track(TrackingEvent.RunAssembly);

            // run hotfix assemblys
            Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblys.Length != 0)
            {
                RunHotfixAssemblys(assemblys);
            }
        }

        /// <summary>
        /// Run hotfix assemblies
        /// </summary>
        /// <param name="appConf"></param>
        /// <param name="assemblys"></param>
        private void RunHotfixAssemblys(Assembly[] assemblys)
        {
            if (assemblys.Length != 0)
            {
                List<Type> types = new List<Type>();
                foreach (Assembly assembly in assemblys)
                {
                    foreach (string typeName in conf.plugins)
                    {
                        var type = assembly.GetType(typeName);
                        if (type != null)
                            types.Add(type);
                    }
                }

                bool succeed = false;

                // load and startup plugins
                foreach (Type type in types)
                {
                    XPlugin plugin = XPluginManager.GetSingleton().LoadPlugin(type);
                    if (plugin != null)
                    {
#if UNITY_EDITOR
                        Debug.Log($"Plugin {type} startup succeed");
#endif
                        succeed = true;

                        plugin.Startup();
                    }
                }

                if (!succeed)
                {
                    Debug.LogError($"No plugin startup succeed, please check your app configure");
                }
            }
        }

        /// <summary>
        /// Update to the maximum screen resolution
        /// </summary>
        private void UpdateResolution()
        {
            int maxHeight = 0;
            int maxWidth = 0;

            foreach (var resolution in Screen.resolutions)
            {
                if (resolution.width >= maxWidth && resolution.height >= maxHeight)
                {
                    maxWidth = resolution.width;
                    maxHeight = resolution.height;
                }
            }

            Screen.SetResolution(maxWidth, maxHeight, true);
        }

        /// <summary>
        /// get app configure
        /// </summary>
        /// <returns></returns>
        public XAppConfigure GetAppConfigure()
        {
            return conf;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public bool AddOverlayCamera(Camera camera)
        {
            if (!mainCamera || !camera)
                return false;

            UniversalAdditionalCameraData overlayCamera = camera.GetUniversalAdditionalCameraData();
            overlayCamera.renderType = CameraRenderType.Overlay;

            UniversalAdditionalCameraData baseCamera = mainCamera.GetUniversalAdditionalCameraData();
            baseCamera.cameraStack.Add(camera);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public bool RemoveOverlayCamera(Camera camera) 
        {
            if (!mainCamera || !camera)
                return false;

            UniversalAdditionalCameraData mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
            mainCameraData.cameraStack.Remove(camera);
            return true;
        }
    }
}
