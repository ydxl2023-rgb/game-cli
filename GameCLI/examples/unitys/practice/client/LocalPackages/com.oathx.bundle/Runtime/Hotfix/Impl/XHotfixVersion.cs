using System;

namespace Oathx.Bundle.Assets
{
    /// <summary>
    /// hotfix version info
    /// </summary>
    [Serializable]
    public class XHotfixVersion
    {
        /// <summary>
        /// platform
        /// </summary>
        public string platform;
        /// <summary>
        /// description
        /// </summary>
        public string description;
        /// <summary>
        /// version status
        /// </summary>
        public string status;
        /// <summary>
        /// version create time
        /// </summary>
        public string createTime;
        /// <summary>
        /// version update time
        /// </summary>
        public string updateTime;
        /// <summary>
        /// ip whitelist
        /// </summary>
        public string[] ipWhitelist;
        /// <summary>
        /// app version
        /// </summary>
        public int appVersion;
        /// <summary>
        /// channel
        /// </summary>
        public int channel;
        /// <summary>
        /// paralle version
        /// </summary>
        public int paralleVersion;
        /// <summary>
        /// AOT version
        /// </summary>
        public int aotVersion;
        /// <summary>
        /// resource version
        /// </summary>
        public int resVersion;
        /// <summary>
        /// script version
        /// </summary>
        public int srcVersion;
        /// <summary>
        /// CDN list
        /// </summary>
        public string[] cdns = new string[] { };
        /// <summary>
        /// auth url
        /// </summary>
        public string authUrl;
        /// <summary>
        /// pay url
        /// </summary>
        public string payUrl;
        /// <summary>
        /// version string
        /// </summary>
        public string version;
        
        /// <summary>
        /// contruct version info
        /// </summary>
        public XHotfixVersion()
        {
        }

        /// <summary>
        /// construct version info
        /// </summary>
        /// <param name="text"></param>
        public XHotfixVersion(string text)
        {
            Parse(text);
        }

        /// <summary>
        /// convert version info to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{appVersion}.{channel}.{paralleVersion}.{aotVersion}.{srcVersion}.{resVersion}";
        }

        /// <summary>
        /// return AOT version string
        /// </summary>
        /// <returns></returns>
        public string GetAOTVersion()
        {
            return $"{appVersion}.{channel}.{paralleVersion}.{aotVersion}";
        }

        /// <summary>
        /// return resource hotfix version
        /// </summary>
        /// <returns></returns>
        public string GetHotfixVersion()
        {
            return $"{srcVersion}.{resVersion}";
        }

        /// <summary>
        /// full parse version string
        /// </summary>
        public bool Full()
        {
            if (string.IsNullOrEmpty(version))
                return false;
            
            return Parse(version);
        }

        /// <summary>
        /// parse version string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool Parse(string text)
        {
            string[] splits = text.Split('.');
            if (splits.Length < 6)
                return false;
                
            appVersion = int.Parse(splits[0]);
            channel = int.Parse(splits[1]);
            paralleVersion = int.Parse(splits[2]);
            aotVersion = int.Parse(splits[3]);
            srcVersion = int.Parse(splits[4]);
            resVersion = int.Parse(splits[5]);
        
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(XHotfixVersion exists)
        {
            return exists != null;
        }

        /// <summary>
        /// try parse version string, out version object
        /// </summary>
        /// <param name="text"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool TryParse(string text, out XHotfixVersion version)
        {
            version = new XHotfixVersion();

            if (string.IsNullOrEmpty(text))
                return false;

            return version.Parse(text);
        }
    }
}
