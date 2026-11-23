using Newtonsoft.Json;
using Oathx.Entity;

/// <summary>
/// item entity, responsible for representing an item in the game, including its properties and behaviors.
/// </summary>
public class ItemEntity : XEntity
{
    /// <summary>
    /// item property id, used to identify the type of item, 
    /// such as a sword, a potion, or a piece of armor.
    /// </summary>
    [JsonProperty]
    public int propId
    { get; private set; }

    /// <summary>
    /// iten entity constructor, initializes the item with a specific property id,
    /// which can be used to determine the item's attributes and effects in the game.
    /// </summary>
    /// <param name="itemId"></param>
    public ItemEntity(int itemId)
    {
        propId = itemId;
    }
}
