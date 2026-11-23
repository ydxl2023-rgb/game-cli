using Oathx.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAction : AIState
{
    public ActorEntity actor
    { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    /// <param name="actorEntity"></param>
    public AIAction(Enum state, ActorEntity actorEntity)
        : base(state)
    {
        actor = actorEntity;
    }
}
