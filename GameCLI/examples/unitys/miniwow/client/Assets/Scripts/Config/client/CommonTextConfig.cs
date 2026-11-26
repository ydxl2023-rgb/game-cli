using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class CommonText
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
        /// Localized content
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
        public const int TEXT_HOME_LOCKED = 10001;
        public const int TEXT_HOME_INVALID = 10002;
        public const int TEXT_HOME_PENDING = 10003;
        public const int TEXT_TASK_GUIDE_ASSIST = 20001;
        public const int TEXT_OPENING_SKIP = 30001;
        public const int TEXT_OPENING_SKIP_CONFIRM = 30002;
        public const int TEXT_OPENING_CONTINUE = 30003;
        public const int TEXT_OPENING_CANCEL = 30004;
        public const int TEXT_OPENING_WATER = 30005;
        public const int TEXT_OPENING_FIRE = 30006;
        public const int TEXT_OPENING_EXTINGUISH = 30007;
        public const int TEXT_OPENING_CLAIM = 30008;
        public const int TEXT_OPENING_NEXT = 30009;
        public const int TEXT_OPENING_WATER_COUNT = 30010;
        public const int TEXT_OPENING_RESCUE = 30011;
        public const int TEXT_OPENING_VIDEO_ERROR = 30012;
        public const int TEXT_OPENING_RETRY = 30013;
        public const int TEXT_OPENING_PRIEST = 30014;
        public const int TEXT_OPENING_MERCHANT = 30015;
        public const int TEXT_OPENING_INTRO = 30016;
        public const int TEXT_OPENING_THANKS = 30017;
        public const int TEXT_OPENING_TASK = 30018;
        public const int TEXT_OPENING_HOTEL_OWNER = 30019;
        public const int TEXT_OPENING_HOTEL_THANKS = 30020;
        public const int TEXT_OPENING_HOTEL_FIRE = 30021;
        public const int TEXT_OPENING_HOSPITAL_FIRE = 30022;
        public const int TEXT_OPENING_FETCH = 30023;
        public const int TEXT_OPENING_CLEAR = 30024;
        public const int TEXT_OPENING_SAVE_PERSON = 30025;
        public const int TEXT_OPENING_CLEAR_HINT = 30026;
        public const int TEXT_OPENING_SAVE_HINT = 30027;
        public const int TEXT_OPENING_COUNTDOWN = 30028;
        public const int TEXT_OPENING_AUTOMATIC_CLAIM = 30029;
        public const int TEXT_VILLAGE_RECRUIT_TITLE = 30101;
        public const int TEXT_VILLAGE_RECRUIT_DETAIL = 30102;
        public const int TEXT_VILLAGE_RECRUIT = 30103;
        public const int TEXT_VILLAGE_REPAIR_TITLE = 30104;
        public const int TEXT_VILLAGE_REPAIR_DETAIL = 30105;
        public const int TEXT_VILLAGE_REPAIR = 30106;
        public const int TEXT_VILLAGE_BUY_TITLE = 30107;
        public const int TEXT_VILLAGE_BUY_DETAIL = 30108;
        public const int TEXT_VILLAGE_BUY = 30109;
        public const int TEXT_VILLAGE_CRAFT_TITLE = 30110;
        public const int TEXT_VILLAGE_CRAFT_DETAIL = 30111;
        public const int TEXT_VILLAGE_CRAFT = 30112;
        public const int TEXT_VILLAGE_HEAL_TITLE = 30113;
        public const int TEXT_VILLAGE_HEAL_DETAIL = 30114;
        public const int TEXT_VILLAGE_HEAL = 30115;
        public const int TEXT_VILLAGE_STOCK = 30116;
        public const int TEXT_VILLAGE_RECEIVED = 30117;
        public const int TEXT_VILLAGE_HEALTH = 30118;
        public const int TEXT_VILLAGE_REJECTED = 30119;
        public const int TEXT_VILLAGE_FINISHED = 30120;
        public const int TEXT_VILLAGE_TASK_DONE = 30121;
        public const int TEXT_VILLAGE_CLOSE = 30122;
        public const int TEXT_VILLAGE_NEXT = 30123;
        public const int TEXT_VILLAGE_CLAIM = 30124;
        public const int TEXT_VILLAGE_HEALING = 30125;
        public const int TEXT_VILLAGE_TRAVELING = 30126;
        public const int TEXT_HOSPITAL_NAME = 30201;
        public const int TEXT_HOSPITAL_NPC = 30202;
        public const int TEXT_BUILDING_ITEM_70001 = 240002;
        public const int TEXT_BUILDING_ITEM_DESC_70001 = 240003;
        public const int TEXT_BUILDING_ITEM_77001 = 254002;
        public const int TEXT_BUILDING_ITEM_DESC_77001 = 254003;
        public const int TEXT_BUILDING_ITEM_77002 = 254004;
        public const int TEXT_BUILDING_ITEM_DESC_77002 = 254005;
        public const int TEXT_BUILDING_ITEM_77003 = 254006;
        public const int TEXT_BUILDING_ITEM_DESC_77003 = 254007;
        public const int TEXT_BUILDING_ITEM_77004 = 254008;
        public const int TEXT_BUILDING_ITEM_DESC_77004 = 254009;
        public const int TEXT_BUILDING_ITEM_77005 = 254010;
        public const int TEXT_BUILDING_ITEM_DESC_77005 = 254011;
        public const int TEXT_BUILDING_ITEM_77006 = 254012;
        public const int TEXT_BUILDING_ITEM_DESC_77006 = 254013;
        public const int TEXT_BUILDING_ITEM_77101 = 254202;
        public const int TEXT_BUILDING_ITEM_DESC_77101 = 254203;
        public const int TEXT_BUILDING_ITEM_77102 = 254204;
        public const int TEXT_BUILDING_ITEM_DESC_77102 = 254205;
        public const int TEXT_BUILDING_ITEM_77103 = 254206;
        public const int TEXT_BUILDING_ITEM_DESC_77103 = 254207;
        public const int TEXT_BUILDING_ITEM_77104 = 254208;
        public const int TEXT_BUILDING_ITEM_DESC_77104 = 254209;
        public const int TEXT_BUILDING_ITEM_77105 = 254210;
        public const int TEXT_BUILDING_ITEM_DESC_77105 = 254211;
        public const int TEXT_BUILDING_ITEM_77106 = 254212;
        public const int TEXT_BUILDING_ITEM_DESC_77106 = 254213;
        public const int TEXT_HOSPITAL_REPAIR = 30203;
        public const int TEXT_HOSPITAL_ENTER = 30204;
        public const int TEXT_HOSPITAL_LEVEL = 30205;
        public const int TEXT_HOSPITAL_BONUS = 30206;
        public const int TEXT_HOSPITAL_DETAIL = 30207;
        public const int TEXT_HOSPITAL_STOCK = 30208;
        public const int TEXT_HOSPITAL_MATERIAL = 30209;
        public const int TEXT_HOSPITAL_BACK = 30210;
        public const int TEXT_HOSPITAL_IDLE = 30211;
        public const int TEXT_HOSPITAL_TREATING = 30212;
        public const int TEXT_HOSPITAL_TRAVELING = 30213;
        public const int TEXT_HOSPITAL_FINISHED = 30214;
        public const int TEXT_HOSPITAL_NO_MATERIAL = 30215;
        public const int TEXT_HOSPITAL_NO_GOLD = 30216;
        public const int TEXT_HOSPITAL_CRAFTING = 30217;
        public const int TEXT_HOSPITAL_NEXT_LEVEL = 30218;
        public const int TEXT_HOSPITAL_TASK = 30219;
        public const int TEXT_HOSPITAL_REVENUE = 30220;
        public const int TEXT_HOSPITAL_MATERIAL_TITLE = 30221;
        public const int TEXT_HOSPITAL_PRODUCT_TITLE = 30222;
        public const int TEXT_HOSPITAL_GOLD = 30223;
        public const int TEXT_FORGE_NPC = 30224;
        public const int TEXT_HOTEL_NPC = 30225;
        public const int TEXT_AUCTION_NPC = 30226;
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