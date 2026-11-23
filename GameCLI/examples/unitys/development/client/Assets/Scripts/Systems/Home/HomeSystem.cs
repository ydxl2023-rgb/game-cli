using Config;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>Coordinates lobby data and emits local navigation requests.</summary>
public class HomeSystem : RuntimeSystem
{
    /// <summary>Request the lobby while this system is active.</summary>
    /// <returns>True when page loading was requested.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        UnsubscribeAll();

        Subscribe<ReqHomeOpenDestination>(data => OpenEntry(data.entry));
        Subscribe<ReqHomeOpenActivity>(data => OpenActivity(data.systemId));
        Subscribe<FunctionOpenChanged>(OnFunctionChanged);
        Subscribe<FunctionInputsUpdated>(OnInputsUpdated);

        UISystem.GetSingleton().OpenWidget<UIHome>((widget) => {
            if (widget)
                Refresh();
        });

        return true;
    }

    /// <summary>Release subscriptions and close the owned page.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UIHome)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>Refresh the open lobby from the current entity.</summary>
    /// <returns>True when the visible page was refreshed.</returns>
    public bool Refresh()
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!entity)
            return false;

        var functions = XEntityManager.GetSingleton().QueryEntity<FunctionManagerEntity>();
        if (!functions)
            return false;

        entity.playerLevel = functions.level;

        if (!widget.Invalidate(entity, functions))
            return false;

        XPluginUtility.FireEvent(new HomePresentationRequested() {
            characterKey = entity.characterKey,
            seasonKey = entity.seasonKey
        });

        return true;
    }

    /// <summary>Emit a navigation request without starting an unimplemented system.</summary>
    /// <param name="entry">The destination requested by the caller.</param>
    /// <returns>True when the local request was emitted.</returns>
    public bool OpenEntry(HomeEntry entry)
    {
        if (!IsActive() || !System.Enum.IsDefined(typeof(HomeEntry), entry))
            return false;

        XPluginUtility.FireEvent(new HomeEntryRequested() {
            entry = entry
        });

        return true;
    }

    /// <summary>Request an available activity through the system coordinator.</summary>
    /// <param name="id">FunctionSystem identifier.</param>
    /// <returns>True when an available, implemented target was requested.</returns>
    public bool OpenActivity(int id)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<FunctionManagerEntity>();
        if (!entity)
            return false;

        if (!IsActive() || !entity.IsOpen(id)
            || !Excel.functionSystemConfig.TryGetValue(id, out var row))
            return false;

        var type = FunctionManagerAccess.ResolveType(row);
        if (type == null)
            return false;

        return XPluginUtility.PostEvent(new SystemActive { type = type });
    }

    /// <summary>Refresh entries after a configured availability transition.</summary>
    /// <param name="data">Changed function.</param>
    private void OnFunctionChanged(FunctionOpenChanged data)
    {
        Refresh();
    }

    /// <summary>Refresh only after the availability service has applied the input message.</summary>
    /// <param name="data">Completed input update notification.</param>
    private void OnInputsUpdated(FunctionInputsUpdated data)
    {
        Refresh();
    }
}
