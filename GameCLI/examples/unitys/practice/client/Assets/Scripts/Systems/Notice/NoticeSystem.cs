using Oathx.Core;
using Oathx.Widget;

/// <summary>
/// Manages local announcement content and the notice page lifecycle.
/// </summary>
public class NoticeSystem : RuntimeSystem
{
    /// <summary>
    /// Local sample title used until live announcement content is connected.
    /// </summary>
    private const string defaultTitle = "Practice Trial Announcement";

    /// <summary>Explicit local test content; never a production announcement.</summary>
    private const string defaultContent =
        "<b>Practice trial - local test data only</b>\n\n" +
        "Test wrapping, scrolling, expansion and hyperlink hit areas.\n\n" +
        "<a href=\"https://example.com\">Open the example test website</a>\n\n" +
        "<a href=\"https://example.org\">A second test link that should remain independently clickable even when this deliberately long label wraps across multiple lines.</a>\n\n" +
        "<color=#D46A26>Drag from a link to scroll without opening a browser.</color>\n\n" +
        "Toggle the title to collapse or expand. Toggle the weekly reminder, close and reopen to check persistence.\n\n" +
        "<b>End of local test announcement.</b>";

    /// <summary>
    /// Open the notice page and initialize its local content and close action.
    /// </summary>
    /// <returns>True after requesting page opening while active.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        UISystem.GetSingleton().OpenWidget<UINotice>((widget) => {
            if (widget)
            {
                var vm = widget.vm;
                if (!vm)
                    return;

                widget.Initialize();

                vm.CloseClick = (sender) => CloseNotice();
                widget.SetContent(defaultTitle, defaultContent);
            }
        });

        return true;
    }

    /// <summary>
    /// Release subscriptions and close the notice page when deactivated.
    /// </summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UINotice)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>
    /// Ask MasterSystem to unload the notice after an explicit close request.
    /// </summary>
    /// <returns>True when the unload request was queued.</returns>
    private bool CloseNotice()
    {
        if (!IsActive())
            return false;

        return XPluginUtility.PostEvent(new SystemDetive() {
            type = typeof(NoticeSystem)
        });
    }

}
