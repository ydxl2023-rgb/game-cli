using Oathx.Bundle.Assets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Oathx.Core
{
    /// <summary>
    /// Runtime Type Enum
    /// </summary>
    public enum AppRuntime
    {
        /// <summary>
        /// Mono Runtime
        /// </summary>
        Mono,

        /// <summary>
        /// HybridCLR Runtime
        /// </summary>
        HybridCLR
    }

    /// <summary>
    /// App Region Enum
    /// </summary>
    public enum AppRegion
    {
        /// <summary>
        /// China Region
        /// </summary>
        CN,
        /// <summary>
        /// Japan Region
        /// </summary>
        Jp,
        /// <summary>
        /// Korea Region
        /// </summary>
        Kr,
        /// <summary>
        /// Taiwan Region
        /// </summary>
        Tw,
        /// <summary>
        /// English Region
        /// </summary>
        En
    }

    /// <summary>
    /// App Bundle Source Enum
    /// </summary>
    public enum AppBundleSource
    {
        Local,
        Bundle
    }

    /// <summary>
    /// XApp Configure ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "XAppConfigure", menuName = "Oathx/XAppConfigure", order = 1)]
    public class XAppConfigure : ScriptableObject
    {
        /// <summary>
        /// The runtime type of the XApp.
        /// </summary>
        [SerializeField]
        public AppRuntime runtime;

        /// <summary>
        /// The region of the XApp.
        /// </summary>
        [SerializeField]
        public AppRegion region;

        /// <summary>
        /// The bundle source of the XApp.
        /// </summary>
        [SerializeField]
        public AppBundleSource bundleSource;

        /// <summary>
        /// AssetBundle 自动释放的空闲时长（秒）。
        /// Bundle 引用计数归零后，空闲超过该时长的 Bundle 壳会被自动 Unload。
        /// 传 0 表示用完即卸（每次加载后立即卸载壳，下一波加载会重新 LoadFromFile，造成高频抖动）；
        /// 负值表示永不自动释放；默认 60s，壳本身只占少量内存，驻留换取加载平稳。
        /// </summary>
        [SerializeField]
        public float bundleAutoReleaseTime = 60f;

        /// <summary>
        /// The application ID of the XApp.
        /// </summary>
        [SerializeField]
        public string appId;

        /// <summary>
        /// The URL of the XApp server.
        /// example: http://localhost:8000
        /// </summary>
        [SerializeField]
        public string versionAPI;

        /// <summary>
        /// The URL of the version API for the XApp.
        /// </summary>
        public string VersionURL
        {
            get
            {
#if UNITY_EDITOR
                return $"{versionAPI}/api/v1/version/latest?projectId={appId}&platform={ValidTarget.Android}";
#else
                switch(Application.platform)
                {
                    case RuntimePlatform.Android:
                        return $"{versionAPI}/projectId={appId}&platform={ValidTarget.Android}";

                    case RuntimePlatform.IPhonePlayer:
                        return $"{versionAPI}/projectId={appId}&platform={ValidTarget.iOS}";
                    default:
                        return $"{versionAPI}/projectId={appId}&platform={ValidTarget.StandaloneWindows}";
                }
#endif
            }
        }

        /// <summary>
        /// Indicates whether hotfix is enabled for the XApp.
        /// </summary>
        [SerializeField]
        public bool hotfix;

        /// <summary>
        /// Indicates whether hotfix debug mode is enabled for the XApp.
        /// </summary>
        [SerializeField]
        public bool hotfixDebugEnabled;

        /// <summary>
        /// The list of plugins to be used by the XApp.
        /// </summary>
        [SerializeField]
        public string[] plugins;

        /// <summary>
        /// Indicates whether entity archives should be loaded from and saved to the remote server.
        /// </summary>
        [SerializeField]
        public bool syncEntityArchiveToRemote = true;

        /// <summary>
        /// Indicates whether local entity archive files should be cleared when the app starts.
        /// </summary>
        [SerializeField]
        public bool clearLocalEntityArchiveOnStartup;

        /// <summary>
        /// The configuration bundle name for the XApp.
        /// </summary>
        [SerializeField]
        public string cfgBundle;

        /// <summary>
        /// The authentication URL for the XApp.
        /// </summary>
        [SerializeField]
        public string authUrl;

        /// <summary>
        /// The payment URL for the XApp.
        /// </summary>
        [SerializeField]
        public string payUrl;

        /// <summary>
        /// The product URL for the XApp.
        /// </summary>
        [SerializeField]
        public string productUrl;

        /// <summary>
        /// Update Server URL
        /// </summary>
        /// <param name="authURL"></param>
        /// <param name="payURL"></param>
        public void UpdateServerURL(string authURL, string payURL)
        {
            // reset auth url
            if (!string.IsNullOrEmpty(authURL))
                authUrl = authURL;

            // reset pay url
            if (!string.IsNullOrEmpty(payURL))
                payUrl = payURL;

#if UNITY_EDITOR
            Debug.Log($"[UpdateServerURL] Auth url {authUrl} pay url {payUrl}");
#endif
        }
    }
}
