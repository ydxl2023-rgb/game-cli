using Oathx.Widget;

/// <summary>Drive the reference project's bound entry actions and observe actual Home activations.</summary>
public sealed class SystemScenarioProbe : IUIEventSender
{
    private static SystemScenarioProbe observer;

    private int opened;

    /// <summary>Wait until login has bound its entry action before starting a fresh scenario.</summary>
    public static bool LoginReady()
    {
        return UISystem.GetSingleton().TryGetWidget<UILogin>(out var login) && login.vm && login.vm.EnterClick != null;
    }

    /// <summary>Observe Home before dispatching the normal local login action once.</summary>
    public static bool EnterGame()
    {
        var ui = UISystem.GetSingleton();
        if (!LoginReady() || ui.TryGetWidget<UIHome>(out var home))
            return false;

        if (observer != null)
            ui.UnregisterEventSender(observer);

        observer = new SystemScenarioProbe();
        ui.RegisterEventSender(observer);
        ui.TryGetWidget<UILogin>(out var login);

        return login.vm.EnterClick(null);
    }

    /// <summary>Read the observed Home opening count without triggering navigation.</summary>
    public static int HomeOpenings()
    {
        return observer == null ? -1 : observer.opened;
    }

    /// <summary>Follow the lobby's bound shop entry.</summary>
    public static bool OpenShop()
    {
        return UISystem.GetSingleton().TryGetWidget<UIHome>(out var home) && home.vm && home.vm.ShopEntry &&
            home.vm.ShopEntry.OpenClick != null && home.vm.ShopEntry.OpenClick(null);
    }

    /// <summary>Wait for the shop to bind its navigation action.</summary>
    public static bool ShopReady()
    {
        return UISystem.GetSingleton().TryGetWidget<UIShop>(out var shop) && shop.vm && shop.vm.GroupClick != null;
    }

    public void OnWidgetOpened(IWidget widget)
    {
        if (widget is UIHome)
            opened++;
    }

    public void OnWidgetClosing(IWidget widget)
    {
        // Closing does not count as an activation.
    }
}
