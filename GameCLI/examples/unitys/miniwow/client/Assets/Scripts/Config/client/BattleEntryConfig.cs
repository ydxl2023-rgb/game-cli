using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class BattleEntry
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// AreaID
        /// </summary>
        [JsonProperty]
        public int AreaID { get; private set; }

        /// <summary>
        /// RequiredTaskID
        /// </summary>
        [JsonProperty]
        public int RequiredTaskID { get; private set; }

        /// <summary>
        /// TutorialTaskID
        /// </summary>
        [JsonProperty]
        public int TutorialTaskID { get; private set; }

        /// <summary>
        /// RecommendPower
        /// </summary>
        [JsonProperty]
        public int RecommendPower { get; private set; }

        /// <summary>
        /// TitleTextID
        /// </summary>
        [JsonProperty]
        public int TitleTextID { get; private set; }

        /// <summary>
        /// NameTextID
        /// </summary>
        [JsonProperty]
        public int NameTextID { get; private set; }

        /// <summary>
        /// DescriptionTextID
        /// </summary>
        [JsonProperty]
        public int DescriptionTextID { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (BattleEntry exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class BattleEntryConfig : Config<BattleEntry>
    {
    }

    /// <summary>
    /// export excel BattleEntry data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config BattleEntryConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, BattleEntry> battleEntryConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static BattleEntry propBattleEntryById(int nId)
        {
            BattleEntry data;
            if (!battleEntryConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(BattleEntry).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}