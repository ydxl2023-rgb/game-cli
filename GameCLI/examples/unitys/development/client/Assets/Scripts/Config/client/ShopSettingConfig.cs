using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class ShopSetting
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// NumID
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// StartingBalance
        /// </summary>
        [JsonProperty]
        public int StartingBalance { get; private set; }

        /// <summary>
        /// RewardIcon
        /// </summary>
        [JsonProperty]
        public string RewardIcon { get; private set; }

        /// <summary>
        /// Level
        /// </summary>
        [JsonProperty]
        public int Level { get; private set; }

        /// <summary>
        /// PassPrice
        /// </summary>
        [JsonProperty]
        public string PassPrice { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (ShopSetting exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class ShopSettingConfig : Config<ShopSetting>
    {
    }

    /// <summary>
    /// export excel ShopSetting data
    /// </summary>
    public sealed partial class Excel
    {
        public const int SHOP_LOCAL_SETTINGS = 1;
        /// <summary>
        /// config ShopSettingConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, ShopSetting> shopSettingConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static ShopSetting propShopSettingById(int nId)
        {
            ShopSetting data;
            if (!shopSettingConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(ShopSetting).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}