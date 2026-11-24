using Oathx.Widget;
using UnityEngine;

/// <summary>Presentation bindings for a configured shop catalog.</summary>
public partial class VMShopProductList : VM
{
    /// <summary>Product cards in catalog order.</summary>
    [Binding(typeof(SyncScrollRect))]
    VList<VMShopProductCell> items;

    /// <summary>Level.</summary>
    [Binding(typeof(SyncText))]
    string level;

    /// <summary>Pass Price.</summary>
    [Binding(typeof(SyncText))]
    string passPrice;

    /// <summary>Selected wallpaper artwork.</summary>
    [Binding(typeof(SyncSprite))]
    Sprite preview;

    /// <summary>Preview Title.</summary>
    [Binding(typeof(SyncText))]
    string previewTitle;

    /// <summary>Preview Price.</summary>
    [Binding(typeof(SyncText))]
    string previewPrice;

    /// <summary>Preview the previous product.</summary>
    [Binding(typeof(SyncClick))]
    ButtonClick previous;

    /// <summary>Preview the next product.</summary>
    [Binding(typeof(SyncClick))]
    ButtonClick next;
}
