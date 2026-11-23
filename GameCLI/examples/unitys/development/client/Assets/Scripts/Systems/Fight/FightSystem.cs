using Oathx.Core;
using Oathx.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// start fight event
/// </summary>
public class StartFightEvent : IEvent
{
    /// <summary>
    /// fight config id
    /// </summary>
    public int configId
    { get; set; }

    /// <summary>
    /// fight chapter id
    /// </summary>
    public int chapterId
    { get; set; }
}

/// <summary>
/// home system
/// </summary>
public class FightSystem : RuntimeSystem
{
    /// <summary>
    /// active home system
    /// </summary>
    /// <returns></returns>
    public override bool OnActive()
    {
        Subscribe<StartFightEvent>(OnStartFightEvent);
        return true;
    }

    /// <summary>
    /// deactive home system
    /// </summary>
    public override void OnDetive()
    {
        UnsubscribeAll();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="evt"></param>
    private void OnStartFightEvent(StartFightEvent evt)
    {
        var runtime = FightManager.GetSingleton().TryCreateFight<FightSIM>(evt.configId, out var fight);
        if (runtime)
        {
        }
    }
}
