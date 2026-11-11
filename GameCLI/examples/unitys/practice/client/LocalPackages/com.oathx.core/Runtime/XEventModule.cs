using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Core
{
    public class AutoLoadOnStartup : System.Attribute { }

    /// <summary>
    /// Event base class
    /// </summary>
    public interface IEvent
    {
    }

    /// <summary>
    /// Event callback delegate
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate void EventCallback(IEvent evt);

    /// <summary>
    /// Event generic callback delegate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="evt"></param>
    /// <param name="args"></param>
    public delegate void GenericCallback<T>(T evt) where T : IEvent;

    /// <summary>
    /// Event module base class
    /// </summary>
    public abstract class XEventModule
    {
        /// <summary>
        /// Tigger event class
        /// </summary>
        public class XTiggerEvent
        {
            /// <summary>
            /// event id
            /// </summary>
            public int id
            { get; set; }

            /// <summary>
            /// event callback
            /// </summary>
            public EventCallback cb
            { get; set; }
        }

        /// <summary>
        /// Trigger listener class
        /// </summary>
        public class XTriggerListener
        {
            /// <summary>
            /// listener id
            /// </summary>
            public int listenerId
            { get; set; }

            /// <summary>
            /// type name
            /// </summary>
            public string typeName
            { get; set; }

            /// <summary>
            /// events list
            /// </summary>
            public List<XTiggerEvent> events
            { get; set; }
        }

        /// <summary>
        /// listeners dictionary
        /// </summary>
        private readonly Dictionary<string, XTriggerListener> listeners = new Dictionary<string, XTriggerListener>();

        /// <summary>
        /// register listeners dictionary
        /// </summary>
        private readonly Dictionary<int, XTriggerListener>
            registerListeners = new Dictionary<int, XTriggerListener>();

        /// <summary>
        /// pending event class
        /// </summary>
        public class XPendingEvent
        {
            /// <summary>
            /// event id
            /// </summary>
            public int id
            { get; set; }

            /// <summary>
            /// type name
            /// </summary>
            public string typeName
            { get; set; }

            /// <summary>
            /// event callback
            /// </summary>
            public EventCallback cb
            { get; set; }
        }

        /// <summary>
        /// register pendings dictionary
        /// </summary>
        private readonly Dictionary<string, List<XPendingEvent>> registerPendings = new Dictionary<string, List<XPendingEvent>>();

        /// <summary>
        /// unregister pendings dictionary
        /// </summary>
        private readonly Dictionary<string, List<int>>
            unregisterPendings = new Dictionary<string, List<int>>();

        /// <summary>
        /// triggering dictionary
        /// </summary>

        private readonly Dictionary<string, bool> triggering = new Dictionary<string, bool>();

        /// <summary>
        /// listener id
        /// </summary>
        private int listenerId;

        /// <summary>
        /// active flag
        /// </summary>
        protected bool active = false;

        /// <summary>
        /// dispatcher reference
        /// </summary>
        protected XEventDispatcher dispatcher;

        /// <summary>
        /// set dispatcher
        /// </summary>
        /// <param name="eventDispatcher"></param>
        public virtual void SetDispatcher(XEventDispatcher eventDispatcher)
        {
            dispatcher = eventDispatcher;
        }

        /// <summary>
        /// get dispatcher
        /// </summary>
        /// <returns></returns>
        public virtual XEventDispatcher GetDispatcher()
        {
            return dispatcher;
        }

        /// <summary>
        /// install module, return true if install success, otherwise return false
        /// </summary>
        /// <returns></returns>
        public virtual bool Install() 
        { return true; }

        /// <summary>
        /// uninstall module
        /// </summary>
        public virtual void Uninstall() 
        { 
        }

        /// <summary>
        /// active module
        /// </summary>
        /// <returns></returns>
        public virtual bool Active()
        {
            active = true;
            return OnActive();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsActive() { return active; }

        /// <summary>
        /// detive module
        /// </summary>
        public virtual void Detive()
        {
            active &= false;

            // system detive
            OnDetive();
        }

        /// <summary>
        /// on active, return true if active success, otherwise return false
        /// </summary>
        /// <returns></returns>
        public abstract bool OnActive();

        /// <summary>
        /// on detive
        /// </summary>
        public abstract void OnDetive();

        /// <summary>
        /// has event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual bool HasEvent<T>(T entity) where T : IEvent
        {
            string typeName = entity.GetType().Name;
            if (listeners.TryGetValue(typeName, out var listener))
            {
                return listener.events.Count > 0;
            }

            return false;
        }

        /// <summary>
        /// trigger event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual bool Trigger<T>(T entity) where T : IEvent
        {
            string typeName = entity.GetType().Name;
            if (!listeners.ContainsKey(typeName))
                return false;

            XTriggerListener listener = listeners[typeName];
            if (!triggering.ContainsKey(typeName))
            {
                triggering.Add(typeName, true);
            }

            foreach (var trigger in listener.events)
            {
                trigger.cb?.Invoke(entity);
            }

            if (registerPendings.TryGetValue(typeName, out var pendings))
            {
                foreach(var pendingEvent in pendings)
                {
                    DoRegister(listener, pendingEvent.id, new GenericCallback<T>((entity) => {
                        pendingEvent.cb(entity);
                    }));
                }

                registerPendings.Clear();
            }

            if (unregisterPendings.TryGetValue(typeName, out var unregisters))
            {
                foreach(var unregisterPending in unregisters)
                {
                    DoUnregister(listener, unregisterPending);
                }

                unregisterPendings.Clear();
            }

            triggering[typeName] = false;

            return true;
        }

        /// <summary>
        /// subscribe event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="tiggerCallback"></param>
        /// <returns></returns>
        public virtual int Subscribe<T>(GenericCallback<T> triggerCallback) where T : IEvent
        {
            int id = listenerId;
            listenerId++;

            string typeName = typeof(T).Name;
            if (!listeners.TryGetValue(typeName, out var listener))
            {
                listener = new XTriggerListener() {
                    listenerId = id, typeName = typeName, events = new List<XTiggerEvent>()
                };

                listeners.Add(typeName, listener);
            }

            if (!registerListeners.ContainsKey(id))
                registerListeners.Add(id, listener);
            
            if (triggering.TryGetValue(typeName, out bool pending))
            {
                if (pending)
                {
                    if (!registerPendings.TryGetValue(typeName, out var pendingList))
                    {
                        pendingList = new List<XPendingEvent>();
                        registerPendings.Add(typeName, pendingList);
                    }

                    pendingList.Add(new XPendingEvent() {
                        typeName = typeName, id = id, cb = (obj) => { triggerCallback?.Invoke((T)obj); }
                    });
                }
                else
                {
                    DoRegister(listener, id, triggerCallback);
                }
            }
            else
            {
                DoRegister(listener, id, triggerCallback);
            }

            return id;
        }

        /// <summary>
        /// unsubscribe event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public virtual bool Unsubscribe<T>(int eventId)
        {
            string typeName = typeof(T).Name;
            if (listeners.TryGetValue(typeName, out var listener))
            {
                var index = listener.events.FindIndex((v) => v.id == eventId);
                if (index >= 0)
                {
                    listener.events.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// unsubscribe all event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public virtual void Unsubscribe<T>()
        {
            string typeName = typeof(T).Name;
            if (listeners.TryGetValue(typeName, out var listener))
            {
                listener.events.Clear();
            }
        }

        /// <summary>
        /// unsubscribe all event
        /// </summary>
        public virtual void UnsubscribeAll()
        {
            foreach(var listener in listeners)
            {
                listener.Value.events.Clear();
            }

            listeners.Clear();
        }

        /// <summary>
        /// do register
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="id"></param>
        /// <param name="tiggerCallback"></param>
        /// <returns></returns>
        protected virtual bool DoRegister<T>(XTriggerListener listener, int id, GenericCallback<T> tiggerCallback) where T : IEvent
        {
            listener.events.Add(new XTiggerEvent() {
                id = id, cb = (entity) => { tiggerCallback?.Invoke((T)entity); }
            });

            return true;
        }

        /// <summary>
        /// do unregister
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="id"></param>
        protected virtual void DoUnregister(XTriggerListener listener, int id)
        {
            for (int idx = 0; idx < listener.events.Count; idx++)
            {
                XTiggerEvent triggerEvent = listener.events[idx];
                if (triggerEvent.id == id)
                {
                    listener.events.RemoveAt(idx);
                    break;
                }
            }
        }

        /// <summary>
        /// unregister
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Unregister(int id)
        {
            if (!registerListeners.ContainsKey(id))
                return false;

            XTriggerListener registerlistener = registerListeners[id];
            triggering.TryGetValue(registerlistener.typeName, out bool trigger);
            if (trigger)
            {
                if (!unregisterPendings.ContainsKey(registerlistener.typeName))
                    unregisterPendings.Add(registerlistener.typeName, new List<int>());

                List<int> pending = unregisterPendings[registerlistener.typeName];
                pending.Add(id);
            }
            else
            {
                DoUnregister(registerlistener, id);
            }

            return true;
        }

        /// <summary>
        /// reset module
        /// </summary>
        public virtual void Reset()
        {
            registerPendings.Clear();
            unregisterPendings.Clear();
            registerListeners.Clear();
            triggering.Clear();
            listeners.Clear();
        }
    }
}
