using Oathx.Core;

/// <summary>Updates the local switch without bypassing level or time requirements.</summary>
public class FunctionSwitchChanged : IEvent
{
    /// <summary>FunctionSystem configuration identifier.</summary>
    public int id
    { get; set; }

    /// <summary>Whether the product permits this function.</summary>
    public bool enabled
    { get; set; }
}

/// <summary>Supplies the current player level to system availability checks.</summary>
public class FunctionLevelChanged : IEvent
{
    /// <summary>Nonnegative current player level.</summary>
    public int level
    { get; set; }
}

/// <summary>Reports a calculated availability transition.</summary>
public class FunctionOpenChanged : IEvent
{
    /// <summary>FunctionSystem configuration identifier.</summary>
    public int id
    { get; set; }

    /// <summary>Whether all configured requirements currently pass.</summary>
    public bool opened
    { get; set; }
}

/// <summary>Signals that product inputs have been applied before views refresh.</summary>
public class FunctionInputsUpdated : IEvent
{
}
