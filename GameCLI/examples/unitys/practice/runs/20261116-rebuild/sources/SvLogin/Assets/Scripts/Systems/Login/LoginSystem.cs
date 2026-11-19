using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>
/// Manages the SV login page lifecycle.
/// </summary>
public class LoginSystem : RuntimeSystem
{
    /// <summary>Local server catalog until the server list service is connected.</summary>
    private readonly List<ServerEntry> servers = new List<ServerEntry>();

    /// <summary>Server categories owned by the login flow.</summary>
    private readonly List<List<ServerEntry>> categories = new List<List<ServerEntry>>();

    /// <summary>Category labels matching the category data.</summary>
    private readonly List<string> categoryTitles = new List<string>();

    /// <summary>Current category index.</summary>
    private int categoryIndex;

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
                var vm = widget.vm;
                if (!vm)
                    return;

                vm.EnterClick = (sender) => EnterGame();
                vm.ChangeClick = (sender) => OpenServers();
                vm.AnnouncementClick = (sender) => OpenNotice();

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
        if (entries == null)
            return false;

        var catalog = new List<ServerEntry>();
        var ids = new HashSet<int>();
        foreach (var entry in entries)
        {
            if (entry == null || entry.id <= 0 || string.IsNullOrWhiteSpace(entry.name)
                || !System.Enum.IsDefined(typeof(ServerState), entry.state) || !ids.Add(entry.id))
                return false;

            catalog.Add(entry);
        }

        if (catalog.Count == 0)
            return false;

        servers.Clear();
        servers.AddRange(catalog);

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

        var vm = widget.vm;
        if (!vm)
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (!entity)
            return false;

        if (servers.Count == 0)
        {
            for (int i = 1; i <= 12; i++)
                servers.Add(new ServerEntry() {
                    id = i,
                    name = CommonText.Get(Config.Excel.LOGIN_LOCAL_SERVER_NAME, i),
                    state = i == 3 ? ServerState.Maintenance : i == 2 ? ServerState.Busy : ServerState.Normal,
                    recommended = i <= 2
                });
        }

        var entry = servers.Find(item => item.id == entity.selectedServer) ?? servers[0];

        if (!entity.SelectServer(entry.id))
            return false;

        vm.ServerName = entry.name;
        widget.SetServerState(entry.state);
        vm.CanEnter = entry.state != ServerState.Maintenance;

        return true;
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
                var vm = widget.vm;
                if (!vm)
                    return;

                vm.CloseClick = (sender) => CloseServers();
                vm.GroupTemplate = false;
                vm.ServerTemplate = false;
                vm.GroupClick = (sender, index) => SelectCategory(widget, index);
                vm.ServerClick = (sender, index) => SelectServerIndex(index);

                if (BuildCategories())
                    SelectCategory(widget, 0);
            }
        });
    }

    /// <summary>Build SV categories from the current catalog.</summary>
    /// <returns>True when the login entity was available and categories were rebuilt.</returns>
    private bool BuildCategories()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (!entity)
            return false;

        categories.Clear();
        categoryTitles.Clear();

        int recent = entity.recentServer;

        AddCategory(CommonText.Get(Config.Excel.LOGIN_RECENT_SERVER), servers.Where(entry => entry.id == recent).ToList());
        AddCategory(CommonText.Get(Config.Excel.LOGIN_RECOMMENDED_SERVER), servers.Where(entry => entry.recommended).ToList());
        for (int start = 0; start < servers.Count; start += 10)
            AddCategory(CommonText.Get(Config.Excel.LOGIN_SERVER_RANGE, start + 1, start + 10), servers.Skip(start).Take(10).ToList());

        return true;
    }

    /// <summary>Add a populated category.</summary>
    /// <param name="title">Display label.</param>
    /// <param name="entries">Server definitions.</param>
    private void AddCategory(string title, List<ServerEntry> entries)
    {
        if (entries.Count > 0)
        {
            categoryTitles.Add(title);
            categories.Add(entries);
        }
    }

    /// <summary>Publish category and server view models through VList bindings.</summary>
    /// <param name="widget">Bound page.</param>
    /// <param name="index">Category index.</param>
    /// <returns>True when the active page has been refreshed.</returns>
    private bool SelectCategory(UIServerSelect widget, int index)
    {
        if (!IsActive() || !widget || index < 0 || index >= categories.Count)
            return false;

        var vm = widget.vm;
        if (!vm)
            return false;

        categoryIndex = index;

        var groups = new VList<VMServerGroupItem>();
        for (int i = 0; i < categories.Count; i++)
            groups.Add(new VMServerGroupItem { Title = categoryTitles[i], Selected = i == index, Unselected = i != index });

        vm.Groups = groups;

        var items = new VList<VMServerItem>();
        foreach (var entry in categories[index])
        {
            string icon = entry.state == ServerState.Maintenance ? "dl_icon_wh" : entry.state == ServerState.Busy ? "dl_icon_ml" : "dl_icon_zc";
            items.Add(new VMServerItem {
                ServerName = entry.name,
                ServerStatus = $"Assets/Data/Widgets/ServerSelect/ServerSelectAssets/{icon}.png",
                Recommend = entry.recommended,
                PlayerHead = false
            });
        }

        vm.Servers = items;

        widget.ResetScroll();

        return true;
    }

    /// <summary>Resolve a Fancy cell index to the current server definition.</summary>
    /// <param name="index">Index in the selected category.</param>
    /// <returns>True when a server was selected.</returns>
    private bool SelectServerIndex(int index)
    {
        if (!IsActive() || categoryIndex < 0 || categoryIndex >= categories.Count
            || index < 0 || index >= categories[categoryIndex].Count)
            return false;

        return SelectServer(categories[categoryIndex][index]);
    }

    /// <summary>Apply a selected server and return to the login page.</summary>
    /// <param name="entry">Selected local server.</param>
    /// <returns>True when the selection was saved and the login page was refreshed.</returns>
    private bool SelectServer(ServerEntry entry)
    {
        if (!IsActive() || entry == null || !UISystem.GetSingleton().TryGetWidget<UILogin>(out var widget))
            return false;

        var vm = widget.vm;
        if (!vm)
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (!entity)
            return false;

        if (!entity.SelectServer(entry.id))
            return false;

        vm.ServerName = entry.name;
        widget.SetServerState(entry.state);
        vm.CanEnter = entry.state != ServerState.Maintenance;

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

        var vm = widget.vm;
        if (!vm)
            return false;

        if (!vm.CanEnter)
            return false;

        var login = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
        if (!login)
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
