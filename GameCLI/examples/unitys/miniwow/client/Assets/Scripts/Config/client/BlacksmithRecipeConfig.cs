using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class BlacksmithRecipe
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// Level
        /// </summary>
        [JsonProperty]
        public int Level { get; private set; }

        /// <summary>
        /// NameTextID
        /// </summary>
        [JsonProperty]
        public int NameTextID { get; private set; }

        /// <summary>
        /// Trade
        /// </summary>
        [JsonProperty]
        public int Trade { get; private set; }

        /// <summary>
        /// Price
        /// </summary>
        [JsonProperty]
        public int Price { get; private set; }

        /// <summary>
        /// IconPath
        /// </summary>
        [JsonProperty]
        public string IconPath { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (BlacksmithRecipe exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class BlacksmithRecipeConfig : Config<BlacksmithRecipe>
    {
    }

    /// <summary>
    /// export excel BlacksmithRecipe data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config BlacksmithRecipeConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, BlacksmithRecipe> blacksmithRecipeConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static BlacksmithRecipe propBlacksmithRecipeById(int nId)
        {
            BlacksmithRecipe data;
            if (!blacksmithRecipeConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(BlacksmithRecipe).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}