using Oathx.Core;
using Oathx.Widget;
using UnityEngine;

/// <summary>Business request sent by a widget to its owning system.</summary>
public abstract class UIRequestEvent : IEvent, IUIRequest
{
    /// <summary>UISystem key of the widget that issued the request.</summary>
    public string sourceKey
    { get; set; }

    /// <summary>Local Unity frame captured when the sender created the request.</summary>
    public int createdFrame
    { get; set; } = Time.frameCount;
}
