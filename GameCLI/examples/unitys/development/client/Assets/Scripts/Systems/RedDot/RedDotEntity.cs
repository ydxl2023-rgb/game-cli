using System.Collections.Generic;
using Oathx.Entity;

/// <summary>Session-owned condition inputs and calculated notification counts.</summary>
[Initialize]
public partial class RedDotEntity : XEntity
{
    /// <summary>Counts supplied by business messages, independent of widget lifetime.</summary>
    public Dictionary<int, int> signals
    { get; private set; } = new Dictionary<int, int>();

    /// <summary>Last own-condition result before parent aggregation.</summary>
    public Dictionary<int, int> evaluated
    { get; private set; } = new Dictionary<int, int>();

    /// <summary>Last evaluated counts including parent aggregation.</summary>
    public Dictionary<int, int> counts
    { get; private set; } = new Dictionary<int, int>();
}
