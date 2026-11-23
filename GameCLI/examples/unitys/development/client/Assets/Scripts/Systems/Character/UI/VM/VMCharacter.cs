
using Oathx.Widget;

public enum EquipPartType
{
    None = 0,
    Belt = 1,
    Cloth = 2,
    Crown = 3,
    Shoulder = 4,
    Glove = 5,
    Ring = 6,
    Bracelet = 7,
    Hat = 8,
    Ornaments = 9,
    Shoe = 10,
    Weapon = 11,
    Face = 12,
    Assistant = 13,
}

/// <summary>
/// character view model
/// </summary>
public partial class VMCharacter : VM
{
    /// <summary>
    /// scroll to index
    /// </summary>
    [Binding]
    int scrollTo;

    /// <summary>
    /// button click event of tidy
    /// </summary>
    [Binding]
    ButtonClick tidy;

    /// <summary>
    /// resolve button click event
    /// </summary>
    [Binding]
    ButtonClick resolve;

    /// <summary>
    /// close button click event
    /// </summary>
    [Binding]
    ButtonClick close;

    /// <summary>
    /// part slots
    /// </summary>
    [Binding]
    VList<VMItemSlot> parts;

    /// <summary>
    /// hero slots
    /// </summary>
    [Binding]
    VList<VMHeroSlot> heros;

    /// <summary>
    /// pack slots
    /// </summary>
    [Binding]
    VList<VMPackSlot> packs;

    /// <summary>
    /// item slots
    /// </summary>
    [Binding]
    VList<VMItemSlot> items;
}

