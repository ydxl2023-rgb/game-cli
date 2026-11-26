using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class HomeOpening
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
        /// NextTaskID
        /// </summary>
        [JsonProperty]
        public int NextTaskID { get; private set; }

        /// <summary>
        /// WaterItemID
        /// </summary>
        [JsonProperty]
        public int WaterItemID { get; private set; }

        /// <summary>
        /// WaterAmount
        /// </summary>
        [JsonProperty]
        public int WaterAmount { get; private set; }

        /// <summary>
        /// WaterX
        /// </summary>
        [JsonProperty]
        public float WaterX { get; private set; }

        /// <summary>
        /// WaterY
        /// </summary>
        [JsonProperty]
        public float WaterY { get; private set; }

        /// <summary>
        /// WaterZ
        /// </summary>
        [JsonProperty]
        public float WaterZ { get; private set; }

        /// <summary>
        /// WaterSizeX
        /// </summary>
        [JsonProperty]
        public float WaterSizeX { get; private set; }

        /// <summary>
        /// WaterSizeY
        /// </summary>
        [JsonProperty]
        public float WaterSizeY { get; private set; }

        /// <summary>
        /// WaterSizeZ
        /// </summary>
        [JsonProperty]
        public float WaterSizeZ { get; private set; }

        /// <summary>
        /// InteractionRadius
        /// </summary>
        [JsonProperty]
        public float InteractionRadius { get; private set; }

        /// <summary>
        /// IntroDialogueID
        /// </summary>
        [JsonProperty]
        public int IntroDialogueID { get; private set; }

        /// <summary>
        /// VideoPath
        /// </summary>
        [JsonProperty]
        public string VideoPath { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (HomeOpening exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class HomeOpeningConfig : Config<HomeOpening>
    {
    }

    /// <summary>
    /// export excel HomeOpening data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config HomeOpeningConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, HomeOpening> homeOpeningConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static HomeOpening propHomeOpeningById(int nId)
        {
            HomeOpening data;
            if (!homeOpeningConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(HomeOpening).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}