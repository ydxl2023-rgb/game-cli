using System.Collections.Generic;

namespace Config
{
    /// <summary>
    /// 角色类型
    /// </summary>
    public enum ECharacterType
    {
        None = 0,
        Hero = 1,
        Monster = 2,
        Npc = 4,
    }

    /// <summary>
    /// 角色默认所属层
    /// </summary>
    public enum ECameraLayer
    {
        None = 0,
        Unit = 1,
    }

    /// <summary>
    /// 职业类型
    /// </summary>
    public enum EProfessionType
    {
        None = 0,
        Hunter = 1,
        Magic = 2,
        Pastor = 4,
        Robbers = 8,
        Warrior = 16,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ETargetType
    {
        None = 0,
        Self = 1,
        Target = 2,
    }

    /// <summary>
    /// 道具类型
    /// </summary>
    public enum EItemType
    {
        None = 0,
        Equip = 1,
    }

    /// <summary>
    /// 装备部位
    /// </summary>
    public enum EPartType
    {
        None = 0,
        Assistant = 1,
        Belt = 2,
        Cloth = 4,
        Crown = 8,
        Face = 16,
        Glove = 32,
        Hair = 64,
        HairHalf = 128,
        Hat = 256,
        Helm = 512,
        Shoe = 1024,
        ShoulderPad = 2048,
        Weapon = 4096,
    }

    /// <summary>
    /// 道具品质
    /// </summary>
    public enum EQuality
    {
        None = 0,
        Orange = 1,
        White = 2,
    }

    /// <summary>
    /// Condition detector
    /// </summary>
    public enum ERedDotCondition
    {
        None = 0,
        SignalCount = 1,
    }

    /// <summary>
    /// Display mode
    /// </summary>
    public enum ERedDotDisplay
    {
        None = 0,
        Count = 1,
        Dot = 2,
    }

    /// <summary>
    /// 技能类型
    /// </summary>
    public enum ESkillType
    {
        Damage = 0,
        Heal = 1,
        None = 2,
    }

    /// <summary>
    /// 技能目标
    /// </summary>
    public enum ESkillTarget
    {
        None = 0,
        Enemy = 1,
        Friend = 2,
        Patient = 4,
        Self = 8,
        Target = 16,
    }

    /// <summary>
    /// 检测类型
    /// </summary>
    public enum ESkillCheckType
    {
        None = 0,
        Fanshaped = 1,
        Target = 2,
    }
}