using System.Collections;
using Oathx.Core;
using Oathx.Entity;
using UnityEngine;

/// <summary>Stable numeric configuration keys used at task system boundaries.</summary>
public static class TaskConfigurationIds
{
    /// <summary>The TaskSystem row in the function system table.</summary>
    public const int system = 10004;

    /// <summary>The product task settings row.</summary>
    public const int settings = 1;
}

/// <summary>Coordinates task requests, entity rules and owned guide presentation through events.</summary>
public sealed class TaskSystem : RuntimeSystem
{
    /// <summary>Presentation execution owned by this installed system.</summary>
    private TaskGuideRunner guides;

    /// <summary>Guide clock stopped when this system is deactivated.</summary>
    private Coroutine guideClock;

    /// <summary>Optional session economy integration, distinct from task claim receipts.</summary>
    private ITaskRewardReceiver rewards;

    /// <summary>The task whose guide is currently selected.</summary>
    private int guidedTaskId;

    /// <summary>Subscribes to task intentions and installs the owned runtime work.</summary>
    /// <returns>Whether task installation succeeded.</returns>
    public override bool OnActive()
    {
        Subscribe<TaskDispatchRequested>(OnDispatch);
        Subscribe<TaskChapterRequested>(OnChapter);
        Subscribe<TaskProgressReported>(OnProgress);
        Subscribe<TaskClaimRequested>(OnClaim);
        Subscribe<TaskRewardsRegistered>(OnRewardsRegistered);
        Subscribe<TaskGuideRequested>(OnGuide);
        Subscribe<TaskGuideCancelled>(OnGuideCancelled);
        Subscribe<TaskGuideRetryRequested>(OnGuideRetry);

        return InstallTasks();
    }

    /// <summary>Releases subscriptions and cancels only the owned guide presentation.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        UninstallTasks();
    }

    /// <summary>Initializes the configured first chapter and the expression runtime.</summary>
    /// <returns>True after synchronous initialization.</returns>
    private bool InstallTasks()
    {
        var collection = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (collection.chapterId == 0)
            collection.Restore();

        if (collection.chapterId == 0)
        {
            var settings = Config.Excel.taskSettingConfig[TaskConfigurationIds.settings];
            collection.OpenChapter(settings.InitialChapterID);
        }

        guides = new TaskGuideRunner(Config.Excel.taskGuideConfig);
        guideClock = XCoroutine.Run(UpdateGuides());

        return true;
    }

    /// <summary>Stops the clock and releases guide registrations and session adapters.</summary>
    private void UninstallTasks()
    {
        if (guideClock != null)
        {
            XCoroutine.StopCoroutine(guideClock);
            guideClock = null;
        }

        guides?.Clear();
        guides = null;
        rewards = null;
        guidedTaskId = 0;
    }

    /// <summary>Ticks presentation without polling task completion rules.</summary>
    /// <returns>The owned frame clock.</returns>
    private IEnumerator UpdateGuides()
    {
        while (true)
        {
            yield return null;

            try
            {
                guides.Tick(Time.deltaTime);
            }
            catch (System.Exception exception)
            {
                guides.Cancel();
                Debug.LogException(exception);
            }
        }
    }

    /// <summary>Issues a configured task once.</summary>
    /// <param name="message">The dispatch request.</param>
    private void OnDispatch(TaskDispatchRequested message)
    {
        var collection = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (collection.Dispatch(message.taskId))
        {
            collection.Save();
            XPluginUtility.PostEvent(new TaskChanged { taskId = message.taskId });
        }
    }

    /// <summary>Opens an explicitly requested chapter after entity validation.</summary>
    /// <param name="message">The chapter request.</param>
    private void OnChapter(TaskChapterRequested message)
    {
        var collection = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        collection.OpenChapter(message.chapterId);
        guides.Cancel();
        guidedTaskId = 0;
        collection.Save();

        XPluginUtility.PostEvent(new TaskChanged());
    }

    /// <summary>Passes trusted observations to the matching task's business rules.</summary>
    /// <param name="message">The absolute gameplay observation.</param>
    private void OnProgress(TaskProgressReported message)
    {
        var collection = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var task = collection.GetTask(message.taskId);
        if (!task.Apply(message))
            return;

        if (task.status == TaskStatus.Completed && guidedTaskId == message.taskId)
        {
            guides.Cancel();
            guidedTaskId = 0;
        }

        collection.Save();
        XPluginUtility.PostEvent(new TaskChanged { taskId = message.taskId });
    }

    /// <summary>Runs the entity-owned reward transaction and reports its actual outcome.</summary>
    /// <param name="message">The claim request.</param>
    private void OnClaim(TaskClaimRequested message)
    {
        var collection = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var task = collection.GetTask(message.taskId);
        TaskClaimResult result = task.Claim(rewards);

        collection.Save();

        XPluginUtility.PostEvent(new TaskClaimed { taskId = message.taskId, result = result });
        if (result == TaskClaimResult.Claimed)
            XPluginUtility.PostEvent(new TaskChanged { taskId = message.taskId });
    }

    /// <summary>Connects or disconnects the economy integration for this system lifetime.</summary>
    /// <param name="message">The integration registration.</param>
    private void OnRewardsRegistered(TaskRewardsRegistered message)
    {
        rewards = message.receiver;
    }

    /// <summary>Starts only the requested active task's configured guide.</summary>
    /// <param name="message">The task selected for guidance.</param>
    private void OnGuide(TaskGuideRequested message)
    {
        var collection = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var task = collection.GetTask(message.taskId);
        if (task.status != TaskStatus.Active)
            return;

        guides.Start(task.configuration.GuideID);
        guidedTaskId = message.taskId;
    }

    /// <summary>Cancels the selected guide without changing task progress.</summary>
    /// <param name="message">The explicit cancellation request.</param>
    private void OnGuideCancelled(TaskGuideCancelled message)
    {
        guides.Cancel();
        guidedTaskId = 0;
    }

    /// <summary>Rechecks a condition following a business state change.</summary>
    /// <param name="message">The explicit retry request.</param>
    private void OnGuideRetry(TaskGuideRetryRequested message)
    {
        guides.Retry();
    }
}
