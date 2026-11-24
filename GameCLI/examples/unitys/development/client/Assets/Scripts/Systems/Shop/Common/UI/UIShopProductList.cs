using Config;
using System;
using System.Collections.Generic;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Present one configured shop track using the native scroll layout and Widget cells.</summary>
public class UIShopProductList : UIWidget<VMShopProductList>
{
    /// <summary>Artwork shipped with this skin and resolved synchronously.</summary>
    [SerializeField]
    private Sprite[] sprites;

    /// <summary>Source card for the Widget scroll synchronizer.</summary>
    [SerializeField]
    private UIShopProductCell template;

    /// <summary>Visible area of the authored scroll view.</summary>
    [SerializeField]
    private RectTransform viewport;

    /// <summary>Positions the bound cards within the content area.</summary>
    [SerializeField]
    private GridLayoutGroup layout;

    /// <summary>Scroll container targeted by the list binding.</summary>
    [SerializeField]
    private ScrollRect scroll;

    /// <summary>Number of cards per row for the vertical grid.</summary>
    [SerializeField]
    private int columns;

    /// <summary>Authored dimensions of a horizontal card.</summary>
    [SerializeField]
    private Vector2 cellSize;

    /// <summary>Whether selecting a card updates the wallpaper preview.</summary>
    [SerializeField]
    private bool wallpaper;

    private readonly Dictionary<string, Sprite> images = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

    private ShopPageItem[] entries;

    private int selection;

    /// <summary>Load the track once for the lifetime of this page instance.</summary>
    public bool Invalidate(ShopEntity entity, int tab)
    {
        if (!entity)
            return false;

        if (vm)
            return true;

        if (!template || !viewport || !scroll || !layout)
            return false;

        entries = entity.GetPageItems(tab);
        if (entries.Length == 0 || Excel.shopSettingConfig == null
            || !Excel.shopSettingConfig.TryGetValue(Excel.SHOP_LOCAL_SETTINGS, out var settings))
            return false;

        foreach (var sprite in sprites)
        {
            if (sprite)
                images[sprite.name + ".png"] = sprite;
        }

        ConfigureLayout();
        if (!Ref(new VMShopProductList()))
            return false;

        var items = new VList<VMShopProductCell>();
        for (int index = 0; index < entries.Length; index++)
            items.Add(CreateCard(entries[index], index));

        vm.Level = settings.Level.ToString();
        vm.PassPrice = settings.PassPrice;
        vm.Items = items;
        Select(0);

        return true;
    }

    /// <summary>Connect the preview controls to local page navigation.</summary>
    protected override bool Bind()
    {
        vm.Previous = (sender) => Select(selection - 1);
        vm.Next = (sender) => Select(selection + 1);

        return true;
    }

    /// <summary>Scale the content as a unit so the layout preserves the card artwork and gutters.</summary>
    private void ConfigureLayout()
    {
        scroll.enabled = true;
        template.gameObject.SetActive(false);
        template.transform.SetParent(viewport, false);
        template.transform.SetAsFirstSibling();
        Canvas.ForceUpdateCanvases();
        bool grid = columns > 1;
        if (grid)
        {
            var root = (RectTransform)transform;
            var parent = (RectTransform)viewport.parent;
            var area = root.rect;
            var min = parent.InverseTransformPoint(root.TransformPoint(area.min + Vector2.Scale(area.size, new Vector2(.16f, .07f))));
            var max = parent.InverseTransformPoint(root.TransformPoint(area.min + Vector2.Scale(area.size, new Vector2(.96f, .81f))));
            viewport.anchorMin = viewport.anchorMax = parent.pivot;
            viewport.pivot = Vector2.zero;
            viewport.anchoredPosition = min;
            viewport.sizeDelta = max - min;
        }

        var card = grid ? ((RectTransform)template.transform).rect.size : cellSize;
        var gap = grid ? new Vector2(24f, 16f) : Vector2.zero;
        var stride = card + gap;
        var areaSize = viewport.rect.size;
        float scale = grid
            ? Mathf.Min(areaSize.x / (columns * stride.x), areaSize.y / (2f * stride.y))
            : Mathf.Min(1f, areaSize.y / card.y);
        int across = grid ? columns : entries.Length;
        int rows = Mathf.CeilToInt((float)entries.Length / across);

        layout.cellSize = card;
        layout.spacing = gap;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = across;
        layout.childAlignment = grid ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;
        layout.padding.left = Mathf.RoundToInt(gap.x * .5f);
        layout.padding.top = Mathf.RoundToInt(gap.y * .5f);
        scroll.content.localScale = Vector3.one * scale;
        scroll.content.sizeDelta = Vector2.Max(areaSize / scale, new Vector2(across * stride.x, rows * stride.y));
    }

    /// <summary>Translate catalog values into bindings shared by the four card skins.</summary>
    private VMShopProductCell CreateCard(ShopPageItem entry, int index)
    {
        var card = new VMShopProductCell {
            Title = DisplayText(entry.NameTextId, entry.NameValue),
            Price = DisplayText(entry.PriceTextId, entry.PriceValue),
            Quantity = entry.Count.ToString(),
            Limit = entry.Limit > 0 ? $"{entry.Limit}/{entry.Limit}" : string.Empty,
            Discount = DisplayText(entry.DiscountTextId),
            Condition = DisplayText(entry.StateTextId),
            DiscountTag = entry.DiscountTextId != 0,
            Purchased = (ShopOfferState)entry.Availability == ShopOfferState.Purchased || (ShopOfferState)entry.Availability == ShopOfferState.Claimed,
            Locked = (ShopOfferState)entry.Availability == ShopOfferState.Locked,
            Select = (sender) => Select(index)
        };

        var icon = ImageFor(wallpaper ? entry.Preview : entry.Icon);
        var currency = ImageFor(entry.Currency);
        var premium = ImageFor(entry.Preview);
        if (icon)
            card.Icon = icon;
        if (currency)
            card.Currency = currency;
        if (premium)
            card.Premium = premium;

        card.IconVisible = icon;
        card.CurrencyVisible = currency;
        card.PremiumVisible = premium;

        return card;
    }

    /// <summary>Resolve localized labels or display a configured numeric value.</summary>
    private static string DisplayText(int textId, string value = "")
    {
        return textId > 0 ? CommonText.Get(textId) : value;
    }

    /// <summary>Resolve artwork already loaded with this page.</summary>
    private Sprite ImageFor(string key)
    {
        if (!string.IsNullOrEmpty(key) && images.TryGetValue(key, out var sprite))
            return sprite;

        return null;
    }

    /// <summary>Keep the selected wallpaper, caption and scroll position together.</summary>
    private bool Select(int index)
    {
        if (!wallpaper || !vm || entries == null || entries.Length == 0)
            return false;

        selection = Mathf.Clamp(index, 0, entries.Length - 1);
        var entry = entries[selection];
        var artwork = ImageFor(entry.Preview);
        if (artwork)
            vm.Preview = artwork;

        vm.PreviewTitle = DisplayText(entry.NameTextId, entry.NameValue);
        vm.PreviewPrice = DisplayText(entry.PriceTextId, entry.PriceValue);
        if (entries.Length > 1)
            scroll.horizontalNormalizedPosition = (float)selection / (entries.Length - 1);

        return true;
    }

    /// <summary>Detach navigation and presentation when the cached page is released.</summary>
    private void OnDestroy()
    {
        if (vm)
            Unref(vm);
    }
}
