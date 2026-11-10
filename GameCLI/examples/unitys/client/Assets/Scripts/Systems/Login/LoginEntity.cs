using Oathx.Entity;
using Oathx.HttpProtocol;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[Initialize]
public partial class LoginEntity : XEntity
{
    /// <summary>
    /// constructor
    /// </summary>
    public int pid
    { get; set; }

    /// <summary>
    /// login time
    /// </summary>
    public long loginAt
    { get; set; }

    /// <summary>
    /// create time
    /// </summary>
    public long tmcreate
    { get; set; }

    /// <summary>
    /// query login entity
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public bool Refresh(LoginResponse response)
    {
        pid = response.pid; loginAt = response.loginAt; tmcreate = response.tmcreate;
        return true;
    }
}
