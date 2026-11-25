using Oathx.Core;
using Spine.Unity;
using UnityEngine;

/// <summary>Shared animation names used by actor presentation and authoring.</summary>
public static class ActorAnimationNames
{
    /// <summary>Stationary actor animation.</summary>
    public const string idle = "idle";

    /// <summary>Locomotion actor animation.</summary>
    public const string walk = "walk";

    /// <summary>Original player locomotion animation.</summary>
    public const string run = "run";
}

/// <summary>Projects an actor entity into a Transform and Spine animation without mutating its data.</summary>
public class ActorShape : XActionStateMachine
{
    /// <summary>Original Spine component owned exclusively by this view.</summary>
    [SerializeField]
    private SkeletonAnimation animationView;

    /// <summary>Camera used only to project logical facing onto the screen.</summary>
    [SerializeField]
    private Camera sceneCamera;

    /// <summary>Optional moving animation; static residents use only idle.</summary>
    [SerializeField]
    private string walkAnimation;

    /// <summary>Animation used for stationary logical phases.</summary>
    [SerializeField]
    private string idleAnimation = ActorAnimationNames.idle;

    /// <summary>The bound logical actor, owned by TownEntity rather than this shape.</summary>
    private ActorEntity actor;

    /// <summary>Whether this shape has registered its presentation states.</summary>
    private bool statesRegistered;

    /// <summary>Binds a valid entity without initializing or resetting its logical state.</summary>
    /// <param name="entity">The actor to display.</param>
    /// <returns>Whether the view bindings and animation contract are valid.</returns>
    public bool Bind(ActorEntity entity)
    {
        if (!entity || entity.phase == ActorPhase.Stopped || !animationView || !sceneCamera)
            return false;

        animationView.Initialize(false);
        if (animationView.Skeleton == null || animationView.Skeleton.Data.FindAnimation(idleAnimation) == null || (!string.IsNullOrEmpty(walkAnimation) && animationView.Skeleton.Data.FindAnimation(walkAnimation) == null))
            return false;

        if (!statesRegistered)
        {
            RegisterStates();
            statesRegistered = true;
        }

        Unbind();
        actor = entity;

        Refresh();

        return true;
    }

    /// <summary>Releases only the view binding, leaving the entity untouched.</summary>
    public void Unbind()
    {
        actor = null;
        ClearOrBlockState(null);

        if (animationView && animationView.AnimationState != null)
            animationView.AnimationState.ClearTracks();
    }

    /// <summary>Refreshes rendering from the latest authoritative entity state.</summary>
    /// <returns>Whether a live actor was presented.</returns>
    public bool Refresh()
    {
        if (!actor || !animationView || animationView.Skeleton == null || !sceneCamera)
            return false;

        ActionState current = GetCurrentState();
        if (current == null || !current.state.Equals(actor.phase))
            SetState(actor.phase);

        if (actor.phase == ActorPhase.Stopped)
            return false;

        transform.position = actor.position;
        float facing = Vector3.Dot(sceneCamera.transform.right, actor.direction);
        if (Mathf.Abs(facing) > 0.001f)
            animationView.Skeleton.ScaleX = facing < 0f ? -1f : 1f;

        return true;
    }

    /// <summary>Reads the final logical position after the framework entity update.</summary>
    protected virtual void LateUpdate()
    {
        if (Refresh())
            UpdateMachine(Time.deltaTime);
    }

    /// <summary>Clears the view reference without destroying or stopping its entity.</summary>
    protected virtual void OnDestroy()
    {
        Unbind();
    }

    /// <summary>Registers presentation states; derived shapes can extend the state callbacks.</summary>
    protected virtual void RegisterStates()
    {
        AddState(ActorPhase.Idle, EnterIdle);
        AddState(ActorPhase.Moving, EnterMoving);
        AddState(ActorPhase.Waiting, EnterIdle);
        AddState(ActorPhase.Returning, EnterMoving);
        AddState(ActorPhase.Stopped, EnterStopped);
    }

    /// <summary>Starts the stationary animation once on state entry.</summary>
    private void EnterIdle()
    {
        animationView.AnimationState.SetAnimation(0, idleAnimation, true);
    }

    /// <summary>Starts locomotion, retaining idle for residents without a walk animation.</summary>
    private void EnterMoving()
    {
        string animation = string.IsNullOrEmpty(walkAnimation) ? idleAnimation : walkAnimation;
        animationView.AnimationState.SetAnimation(0, animation, true);
    }

    /// <summary>Stops presentation when the owning entity has been released.</summary>
    private void EnterStopped()
    {
        animationView.AnimationState.ClearTracks();
    }

}
