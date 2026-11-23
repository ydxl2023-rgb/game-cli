using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class Level
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
        /// 名称
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// 背景音乐
        /// </summary>
        [JsonProperty]
        public List<string> Bgms { get; private set; }

        /// <summary>
        /// 路径
        /// </summary>
        [JsonProperty]
        public string Path { get; private set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        [JsonProperty]
        public string Version { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (Level exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class LevelConfig : Config<Level>
    {
    }

    /// <summary>
    /// export excel Level data
    /// </summary>
    public sealed partial class Excel
    {
        public const int LEVEL_HOME = 10001;
        /// <summary>
        /// config LevelConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, Level> levelConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static Level propLevelById(int nId)
        {
            Level data;
            if (!levelConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(Level).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}