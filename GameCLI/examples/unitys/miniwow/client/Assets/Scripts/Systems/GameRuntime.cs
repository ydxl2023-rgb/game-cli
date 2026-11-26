using Oathx.Core;
using UnityEngine;

/// <summary>Shared resource lifecycle phases for gameplay runtimes.</summary>
public enum RuntimeState
{
    /// <summary>Loads the runtime's scene and resources.</summary>
    Load,

    /// <summary>Provides initialized resources to the owning system.</summary>
    Ready,

    /// <summary>Releases interactions and asynchronous resources.</summary>
    Unload,

    /// <summary>Owns no resources and can be destroyed.</summary>
    Stopped
}

/// <summary>Registers and advances runtime states using the development FightRuntime inheritance pattern.</summary>
public abstract class GameRuntime : XActionStateMachine
{
    /// <summary>Enters loading after the concrete runtime has validated its inputs.</summary>
    /// <returns>Whether initialization was accepted.</returns>
    public virtual bool Startup()
    {
        SetState(RuntimeState.Load);

        return true;
    }

    /// <summary>Begins cleanup while preserving the component until asynchronous work finishes.</summary>
    public void Shutdown()
    {
        if (IsStopped())
            return;

        SetState(RuntimeState.Unload);
    }

    /// <summary>Reports whether initialization has reached the shared ready phase.</summary>
    /// <returns>Whether the runtime is ready.</returns>
    public bool IsReady()
    {
        return GetCurrentState().state.Equals(RuntimeState.Ready);
    }

    /// <summary>Reports whether RuntimeManager can destroy this component.</summary>
    /// <returns>Whether asynchronous cleanup has completed.</returns>
    public bool IsStopped()
    {
        return GetCurrentState().state.Equals(RuntimeState.Stopped);
    }

    /// <summary>Registers common states; derived runtimes may append their own gameplay states.</summary>
    protected virtual void InitStateMachine()
    {
        AddState(RuntimeState.Load, OnLoadEnter, OnLoadUpdate, OnLoadLeave);
        AddState(RuntimeState.Ready, OnReadyEnter, OnReadyUpdate, OnReadyLeave);
        AddState(RuntimeState.Unload, OnUnloadEnter, OnUnloadUpdate, OnUnloadLeave);
        AddState(RuntimeState.Stopped, null);
    }

    /// <summary>Starts concrete scene and resource loading.</summary>
    protected abstract void OnLoadEnter();

    /// <summary>Advances loading work that is driven by frame updates.</summary>
    /// <param name="deltaTime">Elapsed frame time.</param>
    protected virtual void OnLoadUpdate(float deltaTime) { }

    /// <summary>Releases loading-phase work when the state changes.</summary>
    protected virtual void OnLoadLeave() { }

    /// <summary>Begins ready-state behavior after initialization.</summary>
    protected virtual void OnReadyEnter() { }

    /// <summary>Advances the ready runtime's behavior.</summary>
    /// <param name="deltaTime">Elapsed frame time.</param>
    protected virtual void OnReadyUpdate(float deltaTime) { }

    /// <summary>Ends ready-state behavior when another phase begins.</summary>
    protected virtual void OnReadyLeave() { }

    /// <summary>Starts concrete cleanup and eventually enters the stopped state.</summary>
    protected abstract void OnUnloadEnter();

    /// <summary>Advances cleanup work that is driven by frame updates.</summary>
    /// <param name="deltaTime">Elapsed frame time.</param>
    protected virtual void OnUnloadUpdate(float deltaTime) { }

    /// <summary>Ends the cleanup phase after all owned resources are released.</summary>
    protected virtual void OnUnloadLeave() { }

    /// <summary>Registers states before the manager starts this runtime.</summary>
    private void Awake()
    {
        InitStateMachine();
        SetState(RuntimeState.Stopped);
    }

    /// <summary>Advances the current state's update callback once per frame.</summary>
    private void Update()
    {
        UpdateMachine(Time.deltaTime);
    }
}
