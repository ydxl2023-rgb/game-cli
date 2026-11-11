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