using Oathx.Widget;
using UnityEngine;

/// <summary>Displays a reward entry without owning inventory or claim rules.</summary>
[RequireComponent(typeof(FancyGridScrollViewCell), typeof(Metadata))]
public class UIRewardEntry : UIWidget<VMRewardEntry>
{
    /// <summary>Release the child slot before releasing the reward subscription.</summary>
    /// <param name="refVM">Previously bound reward presentation.</param>
    public override void Unref(VM refVM)
    {
        var current = vm;
        var item = current ? current.Item : null;
        if (current && item)
            current.Item = null;

        base.Unref(refVM);

        // Restore data after unsubscribing so recycled cells can bind this model again.
        if (current && item)
            current.Item = item;
    }

    /// <summary>Release nested subscriptions when a host destroys this entry.</summary>
    private void OnDestroy()
    {
        if (vm)
            Unref(vm);
    }
}
