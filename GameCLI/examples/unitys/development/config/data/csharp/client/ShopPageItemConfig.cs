using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class ShopPageItem
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// NumID
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// PageId
        /// </summary>
        [JsonProperty]
        public int PageId { get; private set; }

        /// <summary>
        /// Sort
        /// </summary>
        [JsonProperty]
        public int Sort { get; private set; }

        /// <summary>
        /// SourceId
        /// </summary>
        [JsonProperty]
        public int SourceId { get; private set; }

        /// <summary>
        /// NameTextId
        /// </summary>
        [JsonProperty]
        public int NameTextId { get; private set; }

        /// <summary>
        /// NameValue
        /// </summary>
        [JsonProperty]
        public string NameValue { get; private set; }

        /// <summary>
        /// PriceTextId
        /// </summary>
        [JsonProperty]
        public int PriceTextId { get; private set; }

        /// <summary>
        /// PriceValue
        /// </summary>
        [JsonProperty]
        public string PriceValue { get; private set; }

        /// <summary>
        /// Count
        /// </summary>
        [JsonProperty]
        public int Count { get; private set; }

        /// <summary>
        /// Limit
        /// </summary>
        [JsonProperty]
        public int Limit { get; private set; }

        /// <summary>
        /// Icon
        /// </summary>
        [JsonProperty]
        public string Icon { get; private set; }

        /// <summary>
        /// Preview
        /// </summary>
        [JsonProperty]
        public string Preview { get; private set; }

        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty]
        public string Currency { get; private set; }

        /// <summary>
        /// DiscountTextId
        /// </summary>
        [JsonProperty]
        public int DiscountTextId { get; private set; }

        /// <summary>
        /// StateTextId
        /// </summary>
        [JsonProperty]
        public int StateTextId { get; private set; }

        /// <summary>
        /// Availability
        /// </summary>
        [JsonProperty]
        public int Availability { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (ShopPageItem exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class ShopPageItemConfig : Config<ShopPageItem>
    {
    }

    /// <summary>
    /// export excel ShopPageItem data
    /// </summary>
    public sealed partial class Excel
    {
        public const int SHOP_PAGE_10102_1 = 1;
        public const int SHOP_PAGE_10102_2 = 2;
        public const int SHOP_PAGE_10102_3 = 3;
        public const int SHOP_PAGE_10102_4 = 4;
        public const int SHOP_PAGE_10102_5 = 5;
        public const int SHOP_PAGE_10102_6 = 6;
        public const int SHOP_PAGE_10102_7 = 7;
        public const int SHOP_PAGE_10102_8 = 8;
        public const int SHOP_PAGE_10102_9 = 9;
        public const int SHOP_PAGE_10102_10 = 10;
        public const int SHOP_PAGE_10102_11 = 11;
        public const int SHOP_PAGE_10102_12 = 12;
        public const int SHOP_PAGE_10102_13 = 13;
        public const int SHOP_PAGE_10102_14 = 14;
        public const int SHOP_PAGE_10102_15 = 15;
        public const int SHOP_PAGE_10102_16 = 16;
        public const int SHOP_PAGE_10102_17 = 17;
        public const int SHOP_PAGE_10102_18 = 18;
        public const int SHOP_PAGE_10103_1 = 19;
        public const int SHOP_PAGE_10103_2 = 20;
        public const int SHOP_PAGE_10103_3 = 21;
        public const int SHOP_PAGE_10103_4 = 22;
        public const int SHOP_PAGE_10103_5 = 23;
        public const int SHOP_PAGE_10103_6 = 24;
        public const int SHOP_PAGE_10103_7 = 25;
        public const int SHOP_PAGE_10103_8 = 26;
        public const int SHOP_PAGE_10103_9 = 27;
        public const int SHOP_PAGE_10104_1 = 28;
        public const int SHOP_PAGE_10104_2 = 29;
        public const int SHOP_PAGE_10104_3 = 30;
        public const int SHOP_PAGE_10104_4 = 31;
        public const int SHOP_PAGE_10104_5 = 32;
        public const int SHOP_PAGE_10104_6 = 33;
        public const int SHOP_PAGE_10104_7 = 34;
        public const int SHOP_PAGE_10104_8 = 35;
        public const int SHOP_PAGE_10104_9 = 36;
        public const int SHOP_PAGE_10105_1 = 37;
        public const int SHOP_PAGE_10105_2 = 38;
        public const int SHOP_PAGE_10105_3 = 39;
        public const int SHOP_PAGE_10105_4 = 40;
        public const int SHOP_PAGE_10105_5 = 41;
        public const int SHOP_PAGE_10105_6 = 42;
        public const int SHOP_PAGE_10105_7 = 43;
        public const int SHOP_PAGE_10105_8 = 44;
        public const int SHOP_PAGE_10105_9 = 45;
        public const int SHOP_PAGE_10105_10 = 46;
        public const int SHOP_PAGE_10105_11 = 47;
        public const int SHOP_PAGE_10105_12 = 48;
        public const int SHOP_PAGE_10105_13 = 49;
        public const int SHOP_PAGE_10105_14 = 50;
        public const int SHOP_PAGE_10105_15 = 51;
        public const int SHOP_PAGE_10105_16 = 52;
        public const int SHOP_PAGE_10105_17 = 53;
        public const int SHOP_PAGE_10105_18 = 54;
        public const int SHOP_PAGE_10302_1 = 55;
        public const int SHOP_PAGE_10302_2 = 56;
        public const int SHOP_PAGE_10302_3 = 57;
        public const int SHOP_PAGE_10302_4 = 58;
        public const int SHOP_PAGE_10302_5 = 59;
        public const int SHOP_PAGE_10302_6 = 60;
        public const int SHOP_PAGE_10302_7 = 61;
        public const int SHOP_PAGE_10302_8 = 62;
        public const int SHOP_PAGE_10302_9 = 63;
        public const int SHOP_PAGE_10302_10 = 64;
        public const int SHOP_PAGE_10302_11 = 65;
        public const int SHOP_PAGE_10302_12 = 66;
        public const int SHOP_PAGE_10802_1 = 67;
        public const int SHOP_PAGE_10802_2 = 68;
        public const int SHOP_PAGE_10802_3 = 69;
        public const int SHOP_PAGE_10802_4 = 70;
        public const int SHOP_PAGE_10802_5 = 71;
        public const int SHOP_PAGE_10802_6 = 72;
        public const int SHOP_PAGE_10802_7 = 73;
        public const int SHOP_PAGE_10802_8 = 74;
        public const int SHOP_PAGE_10802_9 = 75;
        public const int SHOP_PAGE_10802_10 = 76;
        public const int SHOP_PAGE_10802_11 = 77;
        public const int SHOP_PAGE_10802_12 = 78;
        public const int SHOP_PAGE_10802_13 = 79;
        public const int SHOP_PAGE_10802_14 = 80;
        public const int SHOP_PAGE_10802_15 = 81;
        public const int SHOP_PAGE_10802_16 = 82;
        public const int SHOP_PAGE_10802_17 = 83;
        public const int SHOP_PAGE_10802_18 = 84;
        public const int SHOP_PAGE_11001_1 = 85;
        public const int SHOP_PAGE_11001_2 = 86;
        public const int SHOP_PAGE_11001_3 = 87;
        public const int SHOP_PAGE_11001_4 = 88;
        public const int SHOP_PAGE_11001_5 = 89;
        public const int SHOP_PAGE_11001_6 = 90;
        public const int SHOP_PAGE_11001_7 = 91;
        public const int SHOP_PAGE_11001_8 = 92;
        public const int SHOP_PAGE_11901_1 = 93;
        public const int SHOP_PAGE_11901_2 = 94;
        public const int SHOP_PAGE_11901_3 = 95;
        public const int SHOP_PAGE_11901_4 = 96;
        public const int SHOP_PAGE_11902_1 = 97;
        public const int SHOP_PAGE_11902_2 = 98;
        public const int SHOP_PAGE_11902_3 = 99;
        public const int SHOP_PAGE_11902_4 = 100;
        public const int SHOP_PAGE_11903_1 = 101;
        public const int SHOP_PAGE_11903_2 = 102;
        public const int SHOP_PAGE_11903_3 = 103;
        public const int SHOP_PAGE_11903_4 = 104;
        public const int SHOP_PAGE_11904_1 = 105;
        public const int SHOP_PAGE_11904_2 = 106;
        public const int SHOP_PAGE_11904_3 = 107;
        public const int SHOP_PAGE_11904_4 = 108;
        public const int SHOP_PAGE_11905_1 = 109;
        public const int SHOP_PAGE_11905_2 = 110;
        public const int SHOP_PAGE_11905_3 = 111;
        public const int SHOP_PAGE_11905_4 = 112;
        public const int SHOP_PAGE_11906_1 = 113;
        public const int SHOP_PAGE_11906_2 = 114;
        public const int SHOP_PAGE_11906_3 = 115;
        public const int SHOP_PAGE_11906_4 = 116;
        public const int SHOP_PAGE_11906_5 = 117;
        public const int SHOP_PAGE_11907_1 = 118;
        public const int SHOP_PAGE_11907_2 = 119;
        public const int SHOP_PAGE_11907_3 = 120;
        public const int SHOP_PAGE_11907_4 = 121;
        public const int SHOP_PAGE_11907_5 = 122;
        public const int SHOP_PAGE_11908_1 = 123;
        public const int SHOP_PAGE_11908_2 = 124;
        public const int SHOP_PAGE_11908_3 = 125;
        public const int SHOP_PAGE_11908_4 = 126;
        public const int SHOP_PAGE_11908_5 = 127;
        public const int SHOP_PAGE_11909_1 = 128;
        public const int SHOP_PAGE_11909_2 = 129;
        public const int SHOP_PAGE_11909_3 = 130;
        public const int SHOP_PAGE_11909_4 = 131;
        public const int SHOP_PAGE_11909_5 = 132;
        public const int SHOP_PAGE_11910_1 = 133;
        public const int SHOP_PAGE_11910_2 = 134;
        public const int SHOP_PAGE_11910_3 = 135;
        public const int SHOP_PAGE_11910_4 = 136;
        public const int SHOP_PAGE_11910_5 = 137;
        public const int SHOP_PAGE_11911_1 = 138;
        public const int SHOP_PAGE_11911_2 = 139;
        public const int SHOP_PAGE_11911_3 = 140;
        public const int SHOP_PAGE_11911_4 = 141;
        public const int SHOP_PAGE_11911_5 = 142;
        public const int SHOP_PAGE_11912_1 = 143;
        public const int SHOP_PAGE_11912_2 = 144;
        public const int SHOP_PAGE_11912_3 = 145;
        public const int SHOP_PAGE_11912_4 = 146;
        public const int SHOP_PAGE_11912_5 = 147;
        public const int SHOP_PAGE_11913_1 = 148;
        public const int SHOP_PAGE_11913_2 = 149;
        public const int SHOP_PAGE_11913_3 = 150;
        public const int SHOP_PAGE_11913_4 = 151;
        public const int SHOP_PAGE_11913_5 = 152;
        public const int SHOP_PAGE_11914_1 = 153;
        public const int SHOP_PAGE_11914_2 = 154;
        public const int SHOP_PAGE_11914_3 = 155;
        public const int SHOP_PAGE_11914_4 = 156;
        public const int SHOP_PAGE_11914_5 = 157;
        public const int SHOP_PAGE_11915_1 = 158;
        public const int SHOP_PAGE_11915_2 = 159;
        public const int SHOP_PAGE_11915_3 = 160;
        public const int SHOP_PAGE_11915_4 = 161;
        public const int SHOP_PAGE_11915_5 = 162;
        public const int SHOP_PAGE_11301_1 = 163;
        public const int SHOP_PAGE_11301_2 = 164;
        public const int SHOP_PAGE_11301_3 = 165;
        public const int SHOP_PAGE_11301_4 = 166;
        public const int SHOP_PAGE_11301_5 = 167;
        public const int SHOP_PAGE_11301_6 = 168;
        public const int SHOP_PAGE_11301_7 = 169;
        public const int SHOP_PAGE_11301_8 = 170;
        /// <summary>
        /// config ShopPageItemConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, ShopPageItem> shopPageItemConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static ShopPageItem propShopPageItemById(int nId)
        {
            ShopPageItem data;
            if (!shopPageItemConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(ShopPageItem).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}