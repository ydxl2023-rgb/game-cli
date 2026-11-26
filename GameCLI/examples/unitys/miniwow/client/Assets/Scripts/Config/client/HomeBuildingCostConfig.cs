using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class HomeBuildingCost
    {
        /// <summary>
        /// 等级ID乘10加费用序号
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 目标等级ID
        /// </summary>
        [JsonProperty]
        public int LevelID { get; private set; }

        /// <summary>
        /// 消耗物品
        /// </summary>
        [JsonProperty]
        public int ItemID { get; private set; }

        /// <summary>
        /// 数量
        /// </summary>
        [JsonProperty]
        public int Amount { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (HomeBuildingCost exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class HomeBuildingCostConfig : Config<HomeBuildingCost>
    {
    }

    /// <summary>
    /// export excel HomeBuildingCost data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config HomeBuildingCostConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, HomeBuildingCost> homeBuildingCostConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static HomeBuildingCost propHomeBuildingCostById(int nId)
        {
            HomeBuildingCost data;
            if (!homeBuildingCostConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(HomeBuildingCost).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}