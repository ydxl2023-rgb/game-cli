using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Core
{
    public enum CmdStatus
    {
        None,
        Waiting,
        Running,
        Success,
        Failed
    }

    /// <summary>
    /// AI command, used to control AI state machine, such as change state, change target, change parameter, etc.
    /// </summary>
    public class AICommand
    {
        /// <summary>
        /// action command, such as change state, change target, change parameter, etc.
        /// </summary>
        public Enum cmd
        { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public AICommand(Enum actionCommand)
        {
            cmd = actionCommand;
        }

        /// <summary>
        /// undo command, called by PostCommand, return true if action is successful, otherwise return false.
        /// </summary>
        public virtual void Undo()
        { }

        /// <summary>
        /// execute command, called by PostCommand, return true if action is successful, otherwise return false.
        /// </summary>
        /// <returns></returns>
        public virtual CmdStatus Execute()
        {
            return CmdStatus.Success;
        }

        /// <summary>
        /// redo command, called by PostCommand, return true if action is successful, otherwise return false.
        /// </summary>
        public virtual void Redo() 
        { }

        /// <summary>
        /// implicit operator bool, return true if command is not null, otherwise return false.
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(AICommand exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// AI event, used to control AI state machine, such as change state, change target, change parameter, etc.
    /// </summary>
    public class AIEvent
    {
        /// <summary>
        /// event id, such as change state, change target, change parameter, etc.
        /// </summary>
        public Enum Id
        { get; protected set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="enumId"></param>
        public AIEvent(Enum enumId)
        {
            Id = enumId;
        }

        /// <summary>
        /// implicit operator bool, return true if command is not null, otherwise return false.
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(AIEvent exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// AI state machine, used to control AI state, such as change state, change target, change parameter, etc.
    /// </summary>
    public class AIStateMachine
    {
        public NotifyMachineStateChanged notifyStateChanged;

        /// <summary>
        /// curent active state list
        /// </summary>
        public Stack<AIState> stateList = new Stack<AIState>();

        /// <summary>
        /// state map
        /// </summary>
        protected Dictionary<Enum, AIState> stateMap = new Dictionary<Enum, AIState>();

        /// <summary>
        /// 
        /// </summary>
        public bool pause
        { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(AIStateMachine exists)
        {
            return exists != null;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Pause()
        {
            pause = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Resume()
        {
            pause = false;
        }

        /// <summary>
        /// add new state
        /// </summary>
        /// <param name="newState"></param>
        public virtual void AddState(AIState newState)
        {
            if (!stateMap.ContainsKey(newState.Name))
            {
                newState.SetMachine(this);

#if UNITY_EDITOR || UNITY_DEVELOP
                Debug.Log($"Add state {newState.Name}");
#endif

                stateMap.Add(newState.Name, newState);
            }
        }

        /// <summary>
        /// set state change callback
        /// </summary>
        /// <param name="stateChanged"></param>
        public void SetStateChanged(NotifyMachineStateChanged stateChanged)
        {
            notifyStateChanged = stateChanged;
        }

        /// <summary>
        /// push new state to state list and run it
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual bool PushState(Enum stateName)
        {
            if (!stateMap.ContainsKey(stateName))
            {
                return false;
            }

            AIState preState = GetCurrentState();
            if (preState)
            {
                if (preState.Name == stateName)
                    return false;
            }

            AIState curState = stateMap[stateName];
            stateList.Push(curState);

            // state enter
            curState.Flag = false;
            curState.OnEnter();
            curState.Flag = true;

            // notity state changed
            notifyStateChanged?.Invoke(preState, curState);

            return true;
        }

        /// <summary>
        /// pop current state
        /// </summary>
        /// <returns></returns>
        public virtual bool PopState(Enum state)
        {
            AIState preState = stateList.Count != 0 ? stateList.Pop() : null;
            if (preState)
                preState.OnLeave();

            AIState curState = GetCurrentState();
            notifyStateChanged?.Invoke(preState, curState);

            return true;
        }

        /// <summary>
        /// clear state list
        /// </summary>
        /// <returns></returns>
        public virtual bool ClearToGlobalOrBlockState()
        {
            while (stateList.Count != 0)
            {
                AIState state = stateList.Pop();
                if (state)
                    state.OnLeave();
            }

            return true;
        }

        /// <summary>
        /// reset state machine
        /// </summary>
        public virtual void Reset()
        {
            ClearToGlobalOrBlockState();
        }

        /// <summary>
        /// set current state, This call will clear all running states before inserting the new window into the run list
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual bool SetState(Enum name)
        {
            if (pause || !stateMap.ContainsKey(name))
                return false;

            AIState state = GetCurrentState();
            if (!state || state.Name != name)
            {
                ClearToGlobalOrBlockState();

                return PushState(name);
            }

            return false;
        }

        /// <summary>
        /// check is current state
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public bool IsCurrent(Enum stateName)
        {
            AIState state = GetCurrentState();
            if (state != null)
                return state.Name == stateName;

            return false;

        }

        /// <summary>
        /// get curent state
        /// </summary>
        /// <returns></returns>
        public AIState GetCurrentState()
        {
            if (stateList.Count != 0)
                return stateList.Peek();

            return null;
        }

        /// <summary>
        /// update machine, Only the current state is updated
        /// </summary>
        /// <param name="delteTime"></param>
        public virtual void UpdateMachine(float delteTime)
        {
            if (!pause)
            {
                int count = stateList.Count;
                if (count > 0)
                {
                    AIState state = stateList.Peek();
                    if (state.Flag)
                        state.OnUpdate(delteTime);
                }
            }
        }

        /// <summary>
        /// post event to state
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="args"></param>
        public virtual void PostEvent(AIEvent evt)
        {
            if (stateList.Count > 0)
            {
                var states = stateList.ToArray();
                for (int idx = 0; idx < states.Length; idx++)
                {
                    var state = states[idx];
                    if (state)
                        state.OnEvent(evt);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public bool Exist(System.Enum stateId)
        {
            return stateMap.ContainsKey(stateId);
        }
    }

}