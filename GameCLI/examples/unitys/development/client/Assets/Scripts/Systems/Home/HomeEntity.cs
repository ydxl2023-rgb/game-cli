using Oathx.Entity;
using System.Collections.Generic;

/// <summary>Product-owned lobby data; no remote service or sample login is implied.</summary>
[Initialize]
public partial class HomeEntity : XEntity
{
    /// <summary>Current playerName value supplied by the product.</summary>
    public string playerName
    { get; set; }

    /// <summary>Current playerLevel value supplied by the product.</summary>
    public int playerLevel
    { get; set; }

    /// <summary>Current currencyOneAmount value supplied by the product.</summary>
    public long currencyOneAmount
    { get; set; }

    /// <summary>Current currencyTwoAmount value supplied by the product.</summary>
    public long currencyTwoAmount
    { get; set; }

    /// <summary>Current currencyThreeAmount value supplied by the product.</summary>
    public long currencyThreeAmount
    { get; set; }

    /// <summary>Current chatMessage value supplied by the product.</summary>
    public string chatMessage
    { get; set; }

    /// <summary>Current characterKey value supplied by the product.</summary>
    public string characterKey
    { get; set; }

    /// <summary>Current seasonKey value supplied by the product.</summary>
    public string seasonKey
    { get; set; }

    /// <summary>Local reference drop state; the product may replace it.</summary>
    public HomeDropState dropState
    { get; set; } = HomeDropState.Triple;

    /// <summary>Product-owned decoration states keyed by entry identity.</summary>
    private readonly Dictionary<int, ActivityBadge> badges = new Dictionary<int, ActivityBadge> {
        { Config.Excel.SYS_ACTIVITY_FATEBATTLENODE, ActivityBadge.New },
        { (int)HomeEntry.Shop, ActivityBadge.Advertisement },
        { (int)HomeEntry.Recruit, ActivityBadge.New },
        { (int)HomeEntry.Players, ActivityBadge.New },
        { (int)HomeEntry.Club, ActivityBadge.New }
    };

    /// <summary>Read decoration flags without exposing mutable state.</summary>
    /// <param name="entryId">Function or navigation identifier.</param>
    /// <returns>The requested flags, or none when unspecified.</returns>
    public ActivityBadge GetBadges(int entryId)
    {
        return badges.TryGetValue(entryId, out var value) ? value : ActivityBadge.None;
    }

    /// <summary>Update independent decoration states for an entry.</summary>
    /// <param name="entryId">Function or navigation identifier.</param>
    /// <param name="value">Combinable decoration flags.</param>
    /// <returns>True when the input was accepted.</returns>
    public bool SetBadges(int entryId, ActivityBadge value)
    {
        if (entryId < 0 || (value & ~(ActivityBadge.New | ActivityBadge.Advertisement)) != 0)
            return false;

        if (value == ActivityBadge.None)
            badges.Remove(entryId);
        else
            badges[entryId] = value;

        return true;
    }

}

/// <summary>Mutually exclusive reward multiplier presentations.</summary>
public enum HomeDropState
{
    /// <summary>No promotion is displayed.</summary>
    None,

    /// <summary>Display the double drop artwork.</summary>
    Double,

    /// <summary>Display the triple drop artwork.</summary>
    Triple
}
