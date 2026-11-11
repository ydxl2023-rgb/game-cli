using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Widget
{
    [System.Serializable]
    public class ResourceConfigure
    {
        public UILoader loader;
        public UICacher cacher;
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class UIResourceManager
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly UIResourceManager instance = new UIResourceManager();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UIResourceManager GetSingleton()
        {
            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        private UILoader loader;
        private UICacher cacher;

        private readonly Dictionary<string, bool> loading = new Dictionary<string, bool>();

        /// <summary>
        /// cache callbacks
        /// </summary>
        private readonly Dictionary<string, List<System.Action<Object>>> callbacks = new Dictionary<string, List<System.Action<Object>>>();

        /// <summary>
        /// resource startup, set loader and cacher
        /// </summary>
        /// <param name="conf"></param>
        public void Startup(ResourceConfigure conf)
        {
            loader = conf.loader;
            cacher = conf.cacher;
        }

        /// <summary>
        /// close ui resource mananger
        /// </summary>
        public void Shutdown()
        {
            if (cacher != null)
                cacher.Clear();
        }

        /// <summary>
        /// unload resource
        /// </summary>
        /// <param name="path"></param>
        public void Unload(string path)
        {
            var asset = cacher.Find(path);
            if (asset != null)
            {
                if (asset.Unref() <= 0)
                    Resources.UnloadAsset(asset.obj);
            }
        }

        /// <summary>
        /// unload resource
        /// </summary>
        /// <param name="obj"></param>
        public void Unload(UnityEngine.Object obj)
        {
            if (cacher != null)
            {
                var address = obj.name;

                var asset = cacher.Find(address);
                if (asset != null)
                {
                    if (asset.Unref() <= 0)
                    {
                        Resources.UnloadAsset(asset.obj);

                        // unload asset address and remove it
                        cacher.Remove(address);
                    }
                }
            }
        }

        /// <summary>
        /// load resource
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        public bool LoadAsync<T>(string path, System.Action<Object> done) where T : Object
        {
            string address = path.ToLower();
            if (loading.ContainsKey(address))
            {
                if (!callbacks.TryGetValue(address, out var actions))
                {
                    actions = new List<System.Action<Object>>();
                    callbacks.Add(address, actions);
                }

                actions.Add(done);
                return false;
            }

            if (cacher != null)
            {
                var asset = cacher.Find(address);
                if (asset != null)
                {
                    done.Invoke(asset.obj);

                    int count = asset.Ref();
#if UNITY_EDITOR
                    Debug.Log($"Load resource {typeof(T).Name} : {path} ref {count}");
#endif
                    return true;
                }
            }
            
            loading.Add(address, true);

            loader.LoadAsync<T>(address, (Object obj) => {
                if (loading.ContainsKey(address))
                    loading.Remove(address);

                if (obj)
                {
                    done?.Invoke(obj);

                    if (callbacks.TryGetValue(address, out var actions))
                    {
                        foreach (var action in actions)
                        {
                            if (action != null)
                            {
                                action.Invoke(obj);
                            }
                        }

                        callbacks.Remove(address);
                    }

                    if (cacher != null)
                        cacher.Add(address, obj);
                }
                else
                {
                   Debug.LogError($"Can't load {typeof(T).Name} {address}");
                }
            });

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        public bool LoadAsync(string path, System.Type type, System.Action<Object> done)
        {
            string address = path.ToLower();
            if (loading.ContainsKey(address))
            {
                if (!callbacks.TryGetValue(address, out var actions))
                {
                    actions = new List<System.Action<Object>>();
                    callbacks.Add(address, actions);
                }

                actions.Add(done);
                return false;
            }

            var asset = cacher.Find(address);
            if (asset != null)
            {
                done.Invoke(asset.obj);

                int count = asset.Ref();
#if UNITY_EDITOR
                Debug.Log($"Load resource type {type.Name} : {path} ref count {count}");
#endif
                return true;
            }
            
            loading.Add(address, true);

            loader.LoadAsync(address, type, (Object obj) => {
                if (loading.ContainsKey(address))
                    loading.Remove(address);

                if (obj)
                {
                    done?.Invoke(obj);

                    if (callbacks.TryGetValue(address, out var actions))
                    {
                        foreach (var action in actions)
                        {
                            if (action != null)
                            {
                                action.Invoke(obj);
                            }
                        }

                        callbacks.Remove(address);
                    }

                    if (cacher != null)
                        cacher.Add(address, obj);
                }
                else
                {
                    Debug.LogError($"Can't load {type.Name} {address}");
                }
            });

            return true;
        }
    }
}
