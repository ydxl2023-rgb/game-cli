using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class HomeHospitalSetting
    {
        /// <summary>
        /// 医院ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 原作生产表现秒数
        /// </summary>
        [JsonProperty]
        public float CraftSeconds { get; private set; }

        /// <summary>
        /// 原作物品飞行秒数
        /// </summary>
        [JsonProperty]
        public float FlightSeconds { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (HomeHospitalSetting exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class HomeHospitalSettingConfig : Config<HomeHospitalSetting>
    {
    }

    /// <summary>
    /// export excel HomeHospitalSetting data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config HomeHospitalSettingConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, HomeHospitalSetting> homeHospitalSettingConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static HomeHospitalSetting propHomeHospitalSettingById(int nId)
        {
            HomeHospitalSetting data;
            if (!homeHospitalSettingConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(HomeHospitalSetting).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}