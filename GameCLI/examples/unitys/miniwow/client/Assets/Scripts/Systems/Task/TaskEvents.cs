using Oathx.Core;

/// <summary>Reports an absolute gameplay observation matched by configured condition parameters.</summary>
public sealed class TaskProgressReported : IEvent
{
    /// <summary>Optional task scope; zero matches every issued task with the same condition parameters.</summary>
    public int taskId
    { get; set; }

    /// <summary>The observed condition kind.</summary>
    public Config.ETaskConditionType condition
    { get; set; }

    /// <summary>Absolute progress; repeated reports do not accumulate twice.</summary>
    public long value
    { get; set; }

    /// <summary>Optional absolute counts keyed by configuration identity for list-filtered conditions.</summary>
    public System.Collections.Generic.IReadOnlyDictionary<int, long> itemCounts
    { get; set; }

    /// <summary>First condition parameter.</summary>
    public int param1
    { get; set; }

    /// <summary>Second condition parameter.</summary>
    public int param2
    { get; set; }

    /// <summary>Third condition parameter.</summary>
    public int param3
    { get; set; }
}

/// <summary>Requests explicit issuance of a task.</summary>
public sealed class TaskDispatchRequested : IEvent
{
    /// <summary>Task configuration ID.</summary>
    public int taskId
    { get; set; }
}

/// <summary>Requests an explicit chapter transition.</summary>
public sealed class TaskChapterRequested : IEvent
{
    /// <summary>Chapter configuration ID.</summary>
    public int chapterId
    { get; set; }
}

/// <summary>Requests a reward claim for a completed task.</summary>
public sealed class TaskClaimRequested : IEvent
{
    /// <summary>Explicit issued task ID whose reward is requested.</summary>
    public int taskId
    { get; set; }
}

/// <summary>Reports an issued task state change.</summary>
public sealed class TaskChanged : IEvent
{
    /// <summary>Changed task ID.</summary>
    public int taskId
    { get; set; }
}

/// <summary>Reports the outcome of a claim attempt.</summary>
public sealed class TaskClaimed : IEvent
{
    /// <summary>Requested task ID.</summary>
    public int taskId
    { get; set; }

    /// <summary>Claim result, including rejected or unavailable rewards.</summary>
    public TaskClaimResult result
    { get; set; }
}

/// <summary>Connects the session's economy receiver.</summary>
public sealed class TaskRewardsRegistered : IEvent
{
    /// <summary>Reward receiver; null explicitly disconnects it.</summary>
    public ITaskRewardReceiver receiver
    { get; set; }
}

/// <summary>Requests presentation for an active task.</summary>
public sealed class TaskGuideRequested : IEvent
{
    /// <summary>Optional task scope; zero matches every issued task with the same condition parameters.</summary>
    public int taskId
    { get; set; }
}

/// <summary>Cancels the currently owned guide.</summary>
public sealed class TaskGuideCancelled : IEvent
{
}

/// <summary>Rechecks a blocked guide condition after business state changes.</summary>
public sealed class TaskGuideRetryRequested : IEvent
{
}

/// <summary>Requests an external presentation action; completion belongs to its consumer.</summary>
public sealed class TaskPresentationRequested : IEvent
{
    /// <summary>Set by the action owner when it accepts responsibility for completion and cancellation.</summary>
    public bool accepted
    { get; set; }

    /// <summary>The action handle to complete or reject.</summary>
    public TaskPresentationAction action
    { get; set; }
}

/// <summary>Requests cleanup of a canceled presentation.</summary>
public sealed class TaskPresentationCancelled : IEvent
{
    /// <summary>The original action handle.</summary>
    public TaskPresentationAction action
    { get; set; }
}

