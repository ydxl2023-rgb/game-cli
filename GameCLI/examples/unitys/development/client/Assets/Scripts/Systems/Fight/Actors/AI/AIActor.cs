using Oathx.Core;
using System.Collections.Generic;

/// <summary>
/// action command, used to control AI state machine, such as change state, change target, change parameter, etc.
/// </summary>
public enum ActionCommand
{
    None,
    Idle,
    Move,
    Work
}

/// <summary>
/// AI input, control AI state machine, such as change state, change target, change parameter, etc.
/// </summary>
public abstract class AIActor : AIStateMachine
{
    /// <summary>
    /// actor entity
    /// </summary>
    public ActorEntity actor
    { get; protected set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="target"></param>
    public AIActor(ActorEntity target)
    {
        actor = target;
    }
}
