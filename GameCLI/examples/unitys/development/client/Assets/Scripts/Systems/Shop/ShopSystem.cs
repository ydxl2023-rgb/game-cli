using Config;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using System.Globalization;

/// <summary>Coordinates the local mall reference without payment or network APIs.</summary>
public class ShopSystem : RuntimeSystem
{
    /// <summary>Open the mall and initialize its local demonstration catalog once.</summary>
    /// <returns>True when the page was requested.</returns>
    public override bool OnActive()
    {
        if (!IsActive() || !InitializeDemo())
            return false;

        UISystem.GetSingleton().OpenWidget<UIShop>((widget) => {
            if (widget)
            {
                var vm = widget.vm;
                if (!vm)
                    return;

                vm.CloseClick = (sender) => Close();
                vm.HelpClick = (sender) => ShowMessage(Excel.SHOP_HELP);
                vm.PreviousClick = (sender) => MoveSection(-1);
                vm.NextClick = (sender) => MoveSection(1);
                vm.GroupClick = (sender, index) => OpenPage(index, 0);
                vm.TabClick = (sender, index) => SelectTab(index);
                vm.Currency1Click = (sender) => ShowMessage(Excel.SHOP_HELP);
                vm.Currency2Click = (sender) => ShowMessage(Excel.SHOP_HELP);
                vm.Currency3Click = (sender) => ShowMessage(Excel.SHOP_HELP);
                vm.AddCurrency1Click = (sender) => ShowMessage(Excel.SHOP_HELP);
                vm.AddCurrency2Click = (sender) => ShowMessage(Excel.SHOP_HELP);
                vm.AddCurrency3Click = (sender) => ShowMessage(Excel.SHOP_HELP);
                vm.GroupTemplate = false;
                vm.TabTemplate = false;
                vm.MessageVisible = true;

                OpenPage(0, 0);
                ShowMessage(Excel.SHOP_DEMO_NOTICE);
            }
        });

        return true;
    }

    /// <summary>Cancel the local selection and release all owned views.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (entity)
            entity.CancelPurchase();

        System.Type[] widgets = new System.Type[] {
            typeof(UIShopPurchase),
            typeof(UIShopGift),
            typeof(UIShopRecharge),
            typeof(UIShop)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>Refresh the mall and any currently open product list.</summary>
    /// <returns>True when the mall was available.</returns>
    public bool Refresh()
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIShop>(out var widget))
            return false;

        var vm = widget.vm;
        if (!vm)
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        vm.Currency1 = Money(entity.demoBalance);
        vm.Currency2 = "0";
        vm.Currency3 = entity.demoTokens.ToString(CultureInfo.InvariantCulture);
        var groups = new VList<VMShopGroup>();
        for (int i = 0; widget.GetSection(i) != null; i++)
        {
            groups.Add(new VMShopGroup {
                Title = widget.GetSection(i).title,
                icon = widget.GetSection(i).icon,
                Selected = i == entity.sectionIndex,
                Unselected = i != entity.sectionIndex
            });
        }

        var tabs = new VList<VMShopTab>();
        var section = widget.GetSection(entity.sectionIndex);
        if (section != null)
        {
            for (int i = 0; i < section.pages.Length; i++)
            {
                tabs.Add(new VMShopTab {
                    Title = section.pages[i].title,
                    Selected = i == entity.pageIndex,
                    Unselected = i != entity.pageIndex
                });
            }
        }

        vm.Groups = groups;
        vm.Tabs = tabs;
        RefreshProducts(entity);

        return true;
    }

    /// <summary>Open a configured source section and secondary tab.</summary>
    public bool OpenPage(int sectionIndex, int pageIndex)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIShop>(out var widget))
            return false;

        var section = widget.GetSection(sectionIndex);
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity || section == null || pageIndex < 0 || pageIndex >= section.pages.Length)
            return false;

        var page = section.pages[pageIndex];
        entity.SetPage(sectionIndex, pageIndex);
        widget.DisplayPage(page.prefab, section.pages.Length > 1, page.id);

        if (page.window == "RechargeShopView")
            return SelectCategory(ShopCategory.Recharge);

        if (page.window == "NormalGiftView")
            return SelectCategory(ShopCategory.Gift);

        System.Type[] widgets = new System.Type[] {
            typeof(UIShopPurchase),
            typeof(UIShopGift),
            typeof(UIShopRecharge)
        };
        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);

        return Refresh();
    }

    /// <summary>Navigate to the adjacent primary section.</summary>
    private bool MoveSection(int offset)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();

        return entity && OpenPage(entity.sectionIndex + offset, 0);
    }

    /// <summary>Navigate within the current primary section.</summary>
    private bool SelectTab(int index)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();

        return entity && OpenPage(entity.sectionIndex, index);
    }
    /// <summary>Switch between the supported local recharge and gift catalogs.</summary>
    /// <param name="category">Requested category.</param>
    /// <returns>True when the child page was requested.</returns>
    public bool SelectCategory(ShopCategory category)
    {
        if (!IsActive())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        if (!entity.SelectCategory(category))
            return false;

        System.Type[] widgets = new System.Type[] {
            typeof(UIShopPurchase),
            typeof(UIShopGift),
            typeof(UIShopRecharge)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        Refresh();

        if (category == ShopCategory.Recharge)
        {
            UISystem.GetSingleton().OpenWidget<UIShopRecharge>((widget) => {
                if (widget)
                {
                    var vm = widget.vm;
                    if (!vm)
                        return;

                    vm.ProductTemplate = false;
                    vm.ProductClick = (sender, index) => SelectProduct(index);

                    Refresh();
                }
            });
        }
        else
        {
            UISystem.GetSingleton().OpenWidget<UIShopGift>((widget) => {
                if (widget)
                {
                    var vm = widget.vm;
                    if (!vm)
                        return;

                    vm.ProductTemplate = false;
                    vm.ProductClick = (sender, index) => SelectProduct(index);

                    Refresh();
                }
            });
        }

        return true;
    }

    /// <summary>Open the local confirmation for one product in the visible list.</summary>
    /// <param name="index">Index within the current category.</param>
    /// <returns>True when a valid product confirmation was requested.</returns>
    public bool SelectProduct(int index)
    {
        if (!IsActive())
            return false;

        if (!UISystem.GetSingleton().TryGetWidget<UIShop>(out var main) || !main.CanInteract())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        var products = entity.GetProducts();
        if (index < 0 || index >= products.Count || !entity.SelectProduct(products[index].id))
            return false;

        var product = products[index];

        UISystem.GetSingleton().OpenWidget<UIShopPurchase>((widget) => {
            if (widget)
            {
                var vm = widget.vm;
                if (!vm)
                    return;

                vm.CloseClick = (sender) => CancelPurchase();
                vm.ConfirmClick = (sender) => ConfirmPurchase();
                vm.ConfirmText = CommonText.Get(Excel.SHOP_CONFIRM);
                vm.Price = CommonText.Get(Excel.SHOP_PRICE, Money(product.price));
                vm.Title = ProductTitle(product);
                vm.Icon = product.icon;
                vm.RewardTitle = CommonText.Get(Excel.SHOP_REWARDS);
                vm.RewardName = CommonText.Get(Excel.SHOP_TOKEN_NAME);
                vm.RewardCount = CommonText.Get(Excel.SHOP_COUNT, product.reward);
                vm.RewardIcon = ShopDemoCatalog.tokenIcon;
                vm.AlternatePurchase = false;
                vm.FreeText = false;
                vm.RandomTitle = false;
                vm.RandomRewards = false;
                vm.Discount = false;
                vm.Labels = false;
            }
        });

        return true;
    }

    /// <summary>Confirm the pending local transaction once and refresh the visible catalog.</summary>
    /// <returns>True only when the simulated transaction succeeded.</returns>
    public bool ConfirmPurchase()
    {
        if (!IsActive())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        long before = entity.demoTokens;
        var result = entity.ConfirmPurchase();

        CancelPurchase();
        Refresh();
        ShowMessage(result == ShopPurchaseResult.Succeeded ? Excel.SHOP_SUCCESS
            : result == ShopPurchaseResult.InsufficientBalance ? Excel.SHOP_INSUFFICIENT
            : result == ShopPurchaseResult.SoldOut ? Excel.SHOP_SOLD_OUT
            : result == ShopPurchaseResult.CapacityExceeded ? Excel.SHOP_CAPACITY : Excel.SHOP_NO_SELECTION,
            entity.demoTokens - before);

        return result == ShopPurchaseResult.Succeeded;
    }

    /// <summary>Close confirmation without applying a transaction.</summary>
    /// <returns>True when pending selection was cancelled.</returns>
    public bool CancelPurchase()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        entity.CancelPurchase();

        System.Type[] widgets = new System.Type[] {
            typeof(UIShopPurchase)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);

        return true;
    }

    /// <summary>Ask the owning coordinator to deactivate this mall.</summary>
    /// <returns>True when a local deactivation event was emitted.</returns>
    public bool Close()
    {
        if (!IsActive())
            return false;

        XPluginUtility.FireEvent(new SystemDetive { type = typeof(ShopSystem) });

        return true;
    }

    /// <summary>Install explicit sample data only when no product catalog exists.</summary>
    /// <returns>True when a catalog is available.</returns>
    private bool InitializeDemo()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        if (entity.productCount > 0)
            return true;

        return entity.InitializeDemo(ShopDemoCatalog.Create(), 100000);
    }

    /// <summary>Publish cell view models without retaining a second copy of entity data.</summary>
    /// <param name="entity">Validated local mall entity.</param>
    private void RefreshProducts(ShopEntity entity)
    {
        if (entity.category == ShopCategory.Recharge && UISystem.GetSingleton().TryGetWidget<UIShopRecharge>(out var recharge))
        {
            var vm = recharge.vm;
            if (vm)
            {
                var items = new VList<VMShopRechargeItem>();
                foreach (var product in entity.GetProducts())
                    items.Add(new VMShopRechargeItem {
                        Title = ProductTitle(product),
                        Price = CommonText.Get(Excel.SHOP_PRICE, Money(product.price)),
                        Item = new VMItemSlot {
                            Icon = product.icon,
                            IconVisible = true,
                            Text = string.Empty
                        },
                        Bonus = string.Empty,
                        FirstPay = false,
                        FirstPayText = CommonText.Get(Excel.SHOP_FIRST_PAY),
                    });

                vm.Products = items;
            }
        }

        if (entity.category == ShopCategory.Gift && UISystem.GetSingleton().TryGetWidget<UIShopGift>(out var gift))
        {
            var vm = gift.vm;
            if (vm)
            {
                var items = new VList<VMShopGiftItem>();
                foreach (var product in entity.GetProducts())
                    items.Add(new VMShopGiftItem {
                        Title = ProductTitle(product),
                        Price = CommonText.Get(Excel.SHOP_PRICE, Money(product.price)),
                        Item = new VMItemSlot {
                            Icon = product.icon,
                            IconVisible = true,
                            Text = CommonText.Get(Excel.SHOP_COUNT, product.reward)
                        },
                        Limit = CommonText.Get(Excel.SHOP_LIMIT, product.remaining),
                        SoldOut = product.remaining == 0,
                        SoldOutText = CommonText.Get(Excel.SHOP_SOLD_OUT),
                    });

                vm.Products = items;
            }
        }
    }

    /// <summary>Display a localized local-operation message.</summary>
    /// <param name="textId">CommonText identifier.</param>
    /// <param name="args">Localized format arguments.</param>
    /// <returns>True when the current mall message was changed.</returns>
    private bool ShowMessage(int textId, params object[] args)
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIShop>(out var widget))
            return false;

        var vm = widget.vm;
        if (!vm)
            return false;

        vm.Message = CommonText.Get(textId, args);

        return true;
    }

    /// <summary>Resolve a catalog title through CommonText.</summary>
    /// <param name="product">Product to present.</param>
    /// <returns>The localized product title.</returns>
    private string ProductTitle(ShopProduct product)
    {
        return CommonText.Get(product.nameId, product.reward);
    }

    /// <summary>Format minor simulated currency units without binary floating point.</summary>
    /// <param name="amount">Amount in minor units.</param>
    /// <returns>An invariant two-decimal amount for a localized price phrase.</returns>
    private string Money(long amount)
    {
        return (amount / 100m).ToString("0.00", CultureInfo.InvariantCulture);
    }
}




