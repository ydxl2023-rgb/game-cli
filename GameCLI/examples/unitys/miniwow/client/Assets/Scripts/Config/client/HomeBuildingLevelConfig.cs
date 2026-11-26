using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class HomeBuildingLevel
    {
        /// <summary>
        /// 原等级ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 建筑ID
        /// </summary>
        [JsonProperty]
        public int BuildingID { get; private set; }

        /// <summary>
        /// 等级
        /// </summary>
        [JsonProperty]
        public int Level { get; private set; }

        /// <summary>
        /// 战力
        /// </summary>
        [JsonProperty]
        public int Power { get; private set; }

        /// <summary>
        /// 原始建成模型路径
        /// </summary>
        [JsonProperty]
        public string Model { get; private set; }

        /// <summary>
        /// 建造时间
        /// </summary>
        [JsonProperty]
        public int BuildSeconds { get; private set; }

        /// <summary>
        /// 服务价格加成百分比
        /// </summary>
        [JsonProperty]
        public int PricePercent { get; private set; }

        /// <summary>
        /// 使用时间减少秒数
        /// </summary>
        [JsonProperty]
        public int CutSeconds { get; private set; }

        /// <summary>
        /// 同时消费人数
        /// </summary>
        [JsonProperty]
        public int Capacity { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (HomeBuildingLevel exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class HomeBuildingLevelConfig : Config<HomeBuildingLevel>
    {
    }

    /// <summary>
    /// export excel HomeBuildingLevel data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config HomeBuildingLevelConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, HomeBuildingLevel> homeBuildingLevelConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static HomeBuildingLevel propHomeBuildingLevelById(int nId)
        {
            HomeBuildingLevel data;
            if (!homeBuildingLevelConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(HomeBuildingLevel).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}