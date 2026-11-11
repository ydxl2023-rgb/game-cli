using System;
using UnityEngine;

namespace Oathx.Core
{
    public delegate void NotifyMachineStateChanged(AIState preState, AIState curState);

    /// <summary>
    /// base state
    /// </summary>
    public class AIState
    {
        /// <summary>
        /// state name
        /// </summary>
        public Enum Name
        {
            get; internal set;
        }

        /// <summary>
        /// is enter state
        /// </summary>
        public bool Flag
        {
            get; internal set;
        }

        /// <summary>
        /// state owner
        /// </summary>
        protected AIStateMachine machine;

        /// <summary>
        /// construct
        /// </summary>
        /// <param name="stateName"></param>
        public AIState(System.Enum state)
        {
            Flag = false;
            Name = state;
        }

        /// <summary>
        /// ����NULL����ת��ΪBOOLֵ
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(AIState exists)
        {
            return exists != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateMachine"></param>
        public void SetMachine(AIStateMachine stateMachine)
        {
            machine = stateMachine;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="args"></param>
        public bool SetState(Enum state)
        {
            bool result = machine.SetState(state);
            if (!result)
            {
#if UNITY_EDITOR || UNITY_DEVELOP
                Debug.LogError("Enter the same state over and over again");
#endif
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool PushState(Enum state)
        {
            return machine.PushState(state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool PopState(Enum state, params object[] args)
        {
            return machine.PopState(state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public bool Exist(Enum stateId)
        {
            return machine.Exist(stateId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public bool IsCurrent(Enum stateName)
        {
            return machine.IsCurrent(stateName);
        }

        /// <summary>
        /// get curent state
        /// </summary>
        /// <returns></returns>
        public AIState GetCurrentState()
        {
            return machine.GetCurrentState();
        }

        /// <summary>
        /// enter state
        /// </summary>
        /// <param name="args"> state params list </param>
        public virtual bool OnEnter()
        {
#if UNITY_EDITOR || UNITY_DEVELOP
            Debug.Log($"<color=magenta>[AI] {Name}</color> Enter [{Time.frameCount}]({Time.time})");
#endif
            return true;
        }

        /// <summary>
        /// state update
        /// </summary>
        public virtual bool OnUpdate(float deltaTime)
        {
            return true;
        }

        /// <summary>
        /// state exit
        /// </summary>
        public virtual bool OnLeave()
        {
#if UNITY_EDITOR || UNITY_DEVELOP
            Debug.Log($"<color=magenta>[AI] {Name}</color> Leave [{Time.frameCount}]({Time.time})");
#endif
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nEventId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual bool OnEvent(AIEvent evt)
        {
#if UNITY_EDITOR || UNITY_DEVELOP
            Debug.Log($"<color=magenta>[AI]</color> Event [{Time.frameCount}]({Time.time}) State {GetType().Name} {Name}");
#endif
            return true;
        }
    }

}
