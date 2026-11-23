using System.Collections.Generic;
using System.Linq;
using Oathx.Widget;
using UnityEngine;

/// <summary>Presents server selection through bound Fancy lists.</summary>
[Config("Assets/Data/Widgets/ServerSelect/ServerSelect.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UIServerSelect : UIWidget<VMServerSelect>
{
    /// <summary>Server list used to reset the viewport after category changes.</summary>
    [SerializeField] private FancyGridScrollView serverGrid;

    /// <summary>Server categories owned by the login flow.</summary>
    private readonly List<List<ServerEntry>> categories = new List<List<ServerEntry>>();

    /// <summary>Category labels matching the category data.</summary>
    private readonly List<string> categoryTitles = new List<string>();

    /// <summary>Current category index.</summary>
    private int categoryIndex = -1;

    /// <summary>Bind player actions when the page opens.</summary>
    protected override bool Bind()
    {
        vm.CloseClick = Send<ReqLoginCloseServers>;
        vm.GroupClick = (sender, index) => Send(new ReqLoginSelectCategory { index = index });
        vm.ServerClick = (sender, index) => RequestServer(index);
        vm.GroupTemplate = false;
        vm.ServerTemplate = false;

        return true;
    }

    /// <summary>Reset the displayed category to its first row.</summary>
    public void ResetScroll()
    {
        serverGrid.JumpTo(0, Alignment.Upper);
    }

    /// <summary>Build SV categories from the current catalog.</summary>
    /// <returns>True when the login entity was available and categories were rebuilt.</returns>
    public bool Invalidate(LoginEntity entity)
    {
        if (!entity || !vm)
            return false;

        categories.Clear();
        categoryTitles.Clear();

        int recent = entity.recentServer;
        var servers = entity.GetServers();

        AddCategory(CommonText.Get(Config.Excel.LOGIN_RECENT_SERVER), servers.Where(entry => entry.id == recent).ToList());
        AddCategory(CommonText.Get(Config.Excel.LOGIN_RECOMMENDED_SERVER), servers.Where(entry => entry.recommended).ToList());
        for (int start = 0; start < servers.Count; start += 10)
            AddCategory(CommonText.Get(Config.Excel.LOGIN_SERVER_RANGE, start + 1, start + 10), servers.Skip(start).Take(10).ToList());

        return SelectCategory(categoryIndex >= 0 && categoryIndex < categories.Count ? categoryIndex : 0);
    }

    /// <summary>Publish category and server view models through VList bindings.</summary>
    /// <param name="index">Category index.</param>
    /// <returns>True when the active page has been refreshed.</returns>
    public bool SelectCategory(int index)
    {
        if (!vm || index < 0 || index >= categories.Count)
            return false;

        bool changed = categoryIndex != index;
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
                ServerStatus = $"Assets/Data/Widgets/Common/Icons/{icon}.png",
                Recommend = entry.recommended,
                PlayerHead = false
            });
        }

        vm.Servers = items;

        if (changed)
            ResetScroll();

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

    /// <summary>Publish the stable identifier represented by the clicked row.</summary>
    private bool RequestServer(int index)
    {
        if (categoryIndex < 0 || categoryIndex >= categories.Count || index < 0 || index >= categories[categoryIndex].Count)
            return false;

        return Send(new ReqLoginSelectServer { serverId = categories[categoryIndex][index].id });
    }
}
