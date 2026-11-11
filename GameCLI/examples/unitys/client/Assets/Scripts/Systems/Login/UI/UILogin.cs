using Oathx.Widget;

/// <summary>
/// SV login view with serialized Widget Metadata bindings.
/// </summary>
[Config("Assets/Data/Widgets/SVLogin/SVLogin.prefab", WidgetLayer.center, cacheWidget: false)]
public class UILogin : UIWidget<VMLogin>
{
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

        vm.Username = entity.pid > 0 ? entity.pid.ToString() : string.Empty;

        return true;
    }
}
