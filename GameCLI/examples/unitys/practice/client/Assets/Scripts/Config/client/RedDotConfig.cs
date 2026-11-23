using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class RedDot
    {
        /// <summary>
        /// Alias
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// Stable identifier
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// Parent identifier
        /// </summary>
        [JsonProperty]
        public int ParentID { get; private set; }

        /// <summary>
        /// Condition detector
        /// </summary>
        [JsonProperty]
        public ERedDotCondition RedDotCondition { get; private set; }

        /// <summary>
        /// Display mode
        /// </summary>
        [JsonProperty]
        public ERedDotDisplay RedDotDisplay { get; private set; }

        /// <summary>
        /// Enabled
        /// </summary>
        [JsonProperty]
        public int Enabled { get; private set; }

        /// <summary>
        /// Refresh on widget open
        /// </summary>
        [JsonProperty]
        public int RefreshOnOpen { get; private set; }

        /// <summary>
        /// Refresh on widget close
        /// </summary>
        [JsonProperty]
        public int RefreshOnClose { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (RedDot exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class RedDotConfig : Config<RedDot>
    {
    }

    /// <summary>
    /// export excel RedDot data
    /// </summary>
    public sealed partial class Excel
    {
        public const int RED_DOT_HOME = 11000;
        public const int RED_DOT_HOME_MAILTIPS = 11001;
        public const int RED_DOT_HOME_SETTIPS = 11002;
        public const int RED_DOT_HOME_ANNOUNCEMENTTIPS = 11003;
        public const int RED_DOT_HOME_FRIENDTIPS = 11004;
        public const int RED_DOT_HOME_FAVORTIPS = 11005;
        public const int RED_DOT_HOME_MANUALTIPS = 11006;
        public const int RED_DOT_HOME_TASKTIPS = 11007;
        public const int RED_DOT_HOME_REDTIPS = 11008;
        public const int RED_DOT_HOME_REDTIPS0620200 = 11009;
        public const int RED_DOT_HOME_REDTIPS0620300 = 11010;
        public const int RED_DOT_HOME_DIRECTPURCHASEREDDOT = 11011;
        public const int RED_DOT_HOME_DIRECTPURCHASE1REDDOT = 11012;
        public const int RED_DOT_HOME_CNBATTLEPASSREDDOT = 11013;
        public const int RED_DOT_HOME_TIMELIMITGIFTTIPS = 11014;
        public const int RED_DOT_HOME_UPINPROTIPS = 11015;
        public const int RED_DOT_HOME_COMEBACKREDDOT = 11016;
        public const int RED_DOT_HOME_REDDOTBAIT = 11017;
        public const int RED_DOT_HOME_REDDOTKINGSETTER = 11018;
        public const int RED_DOT_HOME_APPEARREDDOT = 11019;
        public const int RED_DOT_HOME_REDDOTPLAYERTHEME = 11020;
        public const int RED_DOT_HOME_IMGREDDOTTURNCARD = 11021;
        public const int RED_DOT_HOME_IMGREDDOTPREORDER = 11022;
        public const int RED_DOT_HOME_IMGREDDOTMATCHGUESS = 11023;
        public const int RED_DOT_HOME_IMGREDDOTTOPBATTLE = 11024;
        public const int RED_DOT_HOME_IMGREDDOTSERVEGAME = 11025;
        public const int RED_DOT_HOME_IMGREDDOTNEWYEAR = 11026;
        public const int RED_DOT_HOME_IMGREDDOTCHAINGIFT = 11027;
        public const int RED_DOT_HOME_IMGREDDOTSPTOPIC = 11028;
        public const int RED_DOT_HOME_IMGREDDOTANNIVERSARY = 11029;
        public const int RED_DOT_HOME_IMGREDDOTFREEMATCH = 11030;
        public const int RED_DOT_HOME_IMGREDDOTRUNNINGGAME = 11031;
        public const int RED_DOT_HOME_IMGREDDOT15ANNIVERSARY = 11032;
        public const int RED_DOT_HOME_IMGREDDOTLUCKYDRAW = 11033;
        public const int RED_DOT_HOME_IMGREDDOTWUYECANTEEN = 11034;
        public const int RED_DOT_HOME_IMGREDDOTACEDUEL = 11035;
        public const int RED_DOT_HOME_IMGREDDOTCAMPTRAINING = 11036;
        public const int RED_DOT_HOME_IMGREDDOTSPECIALOFFERPACKAGE = 11037;
        public const int RED_DOT_HOME_IMGREDDOTBARBECUE = 11038;
        public const int RED_DOT_HOME_IMGREDDOTCOMBINATIONGIFT = 11039;
        public const int RED_DOT_HOME_IMGREDDOTSELECTSELFGIFT = 11040;
        public const int RED_DOT_HOME_IMGREDDOTSELECTSELFGACHA = 11041;
        public const int RED_DOT_HOME_IMGREDDOTSKILLRESONANCE = 11042;
        public const int RED_DOT_HOME_IMGREDDOTNATIONALTOURNAMENT = 11043;
        public const int RED_DOT_HOME_SHOPTIPS = 11044;
        public const int RED_DOT_HOME_FULITIPS = 11045;
        public const int RED_DOT_HOME_BEIBAOTIPS = 11046;
        public const int RED_DOT_HOME_GALLERYTIPS = 11047;
        public const int RED_DOT_HOME_HONORTIPS = 11048;
        public const int RED_DOT_HOME_EXPEDITIONTIPS = 11049;
        public const int RED_DOT_HOME_HESUJIDITIPS = 11050;
        public const int RED_DOT_HOME_ZHAOMUTIPS = 11051;
        public const int RED_DOT_HOME_ASSOCIATIONTIPS = 11052;
        public const int RED_DOT_HOME_DAILYPLAYTIPS = 11053;
        /// <summary>
        /// config RedDotConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, RedDot> redDotConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static RedDot propRedDotById(int nId)
        {
            RedDot data;
            if (!redDotConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(RedDot).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}