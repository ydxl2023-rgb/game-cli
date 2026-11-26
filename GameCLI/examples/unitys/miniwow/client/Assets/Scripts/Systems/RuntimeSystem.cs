using System.Collections;
using Oathx.Core;
using UnityEngine;

/// <summary>Provides shared module loading and unloading for product runtime systems.</summary>
public abstract class RuntimeSystem : XEventModule
{
    /// <summary>Activates once and releases partial work if activation fails.</summary>
    /// <returns>Whether the system is active.</returns>
    public sealed override bool Active()
    {
        if (IsActive())
            return true;

        bool succeeded = false;
        try
        {
            succeeded = base.Active();

            return succeeded;
        }
        finally
        {
            if (!succeeded)
                Detive();
        }
    }

    /// <summary>Reports readiness separately from module installation.</summary>
    /// <returns>Whether this module is ready for its dependents.</returns>
    public virtual bool IsReady()
    {
        return IsActive();
    }

    /// <summary>Loads and activates a module through the owning dispatcher.</summary>
    /// <param name="type">Module type to load.</param>
    /// <param name="complete">Called at frame end; does not indicate scene or UI readiness.</param>
    public virtual void LoadSystem(System.Type type, System.Action complete = null)
    {
        XCoroutine.Run(DoLoadAsync(type, complete));
    }

    /// <summary>Unloads a module through the owning dispatcher.</summary>
    /// <param name="type">Module type to unload.</param>
    /// <param name="complete">Called at frame end; does not await asynchronous resource release.</param>
    public void UnloadSystem(System.Type type, System.Action complete)
    {
        XCoroutine.Run(DoUnloadAsync(type, complete));
    }

    /// <summary>Dispatches loading and preserves the template's frame-end callback timing.</summary>
    /// <param name="type">Module type to load.</param>
    /// <param name="complete">Optional frame-end notification.</param>
    /// <returns>The module-loading coroutine.</returns>
    private IEnumerator DoLoadAsync(System.Type type, System.Action complete)
    {
        var module = dispatcher.Load(type);
        module.Active();

        yield return new WaitForEndOfFrame();

        complete?.Invoke();
    }

    /// <summary>Dispatches unloading and preserves the template's frame-end callback timing.</summary>
    /// <param name="type">Module type to unload.</param>
    /// <param name="complete">Optional frame-end notification.</param>
    /// <returns>The module-unloading coroutine.</returns>
    private IEnumerator DoUnloadAsync(System.Type type, System.Action complete)
    {
        dispatcher.Unload(type);

        yield return new WaitForEndOfFrame();

        complete?.Invoke();
    }
}
