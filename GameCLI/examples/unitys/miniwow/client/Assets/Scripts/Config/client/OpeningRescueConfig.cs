using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class OpeningRescue
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// TaskID
        /// </summary>
        [JsonProperty]
        public int TaskID { get; private set; }

        /// <summary>
        /// BuildingID
        /// </summary>
        [JsonProperty]
        public int BuildingID { get; private set; }

        /// <summary>
        /// WaterCost
        /// </summary>
        [JsonProperty]
        public int WaterCost { get; private set; }

        /// <summary>
        /// TargetX
        /// </summary>
        [JsonProperty]
        public float TargetX { get; private set; }

        /// <summary>
        /// TargetY
        /// </summary>
        [JsonProperty]
        public float TargetY { get; private set; }

        /// <summary>
        /// TargetZ
        /// </summary>
        [JsonProperty]
        public float TargetZ { get; private set; }

        /// <summary>
        /// WaterTargetX
        /// </summary>
        [JsonProperty]
        public float WaterTargetX { get; private set; }

        /// <summary>
        /// WaterTargetY
        /// </summary>
        [JsonProperty]
        public float WaterTargetY { get; private set; }

        /// <summary>
        /// WaterTargetZ
        /// </summary>
        [JsonProperty]
        public float WaterTargetZ { get; private set; }

        /// <summary>
        /// DialogueID
        /// </summary>
        [JsonProperty]
        public int DialogueID { get; private set; }

        /// <summary>
        /// ObjectiveTextID
        /// </summary>
        [JsonProperty]
        public int ObjectiveTextID { get; private set; }

        /// <summary>
        /// DurationSeconds
        /// </summary>
        [JsonProperty]
        public float DurationSeconds { get; private set; }

        /// <summary>
        /// ActionTextID
        /// </summary>
        [JsonProperty]
        public int ActionTextID { get; private set; }

        /// <summary>
        /// AutoClaim
        /// </summary>
        [JsonProperty]
        public int AutoClaim { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (OpeningRescue exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class OpeningRescueConfig : Config<OpeningRescue>
    {
    }

    /// <summary>
    /// export excel OpeningRescue data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config OpeningRescueConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, OpeningRescue> openingRescueConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static OpeningRescue propOpeningRescueById(int nId)
        {
            OpeningRescue data;
            if (!openingRescueConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(OpeningRescue).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}