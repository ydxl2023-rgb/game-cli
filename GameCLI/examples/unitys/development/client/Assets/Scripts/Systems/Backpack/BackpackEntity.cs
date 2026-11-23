using Newtonsoft.Json;
using Oathx.Entity;
using System.Collections.Generic;

/// <summary>
/// Backpack entity, responsible for managing the player's backpack data, including storing items and providing methods to add or remove items from the backpack.
/// The backpack can have a limited number of slots, and each slot can hold one item. 
/// The entity also provides serialization and deserialization capabilities to save and load the backpack data as needed.
/// </summary>
[Initialize]
public class BackpackEntity : XEntitySerialize
{
    /// <summary>
    /// backpack slot data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Slot
    {
        /// <summary>
        /// slot index
        /// </summary>
        public int index
        { get; set; }

        /// <summary>
        /// item in the slot, null if the slot is empty
        /// </summary>
        public ItemEntity item
        { get; set; }

        /// <summary>
        /// operator to check if the slot exists, returns true if the slot is not null, false otherwise
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(Slot exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// backpack slots
    /// </summary>
    [JsonProperty]
    public List<Slot> slots
    { get; protected set; } = new List<Slot>();

    /// <summary>
    /// backpack constructor, initializes the backpack with empty slots
    /// </summary>
    public BackpackEntity()
    {
    }

    /// <summary>
    /// add item to backpack
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Grab(ItemEntity item)
    {
        var slot = slots.Find((v) => !v.item);
        if (!slot)
        {
            slots.Add(new Slot() {
                index = slots.Count, item = item 
            });
        }
        else
        {
            slot.item = item;
        }

        return true;
    }

    /// <summary>
    /// Grab multiple items into backpack, returns true if all items are successfully added, false otherwise. 
    /// If the backpack does not have enough space for all items, 
    /// it will add as many items as possible until the backpack is full.
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public bool GrabItems(List<ItemEntity> items) 
    {
        foreach (var item in items) 
        {
            Grab(item);
        }

        return true;
    }

    /// <summary>
    /// remove item from backpack, returns true if the item is successfully removed, false otherwise.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Drop(ItemEntity item)
    {
        var slot = slots.Find((v) => v.item == item);
        if (!slot)
            return false;
        
        slot.item = null;

        return true;
    }

    /// <summary>
    /// remove multiple items from backpack, returns true if all items are successfully removed, false otherwise.
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public bool DropItems(List<ItemEntity> items)
    {
        return items.TrueForAll((item) => Drop(item));
    }
}
