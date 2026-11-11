using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Core
{
    /// <summary>
    /// game plugin manager
    /// </summary>
    public sealed class XPluginManager : XSimpleSingleton<XPluginManager>
    {
        /// <summary>
        /// current loaded plugins
        /// </summary>
        private readonly Dictionary<string, XPlugin> plugins = new Dictionary<string, XPlugin>();

        /// <summary>
        /// load plugin
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public XPlugin LoadPlugin(System.Type type)
        {
            if (!plugins.ContainsKey(type.Name))
            {
                XPlugin plugin = (XPlugin)System.Activator.CreateInstance(type);
                if (plugin != null)
                {
                    plugin.Install();

#if UNITY_EDITOR
                    Debug.Log($"XPluginManager.LoadPlugin {plugin} done");
#endif

                    plugins.Add(type.Name, plugin);
                }
            }

            return plugins[type.Name];
        }

        /// <summary>
        /// unload plugin
        /// </summary>
        /// <param name="type"></param>
        public void UnloadPlugin(System.Type type)
        {
            if (plugins.TryGetValue(type.Name, out var plugin))
            {
                plugin.Shutdown();
                plugin.Uninstall();

#if UNITY_EDITOR
                Debug.Log($"XPluginManager.UnloadPlugin {plugin} done");
#endif

                plugins.Remove(type.Name);
            }
        }

        /// <summary>
        /// unload all plugins
        /// </summary>
        public void UnloadAllPlugin()
        {
            foreach(var plugin in plugins)
            {
                plugin.Value.Shutdown();

#if UNITY_EDITOR
                Debug.Log($"XPluginManager.UnloadAllPlugin {plugin.Value} done");
#endif
                plugin.Value.Uninstall();
            }

            plugins.Clear();
        }

        /// <summary>
        /// update plugins
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            foreach(var plugin in plugins)
            {
                plugin.Value.Update(deltaTime);
            }

            XPluginUtility.DispatchEvents();
        }

        /// <summary>
        /// load plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadPlugin<T>() where T : XPlugin
        {
            string typeName = typeof(T).Name;
            if (!plugins.ContainsKey(typeName))
            {
                XPlugin plugin = (XPlugin)System.Activator.CreateInstance(typeof(T));
                if (plugin.Install())
                {
#if UNITY_EDITOR
                    Debug.Log($"XPluginManager.LoadPlugin {typeName} done");
#endif
                    plugins.Add(typeName, plugin);
                }
            }

            return (T)plugins[typeName];
        }

        /// <summary>
        /// query plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T QueryPlugin<T>() where T : XPlugin
        {
            return (T)plugins[typeof(T).Name];
        }

        /// <summary>
        /// unload plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnloadPlugin<T>() where T : XPlugin
        {
            string typeName = typeof(T).Name;
            if (plugins.TryGetValue(typeName, out var plugin))
            {
                plugin.Uninstall();

#if UNITY_EDITOR
                Debug.Log($"XPluginManager.UnloadPlugin {typeName} done");
#endif

                plugins.Remove(typeName);
            }
        }

        /// <summary>
        /// fire event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool FireEvent<T>(T entity) where T : IEvent
        {
            foreach(var plugin in plugins)
            {
                plugin.Value.DispatchEvent(entity);
            }

            return true;
        }
    }

    /// <summary>
    /// plugin utility
    /// </summary>
    public sealed class XPluginUtility
    {
        /// <summary>
        /// event queue
        /// </summary>
        static readonly Queue<IEvent> events = new Queue<IEvent>();

        /// <summary>
        /// post event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="postEvent"></param>
        /// <returns></returns>
        public static bool PostEvent<T>(T postEvent) where T : IEvent
        {
            if (!events.Contains(postEvent))
                events.Enqueue(postEvent);

            return true;
        }

        /// <summary>
        /// dispatch events
        /// </summary>
        public static void DispatchEvents()
        {
            while(events.Count > 0)
            {
                var evt = events.Dequeue();
                if (evt != null)
                    FireEvent(evt);
            }
        }

        /// <summary>
        /// fire event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fireEvent"></param>
        /// <returns></returns>
        public static bool FireEvent<T>(T fireEvent) where T : IEvent
        {
#if UNITY_EDITOR
            Debug.Log($"[{Time.frameCount}] Fire event <color=magenta>{fireEvent}</color>");
#endif
            return XPluginManager.GetSingleton().FireEvent(fireEvent);
        }
    }
}

