using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class SceneEffectParamEffect
    {
        /// <summary>
        /// 特效播放参数
        /// </summary>
        [JsonProperty]
        public int Id { get; private set; }

        /// <summary>
        /// 特效播放参数
        /// </summary>
        [JsonProperty]
        public string Path { get; private set; }
    }

    [System.Serializable]
    public class SceneEffectParam
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
        /// 路径
        /// </summary>
        [JsonProperty]
        public string Path { get; private set; }

        /// <summary>
        /// 开启时间
        /// </summary>
        [JsonProperty]
        public string Time { get; private set; }

        /// <summary>
        /// 特效播放参数
        /// </summary>
        [JsonProperty]
        public SceneEffectParamEffect Effect { get; private set; }

        /// <summary>
        /// 道具
        /// </summary>
        [JsonProperty]
        public List<int> Items { get; private set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        [JsonProperty]
        public string Version { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (SceneEffectParam exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class SceneEffectParamConfig : Config<SceneEffectParam>
    {
    }

    /// <summary>
    /// export excel SceneEffectParam data
    /// </summary>
    public sealed partial class Excel
    {
        public const int SCENE_EFFECT_PARAM_10001 = 10001;
        /// <summary>
        /// config SceneEffectParamConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, SceneEffectParam> sceneEffectParamConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static SceneEffectParam propSceneEffectParamById(int nId)
        {
            SceneEffectParam data;
            if (!sceneEffectParamConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(SceneEffectParam).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}