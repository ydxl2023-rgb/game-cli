using Oathx.Widget;

public partial class VMSevenDayLogin : VM
{
    /// <summary>
    /// Oathx.Widget.VList<VMSevenDayLoginSlot>.Days
    /// </summary>
    [Binding]
    VList<VMSevenDayLoginSlot> days;

    /// <summary>
    /// Oathx.Widget.FancyClick.FancyClick
    /// </summary>
    [Binding]
    FancyClick fancyClick;

    /// <summary>
    /// Oathx.Widget.ButtonClick.Claim
    /// </summary>
    [Binding]
    ButtonClick claim;

    /// <summary>
    /// Oathx.Widget.ButtonClick.Close
    /// </summary>
    [Binding]
    ButtonClick close;
}
