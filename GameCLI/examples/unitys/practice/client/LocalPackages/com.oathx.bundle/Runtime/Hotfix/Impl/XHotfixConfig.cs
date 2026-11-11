using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Oathx.Bundle.Assets
{
    /// <summary>
    /// valid build target
    /// </summary>
    public enum ValidTarget
    {
        StandaloneWindows,
        Android,
        iOS,
        WebGL
    }

    /// <summary>
    /// default remote config
    /// </summary>
    public class XHotfixConfig
    {
        /// <summary>
        /// report url
        /// </summary>
        public string reportURL
        { get; protected set; }

        /// <summary>
        /// cdn url
        /// </summary>
        public string cdn
        { get; protected set; }

        /// <summary>
        /// out cache path
        /// </summary>
        public string outCachePath
        { get; protected set; }

        /// <summary>
        /// out cache file
        /// </summary>
        public string outCacheFile
        { get; protected set; }

        /// <summary>
        /// enable debug, show more log
        /// </summary>
        public bool debugEnabled
        { get; protected set; }

        /// <summary>
        /// construct default remote config
        /// </summary>
        /// <param name="bundleSource"></param>
        public XHotfixConfig(bool debugEnable, string reportResultURL="")
        {
            reportURL = reportResultURL;
            debugEnabled = debugEnable;

#if UNITY_ANDROID && !UNITY_EDITOR
            outCachePath = Path.Combine(Application.persistentDataPath, Application.platform.ToString());
#else
            outCachePath = Path.Combine(Application.temporaryCachePath, Application.platform.ToString());
#endif

            if (!Directory.Exists(outCachePath))
                Directory.CreateDirectory(outCachePath);

            outCacheFile = Path.Combine(outCachePath, typeof(XBundleSheet).Name.ToLower() + ".bytes");
        }

        /// <summary>
        /// clearup cache file
        /// </summary>
        public virtual void Clearup()
        {
            if (File.Exists(outCacheFile))
                File.Delete(outCacheFile);

            if (Directory.Exists(outCachePath))
                Directory.Delete(outCachePath, true);
        }

        /// <summary>
        /// return package bundle sheet text
        /// </summary>
        /// <returns></returns>
        public virtual string GetPackageBundleSheet()
        {
            string bundleSheetName = typeof(XBundleSheet).Name.ToLower();
            if (debugEnabled)
            {
                var version = PlayerPrefs.GetString(typeof(XHotfixVersion).Name.ToLower());
                if (!string.IsNullOrEmpty(version))
                    PlayerPrefs.DeleteKey(typeof(XHotfixVersion).Name.ToLower());
            }

            var bundleSheetPath = GetBundleSheetPath();
            if (File.Exists(bundleSheetPath))
            {
                if (debugEnabled)
                {
                    File.Delete(bundleSheetPath);
                }
                else
                {
                    return File.ReadAllText(bundleSheetPath);
                }
            }

            var asset = Resources.Load<TextAsset>(bundleSheetName);
            if (!asset)
                throw new System.NullReferenceException();

            return asset.text;
        }

        /// <summary>
        /// get package version info
        /// </summary>
        /// <returns></returns>
        public virtual XHotfixVersion GetPackageVersion()
        {
            var asset = Resources.Load<TextAsset>(typeof(XBundleSheet).Name.ToLower());
            if (!asset)
                throw new System.NullReferenceException();

            var sheet = JsonUtility.FromJson<BundleSheetInfo>(asset.text);
            return new XHotfixVersion(sheet.version);
        }

        /// <summary>
        /// get bundle sheet path
        /// </summary>
        /// <returns></returns>
        public virtual string GetBundleSheetPath()
        {
            return outCacheFile;
        }

        /// <summary>
        /// get runtime platform
        /// </summary>
        /// <returns></returns>
        public virtual string GetRuntimePlatform()
        {
#if UNITY_EDITOR
            return RuntimePlatform.Android.ToString();
#else

            switch(Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return ValidTarget.StandaloneWindows.ToString();

                case RuntimePlatform.Android:
                    return ValidTarget.Android.ToString();

                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.IPhonePlayer:
                    return ValidTarget.iOS.ToString();
                default:
                    throw new System.NotSupportedException($"No support platform {Application.platform}");
            }
#endif
        }

        /// <summary>
        /// return bundle remote bundle sheet url
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public virtual string GetBundleSheetURL(string baseUrl, XHotfixVersion version)
        {
            var platform = GetRuntimePlatform();
            return Path.Combine(baseUrl, platform, version.GetAOTVersion(),
                typeof(XBundleSheet).Name.ToLower() + ".bytes").Replace("\\", "/");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string MakeDownloadURL(XHotfixVersion version, string path)
        {
            var platform = GetRuntimePlatform();
            return Path.Combine(cdn, platform,
                version.GetAOTVersion(), "hotfix", version.GetHotfixVersion(), path).Replace("\\", "/");
        }

        /// <summary>
        /// set cdn url
        /// </summary>
        /// <param name="url"></param>
        public virtual void SetCDNUrl(string url)
        {
            cdn = url;
        }

        /// <summary>
        /// get cdn url
        /// </summary>
        /// <returns></returns>
        public virtual string GetCDNUrl()
        { 
            return cdn; 
        }

        /// <summary>
        /// report hotfix result
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        public virtual bool Report(string version)
        {
            if (string.IsNullOrEmpty(reportURL))
                return false;
            
            string platform = GetRuntimePlatform();
            XBundleCoroutine.Run(OnPostJson(reportURL, platform, version));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Serializable]
        public class ReportResult
        {
            public string platform;
            public string version;
        }

        /// <summary>
        /// post json data
        /// </summary>
        /// <param name="api"></param>
        /// <param name="platform"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        private IEnumerator OnPostJson(string api, string platform, string version)
        {
            var body = new ReportResult() {
                platform = platform, version = version
            };

            UnityWebRequest request = UnityWebRequest.Put(api, JsonUtility.ToJson(body));
            request.method = UnityWebRequest.kHttpVerbPOST;

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

       
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[Hotfix.Report] report success response {request.downloadHandler.text}");
            }
    
            request.Dispose();
        }
    }
}
