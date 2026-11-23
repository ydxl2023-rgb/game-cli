using Oathx.Core;
using Oathx.Widget;
using UnityEngine;

/// <summary>Temporary implementation smoke probes for the offline integration.</summary>
public static class PracticeSelfCheck
{
    /// <summary>Invoke the bound login button.</summary>
    /// <returns>True when the real login binding accepted the click.</returns>
    public static bool Enter()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UILogin>(out var widget) || !widget.vm)
            return false;

        return widget.vm.EnterClick(null);
    }

    /// <summary>Request an installed system through the coordinator.</summary>
    /// <param name="name">Runtime type name.</param>
    /// <returns>Whether a known request was queued.</returns>
    public static bool Open(string name)
    {
        var type = typeof(PracticePlugin).Assembly.GetType(name);
        if (type == null)
            return false;

        return XPluginUtility.PostEvent(new SystemActive { type = type });
    }

    /// <summary>Inspect the currently instantiated widget roots.</summary>
    /// <returns>Names of active widget components.</returns>
    public static string State()
    {
        var names = new System.Collections.Generic.List<string>();
        foreach (var metadata in Object.FindObjectsOfType<Metadata>())
            names.Add(metadata.gameObject.name);

        return string.Join(",", names);
    }
}
