using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class Skill
    {
        /// <summary>
        /// 技能ID
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
        /// 英文名
        /// </summary>
        [JsonProperty]
        public string EnglishName { get; private set; }

        /// <summary>
        /// 技能描述
        /// </summary>
        [JsonProperty]
        public string Desc { get; private set; }

        /// <summary>
        /// 技能类型
        /// </summary>
        [JsonProperty]
        public ESkillType SkillType { get; private set; }

        /// <summary>
        /// 技能属性
        /// </summary>
        [JsonProperty]
        public int Attr { get; private set; }

        /// <summary>
        /// 技能动作
        /// </summary>
        [JsonProperty]
        public string Clip { get; private set; }

        /// <summary>
        /// 技能优先级
        /// </summary>
        [JsonProperty]
        public int Priority { get; private set; }

        /// <summary>
        /// 技能图标
        /// </summary>
        [JsonProperty]
        public string Icon { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (Skill exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class SkillConfig : Config<Skill>
    {
    }

    /// <summary>
    /// export excel Skill data
    /// </summary>
    public sealed partial class Excel
    {
        public const int SKILL_ATTACK = 11001;
        public const int SKILL_SWEEP = 11002;
        public const int SKILL_ASSASSINATE = 11003;
        public const int SKILL_THROATCUTTING = 11004;
        public const int SKILL_EXECUTE = 11005;
        public const int SKILL_LIGHT = 11006;
        public const int SKILL_COUNTERATTACK = 11007;
        public const int SKILL_CHARGE = 11008;
        public const int SKILL_RIDICULE = 11009;
        public const int SKILL_TENACITY = 11010;
        public const int SKILL_ANGER = 11011;
        public const int SKILL_SERIOUSINJURY = 11012;
        public const int SKILL_ROAR = 11013;
        public const int SKILL_ARMORPENETRATION = 11014;
        public const int SKILL_SHIELDWALL = 11015;
        public const int SKILL_BLOODTHIRSTY = 11016;
        public const int SKILL_SHIELDSTRIKE = 11017;
        public const int SKILL_REVENGE = 11018;
        public const int SKILL_BREAKINGAXE = 11019;
        public const int SKILL_SHOOTING = 12001;
        public const int SKILL_AIMSHOOT = 12002;
        public const int SKILL_PIERCINGHEART = 12003;
        public const int SKILL_ARROWRAIN = 12004;
        public const int SKILL_FLYINGARROW = 12005;
        public const int SKILL_CHOPINDISCRIMINATELY = 13001;
        public const int SKILL_DOUBLESWORDASSASSINATION = 13002;
        public const int SKILL_FIERCEWINDSLASH = 13003;
        public const int SKILL_GARROTE = 13004;
        public const int SKILL_FURIOUSSLASH = 13005;
        public const int SKILL_SLAM = 13006;
        public const int SKILL_MURDER = 13007;
        public const int SKILL_FIREBALL = 14001;
        public const int SKILL_LIGHTNING = 14002;
        public const int SKILL_ELECTRICSHOCKSTORM = 14003;
        public const int SKILL_TELEPORTATION = 14004;
        public const int SKILL_THUNDERBOLT = 14005;
        public const int SKILL_FIRERAIN = 14006;
        public const int SKILL_SUMMONING = 14007;
        public const int SKILL_POISONBALL = 15001;
        public const int SKILL_HEALING = 15002;
        public const int SKILL_LIFEDELIVERY = 15003;
        public const int SKILL_DODGE = 15004;
        public const int SKILL_OFFICEFIRERAIN = 20001;
        public const int SKILL_GROUPDEFENSESCROLL = 30001;
        public const int SKILL_GROUPTHERAPYSCROLL = 30002;
        public const int SKILL_GROUPMAGICSCROLL = 30003;
        public const int SKILL_ACCELERATIONSCROLL = 30004;
        public const int SKILL_ROARSCROLL = 30005;
        public const int SKILL_MOCKINGSCROLL = 30006;
        public const int SKILL_SILENTSCROLL = 30007;
        public const int SKILL_LIFERECOVERYSCROLL = 30008;
        public const int SKILL_MAGICRECOVERYSCROLL = 30009;
        public const int SKILL_GOLDENSHIELD = 30010;
        public const int SKILL_SUMMONINGMAGIC = 30011;
        /// <summary>
        /// config SkillConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, Skill> skillConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static Skill propSkillById(int nId)
        {
            Skill data;
            if (!skillConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(Skill).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}