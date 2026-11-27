using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class RecruitHero
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// ActorID
        /// </summary>
        [JsonProperty]
        public int ActorID { get; private set; }

        /// <summary>
        /// TaskID
        /// </summary>
        [JsonProperty]
        public int TaskID { get; private set; }

        /// <summary>
        /// NameTextID
        /// </summary>
        [JsonProperty]
        public int NameTextID { get; private set; }

        /// <summary>
        /// Model
        /// </summary>
        [JsonProperty]
        public string Model { get; private set; }

        /// <summary>
        /// SkeletonPath
        /// </summary>
        [JsonProperty]
        public string SkeletonPath { get; private set; }

        /// <summary>
        /// StartX
        /// </summary>
        [JsonProperty]
        public float StartX { get; private set; }

        /// <summary>
        /// StartY
        /// </summary>
        [JsonProperty]
        public float StartY { get; private set; }

        /// <summary>
        /// StartZ
        /// </summary>
        [JsonProperty]
        public float StartZ { get; private set; }

        /// <summary>
        /// EndX
        /// </summary>
        [JsonProperty]
        public float EndX { get; private set; }

        /// <summary>
        /// EndY
        /// </summary>
        [JsonProperty]
        public float EndY { get; private set; }

        /// <summary>
        /// EndZ
        /// </summary>
        [JsonProperty]
        public float EndZ { get; private set; }

        /// <summary>
        /// InitialHealthPercent
        /// </summary>
        [JsonProperty]
        public int InitialHealthPercent { get; private set; }

        /// <summary>
        /// InitialGold
        /// </summary>
        [JsonProperty]
        public int InitialGold { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (RecruitHero exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class RecruitHeroConfig : Config<RecruitHero>
    {
    }

    /// <summary>
    /// export excel RecruitHero data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config RecruitHeroConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, RecruitHero> recruitHeroConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static RecruitHero propRecruitHeroById(int nId)
        {
            RecruitHero data;
            if (!recruitHeroConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(RecruitHero).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}