using Oathx.Widget;

/// <summary>Requests a local game session without claiming server authentication.</summary>
public sealed class ReqLoginEnterGame : UIRequestEvent { }

/// <summary>Shows local entry controls using serialized Widget Metadata.</summary>
[Config("Assets/Data/Widgets/Login/UILogin.prefab", WidgetLayer.dialog, cacheWidget: false)]
public sealed class UILogin : UIWidget<VMLogin>
{
    /// <summary>Projects local entry state without owning its rules.</summary>
    /// <param name="entity">Validated local entry data.</param>
    /// <returns>Whether the bound presentation was refreshed.</returns>
    public bool Invalidate(LoginEntity entity)
    {
        if (!entity || !vm)
            return false;

        vm.EnterVisible = entity.phase != LoginPhase.Entering;
        vm.LoadingVisible = entity.phase == LoginPhase.Entering;
        vm.FailedVisible = entity.phase == LoginPhase.Failed;

        return true;
    }

    /// <summary>Binds the start button once for this widget instance.</summary>
    /// <returns>Whether the entry intent is bound.</returns>
    protected override bool Bind()
    {
        vm.EnterClick = Send<ReqLoginEnterGame>;

        return true;
    }
}
