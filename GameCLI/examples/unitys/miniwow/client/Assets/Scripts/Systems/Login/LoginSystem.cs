using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>Requests restoration of the local entry controls.</summary>
public sealed class LoginEntryResetRequested : IEvent
{
    /// <summary>Whether retry feedback should be shown.</summary>
    public bool failed
    { get; set; }
}

/// <summary>Owns the local entry page and submits accepted session intentions.</summary>
public sealed class LoginSystem : RuntimeSystem
{
    /// <summary>Creates entry data and opens the local login page.</summary>
    /// <returns>Whether opening was requested.</returns>
    public override bool OnActive()
    {
        Subscribe<ReqLoginEnterGame>(OnEnterGame);
        Subscribe<LoginEntryResetRequested>(OnLoginEntryReset);

        var login = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        login.Reset(false);

        return UISystem.GetSingleton().OpenWidget<UILogin>(OnLoginOpened);
    }

    /// <summary>Invalidates pending callbacks and closes only the owned page.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UILogin)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>Reports actual page binding completion.</summary>
    /// <returns>Whether this active entry page is ready.</returns>
    public override bool IsReady()
    {
        return IsActive() && UISystem.GetSingleton().TryGetWidget<UILogin>(out var widget) && widget.vm;
    }

    /// <summary>Accepts a local entry request after checking entity state.</summary>
    /// <returns>Whether the request was queued for Master.</returns>
    public bool EnterGame()
    {
        if (!IsReady())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (!entity.TryBegin())
            return false;

        Refresh();

        bool posted = XPluginUtility.PostEvent(new LoginSucceed());
        if (!posted)
            ResetEntry(true);

        return posted;
    }

    /// <summary>Restores the entry control after return or failed startup.</summary>
    /// <param name="failed">Whether to display retry feedback.</param>
    public void ResetEntry(bool failed)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        entity.Reset(failed);

        Refresh();
    }

    /// <summary>Refreshes the bound entry page and releases the launch overlay.</summary>
    /// <param name="widget">The opened login page.</param>
    private void OnLoginOpened(UILogin widget)
    {
        var login = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (widget.Invalidate(login))
            XApp.GetSingleton().HideLaunch();
    }

    /// <summary>Handles the ReqLoginEnterGame event.</summary>
    /// <param name="message">The event payload.</param>
    private void OnEnterGame(ReqLoginEnterGame message)
    {
        EnterGame();
    }

    /// <summary>Handles the LoginEntryResetRequested event.</summary>
    /// <param name="message">The event payload.</param>
    private void OnLoginEntryReset(LoginEntryResetRequested message)
    {
        ResetEntry(message.failed);
    }

    /// <summary>Refreshes the opened widget from its current entity.</summary>
    /// <returns>Whether a live widget accepted the snapshot.</returns>
    private bool Refresh()
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UILogin>(out var widget))
            return false;

        var login = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();

        return widget.Invalidate(login);
    }
}
