using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class Local
    {
        /// <summary>
        /// 别名ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 数字ID
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [JsonProperty]
        public string Host { get; private set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (Local exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class LocalConfig : Config<Local>
    {
    }

    /// <summary>
    /// export excel Local data
    /// </summary>
    public sealed partial class Excel
    {
        public const int GAME_DEVELOP = 10001;
        public const int GAME_INTERNAL = 10002;
        public const int GAME_INTERNAL_DEV = 10003;
        public const int GAME_RELEASE = 10004;
        public const int GAME_RELEASE_DEV = 10005;
        public const int GEME_RELEASE_DEBUG = 10006;
        /// <summary>
        /// config LocalConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, Local> localConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static Local propLocalById(int nId)
        {
            Local data;
            if (!localConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(Local).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}