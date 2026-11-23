using System.Collections.Generic;
using Oathx.Core;

/// <summary>Local preference snapshot for product audio, rendering and gameplay adapters.</summary>
public class SettingsChanged : IEvent
{
    /// <summary>Independent read-only values; does not imply any remote service acknowledged them.</summary>
    public IReadOnlyDictionary<SettingsOption, float> values
    { get; set; }
}

/// <summary>Request a product-owned service without simulating its result.</summary>
public class SettingsActionRequested : IEvent
{
    /// <summary>Original settings entry selected by the user.</summary>
    public SettingsAction action
    { get; set; }
}
