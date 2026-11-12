using Oathx.Widget;

/// <summary>
/// Generated bindings for the SV login page.
/// </summary>
public partial class VMLogin : VM
{
    /// <summary>
    /// Login button callback.
    /// </summary>
    [Binding]
    private ButtonClick enterClick;

    /// <summary>
    /// Whether another login request can be started.
    /// </summary>
    [Binding(typeof(SyncInteractable))]
    private bool canEnter;

    /// <summary>
    /// Selected authentication or game server name.
    /// </summary>
    [Binding(typeof(SyncText))]
    private string serverName;

    /// <summary>
    /// Current login status or retryable failure message.
    /// </summary>
    [Binding(typeof(SyncText))]
    private string serverStatus;

    /// <summary>
    /// Current client resource version.
    /// </summary>
    [Binding(typeof(SyncText))]
    private string versionText;

    /// <summary>
    /// Player identifier obtained from LoginEntity.
    /// </summary>
    [Binding(typeof(SyncText))]
    private string username;

    /// <summary>
    /// Friend suggestions remain hidden until their service is connected.
    /// </summary>
    [Binding(typeof(SyncVisible))]
    private bool friends;

    /// <summary>Request the announcementClick entry.</summary>
    [Binding]
    private ButtonClick announcementClick;

    /// <summary>Request the changeClick entry.</summary>
    [Binding]
    private ButtonClick changeClick;

    /// <summary>Request the friendClick entry.</summary>
    [Binding]
    private ButtonClick friendClick;

    /// <summary>Request the userCenterClick entry.</summary>
    [Binding]
    private ButtonClick userCenterClick;

    /// <summary>Request the cancellationClick entry.</summary>
    [Binding]
    private ButtonClick cancellationClick;

    /// <summary>Request the clearClick entry.</summary>
    [Binding]
    private ButtonClick clearClick;

    /// <summary>Request the legalClick entry.</summary>
    [Binding]
    private ButtonClick legalClick;

    /// <summary>Request the noticeClick entry.</summary>
    [Binding]
    private ButtonClick noticeClick;

    /// <summary>Request the ageIconBtnClick entry.</summary>
    [Binding]
    private ButtonClick ageIconBtnClick;

    /// <summary>Request the exitClick entry.</summary>
    [Binding]
    private ButtonClick exitClick;
}
