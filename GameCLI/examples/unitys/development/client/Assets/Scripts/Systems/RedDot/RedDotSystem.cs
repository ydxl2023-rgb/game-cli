using System.Collections;
using System.Collections.Generic;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;

/// <summary>Evaluates configured notification conditions and owns widget presentation lifetimes.</summary>
public class RedDotSystem : RuntimeSystem
{
    /// <summary>Presentation scopes currently owned by open widgets.</summary>
    private readonly HashSet<RedDotScope> scopes = new HashSet<RedDotScope>();

    /// <summary>Condition identifiers coalesced until the end of the frame.</summary>
    private readonly HashSet<int> pending = new HashSet<int>();

    /// <summary>Scheduled refresh owned exclusively by this system.</summary>
    private Coroutine refresh;

    /// <summary>Whether this instance already owns lifecycle subscriptions.</summary>
    private bool subscribed;

    /// <summary>Validate configuration and subscribe to widget and business events.</summary>
    /// <returns>True when the independent service is ready.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        if (subscribed)
            return true;

        var entity = XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();
        if (!entity)
            return false;

        entity.Initialize();

        Subscribe<WidgetOpenedEvent>(OnWidgetOpened);
        Subscribe<WidgetClosingEvent>(OnWidgetClosing);
        Subscribe<RedDotSignalChanged>(OnSignalChanged);
        Subscribe<RedDotRefreshRequested>(OnRefreshRequested);
        Subscribe<RedDotScopeEnabled>(OnScopeEnabled);
        Subscribe<RedDotScopeDisabled>(OnScopeDisabled);

        subscribed = true;

        foreach (var scope in RedDotScope.Snapshot())
        {
            if (scope)
                scopes.Add(scope);
        }

        RequestRefresh();

        return true;
    }

    /// <summary>Cancel pending work and release visuals while retaining entity state.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        subscribed = false;

        if (refresh != null)
            XCoroutine.StopCoroutine(refresh);

        refresh = null;
        pending.Clear();
        foreach (var scope in scopes)
        {
            if (scope)
                scope.Render(null);
        }

        scopes.Clear();
    }

    /// <summary>Schedule one condition or all configured conditions for reevaluation.</summary>
    /// <param name="id">A configuration identifier, or zero for all conditions.</param>
    /// <returns>True when the request is valid and scheduled.</returns>
    public bool RequestRefresh(int id = 0)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();
        if (!entity)
            return false;

        if (!IsActive() || (id != 0 && !entity.Contains(id)))
            return false;

        if (id == 0)
        {
            foreach (var key in entity.GetIds())
                pending.Add(key);
        }
        else
        {
            pending.Add(id);
        }

        if (refresh == null)
            refresh = XCoroutine.Run(RefreshAtFrameEnd());

        return true;
    }

    /// <summary>Attach only the explicit scope on the widget root after activation.</summary>
    /// <param name="data">The application widget-opened event.</param>
    private void OnWidgetOpened(WidgetOpenedEvent data)
    {
        var widget = data.data.widget;
        if (IsActive() && widget != null && widget.gameObject)
        {
            var scope = widget.gameObject.GetComponent<RedDotScope>();
            if (scope)
                scopes.Add(scope);

            var entity = XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();
            if (entity)
            {
                foreach (int id in entity.GetLifecycleIds(true))
                    RequestRefresh(id);
            }
        }
    }

    /// <summary>Release closing visuals and reevaluate conditions affected by widget closure.</summary>
    /// <param name="data">The application widget-closing event.</param>
    private void OnWidgetClosing(WidgetClosingEvent data)
    {
        var widget = data.data.widget;
        if (IsActive() && widget != null && widget.gameObject)
        {
            var scope = widget.gameObject.GetComponent<RedDotScope>();
            if (scope)
                scopes.Remove(scope);

            var entity = XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();
            if (entity)
            {
                foreach (int id in entity.GetLifecycleIds(false))
                    RequestRefresh(id);
            }
        }
    }

    /// <summary>Restore cached state without bypassing widget refresh configuration.</summary>
    /// <param name="data">The scope becoming available.</param>
    private void OnScopeEnabled(RedDotScopeEnabled data)
    {
        if (IsActive() && data.scope)
        {
            scopes.Add(data.scope);

            var entity = XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();
            if (entity)
                data.scope.Render(entity);
        }
    }

    /// <summary>Release a disabled or destroyed scope without modifying condition inputs.</summary>
    /// <param name="data">The scope being released.</param>
    private void OnScopeDisabled(RedDotScopeDisabled data)
    {
        scopes.Remove(data.scope);
    }

    /// <summary>Accept only valid counts for configurations driven by local business messages.</summary>
    /// <param name="data">Business-owned condition identifier and count.</param>
    private void OnSignalChanged(RedDotSignalChanged data)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();
        if (!entity)
            return;

        if (IsActive() && entity.UpdateSignal(data.id, data.count))
            RequestRefresh(data.id);
    }

    /// <summary>Schedule explicit business-driven condition checks.</summary>
    /// <param name="data">The requested configuration or all configurations.</param>
    private void OnRefreshRequested(RedDotRefreshRequested data)
    {
        RequestRefresh(data.id);
    }

    /// <summary>Coalesce repeated events and render after widget initialization callbacks.</summary>
    /// <returns>The frame-end refresh sequence.</returns>
    private IEnumerator RefreshAtFrameEnd()
    {
        yield return new WaitForEndOfFrame();

        refresh = null;
        if (IsActive())
            Recalculate();
    }

    /// <summary>Evaluate requested conditions and propagate their values through the parent tree.</summary>
    /// <returns>True when current entity state and visible scopes were refreshed.</returns>
    private bool Recalculate()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();
        if (!entity)
            return false;

        var changes = entity.Refresh(pending);
        pending.Clear();

        scopes.RemoveWhere(scope => !scope);
        foreach (var scope in scopes)
        {
            if (!scope.Render(entity))
                Debug.LogError($"Invalid red dot bindings on {scope.name}.");
        }

        foreach (var change in changes)
            XPluginUtility.FireEvent(new RedDotChanged { id = change.Key, count = change.Value });

        return true;
    }

}
