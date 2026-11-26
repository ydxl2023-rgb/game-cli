/// <summary>Highlights any registered UI target until its actual button is clicked.</summary>
public sealed class TaskFingerAction : ITaskAction
{
    /// <summary>The target token copied from configuration.</summary>
    private readonly string targetKey;

    /// <summary>The registered UI instance owning the finger.</summary>
    private TaskGuideTarget target;

    /// <summary>Records page closure even if the target is reenabled before the next frame.</summary>
    private bool targetClosed;

    /// <summary>Current presentation outcome, independent of task progress.</summary>
    public TaskActionStatus status
    { get; private set; }

    /// <summary>Creates the same action for any configured UI target.</summary>
    /// <param name="key">Target token from the guide expression.</param>
    public TaskFingerAction(string key)
    {
        targetKey = key;
    }

    /// <summary>Shows the registered target's authored finger.</summary>
    public void Start()
    {
        target = TaskGuideTarget.Resolve(targetKey);
        target.Show(this);
    }

    /// <summary>Waits for the target's real click or page closure.</summary>
    /// <param name="elapsed">Unused elapsed seconds.</param>
    public void Tick(float elapsed)
    {
        if (targetClosed || !target || !target.isActiveAndEnabled)
            Finish(false);
    }

    /// <summary>Records closure while allowing the current button invocation to finish first.</summary>
    public void OnTargetClosed()
    {
        targetClosed = true;
    }

    /// <summary>Finishes this presentation without reporting business progress.</summary>
    /// <param name="succeeded">Whether the target was clicked before it closed.</param>
    public void Finish(bool succeeded)
    {
        if (status != TaskActionStatus.Running)
            return;

        status = succeeded ? TaskActionStatus.Succeeded : TaskActionStatus.Failed;
        if (target)
            target.Hide(this);
    }

    /// <summary>Removes the finger when its guide is replaced or its system stops.</summary>
    public void Cancel()
    {
        status = TaskActionStatus.Cancelled;
        if (target)
            target.Hide(this);
    }
}
