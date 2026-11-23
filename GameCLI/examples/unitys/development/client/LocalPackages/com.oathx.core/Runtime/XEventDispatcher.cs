using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Core
{
    /// <summary>
    /// event dispatcher
    /// </summary>
    public class XEventDispatcher
	{
        /// <summary>
        /// modules
        /// </summary>
        protected readonly Dictionary<string, XEventModule> modules = new Dictionary<string, XEventModule>();

        /// <summary>
        /// removes
        /// </summary>
        protected readonly Queue<string> removes = new Queue<string>();

        /// <summary>
        /// load module
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual XEventModule Load(System.Type type)
		{
			if (!modules.ContainsKey(type.Name))
			{
				XEventModule module = (XEventModule)System.Activator.CreateInstance(type);
				module.SetDispatcher(this);
				module.Active();

#if UNITY_EDITOR
				Debug.Log($"[{Time.frameCount}] Load module {type.Name}");
#endif

				modules.Add(type.Name, module);
			}

			return modules[type.Name];
		}

        /// <summary>
        /// load module
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Load<T>() where T : XEventModule, new()
		{
			string typeName = typeof(T).Name;
			if (!modules.ContainsKey(typeName))
			{
				XEventModule module = new T();
				module.SetDispatcher(this);
				module.Active();

#if UNITY_EDITOR
				Debug.Log($"[{Time.frameCount}] Load module {typeName}");
#endif

				modules.Add(typeName, module);
			}

			return (T)modules[typeName];
		}

        /// <summary>
        /// unload module
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public virtual void Unload<T>() where T : XEventModule
		{
			string typeName = typeof(T).Name;
			if (modules.TryGetValue(typeName, out var module))
            {
				module.Detive();

#if UNITY_EDITOR
				Debug.Log($"[{Time.frameCount}] Unload module {typeName}");
#endif
				Remove(typeName);
			}
		}

        /// <summary>
        /// unload module
        /// </summary>
        /// <param name="type"></param>
        public virtual void Unload(System.Type type)
		{
			if (modules.ContainsKey(type.Name))
			{
				XEventModule module = modules[type.Name];
				module.Detive();
#if UNITY_EDITOR
				Debug.Log($"[{Time.frameCount}] Unload module {type.Name}");
#endif
				Remove(type.Name);
			}
		}

        /// <summary>
        /// unload all modules
        /// </summary>
        public virtual void UnloadAll()
        {
			foreach(var it in modules)
            {
#if UNITY_EDITOR
				Debug.Log($"[{Time.frameCount}] Unload module {it.Key}");
#endif
				it.Value.Detive();
            }

			modules.Clear();
        }

        /// <summary>
        /// query module
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Query<T>() where T : XEventModule
		{
			return (T)modules[typeof(T).Name];
		}

        /// <summary>
        /// dispatch event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public virtual void DispatchEvent<T>(T entity) where T : IEvent
        {
			foreach (var module in modules)
            {
				var eventModule = module.Value;
				if (eventModule.HasEvent(entity))
                {
					eventModule.Trigger(entity);
				}
            }
        }

        /// <summary>
        /// remove module
        /// </summary>
        /// <param name="typeName"></param>
        public virtual void Remove(string typeName)
        {
			removes.Enqueue(typeName);
        }

        /// <summary>
        /// update
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Update(float deltaTime)
        {
			while (removes.Count != 0)
			{
				var typeName = removes.Dequeue();
				modules.Remove(typeName);
			}
		}
	}
}
