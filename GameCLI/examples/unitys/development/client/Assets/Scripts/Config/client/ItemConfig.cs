using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class Item
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
        /// 道具类型
        /// </summary>
        [JsonProperty]
        public EItemType ItemType { get; private set; }

        /// <summary>
        /// 装备部位
        /// </summary>
        [JsonProperty]
        public EPartType PartType { get; private set; }

        /// <summary>
        /// 道具品质
        /// </summary>
        [JsonProperty]
        public EQuality Quality { get; private set; }

        /// <summary>
        /// 职业限制
        /// </summary>
        [JsonProperty]
        public EProfessionType ProfessionType { get; private set; }

        /// <summary>
        /// 挂点类型
        /// </summary>
        [JsonProperty]
        public string BoneNogPointName { get; private set; }

        /// <summary>
        /// 动画控制器
        /// </summary>
        [JsonProperty]
        public string OverrideController { get; private set; }

        /// <summary>
        /// 道具元素
        /// </summary>
        [JsonProperty]
        public List<string> elements { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (Item exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class ItemConfig : Config<Item>
    {
    }

    /// <summary>
    /// export excel Item data
    /// </summary>
    public sealed partial class Excel
    {
        public const int ITEM_EQUIP_BELT1001 = 1001;
        public const int ITEM_EQUIP_BELT1002 = 1002;
        public const int ITEM_EQUIP_BELT1003 = 1003;
        public const int ITEM_EQUIP_CLOTH1004 = 1004;
        public const int ITEM_EQUIP_CLOTH1005 = 1005;
        public const int ITEM_EQUIP_CLOTH1006 = 1006;
        public const int ITEM_EQUIP_CLOTH1007 = 1007;
        public const int ITEM_EQUIP_CLOTH1008 = 1008;
        public const int ITEM_EQUIP_CLOTH1009 = 1009;
        public const int ITEM_EQUIP_CLOTH1010 = 1010;
        public const int ITEM_EQUIP_CLOTH1011 = 1011;
        public const int ITEM_EQUIP_CLOTH1012 = 1012;
        public const int ITEM_EQUIP_CROWN1013 = 1013;
        public const int ITEM_EQUIP_CROWN1014 = 1014;
        public const int ITEM_EQUIP_CROWN1015 = 1015;
        public const int ITEM_EQUIP_CROWN1016 = 1016;
        public const int ITEM_EQUIP_FACE1017 = 1017;
        public const int ITEM_EQUIP_FACE1018 = 1018;
        public const int ITEM_EQUIP_FACE1019 = 1019;
        public const int ITEM_EQUIP_FACE1020 = 1020;
        public const int ITEM_EQUIP_FACE1021 = 1021;
        public const int ITEM_EQUIP_GLOVE1022 = 1022;
        public const int ITEM_EQUIP_GLOVE1023 = 1023;
        public const int ITEM_EQUIP_GLOVE1024 = 1024;
        public const int ITEM_EQUIP_GLOVE1025 = 1025;
        public const int ITEM_EQUIP_GLOVE1026 = 1026;
        public const int ITEM_EQUIP_GLOVE1027 = 1027;
        public const int ITEM_EQUIP_HAIR1028 = 1028;
        public const int ITEM_EQUIP_HAIR1029 = 1029;
        public const int ITEM_EQUIP_HAIR1030 = 1030;
        public const int ITEM_EQUIP_HAIR1031 = 1031;
        public const int ITEM_EQUIP_HAIR1032 = 1032;
        public const int ITEM_EQUIP_HAIR1033 = 1033;
        public const int ITEM_EQUIP_HAIR1034 = 1034;
        public const int ITEM_EQUIP_HAIRHALF1035 = 1035;
        public const int ITEM_EQUIP_HAIRHALF1036 = 1036;
        public const int ITEM_EQUIP_HAIRHALF1037 = 1037;
        public const int ITEM_EQUIP_HAIRHALF1038 = 1038;
        public const int ITEM_EQUIP_HAIRHALF1039 = 1039;
        public const int ITEM_EQUIP_HAIRHALF1040 = 1040;
        public const int ITEM_EQUIP_HAIRHALF1041 = 1041;
        public const int ITEM_EQUIP_HAT1042 = 1042;
        public const int ITEM_EQUIP_HAT1043 = 1043;
        public const int ITEM_EQUIP_HAT1044 = 1044;
        public const int ITEM_EQUIP_HELM1045 = 1045;
        public const int ITEM_EQUIP_HELM1046 = 1046;
        public const int ITEM_EQUIP_HELM1047 = 1047;
        public const int ITEM_EQUIP_HELM1048 = 1048;
        public const int ITEM_EQUIP_HELM1049 = 1049;
        public const int ITEM_EQUIP_HELM1050 = 1050;
        public const int ITEM_EQUIP_SHOE1051 = 1051;
        public const int ITEM_EQUIP_SHOE1052 = 1052;
        public const int ITEM_EQUIP_SHOE1053 = 1053;
        public const int ITEM_EQUIP_SHOE1054 = 1054;
        public const int ITEM_EQUIP_SHOE1055 = 1055;
        public const int ITEM_EQUIP_SHOE1056 = 1056;
        public const int ITEM_EQUIP_SHOULDERPAD1057 = 1057;
        public const int ITEM_EQUIP_SHOULDERPAD1058 = 1058;
        public const int ITEM_EQUIP_SHOULDERPAD1059 = 1059;
        public const int ITEM_EQUIP_SHOULDERPAD1060 = 1060;
        public const int ITEM_EQUIP_SHOULDERPAD1061 = 1061;
        public const int ITEM_EQUIP_1062 = 1062;
        public const int ITEM_EQUIP_1063 = 1063;
        public const int ITEM_EQUIP_1064 = 1064;
        public const int ITEM_EQUIP_1065 = 1065;
        public const int ITEM_EQUIP_1066 = 1066;
        public const int ITEM_EQUIP_1067 = 1067;
        public const int ITEM_EQUIP_WEAPON1068 = 1068;
        public const int ITEM_EQUIP_WEAPON1069 = 1069;
        public const int ITEM_EQUIP_WEAPON1070 = 1070;
        public const int ITEM_EQUIP_WEAPON1071 = 1071;
        public const int ITEM_EQUIP_WEAPON1072 = 1072;
        public const int ITEM_EQUIP_WEAPON1073 = 1073;
        public const int ITEM_EQUIP_WEAPON1074 = 1074;
        public const int ITEM_EQUIP_WEAPON1075 = 1075;
        public const int ITEM_EQUIP_WEAPON1076 = 1076;
        public const int ITEM_EQUIP_WEAPON1077 = 1077;
        public const int ITEM_EQUIP_WEAPON1078 = 1078;
        public const int ITEM_EQUIP_WEAPON1079 = 1079;
        public const int ITEM_EQUIP_ASSISTANT1080 = 1080;
        public const int ITEM_EQUIP_ASSISTANT1081 = 1081;
        public const int ITEM_EQUIP_ASSISTANT1082 = 1082;
        public const int ITEM_EQUIP_ASSISTANT1083 = 1083;
        public const int ITEM_EQUIP_ASSISTANT1084 = 1084;
        public const int ITEM_EQUIP_ASSISTANT1085 = 1085;
        public const int ITEM_EQUIP_ASSISTANT1086 = 1086;
        public const int ITEM_EQUIP_ASSISTANT1087 = 1087;
        public const int ITEM_EQUIP_ASSISTANT1088 = 1088;
        public const int ITEM_EQUIP_ASSISTANT1089 = 1089;
        public const int ITEM_EQUIP_ASSISTANT1090 = 1090;
        public const int ITEM_EQUIP_ASSISTANT1091 = 1091;
        public const int ITEM_EQUIP_ASSISTANT1092 = 1092;
        public const int ITEM_EQUIP_ASSISTANT1093 = 1093;
        public const int ITEM_EQUIP_ASSISTANT1094 = 1094;
        public const int ITEM_EQUIP_ASSISTANT1095 = 1095;
        public const int ITEM_EQUIP_WEAPON1096 = 1096;
        public const int ITEM_EQUIP_WEAPON1097 = 1097;
        public const int ITEM_EQUIP_WEAPON1098 = 1098;
        public const int ITEM_EQUIP_WEAPON1099 = 1099;
        public const int ITEM_EQUIP_WEAPON1100 = 1100;
        public const int ITEM_EQUIP_WEAPON1101 = 1101;
        public const int ITEM_EQUIP_WEAPON1102 = 1102;
        public const int ITEM_EQUIP_WEAPON1103 = 1103;
        public const int ITEM_EQUIP_WEAPON1104 = 1104;
        public const int ITEM_EQUIP_WEAPON1105 = 1105;
        public const int ITEM_EQUIP_WEAPON1106 = 1106;
        public const int ITEM_EQUIP_WEAPON1107 = 1107;
        public const int ITEM_EQUIP_WEAPON1108 = 1108;
        public const int ITEM_EQUIP_WEAPON1109 = 1109;
        public const int ITEM_EQUIP_WEAPON1110 = 1110;
        public const int ITEM_EQUIP_WEAPON1111 = 1111;
        public const int ITEM_EQUIP_WEAPON1112 = 1112;
        public const int ITEM_EQUIP_WEAPON1113 = 1113;
        public const int ITEM_EQUIP_WEAPON1114 = 1114;
        public const int ITEM_EQUIP_WEAPON1115 = 1115;
        public const int ITEM_EQUIP_ASSISTANT1116 = 1116;
        public const int ITEM_EQUIP_ASSISTANT1117 = 1117;
        public const int ITEM_EQUIP_ASSISTANT1118 = 1118;
        public const int ITEM_EQUIP_ASSISTANT1119 = 1119;
        public const int ITEM_EQUIP_WEAPON1120 = 1120;
        public const int ITEM_EQUIP_WEAPON1121 = 1121;
        public const int ITEM_EQUIP_WEAPON1122 = 1122;
        public const int ITEM_EQUIP_WEAPON1123 = 1123;
        public const int ITEM_EQUIP_WEAPON1124 = 1124;
        public const int ITEM_EQUIP_WEAPON1125 = 1125;
        public const int ITEM_EQUIP_WEAPON1126 = 1126;
        public const int ITEM_EQUIP_WEAPON1127 = 1127;
        public const int ITEM_EQUIP_WEAPON1128 = 1128;
        public const int ITEM_EQUIP_WEAPON1129 = 1129;
        public const int ITEM_EQUIP_WEAPON1130 = 1130;
        public const int ITEM_EQUIP_WEAPON1131 = 1131;
        public const int ITEM_EQUIP_WEAPON1132 = 1132;
        public const int ITEM_EQUIP_WEAPON1133 = 1133;
        public const int ITEM_EQUIP_WEAPON1134 = 1134;
        public const int ITEM_EQUIP_WEAPON1135 = 1135;
        public const int ITEM_EQUIP_WEAPON1136 = 1136;
        public const int ITEM_EQUIP_WEAPON1137 = 1137;
        public const int ITEM_EQUIP_WEAPON1138 = 1138;
        public const int ITEM_EQUIP_WEAPON1139 = 1139;
        public const int ITEM_EQUIP_WEAPON1140 = 1140;
        public const int ITEM_EQUIP_WEAPON1141 = 1141;
        public const int ITEM_EQUIP_WEAPON1142 = 1142;
        public const int ITEM_EQUIP_WEAPON1143 = 1143;
        public const int ITEM_EQUIP_WEAPON1144 = 1144;
        public const int ITEM_EQUIP_WEAPON1145 = 1145;
        public const int ITEM_EQUIP_WEAPON1146 = 1146;
        public const int ITEM_EQUIP_WEAPON1147 = 1147;
        /// <summary>
        /// config ItemConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, Item> itemConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static Item propItemById(int nId)
        {
            Item data;
            if (!itemConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(Item).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}