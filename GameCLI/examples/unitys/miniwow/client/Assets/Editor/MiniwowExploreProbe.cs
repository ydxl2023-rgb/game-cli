using System;
using System.Linq;
using System.IO;
using Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oathx.Core;
using Oathx.Entity;
using UnityEngine;

/// <summary>Verifies isolated exploration rules and reports actual scene progress.</summary>
public static class MiniwowExploreProbe
{
    /// <summary>Exercises real definitions without touching the user's save.</summary>
    /// <returns>Verified business invariants.</returns>
    public static string VerifyRules()
    {
        var definition = Excel.exploreJourneyConfig.Values.First();
        var rules = new ExploreEntity();

        var saved = new HomeVillageSave {
            heroId = definition.HeroID,
            recovery = HeroRecoveryPhase.Complete
        };

        long now = DateTime.UtcNow.Ticks;
        Assert(!rules.Begin(saved, definition), "requires a claimed prerequisite");
        saved.receipts.Add(definition.RequiredTaskID);
        saved.innRestEndTicks = now;
        Assert(!rules.Begin(saved, definition), "cannot leave during a paid meal");
        saved.innRestEndTicks = 0;
        Assert(rules.Begin(saved, definition), "starts a valid departure");
        Assert(!rules.Begin(saved, definition), "duplicate departure rejected");
        Assert(!rules.Advance(saved, definition, long.MaxValue), "waiting for actual world arrival");
        Assert(saved.explore.counts.Count == 0, "departure does not complete a task");
        Assert(rules.Arrive(saved, definition, now), "actual arrival starts exploration");
        long travelDeadline = saved.explore.endTicks;
        Assert(!rules.Arrive(saved, definition, now), "duplicate arrival rejected");
        Assert(!rules.Advance(saved, definition, travelDeadline - 1), "travel must finish");
        saved = JsonConvert.DeserializeObject<HomeVillageSave>(JsonConvert.SerializeObject(saved));
        Assert(rules.Advance(saved, definition, travelDeadline), "restored travel reaches event");
        Assert(saved.explore.counts.Count == 0, "event entry is not completion");
        long eventDeadline = saved.explore.endTicks;
        Assert(!rules.Advance(saved, definition, eventDeadline - 1), "event duration respected");
        Assert(rules.Advance(saved, definition, eventDeadline), "event settles once");
        saved = JsonConvert.DeserializeObject<HomeVillageSave>(JsonConvert.SerializeObject(saved));
        Assert(!rules.Advance(saved, definition, long.MaxValue), "restored result does not settle twice");
        Assert(saved.explore.counts[definition.EventType] == 1, "one absolute event observation");
        Assert(rules.Arrive(saved, definition, eventDeadline), "actual return finishes excursion");
        Assert(!rules.Arrive(saved, definition, eventDeadline), "duplicate return rejected");
        Assert(!rules.Begin(saved, definition), "introductory excursion is not replayed");
        var candidate = JObject.FromObject(definition);
        candidate[nameof(ExploreJourney.ID)] = definition.ID + 1;
        var following = candidate.ToObject<ExploreJourney>();
        Assert(rules.Begin(saved, following), "another configured excursion uses the same rules");
        Assert(saved.items.Count == 0, "no unsupported server rewards fabricated");
        rules.Destroy();

        return JsonConvert.SerializeObject(new { passed = true, checks = 20 });
    }

    /// <summary>Stages a disposable in-progress save from the immutable pre-test user checkpoint.</summary>
    public static void PrepareResumeFixture()
    {
        var backup = JObject.Parse(File.ReadAllText("Library/OpeningQA/explore-user-checkpoint.json"));
        foreach (var property in backup.Properties())
        {
            if (property.Value.Type == JTokenType.Null)
                PlayerPrefs.DeleteKey(property.Name);
            else if (property.Value.Type == JTokenType.Integer)
                PlayerPrefs.SetInt(property.Name, (int)property.Value);
            else
                PlayerPrefs.SetString(property.Name, (string)property.Value);
        }

        string key = StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village);
        var saved = JsonConvert.DeserializeObject<HomeVillageSave>(PlayerPrefs.GetString(key));
        saved.explore = new ExploreSave {
            journeyId = Excel.exploreJourneyConfig.Values.First().ID,
            phase = ExplorePhase.Executing,
            endTicks = DateTime.UtcNow.AddSeconds(60).Ticks
        };
        PlayerPrefs.SetString(key, JsonConvert.SerializeObject(saved));
        PlayerPrefs.Save();
    }

    /// <summary>Unloads only the exploration module through normal lifecycle events.</summary>
    public static void Unload()
    {
        XPluginUtility.FireEvent(new SystemDetive { id = 10009 });
    }

    /// <summary>Reinstalls exploration without recreating the Home scene or its actors.</summary>
    public static void Reload()
    {
        XPluginUtility.FireEvent(new SystemActive { id = 10009 });
    }

    /// <summary>Captures actual actor position, durable event counts, and the configured task outcome.</summary>
    /// <returns>Runtime evidence without mutating progress.</returns>
    public static string Snapshot()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var definition = Excel.exploreJourneyConfig.Values.First();
        var actor = home.FindActor(definition.ActorID);
        var task = tasks.GetTask(60105);
        var saved = home.village.Snapshot();

        return JsonConvert.SerializeObject(new {
            journey = saved.explore,
            position = actor.position.ToString(),
            actorPhase = actor.phase.ToString(),
            taskStatus = task.status.ToString(),
            task.progress,
            saved.items,
            saved.energyPercent
        });
    }

    /// <summary>Fails verification at the first broken business invariant.</summary>
    /// <param name="condition">Invariant outcome.</param>
    /// <param name="description">Expected business behavior.</param>
    private static void Assert(bool condition, string description)
    {
        if (!condition)
            throw new InvalidOperationException("Exploration verification failed: " + description);
    }
}
