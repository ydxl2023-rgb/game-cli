using System;
using System.Collections.Generic;
using Config;
using Oathx.Entity;

/// <summary>Durable stages of a configured village excursion.</summary>
public enum ExplorePhase
{
    /// <summary>No excursion has started.</summary>
    None,
    /// <summary>The world actor is walking to the village exit.</summary>
    Leaving,
    /// <summary>The hero is traveling to the exploration event.</summary>
    Traveling,
    /// <summary>The hero is executing the configured event.</summary>
    Executing,
    /// <summary>The event has settled and the world actor is returning.</summary>
    Returning,
    /// <summary>The configured introductory excursion has finished.</summary>
    Completed
}

/// <summary>Saved excursion intent, independent of its current scene presentation.</summary>
public sealed class ExploreSave
{
    /// <summary>Configured excursion identity; zero means no excursion.</summary>
    public int journeyId
    { get; set; }

    /// <summary>Current durable business stage.</summary>
    public ExplorePhase phase
    { get; set; }

    /// <summary>UTC deadline for the currently executing timed stage.</summary>
    public long endTicks
    { get; set; }

    /// <summary>Completed event counts keyed by original event type.</summary>
    public Dictionary<int, long> counts
    { get; set; } = new Dictionary<int, long>();

    /// <summary>Completed configured excursions that cannot be issued again.</summary>
    public HashSet<int> completed
    { get; set; } = new HashSet<int>();

    /// <summary>Whether the hero is unavailable for village services.</summary>
    /// <returns>Whether an excursion still owns the hero.</returns>
    public bool IsAway()
    {
        return phase != ExplorePhase.None && phase != ExplorePhase.Completed;
    }
}

/// <summary>Computes excursion transitions on detached village transactions.</summary>
[Initialize(InstallType.Startup, true)]
public sealed class ExploreEntity : XEntity
{
    /// <summary>Starts a configured introductory excursion after its prerequisite receipt.</summary>
    /// <param name="next">Detached village transaction.</param>
    /// <param name="definition">Excursion configuration.</param>
    /// <returns>Whether the departure intent was accepted.</returns>
    public bool Begin(HomeVillageSave next, ExploreJourney definition)
    {
        if (next.explore.IsAway() || next.explore.completed.Contains(definition.ID)
            || next.heroId != definition.HeroID
            || !next.receipts.Contains(definition.RequiredTaskID)
            || next.recovery != HeroRecoveryPhase.Complete || next.innRestEndTicks != 0)
            return false;

        next.explore.journeyId = definition.ID;
        next.explore.phase = ExplorePhase.Leaving;

        return true;
    }

    /// <summary>Accepts actual world arrival, never a UI click or an elapsed travel estimate.</summary>
    /// <param name="next">Detached village transaction.</param>
    /// <param name="definition">Current excursion definition.</param>
    /// <param name="now">Current UTC ticks.</param>
    /// <returns>Whether the arrival advanced the saved excursion.</returns>
    public bool Arrive(HomeVillageSave next, ExploreJourney definition, long now)
    {
        if (next.explore.journeyId != definition.ID)
            return false;

        if (next.explore.phase == ExplorePhase.Leaving)
        {
            next.explore.phase = ExplorePhase.Traveling;
            next.explore.endTicks = now + (long)(definition.TravelSeconds * TimeSpan.TicksPerSecond);

            return true;
        }

        if (next.explore.phase != ExplorePhase.Returning)
            return false;

        next.explore.phase = ExplorePhase.Completed;
        next.explore.completed.Add(definition.ID);
        next.explore.endTicks = 0;

        return true;
    }

    /// <summary>Settles elapsed stages once, preserving their original deadlines across reloads.</summary>
    /// <param name="next">Detached village transaction.</param>
    /// <param name="definition">Current excursion definition.</param>
    /// <param name="now">Current UTC ticks.</param>
    /// <returns>Whether a timed stage changed.</returns>
    public bool Advance(HomeVillageSave next, ExploreJourney definition, long now)
    {
        var saved = next.explore;
        if (saved.journeyId != definition.ID || now < saved.endTicks)
            return false;

        bool changed = false;
        if (saved.phase == ExplorePhase.Traveling)
        {
            saved.phase = ExplorePhase.Executing;
            saved.endTicks += (long)(definition.ExecuteSeconds * TimeSpan.TicksPerSecond);
            changed = true;
        }

        if (saved.phase == ExplorePhase.Executing && now >= saved.endTicks)
        {
            saved.counts.TryGetValue(definition.EventType, out long count);
            saved.counts[definition.EventType] = checked(count + 1);
            saved.phase = ExplorePhase.Returning;
            saved.endTicks = 0;
            changed = true;
        }

        return changed;
    }
}
