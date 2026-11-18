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

    /// <summary>The single image shared by both drop promotions.</summary>
    [SerializeField]
    private Image dropImage;

    /// <summary>Original double drop artwork.</summary>
    [SerializeField]
    private Sprite doubleDrop;

    /// <summary>Original triple drop artwork.</summary>
    [SerializeField]
    private Sprite tripleDrop;

    /// <summary>Release the nested currency bar before the page binding ends.</summary>
    /// <param name="refVM">The page presentation being released.</param>
    public override void Unref(VM refVM)
    {
        if (vm)
        {
            vm.Profile = null;
            vm.CurrencyBar = null;
            vm.ProfileShortcuts = null;
            base.Unref(refVM);
        }
    }

    /// <summary>Read identifiers in the same order as the authored SyncGroup children.</summary>
    /// <returns>A detached ordered identifier list.</returns>
    public int[] GetActivityIds()
    {
        var ids = new int[activities.Length];
        for (int index = 0; index < activities.Length; index++)
            ids[index] = activities[index].GetSystemId();

        return ids;
    }

    /// <summary>Read authored promotion artwork and its native display size.</summary>
    /// <param name="triple">Whether to select the triple-drop artwork.</param>
    /// <param name="sprite">The selected authored sprite.</param>
    /// <param name="size">Native size in the target canvas units.</param>
    /// <returns>True when the authored presentation is available.</returns>
    public bool GetDropPresentation(bool triple, out Sprite sprite, out Vector2 size)
    {
        sprite = triple ? tripleDrop : doubleDrop;
        size = Vector2.zero;

        if (!dropImage || !sprite)
            return false;

        float referencePixels = dropImage.canvas ? dropImage.canvas.referencePixelsPerUnit : 100f;
        size = sprite.rect.size * referencePixels / sprite.pixelsPerUnit;

        return true;
    }
}
