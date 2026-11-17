using Oathx.Core;

/// <summary>Publishes a business condition count without referencing a widget.</summary>
public class RedDotSignalChanged : IEvent
{
    /// <summary>Stable identifier generated from propRedDot.</summary>
    public int id
    { get; set; }

    /// <summary>Nonnegative business count; zero clears the condition.</summary>
    public int count
    { get; set; }
}

/// <summary>Requests reevaluation after external entity data changes.</summary>
public class RedDotRefreshRequested : IEvent
{
    /// <summary>Configured identifier, or zero to refresh all conditions.</summary>
    public int id
    { get; set; }
}

/// <summary>Reports a calculated count only when its value changes.</summary>
public class RedDotChanged : IEvent
{
    /// <summary>Changed configuration identifier.</summary>
    public int id
    { get; set; }

    /// <summary>Calculated count including descendants.</summary>
    public int count
    { get; set; }
}

/// <summary>Registers an explicitly bound presentation scope with the active service.</summary>
public class RedDotScopeEnabled : IEvent
{
    /// <summary>The enabled scope; contains no business condition data.</summary>
    public RedDotScope scope
    { get; set; }
}

/// <summary>Releases presentation references when a scope is disabled.</summary>
public class RedDotScopeDisabled : IEvent
{
    /// <summary>The disabled scope.</summary>
    public RedDotScope scope
    { get; set; }
}
