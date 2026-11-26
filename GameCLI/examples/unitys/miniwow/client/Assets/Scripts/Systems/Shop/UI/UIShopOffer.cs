using Oathx.Widget;
using UnityEngine.UI;

/// <summary>Submits the stable offer identity from a reusable catalog cell.</summary>
public sealed class UIShopOffer : UIWidget<VMShopOffer>
{
    /// <summary>Binds the cell intention to its current view model.</summary>
    /// <returns>Whether the cell is ready.</returns>
    protected override bool Bind()
    {
        vm.SelectClick = OnSelectClick;

        return true;
    }

    /// <summary>Requests selection without executing a purchase.</summary>
    /// <param name="button">The clicked offer.</param>
    /// <returns>Whether the request was sent.</returns>
    private bool OnSelectClick(Button button)
    {
        return Send(new ReqShopSelect { offerId = vm.offerId });
    }
}
