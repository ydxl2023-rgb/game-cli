using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class Chapter
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
        /// 玩法模式
        /// </summary>
        [JsonProperty]
        public int Mode { get; private set; }

        /// <summary>
        /// 启动时安装
        /// </summary>
        [JsonProperty]
        public List<int> Levels { get; private set; }

        /// <summary>
        /// 章节奖励
        /// </summary>
        [JsonProperty]
        public List<int> Awards { get; private set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        [JsonProperty]
        public string Version { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (Chapter exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class ChapterConfig : Config<Chapter>
    {
    }

    /// <summary>
    /// export excel Chapter data
    /// </summary>
    public sealed partial class Excel
    {
        public const int CHAPTER_HOME = 10001;
        /// <summary>
        /// config ChapterConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, Chapter> chapterConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static Chapter propChapterById(int nId)
        {
            Chapter data;
            if (!chapterConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(Chapter).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}