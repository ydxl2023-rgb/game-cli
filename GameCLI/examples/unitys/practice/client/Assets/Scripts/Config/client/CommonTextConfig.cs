using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class CommonText
    {
        /// <summary>
        /// 别名ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 数字ID
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// 文本内容
        /// </summary>
        [JsonProperty]
        public string Context { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (CommonText exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class CommonTextConfig : Config<CommonText>
    {
    }

    /// <summary>
    /// export excel CommonText data
    /// </summary>
    public sealed partial class Excel
    {
        public const int ERR_DODGE = 101;
        public const int ERR_BLOCK = 102;
        public const int ERR_NOHIT = 103;
        public const int LOGIN_RECENT_SERVER = 1001;
        public const int LOGIN_RECOMMENDED_SERVER = 1002;
        public const int LOGIN_SERVER_RANGE = 1003;
        public const int LOGIN_LOCAL_SERVER_NAME = 1004;
        public const int LOGIN_SERVER_NORMAL = 1005;
        public const int LOGIN_SERVER_BUSY = 1006;
        public const int LOGIN_SERVER_MAINTENANCE = 1007;
        public const int LOGIN_SERVER_DEFAULT = 1101;
        public const int LOGIN_INTEGRATION_PENDING = 1102;
        public const int NOTICE_EMPTY = 1103;
        public const int HOME_TEXT_0_6_3_0_0_1 = 2001;
        public const int HOME_TEXT_0_6_3_1_0 = 2002;
        public const int HOME_TEXT_0_6_3_2_0 = 2003;
        public const int HOME_TEXT_0_6_4_1_0 = 2004;
        public const int HOME_TEXT_0_6_4_2_0 = 2005;
        public const int HOME_TEXT_0_6_4_3_0 = 2006;
        public const int HOME_TEXT_0_6_4_4_4 = 2007;
        public const int HOME_TEXT_0_6_5_3_4 = 2008;
        public const int HOME_TEXT_0_6_5_4_4 = 2009;
        public const int HOME_TEXT_0_6_5_5_0 = 2010;
        public const int HOME_TEXT_0_6_5_6_0 = 2011;
        public const int HOME_TEXT_0_6_2_0_5_2 = 3025;
        public const int HOME_TEXT_0_6_2_0_6_2 = 3027;
        public const int HOME_TEXT_0_6_2_0_12_0_0_0 = 3033;
        public const int HOME_TEXT_0_6_2_0_12_0_1_0 = 3034;
        public const int HOME_TEXT_0_6_2_0_13_0_0_0 = 3035;
        public const int HOME_TEXT_0_6_2_0_13_0_0_5_0 = 3038;
        public const int HOME_TEXT_0_6_2_0_13_0_1_0 = 3039;
        public const int HOME_TEXT_0_6_2_0_14_0_2_0 = 3040;
        public const int HOME_TEXT_0_6_3_0_2_1 = 3062;
        public const int SHOP_RECHARGE = 3001;
        public const int SHOP_GIFTS = 3002;
        public const int SHOP_DEMO_NOTICE = 3003;
        public const int SHOP_PRICE = 3004;
        public const int SHOP_TOKEN_AMOUNT = 3005;
        public const int SHOP_GIFT_SMALL = 3006;
        public const int SHOP_GIFT_MEDIUM = 3007;
        public const int SHOP_GIFT_LARGE = 3008;
        public const int SHOP_LIMIT = 3009;
        public const int SHOP_SOLD_OUT = 3010;
        public const int SHOP_CONFIRM = 3011;
        public const int SHOP_REWARDS = 3012;
        public const int SHOP_TOKEN_NAME = 3013;
        public const int SHOP_COUNT = 3014;
        public const int SHOP_SUCCESS = 3015;
        public const int SHOP_INSUFFICIENT = 3016;
        public const int SHOP_NO_SELECTION = 3017;
        public const int SHOP_CAPACITY = 3018;
        public const int SHOP_HELP = 3019;
        public const int SHOP_BALANCE = 3020;
        public const int SHOP_FIRST_PAY = 3021;
        /// <summary>
        /// config CommonTextConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, CommonText> commonTextConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static CommonText propCommonTextById(int nId)
        {
            CommonText data;
            if (!commonTextConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(CommonText).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}