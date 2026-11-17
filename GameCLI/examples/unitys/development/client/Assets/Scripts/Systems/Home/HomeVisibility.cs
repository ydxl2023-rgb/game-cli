using System;
using UnityEngine;

/// <summary>Serialized connection between entity state and one lobby element.</summary>
[Serializable]
public class HomeVisibility
{
    /// <summary>Stable entity state key.</summary>
    [SerializeField]
    private HomeIndicator indicator;

    /// <summary>Element controlled by the key.</summary>
    [SerializeField]
    private GameObject target;

    /// <summary>Apply one explicit entity visibility state.</summary>
    /// <param name="entity">The current lobby presentation data.</param>
    /// <returns>True when the serialized target was updated.</returns>
    public bool Apply(HomeEntity entity)
    {
        if (!entity || !target)
            return false;

        target.SetActive(entity.IsIndicatorVisible(indicator));

        return true;
    }
}
