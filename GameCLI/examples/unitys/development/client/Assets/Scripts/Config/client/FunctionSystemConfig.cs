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
        public const int SYS_SHOP = 10010;
        public const int SYS_RED_DOT = 10020;
        public const int SYS_FUNCTION_OPEN = 10021;
        public const int SYS_ACTIVITY_TASKBUTTON = 12000;
        public const int SYS_ACTIVITY_SEVENDAYSTASK = 12002;
        public const int SYS_ACTIVITY_FOURTEENDAYSTASK = 12003;
        public const int SYS_ACTIVITY_DIRECTPURCHASEBUTTON = 12004;
        public const int SYS_ACTIVITY_DIRECTPURCHASEBUTTON1 = 12005;
        public const int SYS_ACTIVITY_CNBATTLEPASSBUTTON = 12006;
        public const int SYS_ACTIVITY_TIMELIMITGIFTBUTTON = 12007;
        public const int SYS_ACTIVITY_UPINPROBUTTON = 12008;
        public const int SYS_ACTIVITY_COMEBACKBUTTON = 12009;
        public const int SYS_ACTIVITY_POWERFULBAIT = 12010;
        public const int SYS_ACTIVITY_KINGSETTER = 12011;
        public const int SYS_ACTIVITY_ZHANSHUNODE = 12012;
        public const int SYS_ACTIVITY_FATEBATTLENODE = 12013;
        public const int SYS_ACTIVITY_YEJIUNODE = 12014;
        public const int SYS_ACTIVITY_PLAYERTHEME = 12015;
        public const int SYS_ACTIVITY_BTNTURNCARD = 12016;
        public const int SYS_ACTIVITY_ALIPAYPACKAGE = 12017;
        public const int SYS_ACTIVITY_QUESTIONNODE = 12018;
        public const int SYS_ACTIVITY_BTNPREORDER = 12019;
        public const int SYS_ACTIVITY_BTNMATCHGUESS = 12020;
        public const int SYS_ACTIVITY_BTNTOPBATTLE = 12021;
        public const int SYS_ACTIVITY_BTNSERVEGAME = 12022;
        public const int SYS_ACTIVITY_BTNNEWYEAR = 12023;
        public const int SYS_ACTIVITY_BTNCHAINGIFT = 12024;
        public const int SYS_ACTIVITY_BTNSPTOPIC = 12025;
        public const int SYS_ACTIVITY_BTNANNIVERSARY = 12026;
        public const int SYS_ACTIVITY_BTNFREEMATCH = 12027;
        public const int SYS_ACTIVITY_BTNRUNNINGGAME = 12028;
        public const int SYS_ACTIVITY_BTN15ANNIVERSARYACTIVE = 12029;
        public const int SYS_ACTIVITY_BTNLUCKYDRAW = 12030;
        public const int SYS_ACTIVITY_BTNWUYECANTEEN = 12031;
        public const int SYS_ACTIVITY_BTNACEDUEL = 12032;
        public const int SYS_ACTIVITY_BTNCAMPTRAINING = 12033;
        public const int SYS_ACTIVITY_BTNSPECIALOFFERPACKAGE = 12034;
        public const int SYS_ACTIVITY_BTNBARBECUE = 12035;
        public const int SYS_ACTIVITY_BTNCOMBINATIONGIFT = 12036;
        public const int SYS_ACTIVITY_BTNSELECTSELFGIFT = 12037;
        public const int SYS_ACTIVITY_BTNSELECTSELFGACHA = 12038;
        public const int SYS_ACTIVITY_BTNSKILLRESONANCE = 12039;
        public const int SYS_ACTIVITY_BTNNATIONALTOURNAMENT = 12040;
        public const int SYS_ACTIVITY_SEVENDAYSLOGIN = 12001;
        public const int SYS_MAIL = 10022;
        public const int SYS_SETTINGS = 10023;
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