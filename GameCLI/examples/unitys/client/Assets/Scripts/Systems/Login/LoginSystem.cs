using Oathx.Core;
using Oathx.Entity;
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
                widget.vm.AnnouncementClick = (sender) => OpenNotice();

                XApp.GetSingleton().ShowLaunchBackground();
            }
        });

        return true;
    }

    /// <summary>
    /// Open the local announcement page on an explicit player request.
    /// </summary>
    /// <returns>True after requesting the asynchronous page opening.</returns>
    private bool OpenNotice()
    {
        if (!IsActive())
            return false;

        return XPluginUtility.PostEvent(new SystemActive() {
            type = typeof(NoticeSystem)
        });
    }

    /// <summary>
    /// Publish local login success when the player enters the game.
    /// </summary>
    /// <param name="vm">View model controlling the enter button.</param>
    /// <returns>True when the local login success event has been queued.</returns>
    private bool EnterGame(VMLogin vm)
    {
        if (!IsActive() || !vm || !vm.CanEnter)
            return false;

        if (!XEntityManager.GetSingleton().TryGetEntity(nameof(MasterEntity), out var entity)
            || !(entity is MasterEntity master))
            return false;

        return XPluginUtility.PostEvent(new LoginSucceed() {
            pid = master.GetAttr(MasterPropertyDef.PID)
        });
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
