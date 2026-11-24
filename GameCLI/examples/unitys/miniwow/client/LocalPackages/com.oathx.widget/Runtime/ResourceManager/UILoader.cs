using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Widget
{
    public interface UILoader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        Coroutine LoadAsync<T>(string path, System.Action<Object> done) where T : Object;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        Coroutine LoadAsync(string path, System.Type type, System.Action<Object> done);
    }


    /// <summary>
    /// 
    /// </summary>
    public class UIDefaultLoader : UILoader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        public Coroutine LoadAsync<T>(string path, System.Action<Object> done) where T : Object
        {
            return UICoroutine.Run(DoLoadAsync<T>(path, done));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        private IEnumerator DoLoadAsync<T>(string path, System.Action<Object> done) where T : Object
        {
            var req = Resources.LoadAsync<T>(path);
            yield return req;
            
            done?.Invoke(req.asset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        public Coroutine LoadAsync(string path, System.Type type, System.Action<Object> done)
        {
            return UICoroutine.Run(DoLoadAsync(path, type, done));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        private IEnumerator DoLoadAsync(string path, System.Type type, System.Action<Object> done)
        {
            var req = Resources.LoadAsync(path, type);
            yield return req;

            done?.Invoke(req.asset);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public interface UICacher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        UIAsset Add(string name, Object cache);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        UIAsset Find(string name);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        void Remove(string name);

        /// <summary>
        /// 
        /// </summary>
        void Clear();
    }

    public class UIAsset
    {
        public string name;
        public UnityEngine.Object obj;

        /// <summary>
        /// current resource asset ref count
        /// </summary>
        public int refcount
        { get; set; }

        public int Ref()
        {
            ++refcount;
            return refcount;
        }

        public int Unref()
        {
            --refcount;
            return refcount;
        }
    }

    public class UIDefaultCacher : UICacher
    {
        private readonly Dictionary<string, UIAsset> assets = new Dictionary<string, UIAsset>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        public UIAsset Add(string name, Object cache)
        {     
            if (!assets.TryGetValue(name, out var asset))
            {
                asset = new UIAsset() { 
                    name = name, obj = cache, refcount = 1
                };

                assets.Add(name, asset);
            }

            return asset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public UIAsset Find(string name)
        {
            if (assets.TryGetValue(name, out var asset))
                return asset;

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            if (assets.ContainsKey(name))
                assets.Remove(name);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            assets.Clear();
        }
    }
}

