using Oathx.Widget;
using UnityEngine;

/// <summary>Displays the original settings layout and owns presentation-only tab selection.</summary>
[Config("Assets/Data/Widgets/Settings/Settings.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UISettings : UIWidget<VMSettings>
{
    /// <summary>Original sound-page animation, played when that tab is selected.</summary>
    [SerializeField]
    private Animator soundAnimation;

    /// <summary>Currently displayed page; this is not a persisted player preference.</summary>
    public SettingsTab selectedTab
    { get; private set; }

    /// <summary>Restore the default page for each opening.</summary>
    /// <returns>True when the widget became active.</returns>
    public override bool Active()
    {
        selectedTab = SettingsTab.Screen;

        return base.Active();
    }

    /// <summary>Select a valid presentation tab and replay its original animation.</summary>
    /// <param name="tab">Page to display.</param>
    /// <returns>True when the tab was selected.</returns>
    public bool SelectTab(SettingsTab tab)
    {
        if (!System.Enum.IsDefined(typeof(SettingsTab), tab))
            return false;

        selectedTab = tab;

        if (tab == SettingsTab.Sound && soundAnimation)
            soundAnimation.Play("Sound", 0, 0f);

        return true;
    }
}
