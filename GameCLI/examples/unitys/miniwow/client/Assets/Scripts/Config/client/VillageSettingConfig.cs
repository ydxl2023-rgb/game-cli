using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class VillageSetting
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// HeroID
        /// </summary>
        [JsonProperty]
        public int HeroID { get; private set; }

        /// <summary>
        /// ActorID
        /// </summary>
        [JsonProperty]
        public int ActorID { get; private set; }

        /// <summary>
        /// BuildingID
        /// </summary>
        [JsonProperty]
        public int BuildingID { get; private set; }

        /// <summary>
        /// GoldID
        /// </summary>
        [JsonProperty]
        public int GoldID { get; private set; }

        /// <summary>
        /// MaterialID
        /// </summary>
        [JsonProperty]
        public int MaterialID { get; private set; }

        /// <summary>
        /// PurchasePrice
        /// </summary>
        [JsonProperty]
        public int PurchasePrice { get; private set; }

        /// <summary>
        /// MaterialCost
        /// </summary>
        [JsonProperty]
        public int MaterialCost { get; private set; }

        /// <summary>
        /// OutputCount
        /// </summary>
        [JsonProperty]
        public int OutputCount { get; private set; }

        /// <summary>
        /// InitialHealthPercent
        /// </summary>
        [JsonProperty]
        public int InitialHealthPercent { get; private set; }

        /// <summary>
        /// HeroGold
        /// </summary>
        [JsonProperty]
        public int HeroGold { get; private set; }

        /// <summary>
        /// HospitalX
        /// </summary>
        [JsonProperty]
        public float HospitalX { get; private set; }

        /// <summary>
        /// HospitalY
        /// </summary>
        [JsonProperty]
        public float HospitalY { get; private set; }

        /// <summary>
        /// HospitalZ
        /// </summary>
        [JsonProperty]
        public float HospitalZ { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (VillageSetting exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class VillageSettingConfig : Config<VillageSetting>
    {
    }

    /// <summary>
    /// export excel VillageSetting data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config VillageSettingConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, VillageSetting> villageSettingConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static VillageSetting propVillageSettingById(int nId)
        {
            VillageSetting data;
            if (!villageSettingConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(VillageSetting).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}