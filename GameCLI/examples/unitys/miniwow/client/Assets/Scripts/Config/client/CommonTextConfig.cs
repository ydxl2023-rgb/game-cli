using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class CommonText
    {
        /// <summary>
        /// Alias
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// Numeric ID
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// Localized content
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
        public const int TEXT_HOME_LOCKED = 10001;
        public const int TEXT_HOME_INVALID = 10002;
        public const int TEXT_HOME_PENDING = 10003;
        public const int TEXT_TASK_GUIDE_ASSIST = 20001;
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