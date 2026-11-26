using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class ShopGoods
    {
        /// <summary>
        /// 原商品ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 物品ID
        /// </summary>
        [JsonProperty]
        public int ItemID { get; private set; }

        /// <summary>
        /// 每份数量
        /// </summary>
        [JsonProperty]
        public int Amount { get; private set; }

        /// <summary>
        /// 货币ID
        /// </summary>
        [JsonProperty]
        public int CurrencyID { get; private set; }

        /// <summary>
        /// 每份价格
        /// </summary>
        [JsonProperty]
        public int Price { get; private set; }

        /// <summary>
        /// 原排序
        /// </summary>
        [JsonProperty]
        public int Order { get; private set; }

        /// <summary>
        /// 原配置限购上限
        /// </summary>
        [JsonProperty]
        public int Limit { get; private set; }

        /// <summary>
        /// 原物品档位
        /// </summary>
        [JsonProperty]
        public int Tier { get; private set; }

        /// <summary>
        /// 名称语言ID
        /// </summary>
        [JsonProperty]
        public int NameTextID { get; private set; }

        /// <summary>
        /// 说明语言ID
        /// </summary>
        [JsonProperty]
        public int DescriptionTextID { get; private set; }

        /// <summary>
        /// 共享图标
        /// </summary>
        [JsonProperty]
        public string IconPath { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (ShopGoods exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class ShopGoodsConfig : Config<ShopGoods>
    {
    }

    /// <summary>
    /// export excel ShopGoods data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config ShopGoodsConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, ShopGoods> shopGoodsConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static ShopGoods propShopGoodsById(int nId)
        {
            ShopGoods data;
            if (!shopGoodsConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(ShopGoods).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}