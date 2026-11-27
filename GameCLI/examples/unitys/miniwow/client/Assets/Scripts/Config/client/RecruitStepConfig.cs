using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class RecruitStep
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// Kind
        /// </summary>
        [JsonProperty]
        public int Kind { get; private set; }

        /// <summary>
        /// BuildingID
        /// </summary>
        [JsonProperty]
        public int BuildingID { get; private set; }

        /// <summary>
        /// RequiredTaskID
        /// </summary>
        [JsonProperty]
        public int RequiredTaskID { get; private set; }

        /// <summary>
        /// DialogueGroupID
        /// </summary>
        [JsonProperty]
        public int DialogueGroupID { get; private set; }

        /// <summary>
        /// HeroID
        /// </summary>
        [JsonProperty]
        public int HeroID { get; private set; }

        /// <summary>
        /// CostItemID
        /// </summary>
        [JsonProperty]
        public int CostItemID { get; private set; }

        /// <summary>
        /// CostCount
        /// </summary>
        [JsonProperty]
        public int CostCount { get; private set; }

        /// <summary>
        /// AutoClaim
        /// </summary>
        [JsonProperty]
        public int AutoClaim { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (RecruitStep exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class RecruitStepConfig : Config<RecruitStep>
    {
    }

    /// <summary>
    /// export excel RecruitStep data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config RecruitStepConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, RecruitStep> recruitStepConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static RecruitStep propRecruitStepById(int nId)
        {
            RecruitStep data;
            if (!recruitStepConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(RecruitStep).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}