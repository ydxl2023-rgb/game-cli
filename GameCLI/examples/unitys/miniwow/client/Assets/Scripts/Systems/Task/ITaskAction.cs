using System;
using Oathx.Expression;
using Oathx.Core;

/// <summary>Presentation outcomes are separate from task completion.</summary>
public enum TaskActionStatus
{
    /// <summary>The presentation has not finished.</summary>
    Running,
    /// <summary>The consumer confirmed successful completion.</summary>
    Succeeded,
    /// <summary>The consumer reported a failure.</summary>
    Failed,
    /// <summary>The owner canceled this presentation.</summary>
    Cancelled
}

/// <summary>Typed presentation operations available to task guide adapters.</summary>
public enum TaskPresentation
{
    /// <summary>Move to the configured building.</summary>
    Navigate,
    /// <summary>Play the specified dialogue group.</summary>
    ShowDialogue,
    /// <summary>Open a configured page.</summary>
    OpenUI,
    /// <summary>Close a configured page.</summary>
    CloseUI,
    /// <summary>Highlight a UI target.</summary>
    GuideFinger,
    /// <summary>Restore the main page.</summary>
    ShowMain,
    /// <summary>Display a localized common text.</summary>
    Tips,
    /// <summary>Select an exploration area.</summary>
    SelectExploreArea
}

/// <summary>Executes and cancels presentation without modifying task business state.</summary>
public interface ITaskAction
{
    /// <summary>Current presentation outcome.</summary>
    TaskActionStatus status { get; }

    /// <summary>Starts the presentation.</summary>
    void Start();

    /// <summary>Advances a running presentation.</summary>
    /// <param name="elapsed">Elapsed seconds.</param>
    void Tick(float elapsed);

    /// <summary>Releases the presentation and invalidates its completion handle.</summary>
    void Cancel();
}

/// <summary>Waits for configured seconds without completing the task itself.</summary>
public sealed class TaskWaitAction : ITaskAction
{
    /// <summary>Seconds remaining in this presentation.</summary>
    private double remaining;

    /// <summary>Current waiting outcome.</summary>
    public TaskActionStatus status
    { get; private set; }

    /// <summary>Creates a wait action from configuration.</summary>
    /// <param name="seconds">Nonnegative duration in seconds.</param>
    public TaskWaitAction(double seconds)
    {
        if (seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds))
            throw new ArgumentOutOfRangeException(nameof(seconds));

        remaining = seconds;
    }

    /// <summary>Completes a zero-duration wait immediately.</summary>
    public void Start()
    {
        if (remaining == 0)
            status = TaskActionStatus.Succeeded;
    }

    /// <summary>Advances only a running wait.</summary>
    /// <param name="elapsed">Elapsed seconds.</param>
    public void Tick(float elapsed)
    {
        if (status != TaskActionStatus.Running)
            return;

        remaining -= elapsed;
        if (remaining <= 0)
            status = TaskActionStatus.Succeeded;
    }

    /// <summary>Stops this wait.</summary>
    public void Cancel()
    {
        status = TaskActionStatus.Cancelled;
    }
}

/// <summary>Owns a completion handle for an external presentation consumer.</summary>
public sealed class TaskPresentationAction : ITaskAction
{
    /// <summary>The typed operation requested by configuration.</summary>
    public TaskPresentation operation
    { get; }

    /// <summary>The copied argument; unused for ShowMain.</summary>
    public ExpressionValue argument
    { get; }

    /// <summary>Current outcome, initially awaiting the consumer.</summary>
    public TaskActionStatus status
    { get; private set; }

    /// <summary>Creates a request without retaining temporary expression argument views.</summary>
    /// <param name="operation">Presentation operation.</param>
    /// <param name="argument">Copied parameter value.</param>
    public TaskPresentationAction(TaskPresentation operation, ExpressionValue argument)
    {
        this.operation = operation;
        this.argument = argument;
    }

    /// <summary>Publishes the action handle to the presentation owner.</summary>
    public void Start()
    {
        XPluginUtility.FireEvent(new TaskPresentationRequested { action = this });
    }

    /// <summary>Waits for the consumer rather than inferring completion from elapsed time.</summary>
    /// <param name="elapsed">Unused elapsed seconds.</param>
    public void Tick(float elapsed)
    {
    }

    /// <summary>Accepts a consumer outcome while this action is still pending.</summary>
    /// <param name="succeeded">Whether the actual presentation succeeded.</param>
    public void Complete(bool succeeded)
    {
        if (status == TaskActionStatus.Running)
            status = succeeded ? TaskActionStatus.Succeeded : TaskActionStatus.Failed;
    }

    /// <summary>Invalidates late completion and requests cleanup from the presentation owner.</summary>
    public void Cancel()
    {
        if (status != TaskActionStatus.Running)
            return;

        status = TaskActionStatus.Cancelled;

        XPluginUtility.FireEvent(new TaskPresentationCancelled { action = this });
    }
}
