using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class TaskGuide
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
        /// GroupID
        /// </summary>
        [JsonProperty]
        public int GroupID { get; private set; }

        /// <summary>
        /// TaskGuideType
        /// </summary>
        [JsonProperty]
        public ETaskGuideType TaskGuideType { get; private set; }

        /// <summary>
        /// SourceGuideType
        /// </summary>
        [JsonProperty]
        public int SourceGuideType { get; private set; }

        /// <summary>
        /// NeedSave
        /// </summary>
        [JsonProperty]
        public bool NeedSave { get; private set; }

        /// <summary>
        /// ConditionExpression
        /// </summary>
        [JsonProperty]
        public string ConditionExpression { get; private set; }

        /// <summary>
        /// ActionExpression
        /// </summary>
        [JsonProperty]
        public string ActionExpression { get; private set; }

        /// <summary>
        /// NextID
        /// </summary>
        [JsonProperty]
        public int NextID { get; private set; }

        /// <summary>
        /// FailureNextID
        /// </summary>
        [JsonProperty]
        public int FailureNextID { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (TaskGuide exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class TaskGuideConfig : Config<TaskGuide>
    {
    }

    /// <summary>
    /// export excel TaskGuide data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config TaskGuideConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, TaskGuide> taskGuideConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static TaskGuide propTaskGuideById(int nId)
        {
            TaskGuide data;
            if (!taskGuideConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(TaskGuide).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}