using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Avatar
{
    public interface IAvatarLoader
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

    public class DefaultAvatarLoader : IAvatarLoader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Coroutine LoadMultiAsync(string[] paths, System.Action<Object[]> callback)
        {
            return StartCoroutine(DoLoadMultiAsync(paths, callback));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator DoLoadMultiAsync(string[] paths, System.Action<Object[]> callback)
        {
            if (paths.Length <= 0)
                yield break;

            Object[] results = new Object[paths.Length];
            bool[] loadDone = new bool[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                loadDone[i] = false;
                LoadAsync(paths[i], typeof(Object), delegate (Object obj, int index)
                {
                    results[index] = obj;
                    loadDone[index] = true;
                }, i);
            }

            for (int i = 0; i < paths.Length; i++)
                while (!loadDone[i])
                    yield return null;

            callback(results);

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
            return StartCoroutine(DoLoadAsync(path, typeof(T), callback));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Coroutine LoadAsync(string path, System.Type type, System.Action<Object> callback)
        {
            return StartCoroutine(DoLoadAsync(path, type, callback));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Coroutine LoadAsync(string path, System.Type type, System.Action<Object, int> callback, int param)
        {
            return StartCoroutine(DoLoadAsync(path, type, delegate (Object obj)
            {
                callback(obj, param);
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator DoLoadAsync(string path, System.Type type, System.Action<Object> callback)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[XRes::Load] sent empty/null path!");
                yield break;
            }

#if UNITY_EDITOR
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
            if (!asset)
                throw new System.NullReferenceException($"Can't load avatar asset {path}");
            yield return new WaitForEndOfFrame();

            callback?.Invoke(asset);
#else
            ResourceRequest req = Resources.LoadAsync(path, type);
            yield return req;

            callback?.Invoke(req.asset);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="em"></param>
        /// <returns></returns>
        private Coroutine StartCoroutine(IEnumerator em)
        {
            return XAvatarCoroutine.Run(em);
        }
    }


    public class XAvatarCacheInfo
    {
        public GameObject prefab;
        public string skeleton;
        public List<string> elements;
    }

    /// <summary>
    /// 排队等待同签名加载完成的回调（携带自身签名）。
    /// 回调按 skeleton+elements 签名分组触发，避免不同签名的加载先完成时串线派发错误模板。
    /// </summary>
    public class XAvatarPendingCallback
    {
        public string skeleton;
        public List<string> elements;
        public System.Action<Object> callback;
    }

    /// <summary>
    /// 
    /// </summary>
    public class XAvatarSystem
    {
        private static Coroutine StartCoroutine(IEnumerator em)
        {
            return XAvatarCoroutine.Run(em);
        }

        private static List<XAvatarCacheInfo> cacheList = new List<XAvatarCacheInfo>();
        private static List<XAvatarCacheInfo> loading = new List<XAvatarCacheInfo>();
        private static List<XAvatarPendingCallback> callbacks = new List<XAvatarPendingCallback>();
        private static int MaxCacheCount = 12;
        private static Transform immortalAvatarSystem;
        private static IAvatarLoader loader;

        private static bool InitialImmortalCache()
        {
            if (immortalAvatarSystem != null)
                return false;

            GameObject immortalGo = GameObject.Find(typeof(XAvatarSystem).Name);
            if (immortalGo == null)
            {
                immortalGo = new GameObject(typeof(XAvatarSystem).Name);
                GameObject.DontDestroyOnLoad(immortalGo);
            }

            immortalAvatarSystem = immortalGo.transform;
            return true;
        }

        public static void DestroyAllAvatar()
        {
            ResetCacheAvatar();

            if (immortalAvatarSystem)
                GameObject.Destroy(immortalAvatarSystem.gameObject);

            System.GC.Collect();

            // clear all callbacks
            callbacks.Clear();
        }

        private static void AddCacheAvatar(string skeleton, string[] elements, GameObject prefab)
        {
            if (prefab)
            {
                Object.DontDestroyOnLoad(prefab);

                prefab.transform.SetParent(immortalAvatarSystem, false);
                prefab.transform.position = Vector3.one * 10000;

                XAvatarCacheInfo info = new XAvatarCacheInfo();
                info.elements = new List<string>();
                if (elements.Length > 0)
                    info.elements.AddRange(elements);

                Animator animator = prefab.GetComponent<Animator>();
                if (animator)
                    animator.enabled = false;

                info.prefab = prefab;
                info.skeleton = skeleton;

                if (cacheList.Count >= MaxCacheCount)
                {
                    XAvatarCacheInfo first = cacheList[0];
                    if (first.prefab)
                        Object.Destroy(first.prefab);

                    first.elements.Clear();
                    first.skeleton = string.Empty;

                    cacheList.RemoveAt(0);
                }

                cacheList.Add(info);
            }
        }

        private static XAvatarCacheInfo GetCacheAvatar(string skeleton, string[] elements)
        {
            for (int idx = 0; idx < cacheList.Count; idx++)
            {
                XAvatarCacheInfo info = cacheList[idx];
                if (info.skeleton == skeleton && info.elements.Count == elements.Length)
                {
                    bool conformably = true;
                    for (int j = 0; j < elements.Length; j++)
                    {
                        if (info.elements[j] != elements[j])
                        {
                            conformably = false;
                            break;
                        }
                    }

                    if (conformably)
                    {
                        return info;
                    }
                }
            }

            return null;
        }

        public static void ResetCacheAvatar()
        {
            for (int idx = 0; idx < cacheList.Count; idx++)
            {
                XAvatarCacheInfo info = cacheList[idx];
                if (info.prefab)
                    GameObject.Destroy(info.prefab);

                info.elements.Clear();
                info.skeleton = string.Empty;
            }

            cacheList.Clear();
        }

        public static bool TryGetCache(string name, string[] elementList)
        {
            XAvatarCacheInfo cache = GetCacheAvatar(name, elementList);
            if (cache != null && cache.prefab)
            {
                for (int idx = 0; idx < cacheList.Count; idx++)
                {
                    XAvatarCacheInfo info = cacheList[idx];
                    if (info.skeleton == name && info.elements.Count == elementList.Length)
                    {
                        bool conformably = true;
                        for (int j = 0; j < elementList.Length; j++)
                        {
                            if (info.elements[j] != elementList[j])
                            {
                                conformably = false;
                                break;
                            }
                        }

                        if (conformably)
                        {
                            cacheList.RemoveAt(idx);
                            break;
                        }
                    }
                }

                AddCacheAvatar(cache.skeleton, cache.elements.ToArray(), cache.prefab);
                return true;
            }

            return false;
        }

        public static void DestroyAvatar(string name, string[] elementList)
        {
            XAvatarCacheInfo cache = GetCacheAvatar(name, elementList);
            if (cache != null && cache.prefab)
            {
                GameObject.Destroy(cache.prefab);

                cache.elements.Clear();
                cache.skeleton = string.Empty;

                cacheList.Remove(cache);
            }
        }

        /// <summary>
        /// 触发并移除与指定签名匹配的排队回调。
        /// 其他签名的回调继续排队，等待各自的加载完成，避免跨签名串线派发错误模板。
        /// </summary>
        /// <param name="skeletonName">完成加载的骨架名</param>
        /// <param name="elementList">完成加载的部件列表</param>
        /// <param name="avatar">加载完成的 Avatar GameObject</param>
        private static void FlushPendingCallbacks(string skeletonName, string[] elementList, GameObject avatar)
        {
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                var pending = callbacks[i];
                if (pending.skeleton != skeletonName || pending.elements.Count != elementList.Length)
                {
                    continue;
                }

                bool conformably = true;
                for (int j = 0; j < elementList.Length; j++)
                {
                    if (pending.elements[j] != elementList[j])
                    {
                        conformably = false;
                        break;
                    }
                }

                if (!conformably)
                {
                    continue;
                }

                callbacks.RemoveAt(i);
                pending.callback?.Invoke(avatar);
            }
        }

        /// <summary>
        /// 初始化角色系统：注入资源加载器，并设定 Avatar 模板 LRU 缓存容量。
        /// </summary>
        /// <param name="avatarLoader">资源加载器，null 时使用默认加载器</param>
        /// <param name="maxCacheCount">模板缓存容量上限；需覆盖业务的全部外观组合数，否则冷组合会被 LRU 淘汰、下次使用时触发骨架与网格合并的全量重建</param>
        public static void Initialize(IAvatarLoader avatarLoader = null, int maxCacheCount = 12)
        {
            loader = avatarLoader ?? new DefaultAvatarLoader();
            MaxCacheCount = maxCacheCount;
        }

        public static GameObject GetAvatar(string name, string[] elementList)
        {
            XAvatarCacheInfo cache = GetCacheAvatar(name, elementList);
            if (cache != null && cache.prefab)
            {
                return cache.prefab;
            }

            return null;
        }

        static IEnumerator OnLoadAsync(string name, string[] elementList, System.Action<Object> callabck)
        {
            if (loader == null)
            {
                callabck?.Invoke(null);
                yield break;
            }

            // find cache avatar
            XAvatarCacheInfo cacheInfo = GetCacheAvatar(name, elementList);
            if (cacheInfo != null && cacheInfo.prefab != null)
            {
                // 命中刷新 LRU 位置：移到队尾，容量淘汰时踢真正最久未用的模板
                cacheList.Remove(cacheInfo);
                cacheList.Add(cacheInfo);

                if (callabck != null)
                {
                    GameObject prefab = cacheInfo.prefab;

                    prefab.transform.position = Vector3.one * 10000;
                    prefab.transform.localScale = Vector3.one;
                    prefab.transform.rotation = Quaternion.identity;

                    callabck(prefab);
                }

                yield break;
            }

            // check current avatar load state
            bool loadingState = false;
            foreach(var load in loading)
            {
                if (load.skeleton == name && load.elements.Count == elementList.Length)
                {
                    bool flag = true;
                    for (int i = 0; i < load.elements.Count; i++)
                    {
                        if (load.elements[i] != elementList[i])
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                        loadingState = true;
                }
            }

            //When loading the same model, cache the callback and wait for the first one to finish loading before calling back uniformly
            if (loadingState)
            {
                callbacks.Add(new XAvatarPendingCallback() {
                    skeleton = name, elements = new List<string>(elementList), callback = callabck
                });
            }
            else
            {
                // add loading info to list
                loading.Add(new XAvatarCacheInfo() {
                    skeleton = name, elements = new List<string>(elementList)
                });

                List<Object> elements = new List<Object>();
                if (elementList.Length > 0)
                {
                    yield return loader.LoadMultiAsync(elementList, delegate (Object[] objs)
                    {
                        for (int i = 0; i < objs.Length; i++)
                        {
                            elements.Add(objs[i]);
                        }
                    });
                }

                yield return loader.LoadAsync<GameObject>(name, delegate (Object obj)
                {
                    // Find the ones that have been loaded and remove them
                    var index = loading.FindIndex((v) => {
                        bool findFlag = true;
                        if (v.skeleton == name)
                        {
                            for (int index = 0; index < elementList.Length; index++)
                            {
                                if (v.elements[index] != elementList[index])
                                {
                                    findFlag = false;
                                    break;
                                }
                            }
                        }

                        return findFlag;
                    });

                    if (index >= 0)
                        loading.RemoveAt(index);

                    // create avatar GameObject
                    GameObject skeleton = GameObject.Instantiate(obj) as GameObject;
                    if (skeleton && elements.Count > 0)
                    {
                        List<CombineInstance> combineInstances = new List<CombineInstance>();
                        Transform[] trans = skeleton.GetComponentsInChildren<Transform>();
                        List<Matrix4x4> bindPoses = new List<Matrix4x4>();
                        List<Transform> boneTrans = new List<Transform>();
                        List<Material> shardMatList = new List<Material>();

                        for (int i = 0; i < elements.Count; i++)
                        {
                            XAvatarElement element = elements[i] as XAvatarElement;
                            if (element && element.Prefab)
                            {
                                GameObject prefab = GameObject.Instantiate(element.Prefab) as GameObject;
                                if (prefab)
                                {
                                    SkinnedMeshRenderer skin = prefab.GetComponent<SkinnedMeshRenderer>();
                                    CombineInstance ci = new CombineInstance();
                                    ci.mesh = skin.sharedMesh;
                                    ci.transform = element.SmrLocalToWorldMatrix;
                                    combineInstances.Add(ci);

                                    List<Transform> curBones = new List<Transform>();
                                    foreach (string transName in element.BoneNames)
                                    {
                                        for (int transIndex = 0; transIndex < trans.Length; transIndex++)
                                        {
                                            if (transName == trans[transIndex].name)
                                            {
                                                curBones.Add(trans[transIndex]);
                                                bindPoses.Add(trans[transIndex].worldToLocalMatrix * skeleton.transform.localToWorldMatrix);
                                                break;
                                            }
                                        }
                                    }

                                    boneTrans.AddRange(curBones);

                                    try
                                    {
                                        Dictionary<string, int> boneWeightDic = element.GenBoneWeightsDic();
                                        foreach (BoneWeight boneWeight in skin.sharedMesh.boneWeights)
                                        {
                                            BoneWeight bw = boneWeight;

                                            bw.boneIndex0 = boneWeightDic[curBones[boneWeight.boneIndex0].name];
                                            bw.boneIndex1 = boneWeightDic[curBones[boneWeight.boneIndex1].name];
                                            bw.boneIndex2 = boneWeightDic[curBones[boneWeight.boneIndex2].name];
                                            bw.boneIndex3 = boneWeightDic[curBones[boneWeight.boneIndex3].name];
                                        }
                                    }
                                    catch (System.Exception e)
                                    {
                                        Debug.LogError($"{name} {prefab.name} {e.Message}");
                                    }

                                    shardMatList.AddRange(element.SharedMaterials);

                                    GameObject.DestroyImmediate(prefab);
                                }
                            }
                        }

                        skeleton.transform.position = Vector3.one * 100000;
                        skeleton.transform.eulerAngles = Vector3.zero;
                        skeleton.name = obj.name;
                        Transform model = skeleton.transform.Find(typeof(Renderer).Name);
                        if (!model)
                            model = skeleton.transform;

                        GameObject renderGo = new GameObject(typeof(Renderer).Name);
                        renderGo.transform.parent = model;
                        renderGo.transform.localScale = Vector3.one;
                        renderGo.transform.localPosition = Vector3.zero;
                        renderGo.transform.localRotation = Quaternion.identity;

                        var renderComp = renderGo.GetComponent<XAvatarRenderer>();
                        if (!renderComp)
                            renderComp = renderGo.AddComponent<XAvatarRenderer>();

                        SkinnedMeshRenderer smr = renderGo.AddComponent<SkinnedMeshRenderer>();
                        smr.sharedMesh = new Mesh();
                        smr.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, false);
                        smr.bones = boneTrans.ToArray();
                        smr.sharedMaterials = shardMatList.ToArray();

                        Animator animator = skeleton.GetComponent<Animator>();
                        if (animator)
                            smr.rootBone = animator.GetBoneTransform(HumanBodyBones.Hips);
                        
                        smr.sharedMesh.RecalculateBounds();

                        // add the avatar to cache list
                        AddCacheAvatar(name, elementList, skeleton);

                        // The first callback with restrictions
                        callabck?.Invoke(skeleton);

                        //Unified execution of cached callbacks (only those queued for the same signature)
                        FlushPendingCallbacks(name, elementList, skeleton);
                    }
                    else
                    {
                        skeleton.transform.position = Vector3.one * 100000;
                        skeleton.name = obj.name;
                        Transform model = skeleton.transform.Find(typeof(Renderer).Name);
                        if (!model)
                            model = skeleton.transform;

                        AddCacheAvatar(name, elementList, skeleton);

                        // The first callback with restrictions
                        callabck?.Invoke(skeleton);

                        //Unified execution of cached callbacks (only those queued for the same signature)
                        FlushPendingCallbacks(name, elementList, skeleton);
                    }
                });
            }
        }

        public static Coroutine LoadAsync(string skeletonName, string[] elementList, System.Action<Object> callback)
        {
            InitialImmortalCache();
            return StartCoroutine(OnLoadAsync(skeletonName, elementList, callback));
        }

        static IEnumerator DoLoadMultiAsync(XAvatarObject[] confs, System.Action<Object[]> callback)
        {
            if (loader == null)
            {
                callback?.Invoke(null);
                yield break;
            }

            Object[] results = new Object[confs.Length];
            bool[] loadDone = new bool[confs.Length];

            for (int i = 0; i < confs.Length; i++)
            {
                loadDone[i] = false;
                int index = i;
                LoadAsync(confs[i].skeleton, confs[i].elements, delegate (Object obj)
                {
                    results[index] = obj;
                    loadDone[index] = true;
                });
            }

            for (int i = 0; i < confs.Length; i++)
            {
                while (!loadDone[i])
                    yield return null;
            }

            callback(results);
        }

        public static Coroutine LoadMultiAsync(XAvatarObject[] confs, System.Action<Object[]> callback)
        {
            InitialImmortalCache();
            return StartCoroutine(DoLoadMultiAsync(confs, callback));
        }
    }
}