using Oathx.Widget;
using UnityEngine;

/// <summary>Currency presentation and callbacks without wallet or navigation ownership.</summary>
public partial class VMCurrencyEntry : VM
{
    /// <summary>Product-owned identifier, independent of Home destinations.</summary>
    public int currencyId
    { get; set; }

    /// <summary>Amount formatted by the host.</summary>
    [Binding(typeof(SyncText))]
    string amount;

    /// <summary>Optional replacement for the authored icon.</summary>
    [Binding(typeof(SyncSprite))]
    Sprite icon;

    /// <summary>Whether the slot is displayed.</summary>
    [Binding(typeof(SyncVisible))]
    bool visible;

    /// <summary>Whether the add button is displayed.</summary>
    [Binding(typeof(SyncVisible))]
    bool canAdd;

    /// <summary>Whether both currency interactions are enabled.</summary>
    [Binding(typeof(SyncInteractable))]
    bool interactable;

    /// <summary>Request currency details from the host.</summary>
    [Binding]
    ButtonClick openClick;

    /// <summary>Request acquisition from the host.</summary>
    [Binding]
    ButtonClick addClick;
}
