using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class HomeBuilding
    {
        /// <summary>
        /// 原建筑ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 原功能ID
        /// </summary>
        [JsonProperty]
        public int FeatureID { get; private set; }

        /// <summary>
        /// 语言表ID
        /// </summary>
        [JsonProperty]
        public int NameTextID { get; private set; }

        /// <summary>
        /// NPC语言表ID
        /// </summary>
        [JsonProperty]
        public int NpcNameTextID { get; private set; }

        /// <summary>
        /// 原始未修复模型路径
        /// </summary>
        [JsonProperty]
        public string InitialModel { get; private set; }

        /// <summary>
        /// 原入口引导ID
        /// </summary>
        [JsonProperty]
        public int EntryGuideID { get; private set; }

        /// <summary>
        /// 医院页面对话ID
        /// </summary>
        [JsonProperty]
        public int ConversationID { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (HomeBuilding exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class HomeBuildingConfig : Config<HomeBuilding>
    {
    }

    /// <summary>
    /// export excel HomeBuilding data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config HomeBuildingConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, HomeBuilding> homeBuildingConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static HomeBuilding propHomeBuildingById(int nId)
        {
            HomeBuilding data;
            if (!homeBuildingConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(HomeBuilding).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}