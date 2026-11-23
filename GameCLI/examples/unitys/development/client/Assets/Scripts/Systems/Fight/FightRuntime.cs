
using Config;
using Oathx.Bundle.Assets;
using Oathx.Core;
using Oathx.Widget;
using System;
using UnityEngine;

/// <summary>
/// fight system, used to control the fight logic
/// </summary>
public class FightRuntime : XActionStateMachine
{
    /// <summary>
    /// fight state, used to control the fight logic, such as load, ready, fight, over, etc.
    /// </summary>
    public enum FightState
    {
        Load,
        Ready,
        Fight,
        Over
    }

    /// <summary>
    /// current fight state
    /// </summary>
    public FightState state
    { get; protected set; } = FightState.Load;

    /// <summary>
    /// fight entity, used to store the fight data, such as the fight state, the fight result, the fight score, etc.
    /// </summary>
    public FightEntity fight
    { get; protected set; }

    /// <summary>
    /// load complete flags
    /// </summary>
    protected bool sceneLoadComplete;
    /// <summary>
    /// heros load complete flag
    /// </summary>
    protected bool herosLoadComplete;
    /// <summary>
    /// widgets load complete flag
    /// </summary>
    protected bool widgetLoadComplete;

    /// <summary>
    /// start fight machine, set initial state to Load
    /// </summary>
    private void Awake()
    {
        InitStateMachine();
    }

    /// <summary>
    /// start fight machine, set initial state to Load
    /// </summary>
    private void Start()
    {
        SetState(FightState.Load);
    }

    /// <summary>
    /// update fight machine, call UpdateMachine in XActionStateMachine, and pass deltaTime to update callback
    /// </summary>
    private void Update()
    {
        UpdateMachine(Time.deltaTime);
    }

    /// <summary>
    /// initialize state machine, add states and their callbacks, such as enter, update, leave, etc.
    /// </summary>
    private void InitStateMachine()
    {
        AddState(FightState.Load, OnLoadEnter, OnLoadUpdate, OnLoadLeave);
        AddState(FightState.Ready, OnReadyEnter, OnReadyUpdate, OnReadyLeave);
        AddState(FightState.Fight, OnFightEnter, OnFightUpdate, OnFightLeave);
        AddState(FightState.Over, OnOverEnter, OnOverUpdate, OnOverLeave);
    }

    /// <summary>
    /// set fight entity, this method should be called before start fight machine, otherwise will cause null reference exception in state callbacks
    /// </summary>
    /// <param name="fightEntity"></param>
    public void SetFight(FightEntity fightEntity)
    {
        fight = fightEntity;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="levelId"></param>
    /// <param name="complete"></param>
    /// <returns></returns>
    public virtual bool LoadLevelAsync(int levelId, Action<UnityEngine.SceneManagement.Scene> complete)
    {
        var prop = Excel.propLevelById(levelId);
        if (string.IsNullOrEmpty(prop.Path))
            return false;

        XRes.LoadSceneAsync(prop.Path, complete);

        return true;
    }

    /// <summary>
    /// fight load, can do some load work in this state
    /// </summary>
    public virtual void OnLoadEnter() 
    {
        var fight = FightManager.GetSingleton().GetCurrentFight();
        if (fight)
        {
            LoadLevelAsync(fight.configId, (scene) =>
            {
                sceneLoadComplete = true;
            });
        }
    }

    public virtual void OnLoadUpdate(float deltaTime) { }
    public virtual void OnLoadLeave() { }

    /// <summary>
    /// fight ready, can do some prepare work in this state
    /// </summary>
    public virtual void OnReadyEnter() { }
    public virtual void OnReadyUpdate(float deltaTime) { }
    public virtual void OnReadyLeave() { }

    /// <summary>
    /// fight start, fight logic run in this state
    /// </summary>
    public virtual void OnFightEnter() { }
    public virtual void OnFightUpdate(float deltaTime) { }
    public virtual void OnFightLeave() { }

    /// <summary>
    /// fight over, can be win or lose, but not decided in fight runtime, but decided by other system
    /// </summary>
    public virtual void OnOverEnter() { }
    public virtual void OnOverUpdate(float deltaTime) { }
    public virtual void OnOverLeave() { }
}