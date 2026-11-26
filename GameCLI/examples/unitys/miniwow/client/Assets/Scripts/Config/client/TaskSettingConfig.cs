using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class TaskSetting
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// InitialChapterID
        /// </summary>
        [JsonProperty]
        public int InitialChapterID { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (TaskSetting exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class TaskSettingConfig : Config<TaskSetting>
    {
    }

    /// <summary>
    /// export excel TaskSetting data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config TaskSettingConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, TaskSetting> taskSettingConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static TaskSetting propTaskSettingById(int nId)
        {
            TaskSetting data;
            if (!taskSettingConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(TaskSetting).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}