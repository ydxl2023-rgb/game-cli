using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class SkillAttr
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 数字ID
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// 技能名称
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// 技能等级
        /// </summary>
        [JsonProperty]
        public int Level { get; private set; }

        /// <summary>
        /// 技能组ID
        /// </summary>
        [JsonProperty]
        public int GroupId { get; private set; }

        /// <summary>
        /// 技能名
        /// </summary>
        [JsonProperty]
        public string Desc { get; private set; }

        /// <summary>
        /// 升级条件
        /// </summary>
        [JsonProperty]
        public string UpgradeConds { get; private set; }

        /// <summary>
        /// 被动技能释放条件
        /// </summary>
        [JsonProperty]
        public string Passives { get; private set; }

        /// <summary>
        /// 技能消耗
        /// </summary>
        [JsonProperty]
        public int Cost { get; private set; }

        /// <summary>
        /// 技能目标
        /// </summary>
        [JsonProperty]
        public ESkillTarget SkillTarget { get; private set; }

        /// <summary>
        /// 冷却时间
        /// </summary>
        [JsonProperty]
        public int cd { get; private set; }

        /// <summary>
        /// 全局CD
        /// </summary>
        [JsonProperty]
        public int Gcd { get; private set; }

        /// <summary>
        /// 施法时间
        /// </summary>
        [JsonProperty]
        public int CastTime { get; private set; }

        /// <summary>
        /// 最小施法距离
        /// </summary>
        [JsonProperty]
        public int MinCastDistance { get; private set; }

        /// <summary>
        /// 最大施法距离
        /// </summary>
        [JsonProperty]
        public int MaxCastDistance { get; private set; }

        /// <summary>
        /// 检测类型
        /// </summary>
        [JsonProperty]
        public ESkillCheckType SkillCheckType { get; private set; }

        /// <summary>
        /// 角度
        /// </summary>
        [JsonProperty]
        public int Angle { get; private set; }

        /// <summary>
        /// 检测多少个目标
        /// </summary>
        [JsonProperty]
        public int CheckCount { get; private set; }

        /// <summary>
        /// 伤害百分比
        /// </summary>
        [JsonProperty]
        public int DamageRate { get; private set; }

        /// <summary>
        /// 附加伤害
        /// </summary>
        [JsonProperty]
        public int Value { get; private set; }

        /// <summary>
        /// 技能瞬发效果
        /// </summary>
        [JsonProperty]
        public string Momentarys { get; private set; }

        /// <summary>
        /// 帧效果
        /// </summary>
        [JsonProperty]
        public string FrameEffects { get; private set; }

        /// <summary>
        /// 技能命中效果
        /// </summary>
        [JsonProperty]
        public string Effects { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty]
        public string Version { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (SkillAttr exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class SkillAttrConfig : Config<SkillAttr>
    {
    }

    /// <summary>
    /// export excel SkillAttr data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config SkillAttrConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, SkillAttr> skillAttrConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static SkillAttr propSkillAttrById(int nId)
        {
            SkillAttr data;
            if (!skillAttrConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(SkillAttr).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}