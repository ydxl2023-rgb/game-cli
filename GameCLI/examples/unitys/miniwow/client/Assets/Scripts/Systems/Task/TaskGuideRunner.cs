using System;
using System.Collections.Generic;
using Config;
using Oathx.Expression;

/// <summary>Configuration command names, distinct from runtime routing enums.</summary>
public static class TaskCommands
{
    /// <summary>Configured delay command.</summary>
    public const string wait = "Wait";

    /// <summary>Configured ShowMain command.</summary>
    public const string showMain = "ShowMain";

    /// <summary>Configured SelectExploreArea command.</summary>
    public const string selectExploreArea = "SelectExploreArea";

    /// <summary>Configured Tips command.</summary>
    public const string tips = "Tips";

    /// <summary>Configured GuideFinger command.</summary>
    public const string guideFinger = "GuideFinger";

    /// <summary>Configured CloseUI command.</summary>
    public const string closeUI = "CloseUI";

    /// <summary>Configured OpenUI command.</summary>
    public const string openUI = "OpenUI";

    /// <summary>Configured ShowDialogue command.</summary>
    public const string showDialogue = "ShowDialogue";

    /// <summary>Configured Navigate command.</summary>
    public const string navigate = "Navigate";
}

/// <summary>Executes one owned guide chain with cached expressions and explicit action completion.</summary>
public sealed class TaskGuideRunner
{
    /// <summary>Shared compiler and command registry for this installed task system.</summary>
    private readonly ExpressionEngine engine = new ExpressionEngine();

    /// <summary>Variables used by both condition and action expressions.</summary>
    private readonly ExpressionContext context;

    /// <summary>Guide definitions read from the exported table.</summary>
    private readonly IReadOnlyDictionary<int, TaskGuide> definitions;

    /// <summary>Presentation currently owned by this chain.</summary>
    private ITaskAction action;

    /// <summary>The current guide ID, or zero after the chain ends.</summary>
    public int nodeId
    { get; private set; }

    /// <summary>Number of cached condition and action programs.</summary>
    public int cacheCount
    { get { return engine.cacheCount; } }

    /// <summary>Creates a guide executor and registers the product command signatures.</summary>
    /// <param name="definitions">Exported guide nodes.</param>
    public TaskGuideRunner(IReadOnlyDictionary<int, TaskGuide> definitions)
    {
        this.definitions = definitions;
        context = engine.CreateContext();

        engine.Register(TaskCommands.wait, ExpressionFunctionKind.Command, ExpressionValueKind.Void,
            OnWait, ExpressionValueKind.Number);
        Register(TaskCommands.navigate, TaskPresentation.Navigate, ExpressionValueKind.Number);
        Register(TaskCommands.showDialogue, TaskPresentation.ShowDialogue, ExpressionValueKind.Number);
        Register(TaskCommands.openUI, TaskPresentation.OpenUI, ExpressionValueKind.String);
        Register(TaskCommands.closeUI, TaskPresentation.CloseUI, ExpressionValueKind.String);
        Register(TaskCommands.guideFinger, TaskPresentation.GuideFinger, ExpressionValueKind.String);
        Register(TaskCommands.tips, TaskPresentation.Tips, ExpressionValueKind.Number);
        Register(TaskCommands.selectExploreArea, TaskPresentation.SelectExploreArea, ExpressionValueKind.Number);
        Register(TaskCommands.showMain, TaskPresentation.ShowMain);
    }

    /// <summary>Starts a guide after canceling the previous owned action.</summary>
    /// <param name="id">Guide ID; zero means no configured guide.</param>
    public void Start(int id)
    {
        Cancel();
        if (id == 0)
            return;

        var definition = definitions[id];
        nodeId = definition.ID;

        Retry();
    }

    /// <summary>Rechecks a blocked condition only when explicitly requested.</summary>
    public void Retry()
    {
        if (nodeId == 0 || action != null)
            return;

        var definition = definitions[nodeId];
        if (!engine.Evaluate(definition.ConditionExpression, context).AsBoolean())
            return;

        try
        {
            engine.Execute(definition.ActionExpression, context);
            if (action == null)
                throw new InvalidOperationException("Guide expression must create one presentation action.");

            action.Start();
        }
        catch
        {
            Cancel();

            throw;
        }
    }

    /// <summary>Advances the action, following its success or failure edge on completion.</summary>
    /// <param name="elapsed">Elapsed seconds.</param>
    public void Tick(float elapsed)
    {
        if (action == null)
            return;

        action.Tick(elapsed);
        if (action.status == TaskActionStatus.Running)
            return;

        var definition = definitions[nodeId];
        int next = action.status == TaskActionStatus.Succeeded ? definition.NextID : definition.FailureNextID;
        action = null;
        nodeId = next > 0 ? next : 0;

        Retry();
    }

    /// <summary>Sets a current condition variable without invalidating compiled programs.</summary>
    /// <param name="name">Expression variable name.</param>
    /// <param name="value">Current value.</param>
    public void SetVariable(string name, ExpressionValue value)
    {
        context.Set(name, value);
    }

    /// <summary>Registers an explicit, side-effect-free business query for conditions.</summary>
    /// <param name="name">Configured method name.</param>
    /// <param name="result">Declared return type.</param>
    /// <param name="query">Query implementation.</param>
    /// <param name="parameters">Declared parameter types.</param>
    public void RegisterQuery(string name, ExpressionValueKind result,
        ExpressionFunction query, params ExpressionValueKind[] parameters)
    {
        engine.Register(name, ExpressionFunctionKind.Query, result, query, parameters);
    }

    /// <summary>Cancels the chain and its currently owned presentation.</summary>
    public void Cancel()
    {
        var previous = action;
        action = null;
        nodeId = 0;

        previous?.Cancel();
    }

    /// <summary>Releases registrations and cached programs at system shutdown.</summary>
    public void Clear()
    {
        Cancel();
        engine.Clear();
        context.Clear();
    }

    /// <summary>Registers an external presentation using the public typed request contract.</summary>
    /// <param name="name">Configured expression command name.</param>
    /// <param name="operation">Typed presentation operation.</param>
    /// <param name="parameters">Expected expression parameter types.</param>
    private void Register(string name, TaskPresentation operation, params ExpressionValueKind[] parameters)
    {
        engine.Register(name, ExpressionFunctionKind.Command, ExpressionValueKind.Void, arguments =>
        {
            var parameter = arguments.count == 0 ? default : arguments[0];
            SelectAction(new TaskPresentationAction(operation, parameter));

            return default;
        }, parameters);
    }

    /// <summary>Creates the built-in wait action.</summary>
    /// <param name="arguments">Configured wait duration.</param>
    /// <returns>A void expression value.</returns>
    private ExpressionValue OnWait(ExpressionArguments arguments)
    {
        SelectAction(new TaskWaitAction(arguments[0].AsNumber()));

        return default;
    }

    /// <summary>Rejects expressions that try to overwrite an action before it executes.</summary>
    /// <param name="candidate">Action created by the command.</param>
    private void SelectAction(ITaskAction candidate)
    {
        if (action != null)
            throw new InvalidOperationException("One guide node must create exactly one action.");

        action = candidate;
    }
}
