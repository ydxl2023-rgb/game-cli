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
        /// LG-CommonTextConfig-CommonText-1101-Context-13-2-0
        /// 本地试跑区
        /// </summary>
        public const int LGCommonTextContext1320 = 1816575537;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-1102-Context-14-2-0
        /// 该入口尚未接入业务接口
        /// </summary>
        public const int LGCommonTextContext1420 = 923892839;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-1103-Context-15-2-0
        /// 暂无公告
        /// </summary>
        public const int LGCommonTextContext1520 = 1156808357;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2001-Context-16-2-0
        /// 商店
        /// </summary>
        public const int LGCommonTextContext1620 = 1558262934;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2002-Context-17-2-0
        /// 活动
        /// </summary>
        public const int LGCommonTextContext1720 = 1454772614;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2003-Context-18-2-0
        /// 排行榜
        /// </summary>
        public const int LGCommonTextContext1820 = 1032949346;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2004-Context-19-2-0
        /// 背包
        /// </summary>
        public const int LGCommonTextContext1920 = 1016374290;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2005-Context-20-2-0
        /// 图鉴
        /// </summary>
        public const int LGCommonTextContext2020 = 1567228007;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2006-Context-21-2-0
        /// 荣誉之路
        /// </summary>
        public const int LGCommonTextContext2120 = 637692895;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2007-Context-22-2-0
        /// 故事
        /// </summary>
        public const int LGCommonTextContext2220 = 1631922027;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2008-Context-23-2-0
        /// 球员
        /// </summary>
        public const int LGCommonTextContext2320 = 1420731099;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2009-Context-24-2-0
        /// 球队
        /// </summary>
        public const int LGCommonTextContext2420 = 1933503287;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2010-Context-25-2-0
        /// 社团
        /// </summary>
        public const int LGCommonTextContext2520 = 597073758;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-2011-Context-26-2-0
        /// 自由时间
        /// </summary>
        public const int LGCommonTextContext2620 = 1845629210;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3025-Context-27-2-0
        /// 直购特典1
        /// </summary>
        public const int LGCommonTextContext2720 = 1284114143;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3033-Context-29-2-0
        /// 剩余:{0}次
        /// </summary>
        public const int LGCommonTextContext2920 = 1700565480;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3034-Context-30-2-0
        /// {0}天{1}时{2}分
        /// </summary>
        public const int LGCommonTextContext3020 = 1641883401;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3035-Context-31-2-0
        /// 乌鸦VS猫
        /// </summary>
        public const int LGCommonTextContext3120 = 347416959;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3039-Context-33-2-0
        /// 预览
        /// </summary>
        public const int LGCommonTextContext3320 = 959632969;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3062-Context-35-2-0
        /// 广告获取
        /// </summary>
        public const int LGCommonTextContext3520 = 1044575055;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3001-Context-36-2-0
        /// 充值
        /// </summary>
        public const int LGCommonTextContext3620 = 1477859555;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3002-Context-37-2-0
        /// 礼包
        /// </summary>
        public const int LGCommonTextContext3720 = 991942359;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3003-Context-38-2-0
        /// 本地模拟商城 · 不产生真实扣款
        /// </summary>
        public const int LGCommonTextContext3820 = 967554707;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3005-Context-40-2-0
        /// {0}钻石
        /// </summary>
        public const int LGCommonTextContext4020 = 1263492890;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3006-Context-41-2-0
        /// 每日补给礼包
        /// </summary>
        public const int LGCommonTextContext4120 = 1266802158;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3007-Context-42-2-0
        /// 成长补给礼包
        /// </summary>
        public const int LGCommonTextContext4220 = 1566417466;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3008-Context-43-2-0
        /// 豪华补给礼包
        /// </summary>
        public const int LGCommonTextContext4320 = 412067390;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3009-Context-44-2-0
        /// 剩余可购：{0}
        /// </summary>
        public const int LGCommonTextContext4420 = 890705618;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3010-Context-45-2-0
        /// 已售罄
        /// </summary>
        public const int LGCommonTextContext4520 = 1970181527;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3011-Context-46-2-0
        /// 模拟购买
        /// </summary>
        public const int LGCommonTextContext4620 = 2124423587;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3012-Context-47-2-0
        /// 模拟购买后获得以下道具
        /// </summary>
        public const int LGCommonTextContext4720 = 499800567;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3013-Context-48-2-0
        /// 钻石
        /// </summary>
        public const int LGCommonTextContext4820 = 223145131;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3015-Context-50-2-0
        /// 本地模拟购买成功，已发放{0}钻石
        /// </summary>
        public const int LGCommonTextContext5020 = 753924116;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3016-Context-51-2-0
        /// 模拟余额不足
        /// </summary>
        public const int LGCommonTextContext5120 = 332452556;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3017-Context-52-2-0
        /// 请选择可购买的商品
        /// </summary>
        public const int LGCommonTextContext5220 = 1337254412;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3018-Context-53-2-0
        /// 本地奖励数量已达上限
        /// </summary>
        public const int LGCommonTextContext5320 = 702302076;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3019-Context-54-2-0
        /// 模板使用本地示例商品与模拟余额；付款、活动及客服接口尚未接入。
        /// </summary>
        public const int LGCommonTextContext5420 = 1574501996;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3020-Context-55-2-0
        /// 模拟余额：{0}
        /// </summary>
        public const int LGCommonTextContext5520 = 1805514107;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-3021-Context-56-2-0
        /// 首充赠送
        /// </summary>
        public const int LGCommonTextContext5620 = 1628507111;
        /// <summary>
        /// LG-FunctionSystemConfig-FunctionSystem-10003-Name-5-2-0
        /// 家园系统
        /// </summary>
        public const int LGFunctionSystemName520 = 1211852315;
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