using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class TaskUnlockConditions
    {
        /// <summary>
        /// Conditions
        /// </summary>
        [JsonProperty]
        public int Type { get; private set; }

        /// <summary>
        /// Conditions
        /// </summary>
        [JsonProperty]
        public int Value { get; private set; }
    }

    [System.Serializable]
    public class TaskUnlockTriggerRewards
    {
        /// <summary>
        /// TriggerRewards
        /// </summary>
        [JsonProperty]
        public int Id { get; private set; }

        /// <summary>
        /// TriggerRewards
        /// </summary>
        [JsonProperty]
        public long Num { get; private set; }
    }

    [System.Serializable]
    public class TaskUnlock
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
        /// TaskID
        /// </summary>
        [JsonProperty]
        public int TaskID { get; private set; }

        /// <summary>
        /// TaskCategory
        /// </summary>
        [JsonProperty]
        public ETaskCategory TaskCategory { get; private set; }

        /// <summary>
        /// SourceTaskType
        /// </summary>
        [JsonProperty]
        public int SourceTaskType { get; private set; }

        /// <summary>
        /// Conditions
        /// </summary>
        [JsonProperty]
        public List<TaskUnlockConditions> Conditions { get; private set; }

        /// <summary>
        /// TriggerRewards
        /// </summary>
        [JsonProperty]
        public List<TaskUnlockTriggerRewards> TriggerRewards { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (TaskUnlock exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class TaskUnlockConfig : Config<TaskUnlock>
    {
    }

    /// <summary>
    /// export excel TaskUnlock data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config TaskUnlockConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, TaskUnlock> taskUnlockConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static TaskUnlock propTaskUnlockById(int nId)
        {
            TaskUnlock data;
            if (!taskUnlockConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(TaskUnlock).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}