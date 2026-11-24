using System;
using System.Collections.Generic;

namespace Config
{
    [System.Serializable]
    public class LanguageConfig : IConfigData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, string> data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object GetData()
        {
            return data;
        }
    }

    /// <summary>
    /// export excel Effect data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// LG-AttrConfig-Attr-110001-Name-3-2-0
        /// 战士（力量）
        /// </summary>
        public const int LGAttrName320 = 156186366;
        /// <summary>
        /// LG-AttrConfig-Attr-120001-Name-104-2-0
        /// 法师(智力）
        /// </summary>
        public const int LGAttrName10420 = 2000663003;
        /// <summary>
        /// LG-AttrConfig-Attr-130001-Name-205-2-0
        /// 牧师（精神）
        /// </summary>
        public const int LGAttrName20520 = 270992616;
        /// <summary>
        /// LG-AttrConfig-Attr-140001-Name-306-2-0
        /// 强盗（耐力）
        /// </summary>
        public const int LGAttrName30620 = 2124626765;
        /// <summary>
        /// LG-AttrConfig-Attr-150001-Name-407-2-0
        /// 猎人（敏捷）
        /// </summary>
        public const int LGAttrName40720 = 543118858;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-100001-Name-3-2-0
        /// 最弱小怪（战士）
        /// </summary>
        public const int LGAttrFactorName320 = 128066077;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-100002-Name-4-2-0
        /// 属性削弱20%
        /// </summary>
        public const int LGAttrFactorName420 = 1801966391;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-100003-Name-5-2-0
        /// 属性削弱40%
        /// </summary>
        public const int LGAttrFactorName520 = 1515913121;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-100004-Name-6-2-0
        /// 属性削弱60%
        /// </summary>
        public const int LGAttrFactorName620 = 1120595615;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-100005-Name-7-2-0
        /// 属性削弱80%
        /// </summary>
        public const int LGAttrFactorName720 = 1210924917;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-100006-Name-8-2-0
        /// 标准属性
        /// </summary>
        public const int LGAttrFactorName820 = 1262250415;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-100007-Name-9-2-0
        /// 属性增强20%
        /// </summary>
        public const int LGAttrFactorName920 = 359540641;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-100008-Name-10-2-0
        /// 属性增强30%
        /// </summary>
        public const int LGAttrFactorName1020 = 1691569154;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-100009-Name-11-2-0
        /// 属性增强50%
        /// </summary>
        public const int LGAttrFactorName1120 = 1422379122;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-100010-Name-12-2-0
        /// 属性增强100%
        /// </summary>
        public const int LGAttrFactorName1220 = 871146729;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-200001-Name-15-2-0
        /// 最弱小怪（法师）
        /// </summary>
        public const int LGAttrFactorName1520 = 985028741;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-300001-Name-27-2-0
        /// 最弱小怪(牧师)
        /// </summary>
        public const int LGAttrFactorName2720 = 1251948873;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-400001-Name-39-2-0
        /// 最弱小怪（强盗）
        /// </summary>
        public const int LGAttrFactorName3920 = 281260033;
        /// <summary>
        /// LG-AttrFactorConfig-AttrFactor-500001-Name-51-2-0
        /// 最弱小怪（猎人）
        /// </summary>
        public const int LGAttrFactorName5120 = 1894863122;
        /// <summary>
        /// LG-CharacterConfig-Character-10001-Name-3-2-0
        /// 战士
        /// </summary>
        public const int LGCharacterName320 = 792994471;
        /// <summary>
        /// LG-CharacterConfig-Character-10002-Name-4-2-0
        /// 法师
        /// </summary>
        public const int LGCharacterName420 = 121508577;
        /// <summary>
        /// LG-CharacterConfig-Character-10003-Name-5-2-0
        /// 牧师
        /// </summary>
        public const int LGCharacterName520 = 519160283;
        /// <summary>
        /// LG-CharacterConfig-Character-10004-Name-6-2-0
        /// 盗贼
        /// </summary>
        public const int LGCharacterName620 = 1613537345;
        /// <summary>
        /// LG-CharacterConfig-Character-10005-Name-7-2-0
        /// 猎人
        /// </summary>
        public const int LGCharacterName720 = 273864367;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-101-Context-3-2-0
        /// 闪避
        /// </summary>
        public const int LGCommonTextContext320 = 1513952641;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-102-Context-4-2-0
        /// 格挡
        /// </summary>
        public const int LGCommonTextContext420 = 786444617;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-103-Context-5-2-0
        /// 未命中
        /// </summary>
        public const int LGCommonTextContext520 = 258711993;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-1001-Context-6-2-0
        /// 最近登录
        /// </summary>
        public const int LGCommonTextContext620 = 530763846;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-1002-Context-7-2-0
        /// 推荐服务器
        /// </summary>
        public const int LGCommonTextContext720 = 1541870028;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-1003-Context-8-2-0
        /// {0}-{1}区
        /// </summary>
        public const int LGCommonTextContext820 = 690591166;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-1004-Context-9-2-0
        /// {0}区 · 本地测试
        /// </summary>
        public const int LGCommonTextContext920 = 1855190616;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-1005-Context-10-2-0
        /// 流畅
        /// </summary>
        public const int LGCommonTextContext1020 = 124732961;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-1006-Context-11-2-0
        /// 火爆
        /// </summary>
        public const int LGCommonTextContext1120 = 920840077;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-1007-Context-12-2-0
        /// 维护
        /// </summary>
        public const int LGCommonTextContext1220 = 767299213;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2001-Context-13-2-0
        /// 商店
        /// </summary>
        public const int LGCommonTextContext1320 = 377939525;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2002-Context-14-2-0
        /// 活动
        /// </summary>
        public const int LGCommonTextContext1420 = 2071662691;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2003-Context-15-2-0
        /// 排行榜
        /// </summary>
        public const int LGCommonTextContext1520 = 1112206753;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2004-Context-16-2-0
        /// 背包
        /// </summary>
        public const int LGCommonTextContext1620 = 608767699;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2005-Context-17-2-0
        /// 图鉴
        /// </summary>
        public const int LGCommonTextContext1720 = 1293577557;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2006-Context-18-2-0
        /// 荣誉之路
        /// </summary>
        public const int LGCommonTextContext1820 = 1149448203;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2007-Context-19-2-0
        /// 故事
        /// </summary>
        public const int LGCommonTextContext1920 = 1190383177;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2008-Context-20-2-0
        /// 球员
        /// </summary>
        public const int LGCommonTextContext2020 = 126344478;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2009-Context-21-2-0
        /// 球队
        /// </summary>
        public const int LGCommonTextContext2120 = 1193117464;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2010-Context-22-2-0
        /// 社团
        /// </summary>
        public const int LGCommonTextContext2220 = 2053174991;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2011-Context-23-2-0
        /// 自由时间
        /// </summary>
        public const int LGCommonTextContext2320 = 124037985;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3001-Context-24-2-0
        /// 充值
        /// </summary>
        public const int LGCommonTextContext2420 = 1162366916;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3002-Context-25-2-0
        /// 礼包
        /// </summary>
        public const int LGCommonTextContext2520 = 1020902332;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3003-Context-26-2-0
        /// 本地模拟商城 · 不产生真实扣款
        /// </summary>
        public const int LGCommonTextContext2620 = 2034574324;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3005-Context-28-2-0
        /// {0}钻石
        /// </summary>
        public const int LGCommonTextContext2820 = 776390596;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3006-Context-29-2-0
        /// 每日补给礼包
        /// </summary>
        public const int LGCommonTextContext2920 = 1987598716;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3007-Context-30-2-0
        /// 成长补给礼包
        /// </summary>
        public const int LGCommonTextContext3020 = 1924541015;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3008-Context-31-2-0
        /// 豪华补给礼包
        /// </summary>
        public const int LGCommonTextContext3120 = 1001512947;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3009-Context-32-2-0
        /// 剩余可购：{0}
        /// </summary>
        public const int LGCommonTextContext3220 = 1553083199;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3010-Context-33-2-0
        /// 已售罄
        /// </summary>
        public const int LGCommonTextContext3320 = 716274734;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3011-Context-34-2-0
        /// 模拟购买
        /// </summary>
        public const int LGCommonTextContext3420 = 507342890;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3012-Context-35-2-0
        /// 模拟购买后获得以下道具
        /// </summary>
        public const int LGCommonTextContext3520 = 663432854;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3013-Context-36-2-0
        /// 钻石
        /// </summary>
        public const int LGCommonTextContext3620 = 1132707122;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3015-Context-38-2-0
        /// 本地模拟购买成功，已发放{0}钻石
        /// </summary>
        public const int LGCommonTextContext3820 = 1985261034;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3016-Context-39-2-0
        /// 模拟余额不足
        /// </summary>
        public const int LGCommonTextContext3920 = 1526122558;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3017-Context-40-2-0
        /// 请选择可购买的商品
        /// </summary>
        public const int LGCommonTextContext4020 = 68645767;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3018-Context-41-2-0
        /// 本地奖励数量已达上限
        /// </summary>
        public const int LGCommonTextContext4120 = 1190604971;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3019-Context-42-2-0
        /// 模板使用本地示例商品与模拟余额；付款、活动及客服接口尚未接入。
        /// </summary>
        public const int LGCommonTextContext4220 = 2006236223;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3020-Context-43-2-0
        /// 模拟余额：{0}
        /// </summary>
        public const int LGCommonTextContext4320 = 1837377152;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3021-Context-44-2-0
        /// 首充赠送
        /// </summary>
        public const int LGCommonTextContext4420 = 763157560;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3025-Context-48-2-0
        /// 直购特典1
        /// </summary>
        public const int LGCommonTextContext4820 = 1476697072;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3033-Context-56-2-0
        /// 剩余:{0}次
        /// </summary>
        public const int LGCommonTextContext5620 = 2118164682;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3034-Context-57-2-0
        /// {0}天{1}时{2}分
        /// </summary>
        public const int LGCommonTextContext5720 = 1258060798;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3035-Context-58-2-0
        /// 乌鸦VS猫
        /// </summary>
        public const int LGCommonTextContext5820 = 1575737738;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3039-Context-62-2-0
        /// 预览
        /// </summary>
        public const int LGCommonTextContext6220 = 1773047179;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3062-Context-85-2-0
        /// 广告获取
        /// </summary>
        public const int LGCommonTextContext8520 = 2016919144;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20000-Context-86-2-0
        /// 超级招募券
        /// </summary>
        public const int LGCommonTextContext8620 = 296039080;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20001-Context-87-2-0
        /// 卓越招募券
        /// </summary>
        public const int LGCommonTextContext8720 = 632736598;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20002-Context-88-2-0
        /// 追忆招募券
        /// </summary>
        public const int LGCommonTextContext8820 = 831840300;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20003-Context-89-2-0
        /// 限定应援招募券
        /// </summary>
        public const int LGCommonTextContext8920 = 1101915470;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20004-Context-90-2-0
        /// 追忆升阶球・红
        /// </summary>
        public const int LGCommonTextContext9020 = 1117519539;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20005-Context-91-2-0
        /// 体力礼盒(大)
        /// </summary>
        public const int LGCommonTextContext9120 = 509777841;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20006-Context-92-2-0
        /// 随机N球员(4星)
        /// </summary>
        public const int LGCommonTextContext9220 = 359010423;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20007-Context-93-2-0
        /// 牛奶兑换券
        /// </summary>
        public const int LGCommonTextContext9320 = 1370199137;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20008-Context-94-2-0
        /// 橙色潜能部位自选礼盒
        /// </summary>
        public const int LGCommonTextContext9420 = 481000283;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20009-Context-95-2-0
        /// 橙色潜能随机礼盒
        /// </summary>
        public const int LGCommonTextContext9520 = 263842105;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20010-Context-96-2-0
        /// 经验值
        /// </summary>
        public const int LGCommonTextContext9620 = 1368292130;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20011-Context-97-2-0
        /// 潜能精华
        /// </summary>
        public const int LGCommonTextContext9720 = 559939552;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20012-Context-98-2-0
        /// 紫色潜能随机礼盒
        /// </summary>
        public const int LGCommonTextContext9820 = 607176662;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20013-Context-99-2-0
        /// 泽村大地碎片
        /// </summary>
        public const int LGCommonTextContext9920 = 363917672;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20014-Context-100-2-0
        /// 田中龙之介碎片
        /// </summary>
        public const int LGCommonTextContext10020 = 570979064;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20015-Context-101-2-0
        /// 青根高伸碎片
        /// </summary>
        public const int LGCommonTextContext10120 = 272723496;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20016-Context-102-2-0
        /// 岩泉一碎片
        /// </summary>
        public const int LGCommonTextContext10220 = 1238955836;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20017-Context-103-2-0
        /// 灰羽列夫碎片
        /// </summary>
        public const int LGCommonTextContext10320 = 441468516;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20018-Context-104-2-0
        /// 金币
        /// </summary>
        public const int LGCommonTextContext10420 = 155063744;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20019-Context-105-2-0
        /// 高级招募券
        /// </summary>
        public const int LGCommonTextContext10520 = 366821552;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20020-Context-106-2-0
        /// SSR球员直升卡
        /// </summary>
        public const int LGCommonTextContext10620 = 483906133;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20021-Context-107-2-0
        /// 橙色追忆直升卡
        /// </summary>
        public const int LGCommonTextContext10720 = 1857535781;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20022-Context-108-2-0
        /// 长高高牛奶
        /// </summary>
        public const int LGCommonTextContext10820 = 704858421;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20023-Context-109-2-0
        /// 能量棒
        /// </summary>
        public const int LGCommonTextContext10920 = 1616302397;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20024-Context-110-2-0
        /// 随机N球员(3星)
        /// </summary>
        public const int LGCommonTextContext11020 = 1621584202;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20025-Context-111-2-0
        /// 紫色应援碎片自选礼盒
        /// </summary>
        public const int LGCommonTextContext11120 = 128830694;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20026-Context-112-2-0
        /// 装饰币
        /// </summary>
        public const int LGCommonTextContext11220 = 2027999658;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20027-Context-113-2-0
        /// 谷地仁花(制服)碎片
        /// </summary>
        public const int LGCommonTextContext11320 = 343126990;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20028-Context-114-2-0
        /// 青叶城西应援团碎片
        /// </summary>
        public const int LGCommonTextContext11420 = 904344274;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20029-Context-115-2-0
        /// 伊达工业应援团碎片
        /// </summary>
        public const int LGCommonTextContext11520 = 496666982;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20030-Context-116-2-0
        /// 泷之上祐辅碎片
        /// </summary>
        public const int LGCommonTextContext11620 = 466848033;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20031-Context-117-2-0
        /// 岛田诚碎片
        /// </summary>
        public const int LGCommonTextContext11720 = 1747348165;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20032-Context-118-2-0
        /// 强攻增强・WS
        /// </summary>
        public const int LGCommonTextContext11820 = 265368565;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20033-Context-119-2-0
        /// 快攻增强・MB
        /// </summary>
        public const int LGCommonTextContext11920 = 2015177737;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20034-Context-120-2-0
        /// 传球增强・S
        /// </summary>
        public const int LGCommonTextContext12020 = 681862092;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20035-Context-121-2-0
        /// 广告补给礼包
        /// </summary>
        public const int LGCommonTextContext12120 = 1294709840;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20036-Context-122-2-0
        /// 免费
        /// </summary>
        public const int LGCommonTextContext12220 = 1535723648;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20037-Context-123-2-0
        /// 随机SR球员碎片礼盒
        /// </summary>
        public const int LGCommonTextContext12320 = 1767621684;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20038-Context-124-2-0
        /// 应援招募券
        /// </summary>
        public const int LGCommonTextContext12420 = 2071887940;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20039-Context-125-2-0
        /// 基地经费
        /// </summary>
        public const int LGCommonTextContext12520 = 329984640;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20040-Context-126-2-0
        /// 改名卡
        /// </summary>
        public const int LGCommonTextContext12620 = 1506653741;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20041-Context-127-2-0
        /// 训练手册
        /// </summary>
        public const int LGCommonTextContext12720 = 2118462337;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20042-Context-128-2-0
        /// 谷地仁花(制服)
        /// </summary>
        public const int LGCommonTextContext12820 = 10031125;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20043-Context-129-2-0
        /// 青叶城西应援团
        /// </summary>
        public const int LGCommonTextContext12920 = 516851457;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20044-Context-130-2-0
        /// 伊达工业应援团
        /// </summary>
        public const int LGCommonTextContext13020 = 1306381962;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20045-Context-131-2-0
        /// 泷之上祐辅
        /// </summary>
        public const int LGCommonTextContext13120 = 1984256082;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20046-Context-132-2-0
        /// 岛田诚
        /// </summary>
        public const int LGCommonTextContext13220 = 1396283602;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20047-Context-133-2-0
        /// 月岛明光
        /// </summary>
        public const int LGCommonTextContext13320 = 387914922;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20048-Context-134-2-0
        /// 天内叶歌
        /// </summary>
        public const int LGCommonTextContext13420 = 1374453930;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20049-Context-135-2-0
        /// 道宫结
        /// </summary>
        public const int LGCommonTextContext13520 = 1019769458;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20050-Context-136-2-0
        /// 田代秀水
        /// </summary>
        public const int LGCommonTextContext13620 = 1860776545;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20051-Context-137-2-0
        /// 内泽英纪
        /// </summary>
        public const int LGCommonTextContext13720 = 1150975401;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20052-Context-138-2-0
        /// 森行成
        /// </summary>
        public const int LGCommonTextContext13820 = 1231781205;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20053-Context-139-2-0
        /// 山本茜
        /// </summary>
        public const int LGCommonTextContext13920 = 1526627165;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20054-Context-140-2-0
        /// 灰羽爱丽莎
        /// </summary>
        public const int LGCommonTextContext14020 = 1377428440;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20055-Context-141-2-0
        /// 中岛夫妇
        /// </summary>
        public const int LGCommonTextContext14120 = 1482211152;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20056-Context-142-2-0
        /// 中岛家的孩子
        /// </summary>
        public const int LGCommonTextContext14220 = 1207220492;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20057-Context-143-2-0
        /// 2.5周年庆典 背景No.1
        /// </summary>
        public const int LGCommonTextContext14320 = 1282039572;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20058-Context-144-2-0
        /// 2.5周年庆典 背景No.2
        /// </summary>
        public const int LGCommonTextContext14420 = 933045216;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20059-Context-145-2-0
        /// 放课后 背景No.1
        /// </summary>
        public const int LGCommonTextContext14520 = 1675745352;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20060-Context-146-2-0
        /// 东峰旭 背景No.1
        /// </summary>
        public const int LGCommonTextContext14620 = 443826805;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20061-Context-147-2-0
        /// 赏樱 背景No.2
        /// </summary>
        public const int LGCommonTextContext14720 = 1181817597;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20062-Context-148-2-0
        /// 爵士乐队 背景No.1 
        /// </summary>
        public const int LGCommonTextContext14820 = 917531341;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20063-Context-149-2-0
        /// 二周年庆典 背景No.1
        /// </summary>
        public const int LGCommonTextContext14920 = 278683989;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20064-Context-150-2-0
        /// 二周年庆典 背景No.2
        /// </summary>
        public const int LGCommonTextContext15020 = 626938874;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20065-Context-151-2-0
        /// 免费礼包
        /// </summary>
        public const int LGCommonTextContext15120 = 472416926;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20066-Context-152-2-0
        /// 新人补给礼包
        /// </summary>
        public const int LGCommonTextContext15220 = 1859815474;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20067-Context-153-2-0
        /// 成长特惠礼包
        /// </summary>
        public const int LGCommonTextContext15320 = 1438371318;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20068-Context-154-2-0
        /// 高级招募券*5
        /// </summary>
        public const int LGCommonTextContext15420 = 1507906082;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20069-Context-155-2-0
        /// 招募超值礼包
        /// </summary>
        public const int LGCommonTextContext15520 = 1659376846;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20070-Context-156-2-0
        /// 已购买
        /// </summary>
        public const int LGCommonTextContext15620 = 983341481;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20071-Context-157-2-0
        /// 8折
        /// </summary>
        public const int LGCommonTextContext15720 = 166989805;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20072-Context-158-2-0
        /// 合宿远征15-15
        /// </summary>
        public const int LGCommonTextContext15820 = 288675517;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20073-Context-159-2-0
        /// 已领取
        /// </summary>
        public const int LGCommonTextContext15920 = 1092090025;
        /// <summary>
        /// LG-FunctionSystemConfig-FunctionSystem-10001-Name-3-2-0
        /// 核心系统
        /// </summary>
        public const int LGFunctionSystemName320 = 2081170663;
        /// <summary>
        /// LG-FunctionSystemConfig-FunctionSystem-10002-Name-4-2-0
        /// 登录系统
        /// </summary>
        public const int LGFunctionSystemName420 = 1409684769;
        /// <summary>
        /// LG-FunctionSystemConfig-FunctionSystem-10003-Name-5-2-0
        /// 家园系统
        /// </summary>
        public const int LGFunctionSystemName520 = 1211852315;
        /// <summary>
        /// LG-FunctionSystemConfig-FunctionSystem-10004-Name-6-2-0
        /// 邮件系统
        /// </summary>
        public const int LGFunctionSystemName620 = 542677633;
        /// <summary>
        /// LG-FunctionSystemConfig-FunctionSystem-10005-Name-7-2-0
        /// 角色系统
        /// </summary>
        public const int LGFunctionSystemName720 = 129883631;
        /// <summary>
        /// LG-FunctionSystemConfig-FunctionSystem-10006-Name-8-2-0
        /// 背包系统
        /// </summary>
        public const int LGFunctionSystemName820 = 1857346161;
        /// <summary>
        /// LG-FunctionSystemConfig-FunctionSystem-10007-Name-9-2-0
        /// 七日登录
        /// </summary>
        public const int LGFunctionSystemName920 = 1216255419;
        /// <summary>
        /// LG-FunctionSystemConfig-FunctionSystem-10008-Name-10-2-0
        /// 战斗系统
        /// </summary>
        public const int LGFunctionSystemName1020 = 1777863556;
        /// <summary>
        /// LG-FunctionSystemConfig-FunctionSystem-10009-Name-11-2-0
        /// 公告系统
        /// </summary>
        public const int LGFunctionSystemName1120 = 518964932;
        /// <summary>
        /// LG-LevelConfig-Level-10001-Name-3-2-0
        /// 主场景关卡
        /// </summary>
        public const int LGLevelName320 = 337398143;
        /// <summary>
        /// LG-LocalConfig-Local-10001-Name-3-3-0
        /// 开发
        /// </summary>
        public const int LGLocalName330 = 1874082008;
        /// <summary>
        /// LG-LocalConfig-Local-10002-Name-4-3-0
        /// 验证
        /// </summary>
        public const int LGLocalName430 = 119451194;
        /// <summary>
        /// LG-LocalConfig-Local-10003-Name-5-3-0
        /// 调试
        /// </summary>
        public const int LGLocalName530 = 1378224108;
        /// <summary>
        /// LG-LocalConfig-Local-10004-Name-6-3-0
        /// 发布
        /// </summary>
        public const int LGLocalName630 = 971227546;
        /// <summary>
        /// LG-LocalConfig-Local-10005-Name-7-3-0
        /// 体验
        /// </summary>
        public const int LGLocalName730 = 618830240;
        /// <summary>
        /// LG-LocalConfig-Local-10006-Name-8-3-0
        /// 测试
        /// </summary>
        public const int LGLocalName830 = 1884463178;
        /// <summary>
        /// LG-SceneConfig-Scene-10001-Name-3-2-0
        /// 登录
        /// </summary>
        public const int LGSceneName320 = 930479215;
        /// <summary>
        /// LG-SceneConfig-Scene-10002-Name-4-2-0
        /// 家园
        /// </summary>
        public const int LGSceneName420 = 341408041;
        /// <summary>
        /// LG-SkillConfig-Skill-11001-Name-3-2-0
        /// 普通攻击
        /// </summary>
        public const int LGSkillName320 = 1832221780;
        /// <summary>
        /// LG-SkillConfig-Skill-11002-Name-4-2-0
        /// 横扫千军
        /// </summary>
        public const int LGSkillName420 = 1040508382;
        /// <summary>
        /// LG-SkillConfig-Skill-11003-Name-5-2-0
        /// 刺杀
        /// </summary>
        public const int LGSkillName520 = 216297956;
        /// <summary>
        /// LG-SkillConfig-Skill-11004-Name-6-2-0
        /// 割喉
        /// </summary>
        public const int LGSkillName620 = 1741689490;
        /// <summary>
        /// LG-SkillConfig-Skill-11005-Name-7-2-0
        /// 斩杀
        /// </summary>
        public const int LGSkillName720 = 595773932;
        /// <summary>
        /// LG-SkillConfig-Skill-11006-Name-8-2-0
        /// 轻盈
        /// </summary>
        public const int LGSkillName820 = 950525358;
        /// <summary>
        /// LG-SkillConfig-Skill-11007-Name-9-2-0
        /// 反击
        /// </summary>
        public const int LGSkillName920 = 1905587020;
        /// <summary>
        /// LG-SkillConfig-Skill-11008-Name-10-2-0
        /// 冲锋
        /// </summary>
        public const int LGSkillName1020 = 1046526021;
        /// <summary>
        /// LG-SkillConfig-Skill-11009-Name-11-2-0
        /// 嘲讽
        /// </summary>
        public const int LGSkillName1120 = 1248360949;
        /// <summary>
        /// LG-SkillConfig-Skill-11010-Name-12-2-0
        /// 坚韧
        /// </summary>
        public const int LGSkillName1220 = 300920226;
        /// <summary>
        /// LG-SkillConfig-Skill-11011-Name-13-2-0
        /// 愤怒
        /// </summary>
        public const int LGSkillName1320 = 1371989410;
        /// <summary>
        /// LG-SkillConfig-Skill-11012-Name-14-2-0
        /// 重伤
        /// </summary>
        public const int LGSkillName1420 = 1672738542;
        /// <summary>
        /// LG-SkillConfig-Skill-11013-Name-15-2-0
        /// 怒吼
        /// </summary>
        public const int LGSkillName1520 = 1235367646;
        /// <summary>
        /// LG-SkillConfig-Skill-11014-Name-16-2-0
        /// 破甲
        /// </summary>
        public const int LGSkillName1620 = 1997310562;
        /// <summary>
        /// LG-SkillConfig-Skill-11015-Name-17-2-0
        /// 盾墙
        /// </summary>
        public const int LGSkillName1720 = 1025088146;
        /// <summary>
        /// LG-SkillConfig-Skill-11016-Name-18-2-0
        /// 嗜血
        /// </summary>
        public const int LGSkillName1820 = 886168550;
        /// <summary>
        /// LG-SkillConfig-Skill-11017-Name-19-2-0
        /// 盾击
        /// </summary>
        public const int LGSkillName1920 = 1742867062;
        /// <summary>
        /// LG-SkillConfig-Skill-11018-Name-20-2-0
        /// 复仇
        /// </summary>
        public const int LGSkillName2020 = 1569098731;
        /// <summary>
        /// LG-SkillConfig-Skill-11019-Name-21-2-0
        /// 破斧
        /// </summary>
        public const int LGSkillName2120 = 1967927427;
        /// <summary>
        /// LG-SkillConfig-Skill-12001-Name-22-2-0
        /// 射击
        /// </summary>
        public const int LGSkillName2220 = 374648296;
        /// <summary>
        /// LG-SkillConfig-Skill-12002-Name-23-2-0
        /// 瞄准射击
        /// </summary>
        public const int LGSkillName2320 = 841268946;
        /// <summary>
        /// LG-SkillConfig-Skill-12003-Name-24-2-0
        /// 一箭穿心
        /// </summary>
        public const int LGSkillName2420 = 549739256;
        /// <summary>
        /// LG-SkillConfig-Skill-12004-Name-25-2-0
        /// 箭雨
        /// </summary>
        public const int LGSkillName2520 = 1770870582;
        /// <summary>
        /// LG-SkillConfig-Skill-12005-Name-26-2-0
        /// 飞箭术
        /// </summary>
        public const int LGSkillName2620 = 1486131304;
        /// <summary>
        /// LG-SkillConfig-Skill-13001-Name-27-2-0
        /// 乱砍
        /// </summary>
        public const int LGSkillName2720 = 1112632370;
        /// <summary>
        /// LG-SkillConfig-Skill-13002-Name-28-2-0
        /// 双剑刺杀
        /// </summary>
        public const int LGSkillName2820 = 236206522;
        /// <summary>
        /// LG-SkillConfig-Skill-13003-Name-29-2-0
        /// 狂风斩
        /// </summary>
        public const int LGSkillName2920 = 47987930;
        /// <summary>
        /// LG-SkillConfig-Skill-13004-Name-30-2-0
        /// 绞喉
        /// </summary>
        public const int LGSkillName3020 = 358535817;
        /// <summary>
        /// LG-SkillConfig-Skill-13005-Name-31-2-0
        /// 怒斩
        /// </summary>
        public const int LGSkillName3120 = 1557367637;
        /// <summary>
        /// LG-SkillConfig-Skill-13006-Name-32-2-0
        /// 猛击
        /// </summary>
        public const int LGSkillName3220 = 686975033;
        /// <summary>
        /// LG-SkillConfig-Skill-13007-Name-33-2-0
        /// 谋杀
        /// </summary>
        public const int LGSkillName3320 = 1368684141;
        /// <summary>
        /// LG-SkillConfig-Skill-14001-Name-34-2-0
        /// 火球术
        /// </summary>
        public const int LGSkillName3420 = 568850693;
        /// <summary>
        /// LG-SkillConfig-Skill-14002-Name-35-2-0
        /// 电击术
        /// </summary>
        public const int LGSkillName3520 = 1362777123;
        /// <summary>
        /// LG-SkillConfig-Skill-14003-Name-36-2-0
        /// 电击风暴
        /// </summary>
        public const int LGSkillName3620 = 1611426101;
        /// <summary>
        /// LG-SkillConfig-Skill-14004-Name-37-2-0
        /// 瞬移术
        /// </summary>
        public const int LGSkillName3720 = 1219869223;
        /// <summary>
        /// LG-SkillConfig-Skill-14005-Name-38-2-0
        /// 雷电术
        /// </summary>
        public const int LGSkillName3820 = 2031532253;
        /// <summary>
        /// LG-SkillConfig-Skill-14006-Name-39-2-0
        /// 火雨
        /// </summary>
        public const int LGSkillName3920 = 2021544003;
        /// <summary>
        /// LG-SkillConfig-Skill-14007-Name-40-2-0
        /// 召唤术
        /// </summary>
        public const int LGSkillName4020 = 1170939596;
        /// <summary>
        /// LG-SkillConfig-Skill-15001-Name-41-2-0
        /// 毒球术
        /// </summary>
        public const int LGSkillName4120 = 2146694500;
        /// <summary>
        /// LG-SkillConfig-Skill-15002-Name-42-2-0
        /// 治疗术
        /// </summary>
        public const int LGSkillName4220 = 1992372052;
        /// <summary>
        /// LG-SkillConfig-Skill-15003-Name-43-2-0
        /// 生命输送
        /// </summary>
        public const int LGSkillName4320 = 516993240;
        /// <summary>
        /// LG-SkillConfig-Skill-20001-Name-45-2-0
        /// 机关火雨
        /// </summary>
        public const int LGSkillName4520 = 411148612;
        /// <summary>
        /// LG-SkillConfig-Skill-30001-Name-46-2-0
        /// 群体防御卷轴
        /// </summary>
        public const int LGSkillName4620 = 271360734;
        /// <summary>
        /// LG-SkillConfig-Skill-30002-Name-47-2-0
        /// 群体治疗卷轴
        /// </summary>
        public const int LGSkillName4720 = 1173704944;
        /// <summary>
        /// LG-SkillConfig-Skill-30003-Name-48-2-0
        /// 群体魔法卷轴
        /// </summary>
        public const int LGSkillName4820 = 1218523462;
        /// <summary>
        /// LG-SkillConfig-Skill-30004-Name-49-2-0
        /// 加速卷轴
        /// </summary>
        public const int LGSkillName4920 = 1061455900;
        /// <summary>
        /// LG-SkillConfig-Skill-30005-Name-50-2-0
        /// 吼叫卷轴
        /// </summary>
        public const int LGSkillName5020 = 946959149;
        /// <summary>
        /// LG-SkillConfig-Skill-30006-Name-51-2-0
        /// 嘲讽卷轴
        /// </summary>
        public const int LGSkillName5120 = 516154247;
        /// <summary>
        /// LG-SkillConfig-Skill-30007-Name-52-2-0
        /// 沉默卷轴
        /// </summary>
        public const int LGSkillName5220 = 605863789;
        /// <summary>
        /// LG-SkillConfig-Skill-30008-Name-53-2-0
        /// 生命恢复卷轴
        /// </summary>
        public const int LGSkillName5320 = 1295965267;
        /// <summary>
        /// LG-SkillConfig-Skill-30009-Name-54-2-0
        /// 魔法恢复卷轴
        /// </summary>
        public const int LGSkillName5420 = 189696725;
        /// <summary>
        /// LG-SkillConfig-Skill-30010-Name-55-2-0
        /// 金色护盾
        /// </summary>
        public const int LGSkillName5520 = 1758220824;
        /// <summary>
        /// config data table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, string> languageConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static string propLanguageById(int nId)
        {
            if (!languageConfig.TryGetValue(nId, out var text))
            {
#if UNITY_EDITOR
			    UnityEngine.Debug.LogError(nId);
#endif
            }

            return text;
        }

        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static string propLanguageById(int nId, params object[] args)
        {
            if (!languageConfig.TryGetValue(nId, out var text))
                throw new NullReferenceException($"Can't find language id {nId}");
            if (args == null || args.Length == 0)
                return text;
            return string.Format(text, args);
        }
    }
}