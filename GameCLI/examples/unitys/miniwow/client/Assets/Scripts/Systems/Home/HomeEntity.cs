using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oathx.Entity;

/// <summary>Fixed shortcut destinations available on the Home page.</summary>
public enum HomeShortcut
{
    /// <summary>Opens the settings destination.</summary>
    Settings,

    /// <summary>Opens the task destination.</summary>
    Task,

    /// <summary>Opens the chat destination.</summary>
    Chat
}

/// <summary>Typed destinations authored in the Home entry configuration.</summary>
[JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
public enum HomeEntryRoute
{
    /// <summary>No destination was accepted.</summary>
    None,

    /// <summary>The Shop destination.</summary>
    Shop,

    /// <summary>The SevenDayLogin destination.</summary>
    SevenDayLogin,

    /// <summary>The OpeningSprint destination.</summary>
    OpeningSprint,

    /// <summary>The FirstPurchase destination.</summary>
    FirstPurchase,

    /// <summary>The GrowthTrial destination.</summary>
    GrowthTrial,

    /// <summary>The Market destination.</summary>
    Market,

    /// <summary>The Task destination.</summary>
    Task,

    /// <summary>The Backpack destination.</summary>
    Backpack,

    /// <summary>The Mail destination.</summary>
    Mail,

    /// <summary>The Rank destination.</summary>
    Rank,

    /// <summary>The Community destination.</summary>
    Community,

    /// <summary>The Notice destination.</summary>
    Notice,

    /// <summary>The Survey destination.</summary>
    Survey,

    /// <summary>The MapPreview destination.</summary>
    MapPreview,

    /// <summary>The Heroes destination.</summary>
    Heroes,

    /// <summary>The Mansion destination.</summary>
    Mansion,

    /// <summary>The PlayMode destination.</summary>
    PlayMode,

    /// <summary>The Guild destination.</summary>
    Guild,

    /// <summary>The World destination.</summary>
    World,

    /// <summary>The Recruit destination.</summary>
    Recruit,
}

/// <summary>Result of checking a Home entry against the current local state.</summary>
public enum HomeEntryResult
{
    /// <summary>The entry does not exist or is hidden.</summary>
    Invalid,

    /// <summary>The local unlock condition has not been met.</summary>
    Locked,

    /// <summary>The entry may dispatch its intention.</summary>
    Allowed
}

/// <summary>Owns Home configuration, entry rules and logical actors.</summary>
[Initialize(InstallType.Startup, true)]
public sealed class HomeEntity : XEntity
{
    /// <summary>Validated product configuration and local presentation state.</summary>
    private HomeSnapshot state;

    /// <summary>Actors keyed by stable instance identity, with no scene object references.</summary>
    private readonly Dictionary<int, ActorEntity> actors = new Dictionary<int, ActorEntity>();

    /// <summary>Number of actors currently owned by this town.</summary>
    public int actorCount
    { get { return actors.Count; } }

    /// <summary>Advances actors whether or not their shapes are visible or enabled.</summary>
    /// <param name="elapsed">Elapsed simulation time from the framework clock.</param>
    public override void Update(float elapsed)
    {
        foreach (ActorEntity actor in actors.Values)
            actor.Update(elapsed);
    }

    /// <summary>Destroys owned actors before releasing framework components.</summary>
    public override void Destroy()
    {
        ClearActors();

        base.Destroy();
    }

    /// <summary>Initializes once from the exported product workbook; invalid data never falls back.</summary>
    /// <param name="json">Exported Home configuration payload.</param>
    /// <returns>Whether configuration has been initialized.</returns>
    public bool Initialize(string json)
    {
        if (state != null)
            return true;

        var candidate = JsonConvert.DeserializeObject<HomeSnapshot>(json);
        if (candidate == null || candidate.schemaVersion != 1 || candidate.profile == null ||
            candidate.entries == null || candidate.texts == null || candidate.entries.Length == 0)
            throw new ArgumentException("Incomplete Home configuration.");

        var ids = new HashSet<int>();
        foreach (var entry in candidate.entries)
        {
            if (!ids.Add(entry.id)
                || entry.route == HomeEntryRoute.None
                || !Enum.IsDefined(typeof(HomeEntryRoute), entry.route)
                || !candidate.texts.ContainsKey(entry.labelId)
                    || (!Enum.TryParse<HomeEntryGroup>(entry.group, out var group)
                    || !Enum.IsDefined(typeof(HomeEntryGroup), group)
                    || entry.group != group.ToString()))
                throw new ArgumentException("Invalid Home entry configuration: " + entry.id);
        }

        if (candidate.profile.gold < 0
            || candidate.profile.gem < 0
            || candidate.profile.energy < 0
            || candidate.profile.energyLimit <= 0)
            throw new ArgumentException("Invalid Home resource values.");

        state = candidate;

        return true;
    }

    /// <summary>Reports whether validated configuration has been installed.</summary>
    /// <returns>Whether valid configuration is installed.</returns>
    public bool IsInitialized()
    {
        return state != null;
    }

    /// <summary>Updates local host-provided entry availability without exposing mutable state.</summary>
    /// <param name="id">Stable entry identifier.</param>
    /// <param name="visible">Whether the entry is displayed.</param>
    /// <param name="unlocked">Whether the entry can be requested.</param>
    /// <returns>Whether the entry exists and was updated.</returns>
    public bool UpdateEntryAvailability(int id, bool visible, bool unlocked)
    {
        var entry = state.entries.FirstOrDefault(item => item.id == id);
        if (entry == null)
            return false;

        entry.visible = visible;
        entry.unlocked = unlocked;

        return true;
    }

    /// <summary>Accepts validated resource values from local gameplay without clamping valid energy overflow.</summary>
    /// <param name="gold">Nonnegative gold balance.</param>
    /// <param name="gem">Nonnegative gem balance.</param>
    /// <param name="energy">Nonnegative available energy.</param>
    /// <param name="energyLimit">Positive base energy capacity.</param>
    /// <returns>Whether all supplied values were accepted.</returns>
    public bool UpdateResources(long gold, long gem, int energy, int energyLimit)
    {
        if (gold < 0 || gem < 0 || energy < 0 || energyLimit <= 0)
            return false;

        state.profile.gold = gold;
        state.profile.gem = gem;
        state.profile.energy = energy;
        state.profile.energyLimit = energyLimit;

        return true;
    }

    /// <summary>Returns an independent snapshot so presentation cannot mutate owned state.</summary>
    /// <returns>A detached copy of the initialized state.</returns>
    public HomeSnapshot Snapshot()
    {
        if (state == null)
            throw new InvalidOperationException("Home configuration is not initialized.");

        return JsonConvert.DeserializeObject<HomeSnapshot>(JsonConvert.SerializeObject(state));
    }

    /// <summary>Rechecks visibility and unlock state at click time.</summary>
    /// <param name="id">Stable entry identifier.</param>
    /// <param name="route">Allowed destination, or None when rejected.</param>
    /// <returns>The validated access result.</returns>
    public HomeEntryResult RequestEntry(int id, out HomeEntryRoute route)
    {
        route = HomeEntryRoute.None;

        var entry = state.entries.FirstOrDefault(item => item.id == id);
        if (entry == null || !entry.visible)
            return HomeEntryResult.Invalid;

        if (!entry.unlocked)
            return HomeEntryResult.Locked;

        route = entry.route;

        return HomeEntryResult.Allowed;
    }

    /// <summary>Atomically installs validated actor data into an empty town.</summary>
    /// <param name="spawns">Detached actor definitions supplied by scene authoring.</param>
    /// <returns>Whether every definition was valid and installed.</returns>
    public bool InitializeActors(ActorSpawn[] spawns)
    {
        if (actors.Count != 0 || spawns == null || spawns.Length == 0)
            return false;

        var candidates = new Dictionary<int, ActorEntity>();
        foreach (ActorSpawn spawn in spawns)
        {
            var actor = new ActorEntity();
            if (!actor.Initialize(spawn) || candidates.ContainsKey(actor.actorId))
            {
                actor.Destroy();
                foreach (ActorEntity candidate in candidates.Values)
                    candidate.Destroy();

                return false;
            }

            candidates.Add(actor.actorId, actor);
        }

        foreach (var candidate in candidates)
            actors.Add(candidate.Key, candidate.Value);

        return true;
    }

    /// <summary>Returns one logical actor without exposing the mutable collection.</summary>
    /// <param name="actorId">Stable actor instance identifier.</param>
    /// <returns>The actor, or null when absent.</returns>
    public ActorEntity FindActor(int actorId)
    {
        return actors.TryGetValue(actorId, out var actor) ? actor : null;
    }

    /// <summary>Releases all owned actor data and invalidates detached references.</summary>
    public void ClearActors()
    {
        foreach (ActorEntity actor in actors.Values)
            actor.Destroy();

        actors.Clear();
    }
}

/// <summary>Explicit layout groups from the product workbook.</summary>
public enum HomeEntryGroup
{
    /// <summary>Activities arranged in a compact grid.</summary>
    Activity,

    /// <summary>Right-hand function rail.</summary>
    Function,

    /// <summary>Top-right external shortcuts.</summary>
    Quick,

    /// <summary>Bottom navigation tabs.</summary>
    Navigation,

    /// <summary>Recruitment shortcut above navigation.</summary>
    Recruit
}

/// <summary>Independent transport snapshot exported from Home.xlsx.</summary>
public sealed class HomeSnapshot
{
    /// <summary>Export schema version.</summary>
    public int schemaVersion
    { get; set; }

    /// <summary>Workbook hash for traceability.</summary>
    public string sourceSha256
    { get; set; }

    /// <summary>Ordered entry definitions.</summary>
    public HomeEntryData[] entries
    { get; set; }

    /// <summary>Local player presentation data.</summary>
    public HomeProfileData profile
    { get; set; }

    /// <summary>Stable localization identifiers.</summary>
    public Dictionary<string, string> texts
    { get; set; }
}

/// <summary>Independent entry data; mutation does not change the entity.</summary>
public sealed class HomeEntryData
{
    /// <summary>Stable product entry identifier.</summary>
    public int id
    { get; set; }

    /// <summary>Declared presentation group.</summary>
    public string group
    { get; set; }

    /// <summary>Localized label identifier.</summary>
    public string labelId
    { get; set; }

    /// <summary>Recovered source artwork name.</summary>
    public string icon
    { get; set; }

    /// <summary>Whether the entry is displayed.</summary>
    public bool visible
    { get; set; }

    /// <summary>Local demonstration unlock state.</summary>
    public bool unlocked
    { get; set; }

    /// <summary>Requested destination contract, not proof of installation.</summary>
    public HomeEntryRoute route
    { get; set; }
}

/// <summary>Local player and resource values authored in the product workbook.</summary>
public sealed class HomeProfileData
{
    /// <summary>Local player display name.</summary>
    public string playerName
    { get; set; }

    /// <summary>Settlement title.</summary>
    public string guildTitle
    { get; set; }

    /// <summary>Displayed combat power.</summary>
    public long power
    { get; set; }

    /// <summary>Gold balance.</summary>
    public long gold
    { get; set; }

    /// <summary>Gem balance.</summary>
    public long gem
    { get; set; }

    /// <summary>Current energy; overflow above capacity is supported.</summary>
    public int energy
    { get; set; }

    /// <summary>Base energy capacity.</summary>
    public int energyLimit
    { get; set; }

    /// <summary>Local task progress summary.</summary>
    public string taskProgress
    { get; set; }

    /// <summary>Local village message.</summary>
    public string chat
    { get; set; }
}
