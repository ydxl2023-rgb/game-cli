using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    /// idle, such as idle in target, or no target
    Idle,
    /// <summary>
    /// in target, such as move to target
    /// </summary>
    In,
    /// <summary>
    /// out target, such as move out of target
    /// </summary>
    Put
}

/// <summary>
/// point component, control point state, such as idle, in, out
/// </summary>
public class PointComponent : MonoBehaviour
{
    public TargetType TargetType;
}
