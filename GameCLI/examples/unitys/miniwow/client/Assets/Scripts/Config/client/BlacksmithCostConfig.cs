using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class BlacksmithCost
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// RecipeID
        /// </summary>
        [JsonProperty]
        public int RecipeID { get; private set; }

        /// <summary>
        /// ItemID
        /// </summary>
        [JsonProperty]
        public int ItemID { get; private set; }

        /// <summary>
        /// Amount
        /// </summary>
        [JsonProperty]
        public int Amount { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (BlacksmithCost exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class BlacksmithCostConfig : Config<BlacksmithCost>
    {
    }

    /// <summary>
    /// export excel BlacksmithCost data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config BlacksmithCostConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, BlacksmithCost> blacksmithCostConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static BlacksmithCost propBlacksmithCostById(int nId)
        {
            BlacksmithCost data;
            if (!blacksmithCostConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(BlacksmithCost).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}