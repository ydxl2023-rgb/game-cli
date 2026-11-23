using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class Character
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
        /// AI
        /// </summary>
        [JsonProperty]
        public int AI { get; private set; }

        /// <summary>
        /// 角色类型
        /// </summary>
        [JsonProperty]
        public ECharacterType CharacterType { get; private set; }

        /// <summary>
        /// 角色默认所属层
        /// </summary>
        [JsonProperty]
        public ECameraLayer CameraLayer { get; private set; }

        /// <summary>
        /// 角色属性ID
        /// </summary>
        [JsonProperty]
        public int AttrId { get; private set; }

        /// <summary>
        /// 属性修正
        /// </summary>
        [JsonProperty]
        public int AttrFactor { get; private set; }

        /// <summary>
        /// 职业类型
        /// </summary>
        [JsonProperty]
        public EProfessionType ProfessionType { get; private set; }

        /// <summary>
        /// 骨骼
        /// </summary>
        [JsonProperty]
        public string Skeleton { get; private set; }

        /// <summary>
        /// 动画动作器
        /// </summary>
        [JsonProperty]
        public string OverrideController { get; private set; }

        /// <summary>
        /// 默认装备
        /// </summary>
        [JsonProperty]
        public List<int> Equips { get; private set; }

        /// <summary>
        /// 默认技能
        /// </summary>
        [JsonProperty]
        public List<int> Skills { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (Character exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class CharacterConfig : Config<Character>
    {
    }

    /// <summary>
    /// export excel Character data
    /// </summary>
    public sealed partial class Excel
    {
        public const int CHAR_BOY_WARRIOR = 10001;
        public const int CHAR_BOY_MAGIC = 10002;
        public const int CHAR_BOY_PASTOR = 10003;
        public const int CHAR_BOY_ROBBERS = 10004;
        public const int CHAR_BOY_HUNTER = 10005;
        public const int CHAR_BOY_MONSTER = 20001;
        public const int CHAR_BOY_NPC = 30001;
        /// <summary>
        /// config CharacterConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, Character> characterConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static Character propCharacterById(int nId)
        {
            Character data;
            if (!characterConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(Character).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}