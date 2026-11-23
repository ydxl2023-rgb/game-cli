using Oathx.Widget;

/// <summary>Presentation bindings for Shop.</summary>
public partial class VMShop : VM
{
    /// <summary>Bound closeClick presentation value.</summary>
    [Binding]
    ButtonClick closeClick;

    /// <summary>Bound helpClick presentation value.</summary>
    [Binding]
    ButtonClick helpClick;

    /// <summary>Bound previousClick presentation value.</summary>
    [Binding]
    ButtonClick previousClick;

    /// <summary>Bound nextClick presentation value.</summary>
    [Binding]
    ButtonClick nextClick;

    /// <summary>Bound currency1 presentation value.</summary>
    [Binding(typeof(SyncText))]
    string currency1;

    /// <summary>Bound currency1Click presentation value.</summary>
    [Binding]
    ButtonClick currency1Click;

    /// <summary>Bound addCurrency1Click presentation value.</summary>
    [Binding]
    ButtonClick addCurrency1Click;

    /// <summary>Bound currency2 presentation value.</summary>
    [Binding(typeof(SyncText))]
    string currency2;

    /// <summary>Bound currency2Click presentation value.</summary>
    [Binding]
    ButtonClick currency2Click;

    /// <summary>Bound addCurrency2Click presentation value.</summary>
    [Binding]
    ButtonClick addCurrency2Click;

    /// <summary>Bound currency3 presentation value.</summary>
    [Binding(typeof(SyncText))]
    string currency3;

    /// <summary>Bound currency3Click presentation value.</summary>
    [Binding]
    ButtonClick currency3Click;

    /// <summary>Bound addCurrency3Click presentation value.</summary>
    [Binding]
    ButtonClick addCurrency3Click;

    /// <summary>Bound message presentation value.</summary>
    [Binding(typeof(SyncText))]
    string message;

    /// <summary>Bound messageVisible presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool messageVisible;

    /// <summary>Bound groups presentation value.</summary>
    [Binding(typeof(SyncFancyGridScrollView))]
    VList<VMShopGroup> groups;

    /// <summary>Bound groupClick presentation value.</summary>
    [Binding(typeof(SyncFancyGridClick))]
    FancyGridClick groupClick;

    /// <summary>Bound groupTemplate presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool groupTemplate;

    /// <summary>Bound tabs presentation value.</summary>
    [Binding(typeof(SyncFancyGridScrollView))]
    VList<VMShopTab> tabs;

    /// <summary>Bound tabClick presentation value.</summary>
    [Binding(typeof(SyncFancyGridClick))]
    FancyGridClick tabClick;

    /// <summary>Bound tabTemplate presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool tabTemplate;
}
