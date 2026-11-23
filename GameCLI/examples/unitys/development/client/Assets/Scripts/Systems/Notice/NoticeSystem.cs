using Oathx.Entity;
using Oathx.Core;
using Oathx.Widget;

/// <summary>
/// Manages local announcement content and the notice page lifecycle.
/// </summary>
public class NoticeSystem : RuntimeSystem
{
    /// <summary>
    /// Open the notice page and initialize its local content and close action.
    /// </summary>
    /// <returns>True after requesting page opening while active.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        UnsubscribeAll();

        Subscribe<ReqNoticeClose>(request => CloseNotice());
        Subscribe<ReqNoticeToggleReminder>(request => ToggleReminder());

        UISystem.GetSingleton().OpenWidget<UINotice>((widget) => {
            if (widget)
            {
                var entity = XEntityManager.GetSingleton().QueryEntity<NoticeEntity>();
                widget.Invalidate(entity);
            }
        });

        return true;
    }

    /// <summary>
    /// Release subscriptions and close the notice page when deactivated.
    /// </summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UINotice)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>
    /// Ask MasterSystem to unload the notice after an explicit close request.
    /// </summary>
    /// <returns>True when the unload request was queued.</returns>
    private bool CloseNotice()
    {
        if (!IsActive())
            return false;

        return XPluginUtility.PostEvent(new SystemDetive() {
            type = typeof(NoticeSystem)
        });
    }

    /// <summary>Change the persisted reminder setting and refresh the open notice.</summary>
    private bool ToggleReminder()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<NoticeEntity>();
        if (!entity || !UISystem.GetSingleton().TryGetWidget<UINotice>(out var widget))
            return false;

        entity.ToggleReminder();

        return widget.Invalidate(entity);
    }
}
