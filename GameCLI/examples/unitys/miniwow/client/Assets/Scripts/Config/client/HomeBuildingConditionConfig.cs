using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class HomeBuildingCondition
    {
        /// <summary>
        /// 数字条件行ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 1功能开放2目标等级
        /// </summary>
        [JsonProperty]
        public int OwnerKind { get; private set; }

        /// <summary>
        /// 所属配置ID
        /// </summary>
        [JsonProperty]
        public int OwnerID { get; private set; }

        /// <summary>
        /// 原UnLockType枚举值
        /// </summary>
        [JsonProperty]
        public int Type { get; private set; }

        /// <summary>
        /// 条件参数
        /// </summary>
        [JsonProperty]
        public int Value { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (HomeBuildingCondition exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class HomeBuildingConditionConfig : Config<HomeBuildingCondition>
    {
    }

    /// <summary>
    /// export excel HomeBuildingCondition data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config HomeBuildingConditionConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, HomeBuildingCondition> homeBuildingConditionConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static HomeBuildingCondition propHomeBuildingConditionById(int nId)
        {
            HomeBuildingCondition data;
            if (!homeBuildingConditionConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(HomeBuildingCondition).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}