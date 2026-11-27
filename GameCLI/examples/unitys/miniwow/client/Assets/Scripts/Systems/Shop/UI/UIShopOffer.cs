using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Submits the stable offer identity from a reusable catalog cell.</summary>
public sealed class UIShopOffer : UIWidget<VMShopOffer>
{
    /// <summary>Authored target shared by the original contextual purchase guide.</summary>
    [SerializeField]
    private TaskGuideTarget guideTarget;

    /// <summary>Binds the cell intention to its current view model.</summary>
    /// <returns>Whether the cell is ready.</returns>
    protected override bool Bind()
    {
        vm.SelectClick = OnSelectClick;
        guideTarget.Select(vm.isGuideTarget);

        return true;
    }

    /// <summary>Applies only the display selection projected by the parent catalog.</summary>
    private void LateUpdate()
    {
        if (vm)
            guideTarget.Select(vm.isGuideTarget);
    }

    /// <summary>Requests selection without executing a purchase.</summary>
    /// <param name="button">The clicked offer.</param>
    /// <returns>Whether the request was sent.</returns>
    private bool OnSelectClick(Button button)
    {
        return Send(new ReqShopSelect { offerId = vm.offerId });
    }
}
