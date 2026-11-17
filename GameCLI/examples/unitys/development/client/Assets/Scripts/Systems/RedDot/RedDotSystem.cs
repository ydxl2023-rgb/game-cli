using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;

/// <summary>Evaluates configured notification conditions and owns widget presentation lifetimes.</summary>
public class RedDotSystem : RuntimeSystem
{
    /// <summary>Validated immutable configuration indexed by stable identifier.</summary>
    private readonly Dictionary<int, RedDot> configurations = new Dictionary<int, RedDot>();

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

        ValidateConfigurations();

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
        configurations.Clear();
    }

    /// <summary>Schedule one condition or all configured conditions for reevaluation.</summary>
    /// <param name="id">A configuration identifier, or zero for all conditions.</param>
    /// <returns>True when the request is valid and scheduled.</returns>
    public bool RequestRefresh(int id = 0)
    {
        if (!IsActive() || (id != 0 && !configurations.ContainsKey(id)))
            return false;

        if (id == 0)
        {
            foreach (var key in configurations.Keys)
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

    /// <summary>Validate IDs, supported conditions, flags and an acyclic parent tree.</summary>
    private void ValidateConfigurations()
    {
        configurations.Clear();
        foreach (var item in Excel.redDotConfig)
        {
            var row = item.Value;
            if (row == null || row.ID <= 0 || row.ID != row.NumID || configurations.ContainsKey(row.ID))
                throw new InvalidOperationException("Invalid or duplicate red dot identifier.");

            if (!Enum.IsDefined(typeof(ERedDotCondition), row.RedDotCondition)
                || (row.RedDotCondition != ERedDotCondition.None && row.RedDotCondition != ERedDotCondition.SignalCount)
                || (row.RedDotDisplay != ERedDotDisplay.Dot && row.RedDotDisplay != ERedDotDisplay.Count)
                || row.Enabled < 0 || row.Enabled > 1 || row.RefreshOnOpen < 0 || row.RefreshOnOpen > 1
                || row.RefreshOnClose < 0 || row.RefreshOnClose > 1)
                throw new InvalidOperationException($"Invalid red dot configuration: {row.ID}.");

            configurations.Add(row.ID, row);
        }

        foreach (var row in configurations.Values)
        {
            var ancestors = new HashSet<int> { row.ID };
            int parent = row.ParentID;
            while (parent != 0)
            {
                if (!configurations.TryGetValue(parent, out var ancestor) || !ancestors.Add(parent))
                    throw new InvalidOperationException($"Missing or cyclic red dot parent: {row.ID}.");

                parent = ancestor.ParentID;
            }
        }
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

            foreach (var row in configurations.Values)
            {
                if (row.RefreshOnOpen != 0)
                    RequestRefresh(row.ID);
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

            foreach (var row in configurations.Values)
            {
                if (row.RefreshOnClose != 0)
                    RequestRefresh(row.ID);
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
        if (IsActive() && data.count >= 0 && configurations.TryGetValue(data.id, out var row)
            && row.RedDotCondition == ERedDotCondition.SignalCount)
        {
            var entity = XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();
            if (entity)
            {
                entity.signals[data.id] = data.count;

                RequestRefresh(data.id);
            }
        }
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

        foreach (int id in pending)
        {
            var configuration = configurations[id];
            entity.evaluated[id] = configuration.Enabled != 0 ? RedDotConditions.Evaluate(configuration, entity) : 0;
        }

        pending.Clear();
        var counts = new Dictionary<int, int>();
        foreach (int id in configurations.Keys)
            Aggregate(id, entity, counts);

        var changes = new List<RedDotChanged>();
        foreach (var pair in counts)
        {
            if (!entity.counts.TryGetValue(pair.Key, out int old) || old != pair.Value)
                changes.Add(new RedDotChanged { id = pair.Key, count = pair.Value });

            entity.counts[pair.Key] = pair.Value;
        }

        scopes.RemoveWhere(scope => !scope);
        foreach (var scope in scopes)
        {
            if (!scope.Render(entity))
                Debug.LogError($"Invalid red dot bindings on {scope.name}.");
        }

        foreach (var change in changes)
            XPluginUtility.FireEvent(change);

        return true;
    }

    /// <summary>Aggregate each configured subtree once, saturating large counts.</summary>
    /// <param name="id">The subtree root.</param>
    /// <param name="entity">Cached condition results.</param>
    /// <param name="counts">Memoized aggregate results for this refresh.</param>
    /// <returns>The count visible at the subtree root.</returns>
    private int Aggregate(int id, RedDotEntity entity, Dictionary<int, int> counts)
    {
        if (counts.TryGetValue(id, out int known))
            return known;

        long total = 0;
        if (configurations[id].Enabled != 0)
        {
            entity.evaluated.TryGetValue(id, out int own);
            total = own;
            foreach (var row in configurations.Values)
            {
                if (row.ParentID == id)
                    total = Math.Min(int.MaxValue, total + Aggregate(row.ID, entity, counts));
            }
        }

        counts[id] = (int)total;

        return (int)total;
    }
}
