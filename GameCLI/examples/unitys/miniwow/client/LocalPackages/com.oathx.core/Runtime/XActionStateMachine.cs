using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Core
{
    /// <summary>
    /// 
    /// </summary>
    public delegate void ActionStateNormal();

    /// <summary>
    /// 
    /// </summary>
    public class ActionState
    {
        /// <summary>
        /// state enum, can be any enum type, but must be unique in state machine, otherwise will cause unexpected behavior
        /// </summary>
        public Enum state
        { get; set; }

        /// <summary>
        /// enter flag, when state enter callback called, set this flag to true, when state leave callback called, set this flag to false,
        /// if state is current active state, then this flag must be true, otherwise must be false, this flag is used to prevent update callback called before enter callback called or after leave callback called
        /// </summary>
        public bool enterFlag
        { get; set; }

        /// <summary>
        /// state callback
        /// </summary>
        public ActionStateNormal enter
        { get; set; }

        /// <summary>
        /// state callback
        /// </summary>
        public ActionStateUpdate update
        { get; set; }

        /// <summary>
        /// state callback
        /// </summary>
        public ActionStateNormal leave
        { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public delegate void ActionStateChanged(ActionState state);

    /// <summary>
    /// 
    /// </summary>
    public delegate void ActionStateUpdate(float deltaTime);

    /// <summary>
    /// action state
    /// </summary>
    public class XActionStateMachine : MonoBehaviour
    {
        public ActionStateChanged notifyChanged;

        /// <summary>
        /// curent active state list
        /// </summary>
        protected List<ActionState> stateList = new List<ActionState>();

        /// <summary>
        /// state map
        /// </summary>
        protected Dictionary<Enum, ActionState> stateMap = new Dictionary<Enum, ActionState>();

        /// <summary>
        /// add action state
        /// </summary>
        /// <param name="newState"></param>
        public void AddState(Enum state,
            ActionStateNormal onEnter, ActionStateUpdate onUpdate=null, ActionStateNormal onLeave=null)
        {
            if (!stateMap.ContainsKey(state))
            {
                stateMap.Add(state, new ActionState()
                {
                    state = state,
                    enterFlag = false,
                    enter = onEnter,
                    update = onUpdate,
                    leave = onLeave
                });
            }
        }

        /// <summary>
        /// get action state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <exception cref="System.NullReferenceException"></exception>
        protected ActionState GetState(Enum state)
        {
            if (!stateMap.ContainsKey(state))
                throw new NullReferenceException(state.ToString());

            return stateMap[state];
        }

        /// <summary>
        /// check state exist
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool IsExist(Enum state)
        {
            return stateMap.ContainsKey(state);
        }

        /// <summary>
        /// check state is current active state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool IsCurrent(Enum state)
        {
            ActionState topState = GetCurrentState();
            if (topState != null)
                return topState.state == state;

            return false;
        }

        /// <summary>
        /// push state to stack, if state exist and not current active state, then push state to stack and call enter callback
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool PushState(Enum state)
        {
            if (!stateMap.ContainsKey(state))
            {
                return false;
            }

            int count = stateList.Count;
            if (count > 0)
            {
                ActionState topState = stateList[count - 1];
                if (topState.state == state)
                {
                    return false;
                }
            }

            ActionState curState = GetState(state);
            stateList.Add(curState);

            curState.enterFlag = false;
            curState.enter?.Invoke();
            curState.enterFlag = true;

            if (notifyChanged != null)
                notifyChanged(curState);

            return true;
        }

        /// <summary>
        /// set state changed callback, when state changed, will call this callback
        /// </summary>
        /// <param name="changed"></param>
        public void SetStateChanged(ActionStateChanged changed)
        {
            notifyChanged = changed;
        }

        /// <summary>
        /// pop state from stack, if stack is not empty, then pop state from stack and call leave callback
        /// </summary>
        public void PopState()
        {
            int count = stateList.Count;
            if (count > 0)
            {
                ActionState topState = stateList[count];
                topState.leave?.Invoke();

                stateList.RemoveAt(count);
            }

            count = stateList.Count;
            if (count > 0)
            {
                notifyChanged?.Invoke(stateList[count]);
            }
        }

        /// <summary>
        /// get current active state, if stack is not empty, then return top state, otherwise return null
        /// </summary>
        /// <returns></returns>
        public ActionState GetCurrentState()
        {
            if (stateList.Count == 0)
                return null;

            return stateList[stateList.Count - 1];
        }

        /// <summary>
        /// set state, if state exist and not current active state, then clear all state in stack and push state to stack, call enter callback
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool SetState(System.Enum state)
        {
            if (!stateMap.ContainsKey(state))
            {
                return false;
            }

            ClearOrBlockState(null);

            return PushState(state);
        }

        /// <summary>
        /// clear or block state, if state is null, then clear all state in stack, otherwise clear all state in stack except state, call leave callback
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool ClearOrBlockState(Enum state)
        {
            ActionState oldState = GetCurrentState();
            if (oldState == null || oldState.state != state)
            {
                for (int index = stateList.Count - 1; index >= 0; index--)
                {
                    ActionState curState = stateList[index];
                    if (curState.state != state)
                    {
                        curState.leave?.Invoke();

                        stateList.RemoveAt(index);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// update state, if stack is not empty, then call update callback of top state
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateMachine(float deltaTime)
        {
            if (stateList.Count != 0)
            {
                ActionState topState = GetCurrentState();
                if (topState != null && topState.enterFlag)
                    topState.update?.Invoke(deltaTime);
            }
        }
    }
}
