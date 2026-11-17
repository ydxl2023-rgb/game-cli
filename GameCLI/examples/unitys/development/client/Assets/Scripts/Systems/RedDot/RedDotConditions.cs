using System;
using Config;

/// <summary>Read-only condition evaluation selected by the configuration enum.</summary>
public static class RedDotConditions
{
    /// <summary>Evaluate one condition without network access or business mutations.</summary>
    /// <param name="configuration">Validated condition configuration.</param>
    /// <param name="entity">Current local signal data.</param>
    /// <returns>The nonnegative count contributed by this condition.</returns>
    public static int Evaluate(RedDot configuration, RedDotEntity entity)
    {
        switch (configuration.RedDotCondition)
        {
            case ERedDotCondition.None:
                return 0;
            case ERedDotCondition.SignalCount:
                return entity.signals.TryGetValue(configuration.ID, out int count) ? Math.Max(0, count) : 0;
            default:
                throw new InvalidOperationException($"Unsupported red dot condition: {configuration.RedDotCondition}.");
        }
    }
}
