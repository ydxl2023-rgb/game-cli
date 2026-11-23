using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Populate a native shop list from the local SV configuration sample.</summary>
public class UIShopDemoList : MonoBehaviour
{
    [SerializeField] private TextAsset catalog;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private UIShopDemoCell template;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private int columns;
    [SerializeField] private Vector2 cellSize;
    [SerializeField] private Image preview;
    [SerializeField] private Text previewTitle;
    [SerializeField] private Text previewPrice;
    [SerializeField] private Button previous;
    [SerializeField] private Button next;
    [SerializeField] private Text level;
    [SerializeField] private Text passPrice;
    private readonly Dictionary<string, Sprite> images = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
    private ShopDemoEntry[] entries;
    private ScrollRect scroll;
    private int selection;

    /// <summary>Populate the selected tab using its configuration key.</summary>
    public bool Populate(int tab)
    {
        var data = JsonUtility.FromJson<ShopDemoData>(catalog.text);
        var track = Array.Find(data.tracks, item => item.id == tab);
        if (track == null || track.entries.Length == 0)
            return false;

        foreach (var sprite in sprites)
            if (sprite) images[sprite.name + ".png"] = sprite;

        entries = track.entries;
        template.gameObject.SetActive(false);
        var content = new GameObject("ConfiguredItems", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        bool grid = columns > 1;
        content.anchorMin = content.anchorMax = content.pivot = new Vector2(0f, 1f);
        int across = grid ? columns : entries.Length;
        Canvas.ForceUpdateCanvases();
        if (grid)
        {
            var root = (RectTransform)transform;
            var parent = (RectTransform)viewport.parent;
            Vector3 a = parent.InverseTransformPoint(root.TransformPoint(new Vector3(root.rect.xMin + root.rect.width * .16f,root.rect.yMin + root.rect.height * .07f)));
            Vector3 b = parent.InverseTransformPoint(root.TransformPoint(new Vector3(root.rect.xMin + root.rect.width * .96f,root.rect.yMin + root.rect.height * .81f)));
            viewport.anchorMin = viewport.anchorMax = parent.pivot;
            viewport.pivot = Vector2.zero;
            viewport.anchoredPosition = a;
            viewport.sizeDelta = b-a;
        }
        float width = viewport.rect.width;
        float height = viewport.rect.height;
        var cardSize = grid ? ((RectTransform)template.transform).rect.size : cellSize;
        var stride = cardSize;
        if (grid)
        {
            // Reserve a gutter around each card in prefab units before scaling the grid.
            stride += new Vector2(24f, 16f);
        }

        float scale = grid
            ? Mathf.Min(width / (columns * stride.x), height / (2f * stride.y))
            : Mathf.Min(1f, height / cardSize.y);
        var size = stride * scale;
        int rows = Mathf.CeilToInt((float)entries.Length / across);
        content.sizeDelta = new Vector2(Mathf.Max(width, across * size.x), Mathf.Max(height, rows * size.y));
        scroll = viewport.GetComponent<ScrollRect>();
        if (!scroll) scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = !grid;
        scroll.vertical = grid;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        for (int i = 0; i < entries.Length; i++)
        {
            var cell = Instantiate(template, content, false);
            var rect = (RectTransform)cell.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0,1);
            rect.pivot = new Vector2(.5f,.5f);
            rect.sizeDelta = cardSize;
            rect.localScale = Vector3.one * scale;
            rect.anchoredPosition = new Vector2((i % across + .5f)*size.x, grid ? -(i / across + .5f)*size.y : -height*.5f);
            if (grid)
                cell.LayoutGridQuantity();

            cell.gameObject.SetActive(true);
            var entry = entries[i];
            int index = i;
            cell.Bind(entry, ImageFor(preview ? entry.preview : entry.icon),ImageFor(entry.currency),ImageFor(entry.preview), () => Select(index));
        }

        if (level) level.text = data.level.ToString();
        if (passPrice) passPrice.text = data.passPrice;
        if (previous) previous.onClick.AddListener(() => Select(selection - 1));
        if (next) next.onClick.AddListener(() => Select(selection + 1));
        Select(0);

        return true;
    }

    private Sprite ImageFor(string key)
    {
        return !string.IsNullOrEmpty(key) && images.TryGetValue(key,out var sprite) ? sprite : null;
    }

    private void Select(int index)
    {
        selection = Mathf.Clamp(index,0,entries.Length-1);
        if (!preview) return;
        var entry = entries[selection];
        preview.sprite = ImageFor(entry.preview);
        preview.enabled = preview.sprite;
        if (previewTitle) previewTitle.text = entry.name;
        if (previewPrice) previewPrice.text = entry.price;
        if (entries.Length > 1) scroll.horizontalNormalizedPosition = (float)selection / (entries.Length-1);
    }
}

[Serializable]
public class ShopDemoData
{
    public int level;
    public string passPrice;
    public ShopDemoTrack[] tracks;
}

[Serializable]
public class ShopDemoTrack
{
    public int id;
    public ShopDemoEntry[] entries;
}

[Serializable]
public class ShopDemoEntry
{
    public int id, count, limit;
    public string name, price, icon, preview, currency, discount, state;
}



