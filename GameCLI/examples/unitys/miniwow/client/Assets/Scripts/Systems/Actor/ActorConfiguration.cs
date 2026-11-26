using UnityEngine;

/// <summary>Validates the generated player configuration before scene installation.</summary>
public static class ActorConfiguration
{
    /// <summary>Rejects incomplete resources and nonfinite movement parameters.</summary>
    /// <param name="value">Generated product configuration.</param>
    /// <returns>Whether the configuration is usable.</returns>
    public static bool Valid(Config.Actor value)
    {
        if (!value || string.IsNullOrWhiteSpace(value.Model))
            return false;

        float[] numbers = {
            value.MoveSpeed,
            value.JoystickRadius,
            value.HorizontalScale,
            value.FollowRate,
            value.ModelScale,
            value.AgentRadius,
            value.AgentHeight,
            value.JoystickSourceWidth
        };

        foreach (float number in numbers)
        {
            if (float.IsNaN(number) || float.IsInfinity(number) || number <= 0f)
                return false;
        }

        var spawn = new Vector3(value.SpawnX, value.SpawnY, value.SpawnZ);

        return !float.IsNaN(spawn.sqrMagnitude) && !float.IsInfinity(spawn.sqrMagnitude) && value.HorizontalScale <= 1f;
    }
}
