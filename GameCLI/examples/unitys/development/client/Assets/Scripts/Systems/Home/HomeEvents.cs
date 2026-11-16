using Oathx.Core;

/// <summary>Stable local destinations exposed by the basic lobby.</summary>
public enum HomeEntry
{
    /// <summary>The Profile entry.</summary>
    Profile,

    /// <summary>The Avatar entry.</summary>
    Avatar,

    /// <summary>The Mail entry.</summary>
    Mail,

    /// <summary>The Settings entry.</summary>
    Settings,

    /// <summary>The Notice entry.</summary>
    Notice,

    /// <summary>The Friends entry.</summary>
    Friends,

    /// <summary>The Affinity entry.</summary>
    Affinity,

    /// <summary>The CurrencyOne entry.</summary>
    CurrencyOne,

    /// <summary>The AddCurrencyOne entry.</summary>
    AddCurrencyOne,

    /// <summary>The CurrencyTwo entry.</summary>
    CurrencyTwo,

    /// <summary>The AddCurrencyTwo entry.</summary>
    AddCurrencyTwo,

    /// <summary>The CurrencyThree entry.</summary>
    CurrencyThree,

    /// <summary>The AddCurrencyThree entry.</summary>
    AddCurrencyThree,

    /// <summary>The Shop entry.</summary>
    Shop,

    /// <summary>The Benefits entry.</summary>
    Benefits,

    /// <summary>The Rankings entry.</summary>
    Rankings,

    /// <summary>The Chat entry.</summary>
    Chat,

    /// <summary>The ChatChannel entry.</summary>
    ChatChannel,

    /// <summary>The Backpack entry.</summary>
    Backpack,

    /// <summary>The Collection entry.</summary>
    Collection,

    /// <summary>The Honor entry.</summary>
    Honor,

    /// <summary>The Story entry.</summary>
    Story,

    /// <summary>The Matches entry.</summary>
    Matches,

    /// <summary>The Training entry.</summary>
    Training,

    /// <summary>The Recruit entry.</summary>
    Recruit,

    /// <summary>The Players entry.</summary>
    Players,

    /// <summary>The Team entry.</summary>
    Team,

    /// <summary>The Club entry.</summary>
    Club,

    /// <summary>The Activities entry.</summary>
    Activities,
}

/// <summary>Requests navigation; the product coordinator owns the destination system.</summary>
public class HomeEntryRequested : IEvent
{
    /// <summary>Entry requested by a real lobby button.</summary>
    public HomeEntry entry
    { get; set; }
}

/// <summary>Offers optional presentation keys to a product-specific renderer.</summary>
public class HomePresentationRequested : IEvent
{
    /// <summary>Character asset key; empty means no character presentation.</summary>
    public string characterKey
    { get; set; }

    /// <summary>Season asset key; empty keeps the static reference background.</summary>
    public string seasonKey
    { get; set; }
}
