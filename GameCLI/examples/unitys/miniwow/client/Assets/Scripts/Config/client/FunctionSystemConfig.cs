using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class FunctionSystem
    {
        /// <summary>
        /// Alias
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// Numeric ID
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// Class type
        /// </summary>
        [JsonProperty]
        public string ClassType { get; private set; }

        /// <summary>
        /// Install on startup
        /// </summary>
        [JsonProperty]
        public int InstallOnStartup { get; private set; }

        /// <summary>
        /// Active
        /// </summary>
        [JsonProperty]
        public int Active { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (FunctionSystem exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class FunctionSystemConfig : Config<FunctionSystem>
    {
    }

    /// <summary>
    /// export excel FunctionSystem data
    /// </summary>
    public sealed partial class Excel
    {
        public const int SYS_MASTER = 10001;
        public const int SYS_LOGIN = 10002;
        public const int SYS_HOME = 10003;
        /// <summary>
        /// config FunctionSystemConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, FunctionSystem> functionSystemConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static FunctionSystem propFunctionSystemById(int nId)
        {
            FunctionSystem data;
            if (!functionSystemConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(FunctionSystem).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}