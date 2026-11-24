using Oathx.Core;
using Oathx.Widget;

/// <summary>Requests the map without starting an unimplemented battle.</summary>
public sealed class ReqHomeMap : IEvent, IUIRequest
{
    /// <summary>Originating widget registration.</summary>
    public string sourceKey
    { get; set; }
}

/// <summary>Requests a return to the camp presentation.</summary>
public sealed class ReqHomeCamp : IEvent, IUIRequest
{
    /// <summary>Originating widget registration.</summary>
    public string sourceKey
    { get; set; }
}

/// <summary>Owns bindings and local presentation for the Home page.</summary>
[Config("Assets/Data/Widgets/Home/Home.prefab", WidgetLayer.center, cacheWidget: false)]
public sealed class UIHome : UIWidget<VMHome>
{
    /// <summary>Bind each intent once for this widget instance.</summary>
    /// <returns>True after binding the two implemented actions.</returns>
    protected override bool Bind()
    {
        vm.MapClick = Send<ReqHomeMap>;
        vm.ReturnClick = Send<ReqHomeCamp>;
        ShowMap(false);

        return true;
    }

    /// <summary>Switch the local presentation without changing player progression.</summary>
    /// <param name="visible">Whether the map should be visible.</param>
    /// <returns>True when the current widget is bound.</returns>
    public bool ShowMap(bool visible)
    {
        if (!vm)
            return false;

        vm.CampVisible = !visible;
        vm.MapVisible = visible;

        return true;
    }
}
