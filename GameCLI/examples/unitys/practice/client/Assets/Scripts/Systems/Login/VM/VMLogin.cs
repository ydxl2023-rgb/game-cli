using Oathx.Widget;

/// <summary>
/// Generated bindings for the SV login page.
/// </summary>
public partial class VMLogin : VM
{
    /// <summary>
    /// Manual announcement button callback.
    /// </summary>
    [Binding]
    ButtonClick announcementClick;

    /// <summary>
    /// Login button callback.
    /// </summary>
    [Binding]
    ButtonClick enterClick;

    /// <summary>
    /// Whether another login request can be started.
    /// </summary>
    [Binding(typeof(SyncInteractable))]
    bool canEnter;

    /// <summary>
    /// Selected authentication or game server name.
    /// </summary>
    [Binding(typeof(SyncText))]
    string serverName;

    /// <summary>
    /// Current login status or retryable failure message.
    /// </summary>
    [Binding(typeof(SyncText))]
    string serverStatus;

    /// <summary>
    /// Current client resource version.
    /// </summary>
    [Binding(typeof(SyncText))]
    string versionText;

    /// <summary>
    /// Player identifier obtained from LoginEntity.
    /// </summary>
    [Binding(typeof(SyncText))]
    string username;

    /// <summary>
    /// Friend suggestions remain hidden until their service is connected.
    /// </summary>
    [Binding(typeof(SyncVisible))]
    bool friends;

    /// <summary>Open the server selection dialog.</summary>
    [Binding]
    ButtonClick changeClick;
}
