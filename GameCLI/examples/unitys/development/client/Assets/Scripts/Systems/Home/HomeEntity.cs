using Oathx.Entity;

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
}
