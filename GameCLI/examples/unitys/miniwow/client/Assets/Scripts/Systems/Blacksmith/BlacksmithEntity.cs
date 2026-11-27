using System;
using System.Collections.Generic;
using System.Linq;
using Oathx.Entity;

/// <summary>Transactions owned by the forge.</summary>
public enum BlacksmithAction
{
    /// <summary>Constructs the first forge.</summary>
    Repair,
    /// <summary>Consumes the selected recipe materials.</summary>
    Forge,
    /// <summary>Transfers the selected equipment to the recruited hero.</summary>
    Sell,
    /// <summary>Transfers earned income into the shared wallet.</summary>
    Collect
}

/// <summary>Equipment identity, independent of its UI representation.</summary>
public sealed class BlacksmithEquipment
{
    /// <summary>Unique saved instance identity.</summary>
    public long id
    { get; set; }

    /// <summary>Original equipment definition.</summary>
    public int recipeId
    { get; set; }
}

/// <summary>Forge data persisted atomically with village inventory.</summary>
public sealed class BlacksmithSave
{
    /// <summary>Next unique equipment identity.</summary>
    public long nextId
    { get; set; } = 1;

    /// <summary>Equipment available for trading.</summary>
    public List<BlacksmithEquipment> shelf
    { get; set; } = new List<BlacksmithEquipment>();

    /// <summary>Equipped weapons keyed by hero identity.</summary>
    public Dictionary<int, BlacksmithEquipment> equipped
    { get; set; } = new Dictionary<int, BlacksmithEquipment>();

    /// <summary>Cumulative production by recipe.</summary>
    public Dictionary<int, long> forged
    { get; set; } = new Dictionary<int, long>();

    /// <summary>Cumulative sales by recipe.</summary>
    public Dictionary<int, long> sold
    { get; set; } = new Dictionary<int, long>();

    /// <summary>Income awaiting collection.</summary>
    public long revenue
    { get; set; }
}

/// <summary>Owns forge selections and validates complete economy transactions.</summary>
[Initialize(InstallType.Startup, true)]
public sealed class BlacksmithEntity : XEntity
{
    /// <summary>Selected recipe, or zero before selection.</summary>
    public int selectedRecipe
    { get; private set; }

    /// <summary>Selected shelf instance, or zero before selection.</summary>
    public long selectedEquipment
    { get; private set; }

    /// <summary>Quantity awaiting explicit confirmation.</summary>
    public int quantity
    { get; private set; }

    /// <summary>Checks the recovered chapter reward prerequisite.</summary>
    /// <param name="tasks">Authoritative task collection.</param>
    /// <returns>Whether the forge is accessible.</returns>
    public bool IsAvailable(TaskCollectionEntity tasks)
    {
        var setting = Config.Excel.blacksmithSettingConfig[60002];

        return tasks.IsClaimed(setting.RequiredTaskID);
    }

    /// <summary>Returns recipes available at the constructed level.</summary>
    /// <param name="village">Committed village state.</param>
    /// <returns>Ordered configuration rows.</returns>
    public Config.BlacksmithRecipe[] GetRecipes(HomeVillageEntity village)
    {
        int level = Math.Max(1, village.buildings.Level(60002));

        return Config.Excel.blacksmithRecipeConfig.Values.Where(row => row.Level <= level)
            .OrderBy(row => row.ID).ToArray();
    }

    /// <summary>Selects a recipe without spending resources.</summary>
    /// <param name="id">Configured recipe identity.</param>
    public void SelectRecipe(int id)
    {
        var recipe = Config.Excel.blacksmithRecipeConfig[id];
        selectedRecipe = recipe.ID;
        quantity = 1;
    }

    /// <summary>Selects a shelf instance without transferring ownership.</summary>
    /// <param name="id">Persistent equipment identity.</param>
    public void SelectEquipment(long id)
    {
        selectedEquipment = id;
    }

    /// <summary>Changes quantity within the recovered per-order limit.</summary>
    /// <param name="delta">Quantity step.</param>
    public void ChangeQuantity(int delta)
    {
        var setting = Config.Excel.blacksmithSettingConfig[60002];
        quantity = Math.Max(1, Math.Min(setting.MaxQuantity, quantity + delta));
    }

    /// <summary>Discards page-owned selections.</summary>
    public void Cancel()
    {
        selectedRecipe = 0;
        selectedEquipment = 0;
        quantity = 0;
    }

    /// <summary>Validates and mutates only an isolated save candidate.</summary>
    /// <param name="action">Requested operation.</param>
    /// <param name="next">Detached save candidate.</param>
    /// <param name="tasks">Feature prerequisites.</param>
    /// <returns>Zero on success, otherwise a localized feedback ID.</returns>
    internal int Execute(BlacksmithAction action, HomeVillageSave next, TaskCollectionEntity tasks)
    {
        if (!IsAvailable(tasks))
            return Config.Excel.TEXT_BLACKSMITH_LOCKED;

        var setting = Config.Excel.blacksmithSettingConfig[60002];
        if (action == BlacksmithAction.Repair)
        {
            if (next.buildings.ContainsKey(setting.ID))
                return Config.Excel.TEXT_BLACKSMITH_REPAIRED;

            if (!next.items.TryGetValue(setting.RepairCurrencyID, out long gold) || gold < setting.RepairCost)
                return Config.Excel.TEXT_BLACKSMITH_NO_REPAIR_GOLD;

            next.items[setting.RepairCurrencyID] -= setting.RepairCost;
            next.buildings.Add(setting.ID, 1);

            return 0;
        }

        if (!next.buildings.ContainsKey(setting.ID))
            return Config.Excel.TEXT_BLACKSMITH_REPAIR_FIRST;

        switch (action)
        {
            case BlacksmithAction.Forge:
                return Forge(next);
            case BlacksmithAction.Sell:
                return Sell(next);
            case BlacksmithAction.Collect:
                next.items.TryGetValue(setting.RepairCurrencyID, out long current);
                next.items[setting.RepairCurrencyID] = checked(current + next.blacksmith.revenue);
                next.blacksmith.revenue = 0;

                return 0;
            default:
                throw new InvalidOperationException("Unsupported forge operation: " + action);
        }
    }

    /// <summary>Commits materials and unique output instances together.</summary>
    /// <param name="next">Detached save candidate.</param>
    /// <returns>Localized rejection or zero.</returns>
    private int Forge(HomeVillageSave next)
    {
        if (selectedRecipe == 0)
            return Config.Excel.TEXT_BLACKSMITH_NO_SELECTION;

        var recipe = Config.Excel.blacksmithRecipeConfig[selectedRecipe];
        if (recipe.Level > next.buildings[60002])
            return Config.Excel.TEXT_BLACKSMITH_REPAIR_FIRST;

        var costs = Config.Excel.blacksmithCostConfig.Values.Where(row => row.RecipeID == selectedRecipe).ToArray();
        foreach (var cost in costs)
        {
            if (!next.items.TryGetValue(cost.ItemID, out long owned) || owned < (long)cost.Amount * quantity)
                return Config.Excel.TEXT_BLACKSMITH_NO_MATERIAL;
        }

        foreach (var cost in costs)
            next.items[cost.ItemID] -= (long)cost.Amount * quantity;

        for (int i = 0; i < quantity; i++)
        {
            var equipment = new BlacksmithEquipment {
                id = next.blacksmith.nextId++,
                recipeId = selectedRecipe
            };

            next.blacksmith.shelf.Add(equipment);
        }

        next.blacksmith.forged.TryGetValue(selectedRecipe, out long count);
        next.blacksmith.forged[selectedRecipe] = checked(count + quantity);

        return 0;
    }

    /// <summary>Exchanges one compatible weapon and hero currency atomically.</summary>
    /// <param name="next">Detached save candidate.</param>
    /// <returns>Localized rejection or zero.</returns>
    private int Sell(HomeVillageSave next)
    {
        var equipment = next.blacksmith.shelf.FirstOrDefault(item => item.id == selectedEquipment);
        if (equipment == null)
            return Config.Excel.TEXT_BLACKSMITH_NO_STOCK;

        if (next.heroId == 0)
            return Config.Excel.TEXT_BLACKSMITH_NO_HERO;

        var recipe = Config.Excel.blacksmithRecipeConfig[equipment.recipeId];
        var hero = Config.Excel.blacksmithHeroConfig[next.heroId];
        if (recipe.Trade != hero.Trade)
            return Config.Excel.TEXT_BLACKSMITH_INCOMPATIBLE;

        if (next.heroGold < recipe.Price)
            return Config.Excel.TEXT_BLACKSMITH_NO_GOLD;

        next.heroGold -= recipe.Price;
        next.blacksmith.revenue = checked(next.blacksmith.revenue + recipe.Price);
        next.blacksmith.shelf.Remove(equipment);
        if (next.blacksmith.equipped.TryGetValue(next.heroId, out var previous))
            next.blacksmith.shelf.Add(previous);

        next.blacksmith.equipped[next.heroId] = equipment;
        next.blacksmith.sold.TryGetValue(recipe.ID, out long count);
        next.blacksmith.sold[recipe.ID] = checked(count + 1);

        return 0;
    }
}
