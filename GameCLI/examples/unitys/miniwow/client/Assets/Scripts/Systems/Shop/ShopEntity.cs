using System;
using System.Linq;
using Oathx.Entity;

/// <summary>Results of validating a confirmed local purchase.</summary>
public enum ShopPurchaseResult
{
    /// <summary>The complete transaction was committed.</summary>
    Success,
    /// <summary>The trading feature is not unlocked.</summary>
    Locked,
    /// <summary>No offer is awaiting confirmation.</summary>
    NoSelection,
    /// <summary>The requested quantity is outside configured bounds.</summary>
    InvalidQuantity,
    /// <summary>The configured offer has insufficient remaining stock.</summary>
    SoldOut,
    /// <summary>The shared wallet cannot cover the complete price.</summary>
    NoGold
}

/// <summary>Owns the trading catalog, pending selection and purchase validation.</summary>
[Initialize(InstallType.Startup, true)]
public sealed class ShopEntity : XEntity
{
    /// <summary>Offer awaiting explicit confirmation, or zero when none is selected.</summary>
    public int selectedId
    { get; private set; }

    /// <summary>Pack quantity for the pending offer.</summary>
    public int quantity
    { get; private set; }

    /// <summary>Checks the original task prerequisite for trading access.</summary>
    /// <param name="tasks">Authoritative task collection.</param>
    /// <returns>Whether trading is unlocked.</returns>
    public bool IsAvailable(TaskCollectionEntity tasks)
    {
        var setting = Config.Excel.shopSettingConfig[60019];

        return tasks.GetTask(setting.RequiredTaskID).status == TaskStatus.Claimed;
    }

    /// <summary>Returns stable original offer identities in catalog order.</summary>
    /// <returns>The currently supported first-tier offer IDs.</returns>
    public int[] GetOffers()
    {
        int tier = Config.Excel.shopSettingConfig[60019].Tier;

        return Config.Excel.shopGoodsConfig.Values.Where(row => row.Tier == tier)
            .OrderBy(row => row.Order).ThenBy(row => row.ID).Select(row => row.ID).ToArray();
    }

    /// <summary>Selects an offer without changing inventory.</summary>
    /// <param name="id">Original offer ID.</param>
    public void Select(int id)
    {
        if (!GetOffers().Contains(id))
            throw new InvalidOperationException("Offer is outside the active catalog: " + id);

        selectedId = id;
        quantity = 1;
    }

    /// <summary>Changes the pending quantity within stock and per-order bounds.</summary>
    /// <param name="delta">Requested step.</param>
    /// <param name="inventory">Shared committed inventory.</param>
    public void ChangeQuantity(int delta, HomeVillageEntity inventory)
    {
        var offer = Config.Excel.shopGoodsConfig[selectedId];
        long maximum = Math.Min(Config.Excel.shopSettingConfig[60019].MaxQuantity,
            offer.Limit - inventory.Purchased(offer));
        quantity = (int)Math.Max(1, Math.Min(maximum, (long)quantity + delta));
    }

    /// <summary>Discards the pending offer on cancel, success or system teardown.</summary>
    public void Cancel()
    {
        selectedId = 0;
        quantity = 0;
    }

    /// <summary>Rechecks access and commits the selected transaction exactly once.</summary>
    /// <param name="inventory">Shared committed economy.</param>
    /// <param name="tasks">Current feature prerequisites.</param>
    /// <returns>The purchase outcome.</returns>
    public ShopPurchaseResult Confirm(HomeVillageEntity inventory, TaskCollectionEntity tasks)
    {
        if (!IsAvailable(tasks))
            return ShopPurchaseResult.Locked;

        if (selectedId == 0)
            return ShopPurchaseResult.NoSelection;

        if (quantity <= 0 || quantity > Config.Excel.shopSettingConfig[60019].MaxQuantity)
            return ShopPurchaseResult.InvalidQuantity;

        var result = inventory.Purchase(Config.Excel.shopGoodsConfig[selectedId], quantity);
        if (result == ShopPurchaseResult.Success)
            Cancel();

        return result;
    }
}
