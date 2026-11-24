using System;
using System.Collections.Generic;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Temporary read-only UI evidence and isolated local transaction tests.</summary>
public static class ShopTemplateProbe
{
    /// <summary>Exercise transaction invariants with isolated entity instances.</summary>
    /// <returns>Passed invariant names; an assertion throws on failure.</returns>
    public static string TestEntity()
    {
        var passed = new List<string>();
        var entity = new ShopEntity();
        var product = new ShopProduct { id = 1, category = ShopCategory.Gift, price = 600, reward = 100, remaining = 1 };
        Require(entity.InitializeCatalog(new[] { product }, 1000), "catalog");
        Require(entity.SelectProduct(1), "selection");
        entity.CancelPurchase();
        Require(entity.demoBalance == 1000 && entity.demoTokens == 0 && entity.products[0].remaining == 1, "cancel is inert");
        passed.Add("cancel");
        Require(entity.SelectProduct(1) && entity.ConfirmPurchase() == ShopPurchaseResult.Succeeded, "purchase");
        Require(entity.demoBalance == 400 && entity.demoTokens == 100 && entity.products[0].remaining == 0, "atomic transaction");
        passed.Add("purchase");
        Require(entity.ConfirmPurchase() == ShopPurchaseResult.NoSelection && entity.demoBalance == 400 && entity.demoTokens == 100, "duplicate");
        passed.Add("duplicate confirmation");
        Require(!entity.SelectProduct(1), "sold out");
        passed.Add("sold out selection");
        Require(entity.InitializeCatalog(new[] { product }, 100), "reset");
        Require(entity.SelectProduct(1) && entity.ConfirmPurchase() == ShopPurchaseResult.InsufficientBalance, "insufficient");
        Require(entity.demoBalance == 100 && entity.demoTokens == 0 && entity.products[0].remaining == 1, "insufficient is inert");
        passed.Add("insufficient balance");
        Require(entity.SelectProduct(1) && entity.SelectCategory(ShopCategory.Gift) && entity.ConfirmPurchase() == ShopPurchaseResult.NoSelection, "category cancels pending");
        passed.Add("category cancels pending");
        Require(!entity.InitializeCatalog(new[] { product, product }, 1000) && entity.demoBalance == 100, "duplicate catalog rejected atomically");
        passed.Add("invalid catalog");
        Require(!entity.SelectCategory((ShopCategory)999), "invalid category");
        passed.Add("invalid category");

        return Newtonsoft.Json.JsonConvert.SerializeObject(passed);
    }

    /// <summary>Request the mall through the actual Master dispatcher.</summary>
    /// <returns>True after posting the local activation.</returns>
    public static bool Open()
    {
        XPluginUtility.PostEvent(new SystemDetive { type = typeof(LoginSystem) });
        XPluginUtility.PostEvent(new SystemDetive { type = typeof(HomeSystem) });
        XApp.GetSingleton().ShowLaunchBackground();
        XPluginUtility.PostEvent(new SystemActive { type = typeof(ShopSystem) });

        return true;
    }

    /// <summary>Open Home to exercise its real mall navigation entry.</summary>
    /// <returns>True after posting the reference navigation setup.</returns>
    public static bool OpenHome()
    {
        XPluginUtility.PostEvent(new SystemDetive { type = typeof(LoginSystem) });
        XPluginUtility.PostEvent(new SystemDetive { type = typeof(ShopSystem) });
        XApp.GetSingleton().ShowLaunchBackground();
        XPluginUtility.PostEvent(new SystemActive { type = typeof(HomeSystem) });

        return true;
    }

    /// <summary>Read actual rendered controls, hit targets and default Animator state.</summary>
    /// <returns>The visible mall evidence.</returns>
    public static string Inspect()
    {
        var widgets = new List<IWidget>();
        if (UISystem.GetSingleton().TryGetWidget<UIShop>(out var main)) widgets.Add(main);
        if (UISystem.GetSingleton().TryGetWidget<UIShopRecharge>(out var recharge)) widgets.Add(recharge);
        if (UISystem.GetSingleton().TryGetWidget<UIShopGift>(out var gift)) widgets.Add(gift);
        if (UISystem.GetSingleton().TryGetWidget<UIShopPurchase>(out var purchase)) widgets.Add(purchase);
        var rows = new List<object>();
        foreach (var widget in widgets)
        {
            var texts = new List<object>();
            foreach (var text in widget.GetComponentsInChildren<Text>()) texts.Add(new { node = Path(text.transform), value = text.text });
            var buttons = new List<object>();
            foreach (var button in widget.GetComponentsInChildren<Button>())
            {
                var pointer = Pointer(button);
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                var hit = hits.Count > 0 ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) : null;
                buttons.Add(new { node = Path(button.transform), interactable = button.IsInteractable(), hit = hit ? Path(hit.transform) : null });
            }
            var animations = new List<object>();
            foreach (var animator in widget.GetComponentsInChildren<Animator>()) animations.Add(new { node = Path(animator.transform), controller = animator.runtimeAnimatorController ? animator.runtimeAnimatorController.name : null, time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime });
            rows.Add(new { widget = widget.GetType().Name, texts, buttons, animations });
        }

        return Newtonsoft.Json.JsonConvert.SerializeObject(rows);
    }

    /// <summary>Send a click only when the named button is actually the top raycast target.</summary>
    /// <param name="suffix">Unique hierarchy suffix from Inspect.</param>
    /// <returns>True when the pointer reached the intended button.</returns>
    public static bool Click(string suffix)
    {
        foreach (var button in UnityEngine.Object.FindObjectsOfType<Button>())
        {
            if (!Path(button.transform).EndsWith(suffix, StringComparison.Ordinal)) continue;
            var pointer = Pointer(button);
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            var hit = hits.Count > 0 ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) : null;
            if (hit != button.gameObject) return false;
            ExecuteEvents.Execute(hit, pointer, ExecuteEvents.pointerClickHandler);

            return true;
        }

        return false;
    }

    /// <summary>Switch category through the system for data inspection.</summary>
    /// <param name="category">Declared category index.</param>
    /// <returns>The operation result.</returns>
    public static bool SelectCategory(int category)
    {
        return XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<ShopSystem>().SelectCategory((ShopCategory)category);
    }

    /// <summary>Read entity values after real UI actions.</summary>
    /// <returns>The local transaction state.</returns>
    public static string State()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();

        return Newtonsoft.Json.JsonConvert.SerializeObject(new { entity.category, entity.selectedProduct, entity.demoBalance, entity.demoTokens, entity.purchaseCount, entity.result });
    }

    /// <summary>Inspect a remaining transparent icon hit target.</summary>
    /// <returns>Graphic state for diagnosis.</returns>
    public static string IconDetails()
    {
        foreach (var button in UnityEngine.Object.FindObjectsOfType<Button>())
        {
            if (button.name != "Icon2") continue;
            var image = button.GetComponent<Image>();
            var groups = new List<object>();
            foreach (var group in button.GetComponentsInParent<CanvasGroup>())
                groups.Add(new { group.name, group.alpha, group.interactable, group.blocksRaycasts });
            var hits = new List<RaycastResult>();
            var pointer = Pointer(button);
            EventSystem.current.RaycastAll(pointer, hits);

            return Newtonsoft.Json.JsonConvert.SerializeObject(new { path = Path(button.transform), image.raycastTarget, image.enabled, color = image.color.ToString(), image.canvasRenderer.cull, image.canvasRenderer.cullTransparentMesh, scale = image.transform.lossyScale.ToString(), position = pointer.position.ToString(), groups, hits = hits.ConvertAll(h => Path(h.gameObject.transform)) });
        }

        return "missing";
    }

    /// <summary>Validate one isolated test assertion.</summary>
    private static void Require(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException(message);
    }

    /// <summary>Get an evidence-only hierarchy path.</summary>
    private static string Path(Transform target)
    {
        return target.parent ? Path(target.parent) + "/" + target.name : target.name;
    }

    /// <summary>Build an actual screen-space pointer at a control center.</summary>
    private static PointerEventData Pointer(Button button)
    {
        var rect = (RectTransform)button.transform;
        var canvas = button.GetComponentInParent<Canvas>().rootCanvas;
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        return new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
    }
}
