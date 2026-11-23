using Oathx.Entity;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// fight entity, used to store the fight data, such as the fight state, the fight result, the fight score, etc.
/// </summary>
[RequireEntityComponent(typeof(ActorProperty))]
public partial class FightEntity : XEntitySerialize
{
    /// <summary>
    /// fight state, such as prepare, fighting, result, etc.
    /// </summary>
    public List<ActorEntity> actors
    { get; protected set; } = new List<ActorEntity>();

    /// <summary>
    /// fight config id, used to identify the fight config, such as the fight level, the fight mode, etc.
    /// </summary>
    public int configId
    { get; protected set; }

    /// <summary>
    /// constructor, create a fight entity with the fight config id
    /// </summary>
    /// <param name="fightId"></param>
    public FightEntity(int fightId)
    {
        configId = fightId;
    }
}
