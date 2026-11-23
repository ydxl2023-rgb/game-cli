using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// idle state
/// </summary>
public class AIIdle : AIAction
{
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="actorEntity"></param>
    public AIIdle(ActorEntity actorEntity) 
        : base(ActionCommand.Idle, actorEntity)
    {

    }

    /// <summary>
    /// enter state
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override bool OnEnter()
    {
        return base.OnEnter();
    }

    /// <summary>
    /// update state
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    public override bool OnUpdate(float deltaTime)
    {
        return base.OnUpdate(deltaTime);
    }

    /// <summary>
    /// leave state
    /// </summary>
    /// <returns></returns>
    public override bool OnLeave()
    {
        return base.OnLeave();
    }
}
