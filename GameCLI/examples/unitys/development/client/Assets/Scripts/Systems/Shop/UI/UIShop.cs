using Config;
using System.Globalization;
using Oathx.Widget;
using System.Collections;
using UnityEngine;

/// <summary>Displays the Shop view through serialized Metadata.</summary>
[Config("Assets/Data/Widgets/Shop/Shop.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UIShop : UIWidget<VMShop>
{
    /// <summary>Blocks mall input during the original opening interval.</summary>
    [SerializeField]
    private CanvasGroup interaction;

    /// <summary>Source navigation and page assets assigned during prefab production.</summary>
    [SerializeField]
    private ShopSection[] sections;

    /// <summary>Native content layer beneath the navigation controls.</summary>
    [SerializeField]
    private Transform pageHost;

    /// <summary>Secondary navigation is visible for sections with multiple destinations.</summary>
    [SerializeField]
    private GameObject tabStrip;

    /// <summary>Current embedded page instance.</summary>
    private GameObject page;

    /// <summary>Bind player intents when the widget receives its view model.</summary>
    protected override bool Bind()
    {
        vm.CloseClick = Send<ReqShopClose>;
        vm.HelpClick = Send<ReqShopShowHelp>;
        vm.PreviousClick = (sender) => Send(new ReqShopShiftSection { offset = -1 });
        vm.NextClick = (sender) => Send(new ReqShopShiftSection { offset = 1 });
        vm.GroupClick = (sender, index) => Send(new ReqShopSelectSection { index = index });
        vm.TabClick = (sender, index) => Send(new ReqShopSelectTab { index = index });
        vm.Currency1Click = Send<ReqShopShowHelp>;
        vm.Currency2Click = Send<ReqShopShowHelp>;
        vm.Currency3Click = Send<ReqShopShowHelp>;
        vm.AddCurrency1Click = Send<ReqShopShowHelp>;
        vm.AddCurrency2Click = Send<ReqShopShowHelp>;
        vm.AddCurrency3Click = Send<ReqShopShowHelp>;
        vm.GroupTemplate = false;
        vm.TabTemplate = false;
        vm.MessageVisible = true;

        return true;
    }

    /// <summary>Render the current mall data.</summary>
    public bool Invalidate(ShopEntity entity)
    {
        if (!entity || !vm)
            return false;

        vm.Currency1 = Money(entity.demoBalance);
        vm.Currency2 = "0";
        vm.Currency3 = entity.demoTokens.ToString(CultureInfo.InvariantCulture);
        var groups = new VList<VMShopGroup>();
        for (int i = 0; GetSection(i) != null; i++)
        {
            groups.Add(new VMShopGroup {
                Title = GetSection(i).title,
                icon = GetSection(i).icon,
                Selected = i == entity.sectionIndex,
                Unselected = i != entity.sectionIndex
            });
        }

        var tabs = new VList<VMShopTab>();
        var section = GetSection(entity.sectionIndex);
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

        return true;
    }

    /// <summary>Display a localized mall notification.</summary>
    public bool ShowMessage(int textId, params object[] args)
    {
        if (!vm)
            return false;

        vm.Message = CommonText.Get(textId, args);

        return true;
    }

    /// <summary>Format the localized offer title.</summary>
    internal static string ProductTitle(ShopProduct product)
    {
        return CommonText.Get(product.nameId, product.reward);
    }

    /// <summary>Format minor currency units.</summary>
    internal static string Money(long amount)
    {
        return (amount / 100m).ToString("0.00", CultureInfo.InvariantCulture);
    }

    /// <summary>Resolve a configured source section by its display index.</summary>
    public ShopSection GetSection(int index)
    {
        return sections != null && index >= 0 && index < sections.Length ? sections[index] : null;
    }

    /// <summary>Replace content beneath the mall navigation.</summary>
    public void DisplayPage(GameObject prefab, bool showTabs, int tab)
    {
        if (tabStrip)
            tabStrip.SetActive(showTabs);

        if (page)
        {
            page.SetActive(false);
            Destroy(page);
        }

        if (prefab && pageHost)
        {
            page = Instantiate(prefab, pageHost, false);
            page.SetActive(true);
            if (page.TryGetComponent<UIShopDemoList>(out var list)) list.Populate(tab);
        }
    }

    /// <summary>Read the source opening gate for separately hosted product pages.</summary>
    /// <returns>True after the mall entrance allows input.</returns>
    public bool CanInteract()
    {
        return interaction && interaction.interactable;
    }

    /// <summary>Restart the original half-second opening interaction gate.</summary>
    private void OnEnable()
    {
        if (interaction)
        {
            interaction.interactable = false;

            StartCoroutine(EnableInteraction());
        }
    }

    /// <summary>Release the gate when the view is hidden or destroyed.</summary>
    private void OnDisable()
    {
        StopAllCoroutines();

        if (interaction)
            interaction.interactable = true;
    }

    /// <summary>Restore input after the source opening gate duration.</summary>
    /// <returns>The unscaled opening delay.</returns>
    private IEnumerator EnableInteraction()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        if (interaction)
            interaction.interactable = true;
    }
}

/// <summary>Configured first-level mall section.</summary>
[System.Serializable]
public class ShopSection
{
    public string title;
    public Sprite icon;
    public ShopPage[] pages;
}

/// <summary>Configured second-level destination and optional native page.</summary>
[System.Serializable]
public class ShopPage
{
    public int id;
    public string title;
    public string window;
    public GameObject prefab;
}

/// <summary>Player request for the mall close action.</summary>
public class ReqShopClose : UIRequestEvent
{
}

/// <summary>Player request for the mall help action.</summary>
public class ReqShopShowHelp : UIRequestEvent
{
}

/// <summary>Player request for the mall section shift action.</summary>
public class ReqShopShiftSection : UIRequestEvent
{
    /// <summary>Requested navigation or product value.</summary>
    public int offset
    { get; set; }
}

/// <summary>Player request for the mall section action.</summary>
public class ReqShopSelectSection : UIRequestEvent
{
    /// <summary>Requested navigation or product value.</summary>
    public int index
    { get; set; }
}

/// <summary>Player request for the mall tab action.</summary>
public class ReqShopSelectTab : UIRequestEvent
{
    /// <summary>Requested navigation or product value.</summary>
    public int index
    { get; set; }
}

/// <summary>Player request for the mall product action.</summary>
public class ReqShopSelectProduct : UIRequestEvent
{
    /// <summary>Requested navigation or product value.</summary>
    public int productId
    { get; set; }
}

/// <summary>Player request for the mall purchase cancel action.</summary>
public class ReqShopCancelPurchase : UIRequestEvent
{
}

/// <summary>Player request for the mall purchase confirm action.</summary>
public class ReqShopConfirmPurchase : UIRequestEvent
{
}
