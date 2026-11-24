using Oathx.Core;
using Oathx.Widget;

/// <summary>Shared application event contract for widget intentions.</summary>
public abstract class UIRequestEvent : IEvent, IUIRequest
{
    /// <summary>Originating widget registration.</summary>
    public string sourceKey
    { get; set; }
}
