using Oathx.Widget;
using UnityEngine;

/// <summary>Reusable shortcut group driven by its host-owned VM.</summary>
[RequireComponent(typeof(Metadata))]
public class UIProfileShortcuts : UIWidget<VMProfileShortcuts>
{
    /// <summary>Whether this instance owns an active VM subscription.</summary>
    private bool bound;

    /// <summary>Bind the presentation once for this lifetime.</summary>
    /// <param name="refVM">Host-supplied shortcut presentation.</param>
    /// <returns>True when the VM was accepted.</returns>
    public override bool Ref(VM refVM)
    {
        if (bound || vm || !(refVM is VMProfileShortcuts))
            return false;

        bound = base.Ref(refVM);

        return bound;
    }

    /// <summary>Release the binding and owned click listeners.</summary>
    /// <param name="refVM">Previously bound presentation.</param>
    public override void Unref(VM refVM)
    {
        if (bound)
        {
            vm.MailEntry = null;
            vm.SettingsEntry = null;
            vm.NoticeEntry = null;
            vm.FriendsEntry = null;
            vm.AffinityEntry = null;

            base.Unref(refVM);

            bound = false;
        }
    }

    /// <summary>Release nested bindings when the host destroys this instance.</summary>
    private void OnDestroy()
    {
        if (bound)
            Unref(vm);
    }
}
