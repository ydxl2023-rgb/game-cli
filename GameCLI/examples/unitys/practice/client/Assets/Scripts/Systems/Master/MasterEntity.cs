using Oathx.Entity;

/// <summary>Stores local trial player state without production save data.</summary>
public class MasterEntity : XEntity
{
    /// <summary>Local player identifier, or zero before successful login.</summary>
    public int pid
    { get; set; }
}
