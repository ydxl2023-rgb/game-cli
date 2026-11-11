using Oathx.Entity;
using Oathx.HttpProtocol;
using UnityEngine;

/// <summary>Owns login data and local server selection.</summary>
[Initialize]
public partial class LoginEntity : XEntity
{
    /// <summary>Currently selected server identifier.</summary>
    public int selectedServer
    { get; private set; }

    /// <summary>Last server used for a local login.</summary>
    public int recentServer
    { get; private set; }

    /// <summary>
    /// Player identifier.
    /// </summary>
    public int pid
    { get; set; }

    /// <summary>
    /// Login timestamp.
    /// </summary>
    public long loginAt
    { get; set; }

    /// <summary>
    /// Account creation timestamp.
    /// </summary>
    public long tmcreate
    { get; set; }



    /// <summary>Restore local server selection when the entity is created.</summary>
    public LoginEntity()
    {
        selectedServer = PlayerPrefs.GetInt(StorageKey.For<LoginSystem, LoginEntity>(entity => entity.selectedServer), 0);
        recentServer = PlayerPrefs.GetInt(StorageKey.For<LoginSystem, LoginEntity>(entity => entity.recentServer), 0);
    }

    /// <summary>Update and persist the selected server.</summary>
    /// <param name="serverId">Server identifier to select and save.</param>
    /// <returns>True when a positive server identifier has been saved.</returns>
    public bool SelectServer(int serverId)
    {
        if (serverId <= 0)
            return false;

        selectedServer = serverId;

        PlayerPrefs.SetInt(StorageKey.For<LoginSystem, LoginEntity>(entity => entity.selectedServer), selectedServer);
        PlayerPrefs.Save();

        return true;
    }

    /// <summary>Record the selected server after local login succeeds.</summary>
    /// <returns>True when the current selection has been recorded.</returns>
    public bool RecordLoginServer()
    {
        if (selectedServer <= 0)
            return false;

        recentServer = selectedServer;

        PlayerPrefs.SetInt(StorageKey.For<LoginSystem, LoginEntity>(entity => entity.recentServer), recentServer);
        PlayerPrefs.Save();

        return true;
    }

    /// <summary>
    /// query login entity
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public bool Refresh(LoginResponse response)
    {
        pid = response.pid; loginAt = response.loginAt; tmcreate = response.tmcreate;

        return true;
    }
}

/// <summary>Availability state of a login server.</summary>
public enum ServerState
{
    /// <summary>The server is available with normal load.</summary>
    Normal = 0,

    /// <summary>The server is available with high load.</summary>
    Busy = 1,

    /// <summary>The server is unavailable during maintenance.</summary>
    Maintenance = 2
}

/// <summary>
/// Local server information, independent of any transport protocol.
/// </summary>
public sealed class ServerEntry
{
    /// <summary>Stable server identifier.</summary>
    public int id
    { get; set; }

    /// <summary>Display name.</summary>
    public string name
    { get; set; }

    /// <summary>Display state: normal, busy or maintenance.</summary>
    public ServerState state
    { get; set; }

    /// <summary>Whether this server is recommended.</summary>
    public bool recommended
    { get; set; }
}
