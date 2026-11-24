using UnityEngine;

/// <summary>Display lifecycle of one independently registered mall destination.</summary>
public class UIShopPage : MonoBehaviour
{
    /// <summary>Current destination assigned by the shop navigation.</summary>
    public int id
    { get; private set; }

    /// <summary>Prepare the newly created page before its first presentation.</summary>
    public virtual bool InitializePage(ShopEntity.Page page)
    {
        if (page == null || !AcceptsRoute(page.id))
            return false;

        id = page.id;

        return true;
    }

    /// <summary>Check whether this instance can display the requested business destination.</summary>
    public bool AcceptsRoute(int key)
    {
        return key > 0 && (id == 0 || id == key);
    }

    /// <summary>Refresh the visible page from the current transaction state.</summary>
    public virtual bool RefreshPage(ShopEntity entity)
    {
        return entity;
    }

    /// <summary>Show this page when navigation selects it.</summary>
    public virtual void ShowPage()
    {
        gameObject.SetActive(true);
    }

    /// <summary>Hide this page while retaining its local presentation.</summary>
    public virtual void HidePage()
    {
        gameObject.SetActive(false);
    }

    /// <summary>Release resources owned by this page before destruction.</summary>
    public virtual void ReleasePage()
    {
    }
}
