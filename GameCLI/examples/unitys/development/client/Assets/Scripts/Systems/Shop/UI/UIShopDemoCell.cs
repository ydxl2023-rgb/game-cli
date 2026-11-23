using UnityEngine;
using UnityEngine.UI;

/// <summary>Display one configuration row on the native product or milestone cell.</summary>
public class UIShopDemoCell : MonoBehaviour
{
    [SerializeField] private Text title, price, quantity, limit, discount, condition;
    [SerializeField] private Image icon, currency, premium;
    [SerializeField] private GameObject purchased, locked, discountTag;
    [SerializeField] private Button select;

    public void Bind(ShopDemoEntry entry, Sprite art, Sprite coin, Sprite reward, UnityEngine.Events.UnityAction click)
    {
        if (title) title.text = entry.name;
        if (price) price.text = entry.price;
        if (quantity) quantity.text = entry.count.ToString();
        if (limit) limit.text = entry.limit > 0 ? string.Format("{0}/{0}",entry.limit) : string.Empty;
        if (discount) discount.text = entry.discount;
        if (condition) condition.text = entry.state;
        SetImage(icon,art);
        SetImage(currency,entry.price == "免费" || entry.price == "广告获取" ? null : coin);
        SetImage(premium,reward);
        if (purchased) purchased.SetActive(entry.state == "已购买" || entry.state == "已领取");
        if (locked) locked.SetActive(!string.IsNullOrEmpty(entry.state) && entry.state != "已购买" && entry.state != "已领取");
        if (discountTag) discountTag.SetActive(!string.IsNullOrEmpty(entry.discount));
        if (select) select.onClick.AddListener(click);
    }

    /// <summary>Place the stack count in the gap between the product artwork and its title.</summary>
    public void LayoutGridQuantity()
    {
        if (quantity)
        {
            var rect = quantity.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, .5f);
            rect.pivot = new Vector2(1f, .5f);
            rect.anchoredPosition = new Vector2(-18f, -60f);
            quantity.alignment = TextAnchor.MiddleRight;
        }
    }
    private static void SetImage(Image image, Sprite sprite)
    {
        if (!image) return;
        image.sprite = sprite;
        image.enabled = sprite;
        image.preserveAspect = true;
    }
}

