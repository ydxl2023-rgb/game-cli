using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class VillageTask
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// Action
        /// </summary>
        [JsonProperty]
        public int Action { get; private set; }

        /// <summary>
        /// RequiredTaskID
        /// </summary>
        [JsonProperty]
        public int RequiredTaskID { get; private set; }

        /// <summary>
        /// TitleTextID
        /// </summary>
        [JsonProperty]
        public int TitleTextID { get; private set; }

        /// <summary>
        /// DetailTextID
        /// </summary>
        [JsonProperty]
        public int DetailTextID { get; private set; }

        /// <summary>
        /// ActionTextID
        /// </summary>
        [JsonProperty]
        public int ActionTextID { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (VillageTask exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class VillageTaskConfig : Config<VillageTask>
    {
    }

    /// <summary>
    /// export excel VillageTask data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config VillageTaskConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, VillageTask> villageTaskConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static VillageTask propVillageTaskById(int nId)
        {
            VillageTask data;
            if (!villageTaskConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(VillageTask).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}