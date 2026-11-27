using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class BlacksmithMaterial
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// NameTextID
        /// </summary>
        [JsonProperty]
        public int NameTextID { get; private set; }

        /// <summary>
        /// IconPath
        /// </summary>
        [JsonProperty]
        public string IconPath { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (BlacksmithMaterial exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class BlacksmithMaterialConfig : Config<BlacksmithMaterial>
    {
    }

    /// <summary>
    /// export excel BlacksmithMaterial data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config BlacksmithMaterialConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, BlacksmithMaterial> blacksmithMaterialConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static BlacksmithMaterial propBlacksmithMaterialById(int nId)
        {
            BlacksmithMaterial data;
            if (!blacksmithMaterialConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(BlacksmithMaterial).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}