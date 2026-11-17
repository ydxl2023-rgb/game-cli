using System;
using System.Collections.Generic;
using System.Globalization;
using Config;
using Oathx.Entity;

/// <summary>Owns function inputs, availability state and condition evaluation.</summary>
[Initialize]
public class FunctionManagerEntity : XEntity
{
    /// <summary>Session overrides for configured switches.</summary>
    private readonly Dictionary<int, bool> switches = new Dictionary<int, bool>();

    /// <summary>Last calculated availability for configured functions.</summary>
    private readonly Dictionary<int, bool> states = new Dictionary<int, bool>();

    /// <summary>Validated player level supplied by the product.</summary>
    public int level
    { get; private set; }

    /// <summary>Read availability using the current product inputs and UTC time.</summary>
    /// <param name="id">FunctionSystem configuration identifier.</param>
    /// <returns>True only when the row exists and every requirement passes.</returns>
    public bool IsOpen(int id)
    {
        if (!Excel.functionSystemConfig.TryGetValue(id, out var row))
            return false;

        if (!string.IsNullOrWhiteSpace(row.ClassType))
        {
            int matches = 0;
            foreach (var candidate in Excel.functionSystemConfig.Values)
            {
                if (candidate.ClassType == row.ClassType)
                    matches++;
            }

            if (matches != 1 || FunctionManagerAccess.ResolveType(row) == null)
                return false;
        }

        return Evaluate(row, DateTimeOffset.UtcNow);
    }

    /// <summary>Apply a validated local switch without bypassing other requirements.</summary>
    /// <param name="id">Configured function identifier.</param>
    /// <param name="enabled">Requested local switch.</param>
    /// <returns>True when the configured function was updated.</returns>
    public bool UpdateSwitch(int id, bool enabled)
    {
        if (!Excel.functionSystemConfig.ContainsKey(id))
            return false;

        switches[id] = enabled;

        return true;
    }

    /// <summary>Apply a nonnegative product level.</summary>
    /// <param name="value">Current player level.</param>
    /// <returns>True when the level was accepted.</returns>
    public bool UpdateLevel(int value)
    {
        if (value < 0)
            return false;

        level = value;

        return true;
    }

    /// <summary>Recalculate all functions and return only changed availability values.</summary>
    /// <returns>A detached transition snapshot for the caller to publish.</returns>
    public Dictionary<int, bool> Refresh()
    {
        var changes = new Dictionary<int, bool>();
        foreach (var row in Excel.functionSystemConfig.Values)
        {
            bool opened = IsOpen(row.ID);
            if (!states.TryGetValue(row.ID, out bool previous) || previous != opened)
            {
                states[row.ID] = opened;
                changes[row.ID] = opened;
            }
        }

        return changes;
    }

    /// <summary>Evaluate AND conditions and an inclusive-start, exclusive-end UTC window.</summary>
    /// <param name="row">The function definition.</param>
    /// <param name="now">UTC evaluation time.</param>
    /// <returns>False for malformed conditions or timestamps as well as closed functions.</returns>
    public bool Evaluate(Config.FunctionSystem row, DateTimeOffset now)
    {
        if (row == null)
            return false;

        bool enabled = true;
        if (row.Condtions != null)
        {
            foreach (var condition in row.Condtions)
            {
                if (string.IsNullOrWhiteSpace(condition))
                    continue;

                string[] parts = condition.Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int value))
                    return false;

                if (parts[0] == "Level")
                {
                    if (level < value)
                        return false;
                }
                else if (parts[0] == "Switch" && (value == 0 || value == 1))
                    enabled &= value == 1;
                else
                    return false;
            }
        }

        if (switches.TryGetValue(row.ID, out bool local))
            enabled = local;

        if (!enabled || !ReadTime(row.StartTime, DateTimeOffset.MinValue, out var start)
            || !ReadTime(row.StopTime, DateTimeOffset.MaxValue, out var stop))
            return false;

        return start < stop && now >= start && now < stop;
    }

    /// <summary>Accept explicit UTC or offset timestamps; empty endpoints are unbounded.</summary>
    /// <param name="value">ISO 8601 timestamp with a timezone.</param>
    /// <param name="fallback">Unbounded endpoint value.</param>
    /// <param name="time">The parsed endpoint.</param>
    /// <returns>True for an empty value or supported timestamp.</returns>
    private static bool ReadTime(string value, DateTimeOffset fallback, out DateTimeOffset time)
    {
        time = fallback;
        if (string.IsNullOrWhiteSpace(value))
            return true;

        string[] formats = { "yyyy-MM-dd'T'HH:mm:ss'Z'", "yyyy-MM-dd'T'HH:mm:sszzz" };

        return DateTimeOffset.TryParseExact(value, formats, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out time);
    }
}
