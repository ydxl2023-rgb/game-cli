using Oathx.Widget;

/// <summary>
/// Bindings for the local login notice page.
/// </summary>
public partial class VMNotice : VM
{
    /// <summary>
    /// Close button callback.
    /// </summary>
    [Binding]
    private ButtonClick closeClick;

    /// <summary>
    /// Notice header callback.
    /// </summary>
    [Binding]
    private ButtonClick expandClick;

    /// <summary>
    /// Notice arrow callback.
    /// </summary>
    [Binding]
    private ButtonClick arrowClick;

    /// <summary>
    /// Weekly reminder checkbox callback.
    /// </summary>
    [Binding]
    private ButtonClick weekClick;

    /// <summary>
    /// Weekly reminder label callback.
    /// </summary>
    [Binding]
    private ButtonClick weekAreaClick;

    /// <summary>
    /// Current local notice title.
    /// </summary>
    [Binding(typeof(SyncText))]
    private string noticeTitleName;

    /// <summary>
    /// Current local notice body.
    /// </summary>
    [Binding(typeof(SyncText))]
    private string body;

    /// <summary>
    /// Whether a local notice is available.
    /// </summary>
    [Binding(typeof(SyncVisible))]
    private bool entry;

    /// <summary>
    /// Whether the empty state is visible.
    /// </summary>
    [Binding(typeof(SyncVisible))]
    private bool empty;

    /// <summary>
    /// Whether the notice body is expanded.
    /// </summary>
    [Binding(typeof(SyncVisible))]
    private bool expanded;

    /// <summary>
    /// Whether automatic notices are suppressed for one week.
    /// </summary>
    [Binding(typeof(SyncVisible))]
    private bool selected;

    /// <summary>Localized text shown when no announcement is available.</summary>
    [Binding(typeof(SyncText))]
    private string emptyText;
}
