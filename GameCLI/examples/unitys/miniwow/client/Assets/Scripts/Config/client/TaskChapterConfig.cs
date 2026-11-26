using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class TaskChapter
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// NumID
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// TaskIDs
        /// </summary>
        [JsonProperty]
        public List<int> TaskIDs { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (TaskChapter exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class TaskChapterConfig : Config<TaskChapter>
    {
    }

    /// <summary>
    /// export excel TaskChapter data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config TaskChapterConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, TaskChapter> taskChapterConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static TaskChapter propTaskChapterById(int nId)
        {
            TaskChapter data;
            if (!taskChapterConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(TaskChapter).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}