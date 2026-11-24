using System;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Present both monthly passes from the local recharge catalog.</summary>
public class UIShopMonthCard : MonoBehaviour
{
    /// <summary>SV prices and immediate, accumulated and daily reward rows.</summary>
    [SerializeField]
    private TextAsset catalog;

    /// <summary>Purchase labels in regular and premium pass order.</summary>
    [SerializeField]
    private Text[] prices;

    /// <summary>Reward slots in catalog display order.</summary>
    [SerializeField]
    private UIItemSlot[] slots;

    /// <summary>Display the configured pass prices and bind each reward to its slot.</summary>
    private void OnEnable()
    {
        if (!catalog)
            return;

        var cards = JsonUtility.FromJson<Catalog>(catalog.text).cards;
        int index = 0;
        for (int i = 0; i < cards.Length; i++)
        {
            var card = cards[i];
            prices[i].text = card.price;
            foreach (var group in new[] { card.immediate, card.total, card.daily })
            {
                foreach (var reward in group)
                {
                    slots[index++].Ref(new VMItemSlot {
                        Icon = reward.icon,
                        IconVisible = true,
                        Text = reward.count.ToString(),
                        Edge = Color.white
                    });
                }
            }
        }
    }

    /// <summary>Release each bound reward slot when the monthly pass page is hidden.</summary>
    private void OnDisable()
    {
        if (slots != null)
        {
            foreach (var slot in slots)
            {
                if (slot && slot.vm)
                    slot.Unref(slot.vm);
            }
        }
    }

    /// <summary>Monthly passes shown together on the shop page.</summary>
    [Serializable]
    public class Catalog
    {
        /// <summary>Passes in the same order as the purchase labels.</summary>
        public Card[] cards;
    }

    /// <summary>Display price and reward groups for one monthly pass.</summary>
    [Serializable]
    public class Card
    {
        /// <summary>Formatted price shown on the purchase button.</summary>
        public string price;

        /// <summary>Rewards granted when the pass is purchased.</summary>
        public Reward[] immediate;

        /// <summary>Cumulative rewards shown for the full pass period.</summary>
        public Reward[] total;

        /// <summary>Rewards available for a single daily claim.</summary>
        public Reward[] daily;
    }

    /// <summary>Item artwork and quantity displayed in one reward slot.</summary>
    [Serializable]
    public class Reward
    {
        /// <summary>Sprite asset path used by the item slot.</summary>
        public string icon;

        /// <summary>Item quantity shown on the reward icon.</summary>
        public int count;
    }
}
