using Oathx.Widget;

/// <summary>
/// SV login view with serialized Widget Metadata bindings.
/// </summary>
[Config("Assets/Data/Widgets/SVLogin/SVLogin.prefab", WidgetLayer.center, cacheWidget: false)]
public class UILogin : UIWidget<VMLogin>
{
    /// <summary>Bind player actions when the page opens.</summary>
    protected override bool Bind()
    {
        vm.EnterClick = Send<ReqLoginEnterGame>;
        vm.ChangeClick = Send<ReqLoginOpenServers>;
        vm.AnnouncementClick = Send<ReqLoginOpenNotice>;

        return true;
    }

    /// <summary>Render the server state using this page's display labels.</summary>
    /// <param name="state">Availability state supplied by the login flow.</param>
    /// <returns>True when the bound view model was updated.</returns>
    public bool SetServerState(ServerState state)
    {
        if (!vm)
            return false;

        switch (state)
        {
            case ServerState.Normal:
                vm.ServerStatus = CommonText.Get(Config.Excel.LOGIN_SERVER_NORMAL);
                break;
            case ServerState.Busy:
                vm.ServerStatus = CommonText.Get(Config.Excel.LOGIN_SERVER_BUSY);
                break;
            case ServerState.Maintenance:
                vm.ServerStatus = CommonText.Get(Config.Excel.LOGIN_SERVER_MAINTENANCE);
                break;
            default:
                vm.ServerStatus = string.Empty;
                break;
        }

        return true;
    }

    /// <summary>
    /// Refresh the account display from the existing login entity.
    /// </summary>
    /// <param name="entity">Current login data.</param>
    /// <returns>True when the view model was refreshed.</returns>
    public bool Invalidate(LoginEntity entity)
    {
        if (!entity || !vm)
            return false;

        var entry = entity.GetSelectedServer();
        if (entry != null)
        {
            vm.ServerName = entry.name;
            SetServerState(entry.state);
        }
        vm.CanEnter = entity.CanEnter();
        vm.Username = entity.pid > 0 ? entity.pid.ToString() : string.Empty;

        return true;
    }
}

/// <summary>Request entry into the game.</summary>
public sealed class ReqLoginEnterGame : UIRequestEvent
{
}

/// <summary>Request the server selector.</summary>
public sealed class ReqLoginOpenServers : UIRequestEvent
{
}

/// <summary>Request the login notice.</summary>
public sealed class ReqLoginOpenNotice : UIRequestEvent
{
}

/// <summary>Dismiss the server selector.</summary>
public sealed class ReqLoginCloseServers : UIRequestEvent
{
}

/// <summary>Select a server category.</summary>
public sealed class ReqLoginSelectCategory : UIRequestEvent
{
    /// <summary>Selected category index.</summary>
    public int index
    { get; set; }
}

/// <summary>Select a server by its catalog identifier.</summary>
public sealed class ReqLoginSelectServer : UIRequestEvent
{
    /// <summary>Selected server identifier.</summary>
    public int serverId
    { get; set; }
}
