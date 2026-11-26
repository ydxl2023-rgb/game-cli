using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class HomeBuildingGoods
    {
        /// <summary>
        /// 等级ID乘10加商品序号
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 等级ID
        /// </summary>
        [JsonProperty]
        public int LevelID { get; private set; }

        /// <summary>
        /// 原商品零基序号
        /// </summary>
        [JsonProperty]
        public int Order { get; private set; }

        /// <summary>
        /// 制作原料ID
        /// </summary>
        [JsonProperty]
        public int MaterialID { get; private set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        [JsonProperty]
        public int ProductID { get; private set; }

        /// <summary>
        /// 英雄消费基础价格不是制作费用
        /// </summary>
        [JsonProperty]
        public int ServiceBasePrice { get; private set; }

        /// <summary>
        /// 按原算法推导负1表示证据不完整禁止执行
        /// </summary>
        [JsonProperty]
        public int RecoveryPercent { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (HomeBuildingGoods exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class HomeBuildingGoodsConfig : Config<HomeBuildingGoods>
    {
    }

    /// <summary>
    /// export excel HomeBuildingGoods data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config HomeBuildingGoodsConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, HomeBuildingGoods> homeBuildingGoodsConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static HomeBuildingGoods propHomeBuildingGoodsById(int nId)
        {
            HomeBuildingGoods data;
            if (!homeBuildingGoodsConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(HomeBuildingGoods).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}