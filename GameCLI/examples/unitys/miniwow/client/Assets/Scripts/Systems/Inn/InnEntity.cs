using System;
using Oathx.Entity;

/// <summary>Typed inn transactions applied to the shared atomic village save.</summary>
public enum InnAction
{
    /// <summary>Constructs the first inn level.</summary>
    Repair,
    /// <summary>Produces food from the current recipe.</summary>
    Craft,
    /// <summary>Starts one paid meal for the recruited hero.</summary>
    Rest,
    /// <summary>Completes an elapsed meal without charging again.</summary>
    Complete
}

/// <summary>Owns inn rules while the village aggregate owns durable inventory transactions.</summary>
[Initialize(InstallType.Startup, true)]
public sealed class InnEntity : XEntity
{
    /// <summary>Original inn building identity.</summary>
    public int buildingId
    { get { return 60017; } }

    /// <summary>Evaluates and applies one inn operation to a detached candidate.</summary>
    /// <param name="action">Requested business operation.</param>
    /// <param name="next">Detached save candidate.</param>
    /// <param name="village">Current building and inventory projections.</param>
    /// <param name="tasks">Authoritative task availability.</param>
    /// <returns>Whether the transaction changed the candidate.</returns>
    internal bool Execute(InnAction action, HomeVillageSave next, HomeVillageEntity village,
        TaskCollectionEntity tasks)
    {
        if (!village.buildings.IsAvailable(buildingId, tasks))
            return false;

        if (action == InnAction.Repair)
            return village.buildings.Construct(next, buildingId);

        if (village.buildings.Level(buildingId) == 0)
            return false;

        var service = village.buildings.GetService(buildingId);
        var setting = Config.Excel.homeInnSettingConfig[buildingId];
        switch (action)
        {
            case InnAction.Craft:
                if (village.Count(service.materialId) < setting.MaterialCount)
                    return false;

                next.items[service.materialId] -= setting.MaterialCount;
                next.items.TryGetValue(service.productId, out long stock);
                next.items[service.productId] = checked(stock + setting.OutputCount);
                next.innCrafts++;

                return true;
            case InnAction.Rest:
                if (next.heroId == 0 || next.innRestEndTicks != 0 || next.energyPercent >= 100
                    || next.recovery == HeroRecoveryPhase.Traveling || next.recovery == HeroRecoveryPhase.Healing
                    || village.Count(service.productId) == 0 || next.heroGold < service.price)
                    return false;

                next.items[service.productId]--;
                next.heroGold -= service.price;
                next.innRevenue = checked(next.innRevenue + service.price);
                next.innRestEndTicks = DateTime.UtcNow.AddSeconds(service.seconds).Ticks;

                return true;
            case InnAction.Complete:
                if (next.innRestEndTicks == 0 || next.innRestEndTicks > DateTime.UtcNow.Ticks)
                    return false;

                next.energyPercent = Math.Min(100, next.energyPercent + service.recoveryPercent);
                next.innRestEndTicks = 0;
                next.innMeals++;

                return true;
            default:
                throw new InvalidOperationException("Unsupported inn operation: " + action);
        }
    }
}
