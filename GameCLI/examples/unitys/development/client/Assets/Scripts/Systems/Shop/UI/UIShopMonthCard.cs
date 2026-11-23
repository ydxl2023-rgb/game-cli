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

    private void OnDisable()
    {
        if (slots == null)
            return;

        foreach (var slot in slots)
            if (slot && slot.vm) slot.Unref(slot.vm);
    }

    [Serializable]
    public class Catalog
    {
        public Card[] cards;
    }

    [Serializable]
    public class Card
    {
        public string price;
        public Reward[] immediate, total, daily;
    }

    [Serializable]
    public class Reward
    {
        public string icon;
        public int count;
    }
}
