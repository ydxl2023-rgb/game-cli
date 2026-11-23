using System.Collections.Generic;
using Oathx.Entity;

using UnityEngine;

/// <summary>Owns login data and local server selection.</summary>
[Initialize]
public partial class LoginEntity : XEntity
{
    /// <summary>Owned server definitions, independent of page lifetime.</summary>
    private List<ServerEntry> servers = new List<ServerEntry>();

    /// <summary>Number of configured local servers.</summary>
    public int serverCount
    {
        get { return servers.Count; }
    }

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

    /// <summary>Replace the server catalog after validating every entry.</summary>
    /// <param name="entries">Product-supplied server definitions.</param>
    /// <returns>True when a nonempty catalog was accepted atomically.</returns>
    public bool SetServers(IEnumerable<ServerEntry> entries)
    {
        if (entries == null)
            return false;

        var catalog = new List<ServerEntry>();
        var ids = new HashSet<int>();
        foreach (var entry in entries)
        {
            if (entry == null || entry.id <= 0 || string.IsNullOrWhiteSpace(entry.name)
                || !System.Enum.IsDefined(typeof(ServerState), entry.state) || !ids.Add(entry.id))
                return false;

            catalog.Add(CopyServer(entry));
        }

        if (catalog.Count == 0)
            return false;

        servers = catalog;

        return true;
    }

    /// <summary>Read detached definitions for category presentation.</summary>
    /// <returns>A snapshot that cannot modify the owned catalog.</returns>
    public List<ServerEntry> GetServers()
    {
        return servers.ConvertAll(CopyServer);
    }

    /// <summary>Restore the current selection or select the first configured server.</summary>
    /// <returns>True when a configured selection has been persisted.</returns>
    public bool RestoreSelection()
    {
        var entry = servers.Find(item => item.id == selectedServer);
        if (entry == null && servers.Count > 0)
            entry = servers[0];

        return entry != null && SelectServer(entry.id);
    }

    /// <summary>Read the selected server without exposing its owned definition.</summary>
    /// <returns>A detached definition, or null if selection is unavailable.</returns>
    public ServerEntry GetSelectedServer()
    {
        var entry = servers.Find(item => item.id == selectedServer);

        return entry == null ? null : CopyServer(entry);
    }

    /// <summary>Check the current catalog before accepting local login.</summary>
    /// <returns>True when the selected server is available.</returns>
    public bool CanEnter()
    {
        var entry = servers.Find(item => item.id == selectedServer);

        return entry != null && entry.state != ServerState.Maintenance;
    }

    /// <summary>Update and persist the selected server.</summary>
    /// <param name="serverId">Server identifier to select and save.</param>
    /// <returns>True when a positive server identifier has been saved.</returns>
    public bool SelectServer(int serverId)
    {
        if (serverId <= 0 || !servers.Exists(entry => entry.id == serverId))
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
        if (!CanEnter())
            return false;

        recentServer = selectedServer;

        PlayerPrefs.SetInt(StorageKey.For<LoginSystem, LoginEntity>(entity => entity.recentServer), recentServer);
        PlayerPrefs.Save();

        return true;
    }

    /// <summary>Copy a definition across the entity boundary.</summary>
    /// <param name="entry">Validated server definition.</param>
    /// <returns>A detached server value.</returns>
    private static ServerEntry CopyServer(ServerEntry entry)
    {
        return new ServerEntry {
            id = entry.id,
            name = entry.name,
            state = entry.state,
            recommended = entry.recommended
        };
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
