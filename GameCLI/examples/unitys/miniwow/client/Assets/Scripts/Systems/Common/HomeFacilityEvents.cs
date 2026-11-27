using Oathx.Core;

/// <summary>Requests the already running Home session when a facility is installed later.</summary>
public sealed class HomeSessionRequested : IEvent { }

/// <summary>Confirms that the existing Home scene and shared economy are available.</summary>
public sealed class HomeSessionAvailable : IEvent { }

/// <summary>Notifies facilities before the Home scene and session references are released.</summary>
public sealed class HomeSessionClosing : IEvent { }

/// <summary>Reports that a facility page and its guide targets are initialized.</summary>
public sealed class HomeFacilityOpened : IEvent
{
    /// <summary>Optional guide whose page request produced this result.</summary>
    public TaskPresentationAction action
    { get; set; }

    /// <summary>Original building served by the initialized page.</summary>
    public int buildingId
    { get; set; }

    /// <summary>Whether the page and guide targets were initialized successfully.</summary>
    public bool succeeded
    { get; set; }
}

/// <summary>Reports completion or interruption of a scene-owned resident route.</summary>
public sealed class HomeResidentNavigationFinished : IEvent
{
    /// <summary>Exact guide presentation that owns this route completion.</summary>
    public TaskPresentationAction action
    { get; set; }

    /// <summary>Original building requested by the route.</summary>
    public int buildingId
    { get; set; }

    /// <summary>Whether the player reached the destination.</summary>
    public bool arrived
    { get; set; }
}

/// <summary>Returns a deferred system activation to the guide owner before opening its page.</summary>
public sealed class HomeResidentSystemReady : IEvent
{
    /// <summary>Exact presentation still entitled to open the destination.</summary>
    public TaskPresentationAction action
    { get; set; }

    /// <summary>Original destination building.</summary>
    public int buildingId
    { get; set; }
}

/// <summary>Reports manual player input superseding an unfinished guide movement.</summary>
public sealed class HomeManualMovementStarted : IEvent { }

/// <summary>Reports whether a facility page currently owns Home input.</summary>
public sealed class HomeFacilityPresentationChanged : IEvent
{
    /// <summary>Configured system owning the page.</summary>
    public int systemId
    { get; set; }

    /// <summary>Whether its page is open.</summary>
    public bool visible
    { get; set; }
}

/// <summary>Requests scene navigation after hospital access was accepted.</summary>
public sealed class HospitalNavigationRequested : IEvent { }

/// <summary>Reports the patient's actual arrival without performing a transaction in the scene.</summary>
public sealed class HospitalPatientArrived : IEvent { }

/// <summary>Requests the scene-owned repair cost presentation.</summary>
public sealed class HospitalRepairPresentationRequested : IEvent
{
    /// <summary>Building to repair.</summary>
    public int buildingId
    { get; set; }

    /// <summary>Whether the current inventory covers construction.</summary>
    public bool affordable
    { get; set; }
}
