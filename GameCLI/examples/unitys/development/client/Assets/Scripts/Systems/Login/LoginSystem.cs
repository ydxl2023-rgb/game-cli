using System.Collections.Generic;
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

        UnsubscribeAll();

        Subscribe<ReqLoginEnterGame>(request => EnterGame());
        Subscribe<ReqLoginOpenServers>(request => OpenServers());
        Subscribe<ReqLoginOpenNotice>(request => OpenNotice());
        Subscribe<ReqLoginCloseServers>(request => CloseServers());
        Subscribe<ReqLoginSelectCategory>(request => SelectCategory(request.index));
        Subscribe<ReqLoginSelectServer>(request => SelectServer(request.serverId));

        UISystem.GetSingleton().OpenWidget<UILogin>((widget) => {
            if (widget)
            {
                InitializeServers();

                XApp.GetSingleton().ShowLaunchBackground();
            }
        });

        return true;
    }

    /// <summary>
    /// Release subscriptions and close the SV login page when deactivated.
    /// </summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UIServerSelect), 
            typeof(UILogin)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>Replace the local catalog with caller-supplied server definitions.</summary>
    /// <param name="entries">Unique server definitions supplied by the owner.</param>
    /// <returns>True when the nonempty catalog has been accepted.</returns>
    public bool SetServers(IEnumerable<ServerEntry> entries)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (!entity)
            return false;

        if (!entity.SetServers(entries))
            return false;

        if (IsActive())
        {
            CloseServers();
            InitializeServers();
        }

        return true;
    }

    /// <summary>Initialize the explicitly local test catalog and restore selection.</summary>
    /// <returns>True when the opened login page was refreshed.</returns>
    private bool InitializeServers()
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UILogin>(out var widget))
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (!entity)
            return false;

        if (entity.serverCount == 0)
        {
            var servers = new List<ServerEntry>();
            for (int i = 1; i <= 12; i++)
                servers.Add(new ServerEntry() {
                    id = i,
                    name = CommonText.Get(Config.Excel.LOGIN_LOCAL_SERVER_NAME, i),
                    state = i == 3 ? ServerState.Maintenance : i == 2 ? ServerState.Busy : ServerState.Normal,
                    recommended = i <= 2
                });

            if (!entity.SetServers(servers))
                return false;
        }

        if (!entity.RestoreSelection())
            return false;

        return widget.Invalidate(entity);
    }

    /// <summary>Open the server selector while Login owns its lifecycle.</summary>
    /// <returns>True when the opening request has been submitted.</returns>
    private bool OpenServers()
    {
        if (!IsActive())
            return false;

        return UISystem.GetSingleton().OpenWidget<UIServerSelect>((widget) => {
            if (widget)
            {
                var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
                widget.Invalidate(entity);
            }
        });
    }

    /// <summary>Refresh the requested server category.</summary>
    private bool SelectCategory(int index)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIServerSelect>(out var widget))
            return false;

        return widget.SelectCategory(index);
    }

    /// <summary>Apply a selected server and return to the login page.</summary>
    /// <param name="serverId">Selected local server identifier.</param>
    /// <returns>True when the selection was saved and the login page was refreshed.</returns>
    private bool SelectServer(int serverId)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UILogin>(out var widget))
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (!entity)
            return false;

        if (!entity.SelectServer(serverId))
            return false;

        widget.Invalidate(entity);

        CloseServers();

        return true;
    }

    /// <summary>Close the server selector without changing selection.</summary>
    /// <returns>True after requesting closure.</returns>
    private bool CloseServers()
    {
        System.Type[] widgets = new System.Type[] {
            typeof(UIServerSelect)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);

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
    /// <returns>True when the local login success event has been queued.</returns>
    private bool EnterGame()
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UILogin>(out var widget))
            return false;

        var login = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (!login)
            return false;

        if (!login.CanEnter())
            return false;

        if (!XEntityManager.GetSingleton().TryGetEntity(nameof(MasterEntity), out var entity)
            || !(entity is MasterEntity master) || !master)
            return false;

        bool posted = XPluginUtility.PostEvent(new LoginSucceed() {
            pid = master.GetAttr(MasterPropertyDef.PID)
        });

        if (posted)
        {
            login.RecordLoginServer();
        }

        return posted;
    }

}
