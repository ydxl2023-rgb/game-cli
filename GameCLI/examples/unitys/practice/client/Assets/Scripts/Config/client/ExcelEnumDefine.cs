using System.Collections.Generic;

namespace Config
{
    /// <summary>
    /// 
    /// </summary>
    public enum ETargetType
    {
        None = 0,
        Self = 1,
        Target = 2,
    }

    /// <summary>
    /// Condition detector
    /// </summary>
    public enum ERedDotCondition
    {
        None = 0,
        SignalCount = 1,
    }

    /// <summary>
    /// Display mode
    /// </summary>
    public enum ERedDotDisplay
    {
        None = 0,
        Count = 1,
        Dot = 2,
    }
}