using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class BlacksmithHero
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// Trade
        /// </summary>
        [JsonProperty]
        public int Trade { get; private set; }

        /// <summary>
        /// NameTextID
        /// </summary>
        [JsonProperty]
        public int NameTextID { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (BlacksmithHero exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class BlacksmithHeroConfig : Config<BlacksmithHero>
    {
    }

    /// <summary>
    /// export excel BlacksmithHero data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config BlacksmithHeroConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, BlacksmithHero> blacksmithHeroConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static BlacksmithHero propBlacksmithHeroById(int nId)
        {
            BlacksmithHero data;
            if (!blacksmithHeroConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(BlacksmithHero).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}