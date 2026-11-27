using System;
using System.Collections.Generic;
using Config;
using Newtonsoft.Json;
using UnityEngine;
using Oathx.Entity;

/// <summary>Owns issued task instances and preserves each chapter's authored task order.</summary>
[Initialize(InstallType.Startup, true)]
public sealed class TaskCollectionEntity : XEntity
{
    /// <summary>Storage key derived from the selected chapter member.</summary>
    private static readonly string storageKey =
        StorageKey.For<TaskSystem, TaskCollectionEntity>(entity => entity.chapterId);

    /// <summary>All issued tasks, including claimed tasks retained for duplicate-claim protection.</summary>
    private readonly Dictionary<int, TaskEntity> tasks = new Dictionary<int, TaskEntity>();

    /// <summary>Task order copied from the current chapter definition.</summary>
    private readonly List<int> chapterTasks = new List<int>();

    /// <summary>The current chapter, or zero before the first chapter is issued.</summary>
    public int chapterId
    { get; private set; }

    /// <summary>Restores this collection only before its first chapter is installed.</summary>
    public void Restore()
    {
        if (!PlayerPrefs.HasKey(storageKey))
            return;

        string json = PlayerPrefs.GetString(storageKey);
        var saved = JsonConvert.DeserializeObject<TaskCollectionSave>(json);
        OpenChapter(saved.chapterId);
        foreach (var receipt in saved.tasks)
        {
            Dispatch(receipt.id);
            tasks[receipt.id].Restore(receipt.progress, receipt.status);
        }
    }

    /// <summary>Persists completed task transactions, including claim receipts.</summary>
    public void Save()
    {
        var saved = new TaskCollectionSave { chapterId = chapterId };
        foreach (var task in tasks.Values)
        {
            saved.tasks.Add(new TaskReceipt {
                id = task.configuration.ID,
                progress = task.progress,
                status = task.status
            });
        }

        PlayerPrefs.SetString(storageKey, JsonConvert.SerializeObject(saved));
        PlayerPrefs.Save();
    }

    /// <summary>Starts a chapter after its predecessor has been fully claimed.</summary>
    /// <param name="id">The explicit chapter configuration ID, not an array index.</param>
    public void OpenChapter(int id)
    {
        if (chapterId == id)
            return;

        foreach (int taskId in chapterTasks)
        {
            if (tasks[taskId].status != TaskStatus.Claimed)
                throw new InvalidOperationException("Claim the current chapter before opening another.");
        }

        var chapter = Excel.taskChapterConfig[id];
        var candidates = new Dictionary<int, TaskEntity>();
        foreach (int taskId in chapter.TaskIDs)
        {
            if (tasks.ContainsKey(taskId))
                throw new InvalidOperationException("A chapter cannot reissue a claimed task: " + taskId);

            candidates.Add(taskId, new TaskEntity(Excel.taskConfig[taskId]));
        }

        foreach (var pair in candidates)
            tasks.Add(pair.Key, pair.Value);

        chapterTasks.Clear();
        chapterTasks.AddRange(chapter.TaskIDs);
        chapterId = id;
    }

    /// <summary>Issues an explicitly selected side task without affecting chapter ordering.</summary>
    /// <param name="id">Task configuration ID.</param>
    /// <returns>Whether a new instance was issued.</returns>
    public bool Dispatch(int id)
    {
        if (tasks.ContainsKey(id))
            return false;

        tasks.Add(id, new TaskEntity(Excel.taskConfig[id]));

        return true;
    }

    /// <summary>Queries an issued instance; unknown IDs expose a caller error.</summary>
    /// <param name="id">Task configuration ID.</param>
    /// <returns>The task owned by this collection.</returns>
    public TaskEntity GetTask(int id)
    {
        return tasks[id];
    }

    /// <summary>Checks a reward receipt, including tasks not yet dispatched.</summary>
    /// <param name="id">Original task identity.</param>
    /// <returns>Whether the task reward has been claimed.</returns>
    public bool IsClaimed(int id)
    {
        return tasks.TryGetValue(id, out var task) && task.status == TaskStatus.Claimed;
    }

    /// <summary>Finds the first pending reward, then the first active task in chapter order.</summary>
    /// <returns>The selected task, or null when the chapter has been fully claimed.</returns>
    public TaskEntity GetTrackedTask()
    {
        foreach (int id in chapterTasks)
        {
            if (tasks[id].status == TaskStatus.Completed)
                return tasks[id];
        }

        foreach (int id in chapterTasks)
        {
            if (tasks[id].status == TaskStatus.Active)
                return tasks[id];
        }

        return null;
    }

    /// <summary>Lists issued instances without exposing the mutable collection.</summary>
    /// <returns>A read-only copy of task references.</returns>
    public IReadOnlyList<TaskEntity> GetTasks()
    {
        return new List<TaskEntity>(tasks.Values).AsReadOnly();
    }

    /// <summary>Releases child entities when the plugin releases this collection.</summary>
    public override void Destroy()
    {
        foreach (TaskEntity task in tasks.Values)
            task.Destroy();

        tasks.Clear();
        chapterTasks.Clear();
        chapterId = 0;

        base.Destroy();
    }
}

/// <summary>Local task collection persistence payload.</summary>
public sealed class TaskCollectionSave
{
    /// <summary>The selected chapter.</summary>
    public int chapterId { get; set; }

    /// <summary>All issued task receipts.</summary>
    public List<TaskReceipt> tasks { get; set; } = new List<TaskReceipt>();
}

/// <summary>Durable progress for one issued task.</summary>
public sealed class TaskReceipt
{
    /// <summary>The task configuration ID.</summary>
    public int id { get; set; }

    /// <summary>The accepted absolute progress.</summary>
    public long progress { get; set; }

    /// <summary>The stable task status.</summary>
    public TaskStatus status { get; set; }
}
