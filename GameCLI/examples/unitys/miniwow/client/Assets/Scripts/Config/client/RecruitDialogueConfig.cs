using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class RecruitDialogue
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// GroupID
        /// </summary>
        [JsonProperty]
        public int GroupID { get; private set; }

        /// <summary>
        /// SpeakerTextID
        /// </summary>
        [JsonProperty]
        public int SpeakerTextID { get; private set; }

        /// <summary>
        /// ContentTextID
        /// </summary>
        [JsonProperty]
        public int ContentTextID { get; private set; }

        /// <summary>
        /// PortraitPath
        /// </summary>
        [JsonProperty]
        public string PortraitPath { get; private set; }

        /// <summary>
        /// Direction
        /// </summary>
        [JsonProperty]
        public int Direction { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (RecruitDialogue exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class RecruitDialogueConfig : Config<RecruitDialogue>
    {
    }

    /// <summary>
    /// export excel RecruitDialogue data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config RecruitDialogueConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, RecruitDialogue> recruitDialogueConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static RecruitDialogue propRecruitDialogueById(int nId)
        {
            RecruitDialogue data;
            if (!recruitDialogueConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(RecruitDialogue).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}