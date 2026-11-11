using System.Collections;
using UnityEngine;
using Oathx.Core;

/// <summary>
/// runtime logic system
/// </summary>
public abstract class RuntimeSystem : XEventModule
{
    /// <summary>
    /// load system
    /// </summary>
    /// <param name="type"></param>
    /// <param name="complete"></param>
    public virtual void LoadSystem(System.Type type, System.Action complete=null)
    {
        XCoroutine.Run(DoLoadAsync(type, complete));
    }

    /// <summary>
    /// do load async
    /// </summary>
    /// <param name="type"></param>
    /// <param name="complete"></param>
    /// <returns></returns>
    private IEnumerator DoLoadAsync(System.Type type, System.Action complete)
    {
        dispatcher.Load(type);
        yield return new WaitForEndOfFrame();

        complete?.Invoke();
    }

    /// <summary>
    /// unload system
    /// </summary>
    /// <param name="type"></param>
    /// <param name="complete"></param>
    public void UnloadSystem(System.Type type, System.Action complete)
    {
        XCoroutine.Run(DoUnloadAsync(type, complete));
    }

    /// <summary>
    /// do unload async
    /// </summary>
    /// <param name="type"></param>
    /// <param name="complete"></param>
    /// <returns></returns>
    IEnumerator DoUnloadAsync(System.Type type, System.Action complete)
    {
        dispatcher.Unload(type);
        yield return new WaitForEndOfFrame();
        complete?.Invoke();
    }
}
