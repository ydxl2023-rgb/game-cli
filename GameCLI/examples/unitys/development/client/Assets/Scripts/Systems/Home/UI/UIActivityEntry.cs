using Oathx.Widget;
using UnityEngine;

/// <summary>Reusable activity item preserving its source artwork and local animations.</summary>
[RequireComponent(typeof(Metadata))]
public class UIActivityEntry : UIWidget<VMActivityEntry>
{
    /// <summary>FunctionSystem identifier for this authored item.</summary>
    [SerializeField]
    private int id;

    /// <summary>Optional presentation states inside this item; red dots have their own scope.</summary>
    [SerializeField]
    private HomeVisibility[] indicators;

    /// <summary>Read the activity configuration identifier.</summary>
    /// <returns>The associated FunctionSystem identifier.</returns>
    public int GetSystemId()
    {
        return id;
    }

    /// <summary>Refresh optional NEW and other local presentation states.</summary>
    /// <returns>True when entity data is available.</returns>
    public bool RefreshPresentation()
    {
        var entity = Oathx.Entity.XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!entity)
            return false;

        foreach (var indicator in indicators)
            indicator.Apply(entity);

        return true;
    }
}
