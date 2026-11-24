using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class ShopOffer
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
        /// Category
        /// </summary>
        [JsonProperty]
        public int Category { get; private set; }

        /// <summary>
        /// NameTextId
        /// </summary>
        [JsonProperty]
        public int NameTextId { get; private set; }

        /// <summary>
        /// Price
        /// </summary>
        [JsonProperty]
        public int Price { get; private set; }

        /// <summary>
        /// Reward
        /// </summary>
        [JsonProperty]
        public int Reward { get; private set; }

        /// <summary>
        /// Stock
        /// </summary>
        [JsonProperty]
        public int Stock { get; private set; }

        /// <summary>
        /// Icon
        /// </summary>
        [JsonProperty]
        public string Icon { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (ShopOffer exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class ShopOfferConfig : Config<ShopOffer>
    {
    }

    /// <summary>
    /// export excel ShopOffer data
    /// </summary>
    public sealed partial class Excel
    {
        public const int SHOP_RECHARGE_60 = 1;
        public const int SHOP_RECHARGE_300 = 2;
        public const int SHOP_RECHARGE_980 = 3;
        public const int SHOP_RECHARGE_1980 = 4;
        public const int SHOP_RECHARGE_3280 = 5;
        public const int SHOP_RECHARGE_6480 = 6;
        public const int SHOP_LOCAL_GIFT_1 = 101;
        public const int SHOP_LOCAL_GIFT_2 = 102;
        public const int SHOP_LOCAL_GIFT_3 = 103;
        /// <summary>
        /// config ShopOfferConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, ShopOffer> shopOfferConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static ShopOffer propShopOfferById(int nId)
        {
            ShopOffer data;
            if (!shopOfferConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(ShopOffer).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}