using System;
using Config;
using Oathx.Entity;

/// <summary>Business states of an issued task, independent of guide presentation.</summary>
public enum TaskStatus
{
    /// <summary>The completion requirement has not been met.</summary>
    Active,
    /// <summary>The requirement is met and rewards may be claimed.</summary>
    Completed,
    /// <summary>The reward transaction has been accepted.</summary>
    Claimed,
    /// <summary>The synchronous reward transaction is currently committing.</summary>
    Claiming
}

/// <summary>Result of attempting the complete reward transaction.</summary>
public enum TaskClaimResult
{
    /// <summary>A reward receiver reentered the same claim before its transaction returned.</summary>
    InProgress,
    /// <summary>The task has not completed.</summary>
    NotCompleted,
    /// <summary>The task already has a claim receipt.</summary>
    AlreadyClaimed,
    /// <summary>No economy adapter is installed for nonempty rewards.</summary>
    MissingRewardReceiver,
    /// <summary>The economy rejected the transaction without applying any changes.</summary>
    Rejected,
    /// <summary>The rewards and claim receipt were accepted.</summary>
    Claimed
}

/// <summary>Commits all rewards atomically; false or an exception must leave the economy unchanged.</summary>
public interface ITaskRewardReceiver
{
    /// <summary>Grants a complete reward batch using the task ID as an idempotency key.</summary>
    /// <param name="configuration">The immutable exported task definition including rewards and quest value.</param>
    /// <returns>True only after the entire batch was applied or its prior receipt was found.</returns>
    bool Grant(Config.Task configuration);
}

/// <summary>Owns one issued task's configuration, completion rules, progress and claim state.</summary>
public sealed class TaskEntity : XEntity
{
    /// <summary>The exported definition used by all rules for this instance.</summary>
    public Config.Task configuration
    { get; private set; }

    /// <summary>Accepted absolute progress, capped at the configured target.</summary>
    public long progress
    { get; private set; }

    /// <summary>Current business state.</summary>
    public TaskStatus status
    { get; private set; }

    /// <summary>Creates one task instance from an exported definition.</summary>
    /// <param name="definition">The definition selected by the collection.</param>
    public TaskEntity(Config.Task definition) : base(definition.ID)
    {
        if (definition.FinishValue <= 0)
            throw new InvalidOperationException("Task target must be positive: " + definition.ID);

        configuration = definition;
    }

    /// <summary>Restores a validated local task receipt without executing rewards again.</summary>
    /// <param name="savedProgress">Saved absolute progress.</param>
    /// <param name="savedStatus">Saved stable task status.</param>
    public void Restore(long savedProgress, TaskStatus savedStatus)
    {
        if (savedProgress < 0 || savedProgress > configuration.FinishValue
            || savedStatus == TaskStatus.Claiming
            || !Enum.IsDefined(typeof(TaskStatus), savedStatus)
            || (savedStatus != TaskStatus.Active && savedProgress != configuration.FinishValue))
            throw new InvalidOperationException("Invalid saved task receipt: " + configuration.ID);

        progress = savedProgress;
        status = savedStatus;
    }

    /// <summary>Accepts an absolute observation only when its task and condition parameters match.</summary>
    /// <param name="observation">A trusted gameplay observation, never a UI completion request.</param>
    /// <returns>Whether progress or completion changed.</returns>
    public bool Apply(TaskProgressReported observation)
    {
        if (observation.value < 0)
            throw new ArgumentOutOfRangeException(nameof(observation.value));

        if (status != TaskStatus.Active
            || (observation.taskId != 0 && observation.taskId != configuration.ID)
            || observation.condition != configuration.TaskConditionType
            || !Matches(observation))
            return false;

        long next = Math.Min(observation.value, configuration.FinishValue);
        if (next <= progress)
            return false;

        progress = next;
        if (progress == configuration.FinishValue)
            status = TaskStatus.Completed;

        return true;
    }

    /// <summary>Records a claim only after the reward receiver commits the entire reward batch.</summary>
    /// <param name="receiver">Optional economy integration, required when rewards are nonempty.</param>
    /// <returns>The business result of the claim.</returns>
    public TaskClaimResult Claim(ITaskRewardReceiver receiver)
    {
        if (status == TaskStatus.Claiming)
            return TaskClaimResult.InProgress;

        if (status == TaskStatus.Claimed)
            return TaskClaimResult.AlreadyClaimed;

        if (status != TaskStatus.Completed)
            return TaskClaimResult.NotCompleted;

        if (configuration.Rewards.Count != 0 || configuration.QuestValue != 0)
        {
            if (receiver == null)
                return TaskClaimResult.MissingRewardReceiver;

            status = TaskStatus.Claiming;
            try
            {
                if (!receiver.Grant(configuration))
                    return TaskClaimResult.Rejected;
            }
            finally
            {
                status = TaskStatus.Completed;
            }
        }

        status = TaskStatus.Claimed;

        return TaskClaimResult.Claimed;
    }

    /// <summary>Matches the configured parameter shape without guessing unsupported list semantics.</summary>
    /// <param name="observation">Gameplay parameters accompanying the absolute progress.</param>
    /// <returns>Whether the observation targets this definition's condition.</returns>
    private bool Matches(TaskProgressReported observation)
    {
        switch (configuration.TaskConditionShape)
        {
            case ETaskConditionShape.EmptyCondition:
                return true;
            case ETaskConditionShape.OneParamCondition:
                return observation.param1 == configuration.Param1;
            case ETaskConditionShape.TwoParamCondition:
                return observation.param1 == configuration.Param1 && observation.param2 == configuration.Param2;
            case ETaskConditionShape.ThreeParamCondition:
                return observation.param1 == configuration.Param1
                    && observation.param2 == configuration.Param2
                    && observation.param3 == configuration.Param3;
            default:
                throw new NotSupportedException("Task condition adapter required: " + configuration.TaskConditionShape);
        }
    }
}
