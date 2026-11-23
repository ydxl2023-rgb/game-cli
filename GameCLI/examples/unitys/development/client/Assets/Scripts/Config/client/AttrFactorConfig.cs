using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class AttrFactor
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
        /// 备注
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// 走路速度
        /// </summary>
        [JsonProperty]
        public float Walk { get; private set; }

        /// <summary>
        /// 跑步速度
        /// </summary>
        [JsonProperty]
        public float Speed { get; private set; }

        /// <summary>
        /// 转向速度
        /// </summary>
        [JsonProperty]
        public float AngularSpeed { get; private set; }

        /// <summary>
        /// 力量
        /// </summary>
        [JsonProperty]
        public float Strength { get; private set; }

        /// <summary>
        /// 敏捷
        /// </summary>
        [JsonProperty]
        public float Agility { get; private set; }

        /// <summary>
        /// 智力
        /// </summary>
        [JsonProperty]
        public float Wit { get; private set; }

        /// <summary>
        /// 耐力
        /// </summary>
        [JsonProperty]
        public float Stamina { get; private set; }

        /// <summary>
        /// 精神
        /// </summary>
        [JsonProperty]
        public float Spirit { get; private set; }

        /// <summary>
        /// 生命值
        /// </summary>
        [JsonProperty]
        public float Hp { get; private set; }

        /// <summary>
        /// 法力值
        /// </summary>
        [JsonProperty]
        public float Mp { get; private set; }

        /// <summary>
        /// 攻击力
        /// </summary>
        [JsonProperty]
        public float Attack { get; private set; }

        /// <summary>
        /// 防御力
        /// </summary>
        [JsonProperty]
        public float Defense { get; private set; }

        /// <summary>
        /// 命中率
        /// </summary>
        [JsonProperty]
        public float HitRate { get; private set; }

        /// <summary>
        /// 闪避率
        /// </summary>
        [JsonProperty]
        public float DodgeRate { get; private set; }

        /// <summary>
        /// 格挡率
        /// </summary>
        [JsonProperty]
        public float BlockRate { get; private set; }

        /// <summary>
        /// 暴击率
        /// </summary>
        [JsonProperty]
        public float CriticalRate { get; private set; }

        /// <summary>
        /// 致命一击率
        /// </summary>
        [JsonProperty]
        public float CriticalStrikeRate { get; private set; }

        /// <summary>
        /// 圣光攻击力
        /// </summary>
        [JsonProperty]
        public float Light { get; private set; }

        /// <summary>
        /// 火焰攻击力
        /// </summary>
        [JsonProperty]
        public float Fire { get; private set; }

        /// <summary>
        /// 冰霜攻击力
        /// </summary>
        [JsonProperty]
        public float Ice { get; private set; }

        /// <summary>
        /// 黑暗攻击力
        /// </summary>
        [JsonProperty]
        public float Dark { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (AttrFactor exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class AttrFactorConfig : Config<AttrFactor>
    {
    }

    /// <summary>
    /// export excel AttrFactor data
    /// </summary>
    public sealed partial class Excel
    {
        /// <summary>
        /// config AttrFactorConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, AttrFactor> attrFactorConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static AttrFactor propAttrFactorById(int nId)
        {
            AttrFactor data;
            if (!attrFactorConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(AttrFactor).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}