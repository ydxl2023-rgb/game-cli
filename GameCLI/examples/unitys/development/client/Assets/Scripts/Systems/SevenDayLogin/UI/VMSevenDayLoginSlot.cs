using Oathx.Widget;

public partial class VMSevenDayLoginSlot : VM
{
    /// <summary>Configured day represented by this row.</summary>
    public int day
    { get; set; }

    /// <summary>
    /// day title, used to represent the title or label for a specific day in the seven day login event,
    /// </summary>
    [Binding]
    string title;
}
