using Oathx.Widget;

/// <summary>Submits one typed intention for each click; the entity rechecks access.</summary>
public sealed class UIHomeEntry : UIWidget<VMHomeEntry>
{
    /// <summary>Binds once during Ref, never during presentation refresh.</summary>
    /// <returns>Whether entry interaction was bound.</returns>
    protected override bool Bind()
    {
        vm.OpenClick = button => Send(new ReqHomeEntry { entryId = vm.entryId });

        return true;
    }
}
