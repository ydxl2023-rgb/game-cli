using System.Collections;
using Oathx.Core;
using UnityEngine;

/// <summary>Provides shared module loading and unloading for product runtime systems.</summary>
public abstract class RuntimeSystem : XEventModule
{
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
        if (module != null && !module.IsActive())
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
