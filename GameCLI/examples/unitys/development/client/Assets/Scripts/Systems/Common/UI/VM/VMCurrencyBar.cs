using Oathx.Widget;

/// <summary>Ordered fixed currency slots supplied by the hosting page.</summary>
public partial class VMCurrencyBar : VM
{
    /// <summary>Entries matching the three authored direct children in order.</summary>
    [Binding(typeof(SyncGroup))]
    VList<VMCurrencyEntry> entries;
}
