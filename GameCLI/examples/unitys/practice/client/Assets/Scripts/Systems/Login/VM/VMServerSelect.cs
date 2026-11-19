using Oathx.Widget;

/// <summary>Server selection page bindings.</summary>
public partial class VMServerSelect : VM
{
    /// <summary>Close the dialog.</summary>
    [Binding]
    ButtonClick closeClick;

    /// <summary>Available server categories.</summary>
    [Binding(typeof(SyncFancyGridScrollView))]
    VList<VMServerGroupItem> groups;

    /// <summary>Servers in the selected category.</summary>
    [Binding(typeof(SyncFancyGridScrollView))]
    VList<VMServerItem> servers;

    /// <summary>Category selection callback.</summary>
    [Binding]
    FancyGridClick groupClick;

    /// <summary>Server selection callback.</summary>
    [Binding]
    FancyGridClick serverClick;

    /// <summary>Keep the source category cell hidden outside the recycled list.</summary>
    [Binding(typeof(SyncVisible))]
    bool groupTemplate;

    /// <summary>Keep the source server cell hidden outside the recycled list.</summary>
    [Binding(typeof(SyncVisible))]
    bool serverTemplate;
}
