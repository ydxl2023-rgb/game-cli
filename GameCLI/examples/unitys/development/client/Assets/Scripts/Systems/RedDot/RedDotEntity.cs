using System;
using System.Collections.Generic;
using Config;
using Oathx.Entity;

/// <summary>Owns notification inputs, configuration validation and aggregated counts.</summary>
[Initialize]
public partial class RedDotEntity : XEntity
{
    /// <summary>Validated configurations indexed by stable identifier.</summary>
    private readonly Dictionary<int, RedDot> configurations = new Dictionary<int, RedDot>();

    /// <summary>Business inputs retained independently of widget lifetime.</summary>
    private readonly Dictionary<int, int> signals = new Dictionary<int, int>();

    /// <summary>Cached own-condition results before parent aggregation.</summary>
    private readonly Dictionary<int, int> evaluated = new Dictionary<int, int>();

    /// <summary>Last calculated counts including child contributions.</summary>
    private readonly Dictionary<int, int> counts = new Dictionary<int, int>();

    /// <summary>Validate IDs, supported conditions, flags and an acyclic parent tree.</summary>
    public void Initialize()
    {
        var validated = new Dictionary<int, RedDot>();
        foreach (var item in Excel.redDotConfig)
        {
            var row = item.Value;
            if (row == null || row.ID <= 0 || row.ID != row.NumID || validated.ContainsKey(row.ID))
                throw new InvalidOperationException("Invalid or duplicate red dot identifier.");

            if (!Enum.IsDefined(typeof(ERedDotCondition), row.RedDotCondition)
                || (row.RedDotCondition != ERedDotCondition.None && row.RedDotCondition != ERedDotCondition.SignalCount)
                || (row.RedDotDisplay != ERedDotDisplay.Dot && row.RedDotDisplay != ERedDotDisplay.Count)
                || row.Enabled < 0 || row.Enabled > 1 || row.RefreshOnOpen < 0 || row.RefreshOnOpen > 1
                || row.RefreshOnClose < 0 || row.RefreshOnClose > 1)
                throw new InvalidOperationException($"Invalid red dot configuration: {row.ID}.");

            validated.Add(row.ID, row);
        }

        foreach (var row in validated.Values)
        {
            var ancestors = new HashSet<int> { row.ID };
            int parent = row.ParentID;
            while (parent != 0)
            {
                if (!validated.TryGetValue(parent, out var ancestor) || !ancestors.Add(parent))
                    throw new InvalidOperationException($"Missing or cyclic red dot parent: {row.ID}.");

                parent = ancestor.ParentID;
            }
        }

        configurations.Clear();
        foreach (var item in validated)
            configurations.Add(item.Key, item.Value);
    }

    /// <summary>Read configured identifiers without exposing the owned collection.</summary>
    /// <returns>A detached configuration identifier snapshot.</returns>
    public List<int> GetIds()
    {
        return new List<int>(configurations.Keys);
    }

    /// <summary>Check whether a configured condition can be refreshed.</summary>
    /// <param name="id">Configuration identifier.</param>
    /// <returns>True when the configuration exists.</returns>
    public bool Contains(int id)
    {
        return configurations.ContainsKey(id);
    }

    /// <summary>Select conditions affected by a widget lifecycle notification.</summary>
    /// <param name="opened">True for opening, false for closing.</param>
    /// <returns>A detached list of affected identifiers.</returns>
    public List<int> GetLifecycleIds(bool opened)
    {
        var ids = new List<int>();
        foreach (var row in configurations.Values)
        {
            if (opened ? row.RefreshOnOpen != 0 : row.RefreshOnClose != 0)
                ids.Add(row.ID);
        }

        return ids;
    }

    /// <summary>Validate and store a business signal for the next refresh.</summary>
    /// <param name="id">Signal-driven configuration identifier.</param>
    /// <param name="count">Nonnegative notification count.</param>
    /// <returns>True when the input was accepted.</returns>
    public bool UpdateSignal(int id, int count)
    {
        if (count < 0 || !configurations.TryGetValue(id, out var row)
            || row.RedDotCondition != ERedDotCondition.SignalCount)
            return false;

        signals[id] = count;

        return true;
    }

    /// <summary>Read the calculated count without exposing mutable state.</summary>
    /// <param name="id">Configuration identifier.</param>
    /// <returns>The last calculated count, or zero when unavailable.</returns>
    public int GetCount(int id)
    {
        return counts.TryGetValue(id, out int count) ? count : 0;
    }

    /// <summary>Read a detached snapshot of business signal inputs.</summary>
    /// <returns>Current inputs, safe for external inspection.</returns>
    public Dictionary<int, int> GetSignals()
    {
        return new Dictionary<int, int>(signals);
    }

    /// <summary>Read a detached snapshot of calculated counts.</summary>
    /// <returns>Current counts, safe for external inspection.</returns>
    public Dictionary<int, int> GetCounts()
    {
        return new Dictionary<int, int>(counts);
    }

    /// <summary>Evaluate requested conditions and update the complete parent tree.</summary>
    /// <param name="ids">Condition identifiers scheduled by the caller.</param>
    /// <returns>A detached snapshot containing only count changes.</returns>
    public Dictionary<int, int> Refresh(IEnumerable<int> ids)
    {
        if (ids != null)
        {
            foreach (int id in ids)
            {
                if (configurations.TryGetValue(id, out var row))
                    evaluated[id] = row.Enabled != 0 ? Evaluate(row) : 0;
            }
        }

        var totals = new Dictionary<int, int>();
        foreach (int id in configurations.Keys)
            Aggregate(id, totals);

        var changes = new Dictionary<int, int>();
        foreach (var pair in totals)
        {
            if (!counts.TryGetValue(pair.Key, out int previous) || previous != pair.Value)
                changes[pair.Key] = pair.Value;

            counts[pair.Key] = pair.Value;
        }

        return changes;
    }

    /// <summary>Read one configured condition without changing business data.</summary>
    /// <param name="row">Validated condition configuration.</param>
    /// <returns>The count contributed by this condition.</returns>
    private int Evaluate(RedDot row)
    {
        switch (row.RedDotCondition)
        {
            case ERedDotCondition.None:
                return 0;
            case ERedDotCondition.SignalCount:
                return signals.TryGetValue(row.ID, out int count) ? count : 0;
            default:
                throw new InvalidOperationException($"Unsupported red dot condition: {row.RedDotCondition}.");
        }
    }

    /// <summary>Aggregate each configured subtree once, saturating large counts.</summary>
    /// <param name="id">The subtree root.</param>
    /// <param name="totals">Memoized aggregate results for this refresh.</param>
    /// <returns>The count visible at the subtree root.</returns>
    private int Aggregate(int id, Dictionary<int, int> totals)
    {
        if (totals.TryGetValue(id, out int known))
            return known;

        long total = 0;
        if (configurations[id].Enabled != 0)
        {
            evaluated.TryGetValue(id, out int own);
            total = own;
            foreach (var row in configurations.Values)
            {
                if (row.ParentID == id)
                    total = Math.Min(int.MaxValue, total + Aggregate(row.ID, totals));
            }
        }

        totals[id] = (int)total;

        return (int)total;
    }
}
