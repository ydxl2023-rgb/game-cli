using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Expression;

/// <summary>Checks task rules using isolated entities and the exported product configuration.</summary>
public static class MiniwowTaskProbe
{
    /// <summary>Number of verified assertions in the current run.</summary>
    private static int checks;

    /// <summary>Exercises rules, chapter ordering, reward transactions and guide execution.</summary>
    /// <returns>A concise machine-readable result.</returns>
    public static string RunAll()
    {
        checks = 0;
        var collection = new TaskCollectionEntity();
        collection.OpenChapter(1);
        var first = collection.GetTask(60001);
        Check(collection.GetTrackedTask().configuration.ID == 60001, "Authored chapter starts at 60001");
        Check(collection.GetTasks().Count == Excel.taskChapterConfig[1].TaskIDs.Count, "Every chapter task is issued");
        Check(first.Claim(null) == TaskClaimResult.NotCompleted, "Incomplete claims do not succeed");
        Check(!first.Apply(Observation(60002, ETaskConditionType.FinishTalk, 1)), "Another task cannot update this one");
        Check(!first.Apply(Observation(60001, ETaskConditionType.Login, 1)), "Condition types must match");
        Check(first.Apply(Observation(60001, ETaskConditionType.FinishTalk, 1)), "Actual story observation completes task");
        Check(!first.Apply(Observation(60001, ETaskConditionType.FinishTalk, 1)), "Repeated observation is idempotent");
        Check(first.status == TaskStatus.Completed, "Completion awaits claim");
        Check(first.Claim(null) == TaskClaimResult.Claimed, "Empty reward task can be claimed");
        Check(first.Claim(null) == TaskClaimResult.AlreadyClaimed, "Duplicate claim rejected");
        Check(collection.GetTrackedTask().configuration.ID == 60002, "Next active task follows authored order");

        var hero = collection.GetTask(60004);
        var wrongHero = Observation(60004, ETaskConditionType.ReceiveFreeHero, 1);
        wrongHero.param1 = 124;
        Check(!hero.Apply(wrongHero), "Wrong hero must not count");
        wrongHero.param1 = 125;
        Check(hero.Apply(wrongHero), "Configured hero counts");
        Check(collection.GetTrackedTask().configuration.ID == 60004, "Pending reward takes priority");
        Check(!collection.Dispatch(60004), "Duplicate issuance does not reset progress");

        bool blocked = false;
        try { collection.OpenChapter(2); }
        catch (InvalidOperationException) { blocked = true; }
        Check(blocked, "Cannot replace an unfinished chapter");
        Check(collection.chapterId == 1, "Rejected transition preserves chapter");

        CheckRewards();
        CheckGuides();
        foreach (int id in Excel.taskChapterConfig[1].TaskIDs)
        {
            var task = collection.GetTask(id);
            var observation = Observation(id, task.configuration.TaskConditionType, task.configuration.FinishValue);
            observation.param1 = task.configuration.Param1;
            observation.param2 = task.configuration.Param2;
            observation.param3 = task.configuration.Param3;
            task.Apply(observation);
            task.Claim(new RewardReceiver());
        }
        Check(collection.GetTrackedTask() == null, "Claimed chapter has no tracked task");
        collection.OpenChapter(2);
        Check(collection.chapterId == 2, "Explicit next chapter accepted");
        Check(collection.GetTask(60001).status == TaskStatus.Claimed, "Prior receipts retained");
        collection.Destroy();

        return JsonConvert.SerializeObject(new { status = "passed", checks });
    }

    /// <summary>Checks atomic acceptance, rejection, missing integration and reentrant claims.</summary>
    private static void CheckRewards()
    {
        var definition = JsonConvert.DeserializeObject<Config.Task>(
            "{'ID':900000,'FinishValue':3,'TaskConditionType':32,'TaskConditionShape':1,"
            + "'Rewards':[{'Id':1,'Num':10}],'QuestValue':2}");
        var task = new TaskEntity(definition);
        task.Apply(Observation(900000, ETaskConditionType.FinishTalk, 2));
        Check(task.progress == 2 && task.status == TaskStatus.Active, "Partial progress stays active");
        task.Apply(Observation(900000, ETaskConditionType.FinishTalk, 1));
        Check(task.progress == 2, "Old absolute progress does not roll back");
        task.Apply(Observation(900000, ETaskConditionType.FinishTalk, 10));
        Check(task.progress == 3, "Progress caps at target");
        Check(task.Claim(null) == TaskClaimResult.MissingRewardReceiver, "No fabricated inventory grant");
        var receiver = new RewardReceiver { accept = false, reenter = task };
        Check(task.Claim(receiver) == TaskClaimResult.Rejected, "Rejected economy transaction remains claimable");
        Check(receiver.reentry == TaskClaimResult.InProgress, "Reentrant grant cannot duplicate reward");
        Check(task.status == TaskStatus.Completed, "Rejected claim preserves completed state");
        receiver.fail = true;
        bool threw = false;
        try { task.Claim(receiver); }
        catch (InvalidOperationException) { threw = true; }
        Check(threw && task.status == TaskStatus.Completed, "Failed receiver restores claimable state");
        receiver.fail = false;
        receiver.accept = true;
        Check(task.Claim(receiver) == TaskClaimResult.Claimed, "Successful grant records claim");
        int calls = receiver.calls;
        Check(task.Claim(receiver) == TaskClaimResult.AlreadyClaimed && receiver.calls == calls,
            "Repeated claim never calls economy again");
    }

    /// <summary>Checks cached conditions, asynchronous waits, edges and canceled completion handles.</summary>
    private static void CheckGuides()
    {
        var nodes = new Dictionary<int, TaskGuide> {
            [1] = Guide(1, "hp > 100", "Wait(0.5)", 2),
            [2] = Guide(2, "true", "Wait(0)", -1),
            [3] = Guide(3, "true", "UnknownCommand()", -1)
        };
        var runner = new TaskGuideRunner(nodes);
        runner.SetVariable("hp", new ExpressionValue(80.0));
        runner.Start(1);
        runner.Tick(1);
        Check(runner.nodeId == 1, "False condition waits without following a failure edge");
        int cached = runner.cacheCount;
        runner.SetVariable("hp", new ExpressionValue(150.0));
        runner.Retry();
        Check(runner.cacheCount == cached + 1, "Condition reused, action compiled once");
        runner.Tick(0.2f);
        Check(runner.nodeId == 1, "Wait does not complete before duration");
        runner.Tick(0.4f);
        Check(runner.nodeId == 2, "Success follows NextID");
        runner.Tick(0);
        Check(runner.nodeId == 0, "Negative successor ends chain");
        cached = runner.cacheCount;
        runner.Start(1);
        Check(runner.cacheCount == cached, "Repeated guide reuses both programs");
        runner.Cancel();
        runner.Tick(10);
        Check(runner.nodeId == 0, "Cancellation prevents successor execution");
        bool unknown = false;
        try { runner.Start(3); }
        catch (ExpressionException) { unknown = true; }
        Check(unknown && runner.nodeId == 0, "Unknown commands fail explicitly and release chain");
        var external = new TaskPresentationAction(TaskPresentation.Navigate, new ExpressionValue(60002.0));
        external.Cancel();
        external.Complete(true);
        Check(external.status == TaskActionStatus.Cancelled, "Late external completion cannot revive canceled action");
        runner.Clear();
        Check(runner.cacheCount == 0, "Shutdown clears expression cache");
    }

    /// <summary>Starts the actual login-to-game path through its public event.</summary>
    public static void EnterGame()
    {
        XPluginUtility.FireEvent(new LoginSucceed());
    }

    /// <summary>Checks task system installation and dispatch through the real plugin.</summary>
    /// <returns>Live runtime evidence.</returns>
    public static string VerifyInstalled()
    {
        var collection = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        Check(collection.chapterId == 1, "Configured initial chapter installed");
        XPluginUtility.FireEvent(Observation(60001, ETaskConditionType.FinishTalk, 1));
        Check(collection.GetTask(60001).status == TaskStatus.Completed, "TaskSystem receives progress event");
        XPluginUtility.FireEvent(new TaskClaimRequested { taskId = 60001 });
        Check(collection.GetTask(60001).status == TaskStatus.Claimed, "TaskSystem receives claim event");

        return JsonConvert.SerializeObject(new {
            status = "passed",
            chapter = collection.chapterId,
            nextTask = collection.GetTrackedTask().configuration.ID,
            systemId = TaskConfigurationIds.system
        });
    }

    /// <summary>Simulates a failed game entry through the coordinator's public event.</summary>
    public static void FailEntry()
    {
        XPluginUtility.FireEvent(new ReturnToLogin { failed = true });
    }

    /// <summary>Returns through the public session event.</summary>
    public static void Return()
    {
        XPluginUtility.FireEvent(new ReturnToLogin());
    }

    /// <summary>Checks that stopped systems no longer consume task events.</summary>
    /// <returns>Runtime teardown evidence.</returns>
    public static string VerifyUnloaded()
    {
        var collection = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        XPluginUtility.FireEvent(Observation(60002, ETaskConditionType.FinishTalk, 1));
        Check(collection.GetTask(60002).status == TaskStatus.Active, "Unloaded system ignores progress events");

        return "passed: task subscriptions released; entity state retained until plugin shutdown";
    }

    /// <summary>Creates an isolated gameplay report.</summary>
    /// <param name="id">Target task.</param>
    /// <param name="type">Condition type.</param>
    /// <param name="value">Absolute progress.</param>
    /// <returns>The gameplay observation.</returns>
    private static TaskProgressReported Observation(int id, ETaskConditionType type, long value)
    {
        return new TaskProgressReported { taskId = id, condition = type, value = value };
    }

    /// <summary>Creates an isolated guide node.</summary>
    /// <param name="id">Node ID.</param>
    /// <param name="condition">Condition expression.</param>
    /// <param name="action">Action expression.</param>
    /// <param name="next">Success successor.</param>
    /// <returns>The guide fixture.</returns>
    private static TaskGuide Guide(int id, string condition, string action, int next)
    {
        return JsonConvert.DeserializeObject<TaskGuide>(JsonConvert.SerializeObject(new {
            ID = id, ConditionExpression = condition, ActionExpression = action, NextID = next, FailureNextID = 0
        }));
    }

    /// <summary>Raises a precise failure when an expected rule is broken.</summary>
    /// <param name="passed">Observed result.</param>
    /// <param name="message">Rule being checked.</param>
    private static void Check(bool passed, string message)
    {
        if (!passed)
            throw new InvalidOperationException(message);

        checks++;
    }

    /// <summary>Isolated transaction fixture; never writes to the product inventory.</summary>
    private sealed class RewardReceiver : ITaskRewardReceiver
    {
        /// <summary>Whether the test receiver commits the batch.</summary>
        public bool accept { get; set; } = true;

        /// <summary>Whether the receiver injects an atomic failure.</summary>
        public bool fail { get; set; }

        /// <summary>Number of grant invocations.</summary>
        public int calls { get; private set; }

        /// <summary>Optional task used to verify synchronous reentrancy.</summary>
        public TaskEntity reenter { get; set; }

        /// <summary>Result observed by a reentrant attempt.</summary>
        public TaskClaimResult reentry { get; private set; }

        /// <summary>Simulates a complete economy transaction.</summary>
        /// <param name="configuration">Task definition passed by the claim owner.</param>
        /// <returns>Whether the fixture accepts this batch.</returns>
        public bool Grant(Config.Task configuration)
        {
            calls++;
            if (reenter != null)
                reentry = reenter.Claim(this);
            if (fail)
                throw new InvalidOperationException("Injected atomic rejection");

            return accept;
        }
    }
}
