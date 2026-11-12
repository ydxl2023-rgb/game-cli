using System.Collections;
using Oathx.Core;

/// <summary>Provides deferred lifecycle operations for product systems.</summary>
public abstract class RuntimeSystem : XEventModule
{
    /// <summary>Schedule activation after the current event dispatch.</summary>
    /// <param name="type">System type to activate.</param>
    /// <param name="complete">Optional lifecycle completion callback, not UI readiness.</param>
    public void LoadSystem(System.Type type, System.Action complete = null)
    {
        XCoroutine.Run(ChangeSystem(type, true, complete));
    }

    /// <summary>Schedule deactivation after the current event dispatch.</summary>
    /// <param name="type">System type to deactivate.</param>
    /// <param name="complete">Optional lifecycle completion callback.</param>
    public void UnloadSystem(System.Type type, System.Action complete = null)
    {
        XCoroutine.Run(ChangeSystem(type, false, complete));
    }

    /// <summary>Apply a lifecycle request outside dispatcher enumeration.</summary>
    /// <param name="type">Target system type.</param>
    /// <param name="activate">Whether to activate the target.</param>
    /// <param name="complete">Optional lifecycle completion callback.</param>
    /// <returns>The deferred lifecycle coroutine.</returns>
    private IEnumerator ChangeSystem(System.Type type, bool activate, System.Action complete)
    {
        yield return null;
        if (IsActive() && type != null && dispatcher != null)
        {
            if (activate)
            {
                dispatcher.Load(type);
            }
            else
            {
                dispatcher.Unload(type);
            }

            complete?.Invoke();
        }
    }
}
