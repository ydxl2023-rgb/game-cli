using Oathx.Core;

/// <summary>Requests movement while the requesting module retains its own interaction.</summary>
public sealed class HomeResidentNavigationRequested : IEvent
{
    /// <summary>Original destination identity.</summary>
    public int buildingId
    { get; set; }

    /// <summary>Presentation owning the route result.</summary>
    public TaskPresentationAction action
    { get; set; }
}

/// <summary>Cancels the outstanding external resident route.</summary>
public sealed class HomeResidentNavigationCancelled : IEvent
{
    /// <summary>Exact presentation owner whose route may be released.</summary>
    public TaskPresentationAction action
    { get; set; }
}
