using Oathx.Core;
using Oathx.Widget;

/// <summary>
/// Manages the SV login page lifecycle.
/// </summary>
public class LoginSystem : RuntimeSystem
{
    /// <summary>
    /// Load and open the SV login page through UISystem.
    /// </summary>
    /// <returns>True after starting the page open request; false if the system is inactive.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        UISystem.GetSingleton().OpenWidget<UILogin>((widget) => {
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
    /// Handle the enter button until the replacement login flow is connected.
    /// </summary>
    /// <param name="vm">View model controlling the enter button.</param>
    /// <returns>False because no login operation is currently connected.</returns>
    private bool EnterGame(VMLogin vm)
    {
        if (!IsActive() || !vm || !vm.CanEnter)
            return false;

        // Keep the button available while login integration is pending.
        return false;
    }

    /// <summary>
    /// Release subscriptions and close the SV login page when deactivated.
    /// </summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        UISystem.GetSingleton().CloseWidget<UILogin>(immediate: true);
    }
}
