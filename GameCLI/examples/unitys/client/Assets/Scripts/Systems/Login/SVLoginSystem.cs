using Oathx.Core;
using Oathx.Widget;

/// <summary>
/// Manages the SV login page lifecycle.
/// </summary>
public class SVLoginSystem : RuntimeSystem
{
    /// <summary>
    /// Load and open the SV login page through UISystem.
    /// </summary>
    /// <returns>True after starting the page open request; false if the system is inactive.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        UISystem.GetSingleton().OpenWidget<UISVLogin>((widget) => {
            if (widget)
            {
                widget.vm.EnterClick = (sender) => EnterGame(widget.vm);
                widget.vm.CanEnter = true;

                XApp.GetSingleton().ShowLaunchBackground();
            }
        });

        return true;
    }

    /// <summary>
    /// Start the existing login system once from the login page.
    /// </summary>
    /// <param name="vm">View model controlling the enter button.</param>
    /// <returns>True when login was started; false when the click was rejected.</returns>
    private bool EnterGame(VMSVLogin vm)
    {
        if (!IsActive() || !vm || !vm.CanEnter)
            return false;

        vm.CanEnter = false;

        LoadSystem(typeof(LoginSystem));
        return true;
    }

    /// <summary>
    /// Release subscriptions and close the SV login page when deactivated.
    /// </summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        UISystem.GetSingleton().CloseWidget<UISVLogin>(immediate: true);
    }
}
