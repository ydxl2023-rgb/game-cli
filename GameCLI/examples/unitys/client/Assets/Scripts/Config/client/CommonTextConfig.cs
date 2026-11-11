using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class CommonText
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
        /// 文本内容
        /// </summary>
        [JsonProperty]
        public string Context { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (CommonText exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class CommonTextConfig : Config<CommonText>
    {
    }

    /// <summary>
    /// export excel CommonText data
    /// </summary>
    public sealed partial class Excel
    {
        public const int ERR_DODGE = 101;
        public const int ERR_BLOCK = 102;
        public const int ERR_NOHIT = 103;
        public const int LOGIN_RECENT_SERVER = 1001;
        public const int LOGIN_RECOMMENDED_SERVER = 1002;
        public const int LOGIN_SERVER_RANGE = 1003;
        public const int LOGIN_LOCAL_SERVER_NAME = 1004;
        public const int LOGIN_SERVER_NORMAL = 1005;
        public const int LOGIN_SERVER_BUSY = 1006;
        public const int LOGIN_SERVER_MAINTENANCE = 1007;
        /// <summary>
        /// config CommonTextConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, CommonText> commonTextConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static CommonText propCommonTextById(int nId)
        {
            CommonText data;
            if (!commonTextConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(CommonText).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}