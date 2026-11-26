using Oathx.Entity;
using UnityEngine;

/// <summary>Logical movement phases independent of scene objects and animation playback.</summary>
public enum ActorPhase
{
    /// <summary>The actor is at its origin.</summary>
    Idle,

    /// <summary>The actor is traveling toward its destination.</summary>
    Moving,

    /// <summary>The actor has reached its destination.</summary>
    Waiting,

    /// <summary>The actor is returning to its origin.</summary>
    Returning,

    /// <summary>The actor has been released by its owner.</summary>
    Stopped
}

/// <summary>Resolves movement against a world boundary without retaining a presentation object.</summary>
public interface IActorMovement
{
    /// <summary>Returns a reachable logical destination.</summary>
    /// <param name="origin">Current authoritative position.</param>
    /// <param name="target">Requested destination.</param>
    /// <param name="result">Reachable destination when successful.</param>
    /// <returns>Whether navigation resolved the movement.</returns>
    bool TryMove(Vector3 origin, Vector3 target, out Vector3 result);
}

/// <summary>Detached initialization data copied from scene authoring or product configuration.</summary>
public sealed class ActorSpawn
{
    /// <summary>Stable instance identifier, distinct from the resource configuration.</summary>
    public int actorId
    { get; set; }

    /// <summary>Character configuration identifier.</summary>
    public int configId
    { get; set; }

    /// <summary>Optional destination building identifier.</summary>
    public int destinationId
    { get; set; }

    /// <summary>Logical distance traveled per second.</summary>
    public float moveSpeed
    { get; set; }

    /// <summary>Optional manual movement resolver; null retains authored route behavior.</summary>
    public IActorMovement movement
    { get; set; }

    /// <summary>World-space route values with no retained Transform references.</summary>
    public Vector3[] route
    { get; set; }
}

/// <summary>Owns actor identity, position and movement rules without a scene or Spine dependency.</summary>
public sealed class ActorEntity : XEntity
{
    /// <summary>Boundary adapter for manually controlled actors.</summary>
    private IActorMovement movement;

    /// <summary>Normalized analog movement intent independent of presentation.</summary>
    private Vector3 input;

    /// <summary>Private route copy that cannot be modified by the authoring layer.</summary>
    private Vector3[] route;

    /// <summary>The next route point to reach.</summary>
    private int waypoint;

    /// <summary>Whether initialization completed exactly once.</summary>
    private bool initialized;

    /// <summary>Stable actor instance identifier.</summary>
    public int actorId
    { get; private set; }

    /// <summary>Character configuration identifier.</summary>
    public int configId
    { get; private set; }

    /// <summary>Building associated with the authored route.</summary>
    public int destinationId
    { get; private set; }

    /// <summary>Logical actor position, the sole authoritative movement position.</summary>
    public Vector3 position
    { get; private set; }

    /// <summary>Last nonzero logical movement direction.</summary>
    public Vector3 direction
    { get; private set; }

    /// <summary>Logical movement speed.</summary>
    public float moveSpeed
    { get; private set; }

    /// <summary>Current logical movement phase.</summary>
    public ActorPhase phase
    { get; private set; } = ActorPhase.Stopped;

    /// <summary>Number of completed outbound visits.</summary>
    public int arrivals
    { get; private set; }

    /// <summary>Advances logical movement independently of rendering or active scene objects.</summary>
    /// <param name="elapsed">Nonnegative elapsed simulation time in seconds.</param>
    public override void Update(float elapsed)
    {
        if (!initialized
            || phase == ActorPhase.Stopped
            || (movement == null
            && phase != ActorPhase.Moving
            && phase != ActorPhase.Returning)
            || !Finite(elapsed)
            || elapsed <= 0f)
            return;

        float remaining = moveSpeed * elapsed;
        if (!Finite(remaining))
            return;

        if (movement != null)
        {
            if (input == Vector3.zero)
                return;

            Vector3 target = position + input * remaining;
            if (movement.TryMove(position, target, out Vector3 result)
                && Finite(result.x)
                && Finite(result.y)
                && Finite(result.z)
                && Vector3.ProjectOnPlane(result - position, Vector3.up).magnitude <= remaining + 0.01f)
            {
                position = result;
                phase = ActorPhase.Moving;
                direction = input.normalized;
            }
            else
            {
                phase = ActorPhase.Idle;
            }

            return;
        }

        bool outbound = phase == ActorPhase.Moving;
        while (waypoint >= 0 && waypoint < route.Length)
        {
            Vector3 delta = route[waypoint] - position;
            float distance = delta.magnitude;
            if (distance > 0f)
                direction = delta / distance;

            if (distance > remaining)
            {
                position += direction * remaining;

                return;
            }

            position = route[waypoint];
            remaining -= distance;
            waypoint += outbound ? 1 : -1;
        }

        phase = outbound ? ActorPhase.Waiting : ActorPhase.Idle;
        if (outbound)
            arrivals++;
    }

    /// <summary>Follows a complete native navigation path without modifying the presentation directly.</summary>
    /// <param name="corners">Reachable world-space corners beginning at the current logical position.</param>
    /// <returns>Whether this live NPC accepted a nonempty finite path.</returns>
    public bool FollowRoute(Vector3[] corners)
    {
        if (!initialized || phase == ActorPhase.Stopped || movement != null || corners.Length == 0)
            return false;

        foreach (Vector3 point in corners)
            if (!Finite(point.x) || !Finite(point.y) || !Finite(point.z))
                return false;

        route = (Vector3[])corners.Clone();
        waypoint = 0;
        phase = ActorPhase.Moving;

        return true;
    }

    /// <summary>Stops released logical actors even if a detached shape still references them.</summary>
    public override void Destroy()
    {
        phase = ActorPhase.Stopped;
        route = null;
        movement = null;
        input = Vector3.zero;

        base.Destroy();
    }

    /// <summary>Validates all input before installing a private route copy.</summary>
    /// <param name="spawn">Detached actor initialization values.</param>
    /// <returns>Whether this actor was initialized without invalid or partial state.</returns>
    public bool Initialize(ActorSpawn spawn)
    {
        if (initialized
            || spawn == null
            || spawn.actorId <= 0
            || spawn.configId <= 0
            || spawn.destinationId < 0
            || !Finite(spawn.moveSpeed)
            || spawn.moveSpeed <= 0f
            || spawn.route == null
            || spawn.route.Length == 0)
            return false;

        foreach (Vector3 point in spawn.route)
        {
            if (!Finite(point.x) || !Finite(point.y) || !Finite(point.z))
                return false;
        }

        for (int index = 1; index < spawn.route.Length; index++)
        {
            if (!Finite((spawn.route[index] - spawn.route[index - 1]).magnitude))
                return false;
        }

        actorId = spawn.actorId;
        iid = spawn.actorId;
        configId = spawn.configId;
        destinationId = spawn.destinationId;
        moveSpeed = spawn.moveSpeed;
        movement = spawn.movement;
        route = (Vector3[])spawn.route.Clone();
        position = route[0];
        phase = ActorPhase.Idle;
        initialized = true;

        return true;
    }

    /// <summary>Accepts finite horizontal analog intent for a live manually controlled actor.</summary>
    /// <param name="value">World-space movement direction and strength.</param>
    /// <returns>Whether the input was accepted.</returns>
    public bool SetMoveInput(Vector3 value)
    {
        if (!initialized
            || phase == ActorPhase.Stopped
            || movement == null
            || !Finite(value.x)
            || !Finite(value.y)
            || !Finite(value.z)
            || !Finite(value.sqrMagnitude))
            return false;

        value.y = 0f;
        input = Vector3.ClampMagnitude(value, 1f);
        if (input == Vector3.zero)
            phase = ActorPhase.Idle;

        return true;
    }

    /// <summary>Accepts a visit only for the configured destination from the origin.</summary>
    /// <param name="buildingId">Requested destination identifier.</param>
    /// <returns>Whether outbound movement started.</returns>
    public bool Visit(int buildingId)
    {
        if (!initialized
            || movement != null
            || phase != ActorPhase.Idle
            || route.Length < 2
            || buildingId <= 0
            || buildingId != destinationId)
            return false;

        phase = ActorPhase.Moving;
        waypoint = 1;

        return true;
    }

    /// <summary>Accepts a return only after a completed outbound visit.</summary>
    /// <returns>Whether return movement started.</returns>
    public bool Return()
    {
        if (!initialized || phase != ActorPhase.Waiting)
            return false;

        phase = ActorPhase.Returning;
        waypoint = route.Length - 2;

        return true;
    }

    /// <summary>Rejects invalid numeric input before it can pollute movement state.</summary>
    /// <param name="value">Number to validate.</param>
    /// <returns>Whether the number is finite.</returns>
    private static bool Finite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
