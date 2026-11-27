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
        /// LG-CommonTextConfig-CommonText-10001-Context-3-2-0
        /// 该入口尚未解锁
        /// </summary>
        public const int LGCommonTextContext320 = 102251217;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-10002-Context-4-2-0
        /// 入口配置无效
        /// </summary>
        public const int LGCommonTextContext420 = 896232473;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-10003-Context-5-2-0
        /// 该系统尚未接入
        /// </summary>
        public const int LGCommonTextContext520 = 1796461769;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-20001-Context-6-2-0
        /// 通过其他玩家分享的链接，可助威他人采集
        /// </summary>
        public const int LGCommonTextContext620 = 1431276171;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30001-Context-7-2-0
        /// 跳过剧情
        /// </summary>
        public const int LGCommonTextContext720 = 491674763;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30002-Context-8-2-0
        /// 是否确认跳过剧情？
        /// </summary>
        public const int LGCommonTextContext820 = 242289651;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30003-Context-9-2-0
        /// 继续
        /// </summary>
        public const int LGCommonTextContext920 = 986178619;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30004-Context-10-2-0
        /// 继续观看
        /// </summary>
        public const int LGCommonTextContext1020 = 649505390;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30005-Context-11-2-0
        /// 前往水井取水，救出被困村民
        /// </summary>
        public const int LGCommonTextContext1120 = 716605408;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30006-Context-12-2-0
        /// 前往交易行，使用 {0} 份水救火
        /// </summary>
        public const int LGCommonTextContext1220 = 1219785518;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30007-Context-13-2-0
        /// 正在救火…
        /// </summary>
        public const int LGCommonTextContext1320 = 2062299876;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30008-Context-14-2-0
        /// 任务完成 · 点击领取
        /// </summary>
        public const int LGCommonTextContext1420 = 1564523158;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30009-Context-15-2-0
        /// 任务 {0} · {1}
        /// </summary>
        public const int LGCommonTextContext1520 = 1472544008;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30010-Context-16-2-0
        /// 救火的水 {0}/{1}
        /// </summary>
        public const int LGCommonTextContext1620 = 86867223;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30011-Context-17-2-0
        /// 救火
        /// </summary>
        public const int LGCommonTextContext1720 = 2052942569;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30012-Context-18-2-0
        /// 剧情视频加载失败，请重试或跳过
        /// </summary>
        public const int LGCommonTextContext1820 = 1219562855;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30013-Context-19-2-0
        /// 重试
        /// </summary>
        public const int LGCommonTextContext1920 = 1199433285;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30014-Context-20-2-0
        /// 牧师
        /// </summary>
        public const int LGCommonTextContext2020 = 431670586;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30015-Context-21-2-0
        /// 商人
        /// </summary>
        public const int LGCommonTextContext2120 = 108111572;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30016-Context-22-2-0
        /// 火还没灭！村子到处都有人被困。先去水井打水救他们出来！
        /// </summary>
        public const int LGCommonTextContext2220 = 1201651642;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30017-Context-23-2-0
        /// 货架还在，账册也没烧完。等我好好整理一下，英雄们带回来的东西就有地方卖了。
        /// </summary>
        public const int LGCommonTextContext2320 = 1555936888;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30018-Context-24-2-0
        /// 任务 {0} · {1} ({2}/{3})
        /// </summary>
        public const int LGCommonTextContext2420 = 1562945626;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30019-Context-25-2-0
        /// 旅店老板
        /// </summary>
        public const int LGCommonTextContext2520 = 2060188468;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30020-Context-26-2-0
        /// 十分感谢你，这边剩下的就交给我吧，修整一下旅店就能恢复给大家做饭了。
        /// 你先去隔壁看看，那边也需要帮忙。
        /// </summary>
        public const int LGCommonTextContext2620 = 842283609;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30021-Context-27-2-0
        /// 前往旅店，使用 {0} 份水救火
        /// </summary>
        public const int LGCommonTextContext2720 = 1151424531;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30022-Context-28-2-0
        /// 前往医院，使用 {0} 份水救火
        /// </summary>
        public const int LGCommonTextContext2820 = 1358272729;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30023-Context-29-2-0
        /// 取水
        /// </summary>
        public const int LGCommonTextContext2920 = 177970127;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30024-Context-30-2-0
        /// 清理
        /// </summary>
        public const int LGCommonTextContext3020 = 802931334;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30025-Context-31-2-0
        /// 救人
        /// </summary>
        public const int LGCommonTextContext3120 = 1188089424;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30026-Context-32-2-0
        /// 点击清理倒塌的横梁
        /// </summary>
        public const int LGCommonTextContext3220 = 12258670;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30027-Context-33-2-0
        /// 点击救出困住的人
        /// </summary>
        public const int LGCommonTextContext3320 = 909053292;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30028-Context-34-2-0
        /// 剩余 {0:0.0} 秒
        /// </summary>
        public const int LGCommonTextContext3420 = 1786167478;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30029-Context-35-2-0
        /// 救援完成，正在领取任务
        /// </summary>
        public const int LGCommonTextContext3520 = 2033717024;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30101-Context-36-2-0
        /// 石盾加伦（战士）
        /// </summary>
        public const int LGCommonTextContext3620 = 999511386;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30102-Context-37-2-0
        /// 咳……我还活着。这条命是你从火里抢回来的。给我一把剑，我会为斯坦恩村而战。
        /// </summary>
        public const int LGCommonTextContext3720 = 1784081962;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30103-Context-38-2-0
        /// 招募加伦
        /// </summary>
        public const int LGCommonTextContext3820 = 1691363286;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30104-Context-39-2-0
        /// 修复医院
        /// </summary>
        public const int LGCommonTextContext3920 = 1526948022;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30105-Context-40-2-0
        /// 火是灭了……可病床、药柜和治疗台都毁了。如果不马上修好医院，这些伤员撑不过今晚....
        /// 修复费用：{0} 金币
        /// </summary>
        public const int LGCommonTextContext4020 = 556728399;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30107-Context-42-2-0
        /// 交易行 · 宁神花
        /// </summary>
        public const int LGCommonTextContext4220 = 1355902083;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30108-Context-43-2-0
        /// 购买 1 份宁神花：{0} 金币
        /// 用于在医院生产微效药剂。
        /// </summary>
        public const int LGCommonTextContext4320 = 372696163;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30109-Context-44-2-0
        /// 购买宁神花
        /// </summary>
        public const int LGCommonTextContext4420 = 578341175;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30110-Context-45-2-0
        /// 医院 · 药剂生产
        /// </summary>
        public const int LGCommonTextContext4520 = 1830544114;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30111-Context-46-2-0
        /// 消耗 {0} 份宁神花，生产 {1} 份微效药剂。
        /// </summary>
        public const int LGCommonTextContext4620 = 187166646;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30112-Context-47-2-0
        /// 生产微效药剂
        /// </summary>
        public const int LGCommonTextContext4720 = 1864794166;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30113-Context-48-2-0
        /// 加伦疗伤
        /// </summary>
        public const int LGCommonTextContext4820 = 1358161906;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30114-Context-49-2-0
        /// 加伦正在前往医院。抵达后使用 1 份微效药剂疗伤。
        /// </summary>
        public const int LGCommonTextContext4920 = 2104276066;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30115-Context-50-2-0
        /// 前往医院
        /// </summary>
        public const int LGCommonTextContext5020 = 1722245789;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30116-Context-51-2-0
        /// 金币 {0}　宁神花 {1}　微效药剂 {2}
        /// </summary>
        public const int LGCommonTextContext5120 = 1124903249;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30117-Context-52-2-0
        /// 已招募石盾加伦
        /// 加伦已加入村庄，接下来修复医院为他疗伤。
        /// </summary>
        public const int LGCommonTextContext5220 = 1737902673;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30118-Context-53-2-0
        /// 加伦生命 {0}%　治疗剩余 {1:0.0} 秒
        /// </summary>
        public const int LGCommonTextContext5320 = 352359909;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30119-Context-54-2-0
        /// 条件不足，请检查金币、材料和前置任务。
        /// </summary>
        public const int LGCommonTextContext5420 = 499880637;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30120-Context-55-2-0
        /// 加伦已完成治疗，任务完成后点击领取。
        /// </summary>
        public const int LGCommonTextContext5520 = 1635707390;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30121-Context-56-2-0
        /// 操作完成 · 点击领取任务奖励
        /// </summary>
        public const int LGCommonTextContext5620 = 2068645570;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30122-Context-57-2-0
        /// 确认获得
        /// </summary>
        public const int LGCommonTextContext5720 = 2045506490;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30123-Context-58-2-0
        /// 后续任务：{0}
        /// </summary>
        public const int LGCommonTextContext5820 = 307852366;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30124-Context-59-2-0
        /// 领取奖励
        /// </summary>
        public const int LGCommonTextContext5920 = 1079753566;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30125-Context-60-2-0
        /// 正在疗伤
        /// </summary>
        public const int LGCommonTextContext6020 = 1310097651;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30126-Context-61-2-0
        /// 前往医院中
        /// </summary>
        public const int LGCommonTextContext6120 = 1338134531;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30201-Context-62-2-0
        /// 医院
        /// </summary>
        public const int LGCommonTextContext6220 = 898224172;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-240002-Context-64-2-0
        /// 金币
        /// </summary>
        public const int LGCommonTextContext6420 = 865851244;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-240003-Context-65-2-0
        /// 英雄们常用的货币
        /// </summary>
        public const int LGCommonTextContext6520 = 1333702558;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254002-Context-66-2-0
        /// 宁神花
        /// </summary>
        public const int LGCommonTextContext6620 = 1918913023;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254003-Context-67-2-0
        /// 常见的草药，可以在简单难度的探索中获得
        /// </summary>
        public const int LGCommonTextContext6720 = 1484770569;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254004-Context-68-2-0
        /// 银叶草
        /// </summary>
        public const int LGCommonTextContext6820 = 800943675;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254005-Context-69-2-0
        /// 优质的草药，可以在中等难度的探索中获得
        /// </summary>
        public const int LGCommonTextContext6920 = 869966689;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254006-Context-70-2-0
        /// 皇血草
        /// </summary>
        public const int LGCommonTextContext7020 = 704738280;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254007-Context-71-2-0
        /// 稀有的草药，可以在困难难度的探索中获得
        /// </summary>
        public const int LGCommonTextContext7120 = 945431138;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254008-Context-72-2-0
        /// 哀伤苔
        /// </summary>
        public const int LGCommonTextContext7220 = 36499836;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254009-Context-73-2-0
        /// 珍贵的草药，可以在噩梦难度的探索中获得
        /// </summary>
        public const int LGCommonTextContext7320 = 1664542146;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254010-Context-74-2-0
        /// 冰盖草
        /// </summary>
        public const int LGCommonTextContext7420 = 1348180185;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254011-Context-75-2-0
        /// 传说的草药，可以在地狱难度的探索中获得
        /// </summary>
        public const int LGCommonTextContext7520 = 1371904535;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254012-Context-76-2-0
        /// 曦光草
        /// </summary>
        public const int LGCommonTextContext7620 = 546803677;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254202-Context-78-2-0
        /// 微效药剂
        /// </summary>
        public const int LGCommonTextContext7820 = 735181770;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254203-Context-79-2-0
        /// 恢复英雄生命
        /// </summary>
        public const int LGCommonTextContext7920 = 665483044;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254204-Context-80-2-0
        /// 次级药剂
        /// </summary>
        public const int LGCommonTextContext8020 = 1421093603;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254206-Context-82-2-0
        /// 普通药剂
        /// </summary>
        public const int LGCommonTextContext8220 = 1613597983;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254208-Context-84-2-0
        /// 强效药剂
        /// </summary>
        public const int LGCommonTextContext8420 = 1084681227;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254210-Context-86-2-0
        /// 超强药剂
        /// </summary>
        public const int LGCommonTextContext8620 = 1800264990;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254212-Context-88-2-0
        /// 特效药剂
        /// </summary>
        public const int LGCommonTextContext8820 = 859608450;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30203-Context-90-2-0
        /// 修复
        /// </summary>
        public const int LGCommonTextContext9020 = 757645431;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30204-Context-91-2-0
        /// 进入
        /// </summary>
        public const int LGCommonTextContext9120 = 928544323;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30205-Context-92-2-0
        /// Lv.{0} 医院
        /// </summary>
        public const int LGCommonTextContext9220 = 1930984407;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30206-Context-93-2-0
        /// 收益 +{0}%    耗时 −{1}秒    容量 {2}人
        /// </summary>
        public const int LGCommonTextContext9320 = 1116575571;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30207-Context-94-2-0
        /// 恢复生命 {0}%    使用时间 {1}秒
        /// 英雄消费价格 {2} 金币
        /// </summary>
        public const int LGCommonTextContext9420 = 1686576007;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30208-Context-95-2-0
        /// 库存 {0}
        /// </summary>
        public const int LGCommonTextContext9520 = 634669387;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30209-Context-96-2-0
        /// 原料 {0} · 点击生产
        /// </summary>
        public const int LGCommonTextContext9620 = 1264826663;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30210-Context-97-2-0
        /// 返回村庄
        /// </summary>
        public const int LGCommonTextContext9720 = 1892663962;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30211-Context-98-2-0
        /// 暂无英雄疗伤
        /// </summary>
        public const int LGCommonTextContext9820 = 93904366;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30212-Context-99-2-0
        /// 加伦  生命 {0}%  治疗剩余 {1:0.0}秒
        /// </summary>
        public const int LGCommonTextContext9920 = 307268466;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30213-Context-100-2-0
        /// 加伦正在前往医院
        /// </summary>
        public const int LGCommonTextContext10020 = 1289128564;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30214-Context-101-2-0
        /// 加伦已恢复生命
        /// </summary>
        public const int LGCommonTextContext10120 = 1748747770;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30215-Context-102-2-0
        /// 宁神花不足，请前往交易行购买
        /// </summary>
        public const int LGCommonTextContext10220 = 565098052;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30216-Context-103-2-0
        /// 金币不足，无法修复医院
        /// </summary>
        public const int LGCommonTextContext10320 = 514966334;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30217-Context-104-2-0
        /// 生产中 {0:0.0}秒
        /// </summary>
        public const int LGCommonTextContext10420 = 446695092;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30218-Context-105-2-0
        /// 下一等级需公会等级 {0}
        /// </summary>
        public const int LGCommonTextContext10520 = 642993866;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30219-Context-106-2-0
        /// 任务：{0}
        /// </summary>
        public const int LGCommonTextContext10620 = 239300916;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30220-Context-107-2-0
        /// 医院收入 {0}
        /// </summary>
        public const int LGCommonTextContext10720 = 1058470479;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30221-Context-108-2-0
        /// 材料
        /// </summary>
        public const int LGCommonTextContext10820 = 110218249;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30222-Context-109-2-0
        /// 药剂
        /// </summary>
        public const int LGCommonTextContext10920 = 1565581519;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30223-Context-110-2-0
        /// 金币 {0}
        /// </summary>
        public const int LGCommonTextContext11020 = 1598924330;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30224-Context-111-2-0
        /// 铁匠
        /// </summary>
        public const int LGCommonTextContext11120 = 1267141660;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-322012-Context-116-2-0
        /// 美味小鱼
        /// </summary>
        public const int LGCommonTextContext11620 = 862793766;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-322013-Context-117-2-0
        /// 常见的食材，可以在简单难度的探索中获得
        /// </summary>
        public const int LGCommonTextContext11720 = 950238674;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-322022-Context-118-2-0
        /// 铜锭
        /// </summary>
        public const int LGCommonTextContext11820 = 514722471;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-322023-Context-119-2-0
        /// 打造10级装备的基础材料，由最基础的金属矿石熔炼而成，广泛分布于新手区域矿山，质地柔软易加工，是初学者制作各类基础物品的必备之物。
        /// </summary>
        public const int LGCommonTextContext11920 = 191521539;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30301-Context-120-2-0
        /// 交易行
        /// </summary>
        public const int LGCommonTextContext12020 = 3901722;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30303-Context-122-2-0
        /// 装备交易尚未开放
        /// </summary>
        public const int LGCommonTextContext12220 = 1371441110;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30305-Context-124-2-0
        /// 剩余 {0}
        /// </summary>
        public const int LGCommonTextContext12420 = 2043833474;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30306-Context-125-2-0
        /// 已拥有 {0}
        /// </summary>
        public const int LGCommonTextContext12520 = 855294276;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30307-Context-126-2-0
        /// 购买
        /// </summary>
        public const int LGCommonTextContext12620 = 207247974;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30308-Context-127-2-0
        /// 取消
        /// </summary>
        public const int LGCommonTextContext12720 = 725532604;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30310-Context-129-2-0
        /// 购买数量
        /// </summary>
        public const int LGCommonTextContext12920 = 1050112299;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30311-Context-130-2-0
        /// 总价 {0} 金币
        /// </summary>
        public const int LGCommonTextContext13020 = 1563026106;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30312-Context-131-2-0
        /// 金币不足
        /// </summary>
        public const int LGCommonTextContext13120 = 1362014188;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30313-Context-132-2-0
        /// 商品余量不足
        /// </summary>
        public const int LGCommonTextContext13220 = 1938679106;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30314-Context-133-2-0
        /// 请先完成医院修复任务
        /// </summary>
        public const int LGCommonTextContext13320 = 636418328;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30315-Context-134-2-0
        /// 购买成功
        /// </summary>
        public const int LGCommonTextContext13420 = 235622962;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30316-Context-135-2-0
        /// 请选择商品
        /// </summary>
        public const int LGCommonTextContext13520 = 1576572276;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30317-Context-136-2-0
        /// 数量超出可购买范围
        /// </summary>
        public const int LGCommonTextContext13620 = 622195466;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30322-Context-141-2-0
        /// 确认购买
        /// </summary>
        public const int LGCommonTextContext14120 = 537071148;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30323-Context-142-2-0
        /// 10级材料
        /// </summary>
        public const int LGCommonTextContext14220 = 1044217234;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30401-Context-143-2-0
        /// 提升等级可增加经营收益
        /// </summary>
        public const int LGCommonTextContext14320 = 124942124;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30402-Context-144-2-0
        /// 自动生产
        /// </summary>
        public const int LGCommonTextContext14420 = 1906809348;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30403-Context-145-2-0
        /// 没有商品了，请尽快制作
        /// </summary>
        public const int LGCommonTextContext14520 = 533929052;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30404-Context-146-2-0
        /// 点击材料即可开始生产
        /// </summary>
        public const int LGCommonTextContext14620 = 1916956604;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30405-Context-147-2-0
        /// {0}级 医院
        /// </summary>
        public const int LGCommonTextContext14720 = 1583417380;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30406-Context-148-2-0
        /// 原材料
        /// </summary>
        public const int LGCommonTextContext14820 = 1469979132;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30407-Context-149-2-0
        /// 成品
        /// </summary>
        public const int LGCommonTextContext14920 = 1802730868;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254024-Context-152-2-0
        /// 夜鳞鲷鱼
        /// </summary>
        public const int LGCommonTextContext15220 = 1312916185;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254025-Context-153-2-0
        /// 优质的食材，可以在中等难度的探索中获得
        /// </summary>
        public const int LGCommonTextContext15320 = 2005836245;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254026-Context-154-2-0
        /// 银头鲑鱼
        /// </summary>
        public const int LGCommonTextContext15420 = 1624600437;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254027-Context-155-2-0
        /// 稀有的食材，可以在困难难度的探索中获得
        /// </summary>
        public const int LGCommonTextContext15520 = 1723918345;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254028-Context-156-2-0
        /// 黑爪龙虾
        /// </summary>
        public const int LGCommonTextContext15620 = 1638110249;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254029-Context-157-2-0
        /// 珍贵的食材，可以在噩梦难度的探索中获得
        /// </summary>
        public const int LGCommonTextContext15720 = 821758573;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254030-Context-158-2-0
        /// 帝王鲑鱼
        /// </summary>
        public const int LGCommonTextContext15820 = 942252934;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254031-Context-159-2-0
        /// 传说的食材，可以在地狱难度的探索中获得
        /// </summary>
        public const int LGCommonTextContext15920 = 711358202;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254032-Context-160-2-0
        /// 澄光石斑
        /// </summary>
        public const int LGCommonTextContext16020 = 1337959623;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254222-Context-162-2-0
        /// 炸鱼
        /// </summary>
        public const int LGCommonTextContext16220 = 1792534030;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254223-Context-163-2-0
        /// 恢复英雄行动力
        /// </summary>
        public const int LGCommonTextContext16320 = 1925501874;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254224-Context-164-2-0
        /// 夜鳞鱼汤
        /// </summary>
        public const int LGCommonTextContext16420 = 486252842;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254226-Context-166-2-0
        /// 腌制鲑鱼
        /// </summary>
        public const int LGCommonTextContext16620 = 27090206;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254228-Context-168-2-0
        /// 香辣龙虾
        /// </summary>
        public const int LGCommonTextContext16820 = 820838370;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254230-Context-170-2-0
        /// 水煮帝鲑
        /// </summary>
        public const int LGCommonTextContext17020 = 509880958;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-254232-Context-172-2-0
        /// 深海石斑
        /// </summary>
        public const int LGCommonTextContext17220 = 2086570846;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30501-Context-174-2-0
        /// 旅店
        /// </summary>
        public const int LGCommonTextContext17420 = 1746919497;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30502-Context-175-2-0
        /// {0}级 旅店
        /// </summary>
        public const int LGCommonTextContext17520 = 1400107395;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30503-Context-176-2-0
        /// 恢复行动力 {0}% · 用餐 {1}秒 · 英雄支付 {2}金币
        /// </summary>
        public const int LGCommonTextContext17620 = 412515217;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30504-Context-177-2-0
        /// 修复旅店：{0}金币
        /// </summary>
        public const int LGCommonTextContext17720 = 367420207;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30505-Context-178-2-0
        /// 完成并领取医院修复任务后开放旅店
        /// </summary>
        public const int LGCommonTextContext17820 = 701417545;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30506-Context-179-2-0
        /// 原料不足，请到交易行购买鱼
        /// </summary>
        public const int LGCommonTextContext17920 = 1303575187;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30507-Context-180-2-0
        /// 安排英雄用餐
        /// </summary>
        public const int LGCommonTextContext18020 = 294636096;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30508-Context-181-2-0
        /// 用餐中，剩余 {0:0.0}秒
        /// </summary>
        public const int LGCommonTextContext18120 = 601768182;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30509-Context-182-2-0
        /// 英雄行动力 {0}%
        /// </summary>
        public const int LGCommonTextContext18220 = 275983520;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30510-Context-183-2-0
        /// 需要已招募英雄、食物和英雄金币，且行动力未满
        /// </summary>
        public const int LGCommonTextContext18320 = 945010585;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30511-Context-184-2-0
        /// 金币不足，无法修复旅店
        /// </summary>
        public const int LGCommonTextContext18420 = 183520591;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30512-Context-185-2-0
        /// 修复后可以制作食物，供英雄恢复行动力
        /// </summary>
        public const int LGCommonTextContext18520 = 497571369;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30513-Context-186-2-0
        /// 食物制作完成
        /// </summary>
        public const int LGCommonTextContext18620 = 685896883;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30514-Context-187-2-0
        /// 旅店修复完成
        /// </summary>
        public const int LGCommonTextContext18720 = 861937401;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-710001-Context-188-2-0
        /// 学徒大剑
        /// </summary>
        public const int LGCommonTextContext18820 = 239258958;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-710002-Context-189-2-0
        /// 学徒匕首
        /// </summary>
        public const int LGCommonTextContext18920 = 1206991244;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-710003-Context-190-2-0
        /// 学徒火枪
        /// </summary>
        public const int LGCommonTextContext19020 = 895396617;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-710004-Context-191-2-0
        /// 学徒橡木法杖
        /// </summary>
        public const int LGCommonTextContext19120 = 236986347;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-710005-Context-192-2-0
        /// 学徒魔导书
        /// </summary>
        public const int LGCommonTextContext19220 = 1818572717;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-876002-Context-195-2-0
        /// 微光碎片
        /// </summary>
        public const int LGCommonTextContext19520 = 582128738;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30601-Context-196-2-0
        /// 铁匠铺
        /// </summary>
        public const int LGCommonTextContext19620 = 1774752952;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30602-Context-197-2-0
        /// 完成并领取任务 60329 后开放铁匠铺
        /// </summary>
        public const int LGCommonTextContext19720 = 1936787506;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30603-Context-198-2-0
        /// 修复铁匠铺 · {0}金币
        /// </summary>
        public const int LGCommonTextContext19820 = 302488116;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30604-Context-199-2-0
        /// 铁匠铺修复完成
        /// </summary>
        public const int LGCommonTextContext19920 = 1142626778;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30605-Context-200-2-0
        /// 配方
        /// </summary>
        public const int LGCommonTextContext20020 = 2136289958;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30606-Context-201-2-0
        /// 货架
        /// </summary>
        public const int LGCommonTextContext20120 = 1490307164;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30607-Context-202-2-0
        /// 英雄
        /// </summary>
        public const int LGCommonTextContext20220 = 1847013442;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30608-Context-203-2-0
        /// 打造
        /// </summary>
        public const int LGCommonTextContext20320 = 1617903076;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30609-Context-204-2-0
        /// 打造数量 {0}
        /// </summary>
        public const int LGCommonTextContext20420 = 1821012958;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30611-Context-206-2-0
        /// 打造完成，装备已放入货架
        /// </summary>
        public const int LGCommonTextContext20620 = 1520062633;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30612-Context-207-2-0
        /// 材料或金币不足
        /// </summary>
        public const int LGCommonTextContext20720 = 1474718767;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30613-Context-208-2-0
        /// 请先选择配方或装备
        /// </summary>
        public const int LGCommonTextContext20820 = 865945969;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30614-Context-209-2-0
        /// 出售给英雄
        /// </summary>
        public const int LGCommonTextContext20920 = 134519443;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30615-Context-210-2-0
        /// 交易完成，英雄已装备武器
        /// </summary>
        public const int LGCommonTextContext21020 = 1816223894;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30616-Context-211-2-0
        /// 当前英雄职业不适用该武器
        /// </summary>
        public const int LGCommonTextContext21120 = 508275828;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30617-Context-212-2-0
        /// 尚未招募英雄
        /// </summary>
        public const int LGCommonTextContext21220 = 1585574230;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30618-Context-213-2-0
        /// 英雄金币不足
        /// </summary>
        public const int LGCommonTextContext21320 = 1501249008;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30619-Context-214-2-0
        /// 该装备已不在货架上
        /// </summary>
        public const int LGCommonTextContext21420 = 135160710;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30620-Context-215-2-0
        /// 请先修复铁匠铺
        /// </summary>
        public const int LGCommonTextContext21520 = 1763198753;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30622-Context-217-2-0
        /// 售价 {0} 金币
        /// </summary>
        public const int LGCommonTextContext21720 = 1294451729;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30623-Context-218-2-0
        /// 货架 {0} 件
        /// </summary>
        public const int LGCommonTextContext21820 = 68435707;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30624-Context-219-2-0
        /// 已装备：{0}
        /// </summary>
        public const int LGCommonTextContext21920 = 236858353;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30625-Context-220-2-0
        /// 暂无装备
        /// </summary>
        public const int LGCommonTextContext22020 = 1931432826;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30626-Context-221-2-0
        /// 英雄金币 {0}
        /// </summary>
        public const int LGCommonTextContext22120 = 1749335572;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30627-Context-222-2-0
        /// 首级武器 · 打造并出售给英雄
        /// </summary>
        public const int LGCommonTextContext22220 = 1879049094;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30628-Context-223-2-0
        /// 打造中…
        /// </summary>
        public const int LGCommonTextContext22320 = 565785068;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30631-Context-226-2-0
        /// 确认打造
        /// </summary>
        public const int LGCommonTextContext22620 = 1748053229;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30634-Context-229-2-0
        /// 金币不足，无法修复铁匠铺
        /// </summary>
        public const int LGCommonTextContext22920 = 903741819;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30635-Context-230-2-0
        /// 职业 {0}
        /// </summary>
        public const int LGCommonTextContext23020 = 1676029026;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30636-Context-231-2-0
        /// 选择
        /// </summary>
        public const int LGCommonTextContext23120 = 753164524;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30637-Context-232-2-0
        /// 选择配方后确认材料和数量
        /// </summary>
        public const int LGCommonTextContext23220 = 681438810;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30639-Context-234-2-0
        /// 出售并装备 · {0}金币
        /// </summary>
        public const int LGCommonTextContext23420 = 954835322;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30640-Context-235-2-0
        /// 铁匠收入 {0}金币
        /// </summary>
        public const int LGCommonTextContext23520 = 1481140925;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30641-Context-236-2-0
        /// 收取收入
        /// </summary>
        public const int LGCommonTextContext23620 = 1496674255;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900125-Context-237-2-0
        /// 石盾加伦
        /// </summary>
        public const int LGCommonTextContext23720 = 254138813;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900126-Context-238-2-0
        /// 血刃马克斯
        /// </summary>
        public const int LGCommonTextContext23820 = 18350873;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900127-Context-239-2-0
        /// 铁壁格伦
        /// </summary>
        public const int LGCommonTextContext23920 = 230564137;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900128-Context-240-2-0
        /// 铜肩奥恩
        /// </summary>
        public const int LGCommonTextContext24020 = 2060375884;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900129-Context-241-2-0
        /// 影刃凯
        /// </summary>
        public const int LGCommonTextContext24120 = 803886220;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900130-Context-242-2-0
        /// 窃影杰米
        /// </summary>
        public const int LGCommonTextContext24220 = 675522711;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900131-Context-243-2-0
        /// 快刀罗尔
        /// </summary>
        public const int LGCommonTextContext24320 = 1688564631;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900132-Context-244-2-0
        /// 猎峰维克
        /// </summary>
        public const int LGCommonTextContext24420 = 605778551;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900133-Context-245-2-0
        /// 快枪托比
        /// </summary>
        public const int LGCommonTextContext24520 = 1854099239;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900134-Context-246-2-0
        /// 锐射莱姆
        /// </summary>
        public const int LGCommonTextContext24620 = 726126407;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900135-Context-247-2-0
        /// 致命格雷
        /// </summary>
        public const int LGCommonTextContext24720 = 1320863591;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900136-Context-248-2-0
        /// 追猎沃夫
        /// </summary>
        public const int LGCommonTextContext24820 = 621457015;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900137-Context-249-2-0
        /// 烈焰艾拉
        /// </summary>
        public const int LGCommonTextContext24920 = 1462122647;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900138-Context-250-2-0
        /// 奥术科恩
        /// </summary>
        public const int LGCommonTextContext25020 = 369009368;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900139-Context-251-2-0
        /// 寒冰雷姆
        /// </summary>
        public const int LGCommonTextContext25120 = 1040194340;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900140-Context-252-2-0
        /// 剧毒玛拉
        /// </summary>
        public const int LGCommonTextContext25220 = 1052576421;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900141-Context-253-2-0
        /// 赐福伊莱亚
        /// </summary>
        public const int LGCommonTextContext25320 = 1027905945;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900142-Context-254-2-0
        /// 沐光莉亚
        /// </summary>
        public const int LGCommonTextContext25420 = 1029974105;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900143-Context-255-2-0
        /// 护阵科恩
        /// </summary>
        public const int LGCommonTextContext25520 = 864116197;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900144-Context-256-2-0
        /// 盾卫加雷斯
        /// </summary>
        public const int LGCommonTextContext25620 = 1553909493;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900145-Context-257-2-0
        /// 圣光骑士
        /// </summary>
        public const int LGCommonTextContext25720 = 437798561;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900146-Context-258-2-0
        /// 冰霜女巫
        /// </summary>
        public const int LGCommonTextContext25820 = 1686650897;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900147-Context-259-2-0
        /// 赏金枪客
        /// </summary>
        public const int LGCommonTextContext25920 = 1909618149;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900148-Context-260-2-0
        /// 光明使者
        /// </summary>
        public const int LGCommonTextContext26020 = 637615328;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900149-Context-261-2-0
        /// 精灵游侠
        /// </summary>
        public const int LGCommonTextContext26120 = 1572430556;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900150-Context-262-2-0
        /// 自然之主
        /// </summary>
        public const int LGCommonTextContext26220 = 751878603;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900151-Context-263-2-0
        /// 血蹄酋长
        /// </summary>
        public const int LGCommonTextContext26320 = 1128462807;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900152-Context-264-2-0
        /// 迅疾剑客
        /// </summary>
        public const int LGCommonTextContext26420 = 1421389083;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900153-Context-265-2-0
        /// 毁灭先知
        /// </summary>
        public const int LGCommonTextContext26520 = 1823761607;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900154-Context-266-2-0
        /// 逐日卡尔
        /// </summary>
        public const int LGCommonTextContext26620 = 726930163;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900155-Context-267-2-0
        /// 凝甲精灵
        /// </summary>
        public const int LGCommonTextContext26720 = 218863095;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900156-Context-268-2-0
        /// 强袭牧师
        /// </summary>
        public const int LGCommonTextContext26820 = 1510428547;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900157-Context-269-2-0
        /// 风暴酒仙
        /// </summary>
        public const int LGCommonTextContext26920 = 1590279463;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900158-Context-270-2-0
        /// 恶魔之猎
        /// </summary>
        public const int LGCommonTextContext27020 = 216210860;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900159-Context-271-2-0
        /// 复仇领主
        /// </summary>
        public const int LGCommonTextContext27120 = 2010162692;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900160-Context-272-2-0
        /// 月之祭祀
        /// </summary>
        public const int LGCommonTextContext27220 = 1456373085;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900161-Context-273-2-0
        /// 暗影萨满
        /// </summary>
        public const int LGCommonTextContext27320 = 854116837;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900162-Context-274-2-0
        /// 狂怒狱主
        /// </summary>
        public const int LGCommonTextContext27420 = 1485528209;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900163-Context-275-2-0
        /// 巫妖之王
        /// </summary>
        public const int LGCommonTextContext27520 = 2014217185;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900164-Context-276-2-0
        /// 不朽先知
        /// </summary>
        public const int LGCommonTextContext27620 = 281989757;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900165-Context-277-2-0
        /// 深渊海妖
        /// </summary>
        public const int LGCommonTextContext27720 = 853636501;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900166-Context-278-2-0
        /// 风行使者
        /// </summary>
        public const int LGCommonTextContext27820 = 605267945;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900167-Context-279-2-0
        /// 圣光主教
        /// </summary>
        public const int LGCommonTextContext27920 = 673288569;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900168-Context-280-2-0
        /// 剧毒刃拳
        /// </summary>
        public const int LGCommonTextContext28020 = 2118453476;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900169-Context-281-2-0
        /// 瘟疫毒法
        /// </summary>
        public const int LGCommonTextContext28120 = 1072264028;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900170-Context-282-2-0
        /// 凋零游侠
        /// </summary>
        public const int LGCommonTextContext28220 = 1875031831;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900171-Context-283-2-0
        /// 鬼魅猎手
        /// </summary>
        public const int LGCommonTextContext28320 = 217381351;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900172-Context-284-2-0
        /// 怒斩战士
        /// </summary>
        public const int LGCommonTextContext28420 = 961464655;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900173-Context-285-2-0
        /// 汲血之酋
        /// </summary>
        public const int LGCommonTextContext28520 = 483459079;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900174-Context-286-2-0
        /// 炎魔术士
        /// </summary>
        public const int LGCommonTextContext28620 = 1334181479;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900175-Context-287-2-0
        /// 冰霜术士
        /// </summary>
        public const int LGCommonTextContext28720 = 1928316919;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900176-Context-288-2-0
        /// 圣光使者
        /// </summary>
        public const int LGCommonTextContext28820 = 2044784703;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900177-Context-289-2-0
        /// 神圣主教
        /// </summary>
        public const int LGCommonTextContext28920 = 47711831;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900178-Context-290-2-0
        /// 淬毒猎手
        /// </summary>
        public const int LGCommonTextContext29020 = 1910140872;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900179-Context-291-2-0
        /// 烈焰猎手
        /// </summary>
        public const int LGCommonTextContext29120 = 1782023652;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900180-Context-292-2-0
        /// 暗夜刺客
        /// </summary>
        public const int LGCommonTextContext29220 = 325818661;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-900181-Context-293-2-0
        /// 斩杀之刺
        /// </summary>
        public const int LGCommonTextContext29320 = 1100901897;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30324-Context-294-2-0
        /// 1级交易行
        /// </summary>
        public const int LGCommonTextContext29420 = 399152721;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30325-Context-295-2-0
        /// 待售材料
        /// </summary>
        public const int LGCommonTextContext29520 = 824326677;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30326-Context-296-2-0
        /// 材料拍卖
        /// </summary>
        public const int LGCommonTextContext29620 = 801182233;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30327-Context-297-2-0
        /// 装备拍卖
        /// </summary>
        public const int LGCommonTextContext29720 = 68317317;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30328-Context-298-2-0
        /// 交易日报尚未接入
        /// </summary>
        public const int LGCommonTextContext29820 = 17176753;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-30329-Context-299-2-0
        /// 10级
        /// </summary>
        public const int LGCommonTextContext29920 = 1870481997;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-16056002-Context-300-2-0
        /// 兄弟雷恩
        /// </summary>
        public const int LGCommonTextContext30020 = 1924668279;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-16056003-Context-301-2-0
        /// 我这副样子，暂时上不了前线了。但我们还没输。
        /// </summary>
        public const int LGCommonTextContext30120 = 2037062223;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-16056005-Context-303-2-0
        /// 拿着狮鹫哨去找管理员。附近的骑士听到急召，会知道斯坦恩村还没倒下。
        /// </summary>
        public const int LGCommonTextContext30320 = 1012756191;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-16058002-Context-304-2-0
        /// 狮鹫管理员
        /// </summary>
        public const int LGCommonTextContext30420 = 1607958897;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-16058003-Context-305-2-0
        ///  这是骑士团的旧式狮鹫哨。还能响，但上面的传令纹已经裂了，急召不了几次。
        /// </summary>
        public const int LGCommonTextContext30520 = 2031576769;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-16060002-Context-306-2-0
        /// 快枪托比（猎人）
        /// </summary>
        public const int LGCommonTextContext30620 = 2054962714;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-16060003-Context-307-2-0
        /// 急召我收到了。霍尔山的兽群，我追了它们一路。给我一条山道，我就能把路打通。
        /// </summary>
        public const int LGCommonTextContext30720 = 1517503202;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-16062002-Context-308-2-0
        /// 影刃凯（盗贼）
        /// </summary>
        public const int LGCommonTextContext30820 = 1797225798;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-16062003-Context-309-2-0
        /// 这哨声真刺耳……不过，我听见了。说吧，谁需要从黑暗里消失？
        /// </summary>
        public const int LGCommonTextContext30920 = 1515445974;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308001-Context-312-2-0
        /// 霍尔山
        /// </summary>
        public const int LGCommonTextContext31220 = 1378688148;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308002-Context-313-2-0
        /// 1.山脚
        /// </summary>
        public const int LGCommonTextContext31320 = 1897929878;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308003-Context-314-2-0
        /// 霍尔山截断了这片平原的去路，进出只有一条残破旧道，断桥与废弃哨站散落沿途。草场上跑着野猪与草原狼，山腰以上是豺狼人的地盘。斯坦恩村是本区仅存的人类聚落。
        /// </summary>
        public const int LGCommonTextContext31420 = 491374520;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308004-Context-315-2-0
        /// 集结
        /// </summary>
        public const int LGCommonTextContext31520 = 1684979190;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308005-Context-316-2-0
        /// 首战入口已解锁
        /// </summary>
        public const int LGCommonTextContext31620 = 1577384012;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308006-Context-317-2-0
        /// 请先招募英雄
        /// </summary>
        public const int LGCommonTextContext31720 = 1950126390;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308008-Context-319-2-0
        /// 推荐战力 {0}
        /// </summary>
        public const int LGCommonTextContext31920 = 925279974;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308009-Context-320-2-0
        /// 已选择霍尔山首关，战斗执行将在后续接入
        /// </summary>
        public const int LGCommonTextContext32020 = 155771181;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308010-Context-321-2-0
        /// 对话
        /// </summary>
        public const int LGCommonTextContext32120 = 462492704;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308011-Context-322-2-0
        /// 招募
        /// </summary>
        public const int LGCommonTextContext32220 = 2127142754;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308012-Context-323-2-0
        /// 吹响哨子
        /// </summary>
        public const int LGCommonTextContext32320 = 1142273964;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308013-Context-324-2-0
        /// 需要残破的狮鹫哨
        /// </summary>
        public const int LGCommonTextContext32420 = 1113223554;
        /// <summary>
        /// LG-CommonTextConfig-CommonText-308014-Context-325-2-0
        /// 点击继续
        /// </summary>
        public const int LGCommonTextContext32520 = 833830240;
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
        /// LG-TaskConfig-Task-10001-Name-3-2-0
        /// 登陆
        /// </summary>
        public const int LGTaskName320 = 596929255;
        /// <summary>
        /// LG-TaskConfig-Task-10001-Description-3-3-0
        /// 登陆1次
        /// </summary>
        public const int LGTaskDescription330 = 691747469;
        /// <summary>
        /// LG-TaskConfig-Task-10002-Name-4-2-0
        /// 建造旅店
        /// </summary>
        public const int LGTaskName420 = 2072927009;
        /// <summary>
        /// LG-TaskConfig-Task-10003-Name-5-2-0
        /// 生产10份炸鱼
        /// </summary>
        public const int LGTaskName520 = 1875094555;
        /// <summary>
        /// LG-TaskConfig-Task-10004-Name-6-2-0
        /// 建造医院
        /// </summary>
        public const int LGTaskName620 = 1205919873;
        /// <summary>
        /// LG-TaskConfig-Task-10005-Name-7-2-0
        /// 生产10瓶初级治疗药水
        /// </summary>
        public const int LGTaskName720 = 793125871;
        /// <summary>
        /// LG-TaskConfig-Task-10006-Name-8-2-0
        /// 派遣英雄艾文森林
        /// </summary>
        public const int LGTaskName820 = 373104753;
        /// <summary>
        /// LG-TaskConfig-Task-10006-Description-8-3-0
        /// 派遣英雄去艾文森林练级
        /// </summary>
        public const int LGTaskDescription830 = 1085666905;
        /// <summary>
        /// LG-TaskConfig-Task-10007-Name-9-2-0
        /// 英雄升到2级
        /// </summary>
        public const int LGTaskName920 = 1879497659;
        /// <summary>
        /// LG-TaskConfig-Task-10007-Description-9-3-0
        /// 拥有一个2级英雄
        /// </summary>
        public const int LGTaskDescription930 = 1678756209;
        /// <summary>
        /// LG-TaskConfig-Task-10008-Name-10-2-0
        /// 上阵四个英雄
        /// </summary>
        public const int LGTaskName1020 = 629030276;
        /// <summary>
        /// LG-TaskConfig-Task-10009-Name-11-2-0
        /// 通关地下城1-1
        /// </summary>
        public const int LGTaskName1120 = 1517615300;
        /// <summary>
        /// LG-TaskConfig-Task-10010-Name-12-2-0
        /// 建造铁匠铺
        /// </summary>
        public const int LGTaskName1220 = 112235031;
        /// <summary>
        /// LG-TaskConfig-Task-10011-Name-13-2-0
        /// 打造武器
        /// </summary>
        public const int LGTaskName1320 = 1724452103;
        /// <summary>
        /// LG-TaskConfig-Task-10011-Description-13-3-0
        /// 打造武器1次
        /// </summary>
        public const int LGTaskDescription1330 = 2113607117;
        /// <summary>
        /// LG-TaskConfig-Task-10012-Name-14-2-0
        /// 通关地下城1-2
        /// </summary>
        public const int LGTaskName1420 = 1431851187;
        /// <summary>
        /// LG-TaskConfig-Task-10013-Name-15-2-0
        /// 招募英雄
        /// </summary>
        public const int LGTaskName1520 = 398310643;
        /// <summary>
        /// LG-TaskConfig-Task-10013-Description-15-3-0
        /// 抽取英雄1次
        /// </summary>
        public const int LGTaskDescription1530 = 1512200337;
        /// <summary>
        /// LG-TaskConfig-Task-10014-Name-16-2-0
        /// 上阵五个英雄
        /// </summary>
        public const int LGTaskName1620 = 1535064279;
        /// <summary>
        /// LG-TaskConfig-Task-10016-Name-18-2-0
        /// 旅店升到10级
        /// </summary>
        public const int LGTaskName1820 = 1225786347;
        /// <summary>
        /// LG-TaskConfig-Task-10017-Name-19-2-0
        /// 接取订单任务
        /// </summary>
        public const int LGTaskName1920 = 388797339;
        /// <summary>
        /// LG-TaskConfig-Task-10017-Description-19-3-0
        /// 接取订单任务1次
        /// </summary>
        public const int LGTaskDescription1930 = 1066808681;
        /// <summary>
        /// LG-TaskConfig-Task-10018-Name-20-2-0
        /// 集结1
        /// </summary>
        public const int LGTaskName2020 = 1782048074;
        /// <summary>
        /// LG-TaskConfig-Task-10018-Description-20-3-0
        /// 集结去音山矿洞
        /// </summary>
        public const int LGTaskDescription2030 = 204697122;
        /// <summary>
        /// LG-TaskConfig-Task-10019-Name-21-2-0
        /// 领取订单任务奖励
        /// </summary>
        public const int LGTaskName2120 = 627950986;
        /// <summary>
        /// LG-TaskConfig-Task-10019-Description-21-3-0
        /// 领取一次订单任务奖励
        /// </summary>
        public const int LGTaskDescription2130 = 147725208;
        /// <summary>
        /// LG-TaskConfig-Task-10020-Name-22-2-0
        /// 上阵六个英雄
        /// </summary>
        public const int LGTaskName2220 = 711580739;
        /// <summary>
        /// LG-TaskConfig-Task-10022-Name-24-2-0
        /// 解锁裁缝铺
        /// </summary>
        public const int LGTaskName2420 = 1497295211;
        /// <summary>
        /// LG-TaskConfig-Task-10023-Name-25-2-0
        /// 打造防具
        /// </summary>
        public const int LGTaskName2520 = 1353408883;
        /// <summary>
        /// LG-TaskConfig-Task-10024-Name-26-2-0
        /// 解锁炼金商店
        /// </summary>
        public const int LGTaskName2620 = 112328803;
        /// <summary>
        /// LG-TaskConfig-Task-10025-Name-27-2-0
        /// 打造饰品
        /// </summary>
        public const int LGTaskName2720 = 30903843;
        /// <summary>
        /// LG-TaskConfig-Task-10026-Name-28-2-0
        /// 医院升到10级
        /// </summary>
        public const int LGTaskName2820 = 1272180635;
        /// <summary>
        /// LG-TaskConfig-Task-10027-Name-29-2-0
        /// 通关地下城1-4
        /// </summary>
        public const int LGTaskName2920 = 1904801411;
        /// <summary>
        /// LG-TaskConfig-Task-10029-Name-31-2-0
        /// 公会等级升到2级
        /// </summary>
        public const int LGTaskName3120 = 1354380496;
        /// <summary>
        /// LG-TaskConfig-Task-10030-Name-32-2-0
        /// 旅店升到20级
        /// </summary>
        public const int LGTaskName3220 = 1660374871;
        /// <summary>
        /// LG-TaskConfig-Task-10032-Name-34-2-0
        /// 医院升到20级
        /// </summary>
        public const int LGTaskName3420 = 1099237251;
        /// <summary>
        /// LG-TaskConfig-Task-10034-Name-36-2-0
        /// 升级铁匠铺到2级
        /// </summary>
        public const int LGTaskName3620 = 980970759;
        /// <summary>
        /// LG-TaskConfig-Task-10035-Name-37-2-0
        /// 打造绿色武器
        /// </summary>
        public const int LGTaskName3720 = 750443499;
        /// <summary>
        /// LG-TaskConfig-Task-10036-Name-38-2-0
        /// 升级裁缝铺到2级
        /// </summary>
        public const int LGTaskName3820 = 1619097163;
        /// <summary>
        /// LG-TaskConfig-Task-10037-Name-39-2-0
        /// 打造绿色防具
        /// </summary>
        public const int LGTaskName3920 = 231819135;
        /// <summary>
        /// LG-TaskConfig-Task-10037-Description-39-3-0
        /// 打造绿色防具1次
        /// </summary>
        public const int LGTaskDescription3930 = 299976569;
        /// <summary>
        /// LG-TaskConfig-Task-10038-Name-40-2-0
        /// 升级炼金商店到2级
        /// </summary>
        public const int LGTaskName4020 = 1783565854;
        /// <summary>
        /// LG-TaskConfig-Task-10039-Name-41-2-0
        /// 打造绿色饰品
        /// </summary>
        public const int LGTaskName4120 = 1318063970;
        /// <summary>
        /// LG-TaskConfig-Task-10039-Description-41-3-0
        /// 打造绿色饰品1次
        /// </summary>
        public const int LGTaskDescription4130 = 2054256324;
        /// <summary>
        /// LG-TaskConfig-Task-10040-Name-42-2-0
        /// 灵魂圣所升到2级
        /// </summary>
        public const int LGTaskName4220 = 1510980587;
        /// <summary>
        /// LG-TaskConfig-Task-10041-Name-43-2-0
        /// 5名英雄升到15级
        /// </summary>
        public const int LGTaskName4320 = 1570933511;
        /// <summary>
        /// LG-TaskConfig-Task-10041-Description-43-3-0
        /// 拥有5个15级英雄
        /// </summary>
        public const int LGTaskDescription4330 = 1607745799;
        /// <summary>
        /// LG-TaskConfig-Task-10042-Name-44-2-0
        /// 通关地下城1-10
        /// </summary>
        public const int LGTaskName4420 = 1232627619;
        /// <summary>
        /// LG-TaskConfig-Task-10043-Description-45-3-0
        /// 招募英雄5次
        /// </summary>
        public const int LGTaskDescription4530 = 19569283;
        /// <summary>
        /// LG-TaskConfig-Task-10044-Name-46-2-0
        /// 派遣英雄去第二张地图
        /// </summary>
        public const int LGTaskName4620 = 33377219;
        /// <summary>
        /// LG-TaskConfig-Task-10044-Description-46-3-0
        /// 派遣英雄去西部荒原探索
        /// </summary>
        public const int LGTaskDescription4630 = 1442893625;
        /// <summary>
        /// LG-TaskConfig-Task-10045-Name-47-2-0
        /// 公会等级升到3级
        /// </summary>
        public const int LGTaskName4720 = 472041447;
        /// <summary>
        /// LG-TaskConfig-Task-10046-Name-48-2-0
        /// 旅店升到30级
        /// </summary>
        public const int LGTaskName4820 = 861574235;
        /// <summary>
        /// LG-TaskConfig-Task-10047-Name-49-2-0
        /// 生产10份夜鳞鱼汤
        /// </summary>
        public const int LGTaskName4920 = 683905631;
        /// <summary>
        /// LG-TaskConfig-Task-10048-Name-50-2-0
        /// 医院升到30级
        /// </summary>
        public const int LGTaskName5020 = 1275202124;
        /// <summary>
        /// LG-TaskConfig-Task-10049-Name-51-2-0
        /// 生产10瓶次级治疗药水
        /// </summary>
        public const int LGTaskName5120 = 599265588;
        /// <summary>
        /// LG-TaskConfig-Task-10050-Name-52-2-0
        /// 升级铁匠铺到3级
        /// </summary>
        public const int LGTaskName5220 = 1545821287;
        /// <summary>
        /// LG-TaskConfig-Task-10051-Name-53-2-0
        /// 打造二阶武器
        /// </summary>
        public const int LGTaskName5320 = 1275094167;
        /// <summary>
        /// LG-TaskConfig-Task-10052-Name-54-2-0
        /// 升级裁缝铺到3级
        /// </summary>
        public const int LGTaskName5420 = 830071091;
        /// <summary>
        /// LG-TaskConfig-Task-10053-Name-55-2-0
        /// 打造二阶防具
        /// </summary>
        public const int LGTaskName5520 = 174520683;
        /// <summary>
        /// LG-TaskConfig-Task-10054-Name-56-2-0
        /// 领取一次挂机奖励
        /// </summary>
        public const int LGTaskName5620 = 1450664183;
        /// <summary>
        /// LG-TaskConfig-Task-10055-Name-57-2-0
        /// 升级炼金商店到3级
        /// </summary>
        public const int LGTaskName5720 = 739034055;
        /// <summary>
        /// LG-TaskConfig-Task-10056-Name-58-2-0
        /// 打造二阶饰品
        /// </summary>
        public const int LGTaskName5820 = 1043712859;
        /// <summary>
        /// LG-TaskConfig-Task-10057-Name-59-2-0
        /// 通关地下城2-5
        /// </summary>
        public const int LGTaskName5920 = 35954435;
        /// <summary>
        /// LG-TaskConfig-Task-10058-Name-60-2-0
        /// 灵魂圣所升到3级
        /// </summary>
        public const int LGTaskName6020 = 666375626;
        /// <summary>
        /// LG-TaskConfig-Task-10059-Name-61-2-0
        /// 集结2
        /// </summary>
        public const int LGTaskName6120 = 996426410;
        /// <summary>
        /// LG-TaskConfig-Task-10059-Description-61-3-0
        /// 集结去法戈地矿洞
        /// </summary>
        public const int LGTaskDescription6130 = 1649792296;
        /// <summary>
        /// LG-TaskConfig-Task-10060-Name-62-2-0
        /// 公会等级升到4级
        /// </summary>
        public const int LGTaskName6220 = 2120639595;
        /// <summary>
        /// LG-TaskConfig-Task-10061-Name-63-2-0
        /// 旅店升到40级
        /// </summary>
        public const int LGTaskName6320 = 727792683;
        /// <summary>
        /// LG-TaskConfig-Task-10063-Name-65-2-0
        /// 药店升到40级
        /// </summary>
        public const int LGTaskName6520 = 408387147;
        /// <summary>
        /// LG-TaskConfig-Task-10063-Description-65-3-0
        /// 医院升到40级
        /// </summary>
        public const int LGTaskDescription6530 = 1749708463;
        /// <summary>
        /// LG-TaskConfig-Task-10065-Name-67-2-0
        /// 升级铁匠铺到4级
        /// </summary>
        public const int LGTaskName6720 = 1751003;
        /// <summary>
        /// LG-TaskConfig-Task-10066-Name-68-2-0
        /// 打造二阶绿色武器
        /// </summary>
        public const int LGTaskName6820 = 1480197787;
        /// <summary>
        /// LG-TaskConfig-Task-10068-Name-70-2-0
        /// 升级裁缝铺到4级
        /// </summary>
        public const int LGTaskName7020 = 372969804;
        /// <summary>
        /// LG-TaskConfig-Task-10069-Name-71-2-0
        /// 打造二阶绿色防具
        /// </summary>
        public const int LGTaskName7120 = 306465240;
        /// <summary>
        /// LG-TaskConfig-Task-10072-Name-74-2-0
        /// 升级炼金商店到4级
        /// </summary>
        public const int LGTaskName7420 = 1822024275;
        /// <summary>
        /// LG-TaskConfig-Task-10073-Name-75-2-0
        /// 打造二阶绿色饰品
        /// </summary>
        public const int LGTaskName7520 = 582289551;
        /// <summary>
        /// LG-TaskConfig-Task-10074-Name-76-2-0
        /// 通关地下城2-10
        /// </summary>
        public const int LGTaskName7620 = 1169089527;
        /// <summary>
        /// LG-TaskConfig-Task-10078-Name-80-2-0
        /// 通关地下城2-15
        /// </summary>
        public const int LGTaskName8020 = 939387238;
        /// <summary>
        /// LG-TaskConfig-Task-10079-Name-81-2-0
        /// 公会等级升到5级
        /// </summary>
        public const int LGTaskName8120 = 296646578;
        /// <summary>
        /// LG-TaskConfig-Task-10081-Name-83-2-0
        /// 升级铁匠铺到5级
        /// </summary>
        public const int LGTaskName8320 = 596971751;
        /// <summary>
        /// LG-TaskConfig-Task-10082-Name-84-2-0
        /// 打造二阶蓝色武器
        /// </summary>
        public const int LGTaskName8420 = 160480851;
        /// <summary>
        /// LG-TaskConfig-Task-10084-Name-86-2-0
        /// 旅店升到50级
        /// </summary>
        public const int LGTaskName8620 = 1223154795;
        /// <summary>
        /// LG-TaskConfig-Task-10086-Name-88-2-0
        /// 药店升到50级
        /// </summary>
        public const int LGTaskName8820 = 1006414603;
        /// <summary>
        /// LG-TaskConfig-Task-10088-Name-90-2-0
        /// 升级裁缝铺到5级
        /// </summary>
        public const int LGTaskName9020 = 1736148580;
        /// <summary>
        /// LG-TaskConfig-Task-10089-Name-91-2-0
        /// 打造二阶蓝色防具
        /// </summary>
        public const int LGTaskName9120 = 1138418148;
        /// <summary>
        /// LG-TaskConfig-Task-10091-Name-93-2-0
        /// 升级炼金商店到5级
        /// </summary>
        public const int LGTaskName9320 = 9075847;
        /// <summary>
        /// LG-TaskConfig-Task-10092-Name-94-2-0
        /// 打造二阶蓝色饰品
        /// </summary>
        public const int LGTaskName9420 = 1149719443;
        /// <summary>
        /// LG-TaskConfig-Task-10097-Name-99-2-0
        /// 通关地下城2-30
        /// </summary>
        public const int LGTaskName9920 = 2115812251;
        /// <summary>
        /// LG-TaskConfig-Task-10097-Description-99-3-0
        /// 通关地下城2-20
        /// </summary>
        public const int LGTaskDescription9930 = 1833246505;
        /// <summary>
        /// LG-TaskConfig-Task-10099-Name-101-2-0
        /// 派遣英雄去第三张地图
        /// </summary>
        public const int LGTaskName10120 = 1058406565;
        /// <summary>
        /// LG-TaskConfig-Task-10099-Description-101-3-0
        /// 派遣英雄去贫瘠之地
        /// </summary>
        public const int LGTaskDescription10130 = 1270456141;
        /// <summary>
        /// LG-TaskConfig-Task-10100-Name-102-2-0
        /// 通关地下城3-4
        /// </summary>
        public const int LGTaskName10220 = 1568378375;
        /// <summary>
        /// LG-TaskConfig-Task-10101-Name-103-2-0
        /// 集结6
        /// </summary>
        public const int LGTaskName10320 = 148189801;
        /// <summary>
        /// LG-TaskConfig-Task-10101-Description-103-3-0
        /// 集结玉石矿洞1次
        /// </summary>
        public const int LGTaskDescription10330 = 1825642349;
        /// <summary>
        /// LG-TaskConfig-Task-10102-Name-104-2-0
        /// 公会等级升到6级
        /// </summary>
        public const int LGTaskName10420 = 157899051;
        /// <summary>
        /// LG-TaskConfig-Task-10103-Name-105-2-0
        /// 旅店升到60级
        /// </summary>
        public const int LGTaskName10520 = 1166829001;
        /// <summary>
        /// LG-TaskConfig-Task-10104-Name-106-2-0
        /// 生产10份腌制鲑鱼
        /// </summary>
        public const int LGTaskName10620 = 1192269007;
        /// <summary>
        /// LG-TaskConfig-Task-10105-Name-107-2-0
        /// 药店升到60级
        /// </summary>
        public const int LGTaskName10720 = 84846969;
        /// <summary>
        /// LG-TaskConfig-Task-10106-Name-108-2-0
        /// 生产10瓶治疗药水
        /// </summary>
        public const int LGTaskName10820 = 1198094315;
        /// <summary>
        /// LG-TaskConfig-Task-10107-Name-109-2-0
        /// 升级铁匠铺到6级
        /// </summary>
        public const int LGTaskName10920 = 66091625;
        /// <summary>
        /// LG-TaskConfig-Task-10108-Name-110-2-0
        /// 打造二阶紫色武器
        /// </summary>
        public const int LGTaskName11020 = 87509880;
        /// <summary>
        /// LG-TaskConfig-Task-10110-Name-112-2-0
        /// 升级裁缝铺到6级
        /// </summary>
        public const int LGTaskName11220 = 1497085569;
        /// <summary>
        /// LG-TaskConfig-Task-10111-Name-113-2-0
        /// 打造二阶紫色防具
        /// </summary>
        public const int LGTaskName11320 = 1515249795;
        /// <summary>
        /// LG-TaskConfig-Task-10113-Name-115-2-0
        /// 升级炼金商店到6级
        /// </summary>
        public const int LGTaskName11520 = 516025403;
        /// <summary>
        /// LG-TaskConfig-Task-10114-Name-116-2-0
        /// 打造二阶紫色饰品
        /// </summary>
        public const int LGTaskName11620 = 1818657737;
        /// <summary>
        /// LG-TaskConfig-Task-10115-Name-117-2-0
        /// 灵魂圣所升到5级
        /// </summary>
        public const int LGTaskName11720 = 880978059;
        /// <summary>
        /// LG-TaskConfig-Task-10116-Name-118-2-0
        /// 通关地下城3-12
        /// </summary>
        public const int LGTaskName11820 = 438758941;
        /// <summary>
        /// LG-TaskConfig-Task-10118-Name-120-2-0
        /// 通关地下城3-16
        /// </summary>
        public const int LGTaskName12020 = 306561676;
        /// <summary>
        /// LG-TaskConfig-Task-10120-Name-122-2-0
        /// 通关地下城3-20
        /// </summary>
        public const int LGTaskName12220 = 628307023;
        /// <summary>
        /// LG-TaskConfig-Task-10122-Name-124-2-0
        /// 通关地下城3-25
        /// </summary>
        public const int LGTaskName12420 = 1502052083;
        /// <summary>
        /// LG-TaskConfig-Task-10123-Name-125-2-0
        /// 通关地下城3-30
        /// </summary>
        public const int LGTaskName12520 = 2066301985;
        /// <summary>
        /// LG-TaskConfig-Task-10124-Name-126-2-0
        /// 通关地下城4-5
        /// </summary>
        public const int LGTaskName12620 = 1265926791;
        /// <summary>
        /// LG-TaskConfig-Task-10125-Name-127-2-0
        /// 通关地下城4-10
        /// </summary>
        public const int LGTaskName12720 = 164030577;
        /// <summary>
        /// LG-TaskConfig-Task-10126-Name-128-2-0
        /// 通关地下城4-15
        /// </summary>
        public const int LGTaskName12820 = 1025842227;
        /// <summary>
        /// LG-TaskConfig-Task-10127-Name-129-2-0
        /// 通关地下城4-20
        /// </summary>
        public const int LGTaskName12920 = 1655342993;
        /// <summary>
        /// LG-TaskConfig-Task-10128-Name-130-2-0
        /// 通关地下城4-25
        /// </summary>
        public const int LGTaskName13020 = 1881726864;
        /// <summary>
        /// LG-TaskConfig-Task-10129-Name-131-2-0
        /// 通关地下城4-30
        /// </summary>
        public const int LGTaskName13120 = 2053040746;
        /// <summary>
        /// LG-TaskConfig-Task-10130-Name-132-2-0
        /// 通关地下城5-5
        /// </summary>
        public const int LGTaskName13220 = 790885109;
        /// <summary>
        /// LG-TaskConfig-Task-10131-Name-133-2-0
        /// 通关地下城5-10
        /// </summary>
        public const int LGTaskName13320 = 5626055;
        /// <summary>
        /// LG-TaskConfig-Task-10132-Name-134-2-0
        /// 通关地下城5-15
        /// </summary>
        public const int LGTaskName13420 = 699177817;
        /// <summary>
        /// LG-TaskConfig-Task-10133-Name-135-2-0
        /// 通关地下城5-20
        /// </summary>
        public const int LGTaskName13520 = 1222917783;
        /// <summary>
        /// LG-TaskConfig-Task-10134-Name-136-2-0
        /// 通关地下城5-25
        /// </summary>
        public const int LGTaskName13620 = 1048996269;
        /// <summary>
        /// LG-TaskConfig-Task-10135-Name-137-2-0
        /// 通关地下城5-30
        /// </summary>
        public const int LGTaskName13720 = 2037458703;
        /// <summary>
        /// LG-TaskConfig-Task-40001-Name-138-2-0
        /// 藏品探索
        /// </summary>
        public const int LGTaskName13820 = 1336324659;
        /// <summary>
        /// LG-TaskConfig-Task-41001-Name-163-2-0
        /// 宠物捕捉
        /// </summary>
        public const int LGTaskName16320 = 875578312;
        /// <summary>
        /// LG-TaskConfig-Task-42001-Name-188-2-0
        /// 英雄招募中消耗钻石
        /// </summary>
        public const int LGTaskName18820 = 1431306934;
        /// <summary>
        /// LG-TaskConfig-Task-43001-Name-196-2-0
        /// 专精书抄录
        /// </summary>
        public const int LGTaskName19620 = 897850926;
        /// <summary>
        /// LG-TaskConfig-Task-43001-Description-196-3-0
        /// 专精书抄录1次
        /// </summary>
        public const int LGTaskDescription19630 = 2043792504;
        /// <summary>
        /// LG-TaskConfig-Task-43002-Description-197-3-0
        /// 专精书抄录2次
        /// </summary>
        public const int LGTaskDescription19730 = 1020237398;
        /// <summary>
        /// LG-TaskConfig-Task-43003-Description-198-3-0
        /// 专精书抄录3次
        /// </summary>
        public const int LGTaskDescription19830 = 708213488;
        /// <summary>
        /// LG-TaskConfig-Task-43004-Description-199-3-0
        /// 专精书抄录4次
        /// </summary>
        public const int LGTaskDescription19930 = 1237855746;
        /// <summary>
        /// LG-TaskConfig-Task-43005-Description-200-3-0
        /// 专精书抄录5次
        /// </summary>
        public const int LGTaskDescription20030 = 736646434;
        /// <summary>
        /// LG-TaskConfig-Task-43006-Description-201-3-0
        /// 专精书抄录6次
        /// </summary>
        public const int LGTaskDescription20130 = 1051555980;
        /// <summary>
        /// LG-TaskConfig-Task-43007-Description-202-3-0
        /// 专精书抄录7次
        /// </summary>
        public const int LGTaskDescription20230 = 1288891170;
        /// <summary>
        /// LG-TaskConfig-Task-43008-Description-203-3-0
        /// 专精书抄录8次
        /// </summary>
        public const int LGTaskDescription20330 = 390006152;
        /// <summary>
        /// LG-TaskConfig-Task-43009-Description-204-3-0
        /// 专精书抄录9次
        /// </summary>
        public const int LGTaskDescription20430 = 1132947442;
        /// <summary>
        /// LG-TaskConfig-Task-43010-Description-205-3-0
        /// 专精书抄录10次
        /// </summary>
        public const int LGTaskDescription20530 = 280826503;
        /// <summary>
        /// LG-TaskConfig-Task-43011-Description-206-3-0
        /// 专精书抄录11次
        /// </summary>
        public const int LGTaskDescription20630 = 1975834765;
        /// <summary>
        /// LG-TaskConfig-Task-43012-Description-207-3-0
        /// 专精书抄录12次
        /// </summary>
        public const int LGTaskDescription20730 = 2146871607;
        /// <summary>
        /// LG-TaskConfig-Task-43013-Description-208-3-0
        /// 专精书抄录13次
        /// </summary>
        public const int LGTaskDescription20830 = 1652438577;
        /// <summary>
        /// LG-TaskConfig-Task-43014-Description-209-3-0
        /// 专精书抄录14次
        /// </summary>
        public const int LGTaskDescription20930 = 573112567;
        /// <summary>
        /// LG-TaskConfig-Task-43015-Description-210-3-0
        /// 专精书抄录15次
        /// </summary>
        public const int LGTaskDescription21030 = 1774931550;
        /// <summary>
        /// LG-TaskConfig-Task-43016-Description-211-3-0
        /// 专精书抄录16次
        /// </summary>
        public const int LGTaskDescription21130 = 1899503536;
        /// <summary>
        /// LG-TaskConfig-Task-43017-Description-212-3-0
        /// 专精书抄录17次
        /// </summary>
        public const int LGTaskDescription21230 = 1196216874;
        /// <summary>
        /// LG-TaskConfig-Task-43018-Description-213-3-0
        /// 专精书抄录18次
        /// </summary>
        public const int LGTaskDescription21330 = 124550672;
        /// <summary>
        /// LG-TaskConfig-Task-43019-Description-214-3-0
        /// 专精书抄录19次
        /// </summary>
        public const int LGTaskDescription21430 = 562992054;
        /// <summary>
        /// LG-TaskConfig-Task-43020-Description-215-3-0
        /// 专精书抄录20次
        /// </summary>
        public const int LGTaskDescription21530 = 1708276685;
        /// <summary>
        /// LG-TaskConfig-Task-43021-Description-216-3-0
        /// 专精书抄录21次
        /// </summary>
        public const int LGTaskDescription21630 = 1818927635;
        /// <summary>
        /// LG-TaskConfig-Task-43022-Description-217-3-0
        /// 专精书抄录22次
        /// </summary>
        public const int LGTaskDescription21730 = 996294113;
        /// <summary>
        /// LG-TaskConfig-Task-43023-Description-218-3-0
        /// 专精书抄录23次
        /// </summary>
        public const int LGTaskDescription21830 = 399947883;
        /// <summary>
        /// LG-TaskConfig-Task-43024-Description-219-3-0
        /// 专精书抄录24次
        /// </summary>
        public const int LGTaskDescription21930 = 324725717;
        /// <summary>
        /// LG-TaskConfig-Task-43025-Description-220-3-0
        /// 专精书抄录25次
        /// </summary>
        public const int LGTaskDescription22030 = 1086352022;
        /// <summary>
        /// LG-TaskConfig-Task-20001-Name-221-2-0
        /// 酒馆达到1级
        /// </summary>
        public const int LGTaskName22120 = 2052342474;
        /// <summary>
        /// LG-TaskConfig-Task-20002-Name-222-2-0
        /// 酒馆达到2级
        /// </summary>
        public const int LGTaskName22220 = 32982896;
        /// <summary>
        /// LG-TaskConfig-Task-20003-Name-223-2-0
        /// 酒馆达到3级
        /// </summary>
        public const int LGTaskName22320 = 864233958;
        /// <summary>
        /// LG-TaskConfig-Task-20004-Name-224-2-0
        /// 酒馆达到4级
        /// </summary>
        public const int LGTaskName22420 = 123623856;
        /// <summary>
        /// LG-TaskConfig-Task-20005-Name-225-2-0
        /// 酒馆达到5级
        /// </summary>
        public const int LGTaskName22520 = 868905250;
        /// <summary>
        /// LG-TaskConfig-Task-50001-Name-231-2-0
        /// 采购物资
        /// </summary>
        public const int LGTaskName23120 = 956349188;
        /// <summary>
        /// LG-TaskConfig-Task-50001-Description-231-3-0
        /// 征战在即，前往采购充足物资，为战斗做好万全准备。
        /// </summary>
        public const int LGTaskDescription23130 = 1474668438;
        /// <summary>
        /// LG-TaskConfig-Task-50002-Name-232-2-0
        /// 英雄上阵
        /// </summary>
        public const int LGTaskName23220 = 1757114050;
        /// <summary>
        /// LG-TaskConfig-Task-50002-Description-232-3-0
        /// 村庄逐渐繁荣，可入驻更多英雄了。
        /// </summary>
        public const int LGTaskDescription23230 = 122001650;
        /// <summary>
        /// LG-TaskConfig-Task-50003-Name-233-2-0
        /// 3星挑战1
        /// </summary>
        public const int LGTaskName23320 = 1125448560;
        /// <summary>
        /// LG-TaskConfig-Task-50003-Description-233-3-0
        /// 霍尔山山脚最近魔物泛滥，村民不得安宁，你快去把这些怪物清理干净。
        /// </summary>
        public const int LGTaskDescription23330 = 656762538;
        /// <summary>
        /// LG-TaskConfig-Task-50004-Name-234-2-0
        /// 地下城1
        /// </summary>
        public const int LGTaskName23420 = 1313173914;
        /// <summary>
        /// LG-TaskConfig-Task-50004-Description-234-3-0
        /// 地下城前路已开，勇敢打通 1 至 5 层，证明你的基础实力。
        /// </summary>
        public const int LGTaskDescription23430 = 450406006;
        /// <summary>
        /// LG-TaskConfig-Task-50005-Name-235-2-0
        /// 3星挑战2
        /// </summary>
        public const int LGTaskName23520 = 448266276;
        /// <summary>
        /// LG-TaskConfig-Task-50005-Description-235-3-0
        /// 霍尔山中段不断有怪物出没，附近营地告急，你赶紧过去平定这场骚乱。
        /// </summary>
        public const int LGTaskDescription23530 = 856320222;
        /// <summary>
        /// LG-TaskConfig-Task-50006-Description-236-3-0
        /// 挑战继续，打通地下城 1 至 10 层，向着更高难度稳步前进。
        /// </summary>
        public const int LGTaskDescription23630 = 1135901682;
        /// <summary>
        /// LG-TaskConfig-Task-50007-Name-237-2-0
        /// 3星挑战3
        /// </summary>
        public const int LGTaskName23720 = 626392568;
        /// <summary>
        /// LG-TaskConfig-Task-50007-Description-237-3-0
        /// 霍尔山深处有凶兽在四处破坏，再不处理后果严重，你去把它们彻底制服。
        /// </summary>
        public const int LGTaskDescription23730 = 1438071386;
        /// <summary>
        /// LG-TaskConfig-Task-50008-Name-238-2-0
        /// 招兵买马
        /// </summary>
        public const int LGTaskName23820 = 1726262858;
        /// <summary>
        /// LG-TaskConfig-Task-50008-Description-238-3-0
        /// 营地人手紧缺，快去招募可靠勇士，壮大我们的探险队伍。
        /// </summary>
        public const int LGTaskDescription23830 = 740621534;
        /// <summary>
        /// LG-TaskConfig-Task-50009-Name-239-2-0
        /// 三星挑战1
        /// </summary>
        public const int LGTaskName23920 = 762992116;
        /// <summary>
        /// LG-TaskConfig-Task-50009-Description-239-3-0
        /// 致命矿井入口被畸变怪物占领了，矿工无法作业，你去肃清隐患打通道路。
        /// </summary>
        public const int LGTaskDescription23930 = 2074544022;
        /// <summary>
        /// LG-TaskConfig-Task-50010-Name-240-2-0
        /// 集结！
        /// </summary>
        public const int LGTaskName24020 = 293580738;
        /// <summary>
        /// LG-TaskConfig-Task-50010-Description-240-3-0
        /// 为应对危机，发起三次集结，召集同伴一同征战。
        /// </summary>
        public const int LGTaskDescription24030 = 130570780;
        /// <summary>
        /// LG-TaskConfig-Task-50011-Name-241-2-0
        /// 三星挑战2
        /// </summary>
        public const int LGTaskName24120 = 765029220;
        /// <summary>
        /// LG-TaskConfig-Task-50011-Description-241-3-0
        /// 致命矿井的坑道里怪物越来越多，已经影响通行，你去把它们全部清理掉。
        /// </summary>
        public const int LGTaskDescription24130 = 1857564020;
        /// <summary>
        /// LG-TaskConfig-Task-50013-Name-243-2-0
        /// 三星挑战3
        /// </summary>
        public const int LGTaskName24320 = 243070200;
        /// <summary>
        /// LG-TaskConfig-Task-50013-Description-243-3-0
        /// 致命矿井中层秩序混乱，怪物成群作乱，你快去平息祸乱，恢复这里的安全。
        /// </summary>
        public const int LGTaskDescription24330 = 941141684;
        /// <summary>
        /// LG-TaskConfig-Task-50015-Name-245-2-0
        /// 三星挑战4
        /// </summary>
        public const int LGTaskName24520 = 813964156;
        /// <summary>
        /// LG-TaskConfig-Task-50015-Description-245-3-0
        /// 致命矿井深处又黑又危险，不少冒险者失联，你去探查情况并破解危机。
        /// </summary>
        public const int LGTaskDescription24530 = 1471494108;
        /// <summary>
        /// LG-TaskConfig-Task-50016-Description-246-3-0
        /// 地下城危机加剧，全力通关 1 至 15 层，展现你真正的战力。
        /// </summary>
        public const int LGTaskDescription24630 = 1443163556;
        /// <summary>
        /// LG-TaskConfig-Task-50017-Name-247-2-0
        /// 三星挑战5
        /// </summary>
        public const int LGTaskName24720 = 1748152888;
        /// <summary>
        /// LG-TaskConfig-Task-50017-Description-247-3-0
        /// 致命矿井核心区域盘踞着强大魔物，是最大威胁，你去把它们全部清剿。
        /// </summary>
        public const int LGTaskDescription24730 = 1553839404;
        /// <summary>
        /// LG-TaskConfig-Task-50018-Name-248-2-0
        /// 地下城2
        /// </summary>
        public const int LGTaskName24820 = 1480627314;
        /// <summary>
        /// LG-TaskConfig-Task-50018-Description-248-3-0
        /// 突破层层险阻，将地下城 1 至 20 层全部打通，迈向新高度。
        /// </summary>
        public const int LGTaskDescription24830 = 426260572;
        /// <summary>
        /// LG-TaskConfig-Task-50019-Name-249-2-0
        /// 三星挑战6
        /// </summary>
        public const int LGTaskName24920 = 1730330196;
        /// <summary>
        /// LG-TaskConfig-Task-50019-Description-249-3-0
        /// 致命矿井底层被邪恶力量污染，诅咒不断扩散，你去找到源头并彻底终结。
        /// </summary>
        public const int LGTaskDescription24930 = 1285553380;
        /// <summary>
        /// LG-TaskConfig-Task-50021-Description-251-3-0
        /// 尖叫洞穴外层全是吵闹的尖叫怪，骚扰路人，你去把它们全部赶走。
        /// </summary>
        public const int LGTaskDescription25130 = 1712554654;
        /// <summary>
        /// LG-TaskConfig-Task-50023-Description-253-3-0
        /// 尖叫洞穴中层哀嚎声不断，妖物躁动不安，你过去镇压一下，恢复平静。
        /// </summary>
        public const int LGTaskDescription25330 = 45311602;
        /// <summary>
        /// LG-TaskConfig-Task-50025-Description-255-3-0
        /// 尖叫洞穴深处妖物横行，已经危害周边，你立刻过去把它们全部肃清。
        /// </summary>
        public const int LGTaskDescription25530 = 1439032950;
        /// <summary>
        /// LG-TaskConfig-Task-50027-Description-257-3-0
        /// 尖叫洞穴更深处藏着不为人知的危险，你进去探查一番，找出解决办法。
        /// </summary>
        public const int LGTaskDescription25730 = 1236258194;
        /// <summary>
        /// LG-TaskConfig-Task-50028-Description-258-3-0
        /// 地下城 25 层障碍重重，很多人止步于此，你用实力把它们全部打通。
        /// </summary>
        public const int LGTaskDescription25830 = 1674851202;
        /// <summary>
        /// LG-TaskConfig-Task-50029-Description-259-3-0
        /// 尖叫洞穴最深处出现了恐惧之源，力量很强，你勇敢前去把它击溃。
        /// </summary>
        public const int LGTaskDescription25930 = 201653582;
        /// <summary>
        /// LG-TaskConfig-Task-50030-Description-260-3-0
        /// 继续提升自己，把地下城 30 层全部通关，证明你是真正的勇者。
        /// </summary>
        public const int LGTaskDescription26030 = 894879244;
        /// <summary>
        /// LG-TaskConfig-Task-50031-Description-261-3-0
        /// 尖叫洞穴底层被可怕诅咒笼罩，再不阻止会扩散，你去把诅咒彻底终结。
        /// </summary>
        public const int LGTaskDescription26130 = 224556312;
        /// <summary>
        /// LG-TaskConfig-Task-50033-Description-263-3-0
        /// 霍尔山的怪物并没有完全消失，余孽又开始作乱，你再去一趟把它们清干净。
        /// </summary>
        public const int LGTaskDescription26330 = 1083733888;
        /// <summary>
        /// LG-TaskConfig-Task-50035-Description-265-3-0
        /// 霍尔山中段的乱源还没解决，怪物再次聚集，你再去平定纷争保护村民。
        /// </summary>
        public const int LGTaskDescription26530 = 960496152;
        /// <summary>
        /// LG-TaskConfig-Task-50037-Description-267-3-0
        /// 霍尔山深处的凶兽变得更强了，普通冒险者挡不住，你去把它们全部消灭。
        /// </summary>
        public const int LGTaskDescription26730 = 1913642224;
        /// <summary>
        /// LG-TaskConfig-Task-50039-Description-269-3-0
        /// 霍尔山腹地比之前更危险，里面藏着不少强敌，你深入进去把它们斩除。
        /// </summary>
        public const int LGTaskDescription26930 = 933109080;
        /// <summary>
        /// LG-TaskConfig-Task-50040-Description-270-3-0
        /// 你的实力已经明显提升，去挑战第二章地下城前五层，把它们顺利打通。
        /// </summary>
        public const int LGTaskDescription27030 = 952530710;
        /// <summary>
        /// LG-TaskConfig-Task-50041-Description-271-3-0
        /// 霍尔山深处有不少敌人巢穴，不断刷出怪物，你去把这些巢穴全部端掉。
        /// </summary>
        public const int LGTaskDescription27130 = 1719219070;
        /// <summary>
        /// LG-TaskConfig-Task-50042-Description-272-3-0
        /// 继续向前突破，把第二章地下城前十层全部打通，向着更高难度前进。
        /// </summary>
        public const int LGTaskDescription27230 = 536173826;
        /// <summary>
        /// LG-TaskConfig-Task-50043-Description-273-3-0
        /// 霍尔山最底层还有巨大威胁，一直没被解决，这次交给你去彻底终结。
        /// </summary>
        public const int LGTaskDescription27330 = 172951522;
        /// <summary>
        /// LG-TaskConfig-Task-50045-Description-275-3-0
        /// 中等难度的致命矿井里，怪物更强更凶，你去把这些畸变怪全部肃清。
        /// </summary>
        public const int LGTaskDescription27530 = 824313318;
        /// <summary>
        /// LG-TaskConfig-Task-50047-Description-277-3-0
        /// 中等矿井通道里骚乱再起，敌人数量大增，你去平定恢复矿区秩序。
        /// </summary>
        public const int LGTaskDescription27730 = 716630706;
        /// <summary>
        /// LG-TaskConfig-Task-50049-Description-279-3-0
        /// 中等矿井中层出现凶猛恶兽，矿工不敢靠近，你去镇压守护大家安全。
        /// </summary>
        public const int LGTaskDescription27930 = 1301591646;
        /// <summary>
        /// LG-TaskConfig-Task-50051-Description-281-3-0
        /// 中等矿井深处比以前更危险，暗藏很多陷阱和怪物，你去探查破解危机。
        /// </summary>
        public const int LGTaskDescription28130 = 855115516;
        /// <summary>
        /// LG-TaskConfig-Task-50052-Description-282-3-0
        /// 再接再厉不要停，把第二章地下城前 15 层全部打通，展现你的实力。
        /// </summary>
        public const int LGTaskDescription28230 = 1180506508;
        /// <summary>
        /// LG-TaskConfig-Task-50053-Description-283-3-0
        /// 中等矿井核心被很多强敌包围，你去把它们清掉，平定这里的混乱。
        /// </summary>
        public const int LGTaskDescription28330 = 812827508;
        /// <summary>
        /// LG-TaskConfig-Task-50054-Description-284-3-0
        /// 继续努力提升，把第二章地下城前 20 层全部打通，挑战更高的关卡。
        /// </summary>
        public const int LGTaskDescription28430 = 1371840932;
        /// <summary>
        /// LG-TaskConfig-Task-50055-Description-285-3-0
        /// 中等矿井底层的诅咒还没消失，一直在害人，你去把它彻底根除。
        /// </summary>
        public const int LGTaskDescription28530 = 217054212;
        /// <summary>
        /// LG-TaskConfig-Task-50057-Description-287-3-0
        /// 中等难度的尖叫洞穴怪啸不断，妖邪比以前更强，你去把它们赶走。
        /// </summary>
        public const int LGTaskDescription28730 = 159781996;
        /// <summary>
        /// LG-TaskConfig-Task-50059-Description-289-3-0
        /// 中等洞穴中层的恐惧力量在凝聚，再不处理会出事，你过去镇压稳住。
        /// </summary>
        public const int LGTaskDescription28930 = 653809116;
        /// <summary>
        /// LG-TaskConfig-Task-50061-Description-291-3-0
        /// 中等洞穴深处被妖邪占据，环境越来越差，你去清理还这里一片安宁。
        /// </summary>
        public const int LGTaskDescription29130 = 590935686;
        /// <summary>
        /// LG-TaskConfig-Task-50063-Description-293-3-0
        /// 中等洞穴秘境危险大幅提升，暗藏很多秘密，你深入探索找出破局关键。
        /// </summary>
        public const int LGTaskDescription29330 = 224818010;
        /// <summary>
        /// LG-TaskConfig-Task-50064-Description-294-3-0
        /// 继续往前推进，把第二章地下城 25 层全部打通，离终点更近一步。
        /// </summary>
        public const int LGTaskDescription29430 = 1287658178;
        /// <summary>
        /// LG-TaskConfig-Task-50065-Description-295-3-0
        /// 中等洞穴最深处出现类似心魔的怪物，专门干扰心智，你去把它打败。
        /// </summary>
        public const int LGTaskDescription29530 = 1598252030;
        /// <summary>
        /// LG-TaskConfig-Task-50066-Description-296-3-0
        /// 突破自己的极限，把第二章地下城 30 层全部打通，成为更强的冒险者。
        /// </summary>
        public const int LGTaskDescription29630 = 1889955446;
        /// <summary>
        /// LG-TaskConfig-Task-50067-Description-297-3-0
        /// 中等洞穴底层的诅咒还在扩散，很多人受影响，你去把诅咒彻底破除。
        /// </summary>
        public const int LGTaskDescription29730 = 2091435290;
        /// <summary>
        /// LG-TaskConfig-Task-50069-Description-299-3-0
        /// 困难难度的霍尔山魔物非常猖獗，普通队伍打不过，敢去清剿这些强敌吗。
        /// </summary>
        public const int LGTaskDescription29930 = 1129126198;
        /// <summary>
        /// LG-TaskConfig-Task-50071-Description-301-3-0
        /// 困难霍尔山一路都是危险和怪物，障碍重重，你披荆斩棘去平定祸乱。
        /// </summary>
        public const int LGTaskDescription30130 = 1021626853;
        /// <summary>
        /// LG-TaskConfig-Task-50073-Description-303-3-0
        /// 困难霍尔山最深处难关极多，怪物强度很高，你去攻克并制服那些凶顽。
        /// </summary>
        public const int LGTaskDescription30330 = 1462620461;
        /// <summary>
        /// LG-TaskConfig-Task-50075-Description-305-3-0
        /// 困难霍尔山里面就像一个魔窟，藏着祸首，你敢进去探查并把它斩杀吗。
        /// </summary>
        public const int LGTaskDescription30530 = 2089122397;
        /// <summary>
        /// LG-TaskConfig-Task-50076-Description-306-3-0
        /// 是时候真正展现实力了，去把第三章地下城前五层全部打通。
        /// </summary>
        public const int LGTaskDescription30630 = 207343797;
        /// <summary>
        /// LG-TaskConfig-Task-50077-Description-307-3-0
        /// 困难霍尔山的强敌全部出动了，拦路无数，你去横扫敌阵一往无前。
        /// </summary>
        public const int LGTaskDescription30730 = 14700389;
        /// <summary>
        /// LG-TaskConfig-Task-50078-Description-308-3-0
        /// 继续向前冲击，把第三章地下城前十层全部打通，不要停下脚步。
        /// </summary>
        public const int LGTaskDescription30830 = 17840893;
        /// <summary>
        /// LG-TaskConfig-Task-50079-Description-309-3-0
        /// 困难霍尔山的祸根一直在深处作怪，你去彻底征服，终结所有威胁。
        /// </summary>
        public const int LGTaskDescription30930 = 447076357;
        /// <summary>
        /// LG-TaskConfig-Task-50081-Description-311-3-0
        /// 困难难度的致命矿井真的很危险，怪物又多又强，你去血战一场荡平它们。
        /// </summary>
        public const int LGTaskDescription31130 = 1827100779;
        /// <summary>
        /// LG-TaskConfig-Task-50083-Description-313-3-0
        /// 困难矿井里到处都是怪物，几乎被包围，你杀出一条血路平定矿区。
        /// </summary>
        public const int LGTaskDescription31330 = 2096591815;
        /// <summary>
        /// LG-TaskConfig-Task-50085-Description-315-3-0
        /// 困难矿井里怪物成群结队，数量惊人，你去把它们全部扫平。
        /// </summary>
        public const int LGTaskDescription31530 = 294848051;
        /// <summary>
        /// LG-TaskConfig-Task-50087-Description-317-3-0
        /// 困难矿井最深处就是龙潭虎穴，一般人不敢进，你敢去斩杀强敌吗。
        /// </summary>
        public const int LGTaskDescription31730 = 1148749175;
        /// <summary>
        /// LG-TaskConfig-Task-50088-Description-318-3-0
        /// 不要停下脚步，把第三章地下城前 15 层全部打通，继续往上冲。
        /// </summary>
        public const int LGTaskDescription31830 = 1781723639;
        /// <summary>
        /// LG-TaskConfig-Task-50089-Description-319-3-0
        /// 困难矿井核心有强力怪物把守，很难突破，你去把它们全部剿灭。
        /// </summary>
        public const int LGTaskDescription31930 = 1107986923;
        /// <summary>
        /// LG-TaskConfig-Task-50090-Description-320-3-0
        /// 继续突破自我，把第三章地下城前 20 层全部打通，挑战更高难度。
        /// </summary>
        public const int LGTaskDescription32030 = 999891477;
        /// <summary>
        /// LG-TaskConfig-Task-50091-Description-321-3-0
        /// 困难矿井的祸患一直没根除，还在不断害人，你去彻底平定终结乱源。
        /// </summary>
        public const int LGTaskDescription32130 = 1707887465;
        /// <summary>
        /// LG-TaskConfig-Task-50093-Description-323-3-0
        /// 困难尖叫洞穴里群魔乱舞，力量极强，你去震慑住这些妖邪。
        /// </summary>
        public const int LGTaskDescription32330 = 1395086033;
        /// <summary>
        /// LG-TaskConfig-Task-50095-Description-325-3-0
        /// 困难洞穴里到处都是致命危险，不要退缩，只管往前冲，扫清所有阻碍。
        /// </summary>
        public const int LGTaskDescription32530 = 558684913;
        /// <summary>
        /// LG-TaskConfig-Task-50097-Description-327-3-0
        /// 困难洞穴深处妖邪遍地，危害极大，你全力清剿，守护这一片区域。
        /// </summary>
        public const int LGTaskDescription32730 = 967469497;
        /// <summary>
        /// LG-TaskConfig-Task-50099-Description-329-3-0
        /// 困难洞穴秘境里藏着很多玄机和谜题，你去探索并全部破解。
        /// </summary>
        public const int LGTaskDescription32930 = 1859112281;
        /// <summary>
        /// LG-TaskConfig-Task-50100-Description-330-3-0
        /// 再往前进一步，把第三章地下城 25 层全部打通，迈向更高峰。
        /// </summary>
        public const int LGTaskDescription33030 = 861898310;
        /// <summary>
        /// LG-TaskConfig-Task-50101-Description-331-3-0
        /// 困难洞穴最深处有超强怪物当道，你去打败它，在这里称雄。
        /// </summary>
        public const int LGTaskDescription33130 = 1901104302;
        /// <summary>
        /// LG-TaskConfig-Task-50102-Description-332-3-0
        /// 挑战自己的极限，把第三章地下城 30 层全部打通，成为一代强者。
        /// </summary>
        public const int LGTaskDescription33230 = 166930130;
        /// <summary>
        /// LG-TaskConfig-Task-50103-Description-333-3-0
        /// 困难洞穴底层出现了强大魔王，威胁极大，交给你去解决这个最终威胁。
        /// </summary>
        public const int LGTaskDescription33330 = 388860314;
        /// <summary>
        /// LG-TaskConfig-Task-50105-Description-335-3-0
        /// 噩梦难度的霍尔山简直就是绝境，进去九死一生，你敢进去死战清剿吗。
        /// </summary>
        public const int LGTaskDescription33530 = 1423482934;
        /// <summary>
        /// LG-TaskConfig-Task-50107-Description-337-3-0
        /// 在噩梦霍尔山里必须浴血奋战，敌人一波接一波，你奋勇破敌平定危机。
        /// </summary>
        public const int LGTaskDescription33730 = 58128634;
        /// <summary>
        /// LG-TaskConfig-Task-50109-Description-339-3-0
        /// 噩梦霍尔山底部就是死局，很多人折在这里，看你能否力挽狂澜破局。
        /// </summary>
        public const int LGTaskDescription33930 = 1447769390;
        /// <summary>
        /// LG-TaskConfig-Task-50111-Description-341-3-0
        /// 噩梦霍尔山的险境超乎想象，几乎逆天，你奋勇前行，把困局全部破除。
        /// </summary>
        public const int LGTaskDescription34130 = 686113832;
        /// <summary>
        /// LG-TaskConfig-Task-50112-Description-342-3-0
        /// 闯过这个难关，把第四章地下城前五层全部打通，开启噩梦征途。
        /// </summary>
        public const int LGTaskDescription34230 = 821252440;
        /// <summary>
        /// LG-TaskConfig-Task-50113-Description-343-3-0
        /// 在噩梦山里只能靠战斗平息一切，你把所有强敌横扫，稳住整个局面。
        /// </summary>
        public const int LGTaskDescription34330 = 1239773728;
        /// <summary>
        /// LG-TaskConfig-Task-50114-Description-344-3-0
        /// 继续往前不要回头，把第四章地下城前十层全部打通。
        /// </summary>
        public const int LGTaskDescription34430 = 342677672;
        /// <summary>
        /// LG-TaskConfig-Task-50115-Description-345-3-0
        /// 噩梦霍尔山的困局极难破解，一般人束手无策，看你能否完美破局。
        /// </summary>
        public const int LGTaskDescription34530 = 793108144;
        /// <summary>
        /// LG-TaskConfig-Task-50117-Description-347-3-0
        /// 噩梦致命矿井难度极高，九死一生，很多人有去无回，你敢去血战吗。
        /// </summary>
        public const int LGTaskDescription34730 = 1882547688;
        /// <summary>
        /// LG-TaskConfig-Task-50119-Description-349-3-0
        /// 噩梦矿井里战斗根本停不下来，敌人源源不断，看你能否坚持并破敌。
        /// </summary>
        public const int LGTaskDescription34930 = 501288664;
        /// <summary>
        /// LG-TaskConfig-Task-50121-Description-351-3-0
        /// 在噩梦矿井深处只能拼命厮杀，稍有不慎就失败，你要杀出生路破解危机。
        /// </summary>
        public const int LGTaskDescription35130 = 1665837850;
        /// <summary>
        /// LG-TaskConfig-Task-50123-Description-353-3-0
        /// 噩梦矿井深处已经像炼狱一样，环境恶劣怪物凶狠，你敢深入斩杀强敌吗。
        /// </summary>
        public const int LGTaskDescription35330 = 1310870310;
        /// <summary>
        /// LG-TaskConfig-Task-50124-Description-354-3-0
        /// 不要退缩害怕，把第四章地下城前 15 层全部打通，成为噩梦克星。
        /// </summary>
        public const int LGTaskDescription35430 = 1136605518;
        /// <summary>
        /// LG-TaskConfig-Task-50125-Description-355-3-0
        /// 噩梦炼狱里怪物横行无忌，没人敢管，你去把它们全部横扫彰显实力。
        /// </summary>
        public const int LGTaskDescription35530 = 1110563602;
        /// <summary>
        /// LG-TaskConfig-Task-50126-Description-356-3-0
        /// 继续勇敢前进，把第四章地下城前 20 层全部打通，无惧噩梦侵袭。
        /// </summary>
        public const int LGTaskDescription35630 = 1161068754;
        /// <summary>
        /// LG-TaskConfig-Task-50127-Description-357-3-0
        /// 噩梦矿井的关卡难度已经拉满，很少有人通关，看你能否完美过关。
        /// </summary>
        public const int LGTaskDescription35730 = 1675375878;
        /// <summary>
        /// LG-TaskConfig-Task-50129-Description-359-3-0
        /// 噩梦尖叫洞穴里到处都是恐惧气息，让人胆寒，你敢直面迎战破除吗。
        /// </summary>
        public const int LGTaskDescription35930 = 854056554;
        /// <summary>
        /// LG-TaskConfig-Task-50131-Description-361-3-0
        /// 噩梦洞穴里的恐惧力量越来越强，压制所有人，你去把它粉碎。
        /// </summary>
        public const int LGTaskDescription36130 = 688761984;
        /// <summary>
        /// LG-TaskConfig-Task-50133-Description-363-3-0
        /// 噩梦洞穴里的恐惧几乎主宰一切，你要直面它，掌控并破掉这个局面。
        /// </summary>
        public const int LGTaskDescription36330 = 2027596016;
        /// <summary>
        /// LG-TaskConfig-Task-50135-Description-365-3-0
        /// 噩梦洞穴已经完全变成炼狱，你进去探险，和里面的怪物一战到底。
        /// </summary>
        public const int LGTaskDescription36530 = 404794912;
        /// <summary>
        /// LG-TaskConfig-Task-50136-Description-366-3-0
        /// 再往前全力冲锋，把第四章地下城 25 层全部打通，成为噩梦征服者。
        /// </summary>
        public const int LGTaskDescription36630 = 1197812836;
        /// <summary>
        /// LG-TaskConfig-Task-50137-Description-367-3-0
        /// 噩梦洞穴最深处就是噩梦之巅，藏着最终恐惧，你去登顶并把它击溃。
        /// </summary>
        public const int LGTaskDescription36730 = 195110768;
        /// <summary>
        /// LG-TaskConfig-Task-50138-Description-368-3-0
        /// 挑战终极难度，把第四章地下城 30 层全部打通，成为噩梦终结者。
        /// </summary>
        public const int LGTaskDescription36830 = 882117924;
        /// <summary>
        /// LG-TaskConfig-Task-50139-Description-369-3-0
        /// 噩梦洞穴的恐惧之源已经完全苏醒，危害极大，由你前去终结消散阴霾。
        /// </summary>
        public const int LGTaskDescription36930 = 368220160;
        /// <summary>
        /// LG-TaskConfig-Task-50141-Description-371-3-0
        /// 地狱难度的霍尔山就是真正炼狱，怪物强到离谱，你敢直面并清剿强敌吗。
        /// </summary>
        public const int LGTaskDescription37130 = 368686646;
        /// <summary>
        /// LG-TaskConfig-Task-50143-Description-373-3-0
        /// 在炼狱一般的地狱山里展开争锋，敌人异常凶猛，你奋勇征战破局而出。
        /// </summary>
        public const int LGTaskDescription37330 = 1675350466;
        /// <summary>
        /// LG-TaskConfig-Task-50145-Description-375-3-0
        /// 地狱霍尔山底部险境丛生，怪物凶残至极，你要傲视群雄，把它们全部制服。
        /// </summary>
        public const int LGTaskDescription37530 = 1794696862;
        /// <summary>
        /// LG-TaskConfig-Task-50147-Description-377-3-0
        /// 地狱级的霍尔山征途步步惊心，危险无处不在，你奋勇征战去征服它。
        /// </summary>
        public const int LGTaskDescription37730 = 1625297410;
        /// <summary>
        /// LG-TaskConfig-Task-50148-Description-378-3-0
        /// 正式踏入地狱难度，把第五章地下城前五层全部打通，成为地狱行者。
        /// </summary>
        public const int LGTaskDescription37830 = 1069359198;
        /// <summary>
        /// LG-TaskConfig-Task-50149-Description-379-3-0
        /// 在地狱山里要成为真正主宰，所有强敌都不是对手，你去横扫称霸一方。
        /// </summary>
        public const int LGTaskDescription37930 = 294458902;
        /// <summary>
        /// LG-TaskConfig-Task-50150-Description-380-3-0
        /// 继续向前冲击，把第五章地下城前十层全部打通，成就地狱霸主。
        /// </summary>
        public const int LGTaskDescription38030 = 1481909160;
        /// <summary>
        /// LG-TaskConfig-Task-50151-Description-381-3-0
        /// 地狱霍尔山的威胁足以毁灭一切，由你前去完美制霸，彻底终结祸端。
        /// </summary>
        public const int LGTaskDescription38130 = 533599672;
        /// <summary>
        /// LG-TaskConfig-Task-50153-Description-383-3-0
        /// 地狱致命矿井就是炼狱现场，没人敢轻易开荒，你去血战扫清所有强敌。
        /// </summary>
        public const int LGTaskDescription38330 = 1885546536;
        /// <summary>
        /// LG-TaskConfig-Task-50155-Description-385-3-0
        /// 在地狱矿井里将要打一场前所未有的硬仗，全力破局平定混乱。
        /// </summary>
        public const int LGTaskDescription38530 = 294958144;
        /// <summary>
        /// LG-TaskConfig-Task-50157-Description-387-3-0
        /// 地狱矿井里全是炼狱级怪物，四处破坏，你去把它们全部扫平守护矿区。
        /// </summary>
        public const int LGTaskDescription38730 = 960257456;
        /// <summary>
        /// LG-TaskConfig-Task-50159-Description-389-3-0
        /// 地狱矿井已经深达九幽之地，阴森恐怖，你敢进去探索斩杀强敌吗。
        /// </summary>
        public const int LGTaskDescription38930 = 937680248;
        /// <summary>
        /// LG-TaskConfig-Task-50160-Description-390-3-0
        /// 勇往直前不要怕，把第五章地下城前 15 层全部打通，化身地狱先锋。
        /// </summary>
        public const int LGTaskDescription39030 = 1432982326;
        /// <summary>
        /// LG-TaskConfig-Task-50161-Description-391-3-0
        /// 在九幽之地里力争称雄，所有怪物都不是对手，横扫强敌彰显真正实力。
        /// </summary>
        public const int LGTaskDescription39130 = 1074078938;
        /// <summary>
        /// LG-TaskConfig-Task-50162-Description-392-3-0
        /// 继续突破极限，把第五章地下城前 20 层全部打通，成为地狱战神。
        /// </summary>
        public const int LGTaskDescription39230 = 154582586;
        /// <summary>
        /// LG-TaskConfig-Task-50163-Description-393-3-0
        /// 地狱矿井的祸患足以波及整片区域，由你前去彻底平定终结乱源。
        /// </summary>
        public const int LGTaskDescription39330 = 1410118694;
        /// <summary>
        /// LG-TaskConfig-Task-50165-Description-395-3-0
        /// 地狱尖叫洞穴已经沦为九幽之地，邪气冲天，你敢前去一探究竟吗。
        /// </summary>
        public const int LGTaskDescription39530 = 637352882;
        /// <summary>
        /// LG-TaskConfig-Task-50167-Description-397-3-0
        /// 地狱洞穴里九幽妖邪到处横行，无人敢挡，你去震慑稳住整个局面。
        /// </summary>
        public const int LGTaskDescription39730 = 2019627526;
        /// <summary>
        /// LG-TaskConfig-Task-50169-Description-399-3-0
        /// 地狱洞穴里怪物疯狂作乱，环境持续恶化，你去清理还这里安宁。
        /// </summary>
        public const int LGTaskDescription39930 = 164376010;
        /// <summary>
        /// LG-TaskConfig-Task-50171-Description-401-3-0
        /// 地狱洞穴已经到达炼狱顶端，藏着最终秘密，你去探索并一战到底。
        /// </summary>
        public const int LGTaskDescription40130 = 590855787;
        /// <summary>
        /// LG-TaskConfig-Task-50172-Description-402-3-0
        /// 冲击巅峰实力，把第五章地下城 25 层全部打通，登顶地狱至尊。
        /// </summary>
        public const int LGTaskDescription40230 = 1246422555;
        /// <summary>
        /// LG-TaskConfig-Task-50173-Description-403-3-0
        /// 在地狱洞穴深处登顶九幽，打败所有对手，成为这里最强的存在。
        /// </summary>
        public const int LGTaskDescription40330 = 881657387;
        /// <summary>
        /// LG-TaskConfig-Task-50174-Description-404-3-0
        /// 完成这场终极挑战，把第五章地下城 30 层全部打通，成就地狱传说。
        /// </summary>
        public const int LGTaskDescription40430 = 1353356203;
        /// <summary>
        /// LG-TaskConfig-Task-50175-Description-405-3-0
        /// 地狱洞穴底层祸乱滔天，邪恶力量快要失控，交给你前去终结还世间安稳。
        /// </summary>
        public const int LGTaskDescription40530 = 1468591587;
        /// <summary>
        /// LG-TaskConfig-Task-60001-Description-406-3-0
        /// 打水去交易行救火
        /// </summary>
        public const int LGTaskDescription40630 = 65146491;
        /// <summary>
        /// LG-TaskConfig-Task-60002-Description-407-3-0
        /// 打水去旅店和医院救火
        /// </summary>
        public const int LGTaskDescription40730 = 1682469041;
        /// <summary>
        /// LG-TaskConfig-Task-60003-Description-408-3-0
        /// 救出受困的英雄
        /// </summary>
        public const int LGTaskDescription40830 = 366074359;
        /// <summary>
        /// LG-TaskConfig-Task-60004-Description-409-3-0
        /// 招募石盾加伦
        /// </summary>
        public const int LGTaskDescription40930 = 542932433;
        /// <summary>
        /// LG-TaskConfig-Task-60101-Description-410-3-0
        /// 使用退伍金修复医院
        /// </summary>
        public const int LGTaskDescription41030 = 2112746739;
        /// <summary>
        /// LG-TaskConfig-Task-60104-Description-413-3-0
        /// 等待加伦疗伤
        /// </summary>
        public const int LGTaskDescription41330 = 699685601;
        /// <summary>
        /// LG-TaskConfig-Task-60105-Description-414-3-0
        /// 观察加伦前往探索
        /// </summary>
        public const int LGTaskDescription41430 = 1527529683;
        /// <summary>
        /// LG-TaskConfig-Task-60106-Description-415-3-0
        /// 与兄弟对话
        /// </summary>
        public const int LGTaskDescription41530 = 852494521;
        /// <summary>
        /// LG-TaskConfig-Task-60112-Description-416-3-0
        /// 去狮鹫管理员吹响哨子
        /// </summary>
        public const int LGTaskDescription41630 = 843811017;
        /// <summary>
        /// LG-TaskConfig-Task-60113-Description-417-3-0
        /// 招募影刃凯
        /// </summary>
        public const int LGTaskDescription41730 = 1257581405;
        /// <summary>
        /// LG-TaskConfig-Task-60114-Description-418-3-0
        /// 招募快枪托比
        /// </summary>
        public const int LGTaskDescription41830 = 1500251805;
        /// <summary>
        /// LG-TaskConfig-Task-60107-Description-419-3-0
        /// 为英雄们修复旅店
        /// </summary>
        public const int LGTaskDescription41930 = 121428230;
        /// <summary>
        /// LG-TaskConfig-Task-60108-Description-420-3-0
        /// 购买1个美味小鱼
        /// </summary>
        public const int LGTaskDescription42030 = 2141416387;
        /// <summary>
        /// LG-TaskConfig-Task-60109-Description-421-3-0
        /// 找旅店老板生产炸鱼
        /// </summary>
        public const int LGTaskDescription42130 = 1874141503;
        /// <summary>
        /// LG-TaskConfig-Task-60110-Description-422-3-0
        /// 等待他们享用炸鱼
        /// </summary>
        public const int LGTaskDescription42230 = 477403408;
        /// <summary>
        /// LG-TaskConfig-Task-60111-Description-423-3-0
        /// 通关霍尔山1.山脚
        /// </summary>
        public const int LGTaskDescription42330 = 187923604;
        /// <summary>
        /// LG-TaskConfig-Task-60212-Description-424-3-0
        /// 领取医院收入
        /// </summary>
        public const int LGTaskDescription42430 = 452856721;
        /// <summary>
        /// LG-TaskConfig-Task-60218-Description-426-3-0
        /// 吹响狮鹫哨
        /// </summary>
        public const int LGTaskDescription42630 = 1399815577;
        /// <summary>
        /// LG-TaskConfig-Task-60219-Description-427-3-0
        /// 完成一次招募
        /// </summary>
        public const int LGTaskDescription42730 = 917102333;
        /// <summary>
        /// LG-TaskConfig-Task-60220-Description-428-3-0
        /// 集结霍尔山2.旧山道
        /// </summary>
        public const int LGTaskDescription42830 = 1942191804;
        /// <summary>
        /// LG-TaskConfig-Task-60321-Description-429-3-0
        /// 向兄弟了解村庄升级的事
        /// </summary>
        public const int LGTaskDescription42930 = 119605387;
        /// <summary>
        /// LG-TaskConfig-Task-60325-Description-430-3-0
        /// 使村庄等级达到临时营地II
        /// </summary>
        public const int LGTaskDescription43030 = 1780851261;
        /// <summary>
        /// LG-TaskConfig-Task-60327-Description-431-3-0
        /// 累计招募次数
        /// </summary>
        public const int LGTaskDescription43130 = 822323576;
        /// <summary>
        /// LG-TaskConfig-Task-60329-Description-432-3-0
        /// 通关霍尔山2.旧山道
        /// </summary>
        public const int LGTaskDescription43230 = 1426257547;
        /// <summary>
        /// LG-TaskConfig-Task-60432-Description-433-3-0
        /// 对话铁匠铺老板
        /// </summary>
        public const int LGTaskDescription43330 = 1164629745;
        /// <summary>
        /// LG-TaskConfig-Task-60330-Description-434-3-0
        /// 完成任意一次副本
        /// </summary>
        public const int LGTaskDescription43430 = 1724879615;
        /// <summary>
        /// LG-TaskConfig-Task-60431-Description-435-3-0
        /// 领取副本税金分红
        /// </summary>
        public const int LGTaskDescription43530 = 1141330158;
        /// <summary>
        /// LG-TaskConfig-Task-60434-Description-436-3-0
        /// 制造法杖并交易给蕾姆
        /// </summary>
        public const int LGTaskDescription43630 = 81686380;
        /// <summary>
        /// LG-TaskConfig-Task-60435-Description-437-3-0
        /// 制造火枪并交易给托比
        /// </summary>
        public const int LGTaskDescription43730 = 1373125836;
        /// <summary>
        /// LG-TaskConfig-Task-60540-Description-439-3-0
        /// 让6名英雄装备上武器
        /// </summary>
        public const int LGTaskDescription43930 = 939957525;
        /// <summary>
        /// LG-TaskConfig-Task-60543-Description-440-3-0
        /// 集结霍尔山3.碎石坡
        /// </summary>
        public const int LGTaskDescription44030 = 1097661464;
        /// <summary>
        /// LG-TaskConfig-Task-60651-Description-441-3-0
        /// 提升村庄等级到临时营地III
        /// </summary>
        public const int LGTaskDescription44130 = 624275309;
        /// <summary>
        /// LG-TaskConfig-Task-60644-Description-442-3-0
        /// 对话狮鹫管理员
        /// </summary>
        public const int LGTaskDescription44230 = 2118955106;
        /// <summary>
        /// LG-TaskConfig-Task-60754-Description-444-3-0
        /// 新上阵1名英雄
        /// </summary>
        public const int LGTaskDescription44430 = 1992554766;
        /// <summary>
        /// LG-TaskConfig-Task-60755-Description-445-3-0
        /// 让7名英雄装备上武器
        /// </summary>
        public const int LGTaskDescription44530 = 624301918;
        /// <summary>
        /// LG-TaskConfig-Task-60756-Description-446-3-0
        /// 通关霍尔山4.林间岔路
        /// </summary>
        public const int LGTaskDescription44630 = 1533571854;
        /// <summary>
        /// LG-TaskConfig-Task-60972-Description-447-3-0
        /// 提升村庄等级初级村庄
        /// </summary>
        public const int LGTaskDescription44730 = 2146502837;
        /// <summary>
        /// LG-TaskConfig-Task-60757-Description-448-3-0
        /// 提升探索等级到2级
        /// </summary>
        public const int LGTaskDescription44830 = 890515311;
        /// <summary>
        /// LG-TaskConfig-Task-60861-Description-450-3-0
        /// 首次制作10级绿品武器
        /// </summary>
        public const int LGTaskDescription45030 = 924719454;
        /// <summary>
        /// LG-TaskConfig-Task-60862-Description-451-3-0
        /// 出售刚做好的绿品武器
        /// </summary>
        public const int LGTaskDescription45130 = 1457635808;
        /// <summary>
        /// LG-TaskConfig-Task-60758-Description-452-3-0
        /// 完成绿色稀有度副本
        /// </summary>
        public const int LGTaskDescription45230 = 842494325;
        /// <summary>
        /// LG-TaskConfig-Task-60980-Description-453-3-0
        /// 让3名英雄装备绿品武器
        /// </summary>
        public const int LGTaskDescription45330 = 2088218161;
        /// <summary>
        /// LG-TaskConfig-Task-61082-Description-454-3-0
        /// 对话裁缝店老板
        /// </summary>
        public const int LGTaskDescription45430 = 847428748;
        /// <summary>
        /// LG-TaskConfig-Task-60859-Description-455-3-0
        /// 装备白品防具的人数
        /// </summary>
        public const int LGTaskDescription45530 = 1766471542;
        /// <summary>
        /// LG-TaskConfig-Task-61088-Description-456-3-0
        /// 对话首饰店老板
        /// </summary>
        public const int LGTaskDescription45630 = 198001016;
        /// <summary>
        /// LG-TaskConfig-Task-60869-Description-457-3-0
        /// 装备白品饰品的人数
        /// </summary>
        public const int LGTaskDescription45730 = 184287167;
        /// <summary>
        /// LG-TaskConfig-Task-60646-Description-459-3-0
        /// 给任意英雄升星
        /// </summary>
        public const int LGTaskDescription45930 = 477647480;
        /// <summary>
        /// LG-TaskConfig-Task-60975-Description-460-3-0
        /// 安排8名英雄入驻村庄
        /// </summary>
        public const int LGTaskDescription46030 = 462980547;
        /// <summary>
        /// LG-TaskConfig-Task-60978-Description-462-3-0
        /// 让5名英雄装备绿品武器
        /// </summary>
        public const int LGTaskDescription46230 = 321493190;
        /// <summary>
        /// LG-TaskConfig-Task-60981-Description-463-3-0
        /// 集结霍尔山5.半山营地
        /// </summary>
        public const int LGTaskDescription46330 = 1127939177;
        /// <summary>
        /// LG-TaskConfig-Task-61087-Description-465-3-0
        /// 装备绿品防具的人数
        /// </summary>
        public const int LGTaskDescription46530 = 1605157203;
        /// <summary>
        /// LG-TaskConfig-Task-61090-Description-466-3-0
        /// 升级首饰店到2级
        /// </summary>
        public const int LGTaskDescription46630 = 1988647002;
        /// <summary>
        /// LG-TaskConfig-Task-61093-Description-467-3-0
        /// 装备绿品饰品的人数
        /// </summary>
        public const int LGTaskDescription46730 = 101978664;
        /// <summary>
        /// LG-TaskConfig-Task-61194-Description-468-3-0
        /// 通关霍尔山5.半山营地
        /// </summary>
        public const int LGTaskDescription46830 = 463627767;
        /// <summary>
        /// LG-TaskConfig-Task-61197-Description-470-3-0
        /// 集结霍尔山6.断桥
        /// </summary>
        public const int LGTaskDescription47030 = 1716202463;
        /// <summary>
        /// LG-TaskConfig-Task-61100-Description-472-3-0
        /// 集结霍尔山7.兽群巢穴
        /// </summary>
        public const int LGTaskDescription47230 = 1694469589;
        /// <summary>
        /// LG-TaskConfig-Task-61201-Description-473-3-0
        /// 查看首充礼包
        /// </summary>
        public const int LGTaskDescription47330 = 1855321696;
        /// <summary>
        /// LG-TaskConfig-Task-61203-Description-474-3-0
        /// 装备武器的人数
        /// </summary>
        public const int LGTaskDescription47430 = 1481449627;
        /// <summary>
        /// LG-TaskConfig-Task-61204-Description-475-3-0
        /// 装备防具的人数
        /// </summary>
        public const int LGTaskDescription47530 = 1948893729;
        /// <summary>
        /// LG-TaskConfig-Task-61205-Description-476-3-0
        /// 装备饰品的人数
        /// </summary>
        public const int LGTaskDescription47630 = 23561051;
        /// <summary>
        /// LG-TaskConfig-Task-61206-Description-477-3-0
        /// 通关霍尔山8.蛛网密林
        /// </summary>
        public const int LGTaskDescription47730 = 266854629;
        /// <summary>
        /// LG-TaskConfig-Task-61307-Description-478-3-0
        /// 提升探索等级到3级
        /// </summary>
        public const int LGTaskDescription47830 = 1177209824;
        /// <summary>
        /// LG-TaskConfig-Task-61310-Description-481-3-0
        /// 交易10级蓝品武器给英雄
        /// </summary>
        public const int LGTaskDescription48130 = 639494100;
        /// <summary>
        /// LG-TaskConfig-Task-61312-Description-483-3-0
        /// 交易10级蓝品防具给英雄
        /// </summary>
        public const int LGTaskDescription48330 = 2097397596;
        /// <summary>
        /// LG-TaskConfig-Task-61313-Description-484-3-0
        /// 升级首饰店到3级
        /// </summary>
        public const int LGTaskDescription48430 = 2084221126;
        /// <summary>
        /// LG-TaskConfig-Task-61314-Description-485-3-0
        /// 交易10级蓝品饰品给英雄
        /// </summary>
        public const int LGTaskDescription48530 = 381490092;
        /// <summary>
        /// LG-TaskConfig-Task-61316-Description-486-3-0
        /// 集结霍尔山9.豺狼哨站
        /// </summary>
        public const int LGTaskDescription48630 = 1157175191;
        /// <summary>
        /// LG-TaskConfig-Task-61431-Description-488-3-0
        /// 查看每日任务
        /// </summary>
        public const int LGTaskDescription48830 = 165952315;
        /// <summary>
        /// LG-TaskConfig-Task-61418-Description-489-3-0
        /// 在集市购买免费道具
        /// </summary>
        public const int LGTaskDescription48930 = 596663929;
        /// <summary>
        /// LG-TaskConfig-Task-61424-Description-491-3-0
        /// 装备蓝品武器的人数
        /// </summary>
        public const int LGTaskDescription49130 = 1204712635;
        /// <summary>
        /// LG-TaskConfig-Task-61420-Description-492-3-0
        /// 集结霍尔山10.山腰废墟
        /// </summary>
        public const int LGTaskDescription49230 = 772281426;
        /// <summary>
        /// LG-TaskConfig-Task-61421-Description-493-3-0
        /// 查看并领取七日签到奖励
        /// </summary>
        public const int LGTaskDescription49330 = 725961526;
        /// <summary>
        /// LG-TaskConfig-Task-61425-Description-494-3-0
        /// 装备蓝品防具的人数
        /// </summary>
        public const int LGTaskDescription49430 = 938982403;
        /// <summary>
        /// LG-TaskConfig-Task-61422-Description-495-3-0
        /// 集结霍尔山11.风暴山径
        /// </summary>
        public const int LGTaskDescription49530 = 898049493;
        /// <summary>
        /// LG-TaskConfig-Task-61423-Description-496-3-0
        /// 查看并领取成长试炼活动奖励
        /// </summary>
        public const int LGTaskDescription49630 = 1668046883;
        /// <summary>
        /// LG-TaskConfig-Task-61426-Description-497-3-0
        /// 装备蓝品饰品的人数
        /// </summary>
        public const int LGTaskDescription49730 = 1667782339;
        /// <summary>
        /// LG-TaskConfig-Task-61427-Description-498-3-0
        /// 通关霍尔山12.山顶集结
        /// </summary>
        public const int LGTaskDescription49830 = 1840875769;
        /// <summary>
        /// LG-TaskConfig-Task-61428-Description-499-3-0
        /// 切换地图到西部旷野
        /// </summary>
        public const int LGTaskDescription49930 = 1237753007;
        /// <summary>
        /// LG-TaskConfig-Task-61429-Description-500-3-0
        /// 升级旅店到11级
        /// </summary>
        public const int LGTaskDescription50030 = 1800031661;
        /// <summary>
        /// LG-TaskConfig-Task-61430-Description-501-3-0
        /// 升级医院到11级
        /// </summary>
        public const int LGTaskDescription50130 = 1474881976;
        /// <summary>
        /// LG-TaskConfig-Task-61502-Description-503-3-0
        /// 参与1次竞技场
        /// </summary>
        public const int LGTaskDescription50330 = 80670536;
        /// <summary>
        /// LG-TaskConfig-Task-61506-Description-504-3-0
        /// 通关致命矿井1.矿口营地
        /// </summary>
        public const int LGTaskDescription50430 = 1115896457;
        /// <summary>
        /// LG-TaskConfig-Task-61507-Description-505-3-0
        /// 通关圣印之塔1层
        /// </summary>
        public const int LGTaskDescription50530 = 2107950713;
        /// <summary>
        /// LG-TaskConfig-Task-61505-Description-506-3-0
        /// 装配1个绿色圣印
        /// </summary>
        public const int LGTaskDescription50630 = 267612478;
        /// <summary>
        /// LG-TaskConfig-Task-61610-Description-510-3-0
        /// 通关致命矿井2.旧矿栈道
        /// </summary>
        public const int LGTaskDescription51030 = 1604660718;
        /// <summary>
        /// LG-TaskConfig-Task-61701-Description-511-3-0
        /// 提升西部旷野探索等级
        /// </summary>
        public const int LGTaskDescription51130 = 406391100;
        /// <summary>
        /// LG-TaskConfig-Task-61706-Description-514-3-0
        /// 使村庄等级达到初级村庄II
        /// </summary>
        public const int LGTaskDescription51430 = 79396074;
        /// <summary>
        /// LG-TaskConfig-Task-61708-Description-516-3-0
        /// 安排10名英雄入驻村庄
        /// </summary>
        public const int LGTaskDescription51630 = 1725803474;
        /// <summary>
        /// LG-TaskConfig-Task-61709-Description-517-3-0
        /// 装备20级武器的人数
        /// </summary>
        public const int LGTaskDescription51730 = 1677447054;
        /// <summary>
        /// LG-TaskConfig-Task-61710-Description-518-3-0
        /// 装备20级防具的人数
        /// </summary>
        public const int LGTaskDescription51830 = 1912624001;
        /// <summary>
        /// LG-TaskConfig-Task-61711-Description-519-3-0
        /// 装备20级饰品的人数
        /// </summary>
        public const int LGTaskDescription51930 = 1826897541;
        /// <summary>
        /// LG-TaskConfig-Task-61712-Description-520-3-0
        /// 通关致命矿井3.断轨深处
        /// </summary>
        public const int LGTaskDescription52030 = 1747692688;
        /// <summary>
        /// LG-TaskConfig-Task-61801-Description-521-3-0
        /// 累计参与4次竞技场
        /// </summary>
        public const int LGTaskDescription52130 = 1774603894;
        /// <summary>
        /// LG-TaskConfig-Task-61802-Description-522-3-0
        /// 强化1次圣印
        /// </summary>
        public const int LGTaskDescription52230 = 493011242;
        /// <summary>
        /// LG-TaskConfig-Task-61807-Description-526-3-0
        /// 通关致命矿井4.碎石矿坑
        /// </summary>
        public const int LGTaskDescription52630 = 732179409;
        /// <summary>
        /// LG-TaskConfig-Task-61912-Description-532-3-0
        /// 通关致命矿井5.鼠人巷道
        /// </summary>
        public const int LGTaskDescription53230 = 1558135285;
        /// <summary>
        /// LG-TaskConfig-Task-62001-Description-533-3-0
        /// 通关圣印之塔5层
        /// </summary>
        public const int LGTaskDescription53330 = 527021710;
        /// <summary>
        /// LG-TaskConfig-Task-62002-Description-534-3-0
        /// 装配2个绿色圣印
        /// </summary>
        public const int LGTaskDescription53430 = 838986598;
        /// <summary>
        /// LG-TaskConfig-Task-62003-Description-535-3-0
        /// 装备至少20级绿品武器人数
        /// </summary>
        public const int LGTaskDescription53530 = 1721992742;
        /// <summary>
        /// LG-TaskConfig-Task-62004-Description-536-3-0
        /// 装备至少20级绿品防具人数
        /// </summary>
        public const int LGTaskDescription53630 = 347027726;
        /// <summary>
        /// LG-TaskConfig-Task-62005-Description-537-3-0
        /// 装备至少20级绿品饰品人数
        /// </summary>
        public const int LGTaskDescription53730 = 82167166;
        /// <summary>
        /// LG-TaskConfig-Task-62006-Description-538-3-0
        /// 通关致命矿井6.毒蛛矿层
        /// </summary>
        public const int LGTaskDescription53830 = 35503046;
        /// <summary>
        /// LG-TaskConfig-Task-62102-Description-540-3-0
        /// 累计完成20级紫色稀有度副本
        /// </summary>
        public const int LGTaskDescription54030 = 1081981982;
        /// <summary>
        /// LG-TaskConfig-Task-62104-Description-541-3-0
        /// 装备至少20级紫品武器人数
        /// </summary>
        public const int LGTaskDescription54130 = 1575188679;
        /// <summary>
        /// LG-TaskConfig-Task-62105-Description-542-3-0
        /// 装备至少20级紫品防具人数
        /// </summary>
        public const int LGTaskDescription54230 = 2066268173;
        /// <summary>
        /// LG-TaskConfig-Task-62106-Description-543-3-0
        /// 装备至少20级紫品饰品人数
        /// </summary>
        public const int LGTaskDescription54330 = 1260010895;
        /// <summary>
        /// LG-TaskConfig-Task-62107-Description-544-3-0
        /// 使用副本搜索功能
        /// </summary>
        public const int LGTaskDescription54430 = 627233417;
        /// <summary>
        /// LG-TaskConfig-Task-62111-Description-546-3-0
        /// 使村庄等级达到初级村庄III
        /// </summary>
        public const int LGTaskDescription54630 = 1737300212;
        /// <summary>
        /// LG-TaskConfig-Task-62113-Description-548-3-0
        /// 安排11名英雄入驻村庄
        /// </summary>
        public const int LGTaskDescription54830 = 383184280;
        /// <summary>
        /// LG-TaskConfig-Task-62117-Description-552-3-0
        /// 通关致命矿井7.鼠人巷道
        /// </summary>
        public const int LGTaskDescription55230 = 2051853287;
        /// <summary>
        /// LG-TaskConfig-Task-62201-Description-553-3-0
        /// 通关圣印之塔10层
        /// </summary>
        public const int LGTaskDescription55330 = 2029085354;
        /// <summary>
        /// LG-TaskConfig-Task-62202-Description-554-3-0
        /// 装配3个绿色圣印
        /// </summary>
        public const int LGTaskDescription55430 = 1586440062;
        /// <summary>
        /// LG-TaskConfig-Task-62207-Description-558-3-0
        /// 通关致命矿井8.炸药仓库
        /// </summary>
        public const int LGTaskDescription55830 = 1960259677;
        /// <summary>
        /// LG-TaskConfig-Task-62302-Description-560-3-0
        /// 累计完成20级蓝色稀有度副本
        /// </summary>
        public const int LGTaskDescription56030 = 70633790;
        /// <summary>
        /// LG-TaskConfig-Task-62312-Description-564-3-0
        /// 通关致命矿井9.黑铁熔炉
        /// </summary>
        public const int LGTaskDescription56430 = 1634859527;
        /// <summary>
        /// LG-TaskConfig-Task-62401-Description-565-3-0
        /// 累计参与7次竞技场
        /// </summary>
        public const int LGTaskDescription56530 = 32028721;
        /// <summary>
        /// LG-TaskConfig-Task-62402-Description-566-3-0
        /// 累计强化2次圣印
        /// </summary>
        public const int LGTaskDescription56630 = 1017434245;
        /// <summary>
        /// LG-TaskConfig-Task-62405-Description-568-3-0
        /// 装备至少20级蓝品武器人数
        /// </summary>
        public const int LGTaskDescription56830 = 2050489534;
        /// <summary>
        /// LG-TaskConfig-Task-62406-Description-569-3-0
        /// 装备至少20级蓝品防具人数
        /// </summary>
        public const int LGTaskDescription56930 = 2096182628;
        /// <summary>
        /// LG-TaskConfig-Task-62407-Description-570-3-0
        /// 装备至少20级蓝色饰品人数
        /// </summary>
        public const int LGTaskDescription57030 = 317141569;
        /// <summary>
        /// LG-TaskConfig-Task-62408-Description-571-3-0
        /// 通关致命矿井10.塌方深井
        /// </summary>
        public const int LGTaskDescription57130 = 731371051;
        /// <summary>
        /// LG-TaskConfig-Task-62505-Description-574-3-0
        /// 使村庄等级达到中级村庄
        /// </summary>
        public const int LGTaskDescription57430 = 177609592;
        /// <summary>
        /// LG-TaskConfig-Task-62507-Description-575-3-0
        /// 安排12名英雄入驻村庄
        /// </summary>
        public const int LGTaskDescription57530 = 1573212309;
        /// <summary>
        /// LG-TaskConfig-Task-62510-Description-578-3-0
        /// 装备至少20级蓝品饰品人数
        /// </summary>
        public const int LGTaskDescription57830 = 580534370;
        /// <summary>
        /// LG-TaskConfig-Task-62511-Description-579-3-0
        /// 通关致命矿井11.头目矿厅
        /// </summary>
        public const int LGTaskDescription57930 = 256062674;
        /// <summary>
        /// LG-TaskConfig-Task-62601-Description-580-3-0
        /// 通关圣印之塔15层
        /// </summary>
        public const int LGTaskDescription58030 = 1818710142;
        /// <summary>
        /// LG-TaskConfig-Task-62602-Description-581-3-0
        /// 扫荡1次圣印之塔
        /// </summary>
        public const int LGTaskDescription58130 = 1518935400;
        /// <summary>
        /// LG-TaskConfig-Task-62604-Description-583-3-0
        /// 装备至少20级蓝色武器人数
        /// </summary>
        public const int LGTaskDescription58330 = 28960828;
        /// <summary>
        /// LG-TaskConfig-Task-62605-Description-584-3-0
        /// 装备至少20级蓝色防具人数
        /// </summary>
        public const int LGTaskDescription58430 = 1756843830;
        /// <summary>
        /// LG-TaskConfig-Task-62607-Description-586-3-0
        /// 通关致命矿井12.矿底集结
        /// </summary>
        public const int LGTaskDescription58630 = 499368974;
        /// <summary>
        /// LG-TaskConfig-Task-62608-Description-587-3-0
        /// 切换地图到幽暗沼泽
        /// </summary>
        public const int LGTaskDescription58730 = 375561612;
        /// <summary>
        /// LG-TaskConfig-Task-62609-Description-588-3-0
        /// 升级旅店到21级
        /// </summary>
        public const int LGTaskDescription58830 = 970223406;
        /// <summary>
        /// LG-TaskConfig-Task-62610-Description-589-3-0
        /// 升级医院到21级
        /// </summary>
        public const int LGTaskDescription58930 = 323776235;
        /// <summary>
        /// LG-TaskConfig-Task-62702-Description-591-3-0
        /// 去领地采集1次资源
        /// </summary>
        public const int LGTaskDescription59130 = 1297109950;
        /// <summary>
        /// LG-TaskConfig-Task-62704-Description-593-3-0
        /// 通关月影湖1.雾沼入口
        /// </summary>
        public const int LGTaskDescription59330 = 658888286;
        /// <summary>
        /// LG-TaskConfig-Task-62705-Description-594-3-0
        /// 升级铁匠铺到7级
        /// </summary>
        public const int LGTaskDescription59430 = 473469656;
        /// <summary>
        /// LG-TaskConfig-Task-62706-Description-595-3-0
        /// 升级裁缝铺到7级
        /// </summary>
        public const int LGTaskDescription59530 = 1542098902;
        /// <summary>
        /// LG-TaskConfig-Task-62707-Description-596-3-0
        /// 升级炼金商店到7级
        /// </summary>
        public const int LGTaskDescription59630 = 479481852;
        /// <summary>
        /// LG-TaskConfig-Task-62708-Description-597-3-0
        /// 通关月影湖4.荧火浅滩
        /// </summary>
        public const int LGTaskDescription59730 = 1065235478;
        /// <summary>
        /// LG-TaskConfig-Task-62801-Description-598-3-0
        /// 提升幽暗沼泽探索等级
        /// </summary>
        public const int LGTaskDescription59830 = 1267489677;
        /// <summary>
        /// LG-TaskConfig-Task-62803-Description-600-3-0
        /// 使村庄等级达到中级村庄II
        /// </summary>
        public const int LGTaskDescription60030 = 2119889719;
        /// <summary>
        /// LG-TaskConfig-Task-62804-Description-601-3-0
        /// 安排13名英雄入驻村庄
        /// </summary>
        public const int LGTaskDescription60130 = 242774953;
        /// <summary>
        /// LG-TaskConfig-Task-62805-Description-602-3-0
        /// 装备至少30级武器人数
        /// </summary>
        public const int LGTaskDescription60230 = 1632049387;
        /// <summary>
        /// LG-TaskConfig-Task-62806-Description-603-3-0
        /// 装备至少30级防具人数
        /// </summary>
        public const int LGTaskDescription60330 = 1570503113;
        /// <summary>
        /// LG-TaskConfig-Task-62807-Description-604-3-0
        /// 装备至少30级饰品人数
        /// </summary>
        public const int LGTaskDescription60430 = 1303127903;
        /// <summary>
        /// LG-TaskConfig-Task-62808-Description-605-3-0
        /// 通关月影湖6.沼泽边哨
        /// </summary>
        public const int LGTaskDescription60530 = 156774393;
        /// <summary>
        /// LG-TaskConfig-Task-62809-Description-606-3-0
        /// 在领地采集1次资源
        /// </summary>
        public const int LGTaskDescription60630 = 1915451131;
        /// <summary>
        /// LG-TaskConfig-Task-62810-Description-607-3-0
        /// 探索领地秘境难度1第1层
        /// </summary>
        public const int LGTaskDescription60730 = 563165130;
        /// <summary>
        /// LG-TaskConfig-Task-62811-Description-608-3-0
        /// 挑战领地战役关卡1
        /// </summary>
        public const int LGTaskDescription60830 = 1356066864;
        /// <summary>
        /// LG-TaskConfig-Task-62815-Description-612-3-0
        /// 通关月影湖8.灰藤岔路
        /// </summary>
        public const int LGTaskDescription61230 = 792330271;
        /// <summary>
        /// LG-TaskConfig-Task-62903-Description-615-3-0
        /// 升级铁匠铺到8级
        /// </summary>
        public const int LGTaskDescription61530 = 1507322582;
        /// <summary>
        /// LG-TaskConfig-Task-62904-Description-616-3-0
        /// 升级裁缝铺到8级
        /// </summary>
        public const int LGTaskDescription61630 = 1695309922;
        /// <summary>
        /// LG-TaskConfig-Task-62905-Description-617-3-0
        /// 升级炼金商店到8级
        /// </summary>
        public const int LGTaskDescription61730 = 760376406;
        /// <summary>
        /// LG-TaskConfig-Task-62906-Description-618-3-0
        /// 通关月影湖10.雾沼集结
        /// </summary>
        public const int LGTaskDescription61830 = 1479019250;
        /// <summary>
        /// LG-TaskConfig-Task-62909-Description-619-3-0
        /// 装备至少30级绿品武器人数
        /// </summary>
        public const int LGTaskDescription61930 = 1758678672;
        /// <summary>
        /// LG-TaskConfig-Task-62910-Description-620-3-0
        /// 装备至少30级绿品防具人数
        /// </summary>
        public const int LGTaskDescription62030 = 1111974266;
        /// <summary>
        /// LG-TaskConfig-Task-62911-Description-621-3-0
        /// 装备至少30级绿品饰品人数
        /// </summary>
        public const int LGTaskDescription62130 = 773412086;
        /// <summary>
        /// LG-TaskConfig-Task-62912-Description-622-3-0
        /// 通关月影湖12.破网滩涂
        /// </summary>
        public const int LGTaskDescription62230 = 1116947770;
        /// <summary>
        /// LG-TaskConfig-Task-63002-Description-624-3-0
        /// 完成30级橙色稀有度副本
        /// </summary>
        public const int LGTaskDescription62430 = 2066272739;
        /// <summary>
        /// LG-TaskConfig-Task-63003-Description-625-3-0
        /// 装备至少30级橙品装备人数
        /// </summary>
        public const int LGTaskDescription62530 = 161755151;
        /// <summary>
        /// LG-TaskConfig-Task-63005-Description-627-3-0
        /// 使村庄等级达到中级村庄III
        /// </summary>
        public const int LGTaskDescription62730 = 951075387;
        /// <summary>
        /// LG-TaskConfig-Task-63006-Description-628-3-0
        /// 安排14名英雄入驻村庄
        /// </summary>
        public const int LGTaskDescription62830 = 1805705995;
        /// <summary>
        /// LG-TaskConfig-Task-63010-Description-632-3-0
        /// 通关月影湖14.泡沫水洼
        /// </summary>
        public const int LGTaskDescription63230 = 2068222015;
        /// <summary>
        /// LG-TaskConfig-Task-63017-Description-636-3-0
        /// 通关月影湖16.乱鳞巢穴
        /// </summary>
        public const int LGTaskDescription63630 = 868028190;
        /// <summary>
        /// LG-TaskConfig-Task-63103-Description-639-3-0
        /// 升级铁匠铺到9级
        /// </summary>
        public const int LGTaskDescription63930 = 580222753;
        /// <summary>
        /// LG-TaskConfig-Task-63104-Description-640-3-0
        /// 升级裁缝铺到9级
        /// </summary>
        public const int LGTaskDescription64030 = 131734928;
        /// <summary>
        /// LG-TaskConfig-Task-63105-Description-641-3-0
        /// 升级炼金商店到9级
        /// </summary>
        public const int LGTaskDescription64130 = 1673781848;
        /// <summary>
        /// LG-TaskConfig-Task-63106-Description-642-3-0
        /// 通关月影湖18.潮声洞口
        /// </summary>
        public const int LGTaskDescription64230 = 1073656480;
        /// <summary>
        /// LG-TaskConfig-Task-63108-Description-644-3-0
        /// 装备至少30级蓝品武器人数
        /// </summary>
        public const int LGTaskDescription64430 = 94216048;
        /// <summary>
        /// LG-TaskConfig-Task-63109-Description-645-3-0
        /// 装备至少30级蓝品防具人数
        /// </summary>
        public const int LGTaskDescription64530 = 1620632760;
        /// <summary>
        /// LG-TaskConfig-Task-63110-Description-646-3-0
        /// 装备至少30级蓝品饰品人数
        /// </summary>
        public const int LGTaskDescription64630 = 780737667;
        /// <summary>
        /// LG-TaskConfig-Task-63111-Description-647-3-0
        /// 通关月影湖20.鱼人集结
        /// </summary>
        public const int LGTaskDescription64730 = 2121030939;
        /// <summary>
        /// LG-TaskConfig-Task-63202-Description-649-3-0
        /// 使村庄等级达到高级村庄
        /// </summary>
        public const int LGTaskDescription64930 = 1693506892;
        /// <summary>
        /// LG-TaskConfig-Task-63203-Description-650-3-0
        /// 安排15名英雄入驻村庄
        /// </summary>
        public const int LGTaskDescription65030 = 1615189701;
        /// <summary>
        /// LG-TaskConfig-Task-63207-Description-654-3-0
        /// 通关月影湖24.断舟码头
        /// </summary>
        public const int LGTaskDescription65430 = 680184893;
        /// <summary>
        /// LG-TaskConfig-Task-63212-Description-659-3-0
        /// 通关月影湖30.月湖集结
        /// </summary>
        public const int LGTaskDescription65930 = 941284680;
        /// <summary>
        /// LG-TaskConfig-Task-63213-Description-660-3-0
        /// 切换地图到环形山脉
        /// </summary>
        public const int LGTaskDescription66030 = 1902723763;
        /// <summary>
        /// LG-TaskConfig-Task-63214-Description-661-3-0
        /// 升级旅店到31级
        /// </summary>
        public const int LGTaskDescription66130 = 1689266025;
        /// <summary>
        /// LG-TaskConfig-Task-63215-Description-662-3-0
        /// 升级医院到31级
        /// </summary>
        public const int LGTaskDescription66230 = 1685404007;
        /// <summary>
        /// LG-TaskConfig-Task-63302-Description-664-3-0
        /// 在领地升级30次祝福
        /// </summary>
        public const int LGTaskDescription66430 = 2002809414;
        /// <summary>
        /// LG-TaskConfig-Task-63012-Description-666-3-0
        /// 通关圣印之塔70层
        /// </summary>
        public const int LGTaskDescription66630 = 1582200452;
        /// <summary>
        /// LG-TaskConfig-Task-62908-Description-667-3-0
        /// 领地抢夺1次资源
        /// </summary>
        public const int LGTaskDescription66730 = 122810606;
        /// <summary>
        /// LG-TaskConfig-Task-63303-Description-668-3-0
        /// 升级铁匠铺到10级
        /// </summary>
        public const int LGTaskDescription66830 = 479642337;
        /// <summary>
        /// LG-TaskConfig-Task-63304-Description-669-3-0
        /// 升级裁缝铺到10级
        /// </summary>
        public const int LGTaskDescription66930 = 614071539;
        /// <summary>
        /// LG-TaskConfig-Task-63305-Description-670-3-0
        /// 升级炼金商店到10级
        /// </summary>
        public const int LGTaskDescription67030 = 1970197254;
        /// <summary>
        /// LG-TaskConfig-Task-63306-Description-671-3-0
        /// 通关巨骸裂谷4.古木哨点
        /// </summary>
        public const int LGTaskDescription67130 = 236475932;
        /// <summary>
        /// LG-TaskConfig-Task-63307-Description-672-3-0
        /// 使村庄等级达到高级村庄II
        /// </summary>
        public const int LGTaskDescription67230 = 1933884658;
        /// <summary>
        /// LG-TaskConfig-Task-63308-Description-673-3-0
        /// 提升环形山脉探索等级
        /// </summary>
        public const int LGTaskDescription67330 = 776431108;
        /// <summary>
        /// LG-TaskConfig-Task-63309-Description-675-3-0
        /// 通关巨骸裂谷8.暴龙足迹
        /// </summary>
        public const int LGTaskDescription67530 = 733301577;
        /// <summary>
        /// LG-TaskConfig-Task-63402-Description-677-3-0
        /// 升级铁匠铺到11级
        /// </summary>
        public const int LGTaskDescription67730 = 1791083511;
        /// <summary>
        /// LG-TaskConfig-Task-63403-Description-678-3-0
        /// 升级裁缝铺到11级
        /// </summary>
        public const int LGTaskDescription67830 = 2009851537;
        /// <summary>
        /// LG-TaskConfig-Task-63404-Description-679-3-0
        /// 升级炼金商店到11级
        /// </summary>
        public const int LGTaskDescription67930 = 24889651;
        /// <summary>
        /// LG-TaskConfig-Task-63405-Description-681-3-0
        /// 通关巨骸裂谷12.藤叶暗径
        /// </summary>
        public const int LGTaskDescription68130 = 2065012819;
        /// <summary>
        /// LG-TaskConfig-Task-63406-Description-682-3-0
        /// 使村庄等级达到高级村庄III
        /// </summary>
        public const int LGTaskDescription68230 = 2130420331;
        /// <summary>
        /// LG-TaskConfig-Task-63408-Description-684-3-0
        /// 通关巨骸裂谷16.沉木营地
        /// </summary>
        public const int LGTaskDescription68430 = 1968228291;
        /// <summary>
        /// LG-TaskConfig-Task-63502-Description-686-3-0
        /// 升级铁匠铺到12级
        /// </summary>
        public const int LGTaskDescription68630 = 942249904;
        /// <summary>
        /// LG-TaskConfig-Task-63503-Description-687-3-0
        /// 升级裁缝铺到12级
        /// </summary>
        public const int LGTaskDescription68730 = 1626290904;
        /// <summary>
        /// LG-TaskConfig-Task-63504-Description-688-3-0
        /// 升级炼金商店到12级
        /// </summary>
        public const int LGTaskDescription68830 = 898309328;
        /// <summary>
        /// LG-TaskConfig-Task-63505-Description-689-3-0
        /// 通关巨骸裂谷20.雨林集结
        /// </summary>
        public const int LGTaskDescription68930 = 1459701440;
        /// <summary>
        /// LG-TaskConfig-Task-63506-Description-690-3-0
        /// 使村庄等级达到高级村庄IV
        /// </summary>
        public const int LGTaskDescription69030 = 1887150159;
        /// <summary>
        /// LG-TaskConfig-Task-63508-Description-692-3-0
        /// 通关巨骸裂谷30.巨骨集结
        /// </summary>
        public const int LGTaskDescription69230 = 609941223;
        /// <summary>
        /// LG-TaskConfig-Task-63509-Description-693-3-0
        /// 切换地图到埋骨林地
        /// </summary>
        public const int LGTaskDescription69330 = 192106051;
        /// <summary>
        /// LG-TaskConfig-Task-63510-Description-694-3-0
        /// 升级旅店到41级
        /// </summary>
        public const int LGTaskDescription69430 = 560026616;
        /// <summary>
        /// LG-TaskConfig-Task-63511-Description-695-3-0
        /// 升级医院到41级
        /// </summary>
        public const int LGTaskDescription69530 = 9863708;
        /// <summary>
        /// LG-TaskConfig-Task-63602-Description-697-3-0
        /// 升级铁匠铺到13级
        /// </summary>
        public const int LGTaskDescription69730 = 661086447;
        /// <summary>
        /// LG-TaskConfig-Task-63603-Description-698-3-0
        /// 升级裁缝铺到13级
        /// </summary>
        public const int LGTaskDescription69830 = 873977013;
        /// <summary>
        /// LG-TaskConfig-Task-63604-Description-699-3-0
        /// 升级炼金商店到13级
        /// </summary>
        public const int LGTaskDescription69930 = 1493096627;
        /// <summary>
        /// LG-TaskConfig-Task-63605-Description-700-3-0
        /// 通关血色庭院4.乌鸦栅门
        /// </summary>
        public const int LGTaskDescription70030 = 1068969369;
        /// <summary>
        /// LG-TaskConfig-Task-63606-Description-701-3-0
        /// 使村庄等级达到高级村庄V
        /// </summary>
        public const int LGTaskDescription70130 = 761343059;
        /// <summary>
        /// LG-TaskConfig-Task-63607-Description-702-3-0
        /// 提升埋骨林地探索等级
        /// </summary>
        public const int LGTaskDescription70230 = 1535026809;
        /// <summary>
        /// LG-TaskConfig-Task-63608-Description-703-3-0
        /// 通关血色庭院8.白骨岗哨
        /// </summary>
        public const int LGTaskDescription70330 = 2029811447;
        /// <summary>
        /// LG-TaskConfig-Task-63702-Description-705-3-0
        /// 升级铁匠铺到14级
        /// </summary>
        public const int LGTaskDescription70530 = 1060736096;
        /// <summary>
        /// LG-TaskConfig-Task-63703-Description-706-3-0
        /// 升级裁缝铺到14级
        /// </summary>
        public const int LGTaskDescription70630 = 631057174;
        /// <summary>
        /// LG-TaskConfig-Task-63704-Description-707-3-0
        /// 升级炼金商店到14级
        /// </summary>
        public const int LGTaskDescription70730 = 1792594980;
        /// <summary>
        /// LG-TaskConfig-Task-63705-Description-708-3-0
        /// 通关血色庭院12.破旗广场
        /// </summary>
        public const int LGTaskDescription70830 = 630912462;
        /// <summary>
        /// LG-TaskConfig-Task-63706-Description-709-3-0
        /// 使村庄等级达到高级村庄VI
        /// </summary>
        public const int LGTaskDescription70930 = 1666068416;
        /// <summary>
        /// LG-TaskConfig-Task-63708-Description-711-3-0
        /// 通关血色庭院16.圣像残庭
        /// </summary>
        public const int LGTaskDescription71130 = 1736005015;
        /// <summary>
        /// LG-TaskConfig-Task-63802-Description-713-3-0
        /// 升级铁匠铺到15级
        /// </summary>
        public const int LGTaskDescription71330 = 591433274;
        /// <summary>
        /// LG-TaskConfig-Task-63803-Description-714-3-0
        /// 升级裁缝铺到15级
        /// </summary>
        public const int LGTaskDescription71430 = 120423700;
        /// <summary>
        /// LG-TaskConfig-Task-63804-Description-715-3-0
        /// 升级炼金商店到15级
        /// </summary>
        public const int LGTaskDescription71530 = 753319334;
        /// <summary>
        /// LG-TaskConfig-Task-63805-Description-716-3-0
        /// 通关血色庭院20.庭院集结
        /// </summary>
        public const int LGTaskDescription71630 = 1330053172;
        /// <summary>
        /// LG-TaskConfig-Task-63806-Description-717-3-0
        /// 使村庄等级达到高级村庄VII
        /// </summary>
        public const int LGTaskDescription71730 = 1171366762;
        /// <summary>
        /// LG-TaskConfig-Task-63808-Description-719-3-0
        /// 通关圣印之塔145层
        /// </summary>
        public const int LGTaskDescription71930 = 1106083102;
        /// <summary>
        /// LG-TaskConfig-Task-63811-Description-720-3-0
        /// 在领地采集5次资源
        /// </summary>
        public const int LGTaskDescription72030 = 1625443300;
        /// <summary>
        /// LG-TaskConfig-Task-63812-Description-721-3-0
        /// 通关血色庭院24.猩红钟楼
        /// </summary>
        public const int LGTaskDescription72130 = 1041333934;
        /// <summary>
        /// LG-TaskConfig-Task-63813-Description-722-3-0
        /// 提升埋骨林地探索等级到7级
        /// </summary>
        public const int LGTaskDescription72230 = 1823325692;
        /// <summary>
        /// LG-TaskConfig-Task-63814-Description-723-3-0
        /// 通关血色庭院28.暗仪回声
        /// </summary>
        public const int LGTaskDescription72330 = 1723597906;
        /// <summary>
        /// LG-TaskConfig-Task-63815-Description-724-3-0
        /// 提升埋骨林地探索等级到8级
        /// </summary>
        public const int LGTaskDescription72430 = 2054619436;
        /// <summary>
        /// LG-TaskConfig-Task-63816-Description-725-3-0
        /// 通关血色庭院30.血色集结
        /// </summary>
        public const int LGTaskDescription72530 = 806545422;
        /// <summary>
        /// LG-TaskConfig-Task-63817-Description-726-3-0
        /// 切换地图到灰烬矿脊
        /// </summary>
        public const int LGTaskDescription72630 = 733134244;
        /// <summary>
        /// LG-TaskConfig-Task-63818-Description-727-3-0
        /// 通关圣印之塔175层
        /// </summary>
        public const int LGTaskDescription72730 = 891257866;
        /// <summary>
        /// LG-TaskConfig-Task-63819-Description-728-3-0
        /// 升级旅店到51级
        /// </summary>
        public const int LGTaskDescription72830 = 740661444;
        /// <summary>
        /// LG-TaskConfig-Task-63820-Description-729-3-0
        /// 升级医院到51级
        /// </summary>
        public const int LGTaskDescription72930 = 1932829639;
        /// <summary>
        /// LG-TaskConfig-Task-63822-Description-731-3-0
        /// 通关焚骨矿窟4.饿狼岩口
        /// </summary>
        public const int LGTaskDescription73130 = 95424952;
        /// <summary>
        /// LG-TaskConfig-Task-63823-Description-732-3-0
        /// 提升灰烬矿脊探索等级到2级
        /// </summary>
        public const int LGTaskDescription73230 = 1876660470;
        /// <summary>
        /// LG-TaskConfig-Task-63824-Description-733-3-0
        /// 通关焚骨矿窟8.蛛巢矿窟
        /// </summary>
        public const int LGTaskDescription73330 = 2119088328;
        /// <summary>
        /// LG-TaskConfig-Task-63825-Description-734-3-0
        /// 提升灰烬矿脊探索等级到3级
        /// </summary>
        public const int LGTaskDescription73430 = 340527378;
        /// <summary>
        /// LG-TaskConfig-Task-63826-Description-735-3-0
        /// 通关焚骨矿窟12.塌陷矿区
        /// </summary>
        public const int LGTaskDescription73530 = 1803708904;
        /// <summary>
        /// LG-TaskConfig-Task-64263-Description-736-3-0
        /// 使村庄等级达到高级村庄VIII
        /// </summary>
        public const int LGTaskDescription73630 = 837527915;
        /// <summary>
        /// LG-TaskConfig-Task-63827-Description-737-3-0
        /// 提升灰烬矿脊探索等级到4级
        /// </summary>
        public const int LGTaskDescription73730 = 52214185;
        /// <summary>
        /// LG-TaskConfig-Task-63828-Description-738-3-0
        /// 打造任意装备3次
        /// </summary>
        public const int LGTaskDescription73830 = 1134264485;
        /// <summary>
        /// LG-TaskConfig-Task-63829-Description-739-3-0
        /// 通关焚骨矿窟16.悬空矿梯
        /// </summary>
        public const int LGTaskDescription73930 = 649771149;
        /// <summary>
        /// LG-TaskConfig-Task-63830-Description-740-3-0
        /// 提升灰烬矿脊探索等级到5级
        /// </summary>
        public const int LGTaskDescription74030 = 146387923;
        /// <summary>
        /// LG-TaskConfig-Task-63831-Description-741-3-0
        /// 通关焚骨矿窟20.矿窟集结
        /// </summary>
        public const int LGTaskDescription74130 = 1151540571;
        /// <summary>
        /// LG-TaskConfig-Task-64264-Description-742-3-0
        /// 使村庄等级达到高级村庄IX
        /// </summary>
        public const int LGTaskDescription74230 = 970292197;
        /// <summary>
        /// LG-TaskConfig-Task-63832-Description-743-3-0
        /// 提升灰烬矿脊探索等级到6级
        /// </summary>
        public const int LGTaskDescription74330 = 1245765256;
        /// <summary>
        /// LG-TaskConfig-Task-63833-Description-744-3-0
        /// 通关焚骨矿窟24.兽骸残坡
        /// </summary>
        public const int LGTaskDescription74430 = 1939777906;
        /// <summary>
        /// LG-TaskConfig-Task-63834-Description-745-3-0
        /// 提升灰烬矿脊探索等级到7级
        /// </summary>
        public const int LGTaskDescription74530 = 1759816728;
        /// <summary>
        /// LG-TaskConfig-Task-63836-Description-747-3-0
        /// 通关焚骨矿窟28.焚风长廊
        /// </summary>
        public const int LGTaskDescription74730 = 78101568;
        /// <summary>
        /// LG-TaskConfig-Task-63837-Description-748-3-0
        /// 提升灰烬矿脊探索等级到8级
        /// </summary>
        public const int LGTaskDescription74830 = 316478482;
        /// <summary>
        /// LG-TaskConfig-Task-63838-Description-749-3-0
        /// 通关焚骨矿窟30.矿脊集结
        /// </summary>
        public const int LGTaskDescription74930 = 1683806592;
        /// <summary>
        /// LG-TaskConfig-Task-64265-Description-750-3-0
        /// 使村庄等级达到高级村庄X
        /// </summary>
        public const int LGTaskDescription75030 = 256000957;
        /// <summary>
        /// LG-TaskConfig-Task-63839-Description-751-3-0
        /// 切换地图到风暴海岸
        /// </summary>
        public const int LGTaskDescription75130 = 129399958;
        /// <summary>
        /// LG-TaskConfig-Task-63840-Description-752-3-0
        /// 通关圣印之塔205层
        /// </summary>
        public const int LGTaskDescription75230 = 1035675115;
        /// <summary>
        /// LG-TaskConfig-Task-63841-Description-753-3-0
        /// 升级旅店到61级
        /// </summary>
        public const int LGTaskDescription75330 = 646950959;
        /// <summary>
        /// LG-TaskConfig-Task-63842-Description-754-3-0
        /// 升级医院到61级
        /// </summary>
        public const int LGTaskDescription75430 = 668575979;
        /// <summary>
        /// LG-TaskConfig-Task-63844-Description-756-3-0
        /// 通关怒涛海崖4.地精哨卡
        /// </summary>
        public const int LGTaskDescription75630 = 1809157155;
        /// <summary>
        /// LG-TaskConfig-Task-63845-Description-757-3-0
        /// 提升风暴海岸探索等级到2级
        /// </summary>
        public const int LGTaskDescription75730 = 595293183;
        /// <summary>
        /// LG-TaskConfig-Task-63846-Description-758-3-0
        /// 通关怒涛海崖8.海难营地
        /// </summary>
        public const int LGTaskDescription75830 = 1924833859;
        /// <summary>
        /// LG-TaskConfig-Task-63847-Description-759-3-0
        /// 提升风暴海岸探索等级到3级
        /// </summary>
        public const int LGTaskDescription75930 = 974768423;
        /// <summary>
        /// LG-TaskConfig-Task-63848-Description-760-3-0
        /// 通关怒涛海崖12.断桅船骸
        /// </summary>
        public const int LGTaskDescription76030 = 221629310;
        /// <summary>
        /// LG-TaskConfig-Task-63849-Description-761-3-0
        /// 提升风暴海岸探索等级到4级
        /// </summary>
        public const int LGTaskDescription76130 = 1884094738;
        /// <summary>
        /// LG-TaskConfig-Task-63851-Description-763-3-0
        /// 通关怒涛海崖16.珊瑚残礁
        /// </summary>
        public const int LGTaskDescription76330 = 1932144565;
        /// <summary>
        /// LG-TaskConfig-Task-63852-Description-764-3-0
        /// 提升风暴海岸探索等级到5级
        /// </summary>
        public const int LGTaskDescription76430 = 766199309;
        /// <summary>
        /// LG-TaskConfig-Task-63853-Description-765-3-0
        /// 通关怒涛海崖20.海窟集结
        /// </summary>
        public const int LGTaskDescription76530 = 556377793;
        /// <summary>
        /// LG-TaskConfig-Task-64266-Description-766-3-0
        /// 使村庄等级达到无畏要塞I
        /// </summary>
        public const int LGTaskDescription76630 = 1195778629;
        /// <summary>
        /// LG-TaskConfig-Task-63854-Description-767-3-0
        /// 提升风暴海岸探索等级到6级
        /// </summary>
        public const int LGTaskDescription76730 = 1353816638;
        /// <summary>
        /// LG-TaskConfig-Task-63855-Description-768-3-0
        /// 通关怒涛海崖24.悬藤木桥
        /// </summary>
        public const int LGTaskDescription76830 = 966304384;
        /// <summary>
        /// LG-TaskConfig-Task-63856-Description-769-3-0
        /// 提升风暴海岸探索等级到7级
        /// </summary>
        public const int LGTaskDescription76930 = 1981794066;
        /// <summary>
        /// LG-TaskConfig-Task-63857-Description-770-3-0
        /// 通关怒涛海崖28.不朽墓室
        /// </summary>
        public const int LGTaskDescription77030 = 1255644711;
        /// <summary>
        /// LG-TaskConfig-Task-63858-Description-771-3-0
        /// 提升风暴海岸探索等级到8级
        /// </summary>
        public const int LGTaskDescription77130 = 2036988821;
        /// <summary>
        /// LG-TaskConfig-Task-63859-Description-772-3-0
        /// 通关怒涛海崖32.风蚀石林
        /// </summary>
        public const int LGTaskDescription77230 = 77155351;
        /// <summary>
        /// LG-TaskConfig-Task-63860-Description-773-3-0
        /// 提升风暴海岸探索等级到9级
        /// </summary>
        public const int LGTaskDescription77330 = 646217796;
        /// <summary>
        /// LG-TaskConfig-Task-63861-Description-774-3-0
        /// 通关怒涛海崖36.双刃回廊
        /// </summary>
        public const int LGTaskDescription77430 = 1142752438;
        /// <summary>
        /// LG-TaskConfig-Task-63862-Description-775-3-0
        /// 提升风暴海岸探索等级到10级
        /// </summary>
        public const int LGTaskDescription77530 = 1263452828;
        /// <summary>
        /// LG-TaskConfig-Task-63864-Description-777-3-0
        /// 通关怒涛海崖40.风暴集结
        /// </summary>
        public const int LGTaskDescription77730 = 1221292428;
        /// <summary>
        /// LG-TaskConfig-Task-64267-Description-778-3-0
        /// 使村庄等级达到无畏要塞II
        /// </summary>
        public const int LGTaskDescription77830 = 2109058917;
        /// <summary>
        /// LG-TaskConfig-Task-63865-Description-779-3-0
        /// 提升风暴海岸探索等级到11级
        /// </summary>
        public const int LGTaskDescription77930 = 524398265;
        /// <summary>
        /// LG-TaskConfig-Task-63866-Description-780-3-0
        /// 切换地图到赤沙荒原
        /// </summary>
        public const int LGTaskDescription78030 = 612190044;
        /// <summary>
        /// LG-TaskConfig-Task-63867-Description-781-3-0
        /// 通关圣印之塔225层
        /// </summary>
        public const int LGTaskDescription78130 = 1295361820;
        /// <summary>
        /// LG-TaskConfig-Task-63868-Description-782-3-0
        /// 升级旅店到71级
        /// </summary>
        public const int LGTaskDescription78230 = 982954152;
        /// <summary>
        /// LG-TaskConfig-Task-63869-Description-783-3-0
        /// 升级医院到71级
        /// </summary>
        public const int LGTaskDescription78330 = 142537136;
        /// <summary>
        /// LG-TaskConfig-Task-63871-Description-785-3-0
        /// 通关炽日荒垒4.鸟妖巢崖
        /// </summary>
        public const int LGTaskDescription78530 = 318563527;
        /// <summary>
        /// LG-TaskConfig-Task-63872-Description-786-3-0
        /// 提升赤沙荒原探索等级到2级
        /// </summary>
        public const int LGTaskDescription78630 = 983286663;
        /// <summary>
        /// LG-TaskConfig-Task-63873-Description-787-3-0
        /// 通关炽日荒垒8.沙暴哨卡
        /// </summary>
        public const int LGTaskDescription78730 = 795681983;
        /// <summary>
        /// LG-TaskConfig-Task-63874-Description-788-3-0
        /// 提升赤沙荒原探索等级到3级
        /// </summary>
        public const int LGTaskDescription78830 = 348046679;
        /// <summary>
        /// LG-TaskConfig-Task-63875-Description-789-3-0
        /// 通关炽日荒垒12.兽群围栏
        /// </summary>
        public const int LGTaskDescription78930 = 2133971415;
        /// <summary>
        /// LG-TaskConfig-Task-63876-Description-790-3-0
        /// 提升赤沙荒原探索等级到4级
        /// </summary>
        public const int LGTaskDescription79030 = 1993236892;
        /// <summary>
        /// LG-TaskConfig-Task-63878-Description-792-3-0
        /// 通关炽日荒垒16.图腾石阵
        /// </summary>
        public const int LGTaskDescription79230 = 12094620;
        /// <summary>
        /// LG-TaskConfig-Task-63879-Description-793-3-0
        /// 提升赤沙荒原探索等级到5级
        /// </summary>
        public const int LGTaskDescription79330 = 453169392;
        /// <summary>
        /// LG-TaskConfig-Task-63880-Description-794-3-0
        /// 通关炽日荒垒20.部落集结
        /// </summary>
        public const int LGTaskDescription79430 = 1444801053;
        /// <summary>
        /// LG-TaskConfig-Task-64268-Description-795-3-0
        /// 使村庄等级达到无畏要塞III
        /// </summary>
        public const int LGTaskDescription79530 = 109981717;
        /// <summary>
        /// LG-TaskConfig-Task-63881-Description-796-3-0
        /// 提升赤沙荒原探索等级到6级
        /// </summary>
        public const int LGTaskDescription79630 = 1922576920;
        /// <summary>
        /// LG-TaskConfig-Task-63882-Description-797-3-0
        /// 通关炽日荒垒24.兽栅残迹
        /// </summary>
        public const int LGTaskDescription79730 = 510518558;
        /// <summary>
        /// LG-TaskConfig-Task-63883-Description-798-3-0
        /// 提升赤沙荒原探索等级到7级
        /// </summary>
        public const int LGTaskDescription79830 = 1195285504;
        /// <summary>
        /// LG-TaskConfig-Task-63884-Description-799-3-0
        /// 通关炽日荒垒28.掩尘遗迹
        /// </summary>
        public const int LGTaskDescription79930 = 1452922714;
        /// <summary>
        /// LG-TaskConfig-Task-63885-Description-800-3-0
        /// 提升赤沙荒原探索等级到8级
        /// </summary>
        public const int LGTaskDescription80030 = 572518366;
        /// <summary>
        /// LG-TaskConfig-Task-63886-Description-801-3-0
        /// 通关炽日荒垒32.炙阳石林
        /// </summary>
        public const int LGTaskDescription80130 = 2128749840;
        /// <summary>
        /// LG-TaskConfig-Task-63887-Description-802-3-0
        /// 提升赤沙荒原探索等级到9级
        /// </summary>
        public const int LGTaskDescription80230 = 280242830;
        /// <summary>
        /// LG-TaskConfig-Task-63888-Description-803-3-0
        /// 通关炽日荒垒36.弹坑凹地
        /// </summary>
        public const int LGTaskDescription80330 = 438775524;
        /// <summary>
        /// LG-TaskConfig-Task-63889-Description-804-3-0
        /// 提升赤沙荒原探索等级到10级
        /// </summary>
        public const int LGTaskDescription80430 = 1569091262;
        /// <summary>
        /// LG-TaskConfig-Task-63891-Description-806-3-0
        /// 通关炽日荒垒40.赤沙集结
        /// </summary>
        public const int LGTaskDescription80630 = 10000237;
        /// <summary>
        /// LG-TaskConfig-Task-64269-Description-807-3-0
        /// 使村庄等级达到无畏要塞IV
        /// </summary>
        public const int LGTaskDescription80730 = 711516572;
        /// <summary>
        /// LG-TaskConfig-Task-63892-Description-808-3-0
        /// 提升赤沙荒原探索等级到11级
        /// </summary>
        public const int LGTaskDescription80830 = 333385006;
        /// <summary>
        /// LG-TaskConfig-Task-63893-Description-809-3-0
        /// 切换地图到冰霜高地
        /// </summary>
        public const int LGTaskDescription80930 = 471537438;
        /// <summary>
        /// LG-TaskConfig-Task-63894-Description-810-3-0
        /// 通关圣印之塔250层
        /// </summary>
        public const int LGTaskDescription81030 = 894086029;
        /// <summary>
        /// LG-TaskConfig-Task-63895-Description-811-3-0
        /// 升级旅店到81级
        /// </summary>
        public const int LGTaskDescription81130 = 1976023585;
        /// <summary>
        /// LG-TaskConfig-Task-63896-Description-812-3-0
        /// 升级医院到81级
        /// </summary>
        public const int LGTaskDescription81230 = 519856513;
        /// <summary>
        /// LG-TaskConfig-Task-63898-Description-814-3-0
        /// 通关霜冠孤峰4.冰晶石柱
        /// </summary>
        public const int LGTaskDescription81430 = 1109704093;
        /// <summary>
        /// LG-TaskConfig-Task-63899-Description-815-3-0
        /// 提升冰霜高地探索等级到2级
        /// </summary>
        public const int LGTaskDescription81530 = 962845689;
        /// <summary>
        /// LG-TaskConfig-Task-63900-Description-816-3-0
        /// 通关霜冠孤峰8.深谷兽穴
        /// </summary>
        public const int LGTaskDescription81630 = 1930283941;
        /// <summary>
        /// LG-TaskConfig-Task-63901-Description-817-3-0
        /// 提升冰霜高地探索等级到3级
        /// </summary>
        public const int LGTaskDescription81730 = 1880313017;
        /// <summary>
        /// LG-TaskConfig-Task-63902-Description-818-3-0
        /// 通关霜冠孤峰12.封冻浅湾
        /// </summary>
        public const int LGTaskDescription81830 = 1441912669;
        /// <summary>
        /// LG-TaskConfig-Task-63903-Description-819-3-0
        /// 提升冰霜高地探索等级到4级
        /// </summary>
        public const int LGTaskDescription81930 = 203106033;
        /// <summary>
        /// LG-TaskConfig-Task-63905-Description-821-3-0
        /// 通关霜冠孤峰16.雾凇哨口
        /// </summary>
        public const int LGTaskDescription82130 = 695890564;
        /// <summary>
        /// LG-TaskConfig-Task-63906-Description-822-3-0
        /// 提升冰霜高地探索等级到5级
        /// </summary>
        public const int LGTaskDescription82230 = 2122730768;
        /// <summary>
        /// LG-TaskConfig-Task-63907-Description-823-3-0
        /// 通关霜冠孤峰20.冰湖集结
        /// </summary>
        public const int LGTaskDescription82330 = 595046036;
        /// <summary>
        /// LG-TaskConfig-Task-64270-Description-824-3-0
        /// 使村庄等级达到无畏要塞V
        /// </summary>
        public const int LGTaskDescription82430 = 996688081;
        /// <summary>
        /// LG-TaskConfig-Task-63908-Description-825-3-0
        /// 提升冰霜高地探索等级到6级
        /// </summary>
        public const int LGTaskDescription82530 = 1799477091;
        /// <summary>
        /// LG-TaskConfig-Task-63909-Description-826-3-0
        /// 通关霜冠孤峰24.悬索吊桥
        /// </summary>
        public const int LGTaskDescription82630 = 1074814917;
        /// <summary>
        /// LG-TaskConfig-Task-63910-Description-827-3-0
        /// 提升冰霜高地探索等级到7级
        /// </summary>
        public const int LGTaskDescription82730 = 1397433908;
        /// <summary>
        /// LG-TaskConfig-Task-63911-Description-828-3-0
        /// 通关霜冠孤峰28.冷雾回廊
        /// </summary>
        public const int LGTaskDescription82830 = 2091949426;
        /// <summary>
        /// LG-TaskConfig-Task-63912-Description-829-3-0
        /// 提升冰霜高地探索等级到8级
        /// </summary>
        public const int LGTaskDescription82930 = 299459488;
        /// <summary>
        /// LG-TaskConfig-Task-63913-Description-830-3-0
        /// 通关霜冠孤峰32.霜牙石丛
        /// </summary>
        public const int LGTaskDescription83030 = 1512821245;
        /// <summary>
        /// LG-TaskConfig-Task-63914-Description-831-3-0
        /// 提升冰霜高地探索等级到9级
        /// </summary>
        public const int LGTaskDescription83130 = 794642631;
        /// <summary>
        /// LG-TaskConfig-Task-63915-Description-832-3-0
        /// 通关霜冠孤峰36.霜风长廊
        /// </summary>
        public const int LGTaskDescription83230 = 1524694141;
        /// <summary>
        /// LG-TaskConfig-Task-63916-Description-833-3-0
        /// 提升冰霜高地探索等级到10级
        /// </summary>
        public const int LGTaskDescription83330 = 949992963;
        /// <summary>
        /// LG-TaskConfig-Task-63918-Description-835-3-0
        /// 通关霜冠孤峰40.冰霜集结
        /// </summary>
        public const int LGTaskDescription83530 = 1404231791;
        /// <summary>
        /// LG-TaskConfig-Task-64271-Description-836-3-0
        /// 使村庄等级达到无畏要塞VI
        /// </summary>
        public const int LGTaskDescription83630 = 1764951313;
        /// <summary>
        /// LG-TaskConfig-Task-63919-Description-837-3-0
        /// 提升冰霜高地探索等级到11级
        /// </summary>
        public const int LGTaskDescription83730 = 1899710546;
        /// <summary>
        /// LG-TaskConfig-Task-63920-Description-838-3-0
        /// 切换地图到巨魔台地
        /// </summary>
        public const int LGTaskDescription83830 = 1330348107;
        /// <summary>
        /// LG-TaskConfig-Task-63921-Description-839-3-0
        /// 通关圣印之塔280层
        /// </summary>
        public const int LGTaskDescription83930 = 1077193435;
        /// <summary>
        /// LG-TaskConfig-Task-63922-Description-840-3-0
        /// 升级旅店到91级
        /// </summary>
        public const int LGTaskDescription84030 = 951515270;
        /// <summary>
        /// LG-TaskConfig-Task-63923-Description-841-3-0
        /// 升级医院到91级
        /// </summary>
        public const int LGTaskDescription84130 = 1865882246;
        /// <summary>
        /// LG-TaskConfig-Task-63925-Description-843-3-0
        /// 通关颅骨高台4.兽人哨棚
        /// </summary>
        public const int LGTaskDescription84330 = 1495790730;
        /// <summary>
        /// LG-TaskConfig-Task-63926-Description-844-3-0
        /// 提升巨魔台地探索等级到2级
        /// </summary>
        public const int LGTaskDescription84430 = 1296746222;
        /// <summary>
        /// LG-TaskConfig-Task-63927-Description-845-3-0
        /// 通关颅骨高台8.颅骨栅墙
        /// </summary>
        public const int LGTaskDescription84530 = 1434898654;
        /// <summary>
        /// LG-TaskConfig-Task-63928-Description-846-3-0
        /// 提升巨魔台地探索等级到3级
        /// </summary>
        public const int LGTaskDescription84630 = 88471962;
        /// <summary>
        /// LG-TaskConfig-Task-63929-Description-847-3-0
        /// 通关颅骨高台12.烟熏铁炉
        /// </summary>
        public const int LGTaskDescription84730 = 973158122;
        /// <summary>
        /// LG-TaskConfig-Task-63930-Description-848-3-0
        /// 提升巨魔台地探索等级到4级
        /// </summary>
        public const int LGTaskDescription84830 = 1028767285;
        /// <summary>
        /// LG-TaskConfig-Task-63932-Description-850-3-0
        /// 通关颅骨高台16.囚笼围场
        /// </summary>
        public const int LGTaskDescription85030 = 1576594742;
        /// <summary>
        /// LG-TaskConfig-Task-63933-Description-851-3-0
        /// 提升巨魔台地探索等级到5级
        /// </summary>
        public const int LGTaskDescription85130 = 1622765066;
        /// <summary>
        /// LG-TaskConfig-Task-63934-Description-852-3-0
        /// 通关颅骨高台20.村寨集结
        /// </summary>
        public const int LGTaskDescription85230 = 1705926054;
        /// <summary>
        /// LG-TaskConfig-Task-64272-Description-853-3-0
        /// 使村庄等级达到无畏要塞VII
        /// </summary>
        public const int LGTaskDescription85330 = 566434709;
        /// <summary>
        /// LG-TaskConfig-Task-63935-Description-854-3-0
        /// 提升巨魔台地探索等级到6级
        /// </summary>
        public const int LGTaskDescription85430 = 1793724123;
        /// <summary>
        /// LG-TaskConfig-Task-63936-Description-855-3-0
        /// 通关颅骨高台24.龙牙陡阶
        /// </summary>
        public const int LGTaskDescription85530 = 979040073;
        /// <summary>
        /// LG-TaskConfig-Task-63937-Description-856-3-0
        /// 提升巨魔台地探索等级到7级
        /// </summary>
        public const int LGTaskDescription85630 = 1978109695;
        /// <summary>
        /// LG-TaskConfig-Task-63938-Description-857-3-0
        /// 通关颅骨高台28.骸骨谷地
        /// </summary>
        public const int LGTaskDescription85730 = 243784505;
        /// <summary>
        /// LG-TaskConfig-Task-63939-Description-858-3-0
        /// 提升巨魔台地探索等级到8级
        /// </summary>
        public const int LGTaskDescription85830 = 1180443867;
        /// <summary>
        /// LG-TaskConfig-Task-63940-Description-859-3-0
        /// 通关颅骨高台32.哨卫石像
        /// </summary>
        public const int LGTaskDescription85930 = 2142376656;
        /// <summary>
        /// LG-TaskConfig-Task-63941-Description-860-3-0
        /// 提升巨魔台地探索等级到9级
        /// </summary>
        public const int LGTaskDescription86030 = 2008065331;
        /// <summary>
        /// LG-TaskConfig-Task-63942-Description-861-3-0
        /// 通关颅骨高台36.残偶废坑
        /// </summary>
        public const int LGTaskDescription86130 = 552403461;
        /// <summary>
        /// LG-TaskConfig-Task-63943-Description-862-3-0
        /// 提升巨魔台地探索等级到10级
        /// </summary>
        public const int LGTaskDescription86230 = 1308941435;
        /// <summary>
        /// LG-TaskConfig-Task-63945-Description-864-3-0
        /// 通关颅骨高台40.台地集结
        /// </summary>
        public const int LGTaskDescription86430 = 605328355;
        /// <summary>
        /// LG-TaskConfig-Task-64273-Description-865-3-0
        /// 使村庄等级达到无畏要塞VIII
        /// </summary>
        public const int LGTaskDescription86530 = 62611915;
        /// <summary>
        /// LG-TaskConfig-Task-63946-Description-866-3-0
        /// 提升巨魔台地探索等级到11级
        /// </summary>
        public const int LGTaskDescription86630 = 1172055188;
        /// <summary>
        /// LG-TaskConfig-Task-63947-Description-867-3-0
        /// 切换地图到月影废墟
        /// </summary>
        public const int LGTaskDescription86730 = 1124990312;
        /// <summary>
        /// LG-TaskConfig-Task-63948-Description-868-3-0
        /// 通关圣印之塔315层
        /// </summary>
        public const int LGTaskDescription86830 = 907115964;
        /// <summary>
        /// LG-TaskConfig-Task-63949-Description-869-3-0
        /// 升级旅店到101级
        /// </summary>
        public const int LGTaskDescription86930 = 617462448;
        /// <summary>
        /// LG-TaskConfig-Task-63950-Description-870-3-0
        /// 升级医院到101级
        /// </summary>
        public const int LGTaskDescription87030 = 973784328;
        /// <summary>
        /// LG-TaskConfig-Task-63952-Description-872-3-0
        /// 通关月影废墟5.崖边货栈
        /// </summary>
        public const int LGTaskDescription87230 = 1395237836;
        /// <summary>
        /// LG-TaskConfig-Task-63953-Description-873-3-0
        /// 提升月影废墟探索等级到2级
        /// </summary>
        public const int LGTaskDescription87330 = 1249379116;
        /// <summary>
        /// LG-TaskConfig-Task-63954-Description-874-3-0
        /// 通关月影废墟10.崖脚集结
        /// </summary>
        public const int LGTaskDescription87430 = 530313928;
        /// <summary>
        /// LG-TaskConfig-Task-63955-Description-875-3-0
        /// 提升月影废墟探索等级到3级
        /// </summary>
        public const int LGTaskDescription87530 = 1109510512;
        /// <summary>
        /// LG-TaskConfig-Task-63956-Description-876-3-0
        /// 通关月影废墟15.龙骨梁桥
        /// </summary>
        public const int LGTaskDescription87630 = 867619572;
        /// <summary>
        /// LG-TaskConfig-Task-63957-Description-877-3-0
        /// 提升月影废墟探索等级到4级
        /// </summary>
        public const int LGTaskDescription87730 = 151865732;
        /// <summary>
        /// LG-TaskConfig-Task-63959-Description-879-3-0
        /// 通关月影废墟20.村寨集结
        /// </summary>
        public const int LGTaskDescription87930 = 1673579280;
        /// <summary>
        /// LG-TaskConfig-Task-64274-Description-880-3-0
        /// 使村庄等级达到无畏要塞IX
        /// </summary>
        public const int LGTaskDescription88030 = 1843351583;
        /// <summary>
        /// LG-TaskConfig-Task-63960-Description-881-3-0
        /// 提升月影废墟探索等级到5级
        /// </summary>
        public const int LGTaskDescription88130 = 285227619;
        /// <summary>
        /// LG-TaskConfig-Task-63961-Description-882-3-0
        /// 通关月影废墟25.腐骨祭坑
        /// </summary>
        public const int LGTaskDescription88230 = 1283457089;
        /// <summary>
        /// LG-TaskConfig-Task-63962-Description-883-3-0
        /// 提升月影废墟探索等级到6级
        /// </summary>
        public const int LGTaskDescription88330 = 2140121571;
        /// <summary>
        /// LG-TaskConfig-Task-63963-Description-884-3-0
        /// 通关月影废墟30.龙骨集结
        /// </summary>
        public const int LGTaskDescription88430 = 2142698581;
        /// <summary>
        /// LG-TaskConfig-Task-63964-Description-885-3-0
        /// 提升月影废墟探索等级到7级
        /// </summary>
        public const int LGTaskDescription88530 = 2002427515;
        /// <summary>
        /// LG-TaskConfig-Task-63965-Description-886-3-0
        /// 通关月影废墟35.石偶工坊
        /// </summary>
        public const int LGTaskDescription88630 = 1263342313;
        /// <summary>
        /// LG-TaskConfig-Task-63966-Description-887-3-0
        /// 提升月影废墟探索等级到8级
        /// </summary>
        public const int LGTaskDescription88730 = 102616875;
        /// <summary>
        /// LG-TaskConfig-Task-63967-Description-888-3-0
        /// 通关月影废墟40.台地集结
        /// </summary>
        public const int LGTaskDescription88830 = 929625269;
        /// <summary>
        /// LG-TaskConfig-Task-64275-Description-889-3-0
        /// 使村庄等级达到无畏要塞X
        /// </summary>
        public const int LGTaskDescription88930 = 804669471;
        /// <summary>
        /// LG-TaskConfig-Task-63968-Description-890-3-0
        /// 提升月影废墟探索等级到9级
        /// </summary>
        public const int LGTaskDescription89030 = 1970412065;
        /// <summary>
        /// LG-TaskConfig-Task-63969-Description-891-3-0
        /// 通关月影废墟45.台地集结
        /// </summary>
        public const int LGTaskDescription89130 = 1437115205;
        /// <summary>
        /// LG-TaskConfig-Task-63970-Description-892-3-0
        /// 提升月影废墟探索等级到10级
        /// </summary>
        public const int LGTaskDescription89230 = 1230974274;
        /// <summary>
        /// LG-TaskConfig-Task-63972-Description-894-3-0
        /// 通关月影废墟50.台地集结
        /// </summary>
        public const int LGTaskDescription89430 = 778965058;
        /// <summary>
        /// LG-TaskConfig-Task-63973-Description-895-3-0
        /// 提升月影废墟探索等级到11级
        /// </summary>
        public const int LGTaskDescription89530 = 627000246;
        /// <summary>
        /// LG-TaskConfig-Task-63974-Description-896-3-0
        /// 切换地图到断矛峡谷
        /// </summary>
        public const int LGTaskDescription89630 = 349779722;
        /// <summary>
        /// LG-TaskConfig-Task-63975-Description-897-3-0
        /// 通关圣印之塔350层
        /// </summary>
        public const int LGTaskDescription89730 = 506796358;
        /// <summary>
        /// LG-TaskConfig-Task-63976-Description-898-3-0
        /// 升级旅店到111级
        /// </summary>
        public const int LGTaskDescription89830 = 1122802618;
        /// <summary>
        /// LG-TaskConfig-Task-63977-Description-899-3-0
        /// 升级医院到111级
        /// </summary>
        public const int LGTaskDescription89930 = 1820760678;
        /// <summary>
        /// LG-TaskConfig-Task-63979-Description-901-3-0
        /// 通关断矛峡谷5.崖边货栈
        /// </summary>
        public const int LGTaskDescription90130 = 157469370;
        /// <summary>
        /// LG-TaskConfig-Task-63980-Description-902-3-0
        /// 提升断矛峡谷探索等级到2级
        /// </summary>
        public const int LGTaskDescription90230 = 1633084559;
        /// <summary>
        /// LG-TaskConfig-Task-63981-Description-903-3-0
        /// 通关断矛峡谷10.崖脚集结
        /// </summary>
        public const int LGTaskDescription90330 = 1979260767;
        /// <summary>
        /// LG-TaskConfig-Task-63982-Description-904-3-0
        /// 提升断矛峡谷探索等级到3级
        /// </summary>
        public const int LGTaskDescription90430 = 1693726819;
        /// <summary>
        /// LG-TaskConfig-Task-63983-Description-905-3-0
        /// 通关断矛峡谷15.龙骨梁桥
        /// </summary>
        public const int LGTaskDescription90530 = 549370491;
        /// <summary>
        /// LG-TaskConfig-Task-63984-Description-906-3-0
        /// 提升断矛峡谷探索等级到4级
        /// </summary>
        public const int LGTaskDescription90630 = 439237351;
        /// <summary>
        /// LG-TaskConfig-Task-63986-Description-908-3-0
        /// 通关断矛峡谷20.村寨集结
        /// </summary>
        public const int LGTaskDescription90830 = 1944374387;
        /// <summary>
        /// LG-TaskConfig-Task-64276-Description-909-3-0
        /// 使村庄等级达到暴风之城I
        /// </summary>
        public const int LGTaskDescription90930 = 287593765;
        /// <summary>
        /// LG-TaskConfig-Task-63987-Description-910-3-0
        /// 提升断矛峡谷探索等级到5级
        /// </summary>
        public const int LGTaskDescription91030 = 1006025305;
        /// <summary>
        /// LG-TaskConfig-Task-63988-Description-911-3-0
        /// 通关断矛峡谷25.腐骨祭坑
        /// </summary>
        public const int LGTaskDescription91130 = 372462151;
        /// <summary>
        /// LG-TaskConfig-Task-63989-Description-912-3-0
        /// 提升断矛峡谷探索等级到6级
        /// </summary>
        public const int LGTaskDescription91230 = 817446897;
        /// <summary>
        /// LG-TaskConfig-Task-63990-Description-913-3-0
        /// 通关断矛峡谷30.龙骨集结
        /// </summary>
        public const int LGTaskDescription91330 = 1852805844;
        /// <summary>
        /// LG-TaskConfig-Task-63991-Description-914-3-0
        /// 提升断矛峡谷探索等级到7级
        /// </summary>
        public const int LGTaskDescription91430 = 198782010;
        /// <summary>
        /// LG-TaskConfig-Task-63992-Description-915-3-0
        /// 通关断矛峡谷35.石偶工坊
        /// </summary>
        public const int LGTaskDescription91530 = 235669228;
        /// <summary>
        /// LG-TaskConfig-Task-63993-Description-916-3-0
        /// 提升断矛峡谷探索等级到8级
        /// </summary>
        public const int LGTaskDescription91630 = 1362142966;
        /// <summary>
        /// LG-TaskConfig-Task-63994-Description-917-3-0
        /// 通关断矛峡谷40.台地集结
        /// </summary>
        public const int LGTaskDescription91730 = 1041331484;
        /// <summary>
        /// LG-TaskConfig-Task-64277-Description-918-3-0
        /// 使村庄等级达到暴风之城II
        /// </summary>
        public const int LGTaskDescription91830 = 1212534988;
        /// <summary>
        /// LG-TaskConfig-Task-63995-Description-919-3-0
        /// 提升断矛峡谷探索等级到9级
        /// </summary>
        public const int LGTaskDescription91930 = 903239909;
        /// <summary>
        /// LG-TaskConfig-Task-63996-Description-920-3-0
        /// 通关断矛峡谷45.台地集结
        /// </summary>
        public const int LGTaskDescription92030 = 1783265056;
        /// <summary>
        /// LG-TaskConfig-Task-63997-Description-921-3-0
        /// 提升断矛峡谷探索等级到10级
        /// </summary>
        public const int LGTaskDescription92130 = 1152554628;
        /// <summary>
        /// LG-TaskConfig-Task-63999-Description-923-3-0
        /// 通关断矛峡谷50.台地集结
        /// </summary>
        public const int LGTaskDescription92330 = 1645489256;
        /// <summary>
        /// LG-TaskConfig-Task-64000-Description-924-3-0
        /// 提升断矛峡谷探索等级到11级
        /// </summary>
        public const int LGTaskDescription92430 = 499730479;
        /// <summary>
        /// LG-TaskConfig-Task-64001-Description-925-3-0
        /// 切换地图到黑铁堡垒
        /// </summary>
        public const int LGTaskDescription92530 = 1145054403;
        /// <summary>
        /// LG-TaskConfig-Task-64002-Description-926-3-0
        /// 通关圣印之塔385层
        /// </summary>
        public const int LGTaskDescription92630 = 1832579319;
        /// <summary>
        /// LG-TaskConfig-Task-64003-Description-927-3-0
        /// 升级旅店到121级
        /// </summary>
        public const int LGTaskDescription92730 = 395966107;
        /// <summary>
        /// LG-TaskConfig-Task-64004-Description-928-3-0
        /// 升级医院到121级
        /// </summary>
        public const int LGTaskDescription92830 = 1543096071;
        /// <summary>
        /// LG-TaskConfig-Task-64006-Description-930-3-0
        /// 通关黑铁堡垒5.崖边货栈
        /// </summary>
        public const int LGTaskDescription93030 = 651435492;
        /// <summary>
        /// LG-TaskConfig-Task-64007-Description-931-3-0
        /// 提升黑铁堡垒探索等级到2级
        /// </summary>
        public const int LGTaskDescription93130 = 1058631956;
        /// <summary>
        /// LG-TaskConfig-Task-64008-Description-932-3-0
        /// 通关黑铁堡垒10.崖脚集结
        /// </summary>
        public const int LGTaskDescription93230 = 1450560932;
        /// <summary>
        /// LG-TaskConfig-Task-64009-Description-933-3-0
        /// 提升黑铁堡垒探索等级到3级
        /// </summary>
        public const int LGTaskDescription93330 = 670557188;
        /// <summary>
        /// LG-TaskConfig-Task-64010-Description-934-3-0
        /// 通关黑铁堡垒15.龙骨梁桥
        /// </summary>
        public const int LGTaskDescription93430 = 1336578631;
        /// <summary>
        /// LG-TaskConfig-Task-64011-Description-935-3-0
        /// 提升黑铁堡垒探索等级到4级
        /// </summary>
        public const int LGTaskDescription93530 = 212904599;
        /// <summary>
        /// LG-TaskConfig-Task-64013-Description-937-3-0
        /// 通关黑铁堡垒20.村寨集结
        /// </summary>
        public const int LGTaskDescription93730 = 48377315;
        /// <summary>
        /// LG-TaskConfig-Task-64278-Description-938-3-0
        /// 使村庄等级达到暴风之城III
        /// </summary>
        public const int LGTaskDescription93830 = 566265095;
        /// <summary>
        /// LG-TaskConfig-Task-64014-Description-939-3-0
        /// 提升黑铁堡垒探索等级到5级
        /// </summary>
        public const int LGTaskDescription93930 = 1530202796;
        /// <summary>
        /// LG-TaskConfig-Task-64015-Description-940-3-0
        /// 通关黑铁堡垒25.腐骨祭坑
        /// </summary>
        public const int LGTaskDescription94030 = 1882183423;
        /// <summary>
        /// LG-TaskConfig-Task-64016-Description-941-3-0
        /// 提升黑铁堡垒探索等级到6级
        /// </summary>
        public const int LGTaskDescription94130 = 1734262685;
        /// <summary>
        /// LG-TaskConfig-Task-64017-Description-942-3-0
        /// 通关黑铁堡垒30.龙骨集结
        /// </summary>
        public const int LGTaskDescription94230 = 849687287;
        /// <summary>
        /// LG-TaskConfig-Task-64018-Description-943-3-0
        /// 提升黑铁堡垒探索等级到7级
        /// </summary>
        public const int LGTaskDescription94330 = 592087705;
        /// <summary>
        /// LG-TaskConfig-Task-64019-Description-944-3-0
        /// 通关黑铁堡垒35.石偶工坊
        /// </summary>
        public const int LGTaskDescription94430 = 728005647;
        /// <summary>
        /// LG-TaskConfig-Task-64020-Description-945-3-0
        /// 提升黑铁堡垒探索等级到8级
        /// </summary>
        public const int LGTaskDescription94530 = 432922716;
        /// <summary>
        /// LG-TaskConfig-Task-64021-Description-946-3-0
        /// 通关黑铁堡垒40.台地集结
        /// </summary>
        public const int LGTaskDescription94630 = 1573952242;
        /// <summary>
        /// LG-TaskConfig-Task-64022-Description-947-3-0
        /// 提升黑铁堡垒探索等级到9级
        /// </summary>
        public const int LGTaskDescription94730 = 1151986036;
        /// <summary>
        /// LG-TaskConfig-Task-64023-Description-948-3-0
        /// 通关黑铁堡垒45.台地集结
        /// </summary>
        public const int LGTaskDescription94830 = 1502880326;
        /// <summary>
        /// LG-TaskConfig-Task-64024-Description-949-3-0
        /// 提升黑铁堡垒探索等级到10级
        /// </summary>
        public const int LGTaskDescription94930 = 450409420;
        /// <summary>
        /// LG-TaskConfig-Task-64026-Description-951-3-0
        /// 通关黑铁堡垒50.台地集结
        /// </summary>
        public const int LGTaskDescription95130 = 996405599;
        /// <summary>
        /// LG-TaskConfig-Task-64279-Description-952-3-0
        /// 使村庄等级达到暴风之城IV
        /// </summary>
        public const int LGTaskDescription95230 = 173311648;
        /// <summary>
        /// LG-TaskConfig-Task-64027-Description-953-3-0
        /// 提升黑铁堡垒探索等级到11级
        /// </summary>
        public const int LGTaskDescription95330 = 2129337478;
        /// <summary>
        /// LG-TaskConfig-Task-64028-Description-954-3-0
        /// 切换地图到迷雾群岛
        /// </summary>
        public const int LGTaskDescription95430 = 1554125006;
        /// <summary>
        /// LG-TaskConfig-Task-64029-Description-955-3-0
        /// 通关圣印之塔420层
        /// </summary>
        public const int LGTaskDescription95530 = 257021578;
        /// <summary>
        /// LG-TaskConfig-Task-64030-Description-956-3-0
        /// 升级旅店到131级
        /// </summary>
        public const int LGTaskDescription95630 = 1081441269;
        /// <summary>
        /// LG-TaskConfig-Task-64031-Description-957-3-0
        /// 升级医院到131级
        /// </summary>
        public const int LGTaskDescription95730 = 1420660425;
        /// <summary>
        /// LG-TaskConfig-Task-64033-Description-959-3-0
        /// 通关迷雾群岛5.崖边货栈
        /// </summary>
        public const int LGTaskDescription95930 = 1849764417;
        /// <summary>
        /// LG-TaskConfig-Task-64034-Description-960-3-0
        /// 提升迷雾群岛探索等级到2级
        /// </summary>
        public const int LGTaskDescription96030 = 1736139792;
        /// <summary>
        /// LG-TaskConfig-Task-64035-Description-961-3-0
        /// 通关迷雾群岛10.崖脚集结
        /// </summary>
        public const int LGTaskDescription96130 = 1865922148;
        /// <summary>
        /// LG-TaskConfig-Task-64036-Description-962-3-0
        /// 提升迷雾群岛探索等级到3级
        /// </summary>
        public const int LGTaskDescription96230 = 200060744;
        /// <summary>
        /// LG-TaskConfig-Task-64037-Description-963-3-0
        /// 通关迷雾群岛15.龙骨梁桥
        /// </summary>
        public const int LGTaskDescription96330 = 2033795444;
        /// <summary>
        /// LG-TaskConfig-Task-64038-Description-964-3-0
        /// 提升迷雾群岛探索等级到4级
        /// </summary>
        public const int LGTaskDescription96430 = 1786681232;
        /// <summary>
        /// LG-TaskConfig-Task-64040-Description-966-3-0
        /// 通关迷雾群岛20.村寨集结
        /// </summary>
        public const int LGTaskDescription96630 = 976721853;
        /// <summary>
        /// LG-TaskConfig-Task-64041-Description-967-3-0
        /// 提升迷雾群岛探索等级到5级
        /// </summary>
        public const int LGTaskDescription96730 = 1797110113;
        /// <summary>
        /// LG-TaskConfig-Task-64042-Description-968-3-0
        /// 通关迷雾群岛25.腐骨祭坑
        /// </summary>
        public const int LGTaskDescription96830 = 897957353;
        /// <summary>
        /// LG-TaskConfig-Task-64043-Description-969-3-0
        /// 提升迷雾群岛探索等级到6级
        /// </summary>
        public const int LGTaskDescription96930 = 1947120765;
        /// <summary>
        /// LG-TaskConfig-Task-64044-Description-970-3-0
        /// 通关迷雾群岛30.龙骨集结
        /// </summary>
        public const int LGTaskDescription97030 = 214819174;
        /// <summary>
        /// LG-TaskConfig-Task-64045-Description-971-3-0
        /// 提升迷雾群岛探索等级到7级
        /// </summary>
        public const int LGTaskDescription97130 = 403852486;
        /// <summary>
        /// LG-TaskConfig-Task-64046-Description-972-3-0
        /// 通关迷雾群岛35.石偶工坊
        /// </summary>
        public const int LGTaskDescription97230 = 479289642;
        /// <summary>
        /// LG-TaskConfig-Task-64047-Description-973-3-0
        /// 提升迷雾群岛探索等级到8级
        /// </summary>
        public const int LGTaskDescription97330 = 1084873778;
        /// <summary>
        /// LG-TaskConfig-Task-64048-Description-974-3-0
        /// 通关迷雾群岛40.台地集结
        /// </summary>
        public const int LGTaskDescription97430 = 1209541526;
        /// <summary>
        /// LG-TaskConfig-Task-64049-Description-975-3-0
        /// 提升迷雾群岛探索等级到9级
        /// </summary>
        public const int LGTaskDescription97530 = 1819432902;
        /// <summary>
        /// LG-TaskConfig-Task-64050-Description-976-3-0
        /// 通关迷雾群岛45.台地集结
        /// </summary>
        public const int LGTaskDescription97630 = 190466117;
        /// <summary>
        /// LG-TaskConfig-Task-64051-Description-977-3-0
        /// 提升迷雾群岛探索等级到10级
        /// </summary>
        public const int LGTaskDescription97730 = 985930829;
        /// <summary>
        /// LG-TaskConfig-Task-64053-Description-979-3-0
        /// 通关迷雾群岛50.台地集结
        /// </summary>
        public const int LGTaskDescription97930 = 2021889765;
        /// <summary>
        /// LG-TaskConfig-Task-64280-Description-980-3-0
        /// 使村庄等级达到暴风之城V
        /// </summary>
        public const int LGTaskDescription98030 = 1574302487;
        /// <summary>
        /// LG-TaskConfig-Task-64054-Description-981-3-0
        /// 提升迷雾群岛探索等级到11级
        /// </summary>
        public const int LGTaskDescription98130 = 1637947551;
        /// <summary>
        /// LG-TaskConfig-Task-64055-Description-982-3-0
        /// 切换地图到龙骨荒丘
        /// </summary>
        public const int LGTaskDescription98230 = 1608043629;
        /// <summary>
        /// LG-TaskConfig-Task-64056-Description-983-3-0
        /// 通关圣印之塔435层
        /// </summary>
        public const int LGTaskDescription98330 = 200566519;
        /// <summary>
        /// LG-TaskConfig-Task-64057-Description-984-3-0
        /// 升级旅店到141级
        /// </summary>
        public const int LGTaskDescription98430 = 828584625;
        /// <summary>
        /// LG-TaskConfig-Task-64058-Description-985-3-0
        /// 升级医院到141级
        /// </summary>
        public const int LGTaskDescription98530 = 1322875263;
        /// <summary>
        /// LG-TaskConfig-Task-64060-Description-987-3-0
        /// 通关龙骨荒丘5.崖边货栈
        /// </summary>
        public const int LGTaskDescription98730 = 853524666;
        /// <summary>
        /// LG-TaskConfig-Task-64061-Description-988-3-0
        /// 提升龙骨荒丘探索等级到2级
        /// </summary>
        public const int LGTaskDescription98830 = 1046556008;
        /// <summary>
        /// LG-TaskConfig-Task-64062-Description-989-3-0
        /// 通关龙骨荒丘10.崖脚集结
        /// </summary>
        public const int LGTaskDescription98930 = 433259270;
        /// <summary>
        /// LG-TaskConfig-Task-64063-Description-990-3-0
        /// 提升龙骨荒丘探索等级到3级
        /// </summary>
        public const int LGTaskDescription99030 = 1168055711;
        /// <summary>
        /// LG-TaskConfig-Task-64064-Description-991-3-0
        /// 通关龙骨荒丘15.龙骨梁桥
        /// </summary>
        public const int LGTaskDescription99130 = 1547298929;
        /// <summary>
        /// LG-TaskConfig-Task-64065-Description-992-3-0
        /// 提升龙骨荒丘探索等级到4级
        /// </summary>
        public const int LGTaskDescription99230 = 154772439;
        /// <summary>
        /// LG-TaskConfig-Task-64067-Description-994-3-0
        /// 通关龙骨荒丘20.村寨集结
        /// </summary>
        public const int LGTaskDescription99430 = 642755343;
        /// <summary>
        /// LG-TaskConfig-Task-64068-Description-995-3-0
        /// 提升龙骨荒丘探索等级到5级
        /// </summary>
        public const int LGTaskDescription99530 = 1957329657;
        /// <summary>
        /// LG-TaskConfig-Task-64069-Description-996-3-0
        /// 通关龙骨荒丘25.腐骨祭坑
        /// </summary>
        public const int LGTaskDescription99630 = 1727625383;
        /// <summary>
        /// LG-TaskConfig-Task-64070-Description-997-3-0
        /// 提升龙骨荒丘探索等级到6级
        /// </summary>
        public const int LGTaskDescription99730 = 1092895290;
        /// <summary>
        /// LG-TaskConfig-Task-64071-Description-998-3-0
        /// 通关龙骨荒丘30.龙骨集结
        /// </summary>
        public const int LGTaskDescription99830 = 492858524;
        /// <summary>
        /// LG-TaskConfig-Task-64072-Description-999-3-0
        /// 提升龙骨荒丘探索等级到7级
        /// </summary>
        public const int LGTaskDescription99930 = 659544634;
        /// <summary>
        /// LG-TaskConfig-Task-64073-Description-1000-3-0
        /// 通关龙骨荒丘35.石偶工坊
        /// </summary>
        public const int LGTaskDescription100030 = 1513672827;
        /// <summary>
        /// LG-TaskConfig-Task-64074-Description-1001-3-0
        /// 提升龙骨荒丘探索等级到8级
        /// </summary>
        public const int LGTaskDescription100130 = 1685689219;
        /// <summary>
        /// LG-TaskConfig-Task-64075-Description-1002-3-0
        /// 通关龙骨荒丘40.台地集结
        /// </summary>
        public const int LGTaskDescription100230 = 1621455139;
        /// <summary>
        /// LG-TaskConfig-Task-64076-Description-1003-3-0
        /// 提升龙骨荒丘探索等级到9级
        /// </summary>
        public const int LGTaskDescription100330 = 1418105731;
        /// <summary>
        /// LG-TaskConfig-Task-64077-Description-1004-3-0
        /// 通关龙骨荒丘45.台地集结
        /// </summary>
        public const int LGTaskDescription100430 = 479301587;
        /// <summary>
        /// LG-TaskConfig-Task-64078-Description-1005-3-0
        /// 提升龙骨荒丘探索等级到10级
        /// </summary>
        public const int LGTaskDescription100530 = 1480077531;
        /// <summary>
        /// LG-TaskConfig-Task-64080-Description-1007-3-0
        /// 通关龙骨荒丘50.台地集结
        /// </summary>
        public const int LGTaskDescription100730 = 741044128;
        /// <summary>
        /// LG-TaskConfig-Task-64281-Description-1008-3-0
        /// 使村庄等级达到暴风之城VI
        /// </summary>
        public const int LGTaskDescription100830 = 62291450;
        /// <summary>
        /// LG-TaskConfig-Task-64081-Description-1009-3-0
        /// 提升龙骨荒丘探索等级到11级
        /// </summary>
        public const int LGTaskDescription100930 = 1962749671;
        /// <summary>
        /// LG-TaskConfig-Task-64082-Description-1010-3-0
        /// 切换地图到风暴尖塔
        /// </summary>
        public const int LGTaskDescription101030 = 1319149594;
        /// <summary>
        /// LG-TaskConfig-Task-64083-Description-1011-3-0
        /// 通关圣印之塔455层
        /// </summary>
        public const int LGTaskDescription101130 = 2140500896;
        /// <summary>
        /// LG-TaskConfig-Task-64084-Description-1012-3-0
        /// 升级旅店到151级
        /// </summary>
        public const int LGTaskDescription101230 = 1170264918;
        /// <summary>
        /// LG-TaskConfig-Task-64085-Description-1013-3-0
        /// 升级医院到151级
        /// </summary>
        public const int LGTaskDescription101330 = 1540982768;
        /// <summary>
        /// LG-TaskConfig-Task-64087-Description-1015-3-0
        /// 通关风暴尖塔5.崖边货栈
        /// </summary>
        public const int LGTaskDescription101530 = 947477400;
        /// <summary>
        /// LG-TaskConfig-Task-64088-Description-1016-3-0
        /// 提升风暴尖塔探索等级到2级
        /// </summary>
        public const int LGTaskDescription101630 = 1777997278;
        /// <summary>
        /// LG-TaskConfig-Task-64089-Description-1017-3-0
        /// 通关风暴尖塔10.崖脚集结
        /// </summary>
        public const int LGTaskDescription101730 = 1401160936;
        /// <summary>
        /// LG-TaskConfig-Task-64090-Description-1018-3-0
        /// 提升风暴尖塔探索等级到3级
        /// </summary>
        public const int LGTaskDescription101830 = 1230489851;
        /// <summary>
        /// LG-TaskConfig-Task-64091-Description-1019-3-0
        /// 通关风暴尖塔15.龙骨梁桥
        /// </summary>
        public const int LGTaskDescription101930 = 1243214725;
        /// <summary>
        /// LG-TaskConfig-Task-64092-Description-1020-3-0
        /// 提升风暴尖塔探索等级到4级
        /// </summary>
        public const int LGTaskDescription102030 = 2013614474;
        /// <summary>
        /// LG-TaskConfig-Task-64094-Description-1022-3-0
        /// 通关风暴尖塔20.村寨集结
        /// </summary>
        public const int LGTaskDescription102230 = 1856754462;
        /// <summary>
        /// LG-TaskConfig-Task-64095-Description-1023-3-0
        /// 提升风暴尖塔探索等级到5级
        /// </summary>
        public const int LGTaskDescription102330 = 1540700864;
        /// <summary>
        /// LG-TaskConfig-Task-64096-Description-1024-3-0
        /// 通关风暴尖塔25.腐骨祭坑
        /// </summary>
        public const int LGTaskDescription102430 = 790681050;
        /// <summary>
        /// LG-TaskConfig-Task-64097-Description-1025-3-0
        /// 提升风暴尖塔探索等级到6级
        /// </summary>
        public const int LGTaskDescription102530 = 892669008;
        /// <summary>
        /// LG-TaskConfig-Task-64098-Description-1026-3-0
        /// 通关风暴尖塔30.龙骨集结
        /// </summary>
        public const int LGTaskDescription102630 = 2136811734;
        /// <summary>
        /// LG-TaskConfig-Task-64099-Description-1027-3-0
        /// 提升风暴尖塔探索等级到7级
        /// </summary>
        public const int LGTaskDescription102730 = 234607608;
        /// <summary>
        /// LG-TaskConfig-Task-64100-Description-1028-3-0
        /// 通关风暴尖塔35.石偶工坊
        /// </summary>
        public const int LGTaskDescription102830 = 1010390722;
        /// <summary>
        /// LG-TaskConfig-Task-64101-Description-1029-3-0
        /// 提升风暴尖塔探索等级到8级
        /// </summary>
        public const int LGTaskDescription102930 = 1184728608;
        /// <summary>
        /// LG-TaskConfig-Task-64102-Description-1030-3-0
        /// 通关风暴尖塔40.台地集结
        /// </summary>
        public const int LGTaskDescription103030 = 385344077;
        /// <summary>
        /// LG-TaskConfig-Task-64103-Description-1031-3-0
        /// 提升风暴尖塔探索等级到9级
        /// </summary>
        public const int LGTaskDescription103130 = 2126115151;
        /// <summary>
        /// LG-TaskConfig-Task-64104-Description-1032-3-0
        /// 通关风暴尖塔45.台地集结
        /// </summary>
        public const int LGTaskDescription103230 = 2120083189;
        /// <summary>
        /// LG-TaskConfig-Task-64105-Description-1033-3-0
        /// 提升风暴尖塔探索等级到10级
        /// </summary>
        public const int LGTaskDescription103330 = 2053573819;
        /// <summary>
        /// LG-TaskConfig-Task-64107-Description-1035-3-0
        /// 通关风暴尖塔50.台地集结
        /// </summary>
        public const int LGTaskDescription103530 = 1552943743;
        /// <summary>
        /// LG-TaskConfig-Task-64282-Description-1036-3-0
        /// 使村庄等级达到暴风之城VII
        /// </summary>
        public const int LGTaskDescription103630 = 1911090024;
        /// <summary>
        /// LG-TaskConfig-Task-64108-Description-1037-3-0
        /// 提升风暴尖塔探索等级到11级
        /// </summary>
        public const int LGTaskDescription103730 = 1928985298;
        /// <summary>
        /// LG-TaskConfig-Task-64109-Description-1038-3-0
        /// 切换地图到熔火裂谷
        /// </summary>
        public const int LGTaskDescription103830 = 313834714;
        /// <summary>
        /// LG-TaskConfig-Task-64110-Description-1039-3-0
        /// 通关圣印之塔470层
        /// </summary>
        public const int LGTaskDescription103930 = 1043825171;
        /// <summary>
        /// LG-TaskConfig-Task-64111-Description-1040-3-0
        /// 升级旅店到161级
        /// </summary>
        public const int LGTaskDescription104030 = 300837890;
        /// <summary>
        /// LG-TaskConfig-Task-64112-Description-1041-3-0
        /// 升级医院到161级
        /// </summary>
        public const int LGTaskDescription104130 = 1821534658;
        /// <summary>
        /// LG-TaskConfig-Task-64114-Description-1043-3-0
        /// 通关熔火裂谷5.崖边货栈
        /// </summary>
        public const int LGTaskDescription104330 = 729192306;
        /// <summary>
        /// LG-TaskConfig-Task-64115-Description-1044-3-0
        /// 提升熔火裂谷探索等级到2级
        /// </summary>
        public const int LGTaskDescription104430 = 69072802;
        /// <summary>
        /// LG-TaskConfig-Task-64116-Description-1045-3-0
        /// 通关熔火裂谷10.崖脚集结
        /// </summary>
        public const int LGTaskDescription104530 = 129908466;
        /// <summary>
        /// LG-TaskConfig-Task-64117-Description-1046-3-0
        /// 提升熔火裂谷探索等级到3级
        /// </summary>
        public const int LGTaskDescription104630 = 1260005346;
        /// <summary>
        /// LG-TaskConfig-Task-64118-Description-1047-3-0
        /// 通关熔火裂谷15.龙骨梁桥
        /// </summary>
        public const int LGTaskDescription104730 = 610147202;
        /// <summary>
        /// LG-TaskConfig-Task-64119-Description-1048-3-0
        /// 提升熔火裂谷探索等级到4级
        /// </summary>
        public const int LGTaskDescription104830 = 48466082;
        /// <summary>
        /// LG-TaskConfig-Task-64121-Description-1050-3-0
        /// 通关熔火裂谷20.村寨集结
        /// </summary>
        public const int LGTaskDescription105030 = 1509682562;
        /// <summary>
        /// LG-TaskConfig-Task-64122-Description-1051-3-0
        /// 提升熔火裂谷探索等级到5级
        /// </summary>
        public const int LGTaskDescription105130 = 993547802;
        /// <summary>
        /// LG-TaskConfig-Task-64123-Description-1052-3-0
        /// 通关熔火裂谷25.腐骨祭坑
        /// </summary>
        public const int LGTaskDescription105230 = 1755990938;
        /// <summary>
        /// LG-TaskConfig-Task-64124-Description-1053-3-0
        /// 提升熔火裂谷探索等级到6级
        /// </summary>
        public const int LGTaskDescription105330 = 1960208994;
        /// <summary>
        /// LG-TaskConfig-Task-64125-Description-1054-3-0
        /// 通关熔火裂谷30.龙骨集结
        /// </summary>
        public const int LGTaskDescription105430 = 1345357250;
        /// <summary>
        /// LG-TaskConfig-Task-64126-Description-1055-3-0
        /// 提升熔火裂谷探索等级到7级
        /// </summary>
        public const int LGTaskDescription105530 = 914122234;
        /// <summary>
        /// LG-TaskConfig-Task-64127-Description-1056-3-0
        /// 通关熔火裂谷35.石偶工坊
        /// </summary>
        public const int LGTaskDescription105630 = 1380574474;
        /// <summary>
        /// LG-TaskConfig-Task-64128-Description-1057-3-0
        /// 提升熔火裂谷探索等级到8级
        /// </summary>
        public const int LGTaskDescription105730 = 661816674;
        /// <summary>
        /// LG-TaskConfig-Task-64129-Description-1058-3-0
        /// 通关熔火裂谷40.台地集结
        /// </summary>
        public const int LGTaskDescription105830 = 1750673170;
        /// <summary>
        /// LG-TaskConfig-Task-64130-Description-1059-3-0
        /// 提升熔火裂谷探索等级到9级
        /// </summary>
        public const int LGTaskDescription105930 = 1994686975;
        /// <summary>
        /// LG-TaskConfig-Task-64131-Description-1060-3-0
        /// 通关熔火裂谷45.台地集结
        /// </summary>
        public const int LGTaskDescription106030 = 1526758706;
        /// <summary>
        /// LG-TaskConfig-Task-64132-Description-1061-3-0
        /// 提升熔火裂谷探索等级到10级
        /// </summary>
        public const int LGTaskDescription106130 = 610081866;
        /// <summary>
        /// LG-TaskConfig-Task-64134-Description-1063-3-0
        /// 通关熔火裂谷50.台地集结
        /// </summary>
        public const int LGTaskDescription106330 = 1237642634;
        /// <summary>
        /// LG-TaskConfig-Task-64283-Description-1064-3-0
        /// 使村庄等级达到暴风之城VIII
        /// </summary>
        public const int LGTaskDescription106430 = 2004297538;
        /// <summary>
        /// LG-TaskConfig-Task-64135-Description-1065-3-0
        /// 提升熔火裂谷探索等级到11级
        /// </summary>
        public const int LGTaskDescription106530 = 1347284549;
        /// <summary>
        /// LG-TaskConfig-Task-64136-Description-1066-3-0
        /// 切换地图到永夜冰原
        /// </summary>
        public const int LGTaskDescription106630 = 66562391;
        /// <summary>
        /// LG-TaskConfig-Task-64137-Description-1067-3-0
        /// 通关圣印之塔490层
        /// </summary>
        public const int LGTaskDescription106730 = 307305653;
        /// <summary>
        /// LG-TaskConfig-Task-64138-Description-1068-3-0
        /// 升级旅店到171级
        /// </summary>
        public const int LGTaskDescription106830 = 803170979;
        /// <summary>
        /// LG-TaskConfig-Task-64139-Description-1069-3-0
        /// 升级医院到171级
        /// </summary>
        public const int LGTaskDescription106930 = 113390101;
        /// <summary>
        /// LG-TaskConfig-Task-64141-Description-1071-3-0
        /// 通关永夜冰原5.崖边货栈
        /// </summary>
        public const int LGTaskDescription107130 = 249130613;
        /// <summary>
        /// LG-TaskConfig-Task-64142-Description-1072-3-0
        /// 提升永夜冰原探索等级到2级
        /// </summary>
        public const int LGTaskDescription107230 = 993963007;
        /// <summary>
        /// LG-TaskConfig-Task-64143-Description-1073-3-0
        /// 通关永夜冰原10.崖脚集结
        /// </summary>
        public const int LGTaskDescription107330 = 1267770957;
        /// <summary>
        /// LG-TaskConfig-Task-64144-Description-1074-3-0
        /// 提升永夜冰原探索等级到3级
        /// </summary>
        public const int LGTaskDescription107430 = 1180320011;
        /// <summary>
        /// LG-TaskConfig-Task-64145-Description-1075-3-0
        /// 通关永夜冰原15.龙骨梁桥
        /// </summary>
        public const int LGTaskDescription107530 = 347256741;
        /// <summary>
        /// LG-TaskConfig-Task-64146-Description-1076-3-0
        /// 提升永夜冰原探索等级到4级
        /// </summary>
        public const int LGTaskDescription107630 = 1413783583;
        /// <summary>
        /// LG-TaskConfig-Task-64148-Description-1078-3-0
        /// 通关永夜冰原20.村寨集结
        /// </summary>
        public const int LGTaskDescription107830 = 1921704851;
        /// <summary>
        /// LG-TaskConfig-Task-64149-Description-1079-3-0
        /// 提升永夜冰原探索等级到5级
        /// </summary>
        public const int LGTaskDescription107930 = 1139468261;
        /// <summary>
        /// LG-TaskConfig-Task-64150-Description-1080-3-0
        /// 通关永夜冰原25.腐骨祭坑
        /// </summary>
        public const int LGTaskDescription108030 = 716431811;
        /// <summary>
        /// LG-TaskConfig-Task-64151-Description-1081-3-0
        /// 提升永夜冰原探索等级到6级
        /// </summary>
        public const int LGTaskDescription108130 = 1342140541;
        /// <summary>
        /// LG-TaskConfig-Task-64152-Description-1082-3-0
        /// 通关永夜冰原30.龙骨集结
        /// </summary>
        public const int LGTaskDescription108230 = 1443926583;
        /// <summary>
        /// LG-TaskConfig-Task-64153-Description-1083-3-0
        /// 提升永夜冰原探索等级到7级
        /// </summary>
        public const int LGTaskDescription108330 = 1256540813;
        /// <summary>
        /// LG-TaskConfig-Task-64154-Description-1084-3-0
        /// 通关永夜冰原35.石偶工坊
        /// </summary>
        public const int LGTaskDescription108430 = 866904171;
        /// <summary>
        /// LG-TaskConfig-Task-64155-Description-1085-3-0
        /// 提升永夜冰原探索等级到8级
        /// </summary>
        public const int LGTaskDescription108530 = 2051299485;
        /// <summary>
        /// LG-TaskConfig-Task-64156-Description-1086-3-0
        /// 通关永夜冰原40.台地集结
        /// </summary>
        public const int LGTaskDescription108630 = 1614623783;
        /// <summary>
        /// LG-TaskConfig-Task-64157-Description-1087-3-0
        /// 提升永夜冰原探索等级到9级
        /// </summary>
        public const int LGTaskDescription108730 = 945664461;
        /// <summary>
        /// LG-TaskConfig-Task-64158-Description-1088-3-0
        /// 通关永夜冰原45.台地集结
        /// </summary>
        public const int LGTaskDescription108830 = 1895989027;
        /// <summary>
        /// LG-TaskConfig-Task-64159-Description-1089-3-0
        /// 提升永夜冰原探索等级到10级
        /// </summary>
        public const int LGTaskDescription108930 = 905787629;
        /// <summary>
        /// LG-TaskConfig-Task-64161-Description-1091-3-0
        /// 通关永夜冰原50.台地集结
        /// </summary>
        public const int LGTaskDescription109130 = 661535805;
        /// <summary>
        /// LG-TaskConfig-Task-64284-Description-1092-3-0
        /// 使村庄等级达到暴风之城IX
        /// </summary>
        public const int LGTaskDescription109230 = 348185824;
        /// <summary>
        /// LG-TaskConfig-Task-64162-Description-1093-3-0
        /// 提升永夜冰原探索等级到11级
        /// </summary>
        public const int LGTaskDescription109330 = 1564284476;
        /// <summary>
        /// LG-TaskConfig-Task-64163-Description-1094-3-0
        /// 切换地图到星陨荒庭
        /// </summary>
        public const int LGTaskDescription109430 = 19340224;
        /// <summary>
        /// LG-TaskConfig-Task-64164-Description-1095-3-0
        /// 通关圣印之塔505层
        /// </summary>
        public const int LGTaskDescription109530 = 167490824;
        /// <summary>
        /// LG-TaskConfig-Task-64165-Description-1096-3-0
        /// 升级旅店到181级
        /// </summary>
        public const int LGTaskDescription109630 = 1896953092;
        /// <summary>
        /// LG-TaskConfig-Task-64166-Description-1097-3-0
        /// 升级医院到181级
        /// </summary>
        public const int LGTaskDescription109730 = 2147107196;
        /// <summary>
        /// LG-TaskConfig-Task-64168-Description-1099-3-0
        /// 通关星陨荒庭5.崖边货栈
        /// </summary>
        public const int LGTaskDescription109930 = 25932848;
        /// <summary>
        /// LG-TaskConfig-Task-64169-Description-1100-3-0
        /// 提升星陨荒庭探索等级到2级
        /// </summary>
        public const int LGTaskDescription110030 = 1801790992;
        /// <summary>
        /// LG-TaskConfig-Task-64170-Description-1101-3-0
        /// 通关星陨荒庭10.崖脚集结
        /// </summary>
        public const int LGTaskDescription110130 = 209213077;
        /// <summary>
        /// LG-TaskConfig-Task-64171-Description-1102-3-0
        /// 提升星陨荒庭探索等级到3级
        /// </summary>
        public const int LGTaskDescription110230 = 1419016309;
        /// <summary>
        /// LG-TaskConfig-Task-64172-Description-1103-3-0
        /// 通关星陨荒庭15.龙骨梁桥
        /// </summary>
        public const int LGTaskDescription110330 = 1187121793;
        /// <summary>
        /// LG-TaskConfig-Task-64173-Description-1104-3-0
        /// 提升星陨荒庭探索等级到4级
        /// </summary>
        public const int LGTaskDescription110430 = 817544937;
        /// <summary>
        /// LG-TaskConfig-Task-64175-Description-1106-3-0
        /// 通关星陨荒庭20.村寨集结
        /// </summary>
        public const int LGTaskDescription110630 = 201867109;
        /// <summary>
        /// LG-TaskConfig-Task-64176-Description-1107-3-0
        /// 提升星陨荒庭探索等级到5级
        /// </summary>
        public const int LGTaskDescription110730 = 552903769;
        /// <summary>
        /// LG-TaskConfig-Task-64177-Description-1108-3-0
        /// 通关星陨荒庭25.腐骨祭坑
        /// </summary>
        public const int LGTaskDescription110830 = 822638705;
        /// <summary>
        /// LG-TaskConfig-Task-64178-Description-1109-3-0
        /// 提升星陨荒庭探索等级到6级
        /// </summary>
        public const int LGTaskDescription110930 = 1917198741;
        /// <summary>
        /// LG-TaskConfig-Task-64179-Description-1110-3-0
        /// 通关星陨荒庭30.龙骨集结
        /// </summary>
        public const int LGTaskDescription111030 = 338390678;
        /// <summary>
        /// LG-TaskConfig-Task-64180-Description-1111-3-0
        /// 提升星陨荒庭探索等级到7级
        /// </summary>
        public const int LGTaskDescription111130 = 651987281;
        /// <summary>
        /// LG-TaskConfig-Task-64181-Description-1112-3-0
        /// 通关星陨荒庭35.石偶工坊
        /// </summary>
        public const int LGTaskDescription111230 = 402822297;
        /// <summary>
        /// LG-TaskConfig-Task-64182-Description-1113-3-0
        /// 提升星陨荒庭探索等级到8级
        /// </summary>
        public const int LGTaskDescription111330 = 1461653541;
        /// <summary>
        /// LG-TaskConfig-Task-64183-Description-1114-3-0
        /// 通关星陨荒庭40.台地集结
        /// </summary>
        public const int LGTaskDescription111430 = 318516445;
        /// <summary>
        /// LG-TaskConfig-Task-64184-Description-1115-3-0
        /// 提升星陨荒庭探索等级到9级
        /// </summary>
        public const int LGTaskDescription111530 = 1344472201;
        /// <summary>
        /// LG-TaskConfig-Task-64185-Description-1116-3-0
        /// 通关星陨荒庭45.台地集结
        /// </summary>
        public const int LGTaskDescription111630 = 1111786241;
        /// <summary>
        /// LG-TaskConfig-Task-64186-Description-1117-3-0
        /// 提升星陨荒庭探索等级到10级
        /// </summary>
        public const int LGTaskDescription111730 = 2008296565;
        /// <summary>
        /// LG-TaskConfig-Task-64188-Description-1119-3-0
        /// 通关星陨荒庭50.台地集结
        /// </summary>
        public const int LGTaskDescription111930 = 1398200577;
        /// <summary>
        /// LG-TaskConfig-Task-64285-Description-1120-3-0
        /// 使村庄等级达到暴风之城X
        /// </summary>
        public const int LGTaskDescription112030 = 74814887;
        /// <summary>
        /// LG-TaskConfig-Task-64189-Description-1121-3-0
        /// 提升星陨荒庭探索等级到11级
        /// </summary>
        public const int LGTaskDescription112130 = 199222383;
        /// <summary>
        /// LG-TaskConfig-Task-64190-Description-1122-3-0
        /// 切换地图到世界之门
        /// </summary>
        public const int LGTaskDescription112230 = 238560348;
        /// <summary>
        /// LG-TaskConfig-Task-64191-Description-1123-3-0
        /// 通关圣印之塔525层
        /// </summary>
        public const int LGTaskDescription112330 = 921431338;
        /// <summary>
        /// LG-TaskConfig-Task-64192-Description-1124-3-0
        /// 升级旅店到191级
        /// </summary>
        public const int LGTaskDescription112430 = 791682524;
        /// <summary>
        /// LG-TaskConfig-Task-64193-Description-1125-3-0
        /// 升级医院到191级
        /// </summary>
        public const int LGTaskDescription112530 = 455817990;
        /// <summary>
        /// LG-TaskConfig-Task-64195-Description-1127-3-0
        /// 通关世界之门5.崖边货栈
        /// </summary>
        public const int LGTaskDescription112730 = 1410825298;
        /// <summary>
        /// LG-TaskConfig-Task-64196-Description-1128-3-0
        /// 提升世界之门探索等级到2级
        /// </summary>
        public const int LGTaskDescription112830 = 399030612;
        /// <summary>
        /// LG-TaskConfig-Task-64197-Description-1129-3-0
        /// 通关世界之门10.崖脚集结
        /// </summary>
        public const int LGTaskDescription112930 = 116698582;
        /// <summary>
        /// LG-TaskConfig-Task-64198-Description-1130-3-0
        /// 提升世界之门探索等级到3级
        /// </summary>
        public const int LGTaskDescription113030 = 2030717307;
        /// <summary>
        /// LG-TaskConfig-Task-64199-Description-1131-3-0
        /// 通关世界之门15.龙骨梁桥
        /// </summary>
        public const int LGTaskDescription113130 = 423121825;
        /// <summary>
        /// LG-TaskConfig-Task-64200-Description-1132-3-0
        /// 提升世界之门探索等级到4级
        /// </summary>
        public const int LGTaskDescription113230 = 1952992701;
        /// <summary>
        /// LG-TaskConfig-Task-64202-Description-1134-3-0
        /// 通关世界之门20.村寨集结
        /// </summary>
        public const int LGTaskDescription113430 = 18067041;
        /// <summary>
        /// LG-TaskConfig-Task-64203-Description-1135-3-0
        /// 提升世界之门探索等级到5级
        /// </summary>
        public const int LGTaskDescription113530 = 477297695;
        /// <summary>
        /// LG-TaskConfig-Task-64204-Description-1136-3-0
        /// 通关世界之门25.腐骨祭坑
        /// </summary>
        public const int LGTaskDescription113630 = 357736781;
        /// <summary>
        /// LG-TaskConfig-Task-64205-Description-1137-3-0
        /// 提升世界之门探索等级到6级
        /// </summary>
        public const int LGTaskDescription113730 = 281855175;
        /// <summary>
        /// LG-TaskConfig-Task-64206-Description-1138-3-0
        /// 通关世界之门30.龙骨集结
        /// </summary>
        public const int LGTaskDescription113830 = 2015167593;
        /// <summary>
        /// LG-TaskConfig-Task-64207-Description-1139-3-0
        /// 提升世界之门探索等级到7级
        /// </summary>
        public const int LGTaskDescription113930 = 1016472959;
        /// <summary>
        /// LG-TaskConfig-Task-64208-Description-1140-3-0
        /// 通关世界之门35.石偶工坊
        /// </summary>
        public const int LGTaskDescription114030 = 1192027652;
        /// <summary>
        /// LG-TaskConfig-Task-64209-Description-1141-3-0
        /// 提升世界之门探索等级到8级
        /// </summary>
        public const int LGTaskDescription114130 = 49474198;
        /// <summary>
        /// LG-TaskConfig-Task-64210-Description-1142-3-0
        /// 通关世界之门40.台地集结
        /// </summary>
        public const int LGTaskDescription114230 = 698519749;
        /// <summary>
        /// LG-TaskConfig-Task-64211-Description-1143-3-0
        /// 提升世界之门探索等级到9级
        /// </summary>
        public const int LGTaskDescription114330 = 85994999;
        /// <summary>
        /// LG-TaskConfig-Task-64212-Description-1144-3-0
        /// 通关世界之门45.台地集结
        /// </summary>
        public const int LGTaskDescription114430 = 310907361;
        /// <summary>
        /// LG-TaskConfig-Task-64213-Description-1145-3-0
        /// 提升世界之门探索等级到10级
        /// </summary>
        public const int LGTaskDescription114530 = 797815415;
        /// <summary>
        /// LG-TaskConfig-Task-64215-Description-1147-3-0
        /// 通关世界之门50.台地集结
        /// </summary>
        public const int LGTaskDescription114730 = 6565863;
        /// <summary>
        /// LG-TaskConfig-Task-64216-Description-1148-3-0
        /// 提升世界之门探索等级到11级
        /// </summary>
        public const int LGTaskDescription114830 = 631548233;
        /// <summary>
        /// LG-TaskConfig-Task-64217-Description-1149-3-0
        /// 通关圣印之塔565层
        /// </summary>
        public const int LGTaskDescription114930 = 439855879;
        /// <summary>
        /// LG-TaskConfig-Task-64261-Description-1150-3-0
        /// 升级旅店到200级
        /// </summary>
        public const int LGTaskDescription115030 = 2049733614;
        /// <summary>
        /// LG-TaskConfig-Task-64262-Description-1151-3-0
        /// 升级医院到200级
        /// </summary>
        public const int LGTaskDescription115130 = 1479800206;
        /// <summary>
        /// LG-TaskConfig-Task-64221-Description-1153-3-0
        /// 在世界之门集结任意副本3次
        /// </summary>
        public const int LGTaskDescription115330 = 1520208103;
        /// <summary>
        /// LG-TaskConfig-Task-64222-Description-1154-3-0
        /// 在领地采集3次资源
        /// </summary>
        public const int LGTaskDescription115430 = 115630817;
        /// <summary>
        /// LG-TaskConfig-Task-64223-Description-1155-3-0
        /// 领地抢夺3次资源
        /// </summary>
        public const int LGTaskDescription115530 = 2048827071;
        /// <summary>
        /// LG-TaskConfig-Task-64224-Description-1156-3-0
        /// 助威他人获胜3次
        /// </summary>
        public const int LGTaskDescription115630 = 1178149653;
        /// <summary>
        /// LG-TaskConfig-Task-64225-Description-1157-3-0
        /// 招募英雄3次
        /// </summary>
        public const int LGTaskDescription115730 = 1763866607;
        /// <summary>
        /// LG-TaskConfig-Task-64226-Description-1158-3-0
        /// 参与竞技场3次
        /// </summary>
        public const int LGTaskDescription115830 = 616395937;
        /// <summary>
        /// LG-TaskConfig-Task-70001-Name-1160-2-0
        /// 副本
        /// </summary>
        public const int LGTaskName116020 = 1718044560;
        /// <summary>
        /// LG-TaskConfig-Task-70001-Description-1160-3-0
        /// {0}个章节任务后解锁
        /// </summary>
        public const int LGTaskDescription116030 = 579809684;
        /// <summary>
        /// LG-TaskConfig-Task-70002-Name-1161-2-0
        /// 探索等级
        /// </summary>
        public const int LGTaskName116120 = 1735780834;
        /// <summary>
        /// LG-TaskConfig-Task-70003-Name-1162-2-0
        /// 日常任务
        /// </summary>
        public const int LGTaskName116220 = 300290736;
        /// <summary>
        /// LG-TaskConfig-Task-70004-Name-1163-2-0
        /// 七日签到
        /// </summary>
        public const int LGTaskName116320 = 1027538014;
        /// <summary>
        /// LG-TaskConfig-Task-70005-Name-1164-2-0
        /// 成长试炼
        /// </summary>
        public const int LGTaskName116420 = 2047629112;
        /// <summary>
        /// LG-TaskConfig-Task-70006-Name-1165-2-0
        /// 邮件
        /// </summary>
        public const int LGTaskName116520 = 796849906;
        /// <summary>
        /// LG-TaskConfig-Task-70007-Name-1166-2-0
        /// 试炼
        /// </summary>
        public const int LGTaskName116620 = 1485927800;
        /// <summary>
        /// LG-TaskConfig-Task-70008-Name-1167-2-0
        /// 竞技场
        /// </summary>
        public const int LGTaskName116720 = 783229190;
        /// <summary>
        /// LG-TaskConfig-Task-70009-Name-1168-2-0
        /// 府邸
        /// </summary>
        public const int LGTaskName116820 = 201102256;
        /// <summary>
        /// LG-TaskConfig-Task-70010-Name-1169-2-0
        /// 领主圣印
        /// </summary>
        public const int LGTaskName116920 = 729855309;
        /// <summary>
        /// LG-TaskConfig-Task-70011-Name-1170-2-0
        /// 符文之塔
        /// </summary>
        public const int LGTaskName117020 = 1453882212;
        /// <summary>
        /// LG-TaskConfig-Task-70012-Name-1171-2-0
        /// 副本搜索
        /// </summary>
        public const int LGTaskName117120 = 431369510;
        /// <summary>
        /// LG-TaskConfig-Task-70013-Name-1172-2-0
        /// 排行榜
        /// </summary>
        public const int LGTaskName117220 = 7622864;
        /// <summary>
        /// LG-TaskConfig-Task-70014-Name-1173-2-0
        /// 公会
        /// </summary>
        public const int LGTaskName117320 = 259109662;
        /// <summary>
        /// LG-TaskConfig-Task-70015-Name-1174-2-0
        /// 夺宝
        /// </summary>
        public const int LGTaskName117420 = 12814172;
        /// <summary>
        /// LG-TaskConfig-Task-70016-Name-1175-2-0
        /// 藏品
        /// </summary>
        public const int LGTaskName117520 = 934228566;
        /// <summary>
        /// LG-TaskConfig-Task-80002-Name-1177-2-0
        /// 副本集结1次
        /// </summary>
        public const int LGTaskName117720 = 381331684;
        /// <summary>
        /// LG-TaskConfig-Task-80003-Name-1178-2-0
        /// 进行1次圣印之塔挑战
        /// </summary>
        public const int LGTaskName117820 = 1908186910;
        /// <summary>
        /// LG-TaskConfig-Task-80004-Name-1179-2-0
        /// 进行1次竞技场挑战
        /// </summary>
        public const int LGTaskName117920 = 898319536;
        /// <summary>
        /// LG-TaskConfig-Task-80005-Name-1180-2-0
        /// 招募1次英雄
        /// </summary>
        public const int LGTaskName118020 = 2004513531;
        /// <summary>
        /// LG-TaskConfig-Task-80006-Name-1181-2-0
        /// 领取1次商店金币
        /// </summary>
        public const int LGTaskName118120 = 2029597041;
        /// <summary>
        /// LG-TaskConfig-Task-80007-Name-1182-2-0
        /// 领地采集1次资源
        /// </summary>
        public const int LGTaskName118220 = 1143725371;
        /// <summary>
        /// LG-TaskConfig-Task-80009-Name-1184-2-0
        /// 秘境扫荡1次
        /// </summary>
        public const int LGTaskName118420 = 954806467;
        /// <summary>
        /// LG-TaskConfig-Task-80010-Name-1185-2-0
        /// 藏品抽取1次
        /// </summary>
        public const int LGTaskName118520 = 705262626;
        /// <summary>
        /// LG-TaskConfig-Task-80011-Name-1186-2-0
        /// 助威他人获胜1次
        /// </summary>
        public const int LGTaskName118620 = 1516515564;
        /// <summary>
        /// LG-TaskConfig-Task-90001-Description-1187-3-0
        /// 登陆1天
        /// </summary>
        public const int LGTaskDescription118730 = 753915621;
        /// <summary>
        /// LG-TaskConfig-Task-90002-Description-1188-3-0
        /// 登陆2天
        /// </summary>
        public const int LGTaskDescription118830 = 1680968959;
        /// <summary>
        /// LG-TaskConfig-Task-90003-Description-1189-3-0
        /// 登陆3天
        /// </summary>
        public const int LGTaskDescription118930 = 2079490565;
        /// <summary>
        /// LG-TaskConfig-Task-90004-Description-1190-3-0
        /// 登陆4天
        /// </summary>
        public const int LGTaskDescription119030 = 442314424;
        /// <summary>
        /// LG-TaskConfig-Task-90005-Description-1191-3-0
        /// 登陆5天
        /// </summary>
        public const int LGTaskDescription119130 = 356129018;
        /// <summary>
        /// LG-TaskConfig-Task-90006-Description-1192-3-0
        /// 圣印之塔20关通关
        /// </summary>
        public const int LGTaskDescription119230 = 1453315652;
        /// <summary>
        /// LG-TaskConfig-Task-90007-Description-1193-3-0
        /// 圣印之塔40关通关
        /// </summary>
        public const int LGTaskDescription119330 = 485198802;
        /// <summary>
        /// LG-TaskConfig-Task-90008-Description-1194-3-0
        /// 圣印之塔60关通关
        /// </summary>
        public const int LGTaskDescription119430 = 983833696;
        /// <summary>
        /// LG-TaskConfig-Task-90009-Description-1195-3-0
        /// 圣印之塔80关通关
        /// </summary>
        public const int LGTaskDescription119530 = 1133437962;
        /// <summary>
        /// LG-TaskConfig-Task-90010-Description-1196-3-0
        /// 圣印之塔100关通关
        /// </summary>
        public const int LGTaskDescription119630 = 1785660957;
        /// <summary>
        /// LG-TaskConfig-Task-90011-Description-1197-3-0
        /// 圣印之塔120关通关
        /// </summary>
        public const int LGTaskDescription119730 = 1703473231;
        /// <summary>
        /// LG-TaskConfig-Task-90012-Description-1198-3-0
        /// 圣印之塔140关通关
        /// </summary>
        public const int LGTaskDescription119830 = 1519979121;
        /// <summary>
        /// LG-TaskConfig-Task-90013-Description-1199-3-0
        /// 圣印之塔160关通关
        /// </summary>
        public const int LGTaskDescription119930 = 1860635727;
        /// <summary>
        /// LG-TaskConfig-Task-90014-Description-1200-3-0
        /// 圣印之塔180关通关
        /// </summary>
        public const int LGTaskDescription120030 = 63644075;
        /// <summary>
        /// LG-TaskConfig-Task-90015-Description-1201-3-0
        /// 圣印之塔200关通关
        /// </summary>
        public const int LGTaskDescription120130 = 1196760657;
        /// <summary>
        /// LG-TaskConfig-Task-90016-Description-1202-3-0
        /// 圣印之塔250关通关
        /// </summary>
        public const int LGTaskDescription120230 = 1718160903;
        /// <summary>
        /// LG-TaskConfig-Task-90017-Description-1203-3-0
        /// 圣印之塔300关通关
        /// </summary>
        public const int LGTaskDescription120330 = 1619410193;
        /// <summary>
        /// LG-TaskConfig-Task-90018-Description-1204-3-0
        /// 采集10次领地资源
        /// </summary>
        public const int LGTaskDescription120430 = 411833731;
        /// <summary>
        /// LG-TaskConfig-Task-90019-Description-1205-3-0
        /// 采集20次领地资源
        /// </summary>
        public const int LGTaskDescription120530 = 1234277401;
        /// <summary>
        /// LG-TaskConfig-Task-90020-Description-1206-3-0
        /// 采集30次领地资源
        /// </summary>
        public const int LGTaskDescription120630 = 1399027228;
        /// <summary>
        /// LG-TaskConfig-Task-90021-Description-1207-3-0
        /// 采集40次领地资源
        /// </summary>
        public const int LGTaskDescription120730 = 1857164386;
        /// <summary>
        /// LG-TaskConfig-Task-90022-Description-1208-3-0
        /// 采集50次领地资源
        /// </summary>
        public const int LGTaskDescription120830 = 815495880;
        /// <summary>
        /// LG-TaskConfig-Task-90023-Description-1209-3-0
        /// 采集60次领地资源
        /// </summary>
        public const int LGTaskDescription120930 = 497754986;
        /// <summary>
        /// LG-TaskConfig-Task-90024-Description-1210-3-0
        /// 采集70次领地资源
        /// </summary>
        public const int LGTaskDescription121030 = 1201333283;
        /// <summary>
        /// LG-TaskConfig-Task-90025-Description-1211-3-0
        /// 采集80次领地资源
        /// </summary>
        public const int LGTaskDescription121130 = 599124081;
        /// <summary>
        /// LG-TaskConfig-Task-90026-Description-1212-3-0
        /// 所有圣印达到5级以上
        /// </summary>
        public const int LGTaskDescription121230 = 591052655;
        /// <summary>
        /// LG-TaskConfig-Task-90027-Description-1213-3-0
        /// 所有圣印达到10级以上
        /// </summary>
        public const int LGTaskDescription121330 = 1248248433;
        /// <summary>
        /// LG-TaskConfig-Task-90028-Description-1214-3-0
        /// 所有圣印达到15级以上
        /// </summary>
        public const int LGTaskDescription121430 = 247656995;
        /// <summary>
        /// LG-TaskConfig-Task-90029-Description-1215-3-0
        /// 所有圣印达到20级以上
        /// </summary>
        public const int LGTaskDescription121530 = 387185729;
        /// <summary>
        /// LG-TaskConfig-Task-90030-Description-1216-3-0
        /// 所有圣印达到25级以上
        /// </summary>
        public const int LGTaskDescription121630 = 1598688346;
        /// <summary>
        /// LG-TaskConfig-Task-90031-Description-1217-3-0
        /// 所有圣印达到30级以上
        /// </summary>
        public const int LGTaskDescription121730 = 1534137600;
        /// <summary>
        /// LG-TaskConfig-Task-90032-Description-1218-3-0
        /// 领地等级达到2级
        /// </summary>
        public const int LGTaskDescription121830 = 330718550;
        /// <summary>
        /// LG-TaskConfig-Task-90033-Description-1219-3-0
        /// 领地等级达到3级
        /// </summary>
        public const int LGTaskDescription121930 = 1239701208;
        /// <summary>
        /// LG-TaskConfig-Task-90034-Description-1220-3-0
        /// 领地等级达到4级
        /// </summary>
        public const int LGTaskDescription122030 = 1778105819;
        /// <summary>
        /// LG-TaskConfig-Task-90035-Description-1221-3-0
        /// 领地等级达到5级
        /// </summary>
        public const int LGTaskDescription122130 = 1816424977;
        /// <summary>
        /// LG-TaskConfig-Task-90036-Description-1222-3-0
        /// 竞技场挑战5次
        /// </summary>
        public const int LGTaskDescription122230 = 710383943;
        /// <summary>
        /// LG-TaskConfig-Task-90037-Description-1223-3-0
        /// 竞技场挑战10次
        /// </summary>
        public const int LGTaskDescription122330 = 2139767265;
        /// <summary>
        /// LG-TaskConfig-Task-90038-Description-1224-3-0
        /// 竞技场挑战15次
        /// </summary>
        public const int LGTaskDescription122430 = 399804275;
        /// <summary>
        /// LG-TaskConfig-Task-90039-Description-1225-3-0
        /// 竞技场挑战20次
        /// </summary>
        public const int LGTaskDescription122530 = 1168848233;
        /// <summary>
        /// LG-TaskConfig-Task-90040-Description-1226-3-0
        /// 竞技场挑战25次
        /// </summary>
        public const int LGTaskDescription122630 = 1590212740;
        /// <summary>
        /// LG-TaskConfig-Task-90041-Description-1227-3-0
        /// 拥有1个3级以上的藏品
        /// </summary>
        public const int LGTaskDescription122730 = 1110958138;
        /// <summary>
        /// LG-TaskConfig-Task-90042-Description-1228-3-0
        /// 拥有3个3级以上的藏品
        /// </summary>
        public const int LGTaskDescription122830 = 1156020336;
        /// <summary>
        /// LG-TaskConfig-Task-90043-Description-1229-3-0
        /// 拥有5个3级以上的藏品
        /// </summary>
        public const int LGTaskDescription122930 = 533065762;
        /// <summary>
        /// LG-TaskConfig-Task-90044-Description-1230-3-0
        /// 拥有1个5级以上的藏品
        /// </summary>
        public const int LGTaskDescription123030 = 29347627;
        /// <summary>
        /// LG-TaskConfig-Task-90045-Description-1231-3-0
        /// 拥有3个5级以上的藏品
        /// </summary>
        public const int LGTaskDescription123130 = 2146851481;
        /// <summary>
        /// LG-TaskConfig-Task-90046-Description-1232-3-0
        /// 拥有3个蓝色品质藏品
        /// </summary>
        public const int LGTaskDescription123230 = 1651694183;
        /// <summary>
        /// LG-TaskConfig-Task-90047-Description-1233-3-0
        /// 拥有5个蓝色品质藏品
        /// </summary>
        public const int LGTaskDescription123330 = 584203145;
        /// <summary>
        /// LG-TaskConfig-Task-90048-Description-1234-3-0
        /// 拥有1个紫色品质藏品
        /// </summary>
        public const int LGTaskDescription123430 = 1543070539;
        /// <summary>
        /// LG-TaskConfig-Task-90049-Description-1235-3-0
        /// 拥有3个紫色品质藏品
        /// </summary>
        public const int LGTaskDescription123530 = 530459801;
        /// <summary>
        /// LG-TaskConfig-Task-90050-Description-1236-3-0
        /// 装配5个2阶绿色品质以上圣印
        /// </summary>
        public const int LGTaskDescription123630 = 1080694030;
        /// <summary>
        /// LG-TaskConfig-Task-90051-Description-1237-3-0
        /// 装配5个2阶蓝色品质以上圣印
        /// </summary>
        public const int LGTaskDescription123730 = 1867436980;
        /// <summary>
        /// LG-TaskConfig-Task-90052-Description-1238-3-0
        /// 装配5个3阶蓝色品质以上圣印
        /// </summary>
        public const int LGTaskDescription123830 = 991162658;
        /// <summary>
        /// LG-TaskConfig-Task-90053-Description-1239-3-0
        /// 装配3个4阶紫色品质以上圣印
        /// </summary>
        public const int LGTaskDescription123930 = 1407781092;
        /// <summary>
        /// LG-TaskConfig-Task-90054-Description-1240-3-0
        /// 装配5个4阶紫色品质以上圣印
        /// </summary>
        public const int LGTaskDescription124030 = 948108819;
        /// <summary>
        /// LG-TaskConfig-Task-90056-Description-1241-3-0
        /// 领地抢夺10次资源
        /// </summary>
        public const int LGTaskDescription124130 = 1543887922;
        /// <summary>
        /// LG-TaskConfig-Task-90057-Description-1242-3-0
        /// 领地抢夺20次资源
        /// </summary>
        public const int LGTaskDescription124230 = 488603694;
        /// <summary>
        /// LG-TaskConfig-Task-90058-Description-1243-3-0
        /// 领地抢夺30次资源
        /// </summary>
        public const int LGTaskDescription124330 = 1928303546;
        /// <summary>
        /// LG-TaskConfig-Task-90059-Description-1244-3-0
        /// 领地抢夺40次资源
        /// </summary>
        public const int LGTaskDescription124430 = 335136854;
        /// <summary>
        /// LG-TaskConfig-Task-90060-Description-1245-3-0
        /// 英雄招募40次
        /// </summary>
        public const int LGTaskDescription124530 = 1276667517;
        /// <summary>
        /// LG-TaskConfig-Task-90061-Description-1246-3-0
        /// 英雄招募60次
        /// </summary>
        public const int LGTaskDescription124630 = 56459221;
        /// <summary>
        /// LG-TaskConfig-Task-90062-Description-1247-3-0
        /// 英雄招募80次
        /// </summary>
        public const int LGTaskDescription124730 = 2142638301;
        /// <summary>
        /// LG-TaskConfig-Task-90063-Description-1248-3-0
        /// 英雄招募100次
        /// </summary>
        public const int LGTaskDescription124830 = 1776939789;
        /// <summary>
        /// LG-TaskConfig-Task-90064-Description-1249-3-0
        /// 英雄招募150次
        /// </summary>
        public const int LGTaskDescription124930 = 1187348613;
        /// <summary>
        /// LG-TaskConfig-Task-90065-Description-1250-3-0
        /// 英雄升星5次
        /// </summary>
        public const int LGTaskDescription125030 = 1025043102;
        /// <summary>
        /// LG-TaskConfig-Task-90066-Description-1251-3-0
        /// 英雄升星10次
        /// </summary>
        public const int LGTaskDescription125130 = 22157050;
        /// <summary>
        /// LG-TaskConfig-Task-90067-Description-1252-3-0
        /// 英雄升星15次
        /// </summary>
        public const int LGTaskDescription125230 = 348650574;
        /// <summary>
        /// LG-TaskConfig-Task-90068-Description-1253-3-0
        /// 英雄升星20次
        /// </summary>
        public const int LGTaskDescription125330 = 471858442;
        /// <summary>
        /// LG-TaskConfig-Task-90069-Description-1254-3-0
        /// 英雄升星25次
        /// </summary>
        public const int LGTaskDescription125430 = 1645437102;
        /// <summary>
        /// LG-TaskConfig-Task-90070-Description-1255-3-0
        /// 10名英雄拥有40级绿色武器
        /// </summary>
        public const int LGTaskDescription125530 = 819286495;
        /// <summary>
        /// LG-TaskConfig-Task-90071-Description-1256-3-0
        /// 10名英雄拥有40级蓝色武器
        /// </summary>
        public const int LGTaskDescription125630 = 1559848051;
        /// <summary>
        /// LG-TaskConfig-Task-90072-Description-1257-3-0
        /// 10名英雄拥有50级蓝色武器
        /// </summary>
        public const int LGTaskDescription125730 = 362163951;
        /// <summary>
        /// LG-TaskConfig-Task-90073-Description-1258-3-0
        /// 15名英雄拥有50级蓝色武器
        /// </summary>
        public const int LGTaskDescription125830 = 1794283531;
        /// <summary>
        /// LG-TaskConfig-Task-90074-Description-1259-3-0
        /// 10名英雄拥有50级紫色武器
        /// </summary>
        public const int LGTaskDescription125930 = 818082783;
        /// <summary>
        /// LG-TaskConfig-Task-90075-Description-1260-3-0
        /// 15名英雄拥有50级紫色武器
        /// </summary>
        public const int LGTaskDescription126030 = 2142560174;
        /// <summary>
        /// LG-TaskConfig-Task-90076-Description-1261-3-0
        /// 副本集结40次
        /// </summary>
        public const int LGTaskDescription126130 = 1480788098;
        /// <summary>
        /// LG-TaskConfig-Task-90077-Description-1262-3-0
        /// 副本集结80次
        /// </summary>
        public const int LGTaskDescription126230 = 297741310;
        /// <summary>
        /// LG-TaskConfig-Task-90078-Description-1263-3-0
        /// 副本集结100次
        /// </summary>
        public const int LGTaskDescription126330 = 832664122;
        /// <summary>
        /// LG-TaskConfig-Task-90079-Description-1264-3-0
        /// 副本集结120次
        /// </summary>
        public const int LGTaskDescription126430 = 1512282662;
        /// <summary>
        /// LG-TaskConfig-Task-90080-Description-1265-3-0
        /// 副本集结150次
        /// </summary>
        public const int LGTaskDescription126530 = 787922261;
        /// <summary>
        /// LG-TaskConfig-Task-90081-Description-1266-3-0
        /// 任意佣兵等级达到5级
        /// </summary>
        public const int LGTaskDescription126630 = 318006125;
        /// <summary>
        /// LG-TaskConfig-Task-90082-Description-1267-3-0
        /// 任意佣兵等级达到10级
        /// </summary>
        public const int LGTaskDescription126730 = 2137205285;
        /// <summary>
        /// LG-TaskConfig-Task-90083-Description-1268-3-0
        /// 任意佣兵等级达到15级
        /// </summary>
        public const int LGTaskDescription126830 = 596802021;
        /// <summary>
        /// LG-TaskConfig-Task-90084-Description-1269-3-0
        /// 任意佣兵等级达到20级
        /// </summary>
        public const int LGTaskDescription126930 = 716061517;
        /// <summary>
        /// LG-TaskConfig-Task-90085-Description-1270-3-0
        /// 任意佣兵等级达到25级
        /// </summary>
        public const int LGTaskDescription127030 = 1949895270;
        /// <summary>
        /// LG-TaskConfig-Task-90086-Description-1271-3-0
        /// 任意佣兵等级达到30级
        /// </summary>
        public const int LGTaskDescription127130 = 809436898;
        /// <summary>
        /// LG-TaskConfig-Task-90106-Description-1277-3-0
        /// 符文塔10关通关
        /// </summary>
        public const int LGTaskDescription127730 = 400572053;
        /// <summary>
        /// LG-TaskConfig-Task-90107-Description-1278-3-0
        /// 符文塔15关通关
        /// </summary>
        public const int LGTaskDescription127830 = 1889673565;
        /// <summary>
        /// LG-TaskConfig-Task-90108-Description-1279-3-0
        /// 符文塔20关通关
        /// </summary>
        public const int LGTaskDescription127930 = 13106061;
        /// <summary>
        /// LG-TaskConfig-Task-90109-Description-1280-3-0
        /// 符文塔25关通关
        /// </summary>
        public const int LGTaskDescription128030 = 531657148;
        /// <summary>
        /// LG-TaskConfig-Task-90110-Description-1281-3-0
        /// 符文塔30关通关
        /// </summary>
        public const int LGTaskDescription128130 = 1361694257;
        /// <summary>
        /// LG-TaskConfig-Task-90111-Description-1282-3-0
        /// 符文塔40关通关
        /// </summary>
        public const int LGTaskDescription128230 = 1951906113;
        /// <summary>
        /// LG-TaskConfig-Task-90112-Description-1283-3-0
        /// 符文塔50关通关
        /// </summary>
        public const int LGTaskDescription128330 = 1380578497;
        /// <summary>
        /// LG-TaskConfig-Task-90113-Description-1284-3-0
        /// 符文塔60关通关
        /// </summary>
        public const int LGTaskDescription128430 = 2097273985;
        /// <summary>
        /// LG-TaskConfig-Task-90114-Description-1285-3-0
        /// 符文塔70关通关
        /// </summary>
        public const int LGTaskDescription128530 = 468652657;
        /// <summary>
        /// LG-TaskConfig-Task-90115-Description-1286-3-0
        /// 符文塔80关通关
        /// </summary>
        public const int LGTaskDescription128630 = 842772257;
        /// <summary>
        /// LG-TaskConfig-Task-90116-Description-1287-3-0
        /// 符文塔90关通关
        /// </summary>
        public const int LGTaskDescription128730 = 1977772577;
        /// <summary>
        /// LG-TaskConfig-Task-90117-Description-1288-3-0
        /// 符文塔100关通关
        /// </summary>
        public const int LGTaskDescription128830 = 1064396993;
        /// <summary>
        /// LG-TaskConfig-Task-90205-Description-1301-3-0
        /// 所有圣印达到50级以上
        /// </summary>
        public const int LGTaskDescription130130 = 65147011;
        /// <summary>
        /// LG-TaskConfig-Task-90206-Description-1302-3-0
        /// 所有圣印达到100级以上
        /// </summary>
        public const int LGTaskDescription130230 = 718758745;
        /// <summary>
        /// LG-TaskConfig-Task-90207-Description-1303-3-0
        /// 公会夺宝5次
        /// </summary>
        public const int LGTaskDescription130330 = 664065215;
        /// <summary>
        /// LG-TaskConfig-Task-90208-Description-1304-3-0
        /// 公会夺宝10次
        /// </summary>
        public const int LGTaskDescription130430 = 1487528257;
        /// <summary>
        /// LG-TaskConfig-Task-90209-Description-1305-3-0
        /// 公会夺宝15次
        /// </summary>
        public const int LGTaskDescription130530 = 1953399579;
        /// <summary>
        /// LG-TaskConfig-Task-90210-Description-1306-3-0
        /// 公会夺宝20次
        /// </summary>
        public const int LGTaskDescription130630 = 1810965560;
        /// <summary>
        /// LG-TaskConfig-Task-90310-Description-1321-3-0
        /// 拥有1个橙色品质藏品
        /// </summary>
        public const int LGTaskDescription132130 = 853966734;
        /// <summary>
        /// LG-TaskConfig-Task-90316-Description-1327-3-0
        /// 装配5个5阶紫色品质以上圣印
        /// </summary>
        public const int LGTaskDescription132730 = 2100776734;
        /// <summary>
        /// LG-TaskConfig-Task-90401-Description-1328-3-0
        /// 祝福升满1阶
        /// </summary>
        public const int LGTaskDescription132830 = 690413804;
        /// <summary>
        /// LG-TaskConfig-Task-90402-Description-1329-3-0
        /// 祝福升满2阶
        /// </summary>
        public const int LGTaskDescription132930 = 932680692;
        /// <summary>
        /// LG-TaskConfig-Task-90403-Description-1330-3-0
        /// 祝福升满3阶
        /// </summary>
        public const int LGTaskDescription133030 = 1622271695;
        /// <summary>
        /// LG-TaskConfig-Task-90404-Description-1331-3-0
        /// 祝福升满4阶
        /// </summary>
        public const int LGTaskDescription133130 = 1121432611;
        /// <summary>
        /// LG-TaskConfig-Task-90409-Description-1336-3-0
        /// 英雄招募200次
        /// </summary>
        public const int LGTaskDescription133630 = 1844403955;
        /// <summary>
        /// LG-TaskConfig-Task-90512-Description-1353-3-0
        /// 通关西部荒野12关
        /// </summary>
        public const int LGTaskDescription135330 = 1652651023;
        /// <summary>
        /// LG-TaskConfig-Task-90513-Description-1354-3-0
        /// 通关埋骨林地30关
        /// </summary>
        public const int LGTaskDescription135430 = 1678443535;
        /// <summary>
        /// LG-TaskConfig-Task-90514-Description-1355-3-0
        /// 通关幽暗沼泽30关
        /// </summary>
        public const int LGTaskDescription135530 = 863969051;
        /// <summary>
        /// LG-TaskConfig-Task-90515-Description-1356-3-0
        /// 通关幽暗沼泽60关
        /// </summary>
        public const int LGTaskDescription135630 = 129169067;
        /// <summary>
        /// LG-TaskConfig-Task-90516-Description-1357-3-0
        /// 通关环形山脉10关
        /// </summary>
        public const int LGTaskDescription135730 = 1325642103;
        /// <summary>
        /// LG-TaskConfig-Task-90517-Description-1358-3-0
        /// 通关环形山脉20关
        /// </summary>
        public const int LGTaskDescription135830 = 2104614871;
        /// <summary>
        /// LG-TaskConfig-Task-90518-Description-1359-3-0
        /// 通关环形山脉40关
        /// </summary>
        public const int LGTaskDescription135930 = 721442891;
        /// <summary>
        /// LG-TaskConfig-Task-90519-Description-1360-3-0
        /// 通关环形山脉60关
        /// </summary>
        public const int LGTaskDescription136030 = 1154717518;
        /// <summary>
        /// LG-TaskConfig-Task-90520-Description-1361-3-0
        /// 通关环形山脉80关
        /// </summary>
        public const int LGTaskDescription136130 = 138143989;
        /// <summary>
        /// LG-TaskConfig-Task-90521-Description-1362-3-0
        /// 通关环形山脉100关
        /// </summary>
        public const int LGTaskDescription136230 = 1210681721;
        /// <summary>
        /// LG-TaskConfig-Task-91002-Name-1364-2-0
        /// 助威他人获胜2次
        /// </summary>
        public const int LGTaskName136420 = 1440515800;
        /// <summary>
        /// LG-TaskConfig-Task-120001-Name-1365-2-0
        /// 加入游戏圈
        /// </summary>
        public const int LGTaskName136520 = 1854249113;
        /// <summary>
        /// LG-TaskConfig-Task-120002-Name-1366-2-0
        /// 游戏圈每日发贴1次
        /// </summary>
        public const int LGTaskName136620 = 246014293;
        /// <summary>
        /// LG-TaskConfig-Task-120002-Description-1366-3-0
        /// 发表1次贴子数
        /// </summary>
        public const int LGTaskDescription136630 = 2067383867;
        /// <summary>
        /// LG-TaskConfig-Task-120003-Name-1367-2-0
        /// 游戏圈每日点赞3次
        /// </summary>
        public const int LGTaskName136720 = 850051129;
        /// <summary>
        /// LG-TaskConfig-Task-120003-Description-1367-3-0
        /// 点赞3次贴子数
        /// </summary>
        public const int LGTaskDescription136730 = 691131009;
        /// <summary>
        /// LG-TaskConfig-Task-120004-Name-1368-2-0
        /// 游戏圈每日评论3次
        /// </summary>
        public const int LGTaskName136820 = 452683341;
        /// <summary>
        /// LG-TaskConfig-Task-120004-Description-1368-3-0
        /// 评论3次贴子数
        /// </summary>
        public const int LGTaskDescription136830 = 973789819;
        /// <summary>
        /// LG-TaskConfig-Task-130001-Name-1369-2-0
        /// 累计充值30元
        /// </summary>
        public const int LGTaskName136920 = 468123998;
        /// <summary>
        /// LG-TaskConfig-Task-130002-Name-1370-2-0
        /// 累计充值60元
        /// </summary>
        public const int LGTaskName137020 = 277939321;
        /// <summary>
        /// LG-TaskConfig-Task-130003-Name-1371-2-0
        /// 累计充值100元
        /// </summary>
        public const int LGTaskName137120 = 1754297633;
        /// <summary>
        /// LG-TaskConfig-Task-130004-Name-1372-2-0
        /// 累计充值150元
        /// </summary>
        public const int LGTaskName137220 = 1373612993;
        /// <summary>
        /// LG-TaskConfig-Task-130005-Name-1373-2-0
        /// 累计充值200元
        /// </summary>
        public const int LGTaskName137320 = 696484537;
        /// <summary>
        /// LG-TaskConfig-Task-130006-Name-1374-2-0
        /// 累计充值300元
        /// </summary>
        public const int LGTaskName137420 = 522675385;
        /// <summary>
        /// LG-TaskConfig-Task-130007-Name-1375-2-0
        /// 累计充值400元
        /// </summary>
        public const int LGTaskName137520 = 1262765617;
        /// <summary>
        /// LG-TaskConfig-Task-130008-Name-1376-2-0
        /// 累计充值500元
        /// </summary>
        public const int LGTaskName137620 = 366865697;
        /// <summary>
        /// LG-TaskConfig-Task-130009-Name-1377-2-0
        /// 累计充值600元
        /// </summary>
        public const int LGTaskName137720 = 1140628777;
        /// <summary>
        /// LG-TaskConfig-Task-130010-Name-1378-2-0
        /// 累计充值800元
        /// </summary>
        public const int LGTaskName137820 = 802421022;
        /// <summary>
        /// LG-TaskConfig-Task-130011-Name-1379-2-0
        /// 累计充值1000元
        /// </summary>
        public const int LGTaskName137920 = 2062211942;
        /// <summary>
        /// LG-TaskConfig-Task-130012-Name-1380-2-0
        /// 累计充值1200元
        /// </summary>
        public const int LGTaskName138020 = 225285183;
        /// <summary>
        /// LG-TaskConfig-Task-130013-Name-1381-2-0
        /// 累计充值1400元
        /// </summary>
        public const int LGTaskName138120 = 1656023111;
        /// <summary>
        /// LG-TaskConfig-Task-130014-Name-1382-2-0
        /// 累计充值2000元
        /// </summary>
        public const int LGTaskName138220 = 1365736475;
        /// <summary>
        /// LG-TaskConfig-Task-130015-Name-1383-2-0
        /// 累计充值2600元
        /// </summary>
        public const int LGTaskName138320 = 1122143283;
        /// <summary>
        /// LG-TaskConfig-Task-130016-Name-1384-2-0
        /// 累计充值3200元
        /// </summary>
        public const int LGTaskName138420 = 1404423631;
        /// <summary>
        /// LG-TaskConfig-Task-130017-Name-1385-2-0
        /// 累计充值3800元
        /// </summary>
        public const int LGTaskName138520 = 391814535;
        /// <summary>
        /// LG-TaskConfig-Task-130018-Name-1386-2-0
        /// 累计充值4400元
        /// </summary>
        public const int LGTaskName138620 = 1857093219;
        /// <summary>
        /// LG-TaskConfig-Task-130019-Name-1387-2-0
        /// 累计充值5000元
        /// </summary>
        public const int LGTaskName138720 = 185903723;
        /// <summary>
        /// LG-TaskConfig-Task-130020-Name-1388-2-0
        /// 累计充值5600元
        /// </summary>
        public const int LGTaskName138820 = 47190810;
        /// <summary>
        /// LG-TaskConfig-Task-140001-Name-1389-2-0
        /// 消费100钻石
        /// </summary>
        public const int LGTaskName138920 = 1653091417;
        /// <summary>
        /// LG-TaskConfig-Task-140002-Name-1390-2-0
        /// 消费200钻石
        /// </summary>
        public const int LGTaskName139020 = 794446542;
        /// <summary>
        /// LG-TaskConfig-Task-140003-Name-1391-2-0
        /// 消费300钻石
        /// </summary>
        public const int LGTaskName139120 = 411739826;
        /// <summary>
        /// LG-TaskConfig-Task-140004-Name-1392-2-0
        /// 消费400钻石
        /// </summary>
        public const int LGTaskName139220 = 1887279398;
        /// <summary>
        /// LG-TaskConfig-Task-140005-Name-1393-2-0
        /// 消费500钻石
        /// </summary>
        public const int LGTaskName139320 = 2081155034;
        /// <summary>
        /// LG-TaskConfig-Task-150001-Name-1394-2-0
        /// 查看一次商队奖励预览
        /// </summary>
        public const int LGTaskName139420 = 812121520;
        /// <summary>
        /// LG-TaskConfig-Task-150002-Name-1395-2-0
        /// 进行10次藏品探索
        /// </summary>
        public const int LGTaskName139520 = 310557614;
        /// <summary>
        /// LG-TaskConfig-Task-150003-Name-1396-2-0
        /// 进行10次宠物捕捉
        /// </summary>
        public const int LGTaskName139620 = 1927319332;
        /// <summary>
        /// LG-TaskConfig-Task-150004-Name-1397-2-0
        /// 进行10次专精书抄录
        /// </summary>
        public const int LGTaskName139720 = 1799864758;
        /// <summary>
        /// LG-TaskConfig-Task-150005-Name-1398-2-0
        /// 进行一次装备强化
        /// </summary>
        public const int LGTaskName139820 = 489398064;
        /// <summary>
        /// LG-TaskConfig-Task-150006-Name-1399-2-0
        /// 在金币商店进行一次购买
        /// </summary>
        public const int LGTaskName139920 = 1671509406;
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