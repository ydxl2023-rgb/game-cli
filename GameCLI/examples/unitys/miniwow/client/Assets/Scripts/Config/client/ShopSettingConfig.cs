using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class ShopSetting
    {
        /// <summary>
        /// 交易行建筑ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 原开放任务
        /// </summary>
        [JsonProperty]
        public int RequiredTaskID { get; private set; }

        /// <summary>
        /// 本地单次购买上限
        /// </summary>
        [JsonProperty]
        public int MaxQuantity { get; private set; }

        /// <summary>
        /// 首版开放档位
        /// </summary>
        [JsonProperty]
        public int Tier { get; private set; }

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