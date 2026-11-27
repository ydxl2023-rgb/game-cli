using System.Collections.Generic;
using System.Linq;
using Config;
using Oathx.Entity;

/// <summary>Configured introductory recruitment operations.</summary>
public enum RecruitStepKind
{
    /// <summary>Finishes the resident conversation.</summary>
    Conversation = 1,
    /// <summary>Consumes a whistle and plays the original recruitment movie.</summary>
    Horn,
    /// <summary>Receives a configured free hero.</summary>
    Receive
}

/// <summary>Recruitment receipts committed with village inventory and rewards.</summary>
public sealed class RecruitSave
{
    /// <summary>Completed interaction IDs; task rewards retain their separate existing receipts.</summary>
    public HashSet<int> completed
    { get; set; } = new HashSet<int>();

    /// <summary>Owned additional hero identities, independent of scene objects.</summary>
    public HashSet<int> heroes
    { get; set; } = new HashSet<int>();

    /// <summary>Paid horn interaction awaiting its original movie acknowledgement.</summary>
    public int pendingHorn
    { get; set; }
}

/// <summary>Owns interaction selection and atomic recruitment rules.</summary>
[Initialize(InstallType.Startup, true)]
public sealed class RecruitEntity : XEntity
{
    /// <summary>Selected conversation, or zero when no page owns a selection.</summary>
    public int selectedStep
    { get; private set; }

    /// <summary>Current row within the selected original dialogue group.</summary>
    public int dialogueIndex
    { get; private set; }

    /// <summary>Finds the eligible interaction for a building without hardcoding task IDs.</summary>
    /// <param name="buildingId">Original resident identity.</param>
    /// <param name="saved">Current village transaction snapshot.</param>
    /// <param name="tasks">Actual task status and prerequisite receipts.</param>
    /// <returns>The available step, or null when this resident has no interaction.</returns>
    public RecruitStep FindStep(int buildingId, HomeVillageSave saved, TaskCollectionEntity tasks)
    {
        return Excel.recruitStepConfig.Values.FirstOrDefault(step => step.BuildingID == buildingId
            && tasks.IsClaimed(step.RequiredTaskID) && !saved.recruit.completed.Contains(step.ID)
            && tasks.GetTask(step.ID).status == TaskStatus.Active);
    }

    /// <summary>Selects one available conversation for presentation.</summary>
    /// <param name="step">Entity-validated step.</param>
    public void Select(RecruitStep step)
    {
        selectedStep = step.ID;
        dialogueIndex = 0;
    }

    /// <summary>Reads the current original conversation row.</summary>
    /// <returns>The row currently displayed.</returns>
    public RecruitDialogue GetDialogue()
    {
        int group = Excel.recruitStepConfig[selectedStep].DialogueGroupID;

        return Excel.recruitDialogueConfig.Values.Where(row => row.GroupID == group)
            .OrderBy(row => row.ID).ElementAt(dialogueIndex);
    }

    /// <summary>Advances original dialogue without committing gameplay before the final line.</summary>
    /// <returns>Whether another line remains to be displayed.</returns>
    public bool NextDialogue()
    {
        int group = Excel.recruitStepConfig[selectedStep].DialogueGroupID;
        int count = Excel.recruitDialogueConfig.Values.Count(row => row.GroupID == group);
        if (dialogueIndex + 1 >= count)
            return false;

        dialogueIndex++;

        return true;
    }

    /// <summary>Releases transient page selection without cancelling a paid horn operation.</summary>
    public void Cancel()
    {
        selectedStep = 0;
        dialogueIndex = 0;
    }

    /// <summary>Commits the final conversation intention on an isolated village candidate.</summary>
    /// <param name="next">Detached transaction candidate.</param>
    /// <param name="step">Selected operation.</param>
    /// <param name="tasks">Current task receipts.</param>
    /// <returns>Whether ownership or conversation state changed.</returns>
    public bool Execute(HomeVillageSave next, RecruitStep step, TaskCollectionEntity tasks)
    {
        if (!tasks.IsClaimed(step.RequiredTaskID) || tasks.GetTask(step.ID).status != TaskStatus.Active
            || next.recruit.completed.Contains(step.ID))
            return false;

        if ((RecruitStepKind)step.Kind == RecruitStepKind.Horn)
        {
            if (next.recruit.pendingHorn == step.ID)
                return false;

            next.items.TryGetValue(step.CostItemID, out long count);
            if (count < step.CostCount)
                return false;

            next.items[step.CostItemID] = count - step.CostCount;
            next.recruit.pendingHorn = step.ID;

            return true;
        }

        if ((RecruitStepKind)step.Kind == RecruitStepKind.Receive)
            next.recruit.heroes.Add(step.HeroID);

        next.recruit.completed.Add(step.ID);

        return true;
    }

    /// <summary>Acknowledges the paid horn movie without consuming another whistle.</summary>
    /// <param name="next">Detached transaction candidate.</param>
    /// <returns>Whether the pending operation completed.</returns>
    public bool CompleteHorn(HomeVillageSave next)
    {
        if (next.recruit.pendingHorn == 0)
            return false;

        next.recruit.completed.Add(next.recruit.pendingHorn);
        next.recruit.pendingHorn = 0;

        return true;
    }
}
