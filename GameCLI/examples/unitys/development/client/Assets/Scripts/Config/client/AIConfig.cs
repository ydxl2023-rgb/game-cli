using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class AI
    {
        /// <summary>
        /// 别名
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 数字ID
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// 组ID
        /// </summary>
        [JsonProperty]
        public int GroupId { get; private set; }

        /// <summary>
        /// 等级
        /// </summary>
        [JsonProperty]
        public int Level { get; private set; }

        /// <summary>
        /// 警戒半径（米）
        /// </summary>
        [JsonProperty]
        public int AlertRadius { get; private set; }

        /// <summary>
        /// 跟随半径（米）
        /// </summary>
        [JsonProperty]
        public int FollowRadius { get; private set; }

        /// <summary>
        /// 追击时间(秒)
        /// </summary>
        [JsonProperty]
        public int PursuitDuration { get; private set; }

        /// <summary>
        /// 思考时间
        /// </summary>
        [JsonProperty]
        public List<float> ThinkTime { get; private set; }

        /// <summary>
        /// 版本
        /// </summary>
        [JsonProperty]
        public int Version { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (AI exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class AIConfig : Config<AI>
    {
    }

    /// <summary>
    /// export excel AI data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config AIConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, AI> aIConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static AI propAIById(int nId)
        {
            AI data;
            if (!aIConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(AI).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}