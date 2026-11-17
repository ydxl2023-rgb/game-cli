using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays the lobby and its independent promotion states.</summary>
[Config("Assets/Data/Widgets/Home/Home.prefab", WidgetLayer.center, cacheWidget: false)]
public class UIHome : UIWidget<VMHome>
{
    /// <summary>Activity list bound through VMHome.</summary>
    [SerializeField]
    private UIActivityEntry[] activities;

    /// <summary>Explicit serialized state targets; no runtime hierarchy search.</summary>
    [SerializeField]
    private HomeVisibility[] indicators;

    /// <summary>The single image shared by both drop promotions.</summary>
    [SerializeField]
    private Image dropImage;

    /// <summary>Original double drop artwork.</summary>
    [SerializeField]
    private Sprite doubleDrop;

    /// <summary>Original triple drop artwork.</summary>
    [SerializeField]
    private Sprite tripleDrop;

    /// <summary>Read identifiers in the same order as the authored SyncGroup children.</summary>
    /// <returns>A detached ordered identifier list.</returns>
    public int[] GetActivityIds()
    {
        var ids = new int[activities.Length];
        for (int index = 0; index < activities.Length; index++)
            ids[index] = activities[index].GetSystemId();

        return ids;
    }

    /// <summary>Refresh all independent indicators from entity data.</summary>
    /// <param name="entity">The current product or local example state.</param>
    /// <returns>True when all required references were updated.</returns>
    public bool RefreshPresentation(HomeEntity entity)
    {
        if (!entity || indicators == null || !dropImage || !doubleDrop || !tripleDrop)
            return false;

        bool complete = true;
        foreach (var indicator in indicators)
            complete &= indicator != null && indicator.Apply(entity);

        foreach (var activity in activities)
            complete &= activity && activity.RefreshPresentation();

        dropImage.sprite = entity.dropState == HomeDropState.Triple ? tripleDrop : doubleDrop;
        dropImage.SetNativeSize();
        dropImage.gameObject.SetActive(entity.dropState == HomeDropState.Double || entity.dropState == HomeDropState.Triple);

        return complete;
    }
}
