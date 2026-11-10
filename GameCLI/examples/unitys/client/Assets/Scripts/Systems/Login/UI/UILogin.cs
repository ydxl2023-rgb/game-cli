using Oathx.Widget;

/// <summary>
/// SV login view with serialized Widget Metadata bindings.
/// </summary>
[Config("Assets/Resources/UI/SVLogin/SVLogin.prefab", WidgetLayer.center, cacheWidget: false)]
public class UILogin : UIWidget<VMLogin>
{
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
