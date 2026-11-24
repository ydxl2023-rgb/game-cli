using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Oathx.Avatar
{
    public interface IEffectLoader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Coroutine LoadMultiAsync(string[] paths, System.Action<Object[]> callback);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Coroutine LoadAsync<T>(string path,
            System.Action<Object> callback) where T : Object;
    }

    public class XEffectResourceManager
    {
        private static XEffectResourceManager instance = new XEffectResourceManager();

        public static XEffectResourceManager GetSingleton()
        {
            return instance;
        }

        private IEffectLoader loader;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectLoader"></param>
        public void SetLoader(IEffectLoader effectLoader)
        {
            loader = effectLoader;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEffectLoader GetLoader()
        {
            return loader;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Coroutine LoadAsync<T>(string path,
            System.Action<Object> callback) where T : Object
        {
            return loader.LoadAsync<T>(path, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Coroutine LoadMultiAsync(string[] paths, System.Action<Object[]> callback)
        {
            return loader.LoadMultiAsync(paths, callback);
        }
    }

    public class XEffectBaseCache
    {
        public string effectPath;
        public GameObject prefab;
        public bool loading = false;
        public float createTime = 0f;

        public virtual void Release()
        {
            prefab = null;
            loading = false;
            createTime = 0f;
        }
    }


    public class XEffectCache : XEffectBaseCache
    {
        public Queue<XEffectComponent> queue;

        /// <summary>
        /// 
        /// </summary>
        public override void Release()
        {
            if (queue != null)
            {
                while (queue.Count > 0)
                {
                    XEffectComponent efc = queue.Dequeue();
                    if (efc != null && efc.gameObject != null)
                    {
                        GameObject.Destroy(efc.gameObject);
                    }
                }

                queue.Clear();
                queue = null;
            }

            base.Release();
        }
    }

    [System.Serializable]
    public class XEffectConfigure
    {
        public int maxEffectCacheCount = 32;
        public int maxEffectQueueCount = 8;
        public float effectQueueTimeThreshold = 20f;

        public IEffectLoader loader;
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class XEffectManager
    {
        private static readonly XEffectManager instance = new XEffectManager();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static XEffectManager GetSingleton()
        {
            return instance;
        }

        public Transform objectContainer;
        public Dictionary<int, XEffectComponent> effectMap = new Dictionary<int, XEffectComponent>();

        private int effectGlobalId = 1;
        private List<XEffectCache> cacheList = new List<XEffectCache>();
        private Dictionary<string, int> maxEffectCountDic = new Dictionary<string, int>();
        private Dictionary<string, int> currentEffectCountDic = new Dictionary<string, int>();
        private XEffectConfigure effectConfigure;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configure"></param>
        public void Startup(XEffectConfigure configure)
        {
            effectConfigure = configure;
            if (effectConfigure.loader != null)
                XEffectResourceManager.GetSingleton().SetLoader(configure.loader);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Shutdown()
        {
            ClearCacheEffects();
            ClearQueueInCache();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effect"></param>
        private void AddEffectMap(XEffectComponent effect)
        {
            if (effect == null) return;
            effect.gid = effectGlobalId++;

            effectMap.Add(effect.gid, effect);
            AddEffectCount(effect.resPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectPath"></param>
        /// <returns></returns>
        private bool CanEffectInstantial(string effectPath)
        {
            if (currentEffectCountDic.ContainsKey(effectPath) && currentEffectCountDic[effectPath] >= maxEffectCountDic[effectPath])            
                return false;
            
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectPath"></param>
        private void AddEffectCount(string effectPath)
        {
            if (maxEffectCountDic.ContainsKey(effectPath))
            {
                if (!currentEffectCountDic.ContainsKey(effectPath))
                    currentEffectCountDic[effectPath] = 0;

                currentEffectCountDic[effectPath] += 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectPath"></param>
        private void RemoveEffectCount(string effectPath)
        {
            if (currentEffectCountDic.ContainsKey(effectPath) && currentEffectCountDic[effectPath] > 0)
            {
                currentEffectCountDic[effectPath] -= 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectPath"></param>
        /// <param name="maxCount"></param>
        public void SetMaxEffectCount(string effectPath, int maxCount)
        {
            maxEffectCountDic[effectPath] = maxCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cache"></param>
        private void AddEffectCache(string path, XEffectCache cache)
        {
            AddCache<XEffectCache>(cacheList, effectConfigure.maxEffectCacheCount, path, cache);
            ReleaseAllEffectQueueInCacheByTimeThreshold();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheList"></param>
        /// <param name="maxCacheCount"></param>
        /// <param name="path"></param>
        /// <param name="cache"></param>
        private void AddCache<T>(List<T> cacheList, int maxCacheCount, string path, T cache) where T : XEffectBaseCache
        {
            cache.createTime = Time.time;
            cache.effectPath = path;
            if (cacheList.Count < maxCacheCount)
            {
                cacheList.Add(cache);
            }
            else
            {
                cacheList.Sort(delegate (T x, T y)
                {
                    return (int)(x.createTime - y.createTime);
                });
                cacheList[0].Release();
                cacheList[0] = cache;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheList"></param>
        /// <param name="cache"></param>
        private void RemoveCache<T>(List<T> cacheList, T cache) where T : XEffectBaseCache
        {
            if (cache != null && cacheList.Contains(cache))
            {
                cacheList.Remove(cache);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheList"></param>
        /// <param name="path"></param>
        private void RemoveCache<T>(List<T> cacheList, string path) where T : XEffectBaseCache
        {
            T cache = cacheList.Find(delegate (T obj)
            {
                return obj.effectPath == path;
            });

            if (cache != null)
            {
                cacheList.Remove(cache);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="effectCache"></param>
        /// <returns></returns>
        private bool TryGetEffectCache(string path, out XEffectCache effectCache)
        {
            return TryGetCache<XEffectCache>(cacheList, path, out effectCache);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheList"></param>
        /// <param name="path"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        private bool TryGetCache<T>(List<T> cacheList, string path, out T cache) where T : XEffectBaseCache
        {
            T effectInCache = cacheList.Find(delegate (T obj)
            {
                return obj.effectPath == path;
            });
            
            cache = effectInCache;
            if (cache != null)
            {
                cache.createTime = Time.time;
            }

            return cache != null;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReleaseAllEffectQueueInCacheByTimeThreshold()
        {
            for (int i = 0; i < cacheList.Count; i++)
            {
                if ((Time.time - cacheList[i].createTime) > effectConfigure.effectQueueTimeThreshold && cacheList[i].queue != null)
                {
                    while (cacheList[i].queue.Count > effectConfigure.maxEffectQueueCount)
                    {
                        XEffectComponent efc = cacheList[i].queue.Dequeue();
                        GameObject.Destroy(efc.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="timeThreshold"></param>
        /// <returns></returns>
        private bool CheckReleaseQueueInCacheByTimeThreshold(XEffectCache cache, float timeThreshold)
        {
            bool canEnqueue = true;
            if ((Time.time - cache.createTime) > timeThreshold && cache.queue != null)
            {
                while (cache.queue.Count > effectConfigure.maxEffectQueueCount)
                {
                    XEffectComponent efc = cache.queue.Dequeue();
                    GameObject.Destroy(efc.gameObject);
                    canEnqueue = false;
                }
            }
            return canEnqueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        private bool CheckReleaseQueueInCacheByTimeThreshold(XEffectCache cache)
        {
            return CheckReleaseQueueInCacheByTimeThreshold(cache, effectConfigure.effectQueueTimeThreshold);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectPath"></param>
        /// <param name="effectCache"></param>
        /// <returns></returns>
        public XEffectComponent PlayEffect(string effectPath, XEffectCache effectCache)
        {
            XEffectComponent effect = null;
            if (effectCache.queue.Count > 0)
            {
                effect = effectCache.queue.Dequeue();
                if (effect == null || effect.gameObject == null)
                {
                    effect = null;
                }
                else
                {
                    effect.gameObject.SetActive(true);
                }
            }
            else
            {
                if (effectCache.prefab == null)
                {
                    RemoveCache<XEffectCache>(cacheList, effectCache);
                    return null;
                }

                if (!CanEffectInstantial(effectPath))
                    return null;

                GameObject obj = GameObject.Instantiate(effectCache.prefab) as GameObject;
                effect = obj.GetComponent<XEffectComponent>();
                if (effect != null)
                    effect.resPath = effectPath;
            }

            AddEffectMap(effect);
            return effect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectPath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator DoLoadEffectAsync(string effectPath, System.Action<XEffectCache, XEffectComponent> callback)
        {
            XEffectCache effectCache = null;
            XEffectComponent effect = null;

            TryGetEffectCache(effectPath, out effectCache);
            if (effectCache == null)
            {
                effectCache = new XEffectCache();
                effectCache.loading = true;
                AddEffectCache(effectPath, effectCache);

                yield return XEffectResourceManager.GetSingleton().LoadAsync<GameObject>(effectPath, delegate (Object obj)
                {
                    effectCache.queue = new Queue<XEffectComponent>();
                    effectCache.prefab = obj as GameObject;
                    effectCache.loading = false;
                });
            }

            while (effectCache.loading)
                yield return null;

            if (effectCache.prefab != null)
            {
                effect = PlayEffect(effectPath, effectCache);
            }

            callback(effectCache, effect);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectPrefab"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator GetEffect(GameObject effectPrefab, System.Action<XEffectCache, XEffectComponent> callback)
        {
            XEffectCache effectCache = null;
            XEffectComponent effect = null;

            string effectPath = effectPrefab.name;
            TryGetEffectCache(effectPath, out effectCache);
            if (effectCache == null)
            {
                effectCache = new XEffectCache();
                effectCache.loading = true;
                AddEffectCache(effectPath, effectCache);

                effectCache.queue = new Queue<XEffectComponent>();
                effectCache.prefab = effectPrefab;
                effectCache.loading = false;
            }

            while (effectCache.loading)
                yield return null;

            if (effectCache.prefab != null)
            {
                effect = PlayEffect(effectPath, effectCache);
            }

            callback(effectCache, effect);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resList"></param>
        /// <param name="callback"></param>
        public void PreloadEffectPrefabs(string[] resList, System.Action<bool> callback)
        {
            XEffectCoroutine.Run(DoPreloadEffectPrefabs(resList, callback));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resList"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator DoPreloadEffectPrefabs(string[] resList, System.Action<bool> callback)
        {
            List<string> resLoad = new List<string>();
            for (int i = 0; i < resList.Length; i++)
            {
                string effectPath = resList[i];
                XEffectCache effectCache = null;
                TryGetEffectCache(effectPath, out effectCache);
                if (effectCache == null)
                {
                    effectCache = new XEffectCache();
                    AddEffectCache(effectPath, effectCache);
                }

                if (!effectCache.loading)
                {
                    effectCache.loading = true;
                    resLoad.Add(effectPath);
                }
            }

            yield return XEffectResourceManager.GetSingleton().LoadMultiAsync(resLoad.ToArray(), delegate (Object[] obj)
            {
                for (int i = 0; i < obj.Length; i++)
                {
                    XEffectCache effectCache = null;
                    if (TryGetEffectCache(resLoad[i], out effectCache))
                    {
                        effectCache.prefab = obj[i] as GameObject;
                        effectCache.queue = new Queue<XEffectComponent>();
                        effectCache.loading = false;
                    }
                }

                if (callback != null)
                    callback(true);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="destroyed"></param>
        public void DestroyEffect(XEffectComponent effect, bool destroyed = false)
        {
            if (effect == null) return;

            if (!destroyed)
            {
                XEffectCache effectCache = null;
                TryGetEffectCache(effect.resPath, out effectCache);
                if (effectCache != null)
                {
                    if (effectCache.queue == null)
                        effectCache.queue = new Queue<XEffectComponent>();
                    effectCache.queue.Enqueue(effect);
                    effect.transform.parent = objectContainer;
                    effect.gameObject.SetActive(false);
                }
                else if (effect.gameObject != null)
                {
                    GameObject.Destroy(effect.gameObject);
                }
            }

            if (effect.gid > 0)
            {
                effectMap.Remove(effect.gid);
                effect.gid = 0;
                RemoveEffectCount(effect.resPath);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectId"></param>
        public void DestroyEffect(int effectId)
        {
            XEffectComponent effect = null;
            effectMap.TryGetValue(effectId, out effect);
            if (effect != null)
                DestroyEffect(effect, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectPath"></param>
        /// <param name="callback"></param>
        public void DoGenerateAsync(string effectPath, System.Action<XEffectCache, XEffectComponent> callback)
        {
            XEffectCache effectCache = null;
            TryGetEffectCache(effectPath, out effectCache);

            if (effectCache != null && !effectCache.loading)
            {
                callback(effectCache, PlayEffect(effectPath, effectCache));
            }
            else
            {
                XEffectCoroutine.Run(DoLoadEffectAsync(effectPath, callback));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearCacheEffects()
        {
            if (objectContainer != null)
            {
                foreach (Transform child in objectContainer)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            cacheList.Clear();
            effectMap.Clear();
            currentEffectCountDic.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearQueueInCache()
        {
            for (int i = 0; i < cacheList.Count; i++)
            {
                if (cacheList[i].queue != null)
                {
                    while (cacheList[i].queue.Count > 0)
                    {
                        XEffectComponent eff = cacheList[i].queue.Dequeue();
                        if (eff != null && eff.gameObject != null)
                            GameObject.Destroy(eff.gameObject);
                    }
                }
            }

            currentEffectCountDic.Clear();
        }
    }
}
