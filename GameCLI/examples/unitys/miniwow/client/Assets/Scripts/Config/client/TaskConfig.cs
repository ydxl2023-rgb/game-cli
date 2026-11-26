using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class TaskConditionItems
    {
        /// <summary>
        /// ConditionItems
        /// </summary>
        [JsonProperty]
        public int Id { get; private set; }

        /// <summary>
        /// ConditionItems
        /// </summary>
        [JsonProperty]
        public long Num { get; private set; }
    }

    [System.Serializable]
    public class TaskRewards
    {
        /// <summary>
        /// Rewards
        /// </summary>
        [JsonProperty]
        public int Id { get; private set; }

        /// <summary>
        /// Rewards
        /// </summary>
        [JsonProperty]
        public long Num { get; private set; }
    }

    [System.Serializable]
    public class Task
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
        /// Name
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// Description
        /// </summary>
        [JsonProperty]
        public string Description { get; private set; }

        /// <summary>
        /// Icon
        /// </summary>
        [JsonProperty]
        public string Icon { get; private set; }

        /// <summary>
        /// TaskConditionType
        /// </summary>
        [JsonProperty]
        public ETaskConditionType TaskConditionType { get; private set; }

        /// <summary>
        /// SourceConditionType
        /// </summary>
        [JsonProperty]
        public int SourceConditionType { get; private set; }

        /// <summary>
        /// TaskConditionShape
        /// </summary>
        [JsonProperty]
        public ETaskConditionShape TaskConditionShape { get; private set; }

        /// <summary>
        /// Param1
        /// </summary>
        [JsonProperty]
        public int Param1 { get; private set; }

        /// <summary>
        /// Param2
        /// </summary>
        [JsonProperty]
        public int Param2 { get; private set; }

        /// <summary>
        /// Param3
        /// </summary>
        [JsonProperty]
        public int Param3 { get; private set; }

        /// <summary>
        /// ConditionItems
        /// </summary>
        [JsonProperty]
        public List<TaskConditionItems> ConditionItems { get; private set; }

        /// <summary>
        /// FinishValue
        /// </summary>
        [JsonProperty]
        public int FinishValue { get; private set; }

        /// <summary>
        /// ShowTips
        /// </summary>
        [JsonProperty]
        public bool ShowTips { get; private set; }

        /// <summary>
        /// GuideID
        /// </summary>
        [JsonProperty]
        public int GuideID { get; private set; }

        /// <summary>
        /// Rewards
        /// </summary>
        [JsonProperty]
        public List<TaskRewards> Rewards { get; private set; }

        /// <summary>
        /// QuestValue
        /// </summary>
        [JsonProperty]
        public int QuestValue { get; private set; }

        /// <summary>
        /// TaskBuildingType
        /// </summary>
        [JsonProperty]
        public ETaskBuildingType TaskBuildingType { get; private set; }

        /// <summary>
        /// SourceBuildingType
        /// </summary>
        [JsonProperty]
        public int SourceBuildingType { get; private set; }

        /// <summary>
        /// NPCTalkID
        /// </summary>
        [JsonProperty]
        public int NPCTalkID { get; private set; }

        /// <summary>
        /// Priority
        /// </summary>
        [JsonProperty]
        public int Priority { get; private set; }

        /// <summary>
        /// TipsDelayTime
        /// </summary>
        [JsonProperty]
        public int TipsDelayTime { get; private set; }

        /// <summary>
        /// GuideConversationDelayTime
        /// </summary>
        [JsonProperty]
        public int GuideConversationDelayTime { get; private set; }

        /// <summary>
        /// GuideConversationID
        /// </summary>
        [JsonProperty]
        public int GuideConversationID { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (Task exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class TaskConfig : Config<Task>
    {
    }

    /// <summary>
    /// export excel Task data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config TaskConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, Task> taskConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static Task propTaskById(int nId)
        {
            Task data;
            if (!taskConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(Task).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}