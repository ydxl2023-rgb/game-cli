using Config;
using Oathx.Bundle.Assets;
using System.Globalization;
using Oathx.Widget;
using System.Collections;
using System.Collections.Generic;
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
    private ShopEntity.Section[] sections;

    /// <summary>Native content layer beneath the navigation controls.</summary>
    [SerializeField]
    private Transform pageHost;

    /// <summary>Secondary navigation is visible for sections with multiple destinations.</summary>
    [SerializeField]
    private GameObject tabStrip;

    /// <summary>Pages visited during this window's lifetime, indexed by business page ID.</summary>
    private readonly Dictionary<int, UIShopPage> pages = new Dictionary<int, UIShopPage>();

    /// <summary>Page currently displayed in the content area.</summary>
    private UIShopPage currentPage;

    /// <summary>Entity providing available sections and the remembered destination.</summary>
    private ShopEntity model;

    /// <summary>Only the latest outstanding selection may replace the visible page.</summary>
    private ShopEntity.Page loading;

    /// <summary>Owner of the pending destination.</summary>
    private int loadingSection;

    /// <summary>Sequence number invalidating stale asynchronous completions.</summary>
    private int generation;

    /// <summary>Bind player intents when the widget receives its view model.</summary>
    protected override bool Bind()
    {
        vm.CloseClick = Send<ReqShopClose>;
        vm.HelpClick = Send<ReqShopShowHelp>;
        vm.PreviousClick = (sender) => MoveSection(-1);
        vm.NextClick = (sender) => MoveSection(1);
        vm.GroupClick = (sender, index) => OpenPage(index, 0);
        vm.TabClick = (sender, index) => model && OpenPage(IndexOfSection(model.sectionId), index);
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

    /// <summary>Release every visited page before detaching the mall VM.</summary>
    public override void Unref(VM value)
    {
        if (vm && ReferenceEquals(vm, value))
        {
            ClearPages();
            model = null;
            base.Unref(value);
        }
    }

    /// <summary>Render the current mall data.</summary>
    public bool Invalidate(ShopEntity entity)
    {
        if (!entity || !vm)
            return false;

        vm.Currency1 = Money(entity.demoBalance);
        vm.Currency2 = "0";
        vm.Currency3 = entity.demoTokens.ToString(CultureInfo.InvariantCulture);
        if (!ReferenceEquals(model, entity))
        {
            ClearPages();
            model = entity;
            model.InitializeSections(sections);
            RefreshNavigation();
        }

        if (loading != null)
            return true;

        if (currentPage)
            return currentPage.RefreshPage(entity);

        int index = IndexOfSection(entity.sectionId);
        if (index < 0)
            index = 0;

        var section = GetSection(index);
        if (section == null)
            return true;

        int tab = System.Array.FindIndex(section.pages, entry => entry.id == entity.pageId);

        return OpenPage(index, Mathf.Max(0, tab));
    }

    /// <summary>Expose a registered section by its current display index.</summary>
    public ShopEntity.Section GetSection(int index)
    {
        return model && index >= 0 && index < model.sections.Count ? model.sections[index] : null;
    }

    /// <summary>Add a primary tab without loading its content unless it becomes the first tab.</summary>
    public bool RegisterSection(ShopEntity.Section section)
    {
        if (!model || !model.RegisterSection(section))
            return false;

        RefreshNavigation();

        return currentPage || loading != null || OpenPage(0, 0);
    }

    /// <summary>Remove a tab and select the nearest available destination when necessary.</summary>
    public bool RemoveSection(int id)
    {
        int index = IndexOfSection(id);
        var removed = GetSection(index);
        if (removed == null || !model.RemoveSection(id))
            return false;

        if (loadingSection == id)
            CancelLoad();

        if (model.sectionId == id)
        {
            if (model.selectedProduct != 0)
                Send(new ReqShopCancelPurchase());

            if (currentPage)
                currentPage.HidePage();

            currentPage = null;
            model.SetPage(0, 0);
        }

        foreach (var route in removed.pages)
            EvictPage(route.id);

        RefreshNavigation();
        if (!currentPage && loading == null && model.sections.Count > 0)
            return OpenPage(Mathf.Min(index, model.sections.Count - 1), 0);

        return true;
    }

    /// <summary>Add a business page to an existing section without preloading it.</summary>
    public bool RegisterPage(int sectionId, ShopEntity.Page page)
    {
        if (!model || !model.RegisterPage(sectionId, page))
            return false;

        RefreshNavigation();

        return currentPage || loading != null || SelectPage(page.id);
    }

    /// <summary>Select a business destination by its stable ID.</summary>
    public bool SelectPage(int pageId)
    {
        if (!model || !model.FindPage(pageId, out int section, out int tab))
            return false;

        return OpenPage(section, tab);
    }

    /// <summary>Remove a destination, releasing its instance and pending request.</summary>
    public bool RemovePage(int pageId)
    {
        if (!model || !model.FindPage(pageId, out int index, out int tab))
            return false;

        var section = GetSection(index);
        if (section.pages.Length == 1)
            return RemoveSection(section.id);

        if (!model.RemovePage(pageId))
            return false;

        if (loading != null && loading.id == pageId)
            CancelLoad();

        if (model.pageId == pageId)
        {
            if (model.selectedProduct != 0)
                Send(new ReqShopCancelPurchase());

            currentPage = null;
            model.SetPage(0, 0);
        }

        EvictPage(pageId);
        RefreshNavigation();
        if (!currentPage && loading == null)
            return OpenPage(index, Mathf.Min(tab, section.pages.Length - 1));

        return true;
    }

    /// <summary>Resolve a navigation click and request only the selected page prefab.</summary>
    public bool OpenPage(int sectionIndex, int tabIndex)
    {
        var section = GetSection(sectionIndex);
        if (section == null || tabIndex < 0 || tabIndex >= section.pages.Length || !pageHost)
            return false;

        var route = section.pages[tabIndex];
        if (loadingSection == section.id && ReferenceEquals(loading, route))
            return true;

        CancelLoad();
        if (pages.TryGetValue(route.id, out var cached) && cached)
        {
            if (cached == currentPage && model.sectionId == section.id && model.pageId == route.id)
                return true;

            return Present(cached, section.id, route);
        }

        loading = route;
        loadingSection = section.id;
        int request = generation;
        XRes.LoadAsync<GameObject>(route.path, asset => {
            if (this && isActiveAndEnabled && request == generation)
            {
                CancelLoad();
                if (asset is GameObject prefab)
                {
                    var instance = Instantiate(prefab, pageHost, false);
                    if (instance.TryGetComponent<UIShopPage>(out var content))
                    {
                        if (content.InitializePage(route) && Present(content, section.id, route))
                            pages[route.id] = content;
                        else
                            ReleasePage(content);
                    }
                    else
                    {
                        Debug.LogError($"Shop page {route.path} requires an UIShopPage component.", instance);
                        instance.SetActive(false);
                        Destroy(instance);
                    }
                }
                else
                {
                    Debug.LogError($"Could not load shop page {route.path}.", this);
                }
            }
        });

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

    /// <summary>Read the source opening gate for separately hosted product pages.</summary>
    /// <returns>True after the mall entrance allows input.</returns>
    public bool CanInteract()
    {
        return loading == null && interaction && interaction.interactable;
    }

    /// <summary>Commit navigation only after the selected page is ready.</summary>
    private bool Present(UIShopPage instance, int sectionId, ShopEntity.Page route)
    {
        bool cancelPurchase = model.selectedProduct != 0;
        instance.ShowPage();
        if (!instance.RefreshPage(model))
        {
            if (instance != currentPage)
                instance.HidePage();

            Debug.LogError($"Could not bind shop page {route.path}.", instance);

            return false;
        }

        if (currentPage && currentPage != instance)
            currentPage.HidePage();

        if (cancelPurchase)
            Send(new ReqShopCancelPurchase());

        currentPage = instance;
        model.SetPage(sectionId, route.id);
        RefreshNavigation();

        return true;
    }

    /// <summary>Rebuild navigation only when the catalog or selection changes.</summary>
    private void RefreshNavigation()
    {
        var groups = new VList<VMShopGroup>();
        var tabs = new VList<VMShopTab>();
        foreach (var section in model.sections)
        {
            bool selected = section.id == model.sectionId;
            groups.Add(new VMShopGroup {
                Title = section.title, icon = section.icon,
                Selected = selected, Unselected = !selected
            });

            if (selected)
            {
                foreach (var route in section.pages)
                    tabs.Add(new VMShopTab {
                        Title = route.title,
                        Selected = route.id == model.pageId,
                        Unselected = route.id != model.pageId
                    });
            }
        }

        vm.Groups = groups;
        vm.Tabs = tabs;
        if (tabStrip)
            tabStrip.SetActive(tabs.Count > 1);
    }

    /// <summary>Find the display position of a stable section key.</summary>
    private int IndexOfSection(int id)
    {
        for (int i = 0; model && i < model.sections.Count; i++)
        {
            if (model.sections[i].id == id)
                return i;
        }

        return -1;
    }

    /// <summary>Move relative to the most recently requested primary tab.</summary>
    private bool MoveSection(int offset)
    {
        if (!model)
            return false;

        int current = IndexOfSection(loading != null ? loadingSection : model.sectionId);

        return OpenPage(current + offset, 0);
    }

    /// <summary>Invalidate callbacks from previous selections.</summary>
    private void CancelLoad()
    {
        generation++;
        loading = null;
        loadingSection = 0;
    }

    /// <summary>Release the embedded widget before destroying its page instance.</summary>
    private void ReleasePage(UIShopPage instance)
    {
        if (instance)
        {
            instance.HidePage();
            instance.ReleasePage();

            Destroy(instance.gameObject);
        }
    }

    /// <summary>Release only the removed business page.</summary>
    private void EvictPage(int id)
    {
        if (pages.TryGetValue(id, out var instance))
        {
            pages.Remove(id);
            ReleasePage(instance);
        }
    }

    /// <summary>Discard this window's loaded pages and any pending requests.</summary>
    private void ClearPages()
    {
        CancelLoad();
        foreach (var instance in pages.Values)
            ReleasePage(instance);

        pages.Clear();
        currentPage = null;
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
        CancelLoad();
        StopAllCoroutines();

        if (interaction)
            interaction.interactable = true;
    }

    /// <summary>Release the embedded VM when the mall closes.</summary>
    private void OnDestroy()
    {
        ClearPages();
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

/// <summary>Player request for the mall close action.</summary>
public class ReqShopClose : UIRequestEvent
{
}

/// <summary>Player request for the mall help action.</summary>
public class ReqShopShowHelp : UIRequestEvent
{
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
