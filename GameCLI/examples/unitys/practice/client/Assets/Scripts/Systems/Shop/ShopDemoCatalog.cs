using Config;
using System.Collections.Generic;

/// <summary>Explicit example catalog; replace it with product configuration when integrating.</summary>
public static class ShopDemoCatalog
{
    /// <summary>Independent reward image retained with the mall materials.</summary>
    public const string tokenIcon = "Assets/Data/Widgets/Shop/ShopAssets/icon_wupin_zuanshi.png";

    /// <summary>Create local examples without implying real offers or live prices.</summary>
    /// <returns>New independent product objects for the local simulation.</returns>
    public static List<ShopProduct> Create()
    {
        var products = new List<ShopProduct>();
        int[] amounts = new int[] {
            60,
            300,
            980,
            1980,
            3280,
            6480
        };

        for (int i = 0; i < amounts.Length; i++)
        {
            int amount = amounts[i];
            products.Add(new ShopProduct {
                id = i + 1,
                category = ShopCategory.Recharge,
                nameId = Excel.SHOP_TOKEN_AMOUNT,
                price = amount * 10L,
                reward = amount,
                remaining = 99,
                icon = $"Assets/Data/Widgets/Shop/ShopAssets/sc_czsd_icon_{amount}zuanshi_cn.png"
            });
        }

        int[] names = new int[] {
            Excel.SHOP_GIFT_SMALL,
            Excel.SHOP_GIFT_MEDIUM,
            Excel.SHOP_GIFT_LARGE
        };

        for (int i = 0; i < names.Length; i++)
            products.Add(new ShopProduct {
                id = 101 + i,
                category = ShopCategory.Gift,
                nameId = names[i],
                price = 600L * (i + 1),
                reward = 100L * (i + 1),
                remaining = i + 1,
                icon = tokenIcon
            });

        return products;
    }
}
