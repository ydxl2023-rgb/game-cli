using Oathx.Core.Sdk;
using Oathx.Widget;

public partial class VMEmailSlot : VM
{
    /// <summary>
    /// email icon
    /// </summary>
    [Binding]
    string icon;

    /// <summary>
    /// email name
    /// </summary>
    [Binding]
    string name;

    /// <summary>
    /// email claim status
    /// </summary>
    [Binding]
    bool status;

    /// <summary>
    /// select status
    /// </summary>
    [Binding]
    bool select;

    /// <summary>
    /// slot save email source info
    /// </summary>
    public int Id
    { get ; set; }
}

