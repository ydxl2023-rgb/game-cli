using Oathx.Widget;

/// <summary>
/// Binds loading progress and tips to the existing loading prefab.
/// </summary>
public partial class VMLoading : VM
{
    /// <summary>
    /// Loading message and percentage displayed by the Rate label.
    /// </summary>
    [Binding(typeof(SyncText))]
    private string rate;

    /// <summary>
    /// Additional information displayed by the TipText label.
    /// </summary>
    [Binding(typeof(SyncText))]
    private string tipText;
}
