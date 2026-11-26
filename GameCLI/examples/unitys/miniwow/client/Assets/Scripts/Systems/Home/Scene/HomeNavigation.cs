using UnityEngine;
using UnityEngine.AI;

/// <summary>Adapts the original NavMeshAgent.Move behavior to authoritative actor data.</summary>
public sealed class HomeNavigation : IActorMovement
{
    /// <summary>Scene-owned navigation proxy, independent of the actor's visual hierarchy.</summary>
    private readonly NavMeshAgent agent;

    /// <summary>Requires an explicitly bound navigation proxy on the original navigation data.</summary>
    /// <param name="agent">Scene-owned agent with original configuration.</param>
    public HomeNavigation(NavMeshAgent agent)
    {
        if (!agent)
            throw new System.ArgumentNullException(nameof(agent));

        this.agent = agent;
    }

    /// <summary>Moves with Unity's surface constraint and wall sliding, without writing a Shape.</summary>
    /// <param name="origin">Current authoritative entity position.</param>
    /// <param name="target">Requested horizontal movement endpoint.</param>
    /// <param name="result">Navigation result for the entity to validate and accept.</param>
    /// <returns>Whether the live agent resolved the movement.</returns>
    public bool TryMove(Vector3 origin, Vector3 target, out Vector3 result)
    {
        result = origin;
        if (!agent || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return false;

        if ((agent.nextPosition - origin).sqrMagnitude > 0.0001f && !agent.Warp(origin))
            return false;

        agent.Move(target - origin);
        result = agent.nextPosition;

        return true;
    }
}
