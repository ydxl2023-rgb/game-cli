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
    /// <summary>Recruitment ownership and interaction receipts.</summary>
    public RecruitSave recruit
    { get; set; } = new RecruitSave();

    /// <summary>Exploration results committed with village receipts.</summary>
    public ExploreSave explore
    { get; set; } = new ExploreSave();

    /// <summary>Forge inventory and receipts committed with shared costs.</summary>
    public BlacksmithSave blacksmith
    { get; set; } = new BlacksmithSave();

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

    /// <summary>Committed purchase quantities by original shop offer ID.</summary>
    public Dictionary<int, long> shopPurchases
    { get; set; } = new Dictionary<int, long>();

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

    /// <summary>Whether the local inn energy field has been migrated.</summary>
    public bool innEnergyInitialized
    { get; set; }

    /// <summary>Cumulative paid meals, independent of current food stock.</summary>
    public long innMeals
    { get; set; }

    /// <summary>Local normalized hero energy for the inn service.</summary>
    public int energyPercent
    { get; set; }

    /// <summary>UTC meal deadline; zero means no meal is active.</summary>
    public long innRestEndTicks
    { get; set; }

    /// <summary>Cumulative committed food batches.</summary>
    public long innCrafts
    { get; set; }

    /// <summary>Inn service revenue retained independently of player gold.</summary>
    public long innRevenue
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
    { get { return heroes.primary; } }

    /// <summary>Owned persistent heroes, independent of world presentation.</summary>
    public HeroCollectionEntity heroes
    { get; private set; }

    /// <summary>Current recovery operation.</summary>
    public HeroRecoveryPhase recovery
    { get { return hero.recovery; } }

    /// <summary>Current durable excursion stage.</summary>
    public ExplorePhase exploration
    { get { return state.explore.phase; } }

    /// <summary>Current excursion configuration identity.</summary>
    public int journeyId
    { get { return state.explore.journeyId; } }

    /// <summary>Whether the obtained-hero page is awaiting confirmation.</summary>
    public bool awaitingRecruitmentConfirmation
    { get { return state.heroId != 0 && !state.recruitmentAcknowledged; } }

    /// <summary>Remaining real-time treatment duration.</summary>
    public float remainingSeconds
    { get { return hero.remainingSeconds; } }

    /// <summary>Whether the hero owns a paid meal that has not completed yet.</summary>
    public bool dining
    { get { return state.innRestEndTicks != 0; } }

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
        heroes = new HeroCollectionEntity(state);
        if (!state.innEnergyInitialized)
        {
            var next = Snapshot();
            next.energyPercent = Config.Excel.homeInnSettingConfig[60017].InitialEnergyPercent;
            next.innEnergyInitialized = true;
            Commit(next);
        }
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
                if (!buildings.Construct(next, configuration.BuildingID))
                    return false;
                break;
            case HomeVillageAction.Purchase:
                throw new InvalidOperationException("Purchases require the trading system's explicit confirmation.");
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

    /// <summary>Reads committed offer usage, including the original onboarding purchase.</summary>
    /// <param name="offer">Original configured offer.</param>
    /// <returns>Total purchased packs.</returns>
    public long Purchased(Config.ShopGoods offer)
    {
        if (state.shopPurchases.TryGetValue(offer.ID, out long count))
            return count;

        return offer.ItemID == configuration.MaterialID ? state.purchases : 0;
    }

    /// <summary>Commits price, delivered items and offer usage in the same existing save.</summary>
    /// <param name="offer">Validated configured offer.</param>
    /// <param name="quantity">Requested pack count.</param>
    /// <returns>The atomic transaction result.</returns>
    public ShopPurchaseResult Purchase(Config.ShopGoods offer, int quantity)
    {
        if (quantity <= 0)
            return ShopPurchaseResult.InvalidQuantity;

        long purchased = Purchased(offer);
        if (quantity > offer.Limit - purchased)
            return ShopPurchaseResult.SoldOut;

        long cost = checked((long)offer.Price * quantity);
        if (Count(offer.CurrencyID) < cost)
            return ShopPurchaseResult.NoGold;

        var next = Snapshot();
        next.items[offer.CurrencyID] -= cost;
        Add(next, offer.ItemID, checked((long)offer.Amount * quantity));
        next.shopPurchases[offer.ID] = checked(purchased + quantity);
        if (offer.ItemID == configuration.MaterialID)
            next.purchases = checked(next.purchases + quantity);

        Commit(next);

        return ShopPurchaseResult.Success;
    }

    /// <summary>Commits a recruitment intention with inventory and ownership.</summary>
    /// <param name="recruit">Recruitment rule owner.</param>
    /// <param name="step">Configured interaction.</param>
    /// <param name="tasks">Current task receipts.</param>
    /// <returns>Whether the interaction committed.</returns>
    public bool ExecuteRecruit(RecruitEntity recruit, Config.RecruitStep step, TaskCollectionEntity tasks)
    {
        var next = Snapshot();
        if (!recruit.Execute(next, step, tasks))
            return false;

        Commit(next);

        return true;
    }

    /// <summary>Reads a committed recruitment receipt without copying the entire village.</summary>
    /// <param name="stepId">Configured interaction identity.</param>
    /// <returns>Whether the interaction has committed.</returns>
    public bool HasRecruitmentReceipt(int stepId)
    {
        return state.recruit.completed.Contains(stepId);
    }

    /// <summary>Finishes the original recruitment movie with no second item deduction.</summary>
    /// <param name="recruit">Recruitment rule owner.</param>
    /// <returns>Whether the pending horn operation completed.</returns>
    public bool CompleteRecruitHorn(RecruitEntity recruit)
    {
        var next = Snapshot();
        if (!recruit.CompleteHorn(next))
            return false;

        Commit(next);

        return true;
    }

    /// <summary>Commits a configured exploration departure without changing task progress.</summary>
    /// <param name="explore">Excursion rule owner.</param>
    /// <param name="definition">Configured introductory route.</param>
    /// <returns>Whether the departure committed.</returns>
    public bool BeginExplore(ExploreEntity explore, Config.ExploreJourney definition)
    {
        var next = Snapshot();
        if (!explore.Begin(next, definition))
            return false;

        Commit(next);

        return true;
    }

    /// <summary>Commits actual scene arrival in the same save as exploration state.</summary>
    /// <param name="explore">Excursion rule owner.</param>
    /// <param name="definition">Current configured route.</param>
    /// <returns>Whether the arrival committed.</returns>
    public bool ArriveExplore(ExploreEntity explore, Config.ExploreJourney definition)
    {
        var next = Snapshot();
        if (!explore.Arrive(next, definition, DateTime.UtcNow.Ticks))
            return false;

        Commit(next);

        return true;
    }

    /// <summary>Commits elapsed exploration results once across reloads.</summary>
    /// <param name="explore">Excursion rule owner.</param>
    /// <param name="definition">Current configured route.</param>
    /// <returns>Whether the timed stage committed.</returns>
    public bool AdvanceExplore(ExploreEntity explore, Config.ExploreJourney definition)
    {
        var next = Snapshot();
        if (!explore.Advance(next, definition, DateTime.UtcNow.Ticks))
            return false;

        Commit(next);

        return true;
    }

    /// <summary>Commits an inn operation with inventory and hero costs in one durable save.</summary>
    /// <param name="inn">Inn rule owner.</param>
    /// <param name="action">Requested transaction.</param>
    /// <param name="tasks">Current task prerequisites.</param>
    /// <returns>Whether the transaction committed.</returns>
    public bool ExecuteInn(InnEntity inn, InnAction action, TaskCollectionEntity tasks)
    {
        var next = Snapshot();
        if (!inn.Execute(action, next, this, tasks))
            return false;

        Commit(next);

        return true;
    }

    /// <summary>Persists forge costs and ownership changes in the village transaction.</summary>
    /// <param name="forge">Forge rule owner.</param>
    /// <param name="action">Requested business operation.</param>
    /// <param name="tasks">Current task prerequisites.</param>
    /// <returns>Zero on success or a localized rejection.</returns>
    public int ExecuteBlacksmith(BlacksmithEntity forge, BlacksmithAction action, TaskCollectionEntity tasks)
    {
        var next = Snapshot();
        int result = forge.Execute(action, next, tasks);
        if (result != 0)
            return result;

        Commit(next);
        forge.Cancel();

        return 0;
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
        heroes.Destroy();

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
        heroes.Restore(state);
    }
}
