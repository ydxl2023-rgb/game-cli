using System.Collections;
using Oathx.Core;
using Oathx.Entity;
using UnityEngine;

/// <summary>Publishes availability changes from product events and time boundaries.</summary>
public class FunctionManagerSystem : RuntimeSystem
{
    /// <summary>Single time-window polling task owned by this service.</summary>
    private Coroutine clock;

    /// <summary>Subscribe to inputs and start time checks once.</summary>
    /// <returns>True after the service is active.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        if (clock != null)
            return true;

        Subscribe<FunctionSwitchChanged>(OnSwitchChanged);
        Subscribe<FunctionLevelChanged>(OnLevelChanged);
        clock = XCoroutine.Run(CheckTime());

        return true;
    }

    /// <summary>Release event subscriptions and the time check.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        if (clock != null)
            XCoroutine.StopCoroutine(clock);

        clock = null;
    }

    /// <summary>Reevaluate table rows and queue only actual transitions.</summary>
    /// <returns>True when product data was available.</returns>
    public bool Refresh()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<FunctionManagerEntity>();
        if (!IsActive() || !entity)
            return false;

        foreach (var change in entity.Refresh())
        {
            XPluginUtility.PostEvent(new FunctionOpenChanged { id = change.Key, opened = change.Value });
        }

        return true;
    }

    /// <summary>Check time windows independently of whether Home is currently visible.</summary>
    /// <returns>The owned unscaled polling sequence.</returns>
    private IEnumerator CheckTime()
    {
        yield return null;

        while (IsActive())
        {
            Refresh();

            yield return new WaitForSecondsRealtime(1f);
        }
    }

    /// <summary>Update the product switch and recalculate availability.</summary>
    /// <param name="data">The local switch message.</param>
    private void OnSwitchChanged(FunctionSwitchChanged data)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<FunctionManagerEntity>();
        if (!entity)
            return;

        if (!entity.UpdateSwitch(data.id, data.enabled))
            return;

        Refresh();
    }

    /// <summary>Update the level input and recalculate availability.</summary>
    /// <param name="data">The product level message.</param>
    private void OnLevelChanged(FunctionLevelChanged data)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<FunctionManagerEntity>();
        if (!entity)
            return;

        if (!entity.UpdateLevel(data.level))
            return;

        Refresh();
        XPluginUtility.PostEvent(new FunctionInputsUpdated());
    }
}
