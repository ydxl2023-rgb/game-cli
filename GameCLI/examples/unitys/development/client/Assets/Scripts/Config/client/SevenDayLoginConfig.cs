using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class SevenDayLogin
    {
        /// <summary>
        /// 别名
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 数字ID
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// 图标
        /// </summary>
        [JsonProperty]
        public string Icon { get; private set; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty]
        public string Description { get; private set; }

        /// <summary>
        /// 道具类型
        /// </summary>
        [JsonProperty]
        public EItemType ItemType { get; private set; }

        /// <summary>
        /// 道具奖励
        /// </summary>
        [JsonProperty]
        public List<int> Items { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        public bool IsSpecial { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        public EQuality Quality { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (SevenDayLogin exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class SevenDayLoginConfig : Config<SevenDayLogin>
    {
    }

    /// <summary>
    /// export excel SevenDayLogin data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config SevenDayLoginConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, SevenDayLogin> sevenDayLoginConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static SevenDayLogin propSevenDayLoginById(int nId)
        {
            SevenDayLogin data;
            if (!sevenDayLoginConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(SevenDayLogin).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}