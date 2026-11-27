using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class HomeInnSetting
    {
        /// <summary>
        /// 旅店建筑ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 本地每批原料数量
        /// </summary>
        [JsonProperty]
        public int MaterialCount { get; private set; }

        /// <summary>
        /// 本地每批成品数量
        /// </summary>
        [JsonProperty]
        public int OutputCount { get; private set; }

        /// <summary>
        /// 本地旧存档初次接入行动力
        /// </summary>
        [JsonProperty]
        public int InitialEnergyPercent { get; private set; }

        /// <summary>
        /// 原作制作表现秒数
        /// </summary>
        [JsonProperty]
        public float CraftSeconds { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (HomeInnSetting exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class HomeInnSettingConfig : Config<HomeInnSetting>
    {
    }

    /// <summary>
    /// export excel HomeInnSetting data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config HomeInnSettingConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, HomeInnSetting> homeInnSettingConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static HomeInnSetting propHomeInnSettingById(int nId)
        {
            HomeInnSetting data;
            if (!homeInnSettingConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(HomeInnSetting).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}