using Oathx.Entity;

/// <summary>Owns first-stage selection independently of battle execution.</summary>
[Initialize(InstallType.Startup, true)]
public sealed class BattleEntryEntity : XEntity
{
    /// <summary>Stage selected for the future battle runtime, or zero before selection.</summary>
    public int selectedId
    { get; private set; }

    /// <summary>Checks the original feature prerequisite and actual roster.</summary>
    /// <param name="definition">Configured stage.</param>
    /// <param name="tasks">Actual reward receipts.</param>
    /// <param name="heroes">Actually recruited heroes.</param>
    /// <returns>Zero when available, otherwise a localized rejection.</returns>
    public int Check(Config.BattleEntry definition, TaskCollectionEntity tasks, HeroCollectionEntity heroes)
    {
        if (!tasks.IsClaimed(definition.RequiredTaskID))
            return Config.Excel.TEXT_BATTLE_ENTRY_LOCKED;

        if (heroes.count == 0)
            return Config.Excel.TEXT_BATTLE_ENTRY_REQUIRES_HERO;

        return 0;
    }

    /// <summary>Selects a reachable stage without reporting a battle victory.</summary>
    /// <param name="definition">Requested stage.</param>
    /// <param name="tasks">Current prerequisites.</param>
    /// <param name="heroes">Current roster.</param>
    /// <returns>Zero on successful selection, otherwise a localized rejection.</returns>
    public int Select(Config.BattleEntry definition, TaskCollectionEntity tasks, HeroCollectionEntity heroes)
    {
        int result = Check(definition, tasks, heroes);
        if (result != 0)
            return result;

        selectedId = definition.ID;

        return 0;
    }

    /// <summary>Releases session-local selection.</summary>
    public void Clear()
    {
        selectedId = 0;
    }
}
