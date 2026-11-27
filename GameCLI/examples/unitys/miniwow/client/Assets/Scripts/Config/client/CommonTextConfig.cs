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
        public const int TEXT_TRADE_ITEM_77001 = 322002;
        public const int TEXT_TRADE_DESC_77001 = 322003;
        public const int TEXT_TRADE_ITEM_77011 = 322012;
        public const int TEXT_TRADE_DESC_77011 = 322013;
        public const int TEXT_TRADE_ITEM_76001 = 322022;
        public const int TEXT_TRADE_DESC_76001 = 322023;
        public const int TEXT_TRADE_TITLE = 30301;
        public const int TEXT_TRADE_MATERIALS = 30302;
        public const int TEXT_TRADE_EQUIPMENT_PENDING = 30303;
        public const int TEXT_TRADE_GOLD = 30304;
        public const int TEXT_TRADE_REMAINING = 30305;
        public const int TEXT_TRADE_OWNED = 30306;
        public const int TEXT_TRADE_BUY = 30307;
        public const int TEXT_TRADE_CANCEL = 30308;
        public const int TEXT_TRADE_BACK = 30309;
        public const int TEXT_TRADE_QUANTITY = 30310;
        public const int TEXT_TRADE_TOTAL = 30311;
        public const int TEXT_TRADE_NO_GOLD = 30312;
        public const int TEXT_TRADE_SOLD_OUT = 30313;
        public const int TEXT_TRADE_LOCKED = 30314;
        public const int TEXT_TRADE_SUCCESS = 30315;
        public const int TEXT_TRADE_NO_SELECTION = 30316;
        public const int TEXT_TRADE_INVALID_QUANTITY = 30317;
        public const int TEXT_TRADE_TASK = 30318;
        public const int TEXT_TRADE_CLAIM = 30319;
        public const int TEXT_TRADE_MINUS = 30320;
        public const int TEXT_TRADE_PLUS = 30321;
        public const int TEXT_TRADE_CONFIRM = 30322;
        public const int TEXT_TRADE_TIER = 30323;
        public const int TEXT_HOSPITAL_UPGRADE_HINT = 30401;
        public const int TEXT_HOSPITAL_AUTO_CRAFT = 30402;
        public const int TEXT_HOSPITAL_EMPTY = 30403;
        public const int TEXT_HOSPITAL_CRAFT_HINT = 30404;
        public const int TEXT_HOSPITAL_LEVEL_LABEL = 30405;
        public const int TEXT_HOSPITAL_RAW_MATERIAL = 30406;
        public const int TEXT_HOSPITAL_FINISHED_PRODUCT = 30407;
        public const int TEXT_BUILDING_ITEM_77011 = 254022;
        public const int TEXT_BUILDING_ITEM_DESC_77011 = 254023;
        public const int TEXT_BUILDING_ITEM_77012 = 254024;
        public const int TEXT_BUILDING_ITEM_DESC_77012 = 254025;
        public const int TEXT_BUILDING_ITEM_77013 = 254026;
        public const int TEXT_BUILDING_ITEM_DESC_77013 = 254027;
        public const int TEXT_BUILDING_ITEM_77014 = 254028;
        public const int TEXT_BUILDING_ITEM_DESC_77014 = 254029;
        public const int TEXT_BUILDING_ITEM_77015 = 254030;
        public const int TEXT_BUILDING_ITEM_DESC_77015 = 254031;
        public const int TEXT_BUILDING_ITEM_77016 = 254032;
        public const int TEXT_BUILDING_ITEM_DESC_77016 = 254033;
        public const int TEXT_BUILDING_ITEM_77111 = 254222;
        public const int TEXT_BUILDING_ITEM_DESC_77111 = 254223;
        public const int TEXT_BUILDING_ITEM_77112 = 254224;
        public const int TEXT_BUILDING_ITEM_DESC_77112 = 254225;
        public const int TEXT_BUILDING_ITEM_77113 = 254226;
        public const int TEXT_BUILDING_ITEM_DESC_77113 = 254227;
        public const int TEXT_BUILDING_ITEM_77114 = 254228;
        public const int TEXT_BUILDING_ITEM_DESC_77114 = 254229;
        public const int TEXT_BUILDING_ITEM_77115 = 254230;
        public const int TEXT_BUILDING_ITEM_DESC_77115 = 254231;
        public const int TEXT_BUILDING_ITEM_77116 = 254232;
        public const int TEXT_BUILDING_ITEM_DESC_77116 = 254233;
        public const int TEXT_INN_NAME = 30501;
        public const int TEXT_INN_LEVEL = 30502;
        public const int TEXT_INN_DETAIL = 30503;
        public const int TEXT_INN_REPAIR = 30504;
        public const int TEXT_INN_LOCKED = 30505;
        public const int TEXT_INN_NO_MATERIAL = 30506;
        public const int TEXT_INN_REST = 30507;
        public const int TEXT_INN_RESTING = 30508;
        public const int TEXT_INN_ENERGY = 30509;
        public const int TEXT_INN_REST_UNAVAILABLE = 30510;
        public const int TEXT_INN_NO_GOLD = 30511;
        public const int TEXT_INN_REPAIR_HINT = 30512;
        public const int TEXT_INN_CRAFTED = 30513;
        public const int TEXT_INN_REPAIRED = 30514;
        public const int TEXT_FORGE_RECIPE_110001 = 710001;
        public const int TEXT_FORGE_RECIPE_110002 = 710002;
        public const int TEXT_FORGE_RECIPE_110003 = 710003;
        public const int TEXT_FORGE_RECIPE_110004 = 710004;
        public const int TEXT_FORGE_RECIPE_110005 = 710005;
        public const int TEXT_FORGE_MATERIAL_70001 = 870001;
        public const int TEXT_FORGE_MATERIAL_76001 = 876001;
        public const int TEXT_FORGE_MATERIAL_76002 = 876002;
        public const int TEXT_BLACKSMITH_TITLE = 30601;
        public const int TEXT_BLACKSMITH_LOCKED = 30602;
        public const int TEXT_BLACKSMITH_REPAIR = 30603;
        public const int TEXT_BLACKSMITH_REPAIRED = 30604;
        public const int TEXT_BLACKSMITH_RECIPES = 30605;
        public const int TEXT_BLACKSMITH_SHELF = 30606;
        public const int TEXT_BLACKSMITH_HERO = 30607;
        public const int TEXT_BLACKSMITH_FORGE = 30608;
        public const int TEXT_BLACKSMITH_QUANTITY = 30609;
        public const int TEXT_BLACKSMITH_COST = 30610;
        public const int TEXT_BLACKSMITH_SUCCESS = 30611;
        public const int TEXT_BLACKSMITH_NO_MATERIAL = 30612;
        public const int TEXT_BLACKSMITH_NO_SELECTION = 30613;
        public const int TEXT_BLACKSMITH_SELL = 30614;
        public const int TEXT_BLACKSMITH_SOLD = 30615;
        public const int TEXT_BLACKSMITH_INCOMPATIBLE = 30616;
        public const int TEXT_BLACKSMITH_NO_HERO = 30617;
        public const int TEXT_BLACKSMITH_NO_GOLD = 30618;
        public const int TEXT_BLACKSMITH_NO_STOCK = 30619;
        public const int TEXT_BLACKSMITH_REPAIR_FIRST = 30620;
        public const int TEXT_BLACKSMITH_BACK = 30621;
        public const int TEXT_BLACKSMITH_PRICE = 30622;
        public const int TEXT_BLACKSMITH_OWNED = 30623;
        public const int TEXT_BLACKSMITH_EQUIPPED = 30624;
        public const int TEXT_BLACKSMITH_EMPTY = 30625;
        public const int TEXT_BLACKSMITH_HERO_GOLD = 30626;
        public const int TEXT_BLACKSMITH_LOCAL_RULE = 30627;
        public const int TEXT_BLACKSMITH_FORGING = 30628;
        public const int TEXT_BLACKSMITH_MINUS = 30629;
        public const int TEXT_BLACKSMITH_PLUS = 30630;
        public const int TEXT_BLACKSMITH_CONFIRM = 30631;
        public const int TEXT_BLACKSMITH_CANCEL = 30632;
        public const int TEXT_BLACKSMITH_GOLD = 30633;
        public const int TEXT_BLACKSMITH_NO_REPAIR_GOLD = 30634;
        public const int TEXT_BLACKSMITH_TRADE = 30635;
        public const int TEXT_BLACKSMITH_SELECT = 30636;
        public const int TEXT_BLACKSMITH_DETAIL = 30637;
        public const int TEXT_BLACKSMITH_HERO_NAME = 30638;
        public const int TEXT_BLACKSMITH_SELL_CONFIRM = 30639;
        public const int TEXT_BLACKSMITH_REVENUE = 30640;
        public const int TEXT_BLACKSMITH_CLAIM_REVENUE = 30641;
        public const int TEXT_FORGE_HERO_125 = 900125;
        public const int TEXT_FORGE_HERO_126 = 900126;
        public const int TEXT_FORGE_HERO_127 = 900127;
        public const int TEXT_FORGE_HERO_128 = 900128;
        public const int TEXT_FORGE_HERO_129 = 900129;
        public const int TEXT_FORGE_HERO_130 = 900130;
        public const int TEXT_FORGE_HERO_131 = 900131;
        public const int TEXT_FORGE_HERO_132 = 900132;
        public const int TEXT_FORGE_HERO_133 = 900133;
        public const int TEXT_FORGE_HERO_134 = 900134;
        public const int TEXT_FORGE_HERO_135 = 900135;
        public const int TEXT_FORGE_HERO_136 = 900136;
        public const int TEXT_FORGE_HERO_137 = 900137;
        public const int TEXT_FORGE_HERO_138 = 900138;
        public const int TEXT_FORGE_HERO_139 = 900139;
        public const int TEXT_FORGE_HERO_140 = 900140;
        public const int TEXT_FORGE_HERO_141 = 900141;
        public const int TEXT_FORGE_HERO_142 = 900142;
        public const int TEXT_FORGE_HERO_143 = 900143;
        public const int TEXT_FORGE_HERO_144 = 900144;
        public const int TEXT_FORGE_HERO_145 = 900145;
        public const int TEXT_FORGE_HERO_146 = 900146;
        public const int TEXT_FORGE_HERO_147 = 900147;
        public const int TEXT_FORGE_HERO_148 = 900148;
        public const int TEXT_FORGE_HERO_149 = 900149;
        public const int TEXT_FORGE_HERO_150 = 900150;
        public const int TEXT_FORGE_HERO_151 = 900151;
        public const int TEXT_FORGE_HERO_152 = 900152;
        public const int TEXT_FORGE_HERO_153 = 900153;
        public const int TEXT_FORGE_HERO_154 = 900154;
        public const int TEXT_FORGE_HERO_155 = 900155;
        public const int TEXT_FORGE_HERO_156 = 900156;
        public const int TEXT_FORGE_HERO_157 = 900157;
        public const int TEXT_FORGE_HERO_158 = 900158;
        public const int TEXT_FORGE_HERO_159 = 900159;
        public const int TEXT_FORGE_HERO_160 = 900160;
        public const int TEXT_FORGE_HERO_161 = 900161;
        public const int TEXT_FORGE_HERO_162 = 900162;
        public const int TEXT_FORGE_HERO_163 = 900163;
        public const int TEXT_FORGE_HERO_164 = 900164;
        public const int TEXT_FORGE_HERO_165 = 900165;
        public const int TEXT_FORGE_HERO_166 = 900166;
        public const int TEXT_FORGE_HERO_167 = 900167;
        public const int TEXT_FORGE_HERO_168 = 900168;
        public const int TEXT_FORGE_HERO_169 = 900169;
        public const int TEXT_FORGE_HERO_170 = 900170;
        public const int TEXT_FORGE_HERO_171 = 900171;
        public const int TEXT_FORGE_HERO_172 = 900172;
        public const int TEXT_FORGE_HERO_173 = 900173;
        public const int TEXT_FORGE_HERO_174 = 900174;
        public const int TEXT_FORGE_HERO_175 = 900175;
        public const int TEXT_FORGE_HERO_176 = 900176;
        public const int TEXT_FORGE_HERO_177 = 900177;
        public const int TEXT_FORGE_HERO_178 = 900178;
        public const int TEXT_FORGE_HERO_179 = 900179;
        public const int TEXT_FORGE_HERO_180 = 900180;
        public const int TEXT_FORGE_HERO_181 = 900181;
        public const int TEXT_TRADE_LEVEL_TITLE = 30324;
        public const int TEXT_TRADE_SALE_TITLE = 30325;
        public const int TEXT_TRADE_MATERIAL_TAB = 30326;
        public const int TEXT_TRADE_EQUIPMENT_TAB = 30327;
        public const int TEXT_TRADE_LOG_PENDING = 30328;
        public const int TEXT_TRADE_TIER_SHORT = 30329;
        public const int TEXT_RECRUIT_SPEAKER_3028001 = 16056002;
        public const int TEXT_RECRUIT_DIALOGUE_3028001 = 16056003;
        public const int TEXT_RECRUIT_SPEAKER_3028002 = 16056004;
        public const int TEXT_RECRUIT_DIALOGUE_3028002 = 16056005;
        public const int TEXT_RECRUIT_SPEAKER_3029001 = 16058002;
        public const int TEXT_RECRUIT_DIALOGUE_3029001 = 16058003;
        public const int TEXT_RECRUIT_SPEAKER_3030001 = 16060002;
        public const int TEXT_RECRUIT_DIALOGUE_3030001 = 16060003;
        public const int TEXT_RECRUIT_SPEAKER_3031001 = 16062002;
        public const int TEXT_RECRUIT_DIALOGUE_3031001 = 16062003;
        public const int TEXT_RECRUIT_HERO_129 = 307129;
        public const int TEXT_RECRUIT_HERO_133 = 307133;
        public const int TEXT_BATTLE_ENTRY_TITLE = 308001;
        public const int TEXT_BATTLE_ENTRY_NAME = 308002;
        public const int TEXT_BATTLE_ENTRY_DESCRIPTION = 308003;
        public const int TEXT_BATTLE_ENTRY_GATHER = 308004;
        public const int TEXT_BATTLE_ENTRY_READY = 308005;
        public const int TEXT_BATTLE_ENTRY_REQUIRES_HERO = 308006;
        public const int TEXT_BATTLE_ENTRY_LOCKED = 308007;
        public const int TEXT_BATTLE_ENTRY_POWER = 308008;
        public const int TEXT_BATTLE_ENTRY_PREPARED = 308009;
        public const int TEXT_RECRUIT_TALK = 308010;
        public const int TEXT_RECRUIT_RECEIVE = 308011;
        public const int TEXT_RECRUIT_HORN = 308012;
        public const int TEXT_RECRUIT_MISSING = 308013;
        public const int TEXT_RECRUIT_NEXT = 308014;
        public const int TEXT_RECRUIT_RETRY = 308015;
        public const int TEXT_RECRUIT_BROTHER = 308016;
        public const int TEXT_RECRUIT_HORN_NPC = 308017;
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