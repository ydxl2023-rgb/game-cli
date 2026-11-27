using System;
using System.IO;
using System.Linq;
using Config;
using Newtonsoft.Json;
using Oathx.Entity;
using UnityEngine;

/// <summary>Verifies recruitment and first-stage entry using isolated data and explicit replay fixtures.</summary>
public static class MiniwowRecruitProbe
{
    /// <summary>Checks durable recruitment rules without touching PlayerPrefs.</summary>
    /// <returns>Verified transaction invariants.</returns>
    public static string VerifyRules()
    {
        var tasks = new TaskCollectionEntity();
        tasks.OpenChapter(Excel.taskSettingConfig[1].InitialChapterID);
        var recruit = new RecruitEntity();
        var saved = new HomeVillageSave();
        var talk = Excel.recruitStepConfig[60106];
        Assert(!recruit.Execute(saved, talk, tasks), "conversation requires its real prerequisite");
        tasks.GetTask(60105).Restore(1, TaskStatus.Claimed);
        Assert(recruit.Execute(saved, talk, tasks), "acknowledged dialogue commits");
        Assert(!recruit.Execute(saved, talk, tasks), "dialogue cannot commit twice");
        tasks.GetTask(60106).Restore(1, TaskStatus.Claimed);
        var horn = Excel.recruitStepConfig[60112];
        Assert(!recruit.Execute(saved, horn, tasks), "horn requires the reward item");
        saved.items[horn.CostItemID] = 1;
        Assert(recruit.Execute(saved, horn, tasks), "horn consumes its original item");
        Assert(saved.items[horn.CostItemID] == 0, "exactly one whistle consumed");
        Assert(!saved.recruit.completed.Contains(horn.ID), "payment is not movie completion");
        saved = JsonConvert.DeserializeObject<HomeVillageSave>(JsonConvert.SerializeObject(saved));
        Assert(!recruit.Execute(saved, horn, tasks), "reload cannot pay again");
        Assert(recruit.CompleteHorn(saved), "actual movie acknowledgement completes paid intent");
        Assert(!recruit.CompleteHorn(saved), "duplicate movie callback rejected");
        tasks.GetTask(60112).Restore(1, TaskStatus.Claimed);
        var kai = Excel.recruitStepConfig[60113];
        Assert(recruit.Execute(saved, kai, tasks), "first free hero received");
        Assert(!recruit.Execute(saved, kai, tasks), "duplicate hero receive rejected");
        var toby = Excel.recruitStepConfig[60114];
        Assert(!recruit.Execute(saved, toby, tasks), "second hero waits for first receipt");
        tasks.GetTask(60113).Restore(1, TaskStatus.Claimed);
        Assert(recruit.Execute(saved, toby, tasks), "second free hero received");
        saved.heroId = 125;
        var roster = new HeroCollectionEntity(saved);
        Assert(roster.count == 3 && roster.Contains(129) && roster.Contains(133), "three actual hero entities");
        var entry = new BattleEntryEntity();
        var definition = Excel.battleEntryConfig[101001];
        Assert(entry.Select(definition, tasks, roster) != 0, "original feature requirement enforced");
        tasks.GetTask(definition.RequiredTaskID).Restore(1, TaskStatus.Claimed);
        Assert(entry.Select(definition, tasks, roster) == 0, "first stage does not require previous battle or meal");
        Assert(tasks.GetTask(60111).status == TaskStatus.Active, "entry is not a battle victory");
        entry.Destroy();
        roster.Destroy();
        recruit.Destroy();
        tasks.Destroy();

        return JsonConvert.SerializeObject(new { passed = true, checks = 18 });
    }

    /// <summary>Stages only a disposable post-exploration replay after an immutable user backup exists.</summary>
    public static void PrepareReplay()
    {
        if (!File.Exists("Library/OpeningQA/first-battle-user-checkpoint.json"))
            throw new InvalidOperationException("Create the immutable current user checkpoint before replay.");

        int chapterId = Excel.taskSettingConfig[1].InitialChapterID;
        var tasks = new TaskCollectionSave { chapterId = chapterId };
        bool beforeBrother = true;

        var village = new HomeVillageSave {
            heroId = 125,
            healthPercent = 100,
            heroGold = 500000,
            recruitmentAcknowledged = true,
            recovery = HeroRecoveryPhase.Complete,
            innEnergyInitialized = true,
            energyPercent = 0,
            purchases = 1,
            crafts = 1
        };

        foreach (int id in Excel.taskChapterConfig[chapterId].TaskIDs)
        {
            if (id == 60106)
                beforeBrother = false;

            tasks.tasks.Add(new TaskReceipt {
                id = id,
                status = beforeBrother ? TaskStatus.Claimed : TaskStatus.Active,
                progress = beforeBrother ? 1 : 0
            });
            if (beforeBrother)
                village.receipts.Add(id);
        }

        village.items[Excel.villageSettingConfig[1].GoldID] = 19000;
        village.buildings[60018] = 1;
        village.explore.journeyId = 1;
        village.explore.phase = ExplorePhase.Completed;
        village.explore.completed.Add(1);
        village.explore.counts[4] = 1;
        PlayerPrefs.SetString(StorageKey.For<TaskSystem, TaskCollectionEntity>(entity => entity.chapterId),
            JsonConvert.SerializeObject(tasks));
        PlayerPrefs.SetString(StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village),
            JsonConvert.SerializeObject(village));
        PlayerPrefs.SetInt(StorageKey.For<HomeSystem, HomeEntity>(entity => entity.opening),
            (int)HomeOpeningPhase.AwaitRecruit);
        PlayerPrefs.Save();
    }

    /// <summary>Captures actual task, hero, movie and economic state.</summary>
    /// <returns>Detached runtime evidence.</returns>
    public static string Snapshot()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var recruit = XEntityManager.GetSingleton().QueryEntity<RecruitEntity>();
        var entry = XEntityManager.GetSingleton().QueryEntity<BattleEntryEntity>();
        var saved = home.village.Snapshot();

        return JsonConvert.SerializeObject(new {
            tracked = tasks.GetTrackedTask()?.configuration.ID,
            heroes = home.village.heroes.GetHeroIds(),
            saved.recruit,
            saved.items,
            saved.buildings,
            saved.innMeals,
            saved.innCrafts,
            saved.innRestEndTicks,
            selected = entry.selectedId,
            recruit.selectedStep,
            recruit.dialogueIndex,
            tasks = new[] { 60106, 60112, 60113, 60114, 60107, 60108, 60109, 60110, 60111 }
                .Select(id => new { id, status = tasks.GetTask(id).status.ToString(), tasks.GetTask(id).progress })
        });
    }

    /// <summary>Inspects the actual decoder without changing playback.</summary>
    /// <returns>Clip, frame and render-surface state.</returns>
    public static string InspectMovie()
    {
        var widget = UnityEngine.Object.FindObjectOfType<UIRecruit>();
        var movie = widget.GetComponent<UnityEngine.Video.VideoPlayer>();
        var screen = widget.GetComponentInChildren<UnityEngine.UI.RawImage>(true);

        return JsonConvert.SerializeObject(new {
            movie.isPlaying, movie.isPrepared, movie.time, movie.frame, movie.frameCount,
            movie.length, movie.width, movie.height,
            texture = screen.texture ? screen.texture.name : null,
            target = movie.targetTexture ? movie.targetTexture.name : null,
            active = screen.gameObject.activeInHierarchy,
            movie.enabled, movie.gameObject.activeInHierarchy, Time.timeScale,
            movie.playbackSpeed, Application.runInBackground, Application.isFocused,
            gameTime = Time.time, gameFrame = Time.frameCount,
            clock = movie.timeUpdateMode.ToString(), audio = movie.audioOutputMode.ToString()
        });
    }

    /// <summary>Clicks the exact original guide target through its real UI raycast.</summary>
    /// <param name="key">Original configured guide target.</param>
    /// <returns>Whether the real control was clicked.</returns>
    public static bool ClickGuide(string key)
    {
        var target = TaskGuideTarget.Resolve(key);
        string previous = target.name;
        target.name = "RecruitGuideProbe";
        try
        {
            return MiniwowHospitalProbe.Click(target.name);
        }
        finally
        {
            target.name = previous;
        }
    }

    /// <summary>Inspects visible fingers and page multiplicity without changing state.</summary>
    /// <returns>Actual registered targets, visible fingers and page counts.</returns>
    public static string InspectPresentation()
    {
        return JsonConvert.SerializeObject(new {
            pages = UnityEngine.Object.FindObjectsOfType<UIBattleEntry>().Length,
            recruitPages = UnityEngine.Object.FindObjectsOfType<UIRecruit>().Length,
            targets = UnityEngine.Object.FindObjectsOfType<TaskGuideTarget>().Select(target => new {
                name = target.name,
                fingers = target.GetComponentsInChildren<UnityEngine.UI.Graphic>()
                    .Count(graphic => graphic.transform != target.transform)
            })
        });
    }

    /// <summary>Repeats entry intentions to verify one asynchronous page owner.</summary>
    public static void RepeatOpen()
    {
        Oathx.Core.XPluginUtility.FireEvent(new BattleEntryOpenRequested());
        Oathx.Core.XPluginUtility.FireEvent(new BattleEntryOpenRequested());
    }

    /// <summary>Fails at the first broken transaction invariant.</summary>
    /// <param name="condition">Actual result.</param>
    /// <param name="message">Expected behavior.</param>
    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException("Recruit verification failed: " + message);
    }
}
