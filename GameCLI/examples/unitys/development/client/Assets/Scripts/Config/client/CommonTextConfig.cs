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
        public const int HOME_TEXT_0_6_2_0_5_2 = 3025;
        public const int HOME_TEXT_0_6_2_0_6_2 = 3027;
        public const int HOME_TEXT_0_6_2_0_12_0_0_0 = 3033;
        public const int HOME_TEXT_0_6_2_0_12_0_1_0 = 3034;
        public const int HOME_TEXT_0_6_2_0_13_0_0_0 = 3035;
        public const int HOME_TEXT_0_6_2_0_13_0_0_5_0 = 3038;
        public const int HOME_TEXT_0_6_2_0_13_0_1_0 = 3039;
        public const int HOME_TEXT_0_6_2_0_14_0_2_0 = 3040;
        public const int HOME_TEXT_0_6_3_0_2_1 = 3062;
        public const int SHOP_PAGE_10102_1_NAME = 20000;
        public const int SHOP_PAGE_10102_2_NAME = 20001;
        public const int SHOP_PAGE_10102_3_NAME = 20002;
        public const int SHOP_PAGE_10102_4_NAME = 20003;
        public const int SHOP_PAGE_10102_5_NAME = 20004;
        public const int SHOP_PAGE_10102_6_NAME = 20005;
        public const int SHOP_PAGE_10102_7_NAME = 20006;
        public const int SHOP_PAGE_10102_8_NAME = 20007;
        public const int SHOP_PAGE_10102_9_NAME = 20008;
        public const int SHOP_PAGE_10102_10_NAME = 20009;
        public const int SHOP_PAGE_10102_11_NAME = 20010;
        public const int SHOP_PAGE_10102_12_NAME = 20011;
        public const int SHOP_PAGE_10102_13_NAME = 20012;
        public const int SHOP_PAGE_10102_14_NAME = 20013;
        public const int SHOP_PAGE_10102_15_NAME = 20014;
        public const int SHOP_PAGE_10102_16_NAME = 20015;
        public const int SHOP_PAGE_10102_17_NAME = 20016;
        public const int SHOP_PAGE_10102_18_NAME = 20017;
        public const int SHOP_PAGE_10103_1_NAME = 20018;
        public const int SHOP_PAGE_10103_6_NAME = 20019;
        public const int SHOP_PAGE_10103_7_NAME = 20020;
        public const int SHOP_PAGE_10103_8_NAME = 20021;
        public const int SHOP_PAGE_10103_9_NAME = 20022;
        public const int SHOP_PAGE_10104_4_NAME = 20023;
        public const int SHOP_PAGE_10104_6_NAME = 20024;
        public const int SHOP_PAGE_10105_9_NAME = 20025;
        public const int SHOP_PAGE_10105_10_NAME = 20026;
        public const int SHOP_PAGE_10105_11_NAME = 20027;
        public const int SHOP_PAGE_10105_12_NAME = 20028;
        public const int SHOP_PAGE_10105_13_NAME = 20029;
        public const int SHOP_PAGE_10105_14_NAME = 20030;
        public const int SHOP_PAGE_10105_15_NAME = 20031;
        public const int SHOP_PAGE_10105_16_NAME = 20032;
        public const int SHOP_PAGE_10105_17_NAME = 20033;
        public const int SHOP_PAGE_10105_18_NAME = 20034;
        public const int SHOP_PAGE_10302_1_NAME = 20035;
        public const int SHOP_PAGE_10302_2_PRICE = 20036;
        public const int SHOP_PAGE_10302_4_NAME = 20037;
        public const int SHOP_PAGE_10302_6_NAME = 20038;
        public const int SHOP_PAGE_10302_8_NAME = 20039;
        public const int SHOP_PAGE_10302_9_NAME = 20040;
        public const int SHOP_PAGE_10302_10_NAME = 20041;
        public const int SHOP_PAGE_10802_1_NAME = 20042;
        public const int SHOP_PAGE_10802_2_NAME = 20043;
        public const int SHOP_PAGE_10802_3_NAME = 20044;
        public const int SHOP_PAGE_10802_4_NAME = 20045;
        public const int SHOP_PAGE_10802_5_NAME = 20046;
        public const int SHOP_PAGE_10802_6_NAME = 20047;
        public const int SHOP_PAGE_10802_7_NAME = 20048;
        public const int SHOP_PAGE_10802_8_NAME = 20049;
        public const int SHOP_PAGE_10802_9_NAME = 20050;
        public const int SHOP_PAGE_10802_10_NAME = 20051;
        public const int SHOP_PAGE_10802_11_NAME = 20052;
        public const int SHOP_PAGE_10802_12_NAME = 20053;
        public const int SHOP_PAGE_10802_13_NAME = 20054;
        public const int SHOP_PAGE_10802_14_NAME = 20055;
        public const int SHOP_PAGE_10802_15_NAME = 20056;
        public const int SHOP_PAGE_11001_1_NAME = 20057;
        public const int SHOP_PAGE_11001_2_NAME = 20058;
        public const int SHOP_PAGE_11001_3_NAME = 20059;
        public const int SHOP_PAGE_11001_4_NAME = 20060;
        public const int SHOP_PAGE_11001_5_NAME = 20061;
        public const int SHOP_PAGE_11001_6_NAME = 20062;
        public const int SHOP_PAGE_11001_7_NAME = 20063;
        public const int SHOP_PAGE_11001_8_NAME = 20064;
        public const int SHOP_PAGE_11901_1_NAME = 20065;
        public const int SHOP_PAGE_11901_2_NAME = 20066;
        public const int SHOP_PAGE_11901_3_NAME = 20067;
        public const int SHOP_PAGE_11902_2_NAME = 20068;
        public const int SHOP_PAGE_11904_2_NAME = 20069;
        public const int SHOP_PAGE_11915_1_STATE = 20070;
        public const int SHOP_PAGE_11915_2_DISCOUNT = 20071;
        public const int SHOP_PAGE_11915_2_STATE = 20072;
        public const int SHOP_PAGE_11301_1_STATE = 20073;
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