using System.Linq;
using Oathx.Widget;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;

/// <summary>Requests closing all forge pages.</summary>
public sealed class ReqBlacksmithClose : UIRequestEvent { }

/// <summary>Requests selection of a recipe or saved equipment.</summary>
public sealed class ReqBlacksmithSelect : UIRequestEvent
{
    /// <summary>Selected stable identity.</summary>
    public long identity
    { get; set; }

    /// <summary>Whether the selection belongs to the shelf.</summary>
    public bool shelf
    { get; set; }
}

/// <summary>Requests a forge business operation.</summary>
public sealed class ReqBlacksmithAction : UIRequestEvent
{
    /// <summary>Operation to validate and commit.</summary>
    public BlacksmithAction action
    { get; set; }
}

/// <summary>Requests changing the pending quantity.</summary>
public sealed class ReqBlacksmithQuantity : UIRequestEvent
{
    /// <summary>Requested step.</summary>
    public int delta
    { get; set; }
}

/// <summary>Cancels the confirmation without spending resources.</summary>
public sealed class ReqBlacksmithCancel : UIRequestEvent { }

/// <summary>Displays recipes, saved equipment and the recruited hero.</summary>
[Config("Assets/Data/Widgets/Blacksmith/UIBlacksmith.prefab", WidgetLayer.dialog, cacheWidget: false)]
public sealed class UIBlacksmith : UIWidget<VMBlacksmith>
{
    /// <summary>Selected presentation tab, independent of saved game data.</summary>
    private int tab;

    /// <summary>Page-owned post-commit forging presentation.</summary>
    private Coroutine production;

    /// <summary>Releases presentation independently of committed equipment.</summary>
    public override void Deactive()
    {
        if (production != null)
            StopCoroutine(production);

        production = null;

        base.Deactive();
    }

    /// <summary>Projects committed entity data into the selected page.</summary>
    /// <param name="forge">Recipe and transaction rules.</param>
    /// <param name="village">Committed inventory.</param>
    public void Invalidate(BlacksmithEntity forge, HomeVillageEntity village)
    {
        var saved = village.Snapshot();
        var setting = Config.Excel.blacksmithSettingConfig[60002];
        vm.Gold = string.Format(CommonText.Get(Config.Excel.TEXT_BLACKSMITH_GOLD),
            village.Count(setting.RepairCurrencyID));
        vm.RepairText = string.Format(CommonText.Get(Config.Excel.TEXT_BLACKSMITH_REPAIR), setting.RepairCost);
        vm.RepairVisible = village.buildings.Level(setting.ID) == 0;
        vm.CatalogVisible = !vm.RepairVisible && tab != 2;
        vm.HeroVisible = !vm.RepairVisible && tab == 2;
        vm.Revenue = string.Format(CommonText.Get(Config.Excel.TEXT_BLACKSMITH_REVENUE), saved.blacksmith.revenue);
        var entries = new VList<VMBlacksmithEntry>();
        if (tab == 0)
        {
            foreach (var recipe in forge.GetRecipes(village))
                entries.Add(CreateEntry(recipe, recipe.ID, false));
        }
        else if (tab == 1)
        {
            foreach (var item in saved.blacksmith.shelf)
                entries.Add(CreateEntry(Config.Excel.blacksmithRecipeConfig[item.recipeId], item.id, true));
        }

        if (vm.Entries == null)
            vm.Entries = entries;
        else if (!SameEntries(entries))
        {
            vm.Entries.Clear();
            foreach (VMBlacksmithEntry entry in entries)
                vm.Entries.Add(entry);
        }

        vm.HeroText = saved.heroId == 0 ? CommonText.Get(Config.Excel.TEXT_BLACKSMITH_NO_HERO)
            : CommonText.Get(Config.Excel.blacksmithHeroConfig[saved.heroId].NameTextID)
                + "\n" + string.Format(CommonText.Get(Config.Excel.TEXT_BLACKSMITH_HERO_GOLD), saved.heroGold);
        string weapon = saved.blacksmith.equipped.TryGetValue(saved.heroId, out var equipped)
            ? CommonText.Get(Config.Excel.blacksmithRecipeConfig[equipped.recipeId].NameTextID)
            : CommonText.Get(Config.Excel.TEXT_BLACKSMITH_EMPTY);
        vm.EquipmentText = string.Format(CommonText.Get(Config.Excel.TEXT_BLACKSMITH_EQUIPPED), weapon);
    }

    /// <summary>Displays a localized transaction result.</summary>
    /// <param name="textId">Configured feedback text.</param>
    public void ShowFeedback(int textId)
    {
        if (production != null)
            StopCoroutine(production);

        production = null;
        vm.Feedback = CommonText.Get(textId);
    }

    /// <summary>Plays the recovered one-second post-commit forge duration.</summary>
    public void PlayProduction()
    {
        if (production != null)
            StopCoroutine(production);

        production = StartCoroutine(AnimateProduction());
    }

    /// <summary>Binds page intentions.</summary>
    /// <returns>Whether binding succeeded.</returns>
    protected override bool Bind()
    {
        vm.CloseClick = Send<ReqBlacksmithClose>;
        vm.RepairClick = OnRepairClick;
        vm.RecipesClick = OnRecipesClick;
        vm.ShelfClick = OnShelfClick;
        vm.HeroClick = OnHeroClick;
        vm.CollectClick = OnCollectClick;

        return true;
    }

    /// <summary>Preserves existing rows when their stable identities have not changed.</summary>
    /// <param name="entries">Requested presentation rows.</param>
    /// <returns>Whether current row identities match.</returns>
    private bool SameEntries(VList<VMBlacksmithEntry> entries)
    {
        if (vm.Entries.Count != entries.Count)
            return false;

        for (int i = 0; i < entries.Count; i++)
        {
            if (vm.Entries[i].identity != entries[i].identity || vm.Entries[i].shelf != entries[i].shelf)
                return false;
        }

        return true;
    }

    /// <summary>Shows the original forging duration without delaying the durable transaction.</summary>
    /// <returns>The page-owned presentation.</returns>
    private IEnumerator AnimateProduction()
    {
        vm.Feedback = CommonText.Get(Config.Excel.TEXT_BLACKSMITH_FORGING);

        yield return new WaitForSecondsRealtime(Config.Excel.blacksmithSettingConfig[60002].ForgeSeconds);

        vm.Feedback = CommonText.Get(Config.Excel.TEXT_BLACKSMITH_SUCCESS);
        production = null;
    }

    /// <summary>Builds one display row.</summary>
    /// <param name="recipe">Original recipe.</param>
    /// <param name="id">Row identity.</param>
    /// <param name="shelf">Whether the row is a saved weapon.</param>
    /// <returns>Display-only row.</returns>
    private VMBlacksmithEntry CreateEntry(Config.BlacksmithRecipe recipe, long id, bool shelf)
    {
        var entry = new VMBlacksmithEntry {
            identity = id,
            shelf = shelf,
            Title = CommonText.Get(recipe.NameTextID),
            Icon = recipe.IconPath,
            Detail = string.Format(CommonText.Get(Config.Excel.TEXT_BLACKSMITH_PRICE), recipe.Price),
            ActionText = CommonText.Get(shelf ? Config.Excel.TEXT_BLACKSMITH_SELL : Config.Excel.TEXT_BLACKSMITH_FORGE)
        };

        return entry;
    }

    /// <summary>Refreshes the chosen display tab from authoritative entities.</summary>
    /// <param name="value">Presentation tab index.</param>
    /// <returns>Whether the display was refreshed.</returns>
    private bool SelectTab(int value)
    {
        tab = value;
        var forge = Oathx.Entity.XEntityManager.GetSingleton().QueryEntity<BlacksmithEntity>();
        var home = Oathx.Entity.XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        Invalidate(forge, home.village);

        return true;
    }

    /// <summary>Requests construction.</summary>
    /// <param name="button">Clicked control.</param>
    /// <returns>Whether sent.</returns>
    private bool OnRepairClick(Button button)
    {
        return Send(new ReqBlacksmithAction { action = BlacksmithAction.Repair });
    }

    /// <summary>Shows recipe rows.</summary>
    /// <param name="button">Clicked control.</param>
    /// <returns>Whether selected.</returns>
    private bool OnRecipesClick(Button button)
    {
        return SelectTab(0);
    }

    /// <summary>Shows saved equipment.</summary>
    /// <param name="button">Clicked control.</param>
    /// <returns>Whether selected.</returns>
    private bool OnShelfClick(Button button)
    {
        return SelectTab(1);
    }

    /// <summary>Shows the recruited hero.</summary>
    /// <param name="button">Clicked control.</param>
    /// <returns>Whether selected.</returns>
    private bool OnHeroClick(Button button)
    {
        return SelectTab(2);
    }

    /// <summary>Requests collection of earned income.</summary>
    /// <param name="button">Clicked control.</param>
    /// <returns>Whether sent.</returns>
    private bool OnCollectClick(Button button)
    {
        return Send(new ReqBlacksmithAction { action = BlacksmithAction.Collect });
    }
}
