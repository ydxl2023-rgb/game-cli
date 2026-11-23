using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Oathx.Bundle.Editor
{
    static class AssetTreeExtensionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, System.Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, System.Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.ThenBy(selector);
            }
            else
            {
                return source.ThenByDescending(selector);
            }
        }
    }

    [Flags]
    public enum NewMessageFlag
    {
        None = 0x0,

        //this flag is only used to check bits, not set.
        Info = 0x80,
        EmptyBundle = 0x81,
        EmptyFolder = 0x82,

        //this flag is only used to check bits, not set.
        Warning = 0x8000,
        WarningInChildren = 0x8100,
        AssetsDuplicatedInMultBundles = 0x8200,
        VariantBundleMismatch = 0x8400,

        //this flag is only used to check bits, not set.
        Error = 0x800000,
        ErrorInChildren = 0x810000,
        SceneBundleConflict = 0x820000,
        DependencySceneConflict = 0x840000,
    }

    /// <summary>
    /// New Message Info
    /// </summary>
    public class NewMessageInfo
    {
        /// <summary>
        /// message text
        /// </summary>
        public string message
        { get; set; }

        /// <summary>
        /// Message Flag
        /// </summary>
        public NewMessageFlag msgFlag
        { get; set; } = NewMessageFlag.None;

        /// <summary>
        /// Message Icon
        /// </summary>
        public Texture icon
        { get; set; }

        /// <summary>
        /// New Message Info
        /// </summary>
        /// <param name="msg"></param>
        public NewMessageInfo(NewMessageFlag messageFlag, string msg)
        {
            msgFlag = messageFlag;
            message = msg;

            switch(msgFlag)
            {
                case NewMessageFlag.Info:
                    icon = EditorGUIUtility.FindTexture("console.infoicon");
                    break;

                case NewMessageFlag.Warning:
                    icon = EditorGUIUtility.FindTexture("console.warnicon");
                    break;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BundleAssetInfo
    {
        /// <summary>
        /// Asset Path
        /// </summary>
        public string path
        { get; set; }

        /// <summary>
        /// Asset Size
        /// </summary>
        public long size
        { get; set; }

        /// <summary>
        /// Bundle Path
        /// </summary>
        public BundleNode bundle
        { get; set; }

        /// <summary>
        /// Referencing Bundles
        /// </summary>
        public HashSet<BundleNode> depends
        { get; set; } = new HashSet<BundleNode>();

        /// <summary>
        /// Is Direct Asset
        /// </summary>
        public bool isDirectAsset
        { get; set; } = false;

        /// <summary>
        /// Reference Count
        /// </summary>
        public int refCount
        { get; set; } = 1;

        /// <summary>
        /// Error State
        /// </summary>
        public ErrorState state
        { get; set; } = ErrorState.None;

        /// <summary>
        /// Messages
        /// </summary>
        public List<NewMessageInfo> messages
        { get; set; } = new List<NewMessageInfo>();

        /// <summary>
        /// Grab reference
        /// </summary>
        public void Grab()
        {
            ++refCount;
        }

        /// <summary>
        /// Drop reference
        /// </summary>
        public void Drop()
        {
            refCount = Mathf.Max(0, --refCount);
        }

        /// <summary>
        /// Get File Size String
        /// </summary>
        /// <returns></returns>
        public string GetFileSize()
        {
            return size == 0 ? "--" : EditorUtility.FormatBytes(size);
        }

        /// <summary>
        /// Add Message
        /// </summary>
        /// <param name="message"></param>
        public void AddMessage(NewMessageFlag flag, string message)
        {
            messages.Add(new NewMessageInfo(flag, message));
        }

        /// <summary>
        /// bool operator
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(BundleAssetInfo exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// New Bundle Node Type
    /// </summary>
    public enum BundleNodeType
    {
        None,
        Folder,
        File,
        Package
    }

    /// <summary>
    /// New Error State
    /// </summary>
    public enum ErrorState
    {
        None,
        Warning,
        Error
    }

    /// <summary>
    /// New Bundle Node
    /// </summary>
    public class BundleNode
    {
        /// <summary>
        /// Node Path
        /// </summary>
        public string bundlePath
        { get; set; }

        /// <summary>
        /// Node Name
        /// </summary>
        public string bundleName
        { get; set; }

        /// <summary>
        /// Child Nodes
        /// </summary>
        public List<BundleNode> children
        { get; set; } = new List<BundleNode>();

        /// <summary>
        /// Node Type
        /// </summary>
        public BundleNodeType bundleType
        { get; set; }

        /// <summary>
        /// the bundle all dependencies
        /// </summary>
        public HashSet<BundleAssetInfo> dependencies
        { get; set; } = new HashSet<BundleAssetInfo>();

        /// <summary>
        /// the bundle size
        /// </summary>
        public long bundleLength
        { get; set; }

        /// <summary>
        /// the bundle state
        /// </summary>
        public ErrorState state
        { get; set; } = ErrorState.None;

        /// <summary>
        /// the bundle messages
        /// </summary>
        public List<NewMessageInfo> messages
        { get; set; } = new List<NewMessageInfo>();

        /// <summary>
        /// 
        /// </summary>
        public BundleNode parent
        { get; set; }

        /// <summary>
        /// New Bundle Node
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="type"></param>
        public BundleNode(string name, string path, BundleNodeType type)
        {
            bundleName = name;
            bundlePath = path;
            bundleType = type;
        }

        /// <summary>
        /// set parent node
        /// </summary>
        /// <param name="parentNode"></param>
        public void SetParent(BundleNode parentNode)
        {
            parent = parentNode;
        }

        /// <summary>
        /// add warning message
        /// </summary>
        /// <param name="message"></param>
        public void AddMessage(string message)
        {
            messages.Add(new NewMessageInfo(NewMessageFlag.Warning, message));
        }

        /// <summary>
        /// get file size string
        /// </summary>
        /// <returns></returns>
        public string GetFileSize()
        {
            return bundleLength == 0 ? "--" : EditorUtility.FormatBytes(bundleLength);
        }

        /// <summary>
        /// Find Child Node by Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BundleNode FindChild(string name)
        {
            foreach (var node in children)
            {
                if (node.bundleName == name)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Find Node Recursively by Name (search all descendants)
        /// </summary>
        /// <param name="name">Node name to find</param>
        /// <param name="includeSelf">Whether to include current node in search</param>
        /// <returns>Found node or null</returns>
        public BundleNode FindChildRecursive(string name, bool includeSelf = true)
        {
            // Check self first if included
            if (includeSelf && bundleName == name)
            {
                return this;
            }

            // Check direct children
            foreach (var child in children)
            {
                if (child.bundleName == name)
                {
                    return child;
                }
            }

            // Recursively check grandchildren
            foreach (var child in children)
            {
                // Don't include child itself again
                var found = child.FindChildRecursive(name, false);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// add new child node
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fullpath"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public BundleNode AddChild(string name, string fullpath, bool isLast, BundleNodeType type)
        {
            var existing = FindChild(name);
            if (existing != null)
                return existing;

            var newNode = new BundleNode(name, fullpath, type);
            newNode.SetParent(this);

            children.Add(newNode);
            if (isLast)
            {
                long bunleLength = CollectAllDependencies(newNode, true, out var deps);
                if (bunleLength != 0)
                {
                    newNode.dependencies = deps;
                    newNode.bundleLength = bunleLength;
                }
            }

            return newNode;
        }

        /// <summary>
        /// remove child node by name
        /// </summary>
        /// <param name="name"></param>
        public void RemoveChild(BundleNode bundle)
        {
            foreach(var child in children)
            {
                if (child.bundlePath == bundle.bundlePath)
                {
                    children.Remove(child);
                    break;
                }
            }
        }

        /// <summary>
        /// Collect all dependencies of this bundle node.
        /// </summary>
        /// <param name="bundleNode"></param>
        /// <param name="includeDirectAssets"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        private long CollectAllDependencies(BundleNode bundleNode,
            bool includeDirectAssets, out HashSet<BundleAssetInfo> dependencies)
        {
            return BundleDependencyCollector.TryCollectAllDependencies(bundleNode, includeDirectAssets, out dependencies);
        }

        /// <summary>
        /// bool operator
        /// </summary>
        /// <param name="exists"></param>
        public static implicit operator bool(BundleNode exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// Package Type
    /// </summary>
    public enum PackageType
    {
        Default,
        Extensible,
    }

    /// <summary>
    /// New Bundle Package
    /// </summary>
    public class BundlePackage : BundleNode
    {
        public BundlePackage(string name, string path)
            : base(name, path, BundleNodeType.Package)
        {

        }
    }

    /// <summary>
    /// Bundle 
    /// </summary>
    public static class BundleDependencyCollector
    {
        /// <summary>
        /// Collected dependencies
        /// </summary>
        static readonly Dictionary<string, BundleAssetInfo> dependencys = new Dictionary<string, BundleAssetInfo>();
        /// <summary>
        /// Existing bundles
        /// </summary>
        static readonly Dictionary<string, List<BundleNode>> 
            existingBundles = new Dictionary<string, List<BundleNode>>();

        /// <summary>
        /// Clear all collected dependencies
        /// </summary>
        public static void ClearDependencys()
        {
            dependencys.Clear();
            existingBundles.Clear();
        }

        /// <summary>
        /// Add a dependency asset
        /// </summary>
        /// <param name="assetInfo"></param>
        static BundleAssetInfo AddDependency(string path, long size, BundleNode bundle, bool isDirectAsset)
        {
            if (dependencys.TryGetValue(path, out BundleAssetInfo existing)) 
            {
                existing.Grab();

                if (isDirectAsset)
                {
                    // 直接资源以自身归属的 bundle 为准，无条件覆盖之前因间接依赖
                    // 被首个引用 bundle 占用的 bundle 字段；否则当间接依赖先于直接
                    // 归属被处理时（如 common 引用 TMP 先于 textmeshpro 自身处理），
                    // Bundle 列会错误显示引用方而非真正归属的 bundle。
                    existing.bundle = bundle;
                    existing.isDirectAsset = true;
                }
                else
                {
                    existing.depends.Add(bundle);
                }
            }
            else
            {
                existing = new BundleAssetInfo() {
                    path = path, size = size, bundle = bundle
                };

                if (isDirectAsset)
                {
                    existing.bundle = bundle;
                    existing.isDirectAsset = true;
                }

                dependencys.Add(path, existing);
            }

            if (!existingBundles.TryGetValue(path, out var bundles))
            {
                bundles = new List<BundleNode>();
                existingBundles.Add(path, bundles);
            }

            var index = bundles.FindIndex((v) => v.bundleName == bundle.bundleName);
            if (index < 0)
                bundles.Add(bundle);

            return existing;
        }

        /// <summary>
        /// Collect all dependencies of a given bundle node.
        /// </summary>
        /// <param name="bundleNode"></param>
        /// <param name="includeDirectAssets"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        public static long TryCollectAllDependencies(BundleNode bundle, bool includeDirectAssets, out HashSet<BundleAssetInfo> dependencies)
        {
            dependencies = new HashSet<BundleAssetInfo>();
            if (string.IsNullOrEmpty(bundle.bundlePath))
                return 0;

            string[] directAssets = AssetDatabase.GetAssetPathsFromAssetBundle(bundle.bundlePath);
            if (directAssets.Length <= 0)
                return 0;

            long fileSize = 0;
            if (includeDirectAssets)
            {
                foreach (var asset in directAssets)
                {
                    FileInfo fileInfo = new FileInfo(asset);
                    fileSize += fileInfo.Length;

                    // add direct asset as dependency
                    // 注意：size 必须传单文件大小 fileInfo.Length，而非累加值 fileSize。
                    // 早期此处误传 fileSize（前面所有直接资源大小之和），导致排在后面的
                    // 直接资源（如 pizza bundle 的 icon_pizzamoney.png）Size 列被显示为
                    // 整个 bundle 的累计大小，而非自身大小。
                    var assetInfo = AddDependency(asset.ToLower(), fileInfo.Length, bundle, true);
                    if (assetInfo)
                        dependencies.Add(assetInfo);
                }
            }

            // 共享的已访问集合，防止循环依赖导致无限递归（StackOverflow）
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var directAsset in directAssets)
            {
                // 递归收集间接依赖资源（加入 dependencies，供 Assets 面板灰色行显示），
                // 但返回的 size 不计入 fileSize —— bundleLength 只统计本 bundle 的直接资源，
                // 间接依赖的 size 只在它们各自归属的 bundle 里计算，避免共享依赖被多个 bundle 重复计入。
                CollectDependenciesRecursive(directAsset, bundle, dependencies, visited, false);
            }

            return fileSize;
        }

        /// <summary>
        /// collected assets
        /// </summary>
        /// <param name="visited">当前递归链路中已访问过的资源路径集合，用于打破循环依赖</param>
        private static long CollectDependenciesRecursive(string assetPath, BundleNode bundle, HashSet<BundleAssetInfo> collectedDependencies, HashSet<string> visited, bool isDirectAsset)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return 0;
            }

            // if unity built-in resource or not in Assets folder, skip
            if (!IsValidAssetPath(assetPath))
            {
                return 0;
            }

            // 循环依赖保护：若该资源已在当前递归链路上被处理过，直接返回，避免无限递归
            if (!visited.Add(assetPath))
            {
                return 0;
            }

            // get direct dependencies
            string[] directDeps = AssetDatabase.GetDependencies(assetPath, false);
            if (directDeps.Length == 0)
            {
                return 0;
            }

            foreach (var dep in directDeps)
            {
                // skip itself
                if (dep == assetPath)
                {
                    continue;
                }

                // if is valid asset, add to collection and collect its dependencies recursively
                if (IsValidAssetPath(dep))
                {
                    FileInfo fileInfo = new FileInfo(dep);

                    // add dependency asset（无条件加入：每个 bundle 都应在其 dependencies
                    // 里包含它引用的所有依赖，去重靠 collectedDependencies 本身的 HashSet 语义）
                    var assetInfo = AddDependency(dep.ToLower(), fileInfo.Length, bundle, isDirectAsset);
                    if (assetInfo)
                    {
                        collectedDependencies.Add(assetInfo);
                    }

                    CollectDependenciesRecursive(dep, bundle,
                        collectedDependencies, visited, isDirectAsset);
                }
            }

            return 0;
        }

        /// <summary>
        /// update bundle dependencies
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns></returns>
        public static bool UpdateBundleDependencys(List<BundleNode> bundles)
        {
            var nodes = CollectAllBundleNodes(bundles);

            // clear previous collected dependencies
            DetectBundleDependencyConflicts(nodes);
            return true;
        }

        /// <summary>
        /// check bundle dependency conflicts
        /// </summary>
        /// <param name="allBundles"></param>
        public static Dictionary<string, List<string>> DetectBundleDependencyConflicts(List<BundleNode> allBundles)
        {
            var nodes = new Dictionary<string, HashSet<BundleNode>>();

            // build asset to bundles map
            var assets = new Dictionary<string, BundleAssetInfo>();
            foreach (var bundle in allBundles)
            {
                if (bundle.dependencies.Count <= 0)
                    continue;

                foreach (var asset in bundle.dependencies)
                {
                    if (string.IsNullOrEmpty(asset.path))
                        continue;
                    
                    if (!nodes.TryGetValue(asset.path, out var set))
                    {
                        set = new HashSet<BundleNode>();
                        nodes[asset.path] = set;
                    }

                    set.Add(bundle);

                    if (!assets.ContainsKey(asset.path))
                        assets[asset.path] = asset;
                }
            }

            // check conflicts: assets not directly included in any bundle but depended on by multiple bundles
            var result = new Dictionary<string, List<string>>();
            foreach (var kv in nodes)
            {
                var assetPath = kv.Key;
                var bundles = kv.Value;
        
                // check if the asset is directly included in any bundle
                var assetInfo = assets[assetPath];
                if (assetInfo.isDirectAsset)
                {
                    int directContainCount = 0;
                    foreach (var bundle in bundles)
                    {
                        string[] directAssets = AssetDatabase.GetAssetPathsFromAssetBundle(bundle.bundlePath);
                        if (directAssets.Any(a => a.ToLower() == assetPath))
                        {
                            directContainCount++;
                        }
                    }

                    if (directContainCount > 1)
                    {
                        result[assetPath] = bundles.Select(b => b.bundleName).ToList();

                        // update bundle states
                        foreach (var bundle in bundles)
                        {
                            bundle.state = ErrorState.Warning;
                            bundle.AddMessage(assetPath);
                        }

                        assetInfo.state = ErrorState.Warning;
                        RefreshWarning(assetInfo);
                    }
                    else
                    {
                        RefreshInfo(assetInfo);
                    }
                }
                else
                {
                    if (bundles.Count > 1)
                    {
                        result[assetPath] = bundles.Select(b => b.bundleName).ToList();
                        // update bundle states
                        foreach (var bundle in bundles)
                        {
                            bundle.state = ErrorState.Warning;
                            bundle.AddMessage(assetPath);
                        }

                        assetInfo.state = ErrorState.Warning;
                        RefreshWarning(assetInfo);
                    }
                    else
                    {
                        RefreshInfo(assetInfo);
                    }
                }
            }

            return result;
        }

        static void RefreshInfo(BundleAssetInfo assetInfo)
        {
            if (existingBundles.TryGetValue(assetInfo.path, out var refBundles))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(assetInfo.path);
                sb.AppendLine("Is dependent on other bundle's asset(s) or auto included asset(s):");

                foreach (var refBundle in refBundles)
                {
                    sb.AppendLine(refBundle.bundlePath);
                }

                assetInfo.AddMessage(NewMessageFlag.Info,
                    sb.ToString());
            }
        }

        /// <summary>
        /// refresh message
        /// </summary>
        /// <param name="assetInfo"></param>
        static void RefreshWarning(BundleAssetInfo assetInfo)
        {
            if (existingBundles.TryGetValue(assetInfo.path, out var refBundles))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(assetInfo.path);
                sb.AppendLine("Asset is auto-included in multiple bundles due to dependencies:");

                foreach (var refBundle in refBundles)
                {
                    sb.AppendLine(refBundle.bundlePath);
                }
                
                assetInfo.AddMessage(NewMessageFlag.Warning,
                    sb.ToString());
            }
        }

        /// <summary>
        /// collect all bundle nodes recursively
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        static List<BundleNode> CollectAllBundleNodes(List<BundleNode> nodes)
        {
            List<BundleNode> result = new List<BundleNode>();
            foreach (var node in nodes)
            {
                result.Add(node);

                if (node.children != null && node.children.Count > 0)
                {
                    result.AddRange(CollectAllBundleNodes(node.children));
                }
            }

            return result;
        }

        /// <summary>
        /// check if the path is a valid Asset path
        /// </summary>
        private static bool IsValidAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            string extension = Path.GetExtension(path);
            if (!IsValidAssetTypeFromExtension(extension))
                return false;

            if (path.StartsWith("Resources/unity_builtin_extra") ||
                path.StartsWith("Library/") ||
                path.StartsWith("Packages/") && !path.Contains("/Assets/"))
            {
                return false;
            }

            return path.StartsWith("Assets/");
        }

        /// <summary>
        /// is a valid asset type
        /// </summary>
        private static bool IsValidAssetTypeFromExtension(string extension)
        {
            switch (extension)
            {
                case ".prefab":
                case ".unity":
                case ".mat":
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".tga":
                case ".psd":
                case ".fbx":
                case ".obj":
                case ".blend":
                case ".anim": 
                case ".controller":
                case ".shader":
                case ".shadergraph":
                case ".asset":
                case ".mixer":
                case ".wav":
                case ".mp3":
                case ".ogg":
                case ".ttf":
                case ".otf":
                    return true;

                default: 
                    return false;
            }
        }
    }
}