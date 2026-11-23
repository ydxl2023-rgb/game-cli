using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMove : AIAction
{
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="actorEntity"></param>
    public AIMove(ActorEntity actorEntity)
        : base(ActionCommand.Move, actorEntity)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override bool OnEnter()
    {
        return base.OnEnter();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    public override bool OnUpdate(float deltaTime)
    {
        return base.OnUpdate(deltaTime);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool OnLeave()
    {
        return base.OnLeave();
    }
}
