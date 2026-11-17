using Oathx.Entity;
using System.Collections.Generic;

/// <summary>Local demonstration state for the reusable mall template.</summary>
[Initialize]
public partial class ShopEntity : XEntity
{
    /// <summary>Catalog supplied by the product or the explicit demo initializer.</summary>
    private List<ShopProduct> products = new List<ShopProduct>();

    /// <summary>Number of products in the owned catalog.</summary>
    public int productCount
    {
        get { return products.Count; }
    }

    /// <summary>Selected catalog category.</summary>
    public ShopCategory category
    { get; private set; }

    /// <summary>Pending product identifier, or zero when no confirmation is open.</summary>
    public int selectedProduct
    { get; private set; }

    /// <summary>Simulated spending balance in minor currency units, never real money.</summary>
    public long demoBalance
    { get; private set; }

    /// <summary>Locally granted tokens, independent of the product wallet.</summary>
    public long demoTokens
    { get; private set; }

    /// <summary>Number of successful local purchases.</summary>
    public int purchaseCount
    { get; private set; }

    /// <summary>Most recent local purchase result.</summary>
    public ShopPurchaseResult result
    { get; private set; }

    /// <summary>Read detached products for the selected category.</summary>
    /// <returns>A catalog snapshot that cannot mutate purchase state.</returns>
    public List<ShopProduct> GetProducts()
    {
        var result = new List<ShopProduct>();
        foreach (var entry in products)
        {
            if (entry.category == category)
            {
                result.Add(new ShopProduct {
                    id = entry.id,
                    category = entry.category,
                    nameId = entry.nameId,
                    price = entry.price,
                    reward = entry.reward,
                    remaining = entry.remaining,
                    icon = entry.icon
                });
            }
        }

        return result;
    }

    /// <summary>Install a validated local catalog without making a service request.</summary>
    /// <param name="catalog">Product definitions owned by this demonstration.</param>
    /// <param name="balance">Simulated balance in minor currency units.</param>
    /// <returns>True when the complete catalog was accepted.</returns>
    public bool InitializeDemo(IEnumerable<ShopProduct> catalog, long balance)
    {
        if (catalog == null || balance < 0)
            return false;

        var entries = new List<ShopProduct>();
        var identifiers = new HashSet<int>();
        foreach (var entry in catalog)
        {
            if (entry == null || entry.id <= 0 || !identifiers.Add(entry.id)
                || entry.price < 0 || entry.reward <= 0 || entry.remaining < 0
                || !System.Enum.IsDefined(typeof(ShopCategory), entry.category))
                return false;

            entries.Add(new ShopProduct {
                id = entry.id,
                category = entry.category,
                nameId = entry.nameId,
                price = entry.price,
                reward = entry.reward,
                remaining = entry.remaining,
                icon = entry.icon
            });
        }

        if (entries.Count == 0)
            return false;

        products = entries;
        demoBalance = balance;
        demoTokens = 0;
        purchaseCount = 0;
        selectedProduct = 0;
        category = entries[0].category;
        result = ShopPurchaseResult.None;

        return true;
    }

    /// <summary>Switch category and cancel any stale pending purchase.</summary>
    /// <param name="value">Requested category.</param>
    /// <returns>True when the category exists in the catalog.</returns>
    public bool SelectCategory(ShopCategory value)
    {
        if (!System.Enum.IsDefined(typeof(ShopCategory), value)
            || !products.Exists(entry => entry.category == value))
            return false;

        category = value;
        selectedProduct = 0;
        result = ShopPurchaseResult.None;

        return true;
    }

    /// <summary>Select an available product for local confirmation.</summary>
    /// <param name="productId">Identifier in the current category.</param>
    /// <returns>True when confirmation can be opened.</returns>
    public bool SelectProduct(int productId)
    {
        if (selectedProduct != 0)
            return false;

        var entry = products.Find(item => item.id == productId && item.category == category);
        if (entry == null || entry.remaining <= 0)
            return false;

        selectedProduct = productId;
        result = ShopPurchaseResult.None;

        return true;
    }

    /// <summary>Cancel confirmation without changing balance or rewards.</summary>
    public void CancelPurchase()
    {
        selectedProduct = 0;
    }

    /// <summary>Confirm one local purchase and consume the pending selection once.</summary>
    /// <returns>The result to present through the product localization table.</returns>
    public ShopPurchaseResult ConfirmPurchase()
    {
        var entry = products.Find(item => item.id == selectedProduct && item.category == category);
        selectedProduct = 0;

        if (entry == null)
        {
            result = ShopPurchaseResult.NoSelection;

            return result;
        }

        if (entry.remaining <= 0)
        {
            result = ShopPurchaseResult.SoldOut;

            return result;
        }

        if (demoBalance < entry.price)
        {
            result = ShopPurchaseResult.InsufficientBalance;

            return result;
        }

        if (demoTokens > long.MaxValue - entry.reward || purchaseCount == int.MaxValue)
        {
            result = ShopPurchaseResult.CapacityExceeded;

            return result;
        }

        demoBalance -= entry.price;
        demoTokens += entry.reward;
        entry.remaining--;
        purchaseCount++;
        result = ShopPurchaseResult.Succeeded;

        return result;
    }
}

/// <summary>Supported categories in the first local mall reference.</summary>
public enum ShopCategory
{
    /// <summary>Token recharge examples.</summary>
    Recharge,

    /// <summary>Limited gift examples.</summary>
    Gift
}

/// <summary>Local purchase outcome without a localized presentation.</summary>
public enum ShopPurchaseResult
{
    /// <summary>No operation has completed.</summary>
    None,

    /// <summary>One simulated transaction was completed.</summary>
    Succeeded,

    /// <summary>No valid confirmation was pending.</summary>
    NoSelection,

    /// <summary>The local purchase limit was reached.</summary>
    SoldOut,

    /// <summary>The simulated spending balance was insufficient.</summary>
    InsufficientBalance,

    /// <summary>The local reward counter cannot represent another grant.</summary>
    CapacityExceeded
}

/// <summary>A local product definition and its remaining purchase limit.</summary>
public class ShopProduct
{
    /// <summary>Stable catalog identifier.</summary>
    public int id
    { get; set; }

    /// <summary>Owning category.</summary>
    public ShopCategory category
    { get; set; }

    /// <summary>CommonText identifier resolved by the presentation layer.</summary>
    public int nameId
    { get; set; }

    /// <summary>Simulated price in minor currency units.</summary>
    public long price
    { get; set; }

    /// <summary>Number of local tokens granted once.</summary>
    public long reward
    { get; set; }

    /// <summary>Remaining local purchase limit.</summary>
    public int remaining
    { get; set; }

    /// <summary>Product image asset path.</summary>
    public string icon
    { get; set; }
}
