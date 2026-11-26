using System.Collections.Generic;
using System.Linq;
using System;
using Oathx.Entity;
using UnityEngine;

/// <summary>Supported ownership domains of building conditions.</summary>
public enum HomeBuildingConditionOwner
{
    /// <summary>Feature availability.</summary>
    Feature = 1,
    /// <summary>Target building level.</summary>
    Level = 2
}

/// <summary>Original building condition categories used by the first hospital.</summary>
public enum HomeBuildingConditionType
{
    /// <summary>A chapter task reward has been accepted.</summary>
    ChapterTask = 6,
    /// <summary>The guild has reached the specified level.</summary>
    GuildLevel = 7
}

/// <summary>Resolved hospital service values derived from the current building level.</summary>
public sealed class HomeHospitalService
{
    /// <summary>Building that provides this service.</summary>
    public int buildingId
    { get; set; }

    /// <summary>Material consumed by the offline recipe.</summary>
    public int materialId
    { get; set; }

    /// <summary>Medicine produced and consumed.</summary>
    public int productId
    { get; set; }

    /// <summary>Price paid by the hero.</summary>
    public int price
    { get; set; }

    /// <summary>Treatment duration after the building reduction.</summary>
    public float seconds
    { get; set; }

    /// <summary>Recovered health percentage.</summary>
    public int recoveryPercent
    { get; set; }
}

/// <summary>Owns village building levels independently of their scene models.</summary>
public sealed class BuildingEntity : XEntity
{
    /// <summary>Levels keyed by stable building instance ID.</summary>
    private readonly Dictionary<int, int> levels;

    /// <summary>Restores a private copy of the committed building collection.</summary>
    /// <param name="saved">Persisted building levels.</param>
    public BuildingEntity(Dictionary<int, int> saved)
    {
        levels = new Dictionary<int, int>(saved);
    }

    /// <summary>Reads the level of a building, including an unrepaired building.</summary>
    /// <param name="buildingId">Stable building instance ID.</param>
    /// <returns>Zero until construction has committed.</returns>
    public int Level(int buildingId)
    {
        return levels.TryGetValue(buildingId, out int level) ? level : 0;
    }

    /// <summary>Reads first-construction costs from the target level.</summary>
    /// <param name="buildingId">Configured building identity.</param>
    /// <returns>Detached ordered cost rows.</returns>
    public Config.HomeBuildingCost[] GetRepairCosts(int buildingId)
    {
        int levelId = buildingId * 1000 + 1;

        return Config.Excel.homeBuildingCostConfig.Values.Where(row => row.LevelID == levelId)
            .OrderBy(row => row.ID).ToArray();
    }

    /// <summary>Evaluates the implemented feature conditions against authoritative task receipts.</summary>
    /// <param name="buildingId">Configured building identity.</param>
    /// <param name="tasks">Current task collection.</param>
    /// <returns>Whether the building interaction is available.</returns>
    public bool IsAvailable(int buildingId, TaskCollectionEntity tasks)
    {
        var building = Config.Excel.homeBuildingConfig[buildingId];
        foreach (var condition in Config.Excel.homeBuildingConditionConfig.Values)
        {
            if ((HomeBuildingConditionOwner)condition.OwnerKind != HomeBuildingConditionOwner.Feature
                || condition.OwnerID != building.FeatureID)
                continue;

            if ((HomeBuildingConditionType)condition.Type != HomeBuildingConditionType.ChapterTask)
                throw new NotSupportedException("Unsupported hospital feature condition: " + condition.Type);

            if (tasks.GetTask(condition.Value).status != TaskStatus.Claimed)
                return false;
        }

        return true;
    }

    /// <summary>Resolves the first hospital service without duplicating prices in local settings.</summary>
    /// <param name="buildingId">Configured hospital identity.</param>
    /// <returns>The current first service, or the initial service before construction.</returns>
    public HomeHospitalService GetService(int buildingId)
    {
        int levelId = buildingId * 1000 + Math.Max(1, Level(buildingId));
        var level = Config.Excel.homeBuildingLevelConfig[levelId];
        var good = Config.Excel.homeBuildingGoodsConfig.Values
            .Where(row => row.LevelID == levelId).OrderBy(row => row.Order).First();
        if (good.RecoveryPercent < 0)
            throw new InvalidOperationException("Hospital recovery configuration is unresolved: " + good.ID);

        var item = Config.Excel.homeBuildingItemConfig[good.ProductID];

        var service = new HomeHospitalService {
            buildingId = buildingId,
            materialId = good.MaterialID,
            productId = good.ProductID,
            price = Mathf.RoundToInt(good.ServiceBasePrice * (1f + level.PricePercent / 100f)),
            seconds = item.UseSeconds - level.CutSeconds,
            recoveryPercent = good.RecoveryPercent
        };

        return service;
    }

    /// <summary>Validates construction and writes its cost and result into an isolated transaction.</summary>
    /// <param name="next">Transaction candidate committed by the village owner.</param>
    /// <param name="configuration">Configured hospital identity.</param>
    /// <returns>Whether the construction transaction is valid.</returns>
    internal bool Construct(HomeVillageSave next, Config.VillageSetting configuration)
    {
        if (Level(configuration.BuildingID) > 0)
            return false;

        var costs = GetRepairCosts(configuration.BuildingID);
        foreach (var cost in costs)
        {
            if (!next.items.TryGetValue(cost.ItemID, out long count) || count < cost.Amount)
                return false;
        }

        foreach (var cost in costs)
            next.items[cost.ItemID] -= cost.Amount;

        next.buildings.Add(configuration.BuildingID, 1);

        return true;
    }

    /// <summary>Applies committed levels while retaining this collection's entity identity.</summary>
    /// <param name="saved">The committed building state.</param>
    internal void Restore(Dictionary<int, int> saved)
    {
        levels.Clear();
        foreach (var pair in saved)
            levels.Add(pair.Key, pair.Value);
    }
}
