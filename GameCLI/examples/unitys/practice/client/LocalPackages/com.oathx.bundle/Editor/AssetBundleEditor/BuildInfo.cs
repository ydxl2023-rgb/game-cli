using Oathx.Bundle.Assets;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Oathx.Bundle.Editor
{
    /// <summary>
    /// Build Channel
    /// </summary>
    public enum NewChannel
    {
        None,
        Development,
        iOS,
        Google,
        WX,
        Facebook,
        Huawei,
        Oppo,
        Vivo,
        Ucgame,
        Bilibili,
        Taptap,
        Tencent
    }

    /// <summary>
    /// New Build Info
    /// </summary>
    [Serializable]
    public class BuildInfo
    {
        /// <summary>
        /// Build Target Platform
        /// </summary>
        public BuildTarget buildTarget = BuildTarget.Android;

        /// <summary>
        /// Output Directory
        /// </summary>
        public string outputDirectory = "";

        /// <summary>
        /// Build AssetBundle Options
        /// </summary>
        public BuildAssetBundleOptions options = BuildAssetBundleOptions.None;

        /// <summary>
        /// App Version
        /// </summary>
        public int appVersion;
        /// <summary>
        /// Channel
        /// </summary>
        public NewChannel channel = NewChannel.Development;

        /// <summary>
        /// Paralle Version
        /// </summary>
        public int paralleVersion;
        /// <summary>
        /// AOT Version
        /// </summary>
        public int aotVersion;
        /// <summary>
        /// Scripts Version
        /// </summary>
        public int srcVersion;
        /// <summary>
        /// Res Version
        /// </summary>
        public int resVersion;
        /// <summary>
        /// Use SVN Version Info
        /// </summary>
        public bool useSvnVersionInfo;
        /// <summary>
        /// Development Build
        /// </summary>
        public bool development;
        /// <summary>
        /// Connect Profiler
        /// </summary>
        public bool connectProfiler;
        /// <summary>
        /// Allow Debugging
        /// </summary>
        public bool allowDebugging;

        /// <summary>
        /// CDN URLs
        /// </summary>
        public string[] cdns = new string[2] { "", "" };

        /// <summary>
        /// Clear Folders
        /// </summary>
        public bool clearFolders = true;
        /// <summary>
        /// Copy To StreamingAssets
        /// </summary>
        public bool copyToStreaming = true;
        /// <summary>
        /// Use AssetBundle
        /// </summary>
        public bool useBundle = true;
        /// <summary>
        /// Android 平台是否打出 AAB 安装包（勾选 = aab，不勾 = apk）
        /// </summary>
        public bool buildAppBundle = false;
        /// <summary>
        /// Only Build Bundle
        /// </summary>
        public bool onlyBundle = false;
        /// <summary>
        /// Compress Options
        /// </summary>
        public bool zipHotfixPackage = true;
        /// <summary>
        /// Include Installer
        /// </summary>
        public bool includeInstaller = false;
        /// <summary>
        /// Auth URL
        /// </summary>
        public string authUrl;
        /// <summary>
        /// Pay URL
        /// </summary>
        public string payUrl;

        /// <summary>
        /// Get Build Version
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            return $"{appVersion}.{(int)channel}.{paralleVersion}.{aotVersion}.{srcVersion}.{resVersion}";
        }

        /// <summary>
        /// Get Main Version
        /// </summary>
        /// <returns></returns>
        public string GetAOTVersion()
        {
            return $"{appVersion}.{(int)channel}.{paralleVersion}.{aotVersion}";
        }

        /// <summary>
        /// Get Hotfix Version
        /// </summary>
        /// <returns></returns>
        public string GetHotfixVersion()
        {
            return $"{srcVersion}.{resVersion}";
        }

        /// <summary>
        /// Save Version Info
        /// </summary>
        /// <returns></returns>
        public bool SaveVersion(string path) 
        {
            XHotfixVersion version = new XHotfixVersion();
            version.platform = buildTarget.ToString().ToLower();
            version.appVersion = appVersion;
            version.channel = (int)channel;
            version.paralleVersion = paralleVersion;
            version.aotVersion = aotVersion;
            version.srcVersion = srcVersion;
            version.resVersion = resVersion;
            version.version = GetVersion();
            version.authUrl = authUrl;
            version.payUrl = payUrl;
            version.cdns = cdns;
            
            var text = JsonUtility.ToJson(version);
            File.WriteAllText(path, text);

            return true;
        }
    }
}