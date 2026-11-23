using Oathx.Widget;

/// <summary>
/// Presents a local login notice without requesting server content.
/// </summary>
[Config("Assets/Data/Widgets/Notice/Notice.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UINotice : UIWidget<VMNotice>
{
    /// <summary>Connect view interactions when the notice opens.</summary>
    protected override bool Bind()
    {
        vm.CloseClick = Send<ReqNoticeClose>;
        vm.ExpandClick = sender => ToggleContent();
        vm.ArrowClick = sender => ToggleContent();
        vm.WeekClick = Send<ReqNoticeToggleReminder>;
        vm.WeekAreaClick = Send<ReqNoticeToggleReminder>;

        return true;
    }

    /// <summary>Update content and suppression while preserving the reader's expansion choice.</summary>
    public bool Invalidate(NoticeEntity entity)
    {
        if (!entity || !vm)
            return false;

        vm.Selected = entity.suppressed;
        if (vm.NoticeTitleName != entity.title || vm.Body != entity.content)
            return SetContent(entity.title, entity.content);

        return true;
    }

    /// <summary>
    /// Replace the displayed local notice; an empty body displays the empty state.
    /// </summary>
    /// <param name="title">Local notice title.</param>
    /// <param name="text">Local notice body; null or whitespace clears the notice.</param>
    /// <returns>True when the bound page has been updated.</returns>
    public bool SetContent(string title, string text)
    {
        if (!vm)
            return false;

        bool hasContent = !string.IsNullOrWhiteSpace(text);
        vm.NoticeTitleName = title ?? string.Empty;
        vm.Body = text ?? string.Empty;
        vm.Entry = hasContent;
        vm.Empty = !hasContent;
        vm.Expanded = hasContent;

        return true;
    }

    /// <summary>Read the current local preference for callers deciding whether to open a notice.</summary>
    public static bool IsReminderSuppressed()
    {
        return NoticeEntity.ReadSuppression();
    }

    /// <summary>
    /// Toggle the visible body of the local notice.
    /// </summary>
    /// <returns>True when a notice was available to toggle.</returns>
    private bool ToggleContent()
    {
        if (!vm || !vm.Entry)
            return false;

        vm.Expanded = !vm.Expanded;
        return true;
    }
}

/// <summary>Close the notice page.</summary>
public sealed class ReqNoticeClose : UIRequestEvent
{
}

/// <summary>Toggle the seven-day reminder preference.</summary>
public sealed class ReqNoticeToggleReminder : UIRequestEvent
{
}
