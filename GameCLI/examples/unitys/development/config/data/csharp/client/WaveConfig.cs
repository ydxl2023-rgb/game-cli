using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class WaveGroup
    {
        /// <summary>
        /// 刷怪信息
        /// </summary>
        [JsonProperty]
        public int Weight { get; private set; }

        /// <summary>
        /// 刷怪信息
        /// </summary>
        [JsonProperty]
        public int Number { get; private set; }

        /// <summary>
        /// 刷怪信息
        /// </summary>
        [JsonProperty]
        public int Factor { get; private set; }
    }

    [System.Serializable]
    public class Wave
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
        /// 刷怪信息
        /// </summary>
        [JsonProperty]
        public List<WaveGroup> Group { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        public string Version { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (Wave exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class WaveConfig : Config<Wave>
    {
    }

    /// <summary>
    /// export excel Wave data
    /// </summary>
    public sealed partial class Excel
    {
        public const int WAVE_NORMAL = 10001;
        public const int WAVE_ELITE = 10002;
        public const int WAVE_BOSS = 10003;
        /// <summary>
        /// config WaveConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, Wave> waveConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static Wave propWaveById(int nId)
        {
            Wave data;
            if (!waveConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(Wave).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}