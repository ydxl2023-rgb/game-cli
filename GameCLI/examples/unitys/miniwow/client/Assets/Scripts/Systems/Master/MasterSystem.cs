using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Oathx.Core;
using UnityEngine;

/// <summary>Reports an accepted local login.</summary>
public sealed class LoginSucceed : IEvent { }

/// <summary>Requests return to the local entry page.</summary>
public sealed class ReturnToLogin : IEvent
{
    /// <summary>Whether entry should display retry feedback.</summary>
    public bool failed
    { get; set; }
}

/// <summary>Requests activation of a configured business system.</summary>
public sealed class SystemActive : IEvent
{
    /// <summary>Stable system configuration identifier.</summary>
    public int id
    { get; set; }

    /// <summary>Optional event delivered after actual system readiness.</summary>
    public IEvent complete
    { get; set; }
}

/// <summary>Requests unloading of a configured business system.</summary>
public sealed class SystemDetive : IEvent
{
    /// <summary>Stable system configuration identifier.</summary>
    public int id
    { get; set; }
}

/// <summary>Remains installed and handles login and business system lifecycle events.</summary>
public sealed class MasterSystem : RuntimeSystem
{
    /// <summary>Activation requests awaiting loading or actual readiness.</summary>
    private readonly Dictionary<int, SystemActive> pending = new Dictionary<int, SystemActive>();

    /// <summary>Registers lifecycle handlers for the resident coordinator.</summary>
    /// <returns>Whether event subscriptions were installed.</returns>
    public override bool OnActive()
    {
        Subscribe<LoginSucceed>(OnLoginSucceed);
        Subscribe<HomeReady>(OnHomeReady);
        Subscribe<ReturnToLogin>(OnReturnToLogin);
        Subscribe<SystemActive>(OnSystemActive);
        Subscribe<SystemDetive>(OnSystemDetive);

        return true;
    }

    /// <summary>Releases subscriptions and business modules when the plugin shuts down.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        Unload();
    }

    /// <summary>Releases business systems while keeping the startup coordinator installed.</summary>
    public void Unload()
    {
        pending.Clear();

        Assembly assembly = Assembly.GetExecutingAssembly();
        foreach (var system in Config.Excel.functionSystemConfig.Values)
        {
            if (system.ID != Config.Excel.SYS_MASTER)
            {
                var type = assembly.GetType(system.ClassType, true);
                dispatcher.Unload(type);
            }
        }
    }

    /// <summary>Starts Home while keeping Login visible during initialization.</summary>
    /// <param name="message">The login success notification.</param>
    private void OnLoginSucceed(LoginSucceed message)
    {
        RequestActivation(TaskConfigurationIds.system, null);
        RequestActivation(10006, null);
        RequestActivation(10007, null);
        RequestActivation(10009, null);
        RequestActivation(10010, null);
        RequestActivation(10011, null);
        RequestActivation(Config.Excel.SYS_HOME, null);
    }

    /// <summary>Releases Login only after Home has initialized its scene and main page.</summary>
    /// <param name="message">The Home readiness notification.</param>
    private void OnHomeReady(HomeReady message)
    {
        UnloadSystemById(Config.Excel.SYS_LOGIN);
    }

    /// <summary>Unloads gameplay and restores the login page.</summary>
    /// <param name="message">The return request and retry feedback.</param>
    private void OnReturnToLogin(ReturnToLogin message)
    {
        Unload();

        OpenLogin(message.failed);
    }

    /// <summary>Queues an activation after the current event dispatch finishes.</summary>
    /// <param name="message">The requested system and completion event.</param>
    private void OnSystemActive(SystemActive message)
    {
        RequestActivation(message.id, message.complete);
    }

    /// <summary>Cancels pending activation and unloads the requested business system.</summary>
    /// <param name="message">The configured system to unload.</param>
    private void OnSystemDetive(SystemDetive message)
    {
        UnloadSystemById(message.id);
    }

    /// <summary>Requests the entry page and its feedback refresh.</summary>
    /// <param name="failed">Whether the previous entry failed.</param>
    private void OpenLogin(bool failed)
    {
        RequestActivation(Config.Excel.SYS_LOGIN, new LoginEntryResetRequested { failed = failed });
    }

    /// <summary>Records the latest activation for a configured business module.</summary>
    /// <param name="id">The system configuration identifier.</param>
    /// <param name="complete">Optional event dispatched when the system is ready.</param>
    private void RequestActivation(int id, IEvent complete)
    {
        var configuration = Config.Excel.functionSystemConfig[id];
        if (configuration.ID == Config.Excel.SYS_MASTER)
            throw new System.InvalidOperationException("Master is owned by the plugin lifecycle.");

        var request = new SystemActive { id = id, complete = complete };
        pending[id] = request;

        XCoroutine.Run(ActivateSystem(request));
    }

    /// <summary>Resolves and unloads a business module through the dispatcher.</summary>
    /// <param name="id">The configured business system identifier.</param>
    private void UnloadSystemById(int id)
    {
        var configuration = Config.Excel.functionSystemConfig[id];
        if (configuration.ID == Config.Excel.SYS_MASTER)
            throw new System.InvalidOperationException("Master is owned by the plugin lifecycle.");

        pending.Remove(id);

        var type = Assembly.GetExecutingAssembly().GetType(configuration.ClassType, true);
        dispatcher.Unload(type);
    }

    /// <summary>Loads outside registry enumeration and waits for the requested system to become ready.</summary>
    /// <param name="request">The activation request that owns this operation.</param>
    /// <returns>The deferred system activation.</returns>
    private IEnumerator ActivateSystem(SystemActive request)
    {
        yield return null;

        if (!pending.TryGetValue(request.id, out var current) || !ReferenceEquals(current, request))
            yield break;

        RuntimeSystem system;
        try
        {
            var configuration = Config.Excel.functionSystemConfig[request.id];
            var type = Assembly.GetExecutingAssembly().GetType(configuration.ClassType, true);
            system = (RuntimeSystem)dispatcher.Load(type);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            XPluginUtility.FireEvent(new ReturnToLogin { failed = true });

            yield break;
        }

        float deadline = Time.realtimeSinceStartup + 40f;
        while (pending.TryGetValue(request.id, out current) && ReferenceEquals(current, request))
        {
            if (system.IsReady())
            {
                pending.Remove(request.id);
                if (request.complete != null)
                    XPluginUtility.FireEvent(request.complete);

                yield break;
            }

            if (Time.realtimeSinceStartup >= deadline)
            {
                pending.Remove(request.id);
                XPluginUtility.FireEvent(new ReturnToLogin { failed = true });

                yield break;
            }

            yield return null;
        }
    }
}
