
using Oathx.Core;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// fight manager
/// </summary>
public class FightManager : XSimpleSingleton<FightManager>
{
    public class Bind
    {
        /// <summary>
        /// fight entity, used to store the fight data, such as the fight state, the fight result, the fight score, etc.
        /// </summary>
        public FightEntity fight
        { get; set; }

        /// <summary>
        /// FightRuntime, used to control the fight logic, such as load, ready, fight, over, etc.
        /// </summary>
        public FightRuntime runtime
        { get; set; }
    }

    /// <summary>
    /// fight entities, used to store the fight data
    /// </summary>
    protected readonly Dictionary<int, Bind> fights = new Dictionary<int, Bind>();

    /// <summary>
    /// current fight entity
    /// </summary>
    public FightEntity fight
    { get; protected set; }

    /// <summary>
    /// try create fight runtime, if create success, return true and set runtime, otherwise return false
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configId"></param>
    /// <param name="runtime"></param>
    /// <returns></returns>
    /// <exception cref="System.NullReferenceException"></exception>
    public T TryCreateFight<T>(int configId, out FightEntity fightEntity) where T : FightRuntime
    {
        if (!fights.TryGetValue(configId, out Bind bind))
        {
            fightEntity = new FightEntity(configId);

            // create fight game object and add fight runtime component
            GameObject fightGo = new GameObject(typeof(T).Name);
            var runtime = fightGo.AddComponent<T>();
            if (!runtime)
                throw new System.NullReferenceException($"create fight runtime failed, type: {typeof(T).Name} configId {configId}");

            runtime.SetFight(fightEntity);

            // add fight bind to fight manager
            bind = new Bind() { 
                fight = fightEntity, runtime = runtime 
            };
            fights.Add(configId, bind);
        }
        else
        {
            fightEntity = bind.fight;
        }

        SetCurrentFight(fightEntity);
        return (T)bind.runtime;
    }

    /// <summary>
    /// finish current fight, destroy fight game object and clear current fight entity
    /// </summary>
    /// <param name="fight"></param>
    public void DestroyFight(FightEntity fight)
    {
        if (fights.TryGetValue(fight.configId, out Bind bind))
        {
            if (bind.runtime)
                GameObject.Destroy(bind.runtime.gameObject);

            fights.Remove(fight.configId);
        }
    }

    /// <summary>
    /// set current fight entity
    /// </summary>
    /// <param name="newFight"></param>
    public void SetCurrentFight(FightEntity newFight)
    {
        fight = newFight;
    }

    /// <summary>
    /// get current fight entity
    /// </summary>
    /// <returns></returns>
    public FightEntity GetCurrentFight()
    {
        return fight;
    }
}