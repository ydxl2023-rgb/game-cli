using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oathx.Entity;
using UnityEngine;

/// <summary>Configured village operations, distinct from task completion.</summary>
public enum HomeVillageAction
{
    /// <summary>Receives the rescued hero.</summary>
    Recruit = 1,
    /// <summary>Constructs the hospital.</summary>
    Repair,
    /// <summary>Purchases one herb.</summary>
    Purchase,
    /// <summary>Produces one configured batch.</summary>
    Craft,
    /// <summary>Requests hospital attendance.</summary>
    Heal
}

/// <summary>Durable recovery phase; scene movement is recreated from this intent.</summary>
public enum HeroRecoveryPhase
{
    /// <summary>No hospital visit has been requested.</summary>
    Idle,
    /// <summary>The hero is walking to the hospital.</summary>
    Traveling,
    /// <summary>The potion has been consumed and treatment is in progress.</summary>
    Healing,
    /// <summary>Treatment has completed.</summary>
    Complete
}

/// <summary>Atomic local save for village costs, results and reward receipts.</summary>
public sealed class HomeVillageSave
{
    /// <summary>Inventory quantities keyed by item ID.</summary>
    public Dictionary<int, long> items
    { get; set; } = new Dictionary<int, long>();

    /// <summary>Constructed building levels.</summary>
    public Dictionary<int, int> buildings
    { get; set; } = new Dictionary<int, int>();

    /// <summary>Reward transactions already committed.</summary>
    public HashSet<int> receipts
    { get; set; } = new HashSet<int>();

    /// <summary>Owned hero configuration ID.</summary>
    public int heroId
    { get; set; }

    /// <summary>Normalized health for the offline recovery demonstration.</summary>
    public int healthPercent
    { get; set; }

    /// <summary>Personal hero currency.</summary>
    public long heroGold
    { get; set; }

    /// <summary>Whether the obtained-hero presentation has been acknowledged.</summary>
    public bool recruitmentAcknowledged
    { get; set; }

    /// <summary>Cumulative herbs purchased, independent of current stock.</summary>
    public long purchases
    { get; set; }

    /// <summary>Cumulative production batches.</summary>
    public long crafts
    { get; set; }

    /// <summary>Current recovery operation.</summary>
    public HeroRecoveryPhase recovery
    { get; set; }

    /// <summary>UTC treatment deadline, preserved across scene and application restarts.</summary>
    public long healingEndTicks
    { get; set; }

    /// <summary>Hospital revenue awaiting collection, distinct from player currency.</summary>
    public long hospitalRevenue
    { get; set; }

    /// <summary>Accumulated task quest value.</summary>
    public long questValue
    { get; set; }
}

/// <summary>Owns village transactions and persists their results before reporting task observations.</summary>
public sealed class HomeVillageEntity : XEntity, ITaskRewardReceiver
{
    /// <summary>Current committed data, never exposed to callers.</summary>
    private HomeVillageSave state;

    /// <summary>Local persistence key using the existing product storage convention.</summary>
    private readonly string storageKey = StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village);

    /// <summary>Exported prices, recipe and recovery values.</summary>
    public Config.VillageSetting configuration
    { get; private set; }

    /// <summary>Read-only building collection from the committed transaction.</summary>
    public BuildingEntity buildings
    { get; private set; }

    /// <summary>Persistent hero data, separate from its actor and shape.</summary>
    public HeroEntity hero
    { get; private set; }

    /// <summary>Current recovery operation.</summary>
    public HeroRecoveryPhase recovery
    { get { return hero.recovery; } }

    /// <summary>Whether the obtained-hero page is awaiting confirmation.</summary>
    public bool awaitingRecruitmentConfirmation
    { get { return state.heroId != 0 && !state.recruitmentAcknowledged; } }

    /// <summary>Remaining real-time treatment duration.</summary>
    public float remainingSeconds
    { get { return hero.remainingSeconds; } }

    /// <summary>Current configured hospital medicine and service values.</summary>
    public HomeHospitalService service
    { get { return buildings.GetService(configuration.BuildingID); } }

    /// <summary>Restores local transactions or starts with the existing Home-configured balance.</summary>
    /// <param name="initialGold">Existing product Home configuration balance.</param>
    public HomeVillageEntity(long initialGold)
    {
        configuration = Config.Excel.villageSettingConfig[1];
        state = PlayerPrefs.HasKey(storageKey)
            ? JsonConvert.DeserializeObject<HomeVillageSave>(PlayerPrefs.GetString(storageKey))
            : new HomeVillageSave();
        if (!PlayerPrefs.HasKey(storageKey))
            state.items.Add(configuration.GoldID, initialGold);

        buildings = new BuildingEntity(state.buildings);
        hero = new HeroEntity(state);
    }

    /// <summary>Returns a detached diagnostic or persistence snapshot.</summary>
    /// <returns>A copy that cannot mutate live state.</returns>
    public HomeVillageSave Snapshot()
    {
        return JsonConvert.DeserializeObject<HomeVillageSave>(JsonConvert.SerializeObject(state));
    }

    /// <summary>Reads current inventory independently of purchase and production counters.</summary>
    /// <param name="itemId">Configured item ID.</param>
    /// <returns>Current quantity, or zero for an unowned item.</returns>
    public long Count(int itemId)
    {
        return state.items.TryGetValue(itemId, out long count) ? count : 0;
    }

    /// <summary>Executes one unlocked business operation with its actual costs and outputs.</summary>
    /// <param name="taskId">Configured onboarding operation requested by the page.</param>
    /// <param name="tasks">Authoritative task collection used only for prerequisite checks.</param>
    /// <returns>Whether a transaction was committed.</returns>
    public bool Execute(int taskId, TaskCollectionEntity tasks)
    {
        if (!Config.Excel.villageTaskConfig.TryGetValue(taskId, out var step)
            || tasks.GetTask(step.RequiredTaskID).status != TaskStatus.Claimed)
            return false;

        return Execute((HomeVillageAction)step.Action, tasks);
    }

    /// <summary>Executes an available village operation independently of the tracked task.</summary>
    /// <param name="action">Typed business intention.</param>
    /// <param name="tasks">Current feature prerequisites.</param>
    /// <returns>Whether a transaction committed.</returns>
    public bool Execute(HomeVillageAction action, TaskCollectionEntity tasks)
    {
        if ((action == HomeVillageAction.Repair || action == HomeVillageAction.Craft)
            && !buildings.IsAvailable(configuration.BuildingID, tasks))
            return false;

        var next = Snapshot();
        switch (action)
        {
            case HomeVillageAction.Recruit:
                if (!hero.Recruit(next, configuration))
                    return false;
                break;
            case HomeVillageAction.Repair:
                if (!buildings.Construct(next, configuration))
                    return false;
                break;
            case HomeVillageAction.Purchase:
                if (Count(configuration.GoldID) < configuration.PurchasePrice)
                    return false;

                next.items[configuration.GoldID] -= configuration.PurchasePrice;
                Add(next, configuration.MaterialID, 1);
                next.purchases++;
                break;
            case HomeVillageAction.Craft:
                if (buildings.Level(configuration.BuildingID) == 0
                    || Count(service.materialId) < configuration.MaterialCost)
                    return false;

                next.items[service.materialId] -= configuration.MaterialCost;
                Add(next, service.productId, configuration.OutputCount);
                next.crafts++;
                break;
            case HomeVillageAction.Heal:
                if (!hero.RequestTreatment(next, service))
                    return false;
                break;
            default:
                throw new InvalidOperationException("Unsupported village action: " + action);
        }

        Commit(next);

        return true;
    }

    /// <summary>Persists acknowledgment independently of the task reward receipt.</summary>
    public void ConfirmRecruitment()
    {
        var next = Snapshot();
        next.recruitmentAcknowledged = true;
        Commit(next);
    }

    /// <summary>Consumes medicine only after the logical actor actually reaches the hospital.</summary>
    /// <returns>Whether treatment started.</returns>
    public bool BeginHealing()
    {
        var next = Snapshot();
        if (!hero.BeginTreatment(next, service))
            return false;

        Commit(next);

        return true;
    }

    /// <summary>Completes elapsed treatment without consuming a second potion after reconnecting.</summary>
    /// <returns>Whether recovery completed now.</returns>
    public bool CompleteHealing()
    {
        var next = Snapshot();
        if (!hero.CompleteTreatment(next, service))
            return false;

        Commit(next);

        return true;
    }

    /// <summary>Reads actual committed business progress for the configured task adapter.</summary>
    /// <param name="action">Business observation kind.</param>
    /// <returns>Absolute completion evidence.</returns>
    public long Progress(HomeVillageAction action)
    {
        switch (action)
        {
            case HomeVillageAction.Recruit: return state.heroId == configuration.HeroID ? 1 : 0;
            case HomeVillageAction.Repair: return buildings.Level(configuration.BuildingID);
            case HomeVillageAction.Purchase: return state.purchases;
            case HomeVillageAction.Craft: return state.crafts;
            case HomeVillageAction.Heal: return state.recovery == HeroRecoveryPhase.Complete ? 1 : 0;
            default: throw new InvalidOperationException("Unsupported village observation: " + action);
        }
    }

    /// <summary>Commits an entire reward batch and its receipt in the same local save.</summary>
    /// <param name="definition">Task rewards and idempotency key.</param>
    /// <returns>True after this batch or its prior receipt is durable.</returns>
    public bool Grant(Config.Task definition)
    {
        if (state.receipts.Contains(definition.ID))
            return true;

        var next = Snapshot();
        foreach (var reward in definition.Rewards)
            Add(next, reward.Id, reward.Num);

        next.questValue = checked(next.questValue + definition.QuestValue);
        next.receipts.Add(definition.ID);
        Commit(next);

        return true;
    }

    /// <summary>Releases the owned entity projections.</summary>
    public override void Destroy()
    {
        buildings.Destroy();
        hero.Destroy();

        base.Destroy();
    }

    /// <summary>Adds a nonnegative quantity to an isolated transaction candidate.</summary>
    /// <param name="next">Detached transaction candidate.</param>
    /// <param name="itemId">Item identity.</param>
    /// <param name="amount">Quantity to add.</param>
    private static void Add(HomeVillageSave next, int itemId, long amount)
    {
        if (amount < 0)
            throw new InvalidOperationException("Village rewards cannot remove items.");

        next.items.TryGetValue(itemId, out long current);
        next.items[itemId] = checked(current + amount);
    }

    /// <summary>Persists costs and outputs together before publishing the new state.</summary>
    /// <param name="next">Fully calculated transaction candidate.</param>
    private void Commit(HomeVillageSave next)
    {
        PlayerPrefs.SetString(storageKey, JsonConvert.SerializeObject(next));
        PlayerPrefs.Save();
        state = next;
        buildings.Restore(state.buildings);
        hero.Restore(state);
    }
}
