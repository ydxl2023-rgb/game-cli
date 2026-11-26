using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class HomeBuildingItem
    {
        /// <summary>
        /// 原物品ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 名称语言表ID
        /// </summary>
        [JsonProperty]
        public int NameTextID { get; private set; }

        /// <summary>
        /// 说明语言表ID
        /// </summary>
        [JsonProperty]
        public int DescriptionTextID { get; private set; }

        /// <summary>
        /// 原FairyGUI资源标识不是Unity加载路径
        /// </summary>
        [JsonProperty]
        public string IconSource { get; private set; }

        /// <summary>
        /// 工程共享图标路径
        /// </summary>
        [JsonProperty]
        public string IconPath { get; private set; }

        /// <summary>
        /// 原基础使用时间
        /// </summary>
        [JsonProperty]
        public int UseSeconds { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (HomeBuildingItem exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class HomeBuildingItemConfig : Config<HomeBuildingItem>
    {
    }

    /// <summary>
    /// export excel HomeBuildingItem data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config HomeBuildingItemConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, HomeBuildingItem> homeBuildingItemConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static HomeBuildingItem propHomeBuildingItemById(int nId)
        {
            HomeBuildingItem data;
            if (!homeBuildingItemConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(HomeBuildingItem).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}