using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class BlacksmithSetting
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// RequiredTaskID
        /// </summary>
        [JsonProperty]
        public int RequiredTaskID { get; private set; }

        /// <summary>
        /// RepairCurrencyID
        /// </summary>
        [JsonProperty]
        public int RepairCurrencyID { get; private set; }

        /// <summary>
        /// RepairCost
        /// </summary>
        [JsonProperty]
        public int RepairCost { get; private set; }

        /// <summary>
        /// MaxQuantity
        /// </summary>
        [JsonProperty]
        public int MaxQuantity { get; private set; }

        /// <summary>
        /// ForgeSeconds
        /// </summary>
        [JsonProperty]
        public float ForgeSeconds { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (BlacksmithSetting exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class BlacksmithSettingConfig : Config<BlacksmithSetting>
    {
    }

    /// <summary>
    /// export excel BlacksmithSetting data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config BlacksmithSettingConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, BlacksmithSetting> blacksmithSettingConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static BlacksmithSetting propBlacksmithSettingById(int nId)
        {
            BlacksmithSetting data;
            if (!blacksmithSettingConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(BlacksmithSetting).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}