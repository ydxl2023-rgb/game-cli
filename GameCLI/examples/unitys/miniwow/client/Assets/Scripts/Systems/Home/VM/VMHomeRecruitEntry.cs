using Oathx.Widget;

/// <summary>Bindings for original recruitment entrance bubbles.</summary>
public partial class VMHomeRecruitEntry : VM
{
    /// <summary>Brother conversation intention.</summary>
    [Binding]
    ButtonClick brotherClick;

    /// <summary>Horn interaction intention.</summary>
    [Binding]
    ButtonClick hornClick;

    /// <summary>First recruit interaction intention.</summary>
    [Binding]
    ButtonClick firstClick;

    /// <summary>Second recruit interaction intention.</summary>
    [Binding]
    ButtonClick secondClick;
}
