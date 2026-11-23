using System.Collections;
using UnityEngine;
using Oathx.Core;

/// <summary>Provides deferred runtime system transitions through the owning dispatcher.</summary>
public abstract class RuntimeSystem : XEventModule
{
    /// <summary>Lifecycle generation supplied by coordinators that reject stale work.</summary>
    protected virtual int operationVersion { get { return 0; } }

    /// <summary>Load and activate a runtime system.</summary>
    /// <param name="type">Concrete system type.</param>
    /// <param name="complete">Optional callback after the current frame.</param>
    public virtual void LoadSystem(System.Type type, System.Action complete = null)
    {
        XCoroutine.Run(DoLoadAsync(type, complete, operationVersion));
    }

    /// <summary>Unload a runtime system.</summary>
    /// <param name="type">System type to remove.</param>
    /// <param name="complete">Optional callback after the current frame.</param>
    public void UnloadSystem(System.Type type, System.Action complete)
    {
        XCoroutine.Run(DoUnloadAsync(type, complete, operationVersion));
    }

    /// <summary>Activate the system and complete only within the originating lifetime.</summary>
    /// <param name="type">Concrete system type.</param>
    /// <param name="complete">Optional completion callback.</param>
    /// <param name="version">Originating lifecycle generation.</param>
    /// <returns>The frame-delayed activation sequence.</returns>
    private IEnumerator DoLoadAsync(System.Type type, System.Action complete, int version)
    {
        var module = dispatcher.Load(type);
        if (module != null && !module.IsActive())
            module.Active();

        yield return new WaitForEndOfFrame();

        if (IsActive() && version == operationVersion)
            complete?.Invoke();
    }

    /// <summary>Remove the system and complete only within the originating lifetime.</summary>
    /// <param name="type">System type to remove.</param>
    /// <param name="complete">Optional completion callback.</param>
    /// <param name="version">Originating lifecycle generation.</param>
    /// <returns>The frame-delayed removal sequence.</returns>
    private IEnumerator DoUnloadAsync(System.Type type, System.Action complete, int version)
    {
        dispatcher.Unload(type);

        yield return new WaitForEndOfFrame();

        if (IsActive() && version == operationVersion)
            complete?.Invoke();
    }
}

