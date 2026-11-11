using Oathx.Widget;
using UnityEngine;

/// <summary>Presents server selection through bound Fancy lists.</summary>
[Config("Assets/Data/Widgets/ServerSelect/ServerSelect.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UIServerSelect : UIWidget<VMServerSelect>
{
    /// <summary>Server list used to reset the viewport after category changes.</summary>
    [SerializeField] private FancyGridScrollView serverGrid;

    /// <summary>Reset the displayed category to its first row.</summary>
    public void ResetScroll()
    {
        serverGrid.JumpTo(0, Alignment.Upper);
    }
}
