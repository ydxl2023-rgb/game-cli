using Oathx.Core;

/// <summary>Requests the inn page after its independent system has activated.</summary>
public sealed class InnOpenRequested : IEvent
{
    /// <summary>Optional task presentation owning the request; absent for manual entrance clicks.</summary>
    public TaskPresentationAction guide
    { get; set; }
}
