using System;
using Oathx.Widget;
using UnityEngine;

/// <summary>
/// Presents a local login notice without requesting server content.
/// </summary>
[Config("Assets/Data/Widgets/Notice/Notice.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UINotice : UIWidget<VMNotice>
{
    /// <summary>
    /// Persistent expiration key for automatic notice suppression.
    /// </summary>
    private const string reminderKey = "GameCLI.Notice.SuppressUntil";

    /// <summary>
    /// Bind page actions and reset the displayed notice.
    /// </summary>
    /// <returns>True when the page has a bound view model.</returns>
    public bool Initialize()
    {
        if (!vm)
            return false;

        vm.ExpandClick = (sender) => ToggleContent();
        vm.ArrowClick = (sender) => ToggleContent();
        vm.WeekClick = (sender) => ToggleReminder();
        vm.WeekAreaClick = (sender) => ToggleReminder();
        vm.Selected = IsReminderSuppressed();

        return SetContent(string.Empty, string.Empty);
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

    /// <summary>
    /// Toggle and persist automatic notice suppression for seven days.
    /// </summary>
    /// <returns>True when the reminder preference was updated.</returns>
    private bool ToggleReminder()
    {
        if (!vm)
            return false;

        bool selected = !IsReminderSuppressed();
        if (selected)
            PlayerPrefs.SetString(reminderKey, DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture));
        else
            PlayerPrefs.DeleteKey(reminderKey);

        PlayerPrefs.Save();
        vm.Selected = selected;
        return true;
    }

    /// <summary>
    /// Check whether automatic opening is suppressed; manual opening remains available.
    /// </summary>
    /// <returns>True while the saved expiration is in the future.</returns>
    public static bool IsReminderSuppressed()
    {
        return long.TryParse(PlayerPrefs.GetString(reminderKey, string.Empty), out long expires)
            && expires > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
