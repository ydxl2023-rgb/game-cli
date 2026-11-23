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



