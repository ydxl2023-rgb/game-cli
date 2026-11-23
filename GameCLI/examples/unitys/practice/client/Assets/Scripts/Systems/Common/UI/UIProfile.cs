using Oathx.Widget;
using UnityEngine;

/// <summary>Reusable player profile driven by its host-owned VM.</summary>
[RequireComponent(typeof(Metadata))]
public class UIProfile : UIWidget<VMProfile>
{
    /// <summary>Whether this instance owns an active VM subscription.</summary>
    private bool bound;

    /// <summary>Bind the presentation once for this lifetime.</summary>
    /// <param name="refVM">Host-supplied profile presentation.</param>
    /// <returns>True when the VM was accepted.</returns>
    public override bool Ref(VM refVM)
    {
        if (bound || vm || !(refVM is VMProfile))
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
            vm.ProfileEntry = null;
            vm.AvatarEntry = null;

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
