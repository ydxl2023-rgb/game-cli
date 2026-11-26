using System.Linq;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Inspects actual trading UI and exercises configured offers without hidden business shortcuts.</summary>
public static class MiniwowShopProbe
{
    /// <summary>Prepares an isolated post-repair onboarding fixture from the saved user checkpoint.</summary>
    public static void PrepareReplay()
    {
        MiniwowHospitalProbe.PrepareReplay();
        string key = StorageKey.For<TaskSystem, TaskCollectionEntity>(entity => entity.chapterId);
        var tasks = JsonConvert.DeserializeObject<TaskCollectionSave>(PlayerPrefs.GetString(key));
        var repair = tasks.tasks.Single(task => task.id == 60101);
        repair.status = TaskStatus.Claimed;
        repair.progress = 1;
        PlayerPrefs.SetString(key, JsonConvert.SerializeObject(tasks));
        string villageKey = StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village);
        var village = JsonConvert.DeserializeObject<HomeVillageSave>(PlayerPrefs.GetString(villageKey));
        village.buildings[60018] = 1;
        village.receipts.Add(60101);
        PlayerPrefs.SetString(villageKey, JsonConvert.SerializeObject(village));
        PlayerPrefs.Save();
    }

    /// <summary>Exercises transaction rejections using isolated persistence and restores it exactly.</summary>
    /// <returns>Validated rule outcomes.</returns>
    public static string VerifyRules()
    {
        string key = StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village);
        string saved = PlayerPrefs.GetString(key);
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var offer = Config.Excel.shopGoodsConfig[6001];
        try
        {
            var data = JsonConvert.DeserializeObject<HomeVillageSave>(saved);
            data.items[offer.CurrencyID] = 29;
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(data));
            var inventory = new HomeVillageEntity(0);
            var shop = new ShopEntity();
            shop.Select(6001);
            var poor = shop.Confirm(inventory, tasks);
            if (poor != ShopPurchaseResult.NoGold || inventory.Count(offer.CurrencyID) != 29)
                throw new System.InvalidOperationException("Insufficient balance modified inventory.");

            shop.ChangeQuantity(1000, inventory);
            if (shop.quantity != Config.Excel.shopSettingConfig[60019].MaxQuantity)
                throw new System.InvalidOperationException("Per-order cap failed.");

            data.items[offer.CurrencyID] = 100;
            data.shopPurchases[6001] = offer.Limit;
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(data));
            inventory = new HomeVillageEntity(0);
            shop.Select(6001);
            var sold = shop.Confirm(inventory, tasks);
            if (sold != ShopPurchaseResult.SoldOut || inventory.Count(offer.CurrencyID) != 100)
                throw new System.InvalidOperationException("Exhausted stock modified inventory.");

            data.shopPurchases[6001] = 0;
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(data));
            inventory = new HomeVillageEntity(0);
            long before = inventory.Count(offer.ItemID);
            shop.Select(6001);
            var first = shop.Confirm(inventory, tasks);
            var duplicate = shop.Confirm(inventory, tasks);
            if (first != ShopPurchaseResult.Success || duplicate != ShopPurchaseResult.NoSelection
                || inventory.Count(offer.CurrencyID) != 70 || inventory.Count(offer.ItemID) != before + 1)
                throw new System.InvalidOperationException("Duplicate confirmation changed the committed transaction.");

            var restored = new HomeVillageEntity(0);
            if (restored.Count(offer.CurrencyID) != 70 || restored.Purchased(offer) != 1)
                throw new System.InvalidOperationException("Transaction did not survive restore.");

            return JsonConvert.SerializeObject(new {
                poor, sold, first, duplicate, restore = true, quantityCap = true
            });
        }
        finally
        {
            PlayerPrefs.SetString(key, saved);
            PlayerPrefs.Save();
        }
    }

    /// <summary>Unloads the hospital through the production coordinator.</summary>
    public static void UnloadHospital()
    {
        XPluginUtility.FireEvent(new SystemDetive { id = 10006 });
    }

    /// <summary>Reinstalls the hospital without reopening Home.</summary>
    public static void ReloadHospital()
    {
        XPluginUtility.FireEvent(new SystemActive { id = 10006 });
    }

    /// <summary>Exercises the production failed-session return path.</summary>
    public static void FailReturn()
    {
        XPluginUtility.FireEvent(new ReturnToLogin { failed = true });
    }

    /// <summary>Opens the configured trading system through the production event route.</summary>
    public static void Open()
    {
        XPluginUtility.FireEvent(new SystemActive { id = 10005, complete = new ShopOpenRequested() });
    }

    /// <summary>Clicks a real offer button using the existing graphic-raycast test.</summary>
    /// <param name="id">Stable original offer identity.</param>
    /// <returns>Whether the actual control received a click.</returns>
    public static bool ClickOffer(int id)
    {
        var card = Object.FindObjectsOfType<UIShopOffer>()
            .Single(item => ((VMShopOffer)item.GetVM()).offerId == id);
        var button = card.GetComponentInChildren<Button>();
        string name = button.name;
        button.name = "SelectedOffer";
        try
        {
            return MiniwowHospitalProbe.Click(button.name);
        }
        finally
        {
            button.name = name;
        }
    }

    /// <summary>Reports current economy and actual native layout geometry.</summary>
    /// <returns>The detached diagnostic snapshot.</returns>
    public static string Snapshot()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var shop = Object.FindObjectOfType<UIShop>();
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();

        return JsonConvert.SerializeObject(new {
            village = home.village.Snapshot(),
            task = tasks.GetTrackedTask().configuration.ID,
            status = tasks.GetTrackedTask().status.ToString(),
            selected = entity.selectedId,
            quantity = entity.quantity,
            cards = shop.GetComponentsInChildren<UIShopOffer>().Select(card => new {
                id = ((VMShopOffer)card.GetVM()).offerId,
                size = ((RectTransform)card.transform).rect.ToString(),
                position = card.transform.localPosition.ToString(),
                scale = card.transform.localScale.ToString()
            }),
            spine = shop.GetComponentsInChildren<SkeletonGraphic>().Select(graphic => new {
                scale = graphic.transform.localScale.ToString(),
                position = graphic.transform.localPosition.ToString(),
                width = graphic.Skeleton.Data.Width,
                height = graphic.Skeleton.Data.Height,
                materials = graphic.canvasRenderer.materialCount
            })
        });
    }
}
