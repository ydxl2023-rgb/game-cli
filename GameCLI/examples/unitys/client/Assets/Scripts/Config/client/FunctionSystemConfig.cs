using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class FunctionSystem
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
        /// 
        /// </summary>
        [JsonProperty]
        public ETargetType TargetType { get; private set; }

        /// <summary>
        /// 系统类型名
        /// </summary>
        [JsonProperty]
        public string ClassType { get; private set; }

        /// <summary>
        /// 启动时安装
        /// </summary>
        [JsonProperty]
        public int InstallOnStartup { get; private set; }

        /// <summary>
        /// 开启条件
        /// </summary>
        [JsonProperty]
        public List<string> Condtions { get; private set; }

        /// <summary>
        /// 是否立即激活
        /// </summary>
        [JsonProperty]
        public int Active { get; private set; }

        /// <summary>
        /// 开启时间
        /// </summary>
        [JsonProperty]
        public string StartTime { get; private set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [JsonProperty]
        public string StopTime { get; private set; }

        /// <summary>
        /// 场景ID
        /// </summary>
        [JsonProperty]
        public int SceneId { get; private set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        [JsonProperty]
        public string Version { get; private set; }

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
        public const int SYS_EMAIL = 10004;
        public const int SYS_CHARACTER = 10005;
        public const int SYS_BACKPACK = 10006;
        public const int SYS_SEVENDAYLOGIN = 10007;
        public const int SYS_FIGHT = 10008;
        public const int SYS_NOTICE = 10009;
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