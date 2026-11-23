using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class Scene
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
        /// 
        /// </summary>
        [JsonProperty]
        public string Music { get; private set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        [JsonProperty]
        public string Version { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (Scene exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class SceneConfig : Config<Scene>
    {
    }

    /// <summary>
    /// export excel Scene data
    /// </summary>
    public sealed partial class Excel
    {
        public const int SCENE_APP = 10001;
        public const int SCENE_HOME = 10002;
        /// <summary>
        /// config SceneConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, Scene> sceneConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static Scene propSceneById(int nId)
        {
            Scene data;
            if (!sceneConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(Scene).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}