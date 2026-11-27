using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class ExploreJourney
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// HeroID
        /// </summary>
        [JsonProperty]
        public int HeroID { get; private set; }

        /// <summary>
        /// ActorID
        /// </summary>
        [JsonProperty]
        public int ActorID { get; private set; }

        /// <summary>
        /// RequiredTaskID
        /// </summary>
        [JsonProperty]
        public int RequiredTaskID { get; private set; }

        /// <summary>
        /// EventID
        /// </summary>
        [JsonProperty]
        public int EventID { get; private set; }

        /// <summary>
        /// AreaID
        /// </summary>
        [JsonProperty]
        public int AreaID { get; private set; }

        /// <summary>
        /// EventType
        /// </summary>
        [JsonProperty]
        public int EventType { get; private set; }

        /// <summary>
        /// ExecuteSeconds
        /// </summary>
        [JsonProperty]
        public float ExecuteSeconds { get; private set; }

        /// <summary>
        /// TravelSeconds
        /// </summary>
        [JsonProperty]
        public float TravelSeconds { get; private set; }

        /// <summary>
        /// ExitX
        /// </summary>
        [JsonProperty]
        public float ExitX { get; private set; }

        /// <summary>
        /// ExitY
        /// </summary>
        [JsonProperty]
        public float ExitY { get; private set; }

        /// <summary>
        /// ExitZ
        /// </summary>
        [JsonProperty]
        public float ExitZ { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (ExploreJourney exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class ExploreJourneyConfig : Config<ExploreJourney>
    {
    }

    /// <summary>
    /// export excel ExploreJourney data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config ExploreJourneyConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, ExploreJourney> exploreJourneyConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static ExploreJourney propExploreJourneyById(int nId)
        {
            ExploreJourney data;
            if (!exploreJourneyConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(ExploreJourney).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}