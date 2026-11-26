using Oathx.Core;

/// <summary>Requests the already running Home session when a facility is installed later.</summary>
public sealed class HomeSessionRequested : IEvent { }

/// <summary>Confirms that the existing Home scene and shared economy are available.</summary>
public sealed class HomeSessionAvailable : IEvent { }

/// <summary>Notifies facilities before the Home scene and session references are released.</summary>
public sealed class HomeSessionClosing : IEvent { }

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
