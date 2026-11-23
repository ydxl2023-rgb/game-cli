using System;
using System.Collections;
using System.Collections.Generic;
using Oathx.Entity;
using UnityEngine;
using Config;
using Newtonsoft.Json;

/// <summary>
/// Avatar, used to store the hero skeleton info, hero current equip elements, hero current attachments
/// </summary>
[Serializable]
public class Avatar
{
    /// <summary>
    /// hero skeleton info
    /// </summary>
    public string skeletion
    { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string overrideController
    { get; set; }

    /// <summary>
    /// hero current equip elements
    /// </summary>
    public List<string> elements
    { get; set; }

    /// <summary>
    /// hero current attachments
    /// </summary>
    public Dictionary<string, List<string>>
        attachments = new Dictionary<string, List<string>>();
}

/// <summary>
/// actor type, used to identify the actor type, such as worker, customer, cashier, etc.
/// </summary>
public enum ActorType
{
    None = 0,
    Worker = 1,
    Customer = 2,
    Cashier = 4
}

/// <summary>
/// property definition, used to identify the character config, 
/// such as the character name, the character type, the character attribute, etc.
/// </summary>
public class PropertyDef
{
    public const int LV = 0;
    public const int EXP = 1;
    public const int SPEED = 2;
}

/// <summary>
/// fight actor entity
/// </summary>
[RequireComponent(typeof(ActorProperty))]
public class ActorEntity : XEntity
{
    /// <summary>
    /// actor type, used to identify the actor type, such as worker, customer, cashier, etc.
    /// </summary>
    public ActorType actorType
    { get; set; }

    /// <summary>
    /// character config, used to identify the character config, 
    /// such as the character name, the character type, the character attribute, etc.
    /// </summary>
    [JsonIgnore]
    public Character prop
    { get; protected set; }

    /// <summary>
    /// excel property id, used to identify the character config, 
    /// such as the character name, the character type, the character attribute, etc.
    /// </summary>
    [JsonProperty]
    public int propId
    { get; protected set; }

    /// <summary>
    /// attribute factor, used to calculate the final attribute value, 
    /// such as hp, attack, defense, etc.
    /// </summary>
    [JsonProperty]
    public int factor
    { get; set; }

    /// <summary>
    /// actor level, used to calculate the final attribute value, such as hp, attack, defense, etc.
    /// </summary>
    [JsonProperty]
    public int level
    { get; set; }

    /// <summary>
    /// hero avatar, used to store the hero skeleton info,
    /// hero current equip elements, hero current attachments
    /// </summary>
    protected readonly Dictionary<EPartType, ItemEntity> equips = new Dictionary<EPartType, ItemEntity>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="excelPropId"></param>
    /// <param name="defaultLevel"></param>
    /// <param name="defaultFactor"></param>
    public ActorEntity(int excelPropId, int defaultLevel, int defaultFactor)
    {
        actorType = ActorType.None;
        propId = excelPropId;
        factor = defaultFactor;
        level = level;

        InitProp(excelPropId);
        InitAttrs();
    }

    /// <summary>
    /// initialize the character config, if the excel property id is different from the current property id,
    /// </summary>
    /// <param name="excelPropId"></param>
    protected void InitProp(int excelPropId)
    {
        if (propId != excelPropId)
        {
            prop = Excel.propCharacterById(excelPropId);
        }
    }

    /// <summary>
    /// initialize the actor attributes
    /// </summary>
    protected void InitAttrs()
    {
        Dictionary<int, int> complexs = new Dictionary<int, int>()
        {
            { PropertyDef.LV, 1 },
            { PropertyDef.EXP, 0 },
            { PropertyDef.SPEED, 1 },
        };

        ApplyAttrs(complexs);
    }
}

/// <summary>
/// actor level component, used to store the actor level, and calculate the final attribute value, such as hp, attack, defense, etc.
/// </summary>
public class ActorProperty : XEntityComponent<FightEntity>
{
    public ActorProperty()
    {

    }
}