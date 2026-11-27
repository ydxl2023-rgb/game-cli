using Oathx.Widget;
using UnityEngine.UI;

/// <summary>Submits the identity of a reusable recipe or shelf row.</summary>
public sealed class UIBlacksmithEntry : UIWidget<VMBlacksmithEntry>
{
    /// <summary>Binds the current row selection.</summary>
    /// <returns>Whether binding succeeded.</returns>
    protected override bool Bind()
    {
        vm.SelectClick = OnSelectClick;

        return true;
    }

    /// <summary>Sends the represented identity without executing a transaction.</summary>
    /// <param name="button">Clicked row.</param>
    /// <returns>Whether the intention was sent.</returns>
    private bool OnSelectClick(Button button)
    {
        return Send(new ReqBlacksmithSelect { identity = vm.identity, shelf = vm.shelf });
    }
}
