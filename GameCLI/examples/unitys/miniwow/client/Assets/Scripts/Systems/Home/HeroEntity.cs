using Oathx.Entity;
using System;

/// <summary>Persistent hero identity and local recovery data, separate from scene actors.</summary>
public sealed class HeroEntity : XEntity
{
    /// <summary>Hero configuration ID; zero means no hero has been recruited.</summary>
    public int configurationId
    { get; private set; }

    /// <summary>Normalized offline health, not an inferred original combat attribute.</summary>
    public int healthPercent
    { get; private set; }

    /// <summary>Personal currency used to pay for treatment.</summary>
    public long gold
    { get; private set; }

    /// <summary>Persistent hospital operation.</summary>
    public HeroRecoveryPhase recovery
    { get; private set; }

    /// <summary>UTC deadline retained across runtime recreation.</summary>
    private long healingEndTicks;

    /// <summary>Remaining configured recovery time.</summary>
    public float remainingSeconds
    {
        get {
            return (float)Math.Max(0,
                (healingEndTicks - DateTime.UtcNow.Ticks) / (double)TimeSpan.TicksPerSecond);
        }
    }

    /// <summary>Restores detached committed recovery data.</summary>
    /// <param name="saved">Committed hero recovery data.</param>
    public HeroEntity(HomeVillageSave saved)
    {
        Restore(saved);
    }

    /// <summary>Receives the configured hero only once.</summary>
    /// <param name="next">Detached transaction candidate.</param>
    /// <param name="configuration">Configured local starting attributes.</param>
    /// <returns>Whether ownership was granted in the candidate.</returns>
    internal bool Recruit(HomeVillageSave next, Config.VillageSetting configuration)
    {
        if (configurationId != 0)
            return false;

        next.heroId = configuration.HeroID;
        next.healthPercent = configuration.InitialHealthPercent;
        next.heroGold = configuration.HeroGold;

        return true;
    }

    /// <summary>Accepts one hospital visit when its required service is available.</summary>
    /// <param name="next">Detached transaction candidate.</param>
    /// <param name="configuration">Medicine and service configuration.</param>
    /// <returns>Whether the hero can begin traveling.</returns>
    internal bool RequestTreatment(HomeVillageSave next, HomeHospitalService configuration)
    {
        if (recovery != HeroRecoveryPhase.Idle || configurationId == 0 || next.innRestEndTicks != 0
            || !next.buildings.ContainsKey(configuration.buildingId)
            || !next.items.TryGetValue(configuration.productId, out long potions) || potions == 0
            || gold < configuration.price)
            return false;

        next.recovery = HeroRecoveryPhase.Traveling;

        return true;
    }

    /// <summary>Consumes stock and pays the hospital after logical arrival.</summary>
    /// <param name="next">Detached transaction candidate.</param>
    /// <param name="configuration">Medicine, duration and price configuration.</param>
    /// <returns>Whether treatment starts in this candidate.</returns>
    internal bool BeginTreatment(HomeVillageSave next, HomeHospitalService configuration)
    {
        if (recovery != HeroRecoveryPhase.Traveling || next.innRestEndTicks != 0
            || !next.items.TryGetValue(configuration.productId, out long potions) || potions == 0
            || gold < configuration.price)
            return false;

        next.items[configuration.productId]--;
        next.heroGold -= configuration.price;
        next.hospitalRevenue += configuration.price;
        next.healingEndTicks = DateTime.UtcNow.AddSeconds(configuration.seconds).Ticks;
        next.recovery = HeroRecoveryPhase.Healing;

        return true;
    }

    /// <summary>Applies elapsed recovery without charging again.</summary>
    /// <param name="next">Detached transaction candidate.</param>
    /// <param name="configuration">Configured recovery effect.</param>
    /// <returns>Whether treatment completes in this candidate.</returns>
    internal bool CompleteTreatment(HomeVillageSave next, HomeHospitalService configuration)
    {
        if (recovery != HeroRecoveryPhase.Healing || remainingSeconds > 0)
            return false;

        next.healthPercent = Math.Min(100, healthPercent + configuration.recoveryPercent);
        next.recovery = HeroRecoveryPhase.Complete;

        return true;
    }

    /// <summary>Applies committed data while preserving the owned hero entity instance.</summary>
    /// <param name="saved">Committed transaction state.</param>
    internal void Restore(HomeVillageSave saved)
    {
        configurationId = saved.heroId;
        healthPercent = saved.healthPercent;
        gold = saved.heroGold;
        recovery = saved.recovery;
        healingEndTicks = saved.healingEndTicks;
    }
}
