using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class OpeningDialogue
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

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
        /// override bool operator
        /// </summary>
        public static implicit operator bool (OpeningDialogue exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class OpeningDialogueConfig : Config<OpeningDialogue>
    {
    }

    /// <summary>
    /// export excel OpeningDialogue data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config OpeningDialogueConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, OpeningDialogue> openingDialogueConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static OpeningDialogue propOpeningDialogueById(int nId)
        {
            OpeningDialogue data;
            if (!openingDialogueConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(OpeningDialogue).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}