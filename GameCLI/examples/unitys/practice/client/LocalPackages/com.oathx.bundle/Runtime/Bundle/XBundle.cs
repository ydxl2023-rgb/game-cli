using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Bundle.Assets 
{
    /// <summary>
    /// asset bundle info
    /// </summary>
    public class XBundle
    {
        static readonly string dataBasePath = Application.persistentDataPath.Replace("\\", "/");
        static readonly string streamBasePath = Application.streamingAssetsPath.Replace("\\", "/");
       
        /// <summary>
        /// file entry
        /// </summary>
        public Entry entry
        { get; set; }

        /// <summary>
        /// unity bundle object
        /// </summary>
        public AssetBundle bundle
        { get; set; }

        /// <summary>
        /// the bundle is load done
        /// </summary>
        public bool isDone
        { get; set; }

        /// <summary>
        /// the bundle current is loading
        /// </summary>
        public bool isLoading
        { get; set; }

        /// <summary>
        /// the bundle dependencies
        /// </summary>
        public HashSet<string> dependencies
        { get; set; }

        /// <summary>
        /// current bundle ref count
        /// </summary>
        public int refCount
        { get; set; }

        /// <summary>
        /// the bundle max ref count
        /// </summary>
        public int maxRefCount
        { get; set; }

        /// <summary>
        /// bundle loading time
        /// </summary>
        public float loadingTime
        { get; set; }

        /// <summary>
        /// bundle last read time
        /// </summary>
        public float lastReadTime
        { get; set; }

        /// <summary>
        /// bundle start loading time
        /// </summary>
        public float startLoadingTime
        { get; set; }

        /// <summary>
        /// bundle auto release cache time
        /// </summary>
        public float autoReleaseTime
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public XBundle(Entry info, float autoReleaseCacheTime = 0f)
        {
            entry = info;
            bundle = null;
            isDone = false;
            isLoading = false;
            refCount = 0;
            loadingTime = 0;
            startLoadingTime = 0;
            dependencies = new HashSet<string>();
            autoReleaseTime = autoReleaseCacheTime;
        }

        /// <summary>
        /// return current ref
        /// </summary>
        /// <returns></returns>
        public virtual int Ref()
        {
            return refCount;
        }

        /// <summary>
        /// inc ref
        /// </summary>
        /// <param name="time"></param>
        public void IncRef(float time)
        {
            refCount++;

            if (refCount > maxRefCount)
                maxRefCount = refCount;

            lastReadTime = time;
#if UNITY_EDITOR
            Debug.Log($"XAssetBundleInfo.IncRef bundle: {bundle.name} pack {entry.name} refCount {refCount} last read time {Time.realtimeSinceStartup}");

            BundleAnalysis.NotifyBundleLoad(this);
#endif
        }

        /// <summary>
        /// dec ref count
        /// </summary>
        public void DecRef()
        {
            refCount--;

#if UNITY_EDITOR
            Debug.Log($"XAssetBundleInfo.DecRef bundle: {bundle.name} pack {entry.name} refCount {refCount} " +
                $"last read time {Time.realtimeSinceStartup}");

            BundleAnalysis.NotifyBundleLoad(this);
#endif
        }

        /// <summary>
        /// unused bundle
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool Unused(float time)
        {
            if (time - lastReadTime > autoReleaseTime)
                return refCount <= 0;

            return false;
        }

        /// <summary>
        /// reset current bundle
        /// </summary>
        public void ResetRef()
        {
            refCount = 0;
            startLoadingTime = 0;
            lastReadTime = 0;
        }

        /// <summary>
        /// dependencies count
        /// </summary>
        /// <returns></returns>
        public int DependenciesCount()
        {
            return dependencies.Count;
        }

        /// <summary>
        /// add dependencies
        /// </summary>
        /// <param name="bundleName"></param>
        public void AddDependencies(string bundleName)
        {
            dependencies.Add(bundleName);
        }

        /// <summary>
        /// remove dependencies
        /// </summary>
        /// <param name="bundleName"></param>
        public void RemoveDependencies(string bundleName)
        {
            dependencies.Remove(bundleName);
        }

        /// <summary>
        /// start load bundle
        /// </summary>
        public void StartLoad()
        {
            startLoadingTime = Time.realtimeSinceStartup;
#if UNITY_EDITOR
            BundleAnalysis.NotifyBundleLoad(this);
#endif
        }

        /// <summary>
        /// bundle load done
        /// </summary>
        public void LoadDone()
        {
            loadingTime = Time.realtimeSinceStartup - startLoadingTime;
#if UNITY_EDITOR
            BundleAnalysis.NotifyBundleLoad(this);
#endif
        }

        /// <summary>
        /// bundle load path
        /// </summary>
        public string path
        {
            get { return System.IO.Path.Combine(dataBasePath, entry.name); }
        }

        /// <summary>
        /// get bundle url
        /// </summary>
        public string url
        {
            get
            {
                switch (entry.locationType)
                {
                    case LocationType.Remote:
                        return System.IO.Path.Combine(dataBasePath, entry.name);
                        
                    case LocationType.Streaming:
                        switch (Application.platform)
                        {
                            case RuntimePlatform.Android:
                                return System.IO.Path.Combine(streamBasePath, entry.name);
                            default:
                                return System.IO.Path.Combine(streamBasePath, entry.name);
                        }
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
